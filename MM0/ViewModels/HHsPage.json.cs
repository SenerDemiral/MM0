using DBMM0;
using Starcounter;
using System;

namespace MM0.ViewModels
{
    partial class HHsPage : Json
    {
        protected override void OnData()
        {
            base.OnData();

            ulong ppId = Convert.ToUInt64(PPId);
            if (Db.FromId(ppId) is PP pp)
            {
                Hdr = $"{pp.Ad} ► Hesapları";
                HHs.Data = DBMM0.HH.View(pp);  //Db.SQL<HH>("select r from HH r");
            }
        }
    }
}
