using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SignedMail
{
    class Program
    {
        static void Main(string[] args)
        {
            //send5();

            mailer("kasun@zorrosign.com", "zorrosign@zorrosign.com", "zorrosign", "", "Signed Mail", File.ReadAllText(@"D:\MailBody.txt"), "", "");
        }

        private static void mailer(string toaddress, string fromaddress, string fromaddress_disp, string relays, string mailsubject, string bodytext, string ccman, string cccct)
        {
            string certname = "";

            MailAddress from = new MailAddress(fromaddress, fromaddress_disp);
            MailAddress to = new MailAddress(toaddress);

            // MailAddress cc_man = new MailAddress(ccman);
            // MailAddress cc_cct = new MailAddress(cccct);

            MailMessage message = new MailMessage(from, to);
            message.To.Add("kasun.jm@outlook.com");
            message.To.Add("kasun.jm1@gmail.com");
            message.To.Add("kasun@zorrosign.com");

            message.Subject = mailsubject + DateTime.Now.ToShortTimeString();
            message.IsBodyHtml = true;
            string body = "Content-Type: text/html; charset=iso-8859-1 \r\nContent-Transfer-Encoding: 8bit\r\n\r\n" + bodytext;
            byte[] messageData = Encoding.ASCII.GetBytes(body);
            ContentInfo content = new ContentInfo(messageData);

            SignedCms Cms = new SignedCms(content);// new ContentInfo(messageData));
            X509Store store = new X509Store(StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);

            RSACryptoServiceProvider csp = null;
            X509Certificate2Collection certCollection = store.Certificates;
            X509Certificate2 cert = null;
            foreach (X509Certificate2 c in certCollection)
            {
                if ((c.Subject.Contains("zorrosign@zorrosign.com")) && (c.FriendlyName.Contains("ZorroCertificate")))
                {
                    cert = c;
                    break;
                }
            }

            if (cert != null)
            {
                csp = (RSACryptoServiceProvider)cert.PrivateKey;
            }
            else
            {
                throw new Exception("Valid certificate was not found");
            }

            CmsSigner Signer = new CmsSigner(SubjectIdentifierType.IssuerAndSerialNumber, cert);
            Signer.IncludeOption = X509IncludeOption.ExcludeRoot;

            // Encrypt with SHA384
            Signer.DigestAlgorithm = new Oid("2.16.840.1.101.3.4.2.2");
            Signer.SignedAttributes.Add(new Pkcs9SigningTime());
            Cms.ComputeSignature(Signer, false);

            byte[] SignedBytes = Cms.Encode();

            MemoryStream signedStream = new MemoryStream(SignedBytes);
            AlternateView signedView = new AlternateView(signedStream, "application/pkcs7-mime; smime-type=signed-data; name=sig.p7m");
            message.AlternateViews.Add(signedView);
            // message.Body = bodytext;
      


            // Command line argument must the the SMTP host.
            SmtpClient client = new SmtpClient("email-smtp.us-east-1.amazonaws.com", 587);
            client.Credentials = new System.Net.NetworkCredential("AKIAIJECKTRUVNJSRAWQ", "AsFPAWZYJgJKNL//KMSCTThon668Ew43V6MZnQatXFZm");
            client.EnableSsl = true;

            store.Close();

            try
            {
                client.Send(message);
            }
            catch (Exception ex)
            {
                //exception
            }
        }
    }
}
