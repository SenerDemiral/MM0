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


            //HH.Populate();
            //HH.Display();

            //HHspage hp = new HHspage();
            //hp.HHs.Data = HH.sener(2);
            //hp.HHs.Data = Db.SQL<HH>("select r from HH r");

            /*
            Random gen = new Random();
            //DateTime randomDate = DateTime.Today.AddDays(gen.Next(range));
            ulong hhGlrId = 0;
            ulong hhGdrId = 0;
            HH hhGlr, hhGdr;
            Db.Transact(() =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    hhGdrId = (ulong)gen.Next(8, 14);
                    if (hhGdrId == 10 || hhGdrId == 12)
                        hhGdrId++;
                    hhGdr = Db.FromId(hhGdrId) is HH hh ? hh : null;
                    new FF
                    {   
                        PP = Db.FromId(3) is PP pp ? pp : null,  // Proje1
                        Ad = "Apple 1 Gdrrrrrrrrr",
                        HH = hhGdr,  // Apple
                        Trh = DateTime.Today.AddDays(gen.Next(365)),
                        Gdr = gen.Next(1, 100) * 100
                    };
                    FF.PostFF(hhGdr);
                }
                for (int i = 0; i < 10; i++)
                {
                    hhGlrId = (ulong)gen.Next(8, 14);
                    if (hhGlrId == 10 || hhGlrId == 12)
                        hhGlrId++;
                    hhGlr = Db.FromId(hhGlrId) is HH hh2 ? hh2 : null;
                    new FF
                    {
                        PP = Db.FromId(3) is PP pp2 ? pp2 : null,  // Proje1
                        Ad = "Apple 4 Glr",
                        HH = hhGlr,  // Meat
                        Trh = DateTime.Today.AddDays(gen.Next(365)),
                        Glr = gen.Next(1, 100) * 10000
                    };
                    FF.PostFF(hhGlr);
                }
            });
            */

        }
    }
}