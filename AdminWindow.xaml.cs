using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Borne.Models;
using Borne.Services;

namespace Borne.Views
{
    public partial class AdminWindow : Window
    {
        // Plat en cours d'édition (null = nouveau)
        private Plat? _platEdite = null;

        // Palette de couleurs proposées
        private static readonly (string Label, string Hex)[] _palette = new[]
        {
            ("Orange",  "#FF6B35"),
            ("Bleu",    "#2196F3"),
            ("Vert",    "#27AE60"),
            ("Rouge",   "#E74C3C"),
            ("Violet",  "#9B59B6"),
            ("Indigo",  "#3F51B5"),
            ("Rose",    "#E91E63"),
            ("Teal",    "#009688"),
        };

        // ══════════════════════════════════════════════════════════════════════
        public AdminWindow()
        {
            InitializeComponent();
            txtAdminUser.Text = $"Connecté : {Session.Username}";
            ChargerPlats();
            ChargerConfig();
            BuildSwatches();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  ONGLET 1 — Plats
        // ══════════════════════════════════════════════════════════════════════

        private void ChargerPlats()
        {
            gridPlats.ItemsSource = null;
            gridPlats.ItemsSource = PlatService.Instance.GetAll();
        }

        private void GridPlats_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (gridPlats.SelectedItem is Plat plat)
                ChargerPlatDansFormulaire(plat);
        }

        private void ChargerPlatDansFormulaire(Plat plat)
        {
            _platEdite           = plat;
            txtFormTitre.Text    = "Modifier le plat";
            txtNom.Text          = plat.Nom;
            txtDescription.Text  = plat.Description;
            txtPrix.Text         = plat.Prix.ToString("F2");
            cbCategorie.Text     = plat.Categorie;
        }

        private void BtnNouveauPlat_Click(object sender, RoutedEventArgs e)
        {
            _platEdite          = null;
            txtFormTitre.Text   = "Nouveau plat";
            txtNom.Text         = "";
            txtDescription.Text = "";
            txtPrix.Text        = "";
            cbCategorie.Text    = "";
            gridPlats.SelectedItem = null;
            txtNom.Focus();
        }

        private void BtnSauvegarder_Click(object sender, RoutedEventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(txtNom.Text))
            { ShowFeedback(txtFeedback, "⚠ Le nom est obligatoire.", false); return; }

            if (!decimal.TryParse(txtPrix.Text.Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out decimal prix) || prix < 0)
            { ShowFeedback(txtFeedback, "⚠ Prix invalide.", false); return; }

            if (string.IsNullOrWhiteSpace(cbCategorie.Text))
            { ShowFeedback(txtFeedback, "⚠ La catégorie est obligatoire.", false); return; }

            if (_platEdite == null)
            {
                // Création
                var nouveau = new Plat(0, txtNom.Text.Trim(),
                    txtDescription.Text.Trim(), prix, cbCategorie.Text.Trim());
                PlatService.Instance.Add(nouveau);
                ShowFeedback(txtFeedback, "✅ Plat ajouté.", true);
            }
            else
            {
                // Mise à jour
                _platEdite.Nom         = txtNom.Text.Trim();
                _platEdite.Description = txtDescription.Text.Trim();
                _platEdite.Prix        = prix;
                _platEdite.Categorie   = cbCategorie.Text.Trim();
                PlatService.Instance.Update(_platEdite);
                ShowFeedback(txtFeedback, "✅ Plat modifié.", true);
            }

            ChargerPlats();
        }

