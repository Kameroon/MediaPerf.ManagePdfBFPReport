using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace MediaPerf.ManagerPdf.MailService.Implementations
{
    public static class MailConsolidateHelper
    {
        /// <summary>
        /// -- Build the body components of the mail  --
        /// </summary>
        /// <param name="body"></param>
        /// <param name="htmlTemplate"></param>
        /// <param name="currentMonth"></param>
        /// <param name="toEmail"></param>
        /// <returns></returns>
        public static string BuilBody(string body, string htmlTemplate,
            string suject, string currentMonth, string dtSessionRfr)
        {
            body = htmlTemplate.Replace("[DateSession]", Regex.Split(dtSessionRfr, " ")[0]);  //.ToShortDateString() , StringSplitOptions.None
            body += "\n\rCeci est un message automatique.";

            return body;
        }

        /// <summary>
        /// -- Check the CC and BCC mail --
        /// </summary>
        /// <param name="adminEmail"></param>
        /// <param name="mailMessage"></param>
        public static void SendMailPreview(string adminEmail, MailMessage mailMessage, IList<string> contactMailAdress)
        {
            Console.WriteLine("Destiné à : " + mailMessage.To.FirstOrDefault());
            mailMessage.Subject += " - To : " + mailMessage.To.FirstOrDefault();
            mailMessage.To.Clear();
            mailMessage.To.Add(adminEmail);

            foreach (string mailAddress in contactMailAdress)
            {
                //mailMessage.To.Add(adminEmail);
                mailMessage.CC.Add(mailAddress);
                Console.WriteLine($"Destiné à : { mailAddress }");
            }
            mailMessage.CC?.Clear();
            mailMessage.Bcc?.Clear();

            Console.WriteLine($"\r\n => ======================================================== <=");
        }
    }
}
