namespace MediaPerf.ManagerPdf.Model.Contracts
{
    public interface IRoyaltyFee : IBaseEntity
    {
        string Campagne { get; set; }
        string Commune { get; set; }
        string DateDebut { get; set; }
        string DateFin { get; set; }
        string Dp { get; set; }
        string Enseigne { get; set; }
        string TypeProduit { get; set; }
        long Fk_S_TypeProduit { get; set; }
        long IdCmp { get; set; }
        long IdPv { get; set; }
        long MontantRdvcHT { get; set; }
        string Produit { get; set; }
        string Details { get; set; }
    }
}

