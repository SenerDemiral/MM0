using DBMM0;
using Starcounter;
using System;

namespace MM0.ViewModels
{
    partial class PPsPage : Json
    {
        protected override void OnData()
        {
            base.OnData();

            ulong ccId = Convert.ToUInt64(CCId);
            if (Db.FromId(ccId) is CC cc)
            {
                var hh = cc.HHroot;
                GrcGlrTopX = hh.GrcGlrX;
                GrcGdrTopX = hh.GrcGdrX;
                ThmGlrTopX = hh.ThmGlrX;
                ThmGdrTopX = hh.ThmGdrX;

                var recs = Db.SQL<PP>("select r from PP r");
                PPs.Data = recs;

            }
        }
    }
}
