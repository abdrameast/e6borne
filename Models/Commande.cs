using System.Collections.Generic;
using System.Linq;

namespace Borne.Models
{
    public class Commande
    {
        public int    Id          { get; set; }
        public string TypeCommande { get; set; } = string.Empty;

        /// <summary>Lignes du panier (plat + quantité)</summary>
        public List<LigneCommande> Lignes { get; set; } = new();

        /// <summary>Total calculé après appel à CalculerTotal()</summary>
        public decimal Total { get; private set; }

        public void CalculerTotal()
        {
            Total = Lignes.Sum(l => l.SousTotal);
        }

        /// <summary>Nombre total d'articles dans la commande</summary>
        public int NbArticles => Lignes.Sum(l => l.Quantite);
    }
}
