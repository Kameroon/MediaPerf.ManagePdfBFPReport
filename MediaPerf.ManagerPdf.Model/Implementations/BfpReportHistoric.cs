using MediaPerf.ManagerPdf.Model.Contracts;

namespace MediaPerf.ManagerPdf.Model.Implementations
{
    public class BfpReportHistoric : IBfpReportHistoric
    {
        public int Id { get; set; }
        public long Fk_Bfp { get; set; }
        public long Fk_S_StTarif { get; set; }
        public int Fk_S_ModeleEditionBfp { get; set; }
        public long IdPv { get; set; }
        public bool IsDuplicata { get; set; }
        public string SpReportBFPPdfTemplate { get; set; }
        public string SpReportBFPRdvcPv { get; set; }
        public int UserCreation { get; set; }
        public string DateCreation { get; set; }
        public string DateUpdate { get; set; }
        public string DtSessionRfr { get; set; }
        public bool IsSend { get; set; }
    }
}
