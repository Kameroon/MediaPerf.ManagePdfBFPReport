using MediaPerf.ManagerPdf.Model.Contracts;

namespace MediaPerf.ManagerPdf.Model.Implemenations
{
    public class RoyaltyFee : IRoyaltyFee
    {
        public long IdPv { get; set; }
        public string Enseigne { get; set; }
        public string Commune { get; set; }
        public string Produit { get; set; }
        public string TypeProduit { get; set; }
        public string Dp { get; set; }
        public long IdCmp { get; set; }
        public long IdBFP { get; set; }
        public long Fk_S_TypeProduit { get; set; }
        public long MontantRdvcHT { get; set; }
        public string Campagne { get; set; }
        public string DateDebut { get; set; }
        public string DateFin { get; set; }   
        public string Details { get; set; }
    }
}
