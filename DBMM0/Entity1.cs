
using System;
using System.Collections.Generic;
using System.Linq;
using Starcounter;

namespace DBMM0
{
    [Database]
    public class TT // TAGs
    {
        public ulong Id => this.GetObjectNo();
        public PP PP { get; set; }
        public string Ad { get; set; }
        public string Info { get; set; }
        public string Email { get; set; }
        public string Tel { get; set; }

        public static string InsertRec(long ppId, string Ad, string Info, string Email, string Tel)
        {
            string msj = "";
            Db.Transact(() =>
            {
                new TT
                {
                    PP = Db.FromId<PP>((ulong)ppId),
                    Ad = Ad,
                    Info = Info,
                    Email = Email,
                    Tel = Tel,
                };
            });
            return msj;
        }

        public static void UpdateRec(long Id, string Ad, string Info, string Email, string Tel)
        {
            Db.Transact(() =>
            {
                if (Db.FromId((ulong)Id) is TT tt)
                {
                    tt.Ad = Ad;
                    tt.Info = Info;
                    tt.Email = Email;
                    tt.Tel = Tel;
                }
            });

        }

        public static string DeleteRec(long Id)
        {
            string msj = "";
            Db.Transact(() =>
            {
                if (Db.FromId((ulong)Id) is TT tt)
                {
                    var ff = Db.SQL<FF>("select r from FF r where r.TT = ?", tt).FirstOrDefault();
                    if (ff != null)
                        msj = "Kullanılmış, Silemezsiniz";
                    else
                        tt.Delete();
                }
            });
            return msj;
        }

    }

    [Database]
    public class TF // TT-FF
    {
        public ulong Id => this.GetObjectNo();
        public TT TT { get; set; }
        public FF FF { get; set; }
        

        public static IEnumerable<FF> GetIntersectFFsView(params ulong[] ttIds)    // Caller: GetIntersectFFs(1234, 4535, 45452)
        {
            HashSet<ulong> hs1 = new HashSet<ulong>();
            HashSet<ulong> hs2 = new HashSet<ulong>();

            bool ilk = true;
            for (int i = 0; i < ttIds.Length; i++)
            {
                if (Db.FromId<TT>(ttIds[i]) is TT tt)
                {
                    if (ilk)
                    {
                        foreach (var tf in Db.SQL<TF>("select r from TF r where r.TT = ?", tt))
                            hs1.Add(tf.FF.GetObjectNo());

                        ilk = false;
                    }
                    else
                    {
                        if (hs1.Count == 0) // Set bos daha fazla yapmaya gerek yok.
                            break;

                        hs2.Clear();
                        foreach (var tf in Db.SQL<TF>("select r from TF r where r.TT = ?", tt))
                            hs2.Add(tf.FF.GetObjectNo());

                        hs1.IntersectWith(hs2);
                    }
                }
            }
            foreach(var ffId in hs1)
            {
                if (Db.FromId<FF>(ffId) is FF ff)
                    yield return ff;
            }
        }

        public static HashSet<ulong> GetIntersectFFs(params ulong[] ttIds)    // Caller: GetIntersectFFs(1234, 4535, 45452)
        {
            HashSet<ulong> hs1 = new HashSet<ulong>();
            HashSet<ulong> hs2 = new HashSet<ulong>();

            bool ilk = true;
            for (int i = 0; i < ttIds.Length; i++)
            {
                if (Db.FromId<TT>(ttIds[i]) is TT tt)
                {
                    if (ilk)
                    {
                        foreach (var tf in Db.SQL<TF>("select r from TF r where r.TT = ?", tt))
                            hs1.Add(tf.FF.GetObjectNo());

                        ilk = false;
                    }
                    else
                    {
                        if (hs1.Count == 0) // Set bos daha fazla yapmaya gerek yok.
                            break;

                        hs2.Clear();
                        foreach (var tf in Db.SQL<TF>("select r from TF r where r.TT = ?", tt))
                            hs2.Add(tf.FF.GetObjectNo());

                        hs1.IntersectWith(hs2);
                    }
                }
            }
            return hs1;
        }

        public static HashSet<ulong> GetIntersectFFs(List<TT> TTL)
        {
            HashSet<ulong> hs1 = new HashSet<ulong>();
            HashSet<ulong> hs2 = new HashSet<ulong>();

            bool ilk = true;
            foreach (var tt in TTL)
            {
                if (ilk)
                {
                    foreach (var tf in Db.SQL<TF>("select r from TF r where r.TT = ?", tt))
                        hs1.Add(tf.FF.GetObjectNo());
                    ilk = false;
                }
                else
                {
                    if (hs1.Count == 0) // Set bos daha fazla yapmaya gerek yok.
                        break;

                    hs2.Clear();
                    foreach (var tf in Db.SQL<TF>("select r from TF r where r.TT = ?", tt))
                        hs2.Add(tf.FF.GetObjectNo());

                    hs1.IntersectWith(hs2);
                }
            }
            return hs1;
        }

