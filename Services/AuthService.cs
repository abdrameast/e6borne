using System;
using System.Threading.Tasks;
using MySqlConnector;
using Borne.Services;

namespace Borne.Services
{
    /// <summary>
    /// Gère l'authentification des employés/admins contre une BDD MySQL/MariaDB.
    /// Utilise MySqlConnector (compatible .NET 8, pas besoin d'ODBC).
    /// </summary>
    public class AuthService
    {
        // ── Construction de la chaîne de connexion ─────────────────────────────
        private static string BuildConnectionString(
            string server   = "",
            string port     = "",
            string database = "",
            string user     = "",
            string password = "")
        {
            var cfg = BorneConfig.Instance;
            return new MySqlConnectionStringBuilder
            {
                Server   = string.IsNullOrWhiteSpace(server)   ? cfg.DbServer   : server,
                Port     = uint.TryParse(
                               string.IsNullOrWhiteSpace(port) ? cfg.DbPort : port,
                               out uint p) ? p : 3306,
                Database = string.IsNullOrWhiteSpace(database) ? cfg.DbName     : database,
                UserID   = string.IsNullOrWhiteSpace(user)     ? cfg.DbUser     : user,
                Password = string.IsNullOrWhiteSpace(password) ? cfg.DbPassword : password,
                SslMode  = MySqlSslMode.Disabled,
                ConnectionTimeout = 5,
            }.ConnectionString;
        }

        // ── Login ──────────────────────────────────────────────────────────────
        /// <summary>
        /// Tente de connecter l'utilisateur.
        /// Essaie les tables <c>users</c> puis <c>utilisateurs</c> automatiquement.
        /// </summary>
        public bool TryLogin(string username, string password,
                             out string role, out string error)
        {
            role  = "";
            error = "";

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                error = "Identifiant et mot de passe requis.";
                return false;
            }

            try
            {
                using var cn = new MySqlConnection(BuildConnectionString());
                cn.Open();

                foreach (var table in new[] { "users", "utilisateurs" })
                {
                    if (TryLoginOnTable(cn, table, username, password, out role))
                        return true;
                }

                error = "Identifiants invalides.";
                return false;
            }
            catch (Exception ex)
            {
                error = $"Erreur de connexion BDD : {ex.Message}";
                return false;
            }
        }

        // ── Essai sur une table précise ────────────────────────────────────────
        private static bool TryLoginOnTable(MySqlConnection cn,
            string table, string username, string password, out string role)
        {
            role = "";

            // Tolère plusieurs noms de colonnes : username/login, password/mdp
            string sql = $@"
SELECT COALESCE(role, 'employe') AS role_out
FROM   `{table}`
WHERE  COALESCE(username, login, '') = @u
  AND  COALESCE(password, mdp,   '') = @p
LIMIT  1;";

            try
            {
                using var cmd = new MySqlCommand(sql, cn);
                cmd.Parameters.AddWithValue("@u", username);
                cmd.Parameters.AddWithValue("@p", password);

                using var rd = cmd.ExecuteReader();
                if (!rd.Read()) return false;

                role = rd.GetString(0).Trim().ToLowerInvariant();

                // Normalisation du rôle
                if (role != "admin") role = "employe";
                return true;
            }
            catch
            {
                // La table n'existe pas ou colonnes introuvables → on passe
                return false;
            }
        }

        // ── Test de connexion (utilisé depuis AdminWindow) ─────────────────────
        public static async Task<bool> TestConnectionAsync(
            string server, string port, string database,
            string user,   string password)
        {
            try
            {
                var cs = BuildConnectionString(server, port, database, user, password);
                await using var cn = new MySqlConnection(cs);
                await cn.OpenAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
