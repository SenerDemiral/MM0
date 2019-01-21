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

            if (Db.FromId((ulong)CCId) is CC cc)
            {
                Hdr = $"{cc.Ad}";

                var hh = cc.HHroot;
                GrcGlrTopX = hh.GrcGlrX;
                GrcGdrTopX = hh.GrcGdrX;
                ThmGlrTopX = hh.ThmGlrX;
                ThmGdrTopX = hh.ThmGdrX;

                var recs = Db.SQL<PP>("select r from PP r where r.CC = ?", cc);
                PPs.Data = recs;

            }
        }
    }

    [PPsPage_json.PP]
    partial class PPPartil : Json
    {
        void Handle(Input.NewTrgr Action)
        {
            Id = 0;
            BasTrhX = DateTime.Today.ToString("yyyy-MM-dd");
            IsNew = true;
            Opened = true;
        }

        void Handle(Input.InsTrgr Action)
        {
            var p = this.Parent as PPsPage;

            if (!string.IsNullOrWhiteSpace(Ad))
            {
                PP.InsertRec(p.CCId, Ad, BasTrhX, BitTrhX);

                Id = 0;
            }
            Opened = false;

            p.Data = null;

            Session.RunTaskForAll((s, sId) => {
                var cp = (s.Store[nameof(MasterPage)] as MasterPage).CurrentPage;
                if (cp is PPsPage)
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
                PP.UpdateRec(Id, Ad, BasTrhX, BitTrhX);

                Id = 0;

                Session.RunTaskForAll((s, id) =>
                {
                    s.CalculatePatchAndPushOnWebSocket();
                });
            }
            Opened = false;

        }
    }

    [PPsPage_json.PPs]
    partial class PPsPartial : Json
    {
        void Handle(Input.EdtTrgr Action)
        {
            var p = this.Parent.Parent as PPsPage;

            p.PP.Id = Id;
            p.PP.Ad = Ad;
            if (string.IsNullOrEmpty(BasTrhX))
                p.PP.BasTrhX = "";
            else
                p.PP.BasTrhX = Convert.ToDateTime(BasTrhX).ToString("yyyy-MM-dd");
            if (string.IsNullOrEmpty(BitTrhX))
                p.PP.BitTrhX = "";
            else
                p.PP.BitTrhX = Convert.ToDateTime(BitTrhX).ToString("yyyy-MM-dd");

            p.PP.IsNew = false; // Edit
            p.PP.Opened = true;
        }
    }


}
