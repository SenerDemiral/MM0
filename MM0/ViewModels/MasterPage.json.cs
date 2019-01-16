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
                    IsOpened = true;
                }
                else
                {
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
                    Mesaj = "";
                }
                else
                {
                    p.Token = Token;
                    OpnDlgTxt = "Oturum Kapat";
                    p.MorphUrl = $"/mm0/PPs/{cc.Id}";
                    Mesaj = "Signed";
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
                        p.Token = cc.Token;
                        Pwd = "";
                        IsOpened = false;
                        Mesaj = "";
                        OpnDlgTxt = "Oturum Kapat";
                        p.MorphUrl = $"/mm0/PPs/{cc.Id}";
                    }
                    else   // SignUp
                    {
                        var newToken = Hlp.EncodeQueryString(Email); // CreateToken
                        CC ccNew = null;
                        Db.Transact(() =>
                        {
                            ccNew = new CC
                            {
                                Email = Email,
                                Pwd = Pwd,
                                Token = newToken,
                                InsTS = DateTime.Now,
                                IsConfirmed = false,
                            };
                        });
                        Db.Transact(() =>
                        {
                            HH hh = new HH
                            {
                                Ad = ccNew.Ad,
                            };
                            ccNew.HHroot = hh;
                        });

                        var email = Hlp.EncodeQueryString(Email);
                        Hlp.SendMail(email);
                        Email = "";
                        Pwd = "";
                        Token = "";
                        Mesaj = "Mailinize gelen linki týklayarak doðrulama iþlemini tamamlayýn.";
                    }
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
                            Mesaj = "SignedIn";
                            IsOpened = false;
                            OpnDlgTxt = "Oturum Kapat";
                            p.MorphUrl = $"/mm0/PPs/{cc.Id}";
                        }
                        else
                        {
                            Pwd = "";
                            Token = "";
                            p.Token = "";
                            Mesaj = "Hatali eMail/Password";
                        }
                    }
                }
            }

        }
    }
}