        public static HashSet<ulong> GetIntersectFFs(TT[] TTA)
        {
            HashSet<ulong> hs1 = new HashSet<ulong>();
            HashSet<ulong> hs2 = new HashSet<ulong>();

            foreach ( var tf in Db.SQL<TF>("select r from TF r where r.TT = ?", TTA[0]))
                hs1.Add(tf.FF.GetObjectNo());

            for (int i = 1; i < 5; i++)
            {
                hs2.Clear();
                foreach (var tf in Db.SQL<TF>("select r from TF r where r.TT = ?", TTA[i]))
                    hs2.Add(tf.FF.GetObjectNo());

                hs1.IntersectWith(hs2);

                if (hs1.Count == 0) // Set bos daha fazla yapmaya gerek yok.
                    break;
            }

            return hs1;
        }
    }

    // CC Admin dir.
    // CU Admin'in actigi userlar
    // User Belirli projelere ulasabilir.
    [Database]
    public class CU // ClientUsers
    {
        public ulong Id => this.GetObjectNo();
        public CC CC { get; set; }
        public string Email { get; set; }
        public string Ad { get; set; }
        public string Pwd { get; set; }
        public string Token { get; set; }
        public string PPs { get; set; }

        public static CU InsertRec(ulong ccId, string Ad, string Pwd, string PPs)
        {
            CU cuNew = null;
            if (Db.FromId(ccId) is CC cc)
            {
                var NOR = Db.SQL<CU>("select r from CU r where r.CC = ?", cc).Count();
                NOR++;
                Db.Transact(() =>
                {
                    cuNew = new CU()
                    {
                        CC = cc,
                        Ad = Ad,
                        Pwd = Pwd,
                        Email = $"{cc.Email}/{NOR}",
                        PPs = PPs
                    };
                    cuNew.Token = cuNew.Email;
                });
                return cuNew;
            }
            return null;
        }
        public static void UpdateRec(ulong cuId, string Ad, string Pwd, string PPs)
        {
            Db.Transact(() =>
            {
                if (Db.FromId(cuId) is CU cu)
                {
                    cu.Ad = Ad;
                    cu.Pwd = Pwd;
                    cu.PPs = PPs;
                }
            });
        }

    }

    [Database]
    public class CC // Clients
    {
        public ulong Id => this.GetObjectNo();

        public string Ad { get; set; }
        public HH HHroot { get; set; }     // Starting Node @HH

        public string Pwd { get; set; }
        public string Token { get; set; }
        public string Email { get; set; }
        public bool IsConfirmed { get; set; }
        public DateTime InsTS { get; set; }
        public DateTime? UpdTS { get; set; }
        public DateTime? CnfTS { get; set; }

        public string Tel { get; set; }
        public DateTime UyeBasTrh { get; set; }
        public string UyePry { get; set; }  // Uyelik Periyodu. Aylik, Yillik

        public DateTime TstBasTrh { get; set; } // DenemeSuresi baslangici
        public DateTime TstBitTrh { get; set; }

        public static void InsertRec(string Email, string Pwd, string newToken, bool isConfirmed = false)
        {
            CC ccNew = null;
            Db.Transact(() =>
            {
                ccNew = new CC
                {
                    Email = Email,
                    Pwd = Pwd,
                    Token = newToken,
                    InsTS = DateTime.Now,
                    IsConfirmed = isConfirmed,
                };

                int i = Email.IndexOf('@');
                if (i >= 0)
                    ccNew.Ad = Email.Remove(i);
                else
                    ccNew.Ad = Email;
            });
            Db.Transact(() =>
            {
                HH hh = new HH
                {
                    Ad = ccNew.Ad,
                };
                ccNew.HHroot = hh;
            });

            PP ppNew = PP.InsertRec((long)ccNew.GetObjectNo(), "Örnek", null, null);
            Hlp.SablondanEkle(ppNew.GetObjectNo());
            TT.InsertRec((long)ppNew.Id, "Aile", null, null, null);
            TT.InsertRec((long)ppNew.Id, "Baba", null, null, null);
            TT.InsertRec((long)ppNew.Id, "Anne", null, null, null);
            TT.InsertRec((long)ppNew.Id, "Çocuk1", null, null, null);
            TT.InsertRec((long)ppNew.Id, "Çocuk2", null, null, null);
            TT.InsertRec((long)ppNew.Id, "Araç1", null, null, null);
            TT.InsertRec((long)ppNew.Id, "Araç2", null, null, null);
        }

        public static void DeleteAll(ulong ccId)
        {
            if (Db.FromId(ccId) is CC cc)
            {
                Db.Transact(() =>
                {
                    foreach(var pp in Db.SQL<PP>("select r from PP r where r.CC = ?", cc))
                    {
                        PP.DeleteAll(pp.GetObjectNo());
                    }
                    cc.HHroot.Delete();
                    cc.Delete();
                });
            }
        }

    }

