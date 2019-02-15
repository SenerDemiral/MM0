using DBMM0;
using Starcounter;
using System;
using System.Collections.Generic;
using System.Linq;

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

                var r = this.Root as MasterPage;
                if(r.CUId > 0)
                {
                    if (Db.FromId((ulong)r.CUId) is CU cu)
                    {
                        List<string> lPPs = new List<string>(cu.PPs.Split(','));
                        PPs.Data = recs.Where((pp) => lPPs.Contains(pp.Id.ToString()));
                    }

                }
                else
                {
                    PPs.Data = recs;
                }

            }
        }
    }

    [PPsPage_json.DlgDel]
    partial class PPDlgDelPartial : Json
    {
        void Handle(Input.DelTrgr Action)
        {
            var p = this.Parent as PPsPage;

            if (p.DlgRec.Ad != Ad)
                Msj = "Proje adýný doðru girmediniz.";
            else
            {
                PP.DeleteAll((ulong)p.DlgRec.Id);
                Opened = false;
            }
        }
    }

    [PPsPage_json.DlgRec]
    partial class PPPartil : Json
    {
        void Handle(Input.NewTrgr Action)
        {
            if ((Root as MasterPage).CUId == 0)    // Sadece Client/Admn
            {
                Id = 0;
                Ad = "";

                BasTrhX = DateTime.Today.ToString("yyyy-MM-dd");
                BitTrhX = "";
                IsNew = true;
                Opened = true;
            }
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

        void Handle(Input.DelTrgr Action)
        {
            var p = this.Parent as PPsPage;

            Opened = false;
            p.DlgDel.Opened = true;
            p.DlgDel.Msj = "Projeye ait tüm bilgiler silinecektir.";
        }

    }

    [PPsPage_json.PPs]
    partial class PPsPartial : Json
    {
        void Handle(Input.EdtTrgr Action)
        {
            if ((Root as MasterPage).CUId == 0)    // Sadece Client/Admn
            {
                var p = this.Parent.Parent as PPsPage;

                p.DlgRec.Id = Id;
                p.DlgRec.Ad = Ad;
                if (string.IsNullOrEmpty(BasTrhX))
                    p.DlgRec.BasTrhX = "";
                else
                    p.DlgRec.BasTrhX = Convert.ToDateTime(BasTrhX).ToString("yyyy-MM-dd");
                if (string.IsNullOrEmpty(BitTrhX))
                    p.DlgRec.BitTrhX = "";
                else
                    p.DlgRec.BitTrhX = Convert.ToDateTime(BitTrhX).ToString("yyyy-MM-dd");

                p.DlgRec.IsNew = false; // Edit
                p.DlgRec.Opened = true;
            }
        }
    }


}
