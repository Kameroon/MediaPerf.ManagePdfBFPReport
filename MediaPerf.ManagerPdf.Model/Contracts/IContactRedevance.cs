namespace MediaPerf.ManagerPdf.Model.Contracts
{
    public interface IContactRedevance
    {
        string Contact { get; set; }
        string EMail { get; set; }
        long Fk_Crm { get; set; }
        long HIDE_CompteUrbaSiId { get; set; }
        long HIDE_Id { get; set; }
        string Titre { get; set; }
        bool IsRdvcMailPv { get; set; }
    }
}