    [Database]
    public class PP //Projects
    {
        public ulong Id => this.GetObjectNo();

        public CC CC { get; set; }
        public string Ad { get; set; }
        public HH HHroot { get; set; }     // Starting Node @HH

        public DateTime? BasTrh { get; set; }
        public DateTime? BitTrh { get; set; }

        public string CCAd => CC?.Ad;
        public string BasTrhX => $"{BasTrh:dd.MM.yy}";
        public string BitTrhX => $"{BitTrh:dd.MM.yy}";

        public decimal GrcGlr => HHroot.GrcGlr;
        public decimal GrcGdr => HHroot.GrcGdr;
        public decimal ThmGlr => HHroot.ThmGlr;
        public decimal ThmGdr => HHroot.ThmGdr;

        [Transient]
        public decimal FrkGlr;
        [Transient]
        public decimal FrkGdr;


        public string GrcGlrX => $"{HHroot?.GrcGlr:#,#.##;-#,#.##;#}";
        public string GrcGdrX => $"{HHroot?.GrcGdr:#,#.##;-#,#.##;#}";
        public string ThmGlrX => $"{HHroot?.ThmGlr:#,#.##}";
        public string ThmGdrX => $"{HHroot?.ThmGdr:#,#.##}";
        //public string GrcGlrX => HHroot?.GrcGlr == 0 ? "" : $"{HHroot?.GrcGlr:n2}";
        //public string GrcGdrX => HHroot?.GrcGdr == 0 ? "" : $"{HHroot?.GrcGdr:n2}";
        //public string ThmGlrX => HHroot?.ThmGlr == 0 ? "" : $"{HHroot?.ThmGlr:n2}";
        //public string ThmGdrX => HHroot?.ThmGdr == 0 ? "" : $"{HHroot?.ThmGdr:n2}";

        public static IEnumerable<PP> View(string CCId)
        {
            var cc = Db.FromId<CC>(Convert.ToUInt64(CCId));  // Oguz
            var pps = Db.SQL<PP>("select r from PP r where r.CC = ?", cc);

            foreach (var pp in pps)
            {
                pp.FrkGlr = pp.ThmGlr - pp.GrcGlr;
                pp.FrkGdr = pp.ThmGdr - pp.GrcGdr;
                yield return pp;
            }
        }

        public static void UpdateRec(long ppId, string Ad, string BasTrh, string BitTrh)
        {
            Db.Transact(() =>
            {
                if (Db.FromId((ulong)ppId) is PP pp)
                {
                    pp.Ad = Ad;
                    if (string.IsNullOrEmpty(BasTrh))
                        pp.BasTrh = null;
                    else
                        pp.BasTrh = Convert.ToDateTime(BasTrh);
                    if (string.IsNullOrEmpty(BitTrh))
                        pp.BitTrh = null;
                    else
                        pp.BitTrh = Convert.ToDateTime(BitTrh);

                    pp.HHroot.Ad = Ad;
                }
            });
        }

        public static PP InsertRec(long ccId, string Ad, string BasTrh, string BitTrh)
        {
            if (Db.FromId((ulong)ccId) is CC cc)
            {
                PP ppNew = null;
                HH hhNew = null;
                DateTime? nullDT = null;
                Db.Transact(() =>
                {
                    ppNew = new PP()
                    {
                        Ad = Ad,
                        BasTrh = string.IsNullOrEmpty(BasTrh) ? nullDT : DateTime.Parse(BasTrh),  //Convert.ToDateTime(BasTrh),
                        BitTrh = string.IsNullOrEmpty(BitTrh) ? nullDT : Convert.ToDateTime(BitTrh),
                        CC = cc,
                    };
                });
                Db.Transact(() =>
                {
                    hhNew = new HH()
                    {
                        PP = ppNew,
                        Prn = cc.HHroot,
                        Ad = ppNew.Ad,
                        Lvl = 1,
                        Skl = 1,
                    };
                });
                Db.Transact(() =>
                {
                    ppNew.HHroot = hhNew;
                });
                return ppNew;
            }
            return null;
        }

        public static void DeleteAll(ulong ppId)
        {
            if (Db.FromId(ppId) is PP pp)
            {
                Db.Transact(() =>
                {
                    Db.SQL("delete from FF where PP = ?", pp);
                    Db.SQL("delete from TT where PP = ?", pp);
                    Db.SQL("delete from HH where PP = ?", pp);
                    pp.Delete();
                });
            }
        }
    }

    // HH dolaylı yoldan Hesabın kime ait oldugunu biliyor.
    // Performans acisindan PP alanini eklendi
    [Database]
    public class FF  //Fisler
    {
        public ulong Id => this.GetObjectNo();

        public PP PP { get; set; }
        public HH HH { get; set; }  // Calisan hesap (Skl=99)
        public TT TT { get; set; }  // Tag
        public string Ad { get; set; }
        public DateTime Trh { get; set; }
        public decimal Glr { get; set; }
        public decimal Gdr { get; set; }
        public decimal BklGlr { get; set; } // Beklenen
        public decimal BklGdr { get; set; }

