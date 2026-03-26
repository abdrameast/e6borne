using System;
using System.IO;
using System.Text.Json;

namespace Borne.Services
{
    /// <summary>
    /// Configuration globale de la borne.
    /// Persiste dans AppData/Borne/config.json.
    /// </summary>
    public class BorneConfig
    {
        // ── Singleton ──────────────────────────────────────────────────────────
        private static BorneConfig? _instance;
        public  static BorneConfig Instance => _instance ??= new BorneConfig();

        private static readonly string ConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Borne", "config.json");

        // ── Propriétés persistées ──────────────────────────────────────────────

        /// Nom affiché dans la barre et sur l'écran d'accueil
        public string RestaurantName  { get; set; } = "Mon Restaurant";

        /// Message affiché sous le nom d'accueil
        public string WelcomeMessage  { get; set; } = "Commandez en quelques secondes";

        /// Active le bouton « Sur place »
        public bool ShowSurPlace      { get; set; } = true;

        /// Active le bouton « À emporter »
        public bool ShowAEmporter     { get; set; } = true;

        /// Couleur d'accentuation en hexadécimal (#RRGGBB)
        public string AccentColorHex  { get; set; } = "#FF6B35";

        // ── Connexion BDD ──────────────────────────────────────────────────────
        public string DbServer        { get; set; } = "localhost";
        public string DbPort          { get; set; } = "3306";
        public string DbName          { get; set; } = "borne";
        public string DbUser          { get; set; } = "root";
        public string DbPassword      { get; set; } = "";

        // ── Load / Save ────────────────────────────────────────────────────────

        public void Load()
        {
            if (!File.Exists(ConfigPath)) return;

            try
            {
                string json = File.ReadAllText(ConfigPath);
                var loaded  = JsonSerializer.Deserialize<BorneConfig>(json);
                if (loaded == null) return;

                RestaurantName = loaded.RestaurantName;
                WelcomeMessage = loaded.WelcomeMessage;
                ShowSurPlace   = loaded.ShowSurPlace;
                ShowAEmporter  = loaded.ShowAEmporter;
                AccentColorHex = loaded.AccentColorHex;
                DbServer       = loaded.DbServer;
                DbPort         = loaded.DbPort;
                DbName         = loaded.DbName;
                DbUser         = loaded.DbUser;
                DbPassword     = loaded.DbPassword;
            }
            catch { /* fichier invalide → on garde les valeurs par défaut */ }
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);
                File.WriteAllText(ConfigPath,
                    JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch { /* non bloquant */ }
        }
    }
}
