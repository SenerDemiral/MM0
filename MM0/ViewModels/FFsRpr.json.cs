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
            /*
            if (Db.FromId((ulong)HHId) is HH hh)
                Hdr = $"{hh.PP.CC.Ad}►{HH.FullParentAd(hh)}";
            else if (Db.FromId((ulong)TTId) is TT tt)
                Hdr = $"{tt.PP.CC.Ad}►{tt.Ad}";
*/
            DateTime basTrh, bitTrh;
            if (Db.FromId((ulong)PPId) is PP pp)
            {
                Hdr = $"{pp.CC.Ad}►{pp.Ad}";

                if (string.IsNullOrEmpty(BasTrhX))
                    basTrh = DateTime.MinValue;
                else
                    basTrh = Convert.ToDateTime(BasTrhX);
                if (string.IsNullOrEmpty(BitTrhX))
                    bitTrh = DateTime.MaxValue;
                else
                    bitTrh = Convert.ToDateTime(BitTrhX);

                string TrhTurX = "İşlem";
                switch (TrhTur)
                {
                    case "I":
                        TrhTurX = "Giriş";
                        break;
                    case "U":
                        TrhTurX = "Edit";
                        break;
                }

                if (basTrh == bitTrh)
                   Hdr = $"{pp.CC.Ad}►{pp.Ad}►{TrhTurX}►{basTrh:dd.MM.yy}";
                else
                    Hdr = $"{pp.CC.Ad}►{pp.Ad}►{TrhTurX}►{basTrh:dd.MM.yy} >=< {bitTrh:dd.MM.yy}";
            }

            if (Db.FromId((ulong)HHId) is HH hh)
                Hdr = $"{Hdr}►{HH.FullParentAd(hh)}";
            if (Db.FromId((ulong)TTId) is TT tt)
                Hdr = $"{Hdr}►{tt.Ad}";

            IEnumerable<FF> ffs = FF.View(PPId, HHId, TTId, BasTrhX, BitTrhX, TrhTur);
            FFs.Data = ffs; //.OrderByDescending((x) => x.Trh);

            NORX = $"{FFs.Count:n0} Kayıt";
            decimal GlrTop = 0, GdrTop = 0;
            decimal BklGlrTop = 0, BklGdrTop = 0;
            foreach (var ff in ffs)
            {
                GlrTop += ff.Glr;
                GdrTop += ff.Gdr;
                BklGlrTop += ff.BklGlr;
                BklGdrTop += ff.BklGdr;
            }
            GlrTopX = $"{GlrTop:#,#.##;-#,#.##;#}";
            GdrTopX = $"{GdrTop:#,#.##;-#,#.##;#}";
            BklGlrTopX = $"{BklGlrTop:#,#.##;-#,#.##;#}";
            BklGdrTopX = $"{BklGdrTop:#,#.##;-#,#.##;#}";

            if (Db.FromId((ulong)PPId) is PP pp2)
            {
                HHs.Data = Db.SQL<HH>("select r from HH r where r.PP = ? and r.Skl = ? order by r.AdFull", pp2, 99); // Sadece Leafs
                TTs.Data = Db.SQL<TT>("select r from TT r where r.PP = ? order by r.Ad", pp2);
            }
        }

        public void RefreshToplam()
        {
            IEnumerable<FF> ffs = FF.View(PPId, HHId, TTId, BasTrhX, BitTrhX, TrhTur);

            decimal GlrTop = 0, GdrTop = 0;
            decimal BklGlrTop = 0, BklGdrTop = 0;
            int cnt = 0;
            foreach (var ff in ffs)
            {
                GlrTop += ff.Glr;
                GdrTop += ff.Gdr;
                BklGlrTop += ff.BklGlr;
                BklGdrTop += ff.BklGdr;
                cnt++;
            }
            NORX = $"Kayıt Sayısı: {cnt:n0}";
            GlrTopX = $"{GlrTop:#,#.##;-#,#.##;#}";
            GdrTopX = $"{GdrTop:#,#.##;-#,#.##;#}";
            BklGlrTopX = $"{BklGlrTop:#,#.##;-#,#.##;#}";
            BklGdrTopX = $"{BklGdrTop:#,#.##;-#,#.##;#}";
        }


        void Handle(Input.DwnldTrgr Action)
        {
            //MorphUrl = $"/mm0/FFsXlsx/{PPId}";
            DwnldUrl = $"/mm0/FFsXlsx?ppid={PPId}&hhid={HHId}&ttid={TTId}&bastrhx={BasTrhX}&bittrhx={BitTrhX}&trhtur={TrhTur}";
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
            var p = this.Parent as FFsRpr;
            HHId = p.HHId;
            TTId = p.TTId;
            TrhTur = "R";
            Opened = true;
        }
        void Handle(Input.FltTrgr Action)
        {
            var r = Root as MasterPage;
            var p = this.Parent as FFsRpr;

            p.MorphUrl = $"/mm0/FFsRpr?ppid={p.PPId}&hhid={HHId}&ttid={TTId}&bastrhx={BasTrhX}&bittrhx={BitTrhX}&trhtur={TrhTur}";
            
            r.BasTrhX = BasTrhX;
            r.BitTrhX = BitTrhX;
            /*
            var p = this.Parent as FFsRpr;
            p.BasTrhX = BasTrhX;
            p.BitTrhX = BitTrhX;
            p.HHId = HHId;
            p.TTId = TTId;

            Opened = false;
            p.Data = null;*/
        }
    }

    [FFsRpr_json.DlgRec]
    partial class FFsRprDlgPartial : Json
    {
        void Handle(Input.NewTrgr Action)
        {
            var p = this.Parent as FFsRpr;
            DateTime now = DateTime.Now;
            Id = 0;
            /*
            HHId = 0;
            TTId = 0;
            TrhX = DateTime.Today.ToString("yyyy-MM-dd");
            ZmnX = "";
            Gdr = 0;
            Glr = 0;
            Ad = "";*/
            //if (string.IsNullOrEmpty(TrhX))
            {
                TrhX = now.ToString("yyyy-MM-dd");
                ZmnX = now.ToString("HH:mm");
            }
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

            var r = Root as MasterPage;
            var p = this.Parent as FFsRpr;

            Msj = FF.InsertRec(p.PPId, HHId, TTId, $"{TrhX} {ZmnX}", Ad, TutTur, Tut, r.CUId);
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
                var r = Root as MasterPage;

                Msj = FF.UpdateRec((ulong)Id, (ulong)HHId, (ulong)TTId, $"{TrhX} {ZmnX}", Ad, TutTur, Tut, (ulong)r.CUId);
                if (!string.IsNullOrEmpty(Msj))
                {
                    Action.Cancelled = true;
                    return;
                }

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
            var r = Root as MasterPage;
            var p = this.Parent as FFsRpr;

            if (p.DlgRec.Ad != "Sil")
            {
                Msj = "Silmek için Not alanına Sil yazın.";
                Action.Cancelled = true;
                return;
            }

            Msj = FF.DeleteRec((ulong)Id, (ulong)r.CUId);

            if (!string.IsNullOrEmpty(Msj))
            {
                Action.Cancelled = true;
                return;
            }

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
            var r = Root as MasterPage;
            var p = this.Parent.Parent as FFsRpr;

            if (Db.FromId((ulong)Id) is FF ff)
            {
                p.DlgRec.Id = Id;
                p.DlgRec.Ad = ff.Ad;

                p.DlgRec.TutTur = "GX";
                if (ff.Gdr != 0)
                {
                    p.DlgRec.TutTur = "GX";
                    p.DlgRec.Tut = ff.Gdr;
                }
                else if (ff.Glr != 0)
                {
                    p.DlgRec.TutTur = "GI";
                    p.DlgRec.Tut = ff.Glr;
                }
                else if (ff.BklGdr != 0)
                {
                    p.DlgRec.TutTur = "BX";
                    p.DlgRec.Tut = ff.BklGdr;
                }
                else if (ff.BklGlr != 0)
                {
                    p.DlgRec.TutTur = "BI";
                    p.DlgRec.Tut = ff.BklGlr;
                }

                p.DlgRec.HHId = (long)ff.HHId;
                p.DlgRec.TTId = (long)ff.TTId;

                p.DlgRec.TrhX = ff.Trh.ToString("yyyy-MM-dd");
                p.DlgRec.ZmnX = ff.Trh.ToString("HH:mm");

                p.DlgRec.Msj = "";
                p.DlgRec.IsNew = false; // Edit
                p.DlgRec.Opened = true;
            }
        }

    }
}
