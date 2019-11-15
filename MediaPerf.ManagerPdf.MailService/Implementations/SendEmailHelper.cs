using MediaPerf.ManagerPdf.MailService.Contracts;
using NLog;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MediaPerf.ManagerPdf.MailService.Implementations
{
    public class SendEmailHelper
    {
        public static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// --  --
        /// </summary>
        /// <param name="emailMessage"></param>
        /// <returns></returns>
        public static bool SendEmail(IEmailMessageService emailMessage, IDictionary<string, string> contactDictionnary, string dtSessionRfr)
        {
            bool result = false;
            try
            {
                using (MailMessage mailMessage = new MailMessage())
                {
                    if (!string.IsNullOrWhiteSpace(emailMessage.SenderEmail))
                    {
                        mailMessage.From = new MailAddress(emailMessage.SenderEmail);

                        IList<string> contactMailAdress = new List<string>();
                        foreach (var mailAdress in contactDictionnary)
                        {
                            //mailMessage.To.Add(mailAdress.Value);
                            //contactMailAdress.Add(mailAdress.Value);
                        }

                        mailMessage.To.Add(emailMessage.AdminEmail);

                        string date = Regex.Split(dtSessionRfr, " ")[0];
                        string objectMessage = emailMessage.Suject.Replace("[DateSession]", date);
                        mailMessage.Subject = objectMessage;

                        //string body = null;
                        mailMessage.BodyEncoding = Encoding.Default;
                        mailMessage.IsBodyHtml = true;

                        //mailMessage.Body = MailConsolidateHelper.BuilBody(body, emailMessage.MailBody, emailMessage.Suject, currentMonth, dtSessionRfr);

                        mailMessage.Body = emailMessage.MailBody.Replace("[DateSession]", date);

                        #region -- Manage BCC and CC mail --
                        //if (!string.IsNullOrWhiteSpace(emailMessage.Bcc))
                        //    mailMessage.Bcc.Add(emailMessage.Bcc); 

                        //if (!string.IsNullOrWhiteSpace(emailMessage.Cc))
                        //    mailMessage.CC.Add(emailMessage.Cc); 
                        #endregion

                        Attachment attachment;
                        attachment = new Attachment(emailMessage.FilePath);
                        mailMessage.Attachments.Add(attachment);


                        mailMessage.From = new MailAddress($"{Environment.UserName}@mediaperf.com");

                        SmtpClient smtpClient = new SmtpClient(Supplier.SMTP_CREDENTIAL);
                                                                    
                        smtpClient.Send(mailMessage);

                        //if (emailMessage.IsPreviewMail)
                        //{
                        //    MailConsolidateHelper.SendMailPreview(
                        //        emailMessage.AdminEmail,
                        //        mailMessage,
                        //        contactMailAdress);
                        //}

                        result = true;
                    }
                    else
                    {
                        _logger.Error($"==> L'adresse email de l'administrateur est obligatoire.");
                        result = false;
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
                _logger.Error($"==> Une erreur s'est produite pendant l'envoie de mail. [SendEmailHelper.SendMail]"
                    , exception.ToString());
                throw;
            }

            return result;
        }
    }
}
