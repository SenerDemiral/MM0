using DBMM0;
using OfficeOpenXml;
using Starcounter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MM0.ViewModels
{
    partial class FFsRpr : Json
    {
        protected override void OnData()
        {
            base.OnData();

            if (Db.FromId((ulong)PPId) is PP pp)
            {
                Hdr = $"{pp.CC.Ad} ► {pp.Ad}";
                IEnumerable<FF> ffs = Db.SQL<FF>("select r from FF r where r.PP = ? order by r.Trh DESC", pp);

                if (HHId != 0)
                {
                    if (Db.FromId((ulong)HHId) is HH hh)
                    {
                        Hdr = $"{pp.CC.Ad} ► {HH.FullParentAd(hh)}";
                        if (hh.Skl == 99)    // Leaf ise sadece kendi gecenleri
                            ffs = Db.SQL<FF>("select r from FF r where r.HH = ?", hh);
                        else // Altindaki Leaf leri
                            ffs = FF.View(hh); //Db.SQL<FF>("select r from FF r where r.PP = ? and r.HH = ? order by r.Trh DESC", pp, hh);
                    }
                }
                else if (!string.IsNullOrEmpty(Trh))
                {
                    DateTime trh = Convert.ToDateTime(Trh);
                    string tt = string.Format(Hlp.cultureTR, "{0:dd MMMM yyyy dddd}", trh);
                    Hdr = $"{pp.CC.Ad} ► {pp.Ad} ► {tt}";
                    //Hdr = $"{pp.Ad} ► {trh.Date:dddd dd.MMMM.yyyy} ► İşlemleri";
                    //ffs = Db.SQL<FF>("select r from FF r where r.PP = ? and r.Trh >= ? and r.Trh < ? order by r.Trh DESC", pp, trh.Date, trh.Date.AddDays(1));
                    ffs = Db.SQL<FF>("select r from FF r where r.PP = ? and r.Trh >= ? and r.Trh < ?", pp, trh.Date, trh.Date.AddDays(1));
                }
                FFs.Data = ffs.OrderByDescending((x) => x.Trh);
                NORX = $"Kayıt Sayısı: {FFs.Count:n0}";

                decimal GlrTop = 0, GdrTop = 0;
                foreach (var ff in ffs)
                {
                    GlrTop += ff.Glr;
                    GdrTop += ff.Gdr;
                }
                GlrTopX = $"{GlrTop:#,#.##;-#,#.##;#}";
                GdrTopX = $"{GdrTop:#,#.##;-#,#.##;#}";
            }
        }

    }
}