        public DateTime? InsTrh { get; set; }
        public DateTime? UpdTrh { get; set; }
        public CU InsUsr { get; set; }
        public CU UpdUsr { get; set; }

        public string PPAd => PP?.Ad;
        public ulong HHId => HH?.GetObjectNo() ?? 0;
        public string HHAd => HH?.Ad;
        public string HHAdFull => HH?.AdFull;
        public string HHAdPrn => HH?.AdPrn;
        public ulong TTId => TT?.GetObjectNo() ?? 0;
        public string TTAd => TT?.Ad;

        public string TrhZ => $"{Trh:O}";
        public string TrhX => Trh.Hour == 0 && Trh.Minute == 0 ? $"{Trh:dd.MM.yy}" : $"{Trh:dd.MM.yy HH:mm}";
        public string GlrX => $"{Glr:#,#.##;-#,#.##;#}";
        public string GdrX => $"{Gdr:#,#.##;-#,#.##;#}";
        public string BklGlrX => $"{BklGlr:#,#.##;-#,#.##;#}";
        public string BklGdrX => $"{BklGdr:#,#.##;-#,#.##;#}";

        public static string InsertRec(long ppId, long hhId, long ttId, string Trh, string Ad, string TutTur, decimal Tut, long cuId)
        {
            string msj = "";
            DateTime dt;
            Db.Transact(() =>
            {
                if (Db.FromId((ulong)ppId) is PP pp)
                {
                    try
                    {
                        dt = Convert.ToDateTime(Trh);
                    }
                    catch (Exception)
                    {
                        dt = Convert.ToDateTime(Trh.Substring(0, 10));
                    }
                    if (cuId == 0 || dt.Date >= DateTime.Today)
                    {
                        TT tt = Db.FromId((ulong)ttId) as TT;
                        decimal Gdr = 0;
                        decimal Glr = 0;
                        decimal BklGdr = 0;
                        decimal BklGlr = 0;
                        switch (TutTur)
                        {
                            case "GI":
                                Glr = Tut;
                                break;
                            case "GX":
                                Gdr = Tut;
                                break;
                            case "BI":
                                BklGlr = Tut;
                                break;
                            case "BX":
                                BklGdr = Tut;
                                break;
                            default:
                                break;
                        }

                        if (Db.FromId((ulong)hhId) is HH hh)
                        {
                            new FF()
                            {
                                PP = pp,
                                HH = hh,
                                TT = tt,

                                Ad = Ad,
                                Trh = dt, // Convert.ToDateTime(Trh),
                                Glr = Glr,
                                Gdr = Gdr,
                                BklGlr = BklGlr,
                                BklGdr = BklGdr,

                                InsTrh = DateTime.Now,
                                InsUsr = Db.FromId((ulong)cuId) as CU
                            };
                            FF.PostMdf(hh.Id);
                        }
                        else
                            msj = "Tanımsız Hesap!";
                    }
                    else
                        msj = "Geçmiş tarihe kayıt giremezsiniz!";
                }
            });
            return msj;
        }

        public static string UpdateRec(ulong Id, ulong hhId, ulong ttId, string Trh, string Ad, string TutTur, decimal Tut, ulong cuId)
        {
            string msj = "";
            Db.Transact(() =>
            {
                if (Db.FromId((ulong)Id) is FF ff)
                {
                    if (cuId == 0 || ((ulong)cuId == ff.GetObjectNo() && ff.Trh.Date >= DateTime.Today))
                    {
                        TT tt = Db.FromId(ttId) as TT;
                        if (Db.FromId(hhId) is HH hh)
                        {
                            ff.HH = hh;
                            ff.TT = tt;
                            ff.Ad = Ad;
                            ff.Trh = Convert.ToDateTime(Trh);
                            ff.Gdr = 0;
                            ff.Glr = 0;
                            ff.BklGdr = 0;
                            ff.BklGlr = 0;
                            switch (TutTur)
                            {
                                case "GX":
                                    ff.Gdr = Tut;
                                    break;
                                case "GI":
                                    ff.Glr = Tut;
                                    break;
                                case "BX":
                                    ff.BklGdr = Tut;
                                    break;
                                case "BI":
                                    ff.BklGlr = Tut;
                                    break;
                                default:
                                    break;
                            }
                            ff.UpdTrh = DateTime.Now;
                            ff.UpdUsr = Db.FromId(cuId) as CU;
                            FF.PostMdf(hh.Id);
                        }
                    }
                    else
                        msj = "Değiştiremezsiniz! Kayıt sizin değil / Geçmiş tarihli.";
                }
            });
            return msj;
        }

