using System;
using Starcounter;
using DBMM0;
using System.Linq;
using MM0.Api;

namespace MM0
{
    class Program
    {
        static void Main()
        {
            Hlp.Indexes();

            IHandler[] handlers = new IHandler[]
            {
                new MainHandlers(),
                new PartialHandlers()
                //new HookHandlers()
            };

            foreach (IHandler handler in handlers)
            {
                handler.Register();
            }


            HH.Populate();
            HH.Display();

            //HHspage hp = new HHspage();
            //hp.HHs.Data = HH.sener(2);
            //hp.HHs.Data = Db.SQL<HH>("select r from HH r");
            
            Db.Transact(() =>
            {
                new FF
                {
                    PP = Db.FromId(4) is PP pp ? pp : null,  // Proje1
                    Ad = "Apple 1 Gdrrrrrrrrr",
                    HH = Db.FromId(9) is HH hh ? hh : null,  // Apple
                    Trh = DateTime.Now,
                    Gdr = 456.78M
                };
                FF.PostFF(9);

                new FF
                {
                    PP = Db.FromId(4) is PP pp2 ? pp2 : null,  // Proje1
                    Ad = "Apple 4 Glr",
                    HH = Db.FromId(12) is HH hh2 ? hh2 : null,  // Meat
                    Trh = DateTime.Now,
                    Glr = 200.67M
                };
                FF.PostFF(9);
            });
            


        }
    }
}