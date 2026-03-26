namespace Borne.Models
{
    public class Plat
    {
        public int     Id          { get; set; }
        public string  Nom         { get; set; }
        public string  Description { get; set; }
        public decimal Prix        { get; set; }
        public string  Categorie   { get; set; }

        // Constructeur principal
        public Plat(int id, string nom, string description, decimal prix, string categorie)
        {
            Id          = id;
            Nom         = nom;
            Description = description;
            Prix        = prix;
            Categorie   = categorie;
        }

        // Constructeur sans argument requis par certains sérialiseurs
        public Plat()
        {
            Nom         = string.Empty;
            Description = string.Empty;
            Categorie   = string.Empty;
        }
    }
}
