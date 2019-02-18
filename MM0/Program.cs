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

            if (Db.SQL<CC>("select r from CC r where r.Email = ?", "test").FirstOrDefault() == null) 
            {
                var ccNew = CC.InsertRec("test", "test", "test", true);

                Db.Transact(() =>
                {
                    new CU
                    {
                        CC = ccNew,
                        Email = $"{ccNew.Email}/1",
                        Token = $"{ccNew.Email}/1",
                        Ad = "Test1",
                        Pwd = "test/1"
                    };
                });
            }

            string email = "aydin-dogan@live.com";
            if (Db.SQL<CC>("select r from CC r where r.Email = ?", email).FirstOrDefault() == null)
            {
                string pwd = "adlc";
                string token = Hlp.EncodeQueryString($"{email}/{pwd}");
                CC ccNew = CC.InsertRec(email, pwd, token, true);
                PP ppNew = PP.InsertRec((long)ccNew.GetObjectNo(), "TurgutreisMarina", null, null);
                Hlp.SablondanEkle(ppNew.GetObjectNo(), "HHSablonTenis");
            }

        }
    }
}