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
            /*
            Hook<HH>.AfterCommitInsert += (sender, id) =>
            {
                var hh = Db.FromId<HH>(id);
                HH.PostIns(hh);
            };
            
            Hook<FF>.AfterCommitInsert += (sender, id) =>
            {
                var ff = Db.FromId<FF>(id);
                FF.PostMdf(ff.HH);
            };
            Hook<FF>.AfterCommitUpdate += (sender, id) =>
            {
                var ff = Db.FromId<FF>(id);
                FF.PostMdf(ff.HH);
            };
            */

            if(Db.SQL<CC>("select r from CC r").FirstOrDefault() == null)   // DB bos ise
                CC.InsertRec("test", "test", "test", true);


        }
    }
}