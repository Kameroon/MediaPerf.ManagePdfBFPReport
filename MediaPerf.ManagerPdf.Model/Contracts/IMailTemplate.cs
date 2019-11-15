
namespace MediaPerf.ManagerPdf.Model.Contracts
{
    public interface IMailTemplate
    {
        int Id { get; set; }
        string Nom { get; set; }
        string Texte { get; set; }
        string Destinataires { get; set; }
        string Application { get; set; }
        string Objet { get; set; }
    }
}
