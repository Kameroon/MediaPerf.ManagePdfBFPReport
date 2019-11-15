
using MediaPerf.ManagerPdf.Model.Contracts;

namespace MediaPerf.ManagerPdf.Model.Implemenations
{
    public class ContactRedevance : IContactRedevance
    {
        public long HIDE_Id { get; set; }
        public long Fk_Crm { get; set; }
        public long HIDE_CompteUrbaSiId { get; set; }
        public string Contact { get; set; }
        public string EMail { get; set; }
        public string Titre { get; set; }
        public bool IsRdvcMailPv { get; set; }
    }
}
