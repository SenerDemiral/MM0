﻿using MM0.ViewModels;
using Starcounter;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MM0.Api
{
    class PartialHandlers : IHandler
    {
        public void Register()
        {
            Handle.GET("/MM0/partials/aboutpage", () =>
            {
                var page = new AboutPage();
                return page;
            });

            Handle.GET("/MM0/partials/CUs/{?}", (long CCId) =>
            {
                var page = new CUsPage
                {
                    CCId = CCId
                };
                return page;
            });

            Handle.GET("/MM0/partials/CUPs/{?}", (long CUId) =>
            {
                var page = new CUPsPage
                {
                    CUId = CUId
                };
                return page;
            });

            Handle.GET("/MM0/partials/PPs/{?}", (long CCId) =>
            {
                var page = new PPsPage
                {
                    CCId = CCId
                };
                return page;
            });

            Handle.GET("/MM0/partials/TTs/{?}", (long PPId) =>
            {
                var page = new TTsPage
                {
                    PPId = PPId
                };
                return page;
            });

            Handle.GET("/MM0/partials/HHs/{?}", (long PPId) =>
            {
                var page = new HHsPage
                {
                    PPId = PPId
                };
                return page;
            });

            Handle.GET("/MM0/partials/HHsCumBky/{?}", (long HHId) =>
            {
                var page = new HHsCumBky
                {
                    HHId = HHId
                };
                return page;
            });

            Handle.GET("/MM0/partials/FFs/{?}", (long PPId) =>
            {
                var page = new FFsPage
                {
                    PPId = PPId,

                    QryTrhX = DateTime.Today.ToString("yyyy-MM-dd")
                };

                page.DlgRec.NewTrgr++;
                /*
                page.DlgRec.TrhX = page.QryTrhX;
                page.DlgRec.Opened = true;
                page.DlgRec.IsNew = true;
                */
                return page;
            });

            Handle.GET("/MM0/partials/FFsRpr?{?}", (string queryString) =>
            {
                string decodedQuery = HttpUtility.UrlDecode(queryString);
                NameValueCollection queryCollection = HttpUtility.ParseQueryString(decodedQuery);
                string PPId = queryCollection.Get("ppid");
                string HHId = queryCollection["hhid"];
                string TTId = queryCollection["ttid"];
                string BasTrhX = queryCollection["bastrhx"];
                string BitTrhX = queryCollection["bittrhx"];
                string Org = queryCollection["org"];

                if (BasTrhX == "Now")
                {
                    BasTrhX = DateTime.Today.ToString("yyyy-MM-dd");
                    BitTrhX = BasTrhX;
                }

                var page = new FFsRpr
                {
                    PPId = PPId == null ? 0 : long.Parse(PPId),
                    HHId = HHId == null ? 0 : long.Parse(HHId),
                    TTId = TTId == null ? 0 : long.Parse(TTId),
                    BasTrhX = BasTrhX ?? "",
                    BitTrhX = BitTrhX ?? "",
                    Org = Org ?? "PP",
                };
                return page;
            });

        }
    }
}
