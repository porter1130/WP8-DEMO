using Microsoft.Phone.Controls;
using System;
using System.Windows;
using System.Windows.Navigation;

namespace NavigationParameters
{
    public partial class MainPage : PhoneApplicationPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void buttonPage2Fragment_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)radioCoffee.IsChecked)
            {
                NavigationService.Navigate(new Uri(
                    "/Page2.xaml#Coffee", UriKind.Relative));
            }
            else
            {
                NavigationService.Navigate(new Uri(
                    "/Page2.xaml#Tea", UriKind.Relative));
            }
        }

        private void buttonPage2Querystring_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)radioCoffee.IsChecked)
            {
                NavigationService.Navigate(new Uri(
                    "/Page2.xaml?drink=Coffee", UriKind.Relative));
            }
            else
            {
                NavigationService.Navigate(new Uri(
                    "/Page2.xaml?drink=Tea", UriKind.Relative));
            }
        }
    }
}