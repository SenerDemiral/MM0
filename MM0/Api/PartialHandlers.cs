using MM0.ViewModels;
using Starcounter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            Handle.GET("/MM0/partials/PPs/{?}", (string CCId) =>
            {
                var page = new PPsPage
                {
                    CCId = CCId
                };
                return page;
            });

            Handle.GET("/MM0/partials/HHs/{?}", (string PPId) =>
            {
                var page = new HHsPage
                {
                    PPId = PPId
                };
                return page;
            });

            Handle.GET("/MM0/partials/FFsRpr/{?}", (string PPId) =>
            {
                var page = new FFsRpr
                {
                    PPId = PPId
                };
                return page;
            });

            Handle.GET("/MM0/partials/FFsRprHsp/{?}/{?}", (string PPId, string HHId) =>
            {
                var page = new FFsRpr
                {
                    PPId = PPId,
                    HHId = HHId
                };
                return page;
            });

            Handle.GET("/MM0/partials/FFsRprTrh/{?}/{?}", (string PPId, string Trh) =>
            {
                var page = new FFsRpr
                {
                    PPId = PPId,
                    Trh = Trh
                };
                return page;
            });


        }
    }
}
