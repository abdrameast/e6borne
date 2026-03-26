using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Borne.Models;
using Borne.Services;

namespace Borne.Views
{
    public partial class CommandeView : UserControl
    {
        // ── Données ────────────────────────────────────────────────────────────
        public Commande CommandeEnCours { get; private set; } = new();

        private List<Plat>          _tousLesPlats  = new();
        private List<Plat>          _platsAffichés = new();
        private string              _categorieActive = "Tous";
        private ObservableCollection<LigneCommande> _lignes = new();

        // ── Événement retour ───────────────────────────────────────────────────
        public event EventHandler? RetourAuMenu;

        // ══════════════════════════════════════════════════════════════════════
        public CommandeView()
        {
            InitializeComponent();
            _tousLesPlats  = PlatService.Instance.GetAll();
            _platsAffichés = _tousLesPlats;
            itemsPanier.ItemsSource = _lignes;
            BuildCategoryTabs();
            AfficherPlats();
        }

        // ── Initialisation ─────────────────────────────────────────────────────
        private void BuildCategoryTabs()
        {
            panelCategories.Children.Clear();

            var cats = new[] { "Tous" }
                .Concat(_tousLesPlats.Select(p => p.Categorie).Distinct().OrderBy(c => c))
                .ToArray();

            foreach (var cat in cats)
            {
                var btn = new Button
                {
                    Content = cat,
                    FontFamily = new FontFamily("Segoe UI"),
                    FontSize   = 13,
                    FontWeight = cat == _categorieActive ? FontWeights.Bold : FontWeights.Normal,
                    Padding    = new Thickness(16, 0, 16, 0),
                    Height     = 36,
                    Margin     = new Thickness(4, 0, 4, 0),
                    Cursor     = System.Windows.Input.Cursors.Hand,
                    Background = cat == _categorieActive
                                    ? (Brush)FindResource("AccentBrush")
                                    : Brushes.Transparent,
                    Foreground = cat == _categorieActive
                                    ? Brushes.White
                                    : (Brush)FindResource("TextMutedBrush"),
                    BorderThickness = new Thickness(0),
                    Tag = cat
                };

                // Template simple avec CornerRadius
                var tmpl = new ControlTemplate(typeof(Button));
                var factory = new FrameworkElementFactory(typeof(Border));
                factory.SetBinding(Border.BackgroundProperty,
                    new System.Windows.Data.Binding("Background") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
                factory.SetValue(Border.CornerRadiusProperty, new CornerRadius(18));
                factory.SetBinding(Border.PaddingProperty,
                    new System.Windows.Data.Binding("Padding") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
                var cp = new FrameworkElementFactory(typeof(ContentPresenter));
                cp.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                cp.SetValue(ContentPresenter.VerticalAlignmentProperty,   VerticalAlignment.Center);
                factory.AppendChild(cp);
                tmpl.VisualTree = factory;
                btn.Template = tmpl;

                btn.Click += (s, _) =>
                {
                    _categorieActive = (s as Button)?.Tag?.ToString() ?? "Tous";
                    BuildCategoryTabs();
                    AfficherPlats();
                };

                panelCategories.Children.Add(btn);
            }
        }

        public void SetTypeCommande(string type)
        {
            CommandeEnCours = new Commande { TypeCommande = type };
            _lignes.Clear();
            MettreAJourResume();
            txtTypeCommande.Text = $"Commande — {type}";

            // Recharge les plats (l'admin peut en avoir ajouté)
            _tousLesPlats  = PlatService.Instance.GetAll();
            _categorieActive = "Tous";
            BuildCategoryTabs();
            AfficherPlats();
        }

        // ── Filtrage ───────────────────────────────────────────────────────────
        private void AfficherPlats()
        {
            string recherche = txtRecherche?.Text?.ToLowerInvariant() ?? "";

            _platsAffichés = _tousLesPlats
                .Where(p => (_categorieActive == "Tous" || p.Categorie == _categorieActive)
                         && (recherche == "" ||
                             p.Nom.ToLowerInvariant().Contains(recherche) ||
                             p.Description.ToLowerInvariant().Contains(recherche)))
                .ToList();

            itemsPlats.ItemsSource = null;
            itemsPlats.ItemsSource = _platsAffichés;
        }

        private void TxtRecherche_TextChanged(object sender, TextChangedEventArgs e)
        {
            AfficherPlats();
        }

        // ── Actions panier ─────────────────────────────────────────────────────
        private void AjouterPlat_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is not Plat plat) return;

            var ligne = _lignes.FirstOrDefault(l => l.Plat.Id == plat.Id);
            if (ligne != null)
                ligne.Quantite++;
            else
                _lignes.Add(new LigneCommande(plat));

            RefreshLignes();
            MettreAJourResume();
        }

