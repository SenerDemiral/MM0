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
            Handle.GET("/bodved/partials/aboutpage", () =>
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


            Handle.GET("/bodved/partials/PPs", () =>
            {
                var page = new PPsPage();
                //page.PPs.Data = Db.SQL<PP>("SELECT r FROM PP r order by r.RnkIdx");

                //var top = Db.SQL<long>("select COUNT(r) from PP r").FirstOrDefault();
                //var aktif = Db.SQL<long>("select count(r) from PP r where r.IsRun = ?", true).FirstOrDefault();
                //page.PPs.Data = Db.SQL<PP>("SELECT r FROM PP r order by r.Ad");

                page.Data = null;
                return page;
            });
        }
    }
}
