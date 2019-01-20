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

            Handle.GET("/MM0/partials/PPs/{?}", (long CCId) =>
            {
                var page = new PPsPage
                {
                    CCId = CCId
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
                return page;
            });

            Handle.GET("/MM0/partials/FFsRpr?{?}", (string queryString) =>
            {
                string decodedQuery = HttpUtility.UrlDecode(queryString);
                NameValueCollection queryCollection = HttpUtility.ParseQueryString(decodedQuery);
                string PPId = queryCollection.Get("ppid");
                string HHId = queryCollection["hhid"];
                string TrhX = queryCollection["trhx"];

                var page = new FFsRpr
                {
                    PPId = PPId == null ? 0 : long.Parse(PPId),
                    HHId = HHId == null ? 0 : long.Parse(HHId),
                    TrhX = TrhX ?? ""
                };
                return page;
            });

        }
    }
}
