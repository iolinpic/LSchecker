using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace LSchecker
{
    internal class LookupRunner
    {
        private List<Lookup> lookups;

        public LookupRunner()
        {
            lookups = new List<Lookup>();
        }

        public async Task Init(string filename = "links.json")
        {
            using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate))
            {
                var tmpLookups = await JsonSerializer.DeserializeAsync<List<Lookup>>(fs);
                if (tmpLookups != null) lookups = tmpLookups;
            }
        }

        private Task runRequest(Lookup lookup)
        {
            return Task.Run(async () =>
            {
                try
                {
                    var code = await Caller.GetAsync(lookup.Link);
                    lookup.Result = code.ToString();
                }
                catch (Exception ex)
                {
                    lookup.Result = ex.Message;
                }
            });
        }

        public async Task runAll()
        {
            await Task.WhenAll(lookups.ConvertAll((Lookup) => runRequest(Lookup)));
            prepareReport();
            await Task.WhenAll(lookups.Where(lookup => lookup.Result != "OK").ToList().ConvertAll(lookup =>
            {
                var str = new StringBuilder();
                str.Append($"link: {lookup.Link}");
                str.Append(Environment.NewLine);
                str.Append($"error:{lookup.Result}");
                return Caller.SendTGMessage(UrlEncoder.Default.Encode(str.ToString()));
            }));
        }

        private void prepareReport()
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                foreach (var lookup in lookups)
                {
                    var note = new LookupResult { Link = lookup.Link, Result = lookup.Result!, Created = DateTime.UtcNow };
                    db.Add(note);
                }
                db.SaveChanges();
            }
        }
    }
}