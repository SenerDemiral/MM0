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

            if (Db.FromId((ulong)PPId) is PP pp)
            {
                Hdr = $"{pp.CC.Ad}►{pp.Ad}";
                HHs.Data = DBMM0.HH.View(pp);  //Db.SQL<HH>("select r from HH r");
            }
        }
    }

    [HHsPage_json.HH]
    partial class HHPartial : Json
    {
        void Handle(Input.ApdTrgr Action)
        {
            var p = this.Parent as HHsPage;

            if (!string.IsNullOrWhiteSpace(Ad))
            {
                Msj = HH.InsertRec(p.PPId, Id, Ad, ThmGdr, ThmGdr);
                if (!string.IsNullOrEmpty(Msj))
                {
                    Action.Cancelled = true;
                    return;
                }
            }
            Opened = false;

            p.Data = null;

            Session.RunTaskForAll((s, sId) => {
                var cp = (s.Store[nameof(MasterPage)] as MasterPage).CurrentPage;
                if (cp is HHsPage)
                {
                    (s.Store[nameof(MasterPage)] as MasterPage).CurrentPage.Data = null;
                    s.CalculatePatchAndPushOnWebSocket();
                }
            });

        }

        void Handle(Input.UpdTrgr Action)
        {
            HH.UpdateRec(Id, Ad, ThmGdr, ThmGlr);

            Session.RunTaskForAll((s, id) =>
            {
                s.CalculatePatchAndPushOnWebSocket();
            });

            Opened = false;
        }

        void Handle(Input.DelTrgr Action)
        {
            Msj = HH.DeleteRec(Id);
            if (!string.IsNullOrEmpty(Msj))
            {
                Action.Cancelled = true;
                return;
            }

            Session.RunTaskForAll((s, id) =>
            {
                s.CalculatePatchAndPushOnWebSocket();
            });

            Opened = false;
        }
    }

    [HHsPage_json.HHs]
    partial class HHsPartial : Json
    {
        void Handle(Input.EdtTrgr Action)
        {
            var p = this.Parent.Parent as HHsPage;

            p.HH.Ad = Ad;
            p.HH.Id = Id;

            p.HH.ThmGdr = ThmGdr;
            p.HH.ThmGlr = ThmGlr;


            p.HH.Opened = true;

        }

    }
}
