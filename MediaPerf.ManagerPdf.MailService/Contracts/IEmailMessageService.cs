namespace MediaPerf.ManagerPdf.MailService.Contracts
{
    public interface IEmailMessageService
    {
        string ToEmail { get; set; }
        string MailBody { get; set; }
        string Suject { get; set; }
        string FilePath { get; set; }
        string Bcc { get; set; }
        string Cc { get; set; }
        string SenderEmail { get; set; }
        string AdminEmail { get; set; }
        bool IsPreviewMail { get; set; }
    }
}
