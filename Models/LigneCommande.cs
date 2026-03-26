namespace Borne.Models
{
    /// <summary>
    /// Représente une ligne dans le panier :
    /// un plat + une quantité + le calcul du sous-total.
    /// </summary>
    public class LigneCommande
    {
        public Plat Plat      { get; set; }
        public int  Quantite  { get; set; }

        public decimal SousTotal => Plat.Prix * Quantite;

        public LigneCommande(Plat plat, int quantite = 1)
        {
            Plat     = plat;
            Quantite = quantite;
        }
    }
}
