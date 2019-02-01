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

            //var r = Root as MasterPage;
            //DlgFlt.BasTrhX = r.BasTrhX;
            //DlgFlt.BitTrhX = r.BitTrhX;

            if (Db.FromId((ulong)HHId) is HH hh)
                Hdr = $"{hh.PP.CC.Ad}►{HH.FullParentAd(hh)}";
            else if (Db.FromId((ulong)PPId) is PP pp)
            {
                Hdr = $"{pp.CC.Ad}►{pp.Ad}";

                if (!string.IsNullOrEmpty(BasTrhX))
                {
                    DateTime basTrh = Convert.ToDateTime(BasTrhX);
                    //string basT = string.Format(Hlp.cultureTR, "{0:dd.MMMM yyyy dddd}", basTrh);
                    DateTime bitTrh = Convert.ToDateTime(BitTrhX);
                    //string bitT = string.Format(Hlp.cultureTR, "{0:dd MMMM yyyy dddd}", bitTrh);

                    if(basTrh == bitTrh)
                        Hdr = $"{pp.CC.Ad}►{pp.Ad}►{basTrh:dd.MM.yy}";
                    else
                        Hdr = $"{pp.CC.Ad}►{pp.Ad}►{basTrh:dd.MM.yy} >=< {bitTrh:dd.MM.yy}";
                }
            }

            IEnumerable<FF> ffs = FF.View(PPId, HHId, BasTrhX, BitTrhX);
            
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

            if (Db.FromId((ulong)PPId) is PP pp2)
            {
                HHs.Data = Db.SQL<HH>("select r from HH r where r.PP = ? and r.Skl = ? order by r.AdFull", pp2, 99); // Sadece Leafs
                TTs.Data = Db.SQL<TT>("select r from TT r where r.PP = ? order by r.Ad", pp2);
            }
        }

        public void RefreshToplam()
        {
            IEnumerable<FF> ffs = FF.View(PPId, HHId, BasTrhX, BitTrhX);

            decimal GlrTop = 0, GdrTop = 0;
            int cnt = 0;
            foreach (var ff in ffs)
            {
                GlrTop += ff.Glr;
                GdrTop += ff.Gdr;
                cnt++;
            }
            NORX = $"Kayıt Sayısı: {cnt:n0}";
            GlrTopX = $"{GlrTop:#,#.##;-#,#.##;#}";
            GdrTopX = $"{GdrTop:#,#.##;-#,#.##;#}";
        }


        void Handle(Input.DwnldTrgr Action)
        {
            //MorphUrl = $"/mm0/FFsXlsx/{PPId}";
            MorphUrl = $"/mm0/FFsXlsx?ppid={PPId}&hhid={HHId}&bastrhx={BasTrhX}&bittrhx={BitTrhX}";
        }

    }

    [FFsRpr_json.DlgFlt]
    partial class FFsFltDlgPartial : Json
    {
        void Handle(Input.OpnTrgr Action)
        {
            var r = Root as MasterPage;
            BasTrhX = r.BasTrhX;
            BitTrhX = r.BitTrhX;
            Opened = true;
        }
        void Handle(Input.FltTrgr Action)
        {
            var r = Root as MasterPage;
            r.BasTrhX = BasTrhX;
            r.BitTrhX = BitTrhX;

            var p = this.Parent as FFsRpr;
            p.BasTrhX = BasTrhX;
            p.BitTrhX = BitTrhX;
            p.HHId = 0;

            Opened = false;
            p.Data = null;
        }
    }

    [FFsRpr_json.DlgRec]
    partial class FFsRprDlgPartial : Json
    {
        void Handle(Input.NewTrgr Action)
        {
            var p = this.Parent as FFsRpr;
            Id = 0;
            HHId = 0;
            TrhX = DateTime.Today.ToString("yyyy-MM-dd");
            Msj = "";
            IsNew = true;
            Opened = true;
        }

        void Handle(Input.InsTrgr Action)
        {
            //TrhX girilmediginde 00:00:00 geliyor. DateTime'a convert edince Gunun tarihi geliyor!
            //When only the time is present, the date portion uses the current date.
            //When only the date is present, the time portion is midnight.
            //When the year isn't specified in a date, the current year is used.
            //When the day of the month isn't specified, the first of the month is used.

            //How to set only time part of a DateTime variable
            //var newDate = oldDate.Date + new TimeSpan(11, 30, 55);

            var p = this.Parent as FFsRpr;

            Msj = FF.InsertRec(p.PPId, HHId, TTId, TrhX, Ad, Gdr, Glr);
            if (!string.IsNullOrEmpty(Msj))
            {
                Action.Cancelled = true;
                return;
            }

            Id = 0;
            Opened = false;

            p.Data = null;

            Session.RunTaskForAll((s, sId) => {
                var cp = (s.Store[nameof(MasterPage)] as MasterPage).CurrentPage;
                if (cp is FFsPage)
                {
                    (s.Store[nameof(MasterPage)] as MasterPage).CurrentPage.Data = null;
                    s.CalculatePatchAndPushOnWebSocket();
                }
            });

        }

        void Handle(Input.UpdTrgr Action)
        {
            if (Id != 0)
            {
                FF.UpdateRec(Id, HHId, TTId, TrhX, Ad, Gdr, Glr);

                var p = this.Parent as FFsRpr;
                p.RefreshToplam();

                Id = 0;

                Session.RunTaskForAll((s, id) =>
                {
                    s.CalculatePatchAndPushOnWebSocket();
                });
            }
            Opened = false;

        }

        void Handle(Input.DelTrgr Action)
        {
            Msj = FF.DeleteRec(Id);
            if (!string.IsNullOrEmpty(Msj))
            {
                Action.Cancelled = true;
                return;
            }

            var p = this.Parent as FFsRpr;
            p.Data = null;

            Session.RunTaskForAll((s, sId) => {
                var cp = (s.Store[nameof(MasterPage)] as MasterPage).CurrentPage;
                if (cp is FFsPage)
                {
                    (s.Store[nameof(MasterPage)] as MasterPage).CurrentPage.Data = null;
                    s.CalculatePatchAndPushOnWebSocket();
                }
            });

            Opened = false;
        }
    }

    [FFsRpr_json.FFs]
    partial class FFsRprPartial : Json
    {
        void Handle(Input.EdtTrgr Action)
        {
            var p = this.Parent.Parent as FFsRpr;

            if (Db.FromId((ulong)Id) is FF ff)
            {
                p.DlgRec.Id = Id;
                p.DlgRec.Ad = ff.Ad;
                p.DlgRec.Gdr = ff.Gdr;
                p.DlgRec.Glr = ff.Glr;

                p.DlgRec.HHId = (long)ff.HHId;
                p.DlgRec.TTId = (long)ff.TTId;

                p.DlgRec.TrhX = ff.Trh.ToString("yyyy-MM-dd");

                p.DlgRec.Msj = "";
                p.DlgRec.IsNew = false; // Edit
                p.DlgRec.Opened = true;
            }
        }

    }
}
