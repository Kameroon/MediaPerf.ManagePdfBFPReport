using MediaPerf.ManagerPdf.Model.Contracts;

namespace MediaPerf.ManagerPdf.Model.Implementations
{
    public class MailTemplate : IMailTemplate
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public string Texte { get; set; }
        public string Destinataires { get; set; }
        public string Application { get; set; }
        public string Objet { get; set; }
    }
}
