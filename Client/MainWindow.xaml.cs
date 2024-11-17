using Client.Data;
using Client.For_Token;
using Client.Views;
using System.Text.RegularExpressions;
using System.Windows;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void toggleShowPassword_Click(object sender, RoutedEventArgs e)
        {
            if (toggleShowPassword.IsChecked == true)
            {
                pbPassword.Visibility = Visibility.Collapsed;
                tbPassword.Visibility = Visibility.Visible;
                tbPassword.Text = pbPassword.Password;
                toggleShowPassword.Content = "👁️‍🗨️";
            }
            else
            {
                pbPassword.Visibility = Visibility.Visible;
                tbPassword.Visibility = Visibility.Collapsed;
                pbPassword.Password = tbPassword.Text;
                toggleShowPassword.Content = "👁";
            }
        }

        private void bExit_Click(object sender, RoutedEventArgs e)
        {
            tbEmail.Text = string.Empty;
            pbPassword.Password = string.Empty;
            tbPassword.Text = string.Empty;
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            Regex regex = new Regex(pattern);
            return regex.IsMatch(email);
        }
        private bool IsPasswordValid(string password)
        {
            if (string.IsNullOrEmpty(password))
                return false;
            if (password.Length > 60)
                return false;

            return true;
        }
        private async void bEnter_Click(object sender, RoutedEventArgs e)
        {
            var id_user = await Api.GetUserIdByEmail(tbEmail.Text);
            if (id_user != 0)
            {
                string token = TokenManager.GetToken(id_user);
                if (token != null)
                {
                    bool isTokenValid = await Api.VerifyToken(token);
                    if (isTokenValid)
                    {
                        MessageBox.Show("С возвращением.");
                        NavigatePage(token);
                        tbEmail.Text = string.Empty;
                        pbPassword.Password = string.Empty;
                        tbPassword.Text = string.Empty;
                        return;
                    }
                    else { TokenManager.DeleteToken(id_user); }
                }
            }
            if (IsValidEmail(tbEmail.Text) && IsPasswordValid(pbPassword.Password))
            {
                var token = await Api.Login(tbEmail.Text, pbPassword.Password);
                if (token != null)
                {
                    MessageBox.Show("Успешная авторизация.");
                    NavigatePage(token);
                    tbEmail.Text = string.Empty;
                    pbPassword.Password = string.Empty;
                    tbPassword.Text = string.Empty;
                }
                else { MessageBox.Show("Ошибка авторизации."); }
            }
            else { MessageBox.Show("Неверный формат данных."); }
        }
        private void NavigatePage(string token)
        {
            var role = TokenManager.GetRoleByToken(token);
            switch (role)
            {
                case "Admin":
                    frame.NavigationService.Navigate(new AdminPage(token));
                    break;
                case "Manager":
                    frame.NavigationService.Navigate(new ManagerPage(token));
                    break;
                case "User":
                    frame.NavigationService.Navigate(new UserPage(token));
                    break;
                default:
                    MessageBox.Show("Ошибка.");
                    break;
            }
        }

        private void bReg_Click(object sender, RoutedEventArgs e)
        {
            frame.NavigationService.Navigate(new RegistrationPage());
        }
    }
}