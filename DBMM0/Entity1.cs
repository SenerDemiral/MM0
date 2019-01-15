
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
        public string Mail { get; set; }
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

        public string TrhX => $"{Trh:dd.MM.yy}";
        public string GlrX => $"{Glr:#,#.##;-#,#.##;#}";
        public string GdrX => $"{Gdr:#,#.##;-#,#.##;#}";

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
        public int Skl { get; set; }    // 0:Client, 1:Proje, 2..8:AraHesap, 9:CalisanHesap/Leaf

        public decimal GrcGlr { get; set; }
        public decimal GrcGdr { get; set; }
        public decimal ThmGlr { get; set; }
        public decimal ThmGdr { get; set; }

        [Transient]
        public int Lvl;

        public bool IsLeaf => Db.SQL<HH>("select r from DBMM0.HH r where r.Prn = ?", this).FirstOrDefault() == null ? true : false;
        // FFde kaydi varsa Altina hesap acilamaz
        public bool HasHrk => Db.SQL<FF>("select r from DBMM0.FF r where r.HH = ?", this).FirstOrDefault() == null ? false : true;

        public string PPAd => PP?.Ad;
        public string GrcGlrX => $"{GrcGlr:#,#.##;-#,#.##;#}";
        public string GrcGdrX => $"{GrcGdr:#,#.##;-#,#.##;#}";
        public string ThmGlrX => $"{ThmGlr:#,#.##;-#,#.##;#}";
        public string ThmGdrX => $"{ThmGdr:#,#.##;-#,#.##;#}";

        public static IEnumerable<HH> View(string PPId)
        {
            var pp = Db.FromId<PP>(3);  // OguzProje1
            var hh = pp.HHroot;
            List<HH> list = new List<HH>();
            ViewTree(hh, 1, list);

            yield return hh;
            foreach (var h in list)
            {
                yield return h;
            }
        }
        public static void ViewTree(HH prn, int lvl, List<HH> list)
        {
            var HHs = Db.SQL<HH>("select r from HH r where r.Prn = ? order by r.No", prn);
            foreach (var hh in HHs)
            {
                string Ad2 = $"{new string(' ', lvl * 2)}{hh.Ad}";
                Console.WriteLine($"{hh.GetObjectNo():D8} {lvl} {Ad2,-20} {hh.GrcGlr} {hh.GrcGdr}");
                hh.Lvl = lvl;
                list.Add(hh);
                ViewTree(hh, lvl + 1, list);
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
            Leafs(prjRoot, leafList);

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

        public static void Leafs(HH prn, List<HH> hhList)
        {
            // Buna gerek kalmadi, HH.PP alani var artik
            // var leafs = Db.SQL<HH>("select r from HH where r.PP = ? and Skl = ?, PP, 9); // Skl=9 Working nodes

            var HHs = Db.SQL<HH>("select r from HH r where r.Prn = ?", prn);
            foreach (var hh in HHs)
            {
                if (hh.Skl == 9)
                { 
                    Console.WriteLine($"{hh.Ad}");
                    hhList.Add(hh);
                }
                Leafs(hh, hhList);
            }
        }


        public static IEnumerable<HH> sener(ulong no)
        {
            if (Db.FromId(no) is HH hh)
            {
                List<HH> leafList = new List<HH>();

                Leafs(hh, leafList);

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
            Db.Transact(() => {
                var oguzC = new CC
                {
                    Ad = "Oguz",
                };
                var oguzH = new HH
                {
                    No = 0,
                    Skl = 0,
                    Prn = null,
                };
                oguzC.HHroot = oguzH;
                oguzH.Ad = oguzC.Ad;

                var oguzP1 = new PP
                {
                    CC = oguzC,
                    Ad = "OguzProje1",
                    BasTrh = DateTime.Today.AddDays(-100),
                    BitTrh = DateTime.Today.AddDays(100),
                };
                var oguzP1H = new HH
                {
                    No = 1,
                    Skl = 1,
                    Prn = oguzH,
                    ThmGlr = 1000000,
                    ThmGdr = 900000
                };
                oguzP1.HHroot = oguzP1H;
                oguzP1H.Ad = oguzP1.Ad;


                var food = new HH
                {
                    PP = oguzP1,
                    No = 1,
                    Ad = "Food",
                    Skl = 2,
                    Prn = oguzP1H
                };

                    var fruit = new HH
                    {
                        PP = oguzP1,
                        No = 1,
                        Ad = "Fruit",
                        Skl = 2,
                        Prn = food
                    };
                        var red = new HH
                        {
                            PP = oguzP1,
                            No = 1,
                            Ad = "Red",
                            Skl = 2,
                            Prn = fruit
                        };
                            var cherry = new HH
                            {
                                PP = oguzP1,
                                No = 1,
                                Ad = "Cherry",
                                Skl = 9,
                                Prn = red,
                                GrcGlr = 10,
                                GrcGdr = 20
                            };
                            var apple = new HH
                            {
                                PP = oguzP1,
                                No = 2,
                                Ad = "Apple",
                                Skl = 9,
                                Prn = red,
                                GrcGlr = 30,
                                GrcGdr = 40
                            };
                        var yellow = new HH
                        {
                            PP = oguzP1,
                            No = 2,
                            Ad = "Yellow",
                            Skl = 2,
                            Prn = fruit
                        };
                            var banana = new HH
                            {
                                PP = oguzP1,
                                No = 1,
                                Ad = "Banana",
                                Skl = 9,
                                Prn = yellow,
                                GrcGlr = 50,
                                GrcGdr = 60
                            };

                    var meat = new HH
                    {
                        PP = oguzP1,
                        No = 2,
                        Ad = "Meat",
                        Skl = 2,
                        Prn = food
                    };
                        var beef = new HH
                        {
                            PP = oguzP1,
                            No = 1,
                            Ad = "Beef",
                            Skl = 9,
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
                            Ad = "Pork",
                            Skl = 9,
                            Prn = meat,
                            GrcGlr = 90,
                            GrcGdr = 95,
                            ThmGlr = 150000,
                            ThmGdr = 125000
                        };

                UpdateParentsGrcToplam(apple);
                UpdateParentsGrcToplam(cherry);
                UpdateParentsGrcToplam(banana);
                UpdateParentsGrcToplam(beef);
                UpdateParentsGrcToplam(pork);

            });
        }

    }

    public class hhTree
    {
        public ulong ONo;
        public string hNo;
        public string Ad;
    }
}