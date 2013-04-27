using Microsoft.Phone.Controls;
using System;
using System.Windows;
using System.Windows.Navigation;

namespace NavigationManagement
{
    public partial class MainPage : PhoneApplicationPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void publicLink_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/PublicPage.xaml", UriKind.Relative));
        }

        private void privateLink_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/LoginPage.xaml", UriKind.Relative));
        }
    }
}