using DBMM0;
using MM0.ViewModels;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Starcounter;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MM0.Api
{
    class MainHandlers : IHandler
    {
        private static Json GetLauncherPage(string url, bool dbScope = false)
        {
            if (dbScope)
                return Db.Scope(() => Self.GET<Json>(url));
            else
            {
                return Self.GET<Json>(url);
            }
        }

        private static Json WrapPage<T>(string partialPath) where T : Json
        {
            var curSession = Session.Current;

            var master = GetMasterPageFromSession();

            //if (master.CurrentPage != null && master.CurrentPage.GetType() == typeof(T))
            //{
            //    return master;
            //}

            //if (curSession != null)     // Ilk girisinde URL ne olursa olsun MainPage'e gonder
            {
                master.CurrentPage = Self.GET(partialPath);

                if (master.CurrentPage.Data == null)
                {
                    master.CurrentPage.Data = null; //trick to invoke OnData in partial
                }
            }
            return master;
        }

        protected static MasterPage GetMasterPageFromSession()
        {
            // if (Session.Current == null) // IlkGiris
            var master = Session.Ensure().Store[nameof(MasterPage)] as MasterPage;

            if (master == null)
            {
                var TrhStr = DateTime.Today.ToString("yyyy-MM-dd");
                master = new MasterPage
                {
                    //NavPage = Self.GET("/uniformdocs/nav")
                    TrhX = TrhStr,
                    BasTrhX = TrhStr,
                    BitTrhX = TrhStr
                };
            Session.Current.Store[nameof(MasterPage)] = master;
            }

            return master;
        }

        public void Register()
        {
            Application.Current.Use(new HtmlFromJsonProvider());
            Application.Current.Use(new PartialToStandaloneHtmlProvider());

            // Workspace home page (landing page from launchpad) dashboard alias

            Handle.GET("/", () =>
            {
                return Self.GET("/MM0");
            });
            Handle.GET("/MM0", () =>
            {
                MasterPage master = GetMasterPageFromSession();
                return master;
            });
            Handle.GET("/MM0/AboutPage", () => 
            {
                MasterPage master = GetMasterPageFromSession();
                master.CurrentPage = Self.GET("/MM0/partials/AboutPage");
                return master;
            });

            // Client Login yapmis olmali.
            Handle.GET("/MM0/PPs", () =>
            {
                MasterPage master = GetMasterPageFromSession();
                if (master.Token != "")
                {
                    var cc = Db.SQL<CC>("select r from CC r where r.Token = ?", master.Token).FirstOrDefault();
                    if (cc != null)
                        return WrapPage<PPsPage>($"/MM0/partials/PPs/{cc.Id}");
                }
                return master;
            });


            // Client Login yapmis olmali.
            Handle.GET("/MM0/PPs/{?}", (string CCId) =>
            {
                MasterPage master = GetMasterPageFromSession();
                //if (master.Token == "")
                //    return "Not SignIn yet!";

                return WrapPage<PPsPage>($"/MM0/partials/PPs/{CCId}");
            });

            Handle.GET("/MM0/HHs/{?}", (long PPId) => WrapPage<HHsPage>($"/MM0/partials/HHs/{PPId}"));
            Handle.GET("/MM0/HHsCumBky/{?}", (long HHId) => WrapPage<HHsPage>($"/MM0/partials/HHsCumBky/{HHId}"));

            Handle.GET("/MM0/FFs/{?}", (long PPId) => WrapPage<FFsPage>($"/MM0/partials/FFs/{PPId}"));


            Handle.GET("/MM0/FFsRpr?{?}", (string queryString) =>
            {
                return WrapPage<FFsRpr>($"/MM0/partials/FFsRpr?{queryString}");
            });

            Handle.GET("/MM0/confirmemail/{?}", (string deMail) =>
            {
                MasterPage master = GetMasterPageFromSession();
                var eMail = Hlp.DecodeQueryString(deMail);

                master.Token = "";

                CC cc = Db.SQL<CC>("select r from CC r where r.Email = ?", eMail).FirstOrDefault();
                if (cc != null)
                {
                    Db.Transact(() =>
                    {
                        cc.IsConfirmed = true;
                        cc.CnfTS = DateTime.Now;
                    });
                    master.Token = cc.Token;
                    master.Sgn.Token = cc.Token;
                    //master.MorphUrl = $"/MM0/PPs/{cc.Id}";
                }
                return master;
            });

            Handle.GET("/MM0/HHsXlsx/{?}", (long PPId) =>
            {
                return HHsXlsx(PPId);
            });

            Handle.GET("/MM0/FFsXlsx?{?}", (string queryString) =>
            {
                string decodedQuery = HttpUtility.UrlDecode(queryString);
                NameValueCollection queryCollection = HttpUtility.ParseQueryString(decodedQuery);
                string ppid = queryCollection.Get("ppid");
                string hhid = queryCollection["hhid"];
                string BasTrhX = queryCollection["bastrhx"];
                string BitTrhX = queryCollection["bittrhx"];

                long PPId = ppid == null ? 0 : long.Parse(ppid);
                long HHId = hhid == null ? 0 : long.Parse(hhid);
                BasTrhX = BasTrhX ?? "";
                BitTrhX = BitTrhX ?? "";

                return FFsXlsx(PPId, HHId, BasTrhX, BitTrhX);
            });

            Handle.GET("/MM0/HHsCumBkyXlsx/{?}", (long HHId) =>
            {
                return HHsCumBkyXlsx(HHId);
            });

        }

        public static Response HHsXlsx(long PPId)
        {
            using (ExcelPackage pck = new ExcelPackage())
            {
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("HHsRpr");

                // Header (first row)
                ws.Cells[1, 1].Value = "Hesap";
                ws.Cells["A1:A2"].Merge = true;

                ws.Cells[1, 2].Value = "Gercek";
                ws.Cells["B1:C1"].Merge = true;

                ws.Cells[1, 4].Value = "Tahmini";
                ws.Cells["D1:E1"].Merge = true;

                ws.Cells[2, 2].Value = "Gider";
                ws.Cells[2, 3].Value = "Gelir";
                ws.Cells[2, 4].Value = "Gider";
                ws.Cells[2, 5].Value = "Gelir";

                ws.Row(1).Style.Font.Bold = true;
                ws.Row(2).Style.Font.Bold = true;

                ws.Column(2).Style.Numberformat.Format = "#,###";
                ws.Column(3).Style.Numberformat.Format = "#,###";
                ws.Column(4).Style.Numberformat.Format = "#,###";
                ws.Column(5).Style.Numberformat.Format = "#,###";

                if (Db.FromId((ulong)PPId) is PP pp)
                {
                    //var hhs = Db.SQL<HH>("select r from HH r where r.PP = ?", pp);
                    int cr = 3;
                    foreach (var hh in HH.View(pp))
                    {
                        ws.Cells[cr, 1].Value = hh.Ad;
                        ws.Cells[cr, 1].Style.Indent = hh.Lvl - 1;

                        ws.Cells[cr, 2].Value = hh.GrcGdr;
                        ws.Cells[cr, 3].Value = hh.GrcGlr;
                        ws.Cells[cr, 4].Value = hh.ThmGdr;
                        ws.Cells[cr, 5].Value = hh.ThmGlr;

                        cr++;
                    }

                    //ws.Row(1).Height = 20;
                    ws.Row(1).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    ws.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Row(2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    ws.Row(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    ws.Column(1).AutoFit();
                    ws.Column(2).AutoFit();
                    ws.Column(3).AutoFit();
                    ws.Column(4).AutoFit();
                    ws.Column(5).AutoFit();
                }


                Response r = new Response();
                //r.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                r.ContentType = "application/octet-stream";
                r.Headers["Content-Disposition"] = "attachment; filename=\"HHsRpr.xlsx\"";

                var oms = new MemoryStream();
                pck.SaveAs(oms);
                oms.Seek(0, SeekOrigin.Begin);

                r.StreamedBody = oms;
                return r;
            }
        }

        public static Response FFsXlsx(long PPId, long HHId, string BasTrhX, string BitTrhX)
        {
            using (ExcelPackage pck = new ExcelPackage())
            {
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("FFsRpr");

                // Header (first row)
                ws.Cells[1, 1].Value = "Tarih";
                ws.Cells[1, 2].Value = "ÜstHesap";
                ws.Cells[1, 3].Value = "Hesap";
                ws.Cells[1, 4].Value = "Gider";
                ws.Cells[1, 5].Value = "Gelir";
                ws.Cells[1, 6].Value = "Açıklama";

                ws.Row(1).Style.Font.Bold = true;

                ws.Column(1).Style.Numberformat.Format = "dd.mm.yy";
                ws.Column(4).Style.Numberformat.Format = "#,###";
                ws.Column(5).Style.Numberformat.Format = "#,###";

                if (Db.FromId((ulong)PPId) is PP pp)
                {
                    //var ffs = Db.SQL<FF>("select r from FF r where r.PP = ? order by Trh DESC", pp);
                    int cr = 2;
                    foreach (var ff in FF.View(PPId, HHId, BasTrhX, BitTrhX))
                    {
                        ws.Cells[cr, 1].Value = ff.Trh;
                        ws.Cells[cr, 2].Value = ff.HHAdPrn;
                        ws.Cells[cr, 3].Value = ff.HHAd;
                        ws.Cells[cr, 4].Value = ff.Gdr;
                        ws.Cells[cr, 5].Value = ff.Glr;
                        ws.Cells[cr, 6].Value = ff.Ad;

                        cr++;
                    }
                    using (var range = ws.Cells["A1:F1"]) {
                        range.AutoFilter = true;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    }

                    ws.Row(1).Height = 20;
                    ws.Row(1).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    ws.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    ws.Column(1).Width = 12;
                    ws.Column(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Column(2).Width = 15;
                    ws.Column(3).Width = 15;
                    ws.Column(4).Width = 12;
                    ws.Column(5).Width = 12;
                    ws.Column(6).AutoFit();

                    ws.Cells[cr, 4].Formula = $"SUM(D2:D{cr-1})";
                    ws.Cells[cr, 5].Formula = $"SUM(E2:E{cr-1})";
                }


                Response r = new Response();
                //r.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                r.ContentType = "application/octet-stream";
                r.Headers["Content-Disposition"] = "attachment; filename=\"FFsRpr.xlsx\"";

                var oms = new MemoryStream();
                pck.SaveAs(oms);
                oms.Seek(0, SeekOrigin.Begin);

                r.StreamedBody = oms;
                return r;
            }
        }

        public static Response HHsCumBkyXlsx(long HHId)
        {
            using (ExcelPackage pck = new ExcelPackage())
            {
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("HHsCumBky");

                // Header (first row)
                ws.Cells[1, 1].Value = "Yıl";
                ws.Cells[1, 2].Value = "Ay";
                ws.Cells[1, 3].Value = "Gider";
                ws.Cells[1, 4].Value = "Gelir";
                ws.Cells[1, 5].Value = "CumKln";

                ws.Row(1).Style.Font.Bold = true;

                ws.Column(3).Style.Numberformat.Format = "#,###";
                ws.Column(4).Style.Numberformat.Format = "#,###";
                ws.Column(5).Style.Numberformat.Format = "#,###";

                string OutputFileName = "HesapToplam.xlsx";
                if (Db.FromId((ulong)HHId) is HH hh)
                {
                    OutputFileName = $"HesapToplam-{HH.FullParentAd(hh)}-{DateTime.Today.ToString("yyMMdd")}.xlsx";

                    pck.Workbook.Properties.Title = "Hesap Toplamları";
                    pck.Workbook.Properties.Author = "Şener DEMİRAL";
                    pck.Workbook.Properties.Subject = $"{hh.PP.CC.Ad}►{HH.FullParentAd(hh)}";

                    int cr = 2;
                    foreach (var ff in HH.CumBky(hh))
                    {
                        ws.Cells[cr, 1].Value = ff.Yil;
                        ws.Cells[cr, 2].Value = ff.Ay;
                        ws.Cells[cr, 3].Value = ff.Gdr;
                        ws.Cells[cr, 4].Value = ff.Glr;
                        ws.Cells[cr, 5].Value = ff.CumBky;
                        ws.Cells[cr, 6].Value = ff.Adt;

                        cr++;
                    }
                    using (var range = ws.Cells["A1:F1"])
                    {
                        range.AutoFilter = true;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    }

                    ws.Row(1).Height = 20;
                    ws.Row(1).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    ws.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    ws.Column(1).Width = 10;
                    ws.Column(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Column(2).Width = 10;
                    ws.Column(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Column(3).Width = 12;
                    ws.Column(4).Width = 12;
                    ws.Column(5).Width = 12;

                    ws.Column(6).Width = 10;
                    ws.Column(6).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }


                Response r = new Response();
                //r.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                r.ContentType = "application/octet-stream";
                //r.Headers["Content-Disposition"] = $"attachment; filename=\"HHsCumBky{DateTime.Today.ToString("yyMMdd")}.xlsx\"";
                r.Headers["Content-Disposition"] = $"attachment; filename=\"{OutputFileName}\"";

                var oms = new MemoryStream();
                pck.SaveAs(oms);
                oms.Seek(0, SeekOrigin.Begin);

                r.StreamedBody = oms;
                return r;
            }
        }

    }
}
