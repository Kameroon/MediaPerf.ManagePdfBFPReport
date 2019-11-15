namespace MediaPerf.ManagerPdf.MailService.Implementations
{
    public static class Supplier 
    {        
        public const string SMTP_CREDENTIAL = "mediaperf-com.mail.protection.outlook.com";   //"smtp.gmail.com";
        public const string MAILPATTERN = @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                                         @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$";
    }
}
