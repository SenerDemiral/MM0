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
                Hdr = $"{pp.CC.Ad} ► {pp.Ad}";

                IEnumerable<FF> ffs = null;

                if (!string.IsNullOrEmpty(QryTrhX))
                {
                    DateTime trh = Convert.ToDateTime(QryTrhX);
                    ffs = Db.SQL<FF>("select r from FF r where r.PP = ? and r.Trh = ?", pp, trh);
                }

                FFs.Data = ffs;

                HHs.Data = Db.SQL<HH>("select r from HH r where r.PP = ? and Skl = ?", pp, 99); // Sadece Leafs

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

        void Handle(Input.QryTrhX Action)
        {
            if (Action.OldValue != Action.Value)
            {
                QryTrhX = Action.Value;
                Data = null;
            }
        }
    }

    [FFsPage_json.FF]
    partial class FFPartial : Json
    {
        void Handle(Input.NewTrgr Action)
        {
            Id = 0;
            HHId = 0;
            TrhX = DateTime.Today.ToString("yyyy-MM-dd");
            Msj = "";
            IsNew = true;
            Opened = true;
        }

        void Handle(Input.InsTrgr Action)
        {
            if (HHId == 0)
            {
                Action.Cancelled = true;
                Msj = "Hesap girin";
                return;
            }
            //TrhX girilmediginde 00:00:00 geliyor. DateTime'a convert edince Gunun tarihi geliyor!
            //When only the time is present, the date portion uses the current date.
            //When only the date is present, the time portion is midnight.
            //When the year isn't specified in a date, the current year is used.
            //When the day of the month isn't specified, the first of the month is used.
            
            //How to set only time part of a DateTime variable
            //var newDate = oldDate.Date + new TimeSpan(11, 30, 55);

            var p = this.Parent as FFsPage;

            FF.InsertRec(p.PPId, HHId, TrhX, Ad, Gdr, Glr);

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
                FF.UpdateRec(Id, HHId, TrhX, Ad, Gdr, Glr);

                Id = 0;

                Session.RunTaskForAll((s, id) =>
                {
                    s.CalculatePatchAndPushOnWebSocket();
                });
            }
            Opened = false;

        }

    }

    [FFsPage_json.FFs]
    partial class FFsPartial: Json
    {
        void Handle(Input.EdtTrgr Action)
        {
            var p = this.Parent.Parent as FFsPage;

            p.FF.Id = Id;
            p.FF.Ad = Ad;

            p.FF.Gdr = Gdr;
            p.FF.Glr = Glr;

            p.FF.HHId = HHId;

            if (string.IsNullOrEmpty(TrhX))
                p.FF.TrhX = "";
            else
                p.FF.TrhX = Convert.ToDateTime(TrhX).ToString("yyyy-MM-dd");

            p.FF.IsNew = false; // Edit
            p.FF.Opened = true;
        }

    }
}
