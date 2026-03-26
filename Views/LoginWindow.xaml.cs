using System.Windows;
using System.Windows.Input;
using Borne.Services;

namespace Borne.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            Loaded += (_, _) => txtUsername.Focus();
        }

        // ── Connexion ──────────────────────────────────────────────────────────
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            AttemptLogin();
        }

        private void Field_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                AttemptLogin();
        }

        private void AttemptLogin()
        {
            HideError();

            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ShowError("Veuillez remplir tous les champs.");
                return;
            }

            var auth = new AuthService();
            bool ok = auth.TryLogin(username, password, out string role, out string error);

            if (ok)
            {
                Session.SignIn(username, role);
                DialogResult = true;
                Close();
            }
            else
            {
                ShowError(error);
                txtPassword.Clear();
                txtPassword.Focus();
            }
        }

        private void ShowError(string message)
        {
            txtError.Text      = message;
            borderError.Visibility = Visibility.Visible;
        }

        private void HideError()
        {
            borderError.Visibility = Visibility.Collapsed;
        }
    }
}
