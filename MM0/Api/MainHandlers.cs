using DBMM0;
using MM0.ViewModels;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Starcounter;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            if (curSession != null)     // Ilk girisinde URL ne olursa olsun MainPage'e gonder
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
                master = new MasterPage
                {
                    //NavPage = Self.GET("/uniformdocs/nav")
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
            Handle.GET("/MM0/PPs/{?}", (string CCId) =>
            {
                MasterPage master = GetMasterPageFromSession();
                //if (master.Token == "")
                //    return "Not SignIn yet!";

                return WrapPage<PPsPage>($"/MM0/partials/PPs/{CCId}");
            });

            Handle.GET("/MM0/HHs/{?}", (string PPId) => WrapPage<HHsPage>($"/MM0/partials/HHs/{PPId}"));


            Handle.GET("/MM0/FFsRpr/{?}", (string PPId) => WrapPage<FFsRpr>($"/MM0/partials/FFsRpr/{PPId}"));
            Handle.GET("/MM0/FFsRprHsp/{?}/{?}", (string PPId, string HHId) => WrapPage<FFsRpr>($"/MM0/partials/FFsRprHsp/{PPId}/{HHId}"));
            Handle.GET("/MM0/FFsRprTrh/{?}/{?}", (string PPId, string Trh) => WrapPage<FFsRpr>($"/MM0/partials/FFsRprTrh/{PPId}/{Trh}"));

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
                    });
                    master.Token = cc.Token;
                    //return master; // Self.GET("/bodved/DDs");
                }
                return master;
            });

            Handle.GET("/MM0/FFsRprXlsx/{?}", (string PPId) =>
            {
                return FFsRprXlsx(PPId);
            });

        }

        public static Response FFsRprXlsx(string PPId)
        {
            using (ExcelPackage pck = new ExcelPackage())
            {
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("FFsRpr");

                // Header (first row)
                ws.Cells[1, 1].Value = "Tarih";
                ws.Cells[1, 2].Value = "Hesap";
                ws.Cells[1, 3].Value = "Gider";
                ws.Cells[1, 4].Value = "Gelir";
                ws.Cells[1, 5].Value = "Açıklama";

                ws.Row(1).Style.Font.Bold = true;

                ws.Column(1).Style.Numberformat.Format = "dd.mm.yy";
                ws.Column(3).Style.Numberformat.Format = "#,###";
                ws.Column(4).Style.Numberformat.Format = "#,###";

                if (Db.FromId(Convert.ToUInt64(PPId)) is PP pp)
                {
                    var ffs = Db.SQL<FF>("select r from FF r where r.PP = ? and r.Trh < ?", pp, DateTime.Today.AddDays(30));
                    int cr = 2;
                    foreach (var ff in ffs)
                    {
                        ws.Cells[cr, 1].Value = ff.Trh;
                        ws.Cells[cr, 2].Value = ff.HHAd;
                        ws.Cells[cr, 3].Value = ff.Gdr;
                        ws.Cells[cr, 4].Value = ff.Glr;
                        ws.Cells[cr, 5].Value = ff.Ad;

                        cr++;
                    }
                    using (var range = ws.Cells["A1:E1"]) {
                        range.AutoFilter = true;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    }

                    ws.Row(1).Height = 20;
                    ws.Row(1).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    ws.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    ws.Column(1).Width = 12;
                    ws.Column(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Column(2).AutoFit();
                    ws.Column(3).Width = 12;
                    ws.Column(4).Width = 12;
                    ws.Column(5).AutoFit();

                    ws.Cells[cr, 3].Formula = $"SUM(C2:C{cr-1})";
                    ws.Cells[cr, 4].Formula = $"SUM(D2:D{cr-1})";
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

    }
}