        public static string DeleteRec(ulong Id, ulong cuId)
        {
            string msj = "";
            Db.Transact(() =>
            {
                if (Db.FromId(Id) is FF ff)
                {
                    if (cuId == 0 || (cuId == ff.InsUsr.GetObjectNo() && ff.Trh.Date >= DateTime.Today))
                    {
                        var hhId = ff.HHId;
                        ff.Delete();
                        FF.PostMdf(hhId);
                    }
                    else
                        msj = "Silemezsiniz! Kayıt sizin değil / Geçmiş tarihli.";
                }
                else
                    msj = "Kayıt bulunamadı";
            });
            return msj;
        }

        public static void ViewZZ(HH hh)
        {
            List<HH> list = new List<HH>();
            Leafs(hh, list);
            var aaa = list.Count;
            DateTime bbb;
            foreach (var h in list)
            {
                foreach(var f in Db.SQL<FF>("select r from FF r where r.HH = ?", h))
                {
                    bbb = f.Trh;
                }
            }
        }
        
        public static IEnumerable<FF> View(long ppId, long hhId, long ttId, string basTrhX, string bitTrhX, string trhTur = "F")
        {
            bool findHH = false,
                 findTT = false;

            bool foundHH = false,
                 foundTT = false;

            if (Db.FromId((ulong)ppId) is PP pp)
            {
                List<HH> hhList = new List<HH>();
                if (Db.FromId((ulong)hhId) is HH hh)
                {
                    findHH = true;
                    Leafs(hh, hhList);
                }
                if (Db.FromId((ulong)ttId) is TT tt)
                {
                    findTT = true;
                }

                IEnumerable<FF> ffs;

                DateTime basTrh, bitTrh;
                if (string.IsNullOrEmpty(basTrhX))
                    basTrh = DateTime.MinValue;
                else
                    basTrh = Convert.ToDateTime(basTrhX);
                if (string.IsNullOrEmpty(bitTrhX))
                    bitTrh = DateTime.MaxValue;
                else
                    bitTrh = Convert.ToDateTime(bitTrhX).AddDays(1);

                if (trhTur == "Y")
                    ffs = Db.SQL<FF>("select r from FF r where r.PP = ? and r.InsTrh >= ? and r.InsTrh < ? order by r.InsTrh DESC", pp, basTrh, bitTrh);
                else if (trhTur == "E")
                    ffs = Db.SQL<FF>("select r from FF r where r.PP = ? and r.UpdTrh >= ? and r.UpdTrh < ? order by r.UpdTrh DESC", pp, basTrh, bitTrh);
                else
                    ffs = Db.SQL<FF>("select r from FF r where r.PP = ? and r.Trh >= ? and r.Trh < ? order by r.Trh DESC", pp, basTrh, bitTrh);

                foreach(var ff in ffs)
                {
                    foundHH = !findHH || hhList.Contains(ff.HH);
                    foundTT = !findTT || ff.TT?.GetObjectNo() == (ulong)ttId;
                    if(foundHH && foundTT)
                        yield return ff;
                }
            }
        }

        public static IEnumerable<FF> View3(long ppId, long hhId, long ttId, string basTrhX, string bitTrhX)
        {
            if (Db.FromId((ulong)hhId) is HH hh)
            {
                List<HH> hhList = new List<HH>();
                int Lvl = hh.Lvl;

                //if (hh.Skl == 99)    // Zaten leaf
                //    hhList.Add(hh);
                //else
                Leafs(hh, hhList);

                List<FF> ffList = new List<FF>();
                foreach (var h in hhList)
                {
                    foreach (var f in Db.SQL<FF>("select r from FF r where r.HH = ?", h))
                    {
                        ffList.Add(f);
                    }
                }

                foreach (var f in ffList)
                    yield return f;
            }
            else if (Db.FromId((ulong)ttId) is TT tt)
            {
                foreach (var f in Db.SQL<FF>("select r from FF r where r.TT = ?", tt))
                {
                    yield return f;
                }
            }
            else if (Db.FromId((ulong)ppId) is PP pp)
            {
                IEnumerable<FF> ffs;
                if (!string.IsNullOrEmpty(basTrhX))
                {
                    DateTime basTrh = Convert.ToDateTime(basTrhX);
                    DateTime bitTrh = Convert.ToDateTime(bitTrhX);
                    ffs = Db.SQL<FF>("select r from FF r where r.PP = ? and r.Trh >= ? and r.Trh <= ?", pp, basTrh, bitTrh);
                }
                else
                    ffs = Db.SQL<FF>("select r from FF r where r.PP = ? order by r.Trh DESC", pp);

                foreach (var f in ffs)
                    yield return f;
            }
        }


        public static IEnumerable<FF> View(HH hh)
        {
            // Herhangi bir node'un (Leaf olmasi gerekmiyor) altindaki Leaf lerin hareketleri FF
            // 1. FFs of PP
            // 2. FFs of PP + HH(s)
            // 3.        PP + Trh
            List<HH> hhList = new List<HH>();
            int Lvl = hh.Lvl;

            if (hh.Skl == 99)    // Zaten leaf
                hhList.Add(hh);
            else
                Leafs(hh, hhList);

            List<FF> ffList = new List<FF>();
            foreach (var h in hhList)
            {
                foreach (var f in Db.SQL<FF>("select r from FF r where r.HH = ?", h))
                {
                    ffList.Add(f);
                }
            }

            foreach (var f in ffList)
                yield return f;

        }

