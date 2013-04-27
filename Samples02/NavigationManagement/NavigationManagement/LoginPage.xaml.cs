using Microsoft.Phone.Controls;
using System;
using System.Windows;
using System.Windows.Navigation;

namespace NavigationManagement
{
    public partial class LoginPage : PhoneApplicationPage
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/PrivatePage1.xaml", UriKind.Relative));
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}