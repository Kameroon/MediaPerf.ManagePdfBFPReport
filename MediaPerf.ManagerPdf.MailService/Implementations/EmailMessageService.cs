using MediaPerf.ManagerPdf.MailService.Contracts;

namespace MediaPerf.ManagerPdf.MailService.Implementations
{
    public class EmailMessageService : IEmailMessageService
    {
        public string ToEmail { get; set; }
        public string MailBody { get; set; }
        public string Suject { get; set; }
        public string FilePath { get; set; }
        public string Bcc { get; set; }
        public string Cc { get; set; }
        public bool IsPreviewMail { get; set; }
        public string SenderEmail { get; set; }
        public string AdminEmail { get; set; }
    }
}
