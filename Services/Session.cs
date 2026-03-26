namespace Borne.Services
{
    /// <summary>Singleton statique représentant la session de l'utilisateur connecté.</summary>
    public static class Session
    {
        public static string? Username { get; private set; }
        public static string? Role     { get; private set; }  // "admin" | "employe"

        public static bool IsAuthenticated => !string.IsNullOrWhiteSpace(Username);
        public static bool IsAdmin         => Role == "admin";
        public static bool IsEmploye       => Role == "employe";

        public static void SignIn(string username, string role)
        {
            Username = username;
            Role     = role;
        }

        public static void SignOut()
        {
            Username = null;
            Role     = null;
        }
    }
}
