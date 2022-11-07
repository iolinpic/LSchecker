using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using LSchecker.Models;
using Microsoft.Extensions.Logging;

namespace LSchecker.Services
{
    public class DataAnalyse : IDataAnalyse
    {
        private ApplicationContext _db;
        private Dictionary<string, List<Lookup>> _reportData;
        private Dictionary<string, List<ReportStruct>> _reportPreparation;
        private ICaller _caller;
        private const string FILENAME = "report.xlsx";

        private struct ReportStruct
        {
            public DateTime? Start;
            public DateTime? End;
            public string Reason;
            public string Link;
        }

        public DataAnalyse(ApplicationContext db, ICaller caller)
        {
            _db = db;
            _reportData = new Dictionary<string, List<Lookup>>();
            _reportPreparation = new Dictionary<string, List<ReportStruct>>();
            _caller = caller;
        }

        public void prepareReport()
        {
            //get all apropriate lookups from db (by time)
            var weeklyLookups = _db.Lookups.Where(lkp => lkp.Created < DateTime.UtcNow).ToList();
            //make a dictionary in form (link,list<lookup>})
            weeklyLookups.ForEach(lkp =>
            {
                if (!_reportData.ContainsKey(lkp.Link))
                {
                    _reportData[lkp.Link] = new List<Lookup>();
                }
                _reportData[lkp.Link].Add(lkp);
            });
            //make a report summary (total time with error (count as 5min*list lenght))

            prepareData();
            //prepare report for export (xlsx)
            prepareXlsxReport();
            //deliver report for user (tg?,email?)
            Task.Run(async () => await _caller.SendReportFileAsync(FILENAME, "Еженедельный отчет по неработающим сайтам")).Wait();
            //clear db for previos week lookups
            _db.Lookups.RemoveRange(weeklyLookups);
            _db.SaveChanges();
        }

        private void prepareData()
        {
            _reportData.Keys.ToList().ForEach(key =>
            {
                _reportPreparation[key] = new List<ReportStruct>();
                var data = _reportData[key];
                Lookup? tmp = null;
                Lookup? seq = null;
                data.ForEach(lkp =>
                {
                    if (tmp == null)
                    {
                        if (lkp.Result != "OK")
                        {
                            tmp = lkp;
                        }
                        return;
                    }
                    else
                    {
                        if (lkp.Result != "OK" && tmp!.Result != lkp.Result)
                        {
                            _reportPreparation[key].Add(new ReportStruct { Start = tmp.Created, End = lkp.Created, Reason = tmp.Result, Link = key });
                            tmp = lkp;
                            seq = null;
                            return;
                        }
                        if (lkp.Result == "OK")
                        {
                            _reportPreparation[key].Add(new ReportStruct { Start = tmp.Created, End = lkp.Created, Reason = tmp.Result, Link = key });
                            tmp = null;
                            seq = null;
                            return;
                        }
                        if (lkp.Result != "OK" && tmp!.Result == lkp.Result)
                        {
                            seq = lkp;
                            return;
                        }
                    }
                });
                if (tmp != null && seq == null)
                {
                    _reportPreparation[key].Add(new ReportStruct { Start = tmp.Created, End = tmp.Created + new TimeSpan(0, 5, 0), Reason = tmp.Result, Link = key });
                }
                if (tmp != null && seq != null)
                {
                    _reportPreparation[key].Add(new ReportStruct { Start = tmp.Created, End = seq.Created, Reason = tmp.Result, Link = key });
                }
            });
        }

        private void prepareXlsxReport()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Report");
                //setup title
                worksheet.Cell("A1").Value = "Start";
                worksheet.Cell("B1").Value = "End";
                worksheet.Cell("C1").Value = "Error";
                worksheet.Cell("D1").Value = "Link";
                var rngTitle = worksheet.Range("A1:D1");
                rngTitle.Style
                    .Font.SetBold()
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                //output content
                var row = 2;
                foreach (var key in _reportPreparation.Keys)
                {
                    worksheet.Cell(row, 1).Value = _reportPreparation[key].AsEnumerable();
                    row += _reportPreparation[key].Count;
                }
                //setup output
                worksheet.RangeUsed().Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
                worksheet.RangeUsed().SetAutoFilter();
                worksheet.Columns().AdjustToContents();
                workbook.SaveAs(FILENAME);
            }
        }
    }
}