        public static void Leafs(HH node, List<HH> list)
        {
            if (node.Skl == 99)    // Zaten leaf
                list.Add(node);

            var HHs = Db.SQL<HH>("select r from HH r where r.Prn = ?", node);
            foreach (var hh in HHs)
            {
                //if (hh.Skl == 99)
                //    list.Add(hh);
                Leafs(hh, list);
            }
        }

        public static void PostMdf(HH hh)    // FF ins/upd/del Sonrasi yapilacaklar
        {
            // Fisler toplamini bul. Fis insert/edit yapildiginda bunu yap
            // Fislerde sadece Leaf hesaplar calistigi icin 
            decimal glr = 0, gdr = 0;
            foreach (var ff in Db.SQL<FF>("select r from FF r where r.HH = ?", hh))
            {
                gdr += ff.Gdr;
                glr += ff.Glr;
            }
            Db.Transact(() =>
            {
                hh.GrcGlr = glr;
                hh.GrcGdr = gdr;
            });

            // Ust Toplamlari guncelle
            HH.UpdateParentsGrcToplam(hh);

            /*
                Tuple<decimal, decimal> res = Db.SQL<Tuple<decimal, decimal>>("SELECT SUM(r.Glr), SUM(r.Gdr) FROM FF r WHERE r.HH = ?", hh).FirstOrDefault();
            if (res != null)
            {
                // Hesabi guncelle
                Db.Transact(() =>
                {
                    hh.GrcGlr = res.Item1;
                    hh.GrcGdr = res.Item2;
                });

                // Ust Toplamlari guncelle
                HH.UpdateParentsGrcToplam(hh);
            }*/
        }
        public static void PostMdf(ulong hhId)
        {
            if (Db.FromId(hhId) is HH hh)
                PostMdf(hh);
        }
    }

    // HH dolaylı yoldan Hesabın kime ait oldugunu biliyor.
    // Performans acisindan PP alanini ekle
    // CC ve PP icin kayitlari otomatik acilir (CC&PP.HHroot bunlari tutar). Proje altini kullanici acar.
    [Database]
    public class HH    // HesapPlani
    {
        public ulong Id => this.GetObjectNo();

        public PP PP { get; set; }
        public HH Prn { get; set; }       // Parent

        public int No { get; set; }
        public string Ad { get; set; }
        public string Info { get; set; }

        public int Lvl { get; set; }
        public int Skl { get; set; }    // 0:Client, 1:Proje, 2..8:AraHesap, 99:CalisanHesap/Leaf

        public decimal GrcGlr { get; set; }
        public decimal GrcGdr { get; set; }
        public decimal ThmGlr { get; set; }
        public decimal ThmGdr { get; set; }


        public bool IsLeaf => Db.SQL<HH>("select r from DBMM0.HH r where r.Prn = ?", this).FirstOrDefault() == null ? true : false;
        // FFde kaydi varsa Altina hesap acilamaz
        public bool HasHrk => Db.SQL<FF>("select r from DBMM0.FF r where r.HH = ?", this).FirstOrDefault() == null ? false : true;

        public string PPAd => PP?.Ad;
        public string GrcGlrX => $"{GrcGlr:#,#.##;-#,#.##;#}";
        public string GrcGdrX => $"{GrcGdr:#,#.##;-#,#.##;#}";
        public string ThmGlrX => $"{ThmGlr:#,#.##;-#,#.##;#}";
        public string ThmGdrX => $"{ThmGdr:#,#.##;-#,#.##;#}";

        public string AdFull
        {
            get
            {
                string full = this.Ad;
                HH pHH = this.Prn;
                while (pHH != null && pHH.Lvl > 1)
                {
                    //full = $"{full}◄{pHH.Ad}";
                    full = $"{pHH.Ad}.{full}";
                    pHH = pHH.Prn;
                }
                return full;
            }
        }
        public string AdPrn
        {
            get
            {
                string adParent = "";
                HH pHH = Prn;
                while (pHH != null && pHH.Lvl > 1)
                {
                    adParent = $"{pHH.Ad}●{adParent}";
                    pHH = pHH.Prn;
                }
                return adParent.TrimEnd(new char[] { '●' });
            }
        }


