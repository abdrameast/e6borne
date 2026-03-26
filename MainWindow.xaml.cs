using System.Windows;
using Borne.Services;
using Borne.Views;

namespace Borne
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Abonnement aux événements inter-vues
            typeCommandeView.TypeSelectionne += TypeCommandeView_TypeSelectionne;
            commandeView.RetourAuMenu        += CommandeView_RetourAuMenu;

            // Affiche le nom de l'utilisateur connecté
            txtUsername.Text = Session.Username ?? "";

            // Affiche le bouton admin uniquement pour les admins
            btnAdmin.Visibility = Session.IsAdmin ? Visibility.Visible : Visibility.Collapsed;

            // Nom du restaurant depuis la config
            txtRestaurantName.Text = BorneConfig.Instance.RestaurantName;
        }

        // ── Sélection du type de commande ──────────────────────────────────────
        private void TypeCommandeView_TypeSelectionne(object sender, System.EventArgs e)
        {
            string type = typeCommandeView.CommandeEnCours.TypeCommande;
            commandeView.SetTypeCommande(type);
            commandeView.Visibility   = Visibility.Visible;
            typeCommandeView.Visibility = Visibility.Collapsed;
        }

        // ── Retour au choix du type ─────────────────────────────────────────────
        private void CommandeView_RetourAuMenu(object sender, System.EventArgs e)
        {
            typeCommandeView.Visibility = Visibility.Visible;
            commandeView.Visibility     = Visibility.Collapsed;
        }

        // ── Ouvrir le panel admin ───────────────────────────────────────────────
        private void BtnAdmin_Click(object sender, RoutedEventArgs e)
        {
            var admin = new AdminWindow();
            admin.Owner = this;
            admin.ShowDialog();

            // Rafraîchit le nom si modifié par l'admin
            txtRestaurantName.Text = BorneConfig.Instance.RestaurantName;
        }
    }
}
