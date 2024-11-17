using Client.Data;
using Client.DTOs;
using Client.For_Token;
using Client.Models;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace Client.Views
{
    /// <summary>
    /// Логика взаимодействия для UserPage.xaml
    /// </summary>
    public partial class UserPage : Page
    {
        public string userToken;
        public int id_user;
        public UserPage(string token)
        {
            InitializeComponent();

            userToken = token;
            ViewUser(userToken);
        }
        private async Task<User> ViewUser(string token)
        {
            id_user = TokenManager.GetIdUserByToken(userToken);
            if (id_user != 0)
            {
                var user = await Api.GetUser(id_user, token);
                if (user != null)
                {
                    tbName.Text = user.NameUser;
                    tbEmail.Text = user.EmailUser;
                }
            }
            return null;
        }

        private void bExit_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }

        private void toggleShowPassword_Click(object sender, RoutedEventArgs e)
        {
            if (toggleShowPassword.IsChecked == true)
            {
                pbPassword.Visibility = Visibility.Visible;
                tbPassword.Visibility = Visibility.Collapsed;
                pbPassword.Password = tbPassword.Text;
                toggleShowPassword.Content = "👁";
            }
            else
            {
                pbPassword.Visibility = Visibility.Collapsed;
                tbPassword.Visibility = Visibility.Visible;
                tbPassword.Text = pbPassword.Password;
                toggleShowPassword.Content = "👁️‍🗨️";
            }
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
        private bool IsValidPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return false;
            if (password.Length > 60)
                return false;

            return true;
        }
        private async void bEdit_Click(object sender, RoutedEventArgs e)
        {
            if (IsValidUserName(tbName.Text) && IsValidEmail(tbEmail.Text))
            {
                string newPassword = string.Empty;
                if (!string.IsNullOrEmpty(tbPassword.Text))
                {
                    if (IsValidPassword(pbPassword.Password)) { newPassword = pbPassword.Password; }
                }

                var userUpdate = new UserDTO
                {
                    IdUser = id_user,
                    NameUser = tbName.Text,
                    EmailUser = tbEmail.Text,
                    PasswordHash = newPassword,
                    IdRole = 3
                };

                var result = await Api.UpdateUser(id_user, userUpdate, userToken);
                if (result == true) { MessageBox.Show("Информация обновлена."); }
                else { MessageBox.Show("Ошибка при изменении информации."); }
            }
            else { MessageBox.Show("Неверный формат данных."); }
        }
    }
}
