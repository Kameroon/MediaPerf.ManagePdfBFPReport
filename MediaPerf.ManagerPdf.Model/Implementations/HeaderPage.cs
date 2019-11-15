using MediaPerf.ManagerPdf.Model.Contracts;
using System;

namespace MediaPerf.ManagerPdf.Model.Implemenations
{
    public class HeaderPage : IHeaderPage 
    {
        public long IdBFP { get; set; }
        public long NbCmp { get; set; }
        public long NbPv { get; set; }
        public string AdresseMpf { get; set; }
        public string AdressePrestataire { get; set; }
        public string BfpParam1 { get; set; }
        public string BfpParam2 { get; set; }
        public string BfpParam3 { get; set; }
        public string BfpParam4 { get; set; }
        public string BfpParam7 { get; set; }
        public string Mail { get; set; } // --- Pas dans le header
        public string NomComplet { get; set; }  // --- Pas dans le header
        public string Telephone { get; set; }  // --- Pas dans le header
        public string Duplicata { get; set; }
        public string Destinataire { get; set; }
        public string Prestataire { get; set; }
        public Byte[] PathLogoEntete { get; set; }
        public string DateBfp { get; set; }
        public string DateLivraison { get; set; }
    }
}
