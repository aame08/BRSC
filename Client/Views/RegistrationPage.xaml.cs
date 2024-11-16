using Client.Data;
using Client.DTOs;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace Client.Views
{
    /// <summary>
    /// Логика взаимодействия для RegistrationPage.xaml
    /// </summary>
    public partial class RegistrationPage : Page
    {
        public static RegistrationPage registration;
        public RegistrationPage()
        {
            InitializeComponent();

            registration = this;
        }

        private void bExit_Click(object sender, RoutedEventArgs e)
        {
            registration.Visibility = Visibility.Hidden;
        }

        private bool IsValidUserName(string str)
        {
            if (string.IsNullOrEmpty(str))
                return false;
            if (str.Any(char.IsDigit))
                return false;
            if (str.Length > 100)
                return false;
            
            return true;
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
            if (IsValidUserName(tbName.Text) && IsValidEmail(tbEmail.Text) && IsPasswordValid(tbPassword.Text))
            {
                UserDTO newUser = new UserDTO
                {
                    NameUser = tbName.Text,
                    EmailUser = tbEmail.Text,
                    PasswordHash = tbPassword.Text
                };

                var (user, token) = await Api.Register(newUser);

                if (user != null && token != null)
                {
                    MessageBox.Show("Успешно.");

                    registration.Visibility = Visibility.Hidden;
                }
            }
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
    }
}
