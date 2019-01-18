using DBMM0;
using Starcounter;

namespace MM0.ViewModels
{
    partial class HHsCumBky : Json
    {
        protected override void OnData()
        {
            base.OnData();

            if (Db.FromId((ulong)HHId) is HH hh)
            {
                Hdr = $"{hh.PP.CC.Ad}►{HH.FullParentAd(hh)}";

                AAs.Data = HH.CumBky(hh);
            }
        }
    }
}
