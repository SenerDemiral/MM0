﻿using Starcounter;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace DBMM0
{
    public static class Hlp
    {
        public static CultureInfo cultureTR = CultureInfo.CreateSpecificCulture("tr-TR");  // Tarihde gun gostermek icin

        public static void Indexes()
        {
            if (Db.SQL("SELECT i FROM Starcounter.Metadata.\"Index\" i WHERE Name = ?", "FF_Trh").FirstOrDefault() != null)
                Db.SQL("DROP INDEX FF_Trh ON FF");

            // CC:Client

            // PP:Project
            if (Db.SQL("SELECT i FROM Starcounter.Metadata.\"Index\" i WHERE Name = ?", "PP_CC").FirstOrDefault() == null)
                Db.SQL("CREATE INDEX PP_CC ON PP (CC)");

            // HH:HesapPlani
            if (Db.SQL("SELECT i FROM Starcounter.Metadata.\"Index\" i WHERE Name = ?", "HH_PP").FirstOrDefault() == null)
                Db.SQL("CREATE INDEX HH_PP ON HH (PP)");
            if (Db.SQL("SELECT i FROM Starcounter.Metadata.\"Index\" i WHERE Name = ?", "HH_Prn").FirstOrDefault() == null)
                Db.SQL("CREATE INDEX HH_Prn ON HH (Prn)");

            // FF:Fis
            if (Db.SQL("SELECT i FROM Starcounter.Metadata.\"Index\" i WHERE Name = ?", "FF_PP").FirstOrDefault() == null)
                Db.SQL("CREATE INDEX FF_PP ON FF (PP)");
            if (Db.SQL("SELECT i FROM Starcounter.Metadata.\"Index\" i WHERE Name = ?", "FF_HH").FirstOrDefault() == null)
                Db.SQL("CREATE INDEX FF_HH ON FF (HH)");
            if (Db.SQL("SELECT i FROM Starcounter.Metadata.\"Index\" i WHERE Name = ?", "FF_Trh").FirstOrDefault() == null)
                Db.SQL("CREATE INDEX FF_Trh ON FF (Trh DESC)");

        }

        public static string EncodeQueryString(string data)
        {
            string encodedData = String.Empty;
            try
            {
                byte[] data_byte = Encoding.UTF8.GetBytes(data);
                encodedData = System.Net.WebUtility.UrlEncode(Convert.ToBase64String(data_byte));
            }
            catch (Exception exception)
            {
                //Log exception
            }
            return encodedData;
        }

        public static string DecodeQueryString(string data)
        {
            string decodedData = String.Empty;
            try
            {
                //System.Net.WebUtility.UrlEncode
                //byte[] data_byte = Convert.FromBase64String(HttpUtility.UrlDecode(data));
                byte[] data_byte = Convert.FromBase64String(System.Net.WebUtility.UrlDecode(data));
                decodedData = Encoding.UTF8.GetString(data_byte);
            }
            catch (Exception exception)
            {
                //Log exception
            }
            return decodedData;
        }

        public static void SendMail(string eMail)
        {
            // gMail
            string body = $"<!DOCTYPE html><html><body><a href='http://masatenisi.online/bodved/confirmemail/{eMail}'>BODVED üyeliğiniz için tıklayınız</a></body></html>";

            MailMessage mail = new MailMessage();
            mail.To.Add("sener.demiral@gmail.com");
            mail.Subject = "Deneme";

            mail.From = new MailAddress("masatenisi.online@gmail.com", "BODVED");  // gMail
            mail.IsBodyHtml = true;
            mail.Body = body;


            SmtpClient smtp = new SmtpClient("smtp.gmail.com");   // gMail
            smtp.Credentials = new System.Net.NetworkCredential("masatenisi.online", "CanDilSen09");  // gMail
            smtp.EnableSsl = true;    // gMail
            smtp.Port = 587;


            //smtp.Send(mail);
            object userToken = null;
            smtp.SendAsync(mail, userToken);
        }


    }
}
