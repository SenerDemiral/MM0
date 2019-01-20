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
            if (Db.FromId((ulong)HHId) is HH hh)
                Hdr = $"{hh.PP.CC.Ad}►{HH.FullParentAd(hh)}";
            else if (Db.FromId((ulong)PPId) is PP pp)
            {
                Hdr = $"{pp.CC.Ad}►{pp.Ad}";

                if (!string.IsNullOrEmpty(TrhX))
                {
                    DateTime trh = Convert.ToDateTime(TrhX);
                    string tt = string.Format(Hlp.cultureTR, "{0:dd MMMM yyyy dddd}", trh);

                    Hdr = $"{pp.CC.Ad}►{pp.Ad}►{tt}";
                }
            }

            IEnumerable<FF> ffs = FF.View(PPId, HHId, TrhX);
            
            /*
            if (HHId != 0)
            {
                if (Db.FromId((ulong)HHId) is HH hh)
                {
                    Hdr = $"{hh.PP.CC.Ad}►{HH.FullParentAd(hh)}";

                    //if (hh.Skl == 99)    // Leaf ise sadece kendi gecenleri
                    //    ffs = Db.SQL<FF>("select r from FF r where r.HH = ? order by r.Trh DESC", hh);
                    //else // Altindaki Leaf leri
                        ffs = FF.View(hh);
                }
            }

            if (Db.FromId((ulong)PPId) is PP pp)
            {
                Hdr = $"{pp.CC.Ad}►{pp.Ad}";

                if (!string.IsNullOrEmpty(TrhX))
                {
                    DateTime trh = Convert.ToDateTime(TrhX);
                    string tt = string.Format(Hlp.cultureTR, "{0:dd MMMM yyyy dddd}", trh);
                    Hdr = $"{pp.CC.Ad}►{pp.Ad}►{tt}";
                    //Hdr = $"{pp.Ad} ► {trh.Date:dddd dd.MMMM.yyyy} ► İşlemleri";
                    //ffs = Db.SQL<FF>("select r from FF r where r.PP = ? and r.Trh >= ? and r.Trh < ? order by r.Trh DESC", pp, trh.Date, trh.Date.AddDays(1));
                    //ffs = Db.SQL<FF>("select r from FF r where r.PP = ? and r.Trh >= ? and r.Trh < ?", pp, trh.Date, trh.Date.AddDays(1));
                    ffs = Db.SQL<FF>("select r from FF r where r.PP = ? and r.Trh = ?", pp, trh);
                }
                else
                    ffs = Db.SQL<FF>("select r from FF r where r.PP = ? order by r.Trh DESC", pp);
            }
            */
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

        void Handle(Input.DwnldTrgr Action)
        {
            //MorphUrl = $"/mm0/FFsXlsx/{PPId}";
            MorphUrl = $"/mm0/FFsXlsx?ppid={PPId}&hhid={HHId}&trhx={TrhX}";
        }

    }
}
