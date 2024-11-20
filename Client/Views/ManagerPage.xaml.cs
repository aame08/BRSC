using Client.Data;
using Client.For_Token;
using System.Windows;
using System.Windows.Controls;

namespace Client.Views
{
    /// <summary>
    /// Логика взаимодействия для ManagerPage.xaml
    /// </summary>
    public partial class ManagerPage : Page
    {
        public string managerToken;
        public int managerID;
        public ManagerPage(string token)
        {
            InitializeComponent();

            managerToken = token;
            managerID = TokenManager.GetIdUserByToken(managerToken);

            LoadUsers();
        }
        private async void LoadUsers()
        {
            managerToken = await Api.TokenValid(managerID, managerToken);
            if (managerToken != null)
            {
                var result = await Api.GetUsers(managerID, managerToken);
                dgUsers.Items.Clear();
                foreach (var user in result) { dgUsers.Items.Add(user); }
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
    }
}