        public static string InsertRec(long ppId, long prnId, string Ad, decimal ThmGdr, decimal ThmGlr, string Info)
        {
            // Hareketi/FF olan bir hesabin altina kayit acilack ise
            // Mevcut Parent'dan Yeni Parent yarat. Eskisi ve AltHesap buna baglanacak
            // Mevcut Parent'in adina "Eski" ekle, YeniParent'e bagla
            // AltHesapAc Yeniye bagla
            // AAA nin FF hareketleri var altina BBB acilacak
            // Sonunda Boyle olacak:
            // AAA
            //  EskiAAA
            //  BBB

            string msj = "";
            Db.Transact(() =>
            {
                if (Db.FromId((ulong)prnId) is HH phh)
                {
                    if (phh.HasHrk)
                    {
                        //msj = "Hesabın hareketleri var, Bu hesabın altına hesap ekleyemezsiniz.";
                        // Parent kaydi duplicate et, Yeni Parent bu olacak. Eskisi ve AltHesap buna baglanacak
                        HH newPHH = new HH
                        {
                            Prn = phh.Prn,
                            PP = phh.PP,
                            Lvl = phh.Lvl,
                            Skl = phh.Skl,
                            Ad = phh.Ad,
                            Info = phh.Info,
                            ThmGdr = phh.ThmGdr,
                            ThmGlr = phh.ThmGlr
                        };

                        // Mevcut prn'in adine Eski ekleyip Parintini newPHH yap
                        phh.Ad = $"Eski {phh.Ad}";
                        phh.Prn = newPHH;
                        HH.PostIns(phh);

                        // AltHesabi Ac
                        HH newHH = new HH
                        {
                            Prn = newPHH,
                            PP = Db.FromId<PP>((ulong)ppId),
                            Ad = Ad,
                            Info = Info,
                            ThmGdr = ThmGdr,
                            ThmGlr = ThmGlr
                        };
                        HH.PostIns(newHH);
                    }
                    else
                    {
                        HH hh = new HH
                        {
                            Prn = phh,
                            PP = Db.FromId<PP>((ulong)ppId),
                            Ad = Ad,
                            Info = Info,
                            ThmGdr = ThmGdr,
                            ThmGlr = ThmGlr
                        };
                        HH.PostIns(hh);
                    }
                }
            });
            return msj;
        }

        public static void UpdateRec(long Id, string Ad, decimal ThmGdr, decimal ThmGlr, string Info)
        {
            Db.Transact(() =>
            {
                if (Db.FromId((ulong)Id) is HH hh)
                {
                    hh.Ad = Ad;
                    hh.Info = Info;
                    hh.ThmGdr = ThmGdr;
                    hh.ThmGlr = ThmGlr;
                }
            });

        }

        public static string DeleteRec(long Id)
        {
            string msj = "";
            Db.Transact(() =>
            {
                if (Db.FromId((ulong)Id) is HH hh)
                {
                    if (hh.HasHrk)
                        msj = "Hareketleri var silemezsiniz.";
                    else if (hh.Skl == 1)
                        msj = "Bu Hesabı silemezsiniz.";    // Proje
                    else if (hh.Skl < 99)
                        msj = "Alt Hesapları var silemezsiniz.";
                    else
                    {
                        HH pHH = hh.Prn;
                        hh.Delete();
                        PostDel(pHH);
                    }
                }
            });
            return msj;
        }

        public static void PostDel(HH phh)
        {
            if (phh.IsLeaf)
                PostIns(phh);
        }

        public static void PostIns(HH hh)
        {
            // Insert sonrasi Lvl hesapla
            int lvl = 0;
            HH pHH = hh.Prn;

            while(pHH != null)
            {
                lvl++;
                pHH = pHH.Prn;
            }

            Db.Transact(() =>
            {
                hh.Lvl = lvl;
                hh.Skl = lvl;
                if (lvl > 1)    // 0:Client, 1:Proje, 2..8: AraHesaplar, 99:LeafNode
                {
                    hh.Skl = 99; // Leaf
                    hh.Prn.Skl = hh.Prn.Lvl;     // Parent'i AraHesap yap
                }

            });
        }

        public static string AdFullLvl(HH hh, int Lvl)
        {
            string full = hh.Ad;
            HH pHH = hh.Prn;
            while (pHH != null && pHH.Lvl > Lvl)
            {
                full = $"{full}◄{pHH.Ad}";
                pHH = pHH.Prn;
            }
            return full;
        }

        public static string FullParentAd(HH hh)
        {
            string fullAd = $"{hh.Ad}";

            HH pHH = hh.Prn;

            while (pHH != null && pHH.Lvl > 1)
            {
                fullAd = $"{pHH.Ad}►{fullAd}";
                pHH = pHH.Prn;
            }
            return fullAd;
        }

