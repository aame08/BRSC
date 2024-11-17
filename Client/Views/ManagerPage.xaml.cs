using Client.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client.Views
{
    /// <summary>
    /// Логика взаимодействия для ManagerPage.xaml
    /// </summary>
    public partial class ManagerPage : Page
    {
        public string tokenManager;
        public ManagerPage(string token)
        {
            InitializeComponent();

            tokenManager = token;

            LoadUsers();
        }
        private async void LoadUsers()
        {
            var result = await Api.GetUsers(tokenManager);
            dgUsers.Items.Clear();
            foreach (var user in result) { dgUsers.Items.Add(user); }
        }

        private void bExit_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }
    }
}
