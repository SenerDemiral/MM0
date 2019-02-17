using DBMM0;
using Starcounter;
using System;
using System.Linq;

namespace MM0.ViewModels
{
    partial class MasterPage : Json
    {
        [MasterPage_json.Sgn]
        partial class SgnPartial : Json
        {

            void Handle(Input.CancelT Action)
            {
                IsOpened = false;
            }

            void Handle(Input.OpnDlgT Action)
            {
                var p = this.Parent as MasterPage;
                if (p.Token == "")
                {
                    p.Sgn.Msj = "";
                    IsOpened = true;
                }
                else
                {
                    Hlp.Write2Log($"SignOut {Email}");
                    p.Token = "";
                    OpnDlgTxt = "Oturum Aç";
                    IsOpened = false;
                    p.CurrentPage = null;

                    p.MorphUrl = "/mm0/AboutPage";
                }
            }

            void Handle(Input.AutoSignT Action)
            {
                CC cc = null;
                CU cu = null;
                var p = this.Parent as MasterPage;
                // Iki sekli var. 
                // 1.Client/Admin sener.demiral@gmail.com
                // 2.Client/User sener.demiral@gmail.com/1

                cc = Db.SQL<CC>("select r from CC r where r.Token = ?", Token).FirstOrDefault();
                if (cc == null)
                {
                    cu = Db.SQL<CU>("select r from CU r where r.Token = ?", Token).FirstOrDefault();
                    if (cu != null)
                    {
                        cc = cu.CC;
                        p.Token = Token;
                        Email = cu.Email;
                        OpnDlgTxt = cu.Ad; // "Oturum Kapat";
                        p.CCId = (long)cc.Id;
                        p.CUId = (long)cu.Id;

                        p.MorphUrl = $"/mm0/PPs/{cc.Id}";
                        Msj = "Signed";
                        Hlp.Write2Log($"SignInA {cu.Email}");
                    }
                    else
                    {
                        Token = "";
                        p.Token = "";
                        OpnDlgTxt = "Oturum Aç";
                        p.CurrentPage = null;
                        p.MorphUrl = "/mm0/AboutPage";
                        Msj = "";
                        Hlp.Write2Log("SignIn");
                    }
                }
                else
                {
                    p.Token = Token;
                    Email = cc.Email;
                    OpnDlgTxt = "Oturum Kapat";
                    p.CCId = (long)cc.Id;
                    p.CUId = 0;
                    p.MorphUrl = $"/mm0/PPs/{cc.Id}";
                    Msj = "Signed";
                    Hlp.Write2Log($"SignInA {cc.Email}");
                }
                /*
                Session.RunTaskForAll((s, id) =>
                {
                    s.CalculatePatchAndPushOnWebSocket();
                });
                */
            }

            void Handle(Input.SignUpT Action)
            {
                var p = this.Parent as MasterPage;

                if (!string.IsNullOrEmpty(Email) && !string.IsNullOrEmpty(Pwd))
                {
                    // Email sonu /# ile bitiyor ise SignUp yapma
                    if (Email.Contains("/"))
                    {
                        Msj = "Hatalı seçenek.";
                    }
                    else
                    {
                        // Zaten kayitli mi?
                        var cc = Db.SQL<CC>("select r from CC r where r.Email = ?", Email).FirstOrDefault();
                        if (cc != null)    // Kayitli
                        {
                            if (cc.Pwd == Pwd)  // Dogru
                            {
                                if (cc.IsConfirmed) // SignIn
                                {
                                    p.Token = cc.Token;
                                    Pwd = "";
                                    IsOpened = false;
                                    OpnDlgTxt = "Oturum Kapat";
                                    p.MorphUrl = $"/mm0/PPs/{cc.Id}";
                                    Hlp.Write2Log($"SignIn. {cc.Email}");
                                }
                                else
                                {
                                    Msj = "Mailinize gelen linki tıklayarak doğrulama işlemini tamamlayın!";
                                    Hlp.Write2Log($"SignInW {cc.Email}");
                                }
                            }
                            else  // Kayitli Pwd degisikligi yapiyor
                            {
                                Db.Transact(() =>
                                {
                                    cc.IsConfirmed = false;
                                    cc.Pwd = Pwd;
                                    cc.Token = Hlp.EncodeQueryString($"{Email}/{Pwd}");
                                });
                                Hlp.SendMail(Email, cc.Token);
                                Email = "";
                                Pwd = "";
                                Token = "";
                                Msj = "Şifreniz değiştirildi. Mailinize gelen linki tıklayarak doğrulama işlemini tamamlayın.";
                            }
                        }
                        else   // SignUp  // Tekrar Confirm Maili gondermek gerekebilir!
                        {
                            var newToken = Hlp.EncodeQueryString($"{Email}/{Pwd}"); // CreateToken

                            CC.InsertRec(Email, Pwd, newToken);
                            Hlp.Write2Log($"SignUp. {Email}");

                            Hlp.SendMail(Email, newToken);
                            Email = "";
                            Pwd = "";
                            Token = "";
                            Msj = "Mailinize gelen linki tıklayarak doğrulama işlemini tamamlayın.";
                        }
                    }
                }
                else
                {
                    Msj = "Mail adresinizi ve şifrenizi girin.";
                }
            }


            void Handle(Input.SignInT Action)
            {
                CC cc = null;
                CU cu = null;
                var p = this.Parent as MasterPage;

                if (!string.IsNullOrEmpty(Email) && !string.IsNullOrEmpty(Pwd))
                {
                    cc = Db.SQL<CC>("select r from CC r where r.Email = ?", Email).FirstOrDefault();
                    if (cc != null)  // SignIn
                    {
                        if (!cc.IsConfirmed)
                        {
                            Pwd = "";
                            Token = "";
                            p.Token = "";
                            Msj = "Mailinize gelen linki tıklayarak doğrulama işlemini tamamlayın!";
                        }
                        else if (cc.Pwd == Pwd)
                        {
                            Pwd = "";
                            Token = cc.Token;
                            p.Token = cc.Token;
                            Msj = "";
                            IsOpened = false;
                            OpnDlgTxt = "Oturum Kapat";
                            p.CCId = (long)cc.Id;
                            p.CUId = 0;
                            p.MorphUrl = $"/mm0/PPs/{cc.Id}";

                            Hlp.Write2Log($"SignIn. {cc.Email}");
                        }
                        else
                        {
                            Hlp.Write2Log($"SignInX {cc.Email} {Pwd}");

                            Pwd = "";
                            Token = "";
                            p.Token = "";
                            Msj = "Hatali Password";

                        }
                    }
                    else
                    {
                        cu = Db.SQL<CU>("select r from CU r where r.Email = ? and r.Pwd = ?", Email, Pwd).FirstOrDefault();
                        if (cu != null)
                        {
                            cc = cu.CC;
                            Pwd = "";
                            Token = cu.Token;
                            p.Token = cu.Token;
                            Msj = "";
                            IsOpened = false;
                            OpnDlgTxt = cu.Ad; // "Oturum Kapat";
                            p.CCId = (long)cc.Id;
                            p.CUId = (long)cu.Id;
                            p.MorphUrl = $"/mm0/PPs/{cc.Id}";

                            Hlp.Write2Log($"SignIn. {cu.Email}");
                        }
                        else
                        {
                            Hlp.Write2Log($"SignInZ {Email} {Pwd}");

                            //Pwd = "";
                            Token = "";
                            p.Token = "";
                            Msj = "Hatali eMail";
                        }
                    }
                }
            }

        }
    }
}