        public static IEnumerable<CumBkyFF> CumBky(HH node)
        {
            List<HH> hhList = new List<HH>();

            if (node.Skl == 99)    // Zaten leaf
                hhList.Add(node);
            else
                LeafsOfNode(node, hhList);

            List<FF> ffList = new List<FF>();
            foreach (var hh in hhList)
            {
                foreach (var ff in Db.SQL<FF>("select r from FF r where r.HH = ?", hh))
                {
                    ffList.Add(ff);
                }
            }

            var ffGrp = ffList
               .GroupBy(s => new { s.Trh.Year, s.Trh.Month })
               .Select(g => new
               {
                   Yil = g.Key.Year,
                   Ay = g.Key.Month,
                   Glr = g.Sum(x => x.Glr),
                   Gdr = g.Sum(x => x.Gdr),
                   BklGlr = g.Sum(x => x.BklGlr),
                   BklGdr = g.Sum(x => x.BklGdr),
                   Adt = g.Count()
               });

            List<CumBkyFF> cbList = new List<CumBkyFF>();

            decimal CumBky = 0, BklCumBky = 0;
            foreach (var f in ffGrp.OrderBy((x) => x.Yil).ThenBy((x) => x.Ay))
            {
                CumBky += f.Glr - f.Gdr;
                BklCumBky += f.BklGlr - f.BklGdr;
                cbList.Add(new CumBkyFF
                {
                    Yil = f.Yil,
                    Ay = f.Ay,
                    Glr = f.Glr,
                    Gdr = f.Gdr,
                    BklGlr = f.BklGlr,
                    BklGdr = f.BklGdr,
                    Adt = f.Adt,
                    CumBky = CumBky,
                    BklCumBky = BklCumBky,
                });
            }

            foreach (var f in cbList.OrderByDescending((x) => x.Yil).ThenByDescending((x) => x.Ay))
            {
                yield return f;
            }

        }

        public static IEnumerable<HH> View(PP pp)
        {
            var hh = pp.HHroot;
            List<HH> list = new List<HH>();
            ChildreenOfNode(hh, 1, list);

            yield return hh;
            foreach (var h in list)
            {
                yield return h;
            }
        }

        public static void ChildreenOfNode(HH node, int lvl, List<HH> list)
        {
            var HHs = Db.SQL<HH>("select r from HH r where r.Prn = ? order by r.Ad", node);
            foreach (var hh in HHs)
            {
                list.Add(hh);

                ChildreenOfNode(hh, lvl + 1, list);
            }
        }

        public static bool CanCopyTo(PP dpp)
        {
            // dPP nin HH leri olmamali (Sadece PP ye bagli AnaHesap disinda);
            var hhCount = Db.SQL<HH>("select r from HH r where r.PP = ?", dpp).Count();
            return hhCount == 1;
        }

        public static void CopyFromPP(PP spp, PP dpp)
        {
            if (!CanCopyTo(dpp))
                return;

            HH[] dhh = new HH[9];
            dhh[0] = dpp.HHroot;
            int not = 0;

            List<HH> list = new List<HH>();
            ChildreenOfNode(spp.HHroot, 0, list);

            Db.Transact(() =>
            {
                foreach (var hh in list)
                {
                    not = hh.Lvl - 1;   // 2den baslar. 1den baslasin 0:dpp.HHroot 

                    dhh[not] = new HH
                    {
                        PP = dpp,
                        Prn = dhh[not-1],
                        Ad = hh.Ad,
                    };

                }
            });
        }

        public static void LeafsOfNode(HH node, List<HH> hhList)
        {
            var HHs = Db.SQL<HH>("select r from HH r where r.Prn = ?", node);
            foreach (var hh in HHs)
            {
                if (hh.Skl == 99)
                    hhList.Add(hh);

                LeafsOfNode(hh, hhList);
            }
        }

        public static void UpdateParentsGrcToplam(HH leaf)
        {
            // Leaf'den baslayip Roota kadar. Ara hesaplar icin yapilmaz.
            Db.Transact(() =>
            {
                if (leaf.Prn != null)   // Parent/Ust hesabi varsa
                {
                    HH pHH = leaf.Prn;
                    decimal GrcGlr = 0, GrcGdr = 0;
                    do
                    {
                        GrcGlr = 0;
                        GrcGdr = 0;
                        foreach (var hh in Db.SQL<HH>("select r from HH r where r.Prn = ?", pHH))
                        {
                            GrcGlr += hh.GrcGlr;
                            GrcGdr += hh.GrcGdr;
                        }
                        pHH.GrcGlr = GrcGlr;
                        pHH.GrcGdr = GrcGdr;

                        pHH = pHH.Prn;
                    }
                    while (pHH != null);
                }
            });
        }


    }

    public class hhTree
    {
        public ulong ONo;
        public string hNo;
        public string Ad;
    }

    public class CumBkyFF
    {
        public int Yil;
        public int Ay;
        public decimal Glr;
        public decimal Gdr;
        public decimal CumBky;
        public decimal BklGlr;
        public decimal BklGdr;
        public decimal BklCumBky;
        public int Adt;

        public string GlrX => $"{Glr:#,#.##;-#,#.##;#}";
        public string GdrX => $"{Gdr:#,#.##;-#,#.##;#}";
        public string CumBkyX => $"{CumBky:#,#.##;-#,#.##;#}";
        public string BklGlrX => $"{BklGlr:#,#.##;-#,#.##;#}";
        public string BklGdrX => $"{BklGdr:#,#.##;-#,#.##;#}";
        public string BklCumBkyX => $"{BklCumBky:#,#.##;-#,#.##;#}";
    }
}