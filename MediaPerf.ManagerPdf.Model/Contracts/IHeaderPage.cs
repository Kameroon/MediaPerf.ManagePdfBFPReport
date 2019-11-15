using System;

namespace MediaPerf.ManagerPdf.Model.Contracts
{
    public interface IHeaderPage
    {
        string AdresseMpf { get; set; }
        string AdressePrestataire { get; set; }
        string BfpParam1 { get; set; }
        string BfpParam2 { get; set; }
        string BfpParam3 { get; set; }
        string BfpParam4 { get; set; }
        string BfpParam7 { get; set; }
        string DateBfp { get; set; }
        string DateLivraison { get; set; }
        string Destinataire { get; set; }
        string Duplicata { get; set; }
        long IdBFP { get; set; }
        string Mail { get; set; }
        long NbCmp { get; set; }
        long NbPv { get; set; }
        string NomComplet { get; set; }
        Byte[] PathLogoEntete { get; set; }
        string Prestataire { get; set; }
        string Telephone { get; set; }
    }
}