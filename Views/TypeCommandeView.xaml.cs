using System;
using System.Windows;
using System.Windows.Controls;
using Borne.Models;
using Borne.Services;

namespace Borne.Views
{
    public partial class TypeCommandeView : UserControl
    {
        public Commande CommandeEnCours { get; private set; } = new();

        public event EventHandler? TypeSelectionne;

        public TypeCommandeView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Met à jour le texte dynamique depuis la config
            var cfg = BorneConfig.Instance;
            txtNomRestaurant.Text = cfg.RestaurantName;
            txtMessage.Text       = cfg.WelcomeMessage;
        }

        private void SurPlace_Click(object sender, RoutedEventArgs e)
        {
            CommandeEnCours = new Commande { TypeCommande = "Sur place" };
            TypeSelectionne?.Invoke(this, EventArgs.Empty);
        }

        private void Emporter_Click(object sender, RoutedEventArgs e)
        {
            CommandeEnCours = new Commande { TypeCommande = "À emporter" };
            TypeSelectionne?.Invoke(this, EventArgs.Empty);
        }
    }
}
