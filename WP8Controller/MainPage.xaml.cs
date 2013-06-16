using System.Windows;
using System.Windows.Navigation;
using WP8Controller.Views;

namespace WP8Controller
{
    public partial class MainPage : NavigationPhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();           
        }
      

        private void AdvancedOptionsButton_Click(object sender, RoutedEventArgs e)
        {
            AdvancedOptionsPanel.Visibility = AdvancedOptionsPanel.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
            AdvancedOptionsButton.Content = AdvancedOptionsPanel.Visibility == Visibility.Collapsed
                                                ? "Show Advanced Options" 
                                                : "Hide Advanced Options";
        }
        
    }
}