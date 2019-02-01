using DBMM0;
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
                    ffs = Db.SQL<FF>("select r from FF r where r.PP = ? and r.Trh = ?", pp, trh);
                }

                FFs.Data = ffs;

                HHs.Data = Db.SQL<HH>("select r from HH r where r.PP = ? and r.Skl = ? order by r.AdPrn, r.Ad", pp, 99); // Sadece Leafs
                TTs.Data = Db.SQL<TT>("select r from TT r where r.PP = ? order by r.Ad", pp);

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

            var p = this.Parent as FFsPage;

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
            Msj = FF.DeleteRec(Id);
            if (!string.IsNullOrEmpty(Msj))
            {
                Action.Cancelled = true;
                return;
            }

            var p = this.Parent as FFsPage;
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

            p.DlgRec.Id = Id;
            p.DlgRec.Ad = Ad;

            p.DlgRec.Gdr = Gdr;
            p.DlgRec.Glr = Glr;

            p.DlgRec.HHId = HHId;

            if (string.IsNullOrEmpty(TrhX))
                p.DlgRec.TrhX = "";
            else
                p.DlgRec.TrhX = Convert.ToDateTime(TrhX).ToString("yyyy-MM-dd");
            p.DlgRec.Msj = "";
            p.DlgRec.IsNew = false; // Edit
            p.DlgRec.Opened = true;
        }

    }
}
