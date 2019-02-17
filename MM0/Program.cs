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

            if (Db.SQL<CC>("select r from CC r").FirstOrDefault() == null)   // DB bos ise
            {
                CC.InsertRec("test", "test", "test", true);

                if (Db.SQL<CU>("select r from CU r").FirstOrDefault() == null)   // CU bos ise
                {
                    CC cc = Db.FromId<CC>(1);

                    Db.Transact(() =>
                    {
                        new CU
                        {
                            CC = cc,
                            Email = $"{cc.Email}/1",
                            Token = $"{cc.Email}/1",
                            Ad = "Test1",
                            Pwd = "test/1"
                        };
                    });
                }
            }
        }
    }
}