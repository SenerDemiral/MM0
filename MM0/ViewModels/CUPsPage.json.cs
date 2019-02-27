using DBMM0;
using Starcounter;

namespace MM0.ViewModels
{
    partial class CUPsPage : Json
    {
        protected override void OnData()
        {
            base.OnData();

            if (Db.FromId((ulong)CUId) is CU cu)
            {
                CCId = (long)cu.CC.Id;
                {

                    Hdr = $"{cu.CC.Ad}►{cu.Ad}►Projeleri";

                    var recs = Db.SQL<CUP>("select r from CUP r where r.CU = ?", cu);
                    CUPs.Data = recs;

                    PPs.Data = Db.SQL<PP>("select r from PP r where r.CC = ? order by r.Ad", cu.CC);
                }
            }
        }
    }
    [CUPsPage_json.DlgRec]
    partial class CUPsPageDlgRec
    {
        void Handle(Input.NewTrgr Action)
        {
            IsNew = true;
            Opened = true;
        }

        void Handle(Input.InsTrgr Action)
        {
            var p = this.Parent as CUPsPage;

            CUP.InsertRec((ulong)p.CCId, (ulong)p.CUId, (ulong)PPId, (int)Mode);

            Id = 0;
            Opened = false;
            p.Data = null;

            Session.RunTaskForAll((s, sId) =>
            {
                var cp = (s.Store[nameof(MasterPage)] as MasterPage).CurrentPage;
                if (cp is CUPsPage)
                {
                    (s.Store[nameof(MasterPage)] as MasterPage).CurrentPage.Data = null;
                    s.CalculatePatchAndPushOnWebSocket();
                }
            });
        }

        void Handle(Input.UpdTrgr Action)
        {
            var p = this.Parent as CUPsPage;
            if (Id != 0)
            {
                CUP.UpdateRec((ulong)Id, (ulong)PPId, (int)Mode);

                Id = 0;

                Session.RunTaskForAll((s, id) =>
                {
                    s.CalculatePatchAndPushOnWebSocket();
                });
            }
            Opened = false;
        }

    }

    [CUPsPage_json.CUPs]
    partial class CUPsPageCUPs
    {
        void Handle(Input.EdtTrgr Action)
        {
            var r = this.Root as MasterPage;
            var p = this.Parent.Parent as CUPsPage;

            p.DlgRec.Id = Id;
            p.DlgRec.PPId = PPId;
            p.DlgRec.Mode = Mode;


            p.DlgRec.IsNew = false; // Edit
            p.DlgRec.Opened = true;
        }
    }
}
