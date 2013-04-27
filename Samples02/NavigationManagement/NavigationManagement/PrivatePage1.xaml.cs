using Microsoft.Phone.Controls;
using System;
using System.Windows.Navigation;

namespace NavigationManagement
{
    public partial class PrivatePage1 : PhoneApplicationPage
    {
        public PrivatePage1()
        {
            InitializeComponent();
        }

        private void private2Link_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/PrivatePage2.xaml", UriKind.Relative));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // If the user navigated forward to this page from the LoginPage, we remove the LoginPage from the backstack.
            if (e.NavigationMode == NavigationMode.New)
            {
                NavigationService.RemoveBackEntry();
            }
        }
    }
}