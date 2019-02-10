
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
        }

        public static void Kill(ulong ccId)
        {
            if (Db.FromId(ccId) is CC cc)
            {
                Db.Transact(() =>
                {
                    foreach(var pp in Db.SQL<PP>("select r from PP r where r.CC = ?", cc))
                    {
                        PP.Kill(pp.GetObjectNo());
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
            CC cc = Db.FromId<CC>((ulong)ccId);
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
                    CC = cc
                };
            });
            Db.Transact(() =>
            {
                hhNew = new HH()
                {
                    PP = ppNew,
                    Prn = cc.HHroot,
                    Ad = ppNew.Ad,
                };
            });
            Db.Transact(() =>
            {
                ppNew.HHroot = hhNew;
            });
            return ppNew;
        }

        public static void Kill(ulong ppId)
        {
            if (Db.FromId(ppId) is PP pp)
            {
                Db.Transact(() =>
                {
                    Db.SQL("delete from FF where PP = ?", pp);
                    Db.SQL("delete from HH where PP = ?", pp);
                    pp.Delete();
                });
            }
        }
    }

    // HH dolaylı yoldan Hesabın kime ait oldugunu biliyor.
    // Performans acisindan PP alanini eklendi
    [Database]
    public class FF //Fisler
    {
        public ulong Id => this.GetObjectNo();

        public PP PP { get; set; }
        public HH HH { get; set; }  // Altinda baska hesap olmamali
        public TT TT { get; set; }  // Tag
        public string Ad { get; set; }
        public DateTime Trh { get; set; }
        public decimal Glr { get; set; }
        public decimal Gdr { get; set; }

        public string PPAd => PP?.Ad;
        public ulong HHId => HH?.GetObjectNo() ?? 0;
        public string HHAd => HH?.Ad;
        public string HHAdPrn => HH?.AdPrn;
        public ulong TTId => TT?.GetObjectNo() ?? 0;
        public string TTAd => TT?.Ad;

        public string TrhZ => $"{Trh:O}";
        public string TrhX => Trh.Hour == 0 && Trh.Minute == 0 ? $"{Trh:dd.MM.yy}" : $"{Trh:dd.MM.yy HH:mm}";
        public string GlrX => $"{Glr:#,#.##;-#,#.##;#}";
        public string GdrX => $"{Gdr:#,#.##;-#,#.##;#}";

        public static string InsertRec(long ppId, long hhId, long ttId, string Trh, string Ad, decimal Gdr, decimal Glr)
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
                    TT tt = Db.FromId((ulong)ttId) as TT;
                    if (Db.FromId((ulong)hhId) is HH hh)
                    {
                        new FF()
                        {
                            PP = pp,
                            HH = hh,
                            TT = tt,

                            Ad = Ad,
                            Trh = dt, // Convert.ToDateTime(Trh),
                            Gdr = Gdr,
                            Glr = Glr
                        };
                    }
                    else
                        msj = "Tanımsız Hesap";
                }
            });
            return msj;
        }

        public static void UpdateRec(long Id, long hhId, long ttId, string Trh, string Ad, decimal Gdr, decimal Glr)
        {
            Db.Transact(() =>
            {
                if (Db.FromId((ulong)Id) is FF ff)
                {
                    TT tt = Db.FromId((ulong)ttId) as TT;
                    if (Db.FromId((ulong)hhId) is HH hh)
                    {
                        ff.HH = hh;
                        ff.TT = tt;
                        ff.Ad = Ad;
                        ff.Trh = Convert.ToDateTime(Trh);
                        ff.Gdr = Gdr;
                        ff.Glr = Glr;
                    }
                }
            });

        }

        public static string DeleteRec(long Id)
        {
            string msj = "";
            Db.Transact(() =>
            {
                if (Db.FromId((ulong)Id) is FF ff)
                {
                    var hhId = ff.HHId;
                    ff.Delete();
                    FF.PostMdf(hhId);

                }
                else
                    msj = "Silinemedi";
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
        
        public static IEnumerable<FF> View(long ppId, long hhId, long ttId, string basTrhX, string bitTrhX)
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
                if (!string.IsNullOrEmpty(basTrhX))
                {
                    DateTime basTrh = Convert.ToDateTime(basTrhX);
                    DateTime bitTrh = Convert.ToDateTime(bitTrhX).AddDays(1);
                    ffs = Db.SQL<FF>("select r from FF r where r.PP = ? and r.Trh >= ? and r.Trh < ?", pp, basTrh, bitTrh);
                }
                else
                    ffs = Db.SQL<FF>("select r from FF r where r.PP = ? order by r.Trh DESC", pp);

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
    public class HH     // HesapPlani
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


        //public bool IsLeaf => Db.SQL<HH>("select r from DBMM0.HH r where r.Prn = ?", this).FirstOrDefault() == null ? true : false;
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
                    full = $"{full}◄{pHH.Ad}";
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
            string msj = "";
            Db.Transact(() =>
            {
                if (Db.FromId((ulong)prnId) is HH phh)
                {
                    if (phh.HasHrk)
                        msj = "Hesabin hareketleri var, Bu hesabin altına hesap ekleyemezsiniz.";
                    else
                    {
                        new HH
                        {
                            Prn = phh,
                            PP = Db.FromId<PP>((ulong)ppId),
                            Ad = Ad,
                            Info = Info,
                            ThmGdr = ThmGdr,
                            ThmGlr = ThmGlr
                        };
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
                    else if (hh.Skl < 99)
                        msj = "Alt Hesapları var silemezsiniz.";
                    else
                        hh.Delete();
                }
            });
            return msj;
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
                   Gdr = g.Sum(x => x.Gdr),
                   Glr = g.Sum(x => x.Glr),
                   Adt = g.Count()
               });

            List<CumBkyFF> cbList = new List<CumBkyFF>();

            decimal CumBky = 0;
            foreach (var f in ffGrp.OrderBy((x) => x.Yil).ThenBy((x) => x.Ay))
            {
                CumBky += f.Glr - f.Gdr;
                cbList.Add(new CumBkyFF
                {
                    Yil = f.Yil,
                    Ay = f.Ay,
                    Gdr = f.Gdr,
                    Glr = f.Glr,
                    Adt = f.Adt,
                    CumBky = CumBky
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


        public static void Display()
        {
            Db.Transact(() =>
            {
                if (Db.FromId(14) is HH pork) {
                    pork.GrcGlr = 1000;
                    pork.Ad = "          Porks";
                    UpdateParentsGrcToplam(pork);
                }
            });


            List<hhTree> list = new List<hhTree>();
            var cc = Db.FromId<CC>(1);  // Oguz
            var hh = cc.HHroot;  
            int lvl = 0;
            Console.WriteLine($"{hh.GetObjectNo():D8} {hh.Ad}");
            //DisplayChildreen(hh, lvl, $"{hh.No:D2}", list);
            DisplayChildreen(hh, lvl, null, list);

            Console.WriteLine("");
            Console.WriteLine("List---------");
            foreach (var item in list)
            {
                Console.WriteLine($"{item.ONo} {item.Ad} {item.hNo}");
            }


            Console.WriteLine("");
            Console.WriteLine("Leafs---------");
            var prjRoot = Db.FromId<HH>(4);
            List<HH> leafList = new List<HH>();
            LeafsDeneme(prjRoot, leafList);

            foreach(var itm in sener(2))
                Console.WriteLine($"-------{itm.GetObjectNo()} {itm.Ad}");

        }

        public static void DisplayChildreen(HH prn, int lvl, string hspNo, List<hhTree> list)
        {
            var HHs = Db.SQL<HH>("select r from HH r where r.Prn = ? order by r.No", prn);
            foreach (var hh in HHs)
            {
                string hNo = string.IsNullOrEmpty(hspNo) ? $"{hh.No:D2}" : $"{hspNo}.{hh.No:D2}";
                //Console.WriteLine($"{hh.GetObjectNo():D8} {lvl} {hNo,-12} {new string(' ', lvl * 4)} {hh.Ad}"); // <- {hh.Prn?.Ad} ==== ");
                //Console.WriteLine($"{hh.GetObjectNo():D8} {lvl} {hNo} {hh.Ad}");
                //Console.WriteLine($"{hh.GetObjectNo():D8} {lvl} {hNo,-12} {hh.Ad}");
                string Ad2 = $"{new string(' ', lvl * 2)}{hh.Ad}";
                Console.WriteLine($"{hh.GetObjectNo():D8} {lvl} {Ad2, -20} {hh.GrcGlr} {hh.GrcGdr} {hNo}");
                list.Add(new hhTree
                {
                    ONo = hh.GetObjectNo(),
                    hNo = hNo,
                    Ad = hh.Ad
                });
                DisplayChildreen(hh, lvl + 1, hNo, list);
            }
        }

        public static void LeafsDeneme(HH prn, List<HH> hhList)
        {
            // Buna gerek kalmadi, HH.PP alani var artik
            // var leafs = Db.SQL<HH>("select r from HH where r.PP = ? and Skl = ?, PP, 9); // Skl=9 Working nodes

            var HHs = Db.SQL<HH>("select r from HH r where r.Prn = ?", prn);
            foreach (var hh in HHs)
            {
                if (hh.Skl == 99)
                { 
                    Console.WriteLine($"{hh.Ad}");
                    hhList.Add(hh);
                }
                LeafsDeneme(hh, hhList);
            }
        }


        public static IEnumerable<HH> sener(ulong no)
        {
            if (Db.FromId(no) is HH hh)
            {
                List<HH> leafList = new List<HH>();

                LeafsDeneme(hh, leafList);

                foreach (var h in leafList)
                {
                    yield return h;
                }
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


        public static void Populate()
        {
            CC oguzC = null;
            PP oguzP1 = null;
            HH oguzH = null, oguzP1H = null, food = null, fruit = null, red = null, yellow = null, meat = null;

            Db.Transact(() =>
            {
                oguzC = new CC
                {
                    Ad = "TEST",
                    Email = "test",
                    Pwd = "test",
                    Token = "test",
                    IsConfirmed = true
                };
            });
            Db.Transact(() =>
            {
                oguzH = new HH
                {
                    No = 0,
                    Prn = null,
                };
                oguzC.HHroot = oguzH;
                oguzH.Ad = oguzC.Ad;
            });
            Db.Transact(() =>
            {
                oguzP1 = new PP
                {
                    CC = oguzC,
                    Ad = "TestProje1",
                    BasTrh = DateTime.Today.AddDays(-100),
                    BitTrh = DateTime.Today.AddDays(100),
                };
                oguzP1H = new HH
                {
                    PP = oguzP1,
                    No = 1,
                    Prn = oguzH,
                    ThmGlr = 1000000,
                    ThmGdr = 900000
                };
                oguzP1.HHroot = oguzP1H;
                oguzP1H.Ad = oguzP1.Ad;
            });
            Db.Transact(() =>
            {
                food = new HH
                {
                    PP = oguzP1,
                    No = 1,
                    Ad = "Besin",
                    Prn = oguzP1H
                };
            });
            Db.Transact(() =>
            {
                fruit = new HH
                {
                    PP = oguzP1,
                    No = 1,
                    Ad = "Meyve",
                    Prn = food
                };
            });
            Db.Transact(() =>
            {
                red = new HH
                {
                    PP = oguzP1,
                    No = 1,
                    Ad = "Kırmızı Renkli",
                    Prn = fruit
                };
            });

            Db.Transact(() =>
            {
                var cherry = new HH
                {
                    PP = oguzP1,
                    No = 1,
                    Ad = "Çilek",
                    Prn = red,
                    GrcGlr = 10,
                    GrcGdr = 20
                };
                var apple = new HH
                {
                    PP = oguzP1,
                    No = 2,
                    Ad = "Elma",
                    Prn = red,
                    GrcGlr = 30,
                    GrcGdr = 40
                };
            });
            Db.Transact(() =>
            {
                yellow = new HH
                {
                    PP = oguzP1,
                    No = 2,
                    Ad = "Sarı Renkli",
                    Prn = fruit
                };
            });
            Db.Transact(() =>
            {
                var banana = new HH
                {
                    PP = oguzP1,
                    No = 1,
                    Ad = "Muz",
                    Prn = yellow,
                    GrcGlr = 50,
                    GrcGdr = 60
                };

            });
            Db.Transact(() =>
            {
                meat = new HH
                {
                    PP = oguzP1,
                    No = 2,
                    Ad = "Et",
                    Prn = food
                };
            });
            Db.Transact(() =>
            {
                var beef = new HH
                {
                    PP = oguzP1,
                    No = 1,
                    Ad = "Biftek",
                    Prn = meat,
                    GrcGlr = 70,
                    GrcGdr = 80,
                    ThmGlr = 285000,
                    ThmGdr = 200000
                };
                var pork = new HH
                {
                    PP = oguzP1,
                    No = 2,
                    Ad = "Tavuk",
                    Prn = meat,
                    GrcGlr = 90,
                    GrcGdr = 95,
                    ThmGlr = 150000,
                    ThmGdr = 125000
                };
            });

            /*
            Db.TransactAsync(() => {
                UpdateParentsGrcToplam(apple);
                UpdateParentsGrcToplam(cherry);
                UpdateParentsGrcToplam(banana);
                UpdateParentsGrcToplam(beef);
                UpdateParentsGrcToplam(pork);

            });*/
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
        public decimal Gdr;
        public decimal Glr;
        public decimal CumBky;
        public int Adt;

        public string GdrX => $"{Gdr:#,#.##;-#,#.##;#}";
        public string GlrX => $"{Glr:#,#.##;-#,#.##;#}";
        public string CumBkyX => $"{CumBky:#,#.##;-#,#.##;#}";
    }
}