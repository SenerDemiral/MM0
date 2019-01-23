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
                var p = this.Parent as MasterPage;

                cc = Db.SQL<CC>("select r from CC r where r.Token = ?", Token).FirstOrDefault();
                if (cc == null)
                {
                    Token = "";
                    p.Token = "";
                    OpnDlgTxt = "Oturum Aç";
                    p.CurrentPage = null;
                    p.MorphUrl = "/mm0/AboutPage";
                    Msj = "";
                    Hlp.Write2Log("SignIn");
                }
                else
                {
                    p.Token = Token;
                    Email = cc.Email;
                    OpnDlgTxt = "Oturum Kapat";
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
                    // Zaten kayitli mi?
                    var cc = Db.SQL<CC>("select r from CC r where r.Email = ?", Email).FirstOrDefault();
                    if (cc != null && cc.Pwd == Pwd)    // Kayitli ve dogru
                    {
                        if (cc.IsConfirmed)
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
                            Msj = "Mailinize gelen linki týklayarak doðrulama iþlemini tamamlayýn.";
                            Hlp.Write2Log($"SignInW {cc.Email}");
                        }
                    }
                    else   // SignUp  // Tekrar Confirm Maili gondermek gerekebilir!
                    {
                        var newToken = Hlp.EncodeQueryString(Email); // CreateToken

                        CC.InsertRec(Email, Pwd, newToken);
                        Hlp.Write2Log($"SignUp. {Email}");

                        Hlp.SendMail(Email, newToken);
                        Email = "";
                        Pwd = "";
                        Token = "";
                        Msj = "Mailinize gelen linki týklayarak doðrulama iþlemini tamamlayýn.";
                    }
                }
                else
                {
                    Msj = "Mail adresinizi ve þifrenizi girin.";
                }
            }


            void Handle(Input.SignInT Action)
            {
                CC cc = null;
                var p = this.Parent as MasterPage;

                if (!string.IsNullOrEmpty(Email) && !string.IsNullOrEmpty(Pwd))
                {
                    cc = Db.SQL<CC>("select r from CC r where r.Email = ?", Email).FirstOrDefault();
                    if (cc != null)  // SignIn
                    {
                        if (cc.Pwd == Pwd)
                        {
                            Pwd = "";
                            Token = cc.Token;
                            p.Token = cc.Token;
                            Msj = "";
                            IsOpened = false;
                            OpnDlgTxt = "Oturum Kapat";
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
