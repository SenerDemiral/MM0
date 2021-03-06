﻿using DBMM0;
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
                    if (cc == null)
                    {
                        var cu = Db.SQL<CU>("select r from CU r where r.Token = ?", master.Token).FirstOrDefault();
                        if (cu != null)
                            cc = cu.CC;
                    }
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

            Handle.GET("/MM0/CUs/{?}", (string CCId) =>
            {
                MasterPage master = GetMasterPageFromSession();
                if (master.CUId == 0)
                    return WrapPage<CUsPage>($"/MM0/partials/CUs/{CCId}");
                return null;
            });
            Handle.GET("/MM0/CUPs/{?}", (string CUId) =>
            {
                MasterPage master = GetMasterPageFromSession();
                if (master.CUId == 0)
                    return WrapPage<CUPsPage>($"/MM0/partials/CUPs/{CUId}");
                return null;
            });

            Handle.GET("/MM0/TTs/{?}", (long PPId) => WrapPage<TTsPage>($"/MM0/partials/TTs/{PPId}"));

            Handle.GET("/MM0/HHs/{?}", (long PPId) => WrapPage<HHsPage>($"/MM0/partials/HHs/{PPId}"));

            Handle.GET("/MM0/HHsCumBky/{?}", (long HHId) => WrapPage<HHsPage>($"/MM0/partials/HHsCumBky/{HHId}"));

            Handle.GET("/MM0/FFs/{?}", (long PPId) => WrapPage<FFsPage>($"/MM0/partials/FFs/{PPId}"));

            Handle.GET("/MM0/FFsRpr?{?}", (string queryString) =>
            {
                return WrapPage<FFsRpr>($"/MM0/partials/FFsRpr?{queryString}");
            });

            Handle.GET("/MM0/confirmemail/{?}", (string token) =>
            {
                MasterPage master = GetMasterPageFromSession();
                var eMailPwd = Hlp.DecodeQueryString(token);

                int iof = eMailPwd.IndexOf("/");
                string eMail = eMailPwd.Substring(0, iof);
                string pwd = eMailPwd.Substring(iof+1);
                master.Token = "";

                CC cc = Db.SQL<CC>("select r from CC r where r.Email = ? and r.Pwd = ?", eMail, pwd).FirstOrDefault();
                if (cc != null)
                {
                    Db.Transact(() =>
                    {
                        cc.IsConfirmed = true;
                        cc.CnfTS = DateTime.Now;
                    });

                    master.Token = cc.Token;    // MasterPage.html de dolu geldigi icin AutoSignIn i bu token ile yapar
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
                string ttid = queryCollection["ttid"];
                string BasTrhX = queryCollection["bastrhx"];
                string BitTrhX = queryCollection["bittrhx"];

                long PPId = ppid == null ? 0 : long.Parse(ppid);
                long HHId = hhid == null ? 0 : long.Parse(hhid);
                long TTId = ttid == null ? 0 : long.Parse(ttid);
                BasTrhX = BasTrhX ?? "";
                BitTrhX = BitTrhX ?? "";

                return FFsXlsx(PPId, HHId, TTId, BasTrhX, BitTrhX);
            });

            Handle.GET("/MM0/HHsCumBkyXlsx/{?}", (long HHId) =>
            {
                return HHsCumBkyXlsx(HHId);
            });

            Handle.GET("/MM0/TTsXlsx/{?}", (long PPId) =>
            {
                return TTsXlsx(PPId);
            });
        }


        public static Response TTsXlsx(long PPId)
        {
            using (ExcelPackage pck = new ExcelPackage())
            {
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("TTsRpr");

                // Header (first row)
                ws.Cells[1, 1].Value = "Ad";
                ws.Cells[1, 2].Value = "Not";
                ws.Cells[1, 3].Value = "eMail";
                ws.Cells[1, 4].Value = "Tel";

                ws.Row(1).Style.Font.Bold = true;
                using (var range = ws.Cells["A1:D1"])
                {
                    range.AutoFilter = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                }

                if (Db.FromId((ulong)PPId) is PP pp)
                {
                    var tts = Db.SQL<TT>("select r from TT r where r.PP = ?", pp);
                    int cr = 2;
                    foreach (var tt in tts)
                    {
                        ws.Cells[cr, 1].Value = tt.Ad;
                        ws.Cells[cr, 2].Value = tt.Info;
                        ws.Cells[cr, 3].Value = tt.Email;
                        ws.Cells[cr, 4].Value = tt.Tel;

                        cr++;
                    }

                    ws.Column(1).AutoFit();
                    ws.Column(2).AutoFit();
                    ws.Column(3).AutoFit();
                    ws.Column(4).AutoFit();
                }


                Response r = new Response();
                //r.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                r.ContentType = "application/octet-stream";
                r.Headers["Content-Disposition"] = "attachment; filename=\"TTsRpr.xlsx\"";

                var oms = new MemoryStream();
                pck.SaveAs(oms);
                oms.Seek(0, SeekOrigin.Begin);

                r.StreamedBody = oms;
                return r;
            }
        }

        public static Response HHsXlsx(long PPId)
        {
            using (ExcelPackage pck = new ExcelPackage())
            {
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("HHsRpr");

                // Header (first row)
                ws.Cells[1, 1].Value = "Hesap";
                ws.Cells["A1:A2"].Merge = true;
                ws.Cells[1, 6].Value = "Not";
                ws.Cells["F1:F2"].Merge = true;

                ws.Cells[1, 2].Value = "Gercek";
                ws.Cells["B1:C1"].Merge = true;

                ws.Cells[1, 4].Value = "Hedef";
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
                        ws.Cells[cr, 1].Value = hh.AdFull;
                        //ws.Cells[cr, 1].Value = hh.Ad;
                        //ws.Cells[cr, 1].Style.Indent = hh.Lvl - 1;

                        ws.Cells[cr, 2].Value = hh.GrcGdr;
                        ws.Cells[cr, 3].Value = hh.GrcGlr;
                        ws.Cells[cr, 4].Value = hh.ThmGdr;
                        ws.Cells[cr, 5].Value = hh.ThmGlr;
                        ws.Cells[cr, 6].Value = hh.Info;

                        cr++;
                    }
                    using (var range = ws.Cells["A2:F2"])
                    {
                        range.AutoFilter = true;
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
                    ws.Column(6).AutoFit();

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

        public static Response FFsXlsx(long PPId, long HHId, long TTId, string BasTrhX, string BitTrhX)
        {
            using (ExcelPackage pck = new ExcelPackage())
            {
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("FFsRpr");

                // Header (first row)
                ws.Cells[1, 1].Value = "Tarih";
                ws.Cells["A1:A2"].Merge = true;
                ws.Cells[1, 2].Value = "Hesap";
                ws.Cells["B1:B2"].Merge = true;
                ws.Cells[1, 3].Value = "Etiket";
                ws.Cells["C1:C2"].Merge = true;

                ws.Cells[1, 8].Value = "Not";
                ws.Cells["H1:H2"].Merge = true;

                ws.Cells[1, 4].Value = "Gercek";
                ws.Cells["D1:E1"].Merge = true;

                ws.Cells[1, 6].Value = "Beklenen";
                ws.Cells["F1:G1"].Merge = true;

                ws.Cells[2, 4].Value = "Gelir";
                ws.Cells[2, 5].Value = "Gider";
                ws.Cells[2, 6].Value = "Gelir";
                ws.Cells[2, 7].Value = "Gider";

                ws.Row(1).Style.Font.Bold = true;

                ws.Column(1).Style.Numberformat.Format = "dd.mm.yy";
                ws.Column(4).Style.Numberformat.Format = "#,###";
                ws.Column(5).Style.Numberformat.Format = "#,###";
                ws.Column(6).Style.Numberformat.Format = "#,###";
                ws.Column(7).Style.Numberformat.Format = "#,###";

                if (Db.FromId((ulong)PPId) is PP pp)
                {
                    string Hdr = $"{pp.CC.Ad}►{pp.Ad}";

                    if (!string.IsNullOrEmpty(BasTrhX))
                    {
                        DateTime basTrh = Convert.ToDateTime(BasTrhX);
                        DateTime bitTrh = Convert.ToDateTime(BitTrhX);

                        if (basTrh == bitTrh)
                            Hdr = $"{pp.CC.Ad}►{pp.Ad}►{basTrh:dd.MM.yy}";
                        else
                            Hdr = $"{pp.CC.Ad}►{pp.Ad}►{basTrh:dd.MM.yy} >=< {bitTrh:dd.MM.yy}";
                    }

                    if (Db.FromId((ulong)HHId) is HH hh)
                        Hdr = $"{Hdr}►{HH.FullParentAd(hh)}";
                    if (Db.FromId((ulong)TTId) is TT tt)
                        Hdr = $"{Hdr}►{tt.Ad}";

                    ws.HeaderFooter.OddHeader.CenteredText = Hdr;

                    int cr = 3;
                    IEnumerable<FF> ffs = FF.View(PPId, HHId, TTId, BasTrhX, BitTrhX);

                    foreach (var ff in ffs.OrderByDescending((x) => x.Trh))
                    {
                        ws.Cells[cr, 1].Value = ff.Trh;
                        ws.Cells[cr, 2].Value = ff.HHAdFull;
                        ws.Cells[cr, 3].Value = ff.TTAd;
                        ws.Cells[cr, 4].Value = ff.Glr;
                        ws.Cells[cr, 5].Value = ff.Gdr;
                        ws.Cells[cr, 6].Value = ff.BklGlr;
                        ws.Cells[cr, 7].Value = ff.BklGdr;
                        ws.Cells[cr, 8].Value = ff.Ad;

                        cr++;
                    }
                    using (var range = ws.Cells["A2:H2"]) {
                        range.AutoFilter = true;
                        //range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        //range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    }

                    ws.Row(1).Height = 20;
                    ws.Row(1).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    ws.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    ws.Column(1).Width = 12;
                    ws.Column(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Column(2).AutoFit();
                    ws.Column(3).AutoFit();
                    ws.Column(4).Width = 12;
                    ws.Column(5).Width = 12;
                    ws.Column(6).Width = 12;
                    ws.Column(7).Width = 12;
                    ws.Column(8).AutoFit();

                    ws.Cells[cr, 4].Formula = $"SUM(D2:D{cr-1})";
                    ws.Cells[cr, 5].Formula = $"SUM(E2:E{cr-1})";
                    ws.Cells[cr, 6].Formula = $"SUM(F2:F{cr-1})";
                    ws.Cells[cr, 7].Formula = $"SUM(G2:G{cr-1})";

                    using (var range = ws.Cells[$"D{cr}:G{cr}"])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    }
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
