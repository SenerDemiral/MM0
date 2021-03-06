﻿using DBMM0;
using Starcounter;
using System;
using System.Collections.Generic;

namespace MM0.ViewModels
{
    partial class FFsPage : Json
    {
        protected override void OnData()
        {
            base.OnData();

            if (Db.FromId((ulong)PPId) is PP pp)
            {
                Hdr = $"{pp.CC.Ad}►{pp.Ad}";

                IEnumerable<FF> ffs = null;

                if (!string.IsNullOrEmpty(QryTrhX))
                {
                    DateTime trh = Convert.ToDateTime(QryTrhX);
                    DateTime bitTrh = trh.AddDays(1);
                    ffs = Db.SQL<FF>("select r from FF r where r.PP = ? and r.InsTrh >= ? and r.InsTrh < ?", pp, trh, bitTrh);
                }

                FFs.Data = ffs;

                HHs.Data = Db.SQL<HH>("select r from HH r where r.PP = ? and r.Skl = ? order by r.AdFull", pp, 99); // Sadece Leafs
                TTs.Data = Db.SQL<TT>("select r from TT r where r.PP = ? order by r.Ad", pp);

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
            }
        }

        public void RefreshToplam()
        {
            if (Db.FromId((ulong)PPId) is PP pp)
            {
                if (!string.IsNullOrEmpty(QryTrhX))
                {
                    var ffs = Db.SQL<FF>("select r from FF r where r.PP = ? and r.Trh = ?", pp, Convert.ToDateTime(QryTrhX));

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

        void Handle(Input.QryTrhX Action)
        {
            if (Action.OldValue != Action.Value)
            {
                QryTrhX = Action.Value;
                Data = null;
            }
        }
    }

    [FFsPage_json.DlgRec]
    partial class FFPartial : Json
    {
        void Handle(Input.NewTrgr Action)
        {
            var p = this.Parent as FFsPage;
            Id = 0;
            HHId = 0;
            TTId = 0;
            TrhX = p.QryTrhX; // DateTime.Today.ToString("yyyy-MM-dd");
            ZmnX = DateTime.Now.ToString("HH:mm"); //"";
            Ad = "";
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
            var p = this.Parent as FFsPage;

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

                //Msj = FF.UpdateRec((ulong)Id, (ulong)HHId, (ulong)TTId, $"{TrhX} {ZmnX}", Ad, Gdr, Glr, (ulong)r.CUId);
                Msj = FF.UpdateRec((ulong)Id, (ulong)HHId, (ulong)TTId, $"{TrhX} {ZmnX}", Ad, TutTur, Tut, (ulong)r.CUId);

                if (!string.IsNullOrEmpty(Msj))
                {
                    Action.Cancelled = true;
                    return;
                }

                var p = this.Parent as FFsPage;
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
            var p = this.Parent as FFsPage;

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

    [FFsPage_json.FFs]
    partial class FFsPartial: Json
    {
        void Handle(Input.EdtTrgr Action)
        {
            var p = this.Parent.Parent as FFsPage;

            if (Db.FromId((ulong)Id) is FF ff)
            {
                p.DlgRec.Id = (long)ff.Id;
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
                else if (ff.BklGlr != 0) { 
                    p.DlgRec.TutTur = "BI";
                    p.DlgRec.Tut = ff.BklGlr;
                }

                p.DlgRec.HHId = (long)ff.HH.Id;
                p.DlgRec.TTId = (long)ff.TT?.Id;

                p.DlgRec.TrhX = ff.Trh.ToString("yyyy-MM-dd");
                p.DlgRec.ZmnX = ff.Trh.ToString("HH:mm");

            }

            /*
            p.DlgRec.Id = Id;
            p.DlgRec.Ad = Ad;

            p.DlgRec.Gdr = Gdr;
            p.DlgRec.Glr = Glr;

            p.DlgRec.HHId = HHId;

            if (string.IsNullOrEmpty(TrhX))
            {
                p.DlgRec.TrhX = "";
                p.DlgRec.ZmnX = "";
            }
            else
            {
                p.DlgRec.TrhX = Convert.ToDateTime(TrhX).ToString("yyyy-MM-dd");
                p.DlgRec.ZmnX = Convert.ToDateTime(TrhX).ToString("HH:mm");
            }
            */
            p.DlgRec.Msj = "";
            p.DlgRec.IsNew = false; // Edit
            p.DlgRec.Opened = true;
        }

    }
}
