using DBMM0;
using Starcounter;
using System.Collections.Generic;
using System.Linq;

namespace MM0.ViewModels
{
    partial class CUsPage : Json
    {
        protected override void OnData()
        {
            base.OnData();

            if (Db.FromId((ulong)CCId) is CC cc)
            {
                Hdr = $"{cc.Ad}►Kullanıcılar";

                var recs = Db.SQL<CU>("select r from CU r where r.CC = ?", cc);
                CUs.Data = recs;

                PPs.Data = Db.SQL<PP>("select r from PP r where r.CC = ? order by r.Ad", cc);
            }
        }
    }
    [CUsPage_json.DlgRec]
    partial class CUsPageDlgRec
    {
        void Handle(Input.NewTrgr Action)
        {
            IsNew = true;
            Opened = true;
        }

        void Handle(Input.PPIds Action)
        {

        }

        void Handle(Input.InsTrgr Action)
        {
            var p = this.Parent as CUsPage;

            List<string> pps = new List<string>();
            foreach(var pp in p.PPs)
            {
                if (pp.Selected)
                    pps.Add(pp.Id.ToString());
            }

            CU.InsertRec((ulong)p.CCId, Ad, Pwd, string.Join<string>(",", pps));

            Id = 0;
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
            var p = this.Parent as CUsPage;
            if (Id != 0)
            {
                List<string> pps = new List<string>();
                foreach (var pp in p.PPs)
                {
                    if (pp.Selected)
                        pps.Add(pp.Id.ToString());
                }

                CU.UpdateRec((ulong)Id, Ad, Pwd, string.Join<string>(",", pps));

                Id = 0;

                Session.RunTaskForAll((s, id) =>
                {
                    s.CalculatePatchAndPushOnWebSocket();
                });
            }
            Opened = false;
        }
    }

    [CUsPage_json.CUs]
    partial class CUsPageCUs
    {
        void Handle(Input.EdtTrgr Action)
        {
            var r = this.Root as MasterPage;
            var p = this.Parent.Parent as CUsPage;

            p.DlgRec.Id = Id;
            p.DlgRec.Ad = Ad;
            p.DlgRec.Pwd = Pwd;


            if (Db.FromId((ulong)Id) is CU cu)
            {
                string PPs = "";
                //List<string> lPPs = new List<string>(cu.PPs.Split(','));
                if (!string.IsNullOrEmpty(cu.PPs))
                    PPs = cu.PPs;
                List<string> lPPs = new List<string>(PPs.Split(','));

                foreach (var pp in p.PPs)
                {
                    pp.Selected = false;
                    if (lPPs != null && lPPs.Contains(pp.Id.ToString()))
                        pp.Selected = true;
                }
            }

            p.DlgRec.IsNew = false; // Edit
            p.DlgRec.Opened = true;

        }
    }
}

