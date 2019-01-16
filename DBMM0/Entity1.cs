
using System;
using System.Collections.Generic;
using System.Linq;
using Starcounter;

namespace DBMM0
{
    [Database]
    public class CC //Clients
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

        public string Tel { get; set; }
        public DateTime UyeBasTrh { get; set; }
        public string UyePry { get; set; }  // Uyelik Periyodu. Aylik, Yillik

        public DateTime TstBasTrh { get; set; } // DenemeSuresi baslangici
        public DateTime TstBitTrh { get; set; }
    }

    [Database]
    public class PP //Projects
    {
        public ulong Id => this.GetObjectNo();

        public CC CC { get; set; }
        public string Ad { get; set; }
        public HH HHroot { get; set; }     // Starting Node @HH

        public DateTime BasTrh { get; set; }
        public DateTime BitTrh { get; set; }

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

    }

    // HH dolaylı yoldan Hesabın kime ait oldugunu biliyor.
    // Performans acisindan PP alanini eklendi
    [Database]
    public class FF //Fisler
    {
        public ulong Id => this.GetObjectNo();

        public PP PP { get; set; }
        public HH HH { get; set; }  // Altinda baska hesap olmamali
        public string Ad { get; set; }
        public DateTime Trh { get; set; }
        public decimal Glr { get; set; }
        public decimal Gdr { get; set; }

        public string PPAd => PP?.Ad;
        public string HHId => HH?.GetObjectNo().ToString();
        public string HHAd => HH?.Ad;

        public string TrhZ => $"{Trh:O}";
        public string TrhX => $"{Trh:dd.MM.yy}";
        public string GlrX => $"{Glr:#,#.##;-#,#.##;#}";
        public string GdrX => $"{Gdr:#,#.##;-#,#.##;#}";

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
        public static IEnumerable<FF> View(HH hh)
        {
            // Herhangi bir node'un (Leaf olmasi gerekmiyor) altindaki Leaf lerin hareketleri FF
            // 1. FFs of PP
            // 2. FFs of PP + HH(s)
            // 3.        PP + Trh
            List<HH> hhList = new List<HH>();

            if (hh.Skl == 99)    // Zaten leaf
                hhList.Add(hh);
            else
                Leafs(hh, hhList);

            List<FF> ffList = new List<FF>();
            foreach (var h in hhList)
            {
                foreach (var f in Db.SQL<FF>("select r from FF r where r.HH = ?", h))
                    ffList.Add(f);
            }

            foreach (var f in ffList)
                yield return f;

        }
        public static void Leafs(HH prn, List<HH> list)
        {
            var HHs = Db.SQL<HH>("select r from HH r where r.Prn = ?", prn);
            foreach (var hh in HHs)
            {
                if (hh.Skl == 99)
                    list.Add(hh);
                Leafs(hh, list);
            }
        }

        public static void PostFF(HH hh)    // FF ins/upd Sonrasi yapilacaklar
        {
            // Fisler toplamini bul. Fis insert/edit yapildiginda bunu yap
            // Fislerde sadece Leaf hesaplar calistigi icin 
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
            }
        }
        public static void PostFF(long hhONo)
        {
            if (Db.FromId((ulong)hhONo) is HH hh)
                PostFF(hh);
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

        public static string FullParentAd(HH hh)
        {
            string fullAd = $"{hh.Ad}";

            HH pHH = hh.Prn;

            while (pHH != null && pHH.Lvl > 0)
            {
                fullAd = $"{pHH.Ad} ► {fullAd}";
                pHH = pHH.Prn;
            }
            return fullAd;
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
                    Ad = "Test",
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
                    Ad = "Büftek",
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
}