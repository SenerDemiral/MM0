using DBMM0;
using Starcounter;

namespace MM0.ViewModels
{
    partial class TTsPage : Json
    {
        protected override void OnData()
        {
            base.OnData();

            if (Db.FromId((ulong)PPId) is PP pp)
            {
                Hdr = $"{pp.CC.Ad}►{pp.Ad}";
                TTs.Data = Db.SQL<TT>("select r from TT r where r.PP = ?", pp);
            }
        }

        void Handle(Input.DwnldTrgr Action)
        {
            DwnldUrl = $"/mm0/TTsXlsx/{PPId}";
        }
    }

    [TTsPage_json.DlgRec]
    partial class TTPartial : Json
    {
        void Handle(Input.NewTrgr Action)
        {
            Id = 0;
            Ad = "";
            Info = "";
            Email = "";
            Tel = "";
            IsNew = true;
            Opened = true;
        }

        void Handle(Input.InsTrgr Action)
        {
            var p = this.Parent as TTsPage;

            if (!string.IsNullOrWhiteSpace(Ad))
            {
                Msj = TT.InsertRec(p.PPId, Ad, Info, Email, Tel);
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
            TT.UpdateRec(Id, Ad, Info, Email, Tel);

            Session.RunTaskForAll((s, id) =>
            {
                s.CalculatePatchAndPushOnWebSocket();
            });

            Opened = false;
        }

        void Handle(Input.DelTrgr Action)
        {
            Msj = TT.DeleteRec(Id);
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
    [TTsPage_json.TTs]
    partial class TTsPartial: Json
    {
        void Handle(Input.EdtTrgr Action)
        {
            var p = this.Parent.Parent as TTsPage;

            p.DlgRec.Id = Id;
            p.DlgRec.Ad = Ad;
            p.DlgRec.Info = Info;
            p.DlgRec.Email = Email;
            p.DlgRec.Tel = Tel;

            p.DlgRec.Msj = "";

            p.DlgRec.IsNew = false; // Edit
            p.DlgRec.Opened = true;

        }

    }
}
