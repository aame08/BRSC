using Client.Data;
using Client.For_Token;
using Client.Models;
using System.Windows;
using System.Windows.Controls;

namespace Client.Views
{
    /// <summary>
    /// Логика взаимодействия для AdminPage.xaml
    /// </summary>
    public partial class AdminPage : Page
    {
        public string adminToken;
        public int adminID;
        public AdminPage(string token)
        {
            InitializeComponent();

            adminToken = token;
            adminID = TokenManager.GetIdUserByToken(adminToken);

            LoadUsers();
        }
        private async Task RefreshDataGrid()
        {
            await LoadUsers();
        }
        private async Task LoadUsers()
        {
            adminToken = await Api.TokenValid(adminID, adminToken);
            if (adminToken != null)
            {
                var result = await Api.GetUsers(adminID, adminToken);
                dgUsers.ItemsSource = result;
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

        private async void bEdit_Click(object sender, RoutedEventArgs e)
        {
            adminToken = await Api.TokenValid(adminID, adminToken);
            if (adminToken != null)
            {
                int selectedId_User = ((User)dgUsers.SelectedItem).IdUser;
                EditUser editUser = new EditUser(selectedId_User, adminToken, RefreshDataGrid);
                frame.NavigationService.Navigate(editUser, Visibility.Visible);
            }
            else
            {
                MessageBox.Show("Ошибка аутентификации. Войдите снова.");
                this.Visibility = Visibility.Hidden;
            }
        }
    }
}
