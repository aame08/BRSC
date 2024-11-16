using Client.Views;
using System.Windows;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow mainWindow;
        public MainWindow()
        {
            InitializeComponent();

            mainWindow = this;
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
            tbLogin.Text = string.Empty;
            tbPassword.Text = string.Empty;
        }

        private void bEnter_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Иди нахуй слушай)");
        }

        private void bReg_Click(object sender, RoutedEventArgs e)
        {
            frame.NavigationService.Navigate(new RegistrationPage());
        }
    }
}