        private void IncrementerQte_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is not LigneCommande ligne) return;
            ligne.Quantite++;
            RefreshLignes();
            MettreAJourResume();
        }

        private void DecrementerQte_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is not LigneCommande ligne) return;
            if (ligne.Quantite <= 1)
                _lignes.Remove(ligne);
            else
                ligne.Quantite--;
            RefreshLignes();
            MettreAJourResume();
        }

        private void SupprimerLigne_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is not LigneCommande ligne) return;
            _lignes.Remove(ligne);
            RefreshLignes();
            MettreAJourResume();
        }

        private void ViderPanier_Click(object sender, RoutedEventArgs e)
        {
            if (_lignes.Count == 0) return;
            if (MessageBox.Show("Vider le panier ?", "Confirmation",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _lignes.Clear();
                MettreAJourResume();
            }
        }

        // ── Forcer le refresh de l'ItemsControl (ObservableCollection ne notifie
        //    pas sur les propriétés internes des items sans INotifyPropertyChanged
        //    complet, donc on fait un reset simple) ──────────────────────────────
        private void RefreshLignes()
        {
            var snap = _lignes.ToList();
            itemsPanier.ItemsSource = null;
            itemsPanier.ItemsSource = snap;
        }

        private void MettreAJourResume()
        {
            decimal total = _lignes.Sum(l => l.SousTotal);
            txtTotal.Text = $"{total:C}";
            txtNbArticles.Text = _lignes.Count == 0
                ? "Panier vide"
                : $"{_lignes.Sum(l => l.Quantite)} article(s)";
        }

        // ── Retour ─────────────────────────────────────────────────────────────
        private void Retour_Click(object sender, RoutedEventArgs e)
        {
            if (_lignes.Count > 0)
            {
                if (MessageBox.Show(
                    "Retourner au menu principal ?\nVotre panier sera perdu.",
                    "Confirmation", MessageBoxButton.YesNo,
                    MessageBoxImage.Question) != MessageBoxResult.Yes)
                    return;
            }
            _lignes.Clear();
            RetourAuMenu?.Invoke(this, EventArgs.Empty);
        }

        // ── Validation ─────────────────────────────────────────────────────────
        private void Valider_Click(object sender, RoutedEventArgs e)
        {
            if (_lignes.Count == 0)
            {
                MessageBox.Show("Votre panier est vide.",
                    "Commande vide", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Synchronise le modèle Commande
            CommandeEnCours.Lignes = _lignes.ToList();
            CommandeEnCours.CalculerTotal();

            // Récap
            string msg = $"✅  Commande {CommandeEnCours.TypeCommande} validée !\n\n";
            foreach (var l in _lignes)
                msg += $"  • {l.Plat.Nom}  x{l.Quantite}  →  {l.SousTotal:C}\n";
            msg += $"\nTOTAL : {CommandeEnCours.Total:C}";

            MessageBox.Show(msg, "Commande confirmée",
                MessageBoxButton.OK, MessageBoxImage.Information);

            // Réinitialisation
            _lignes.Clear();
            CommandeEnCours = new Commande { TypeCommande = CommandeEnCours.TypeCommande };
            MettreAJourResume();
        }
    }
}
