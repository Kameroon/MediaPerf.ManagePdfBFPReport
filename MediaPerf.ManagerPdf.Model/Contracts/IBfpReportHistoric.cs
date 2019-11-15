namespace MediaPerf.ManagerPdf.Model.Contracts
{
    public interface IBfpReportHistoric
    {
        int Id { get; set; }
        long Fk_Bfp { get; set; }
        long Fk_S_StTarif { get; set; }
        int Fk_S_ModeleEditionBfp { get; set; }
        long IdPv { get; set; }
        bool IsDuplicata { get; set; }
        string SpReportBFPPdfTemplate { get; set; }
        string SpReportBFPRdvcPv { get; set; }
        int UserCreation { get; set; }
        string DateCreation { get; set; }
        string DateUpdate { get; set; }
        string DtSessionRfr { get; set; }
        bool IsSend { get; set; }
    }
}
