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

            void Handle(Input.SignT Action)
            {
                CC cc = null;
                var p = this.Parent as MasterPage;

                if (Action.Value < 0)   // AutoSign In/Out
                {
                    if (!string.IsNullOrEmpty(Token))  // AutoSignIn
                    {
                        cc = Db.SQL<CC>("select r from CC r where r.Token = ?", Token).FirstOrDefault();
                        if (cc == null)
                        {
                            Token = "";
                            p.Token = "";
                            Mesaj = "Hatali Token";
                        }
                        else
                        {
                            p.Token = Token;
                            Mesaj = "Signed";
                        }
                    }
                    else  // AutoSignOut
                    {
                        p.Token = "";
                        Mesaj = "UnSigned";
                    }
                }
                else
                {
                    if (IsOpened)   // SignIn
                    {
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
                                }
                                else
                                {
                                    Pwd = "";
                                    Token = "";
                                    p.Token = "";
                                    Mesaj = "Hatali eMail/Password";
                                }
                            }
                            else   // SignUp
                            {
                                var newToken = Hlp.EncodeQueryString(Email); // CreateToken
                                Db.Transact(() =>
                                {
                                    new CC
                                    {
                                        Email = Email,
                                        Pwd = Pwd,
                                        Token = newToken,
                                        InsTS = DateTime.Now,
                                        IsConfirmed = false,
                                    };
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
                    else   // SignOut / SignIn Request
                    {
                        if (!string.IsNullOrEmpty(Token))  // SignOut
                        {
                            Token = "";
                            p.Token = "";
                            Mesaj = "SignedOut";
                        }
                        else   // SignIn Request
                        {
                            IsOpened = true;
                        }
                    }
                }
            }

        }
    }
}
