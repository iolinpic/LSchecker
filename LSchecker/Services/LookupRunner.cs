using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using LSchecker.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LSchecker.Services
{
    internal class LookupRunner : ILookupRunner
    {
        private List<Lookup> _lookups;
        private Dictionary<string, bool> _lastLookups;
        private string _linksFileName;
        private ICaller _caller;
        private ApplicationContext _db;
        private ILogger _logger;

        public LookupRunner(IConfiguration configuration, ICaller caller, ApplicationContext db, ILoggerFactory loggerFactory)
        {
            _lookups = new List<Lookup>();
            _lastLookups = new Dictionary<string, bool>();
            _linksFileName = configuration.GetSection("links").Value;
            _caller = caller;
            _db = db;
            _logger = loggerFactory.CreateLogger<LookupRunner>();
        }

        private async Task LoadLookupList()
        {
            using (FileStream fs = new FileStream(_linksFileName, FileMode.OpenOrCreate))
            {
                var tmpLookups = await JsonSerializer.DeserializeAsync<List<Lookup>>(fs);
                if (tmpLookups != null) _lookups = tmpLookups;
            }
            //make a dictionary with previos states of request
            foreach (var lookup in _lookups)
            {
                var last = _db.Lookups.Where(lkp => lkp.Link.Equals(lookup.Link)).OrderBy(lkp => lkp.Id).LastOrDefault();
                if (last != null)
                {
                    _lastLookups.Add(lookup.Link, !last.Result.Equals("OK"));
                    continue;
                }
                _lastLookups.Add(lookup.Link, false);
            }
        }

        private Task runRequest(Lookup lookup)
        {
            return Task.Run(async () =>
            {
                //get last request result before check
                var lastFailed = _lastLookups[lookup.Link];
                // start check, notify if last was not OK, or when changed from error to OK
                try
                {
                    var code = await _caller.GetAsync(lookup.Link);
                    lookup.Result = code.ToString();
                    if (lastFailed)
                        await notifyChanel(lookup);
                }
                catch (Exception ex)
                {
                    lookup.Result = ex.Message;
                    if (!lastFailed)
                        await notifyChanel(lookup);
                }
                lookup.Created = DateTime.UtcNow;
                _db.Add(lookup);
            });
        }

        public async Task runLookupListCheckAsync()
        {
            await LoadLookupList();
            await Task.WhenAll(_lookups.ConvertAll((Lookup) => runRequest(Lookup)));
            await _db.SaveChangesAsync();
        }

        private Task notifyChanel(Lookup lookup)
        {
            var str = new StringBuilder();
            str.Append($"link: {lookup.Link}");
            str.Append(Environment.NewLine);
            str.Append($"error:{lookup.Result}");
            return _caller.SendNotificationAsync(UrlEncoder.Default.Encode(str.ToString()));
        }
    }
}