        private void BtnSupprimerPlat_Click(object sender, RoutedEventArgs e)
        {
            if (gridPlats.SelectedItem is not Plat plat)
            {
                MessageBox.Show("Sélectionnez un plat à supprimer.",
                    "Aucune sélection", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show($"Supprimer « {plat.Nom} » ?", "Confirmation",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                PlatService.Instance.Delete(plat.Id);
                _platEdite = null;
                BtnNouveauPlat_Click(this, new RoutedEventArgs());
                ChargerPlats();
                ShowFeedback(txtFeedback, "✅ Plat supprimé.", true);
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  ONGLET 2 — Configuration
        // ══════════════════════════════════════════════════════════════════════

        private void ChargerConfig()
        {
            var cfg = BorneConfig.Instance;
            cfgNomRestaurant.Text  = cfg.RestaurantName;
            cfgWelcomeMessage.Text = cfg.WelcomeMessage;
            chkSurPlace.IsChecked  = cfg.ShowSurPlace;
            chkAEmporter.IsChecked = cfg.ShowAEmporter;
            cfgAccentColor.Text    = cfg.AccentColorHex;

            // DB
            dbServer.Text   = cfg.DbServer;
            dbPort.Text     = cfg.DbPort;
            dbName.Text     = cfg.DbName;
            dbUser.Text     = cfg.DbUser;
            // Mot de passe non pré-rempli pour la sécurité
        }

        private void BuildSwatches()
        {
            panelSwatches.Children.Clear();
            foreach (var (label, hex) in _palette)
            {
                var btn = new Button
                {
                    Width   = 40,
                    Height  = 40,
                    Margin  = new Thickness(4),
                    Tag     = hex,
                    ToolTip = label,
                    Cursor  = System.Windows.Input.Cursors.Hand
                };

                // Template cercle coloré
                var tmpl = new ControlTemplate(typeof(Button));
                var fef  = new FrameworkElementFactory(typeof(Border));
                fef.SetValue(Border.BackgroundProperty, new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex)));
                fef.SetValue(Border.CornerRadiusProperty, new CornerRadius(20));
                tmpl.VisualTree = fef;
                btn.Template    = tmpl;

                btn.Click += (s, _) =>
                {
                    cfgAccentColor.Text = (s as Button)?.Tag?.ToString() ?? "";
                    UpdateColorPreview();
                };

                panelSwatches.Children.Add(btn);
            }
        }

        private void BtnSauvegarderConfig_Click(object sender, RoutedEventArgs e)
        {
            UpdateColorPreview();

            var cfg = BorneConfig.Instance;
            cfg.RestaurantName  = cfgNomRestaurant.Text.Trim();
            cfg.WelcomeMessage  = cfgWelcomeMessage.Text.Trim();
            cfg.ShowSurPlace    = chkSurPlace.IsChecked == true;
            cfg.ShowAEmporter   = chkAEmporter.IsChecked == true;
            cfg.AccentColorHex  = cfgAccentColor.Text.Trim();
            cfg.Save();

            // Application de la couleur d'accentuation en direct
            if (!string.IsNullOrWhiteSpace(cfg.AccentColorHex))
            {
                try
                {
                    var col   = (Color)ColorConverter.ConvertFromString(cfg.AccentColorHex);
                    var brush = new SolidColorBrush(col);
                    Application.Current.Resources["AccentBrush"] = brush;
                }
                catch { /* couleur invalide, on ignore */ }
            }

            ShowFeedback(txtConfigFeedback, "✅ Configuration sauvegardée.", true);
        }

        private void UpdateColorPreview()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(cfgAccentColor.Text))
                {
                    var col = (Color)ColorConverter.ConvertFromString(cfgAccentColor.Text);
                    previewColor.Background = new SolidColorBrush(col);
                }
            }
            catch { previewColor.Background = (Brush)FindResource("AccentBrush"); }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  ONGLET 3 — Base de données
        // ══════════════════════════════════════════════════════════════════════

        private async void BtnTesterDb_Click(object sender, RoutedEventArgs e)
        {
            var cfg = BuildDbConfig();
            ShowFeedback(txtDbFeedback, "⏳ Test en cours...", true);

            bool ok = await AuthService.TestConnectionAsync(
                cfg.Server, cfg.Port, cfg.Database, cfg.User, dbPassword.Password);

            ShowFeedback(txtDbFeedback,
                ok ? "✅ Connexion réussie !" : "❌ Connexion échouée. Vérifiez les paramètres.",
                ok);
        }

        private void BtnSauvegarderDb_Click(object sender, RoutedEventArgs e)
        {
            var cfg = BorneConfig.Instance;
            var db  = BuildDbConfig();
            cfg.DbServer = db.Server;
            cfg.DbPort   = db.Port;
            cfg.DbName   = db.Database;
            cfg.DbUser   = db.User;
            if (!string.IsNullOrWhiteSpace(dbPassword.Password))
                cfg.DbPassword = dbPassword.Password;
            cfg.Save();
            ShowFeedback(txtDbFeedback, "✅ Paramètres DB sauvegardés.", true);
        }

        private (string Server, string Port, string Database, string User) BuildDbConfig()
            => (dbServer.Text.Trim(), dbPort.Text.Trim(),
                dbName.Text.Trim(),   dbUser.Text.Trim());

        // ══════════════════════════════════════════════════════════════════════
        //  Helpers
        // ══════════════════════════════════════════════════════════════════════

        private static void ShowFeedback(TextBlock txt, string msg, bool success)
        {
            txt.Text       = msg;
            txt.Foreground = success
                ? new SolidColorBrush(Color.FromRgb(0x27, 0xAE, 0x60))   // vert
                : new SolidColorBrush(Color.FromRgb(0xE7, 0x4C, 0x3C));  // rouge
            txt.Visibility = Visibility.Visible;
        }
    }
}
