using MM0.ViewModels;
using Starcounter;
using System;
using System.Collections.Generic;
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

            if (master.CurrentPage != null && master.CurrentPage.GetType() == typeof(T))
            {
                return master;
            }

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

        }
    }
}
