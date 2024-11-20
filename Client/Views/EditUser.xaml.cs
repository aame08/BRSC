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
    /// Логика взаимодействия для EditUser.xaml
    /// </summary>
    public partial class EditUser : Page
    {
        public string adminToken;
        public int adminID;
        public int userID;
        public Func<Task> OnUserUpdated;
        public EditUser(int userId, string token, Func<Task> ouUserUpdated)
        {
            InitializeComponent();

            adminToken = token;
            adminID = TokenManager.GetIdUserByToken(adminToken);
            userID = userId;
            OnUserUpdated = ouUserUpdated;

            LoadRoles();
            ViewUser(adminToken);
        }
        private async Task<User> ViewUser(string token)
        {
            token = await Api.TokenValid(adminID, token);
            if (token != null)
            {
                var user = await Api.GetUser(userID, token);
                if (user != null)
                {
                    tbName.Text = user.NameUser;
                    tbEmail.Text = user.EmailUser;
                    cbRoles.SelectedValue = user.IdRole;
                }
            }
            else
            {
                MessageBox.Show("Ошибка аутентификации. Войдите снова.");
                this.Visibility = Visibility.Hidden;
            }
            return null;
        }
        private async void LoadRoles()
        {
            adminToken = await Api.TokenValid(adminID, adminToken);
            if (adminToken != null)
            {
                var roles = await Api.GetRoles(adminID, adminToken);
                cbRoles.ItemsSource = roles;
                cbRoles.DisplayMemberPath = "NameRole";
                cbRoles.SelectedValuePath = "IdRole";
            }
            else
            {
                MessageBox.Show("Ошибка аутентификации. Войдите снова.");
                this.Visibility = Visibility.Hidden;
            }
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
                    IdUser = userID,
                    NameUser = tbName.Text,
                    EmailUser = tbEmail.Text,
                    PasswordHash = newPassword,
                    IdRole = (int)cbRoles.SelectedValue
                };

                adminToken = await Api.TokenValid(adminID, adminToken);
                if (adminToken != null)
                {
                    var result = await Api.UpdateUser(adminID, userID, userUpdate, adminToken);
                    if (result == true)
                    {
                        MessageBox.Show("Информация обновлена.");
                        if (OnUserUpdated != null) { await OnUserUpdated(); }
                        this.Visibility = Visibility.Hidden;
                    }
                    else { MessageBox.Show("Ошибка при изменении информации."); }
                }
                else
                {
                    MessageBox.Show("Ошибка аутентификации. Войдите снова.");
                    this.Visibility = Visibility.Hidden;
                }
            }
            else { MessageBox.Show("Неверный формат данных."); }
        }

        private async void bDelete_Click(object sender, RoutedEventArgs e)
        {
            adminToken = await Api.TokenValid(adminID, adminToken);
            if (adminToken != null)
            {
                var result = await Api.DeleteUser(adminID, userID, adminToken);
                if (result == true)
                {
                    MessageBox.Show("Пользователь удален.");
                    if (OnUserUpdated != null) { await OnUserUpdated(); }
                    this.Visibility = Visibility.Hidden;
                }
                else { MessageBox.Show("Ошибка при удалении пользователя"); }
            }
            else
            {
                MessageBox.Show("Ошибка аутентификации. Войдите снова.");
                this.Visibility = Visibility.Hidden;
            }
        }
    }
}
