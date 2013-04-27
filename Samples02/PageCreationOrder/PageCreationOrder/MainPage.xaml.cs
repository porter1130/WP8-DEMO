using Microsoft.Phone.Controls;
using System.Diagnostics;
using System.Windows.Navigation;

namespace PageCreationOrder
{
    public partial class MainPage : PhoneApplicationPage
    {
        public MainPage()
        {
            Debug.WriteLine("MainPage.ctor");
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Debug.WriteLine("MainPage.OnNavigatedTo");
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Debug.WriteLine("MainPage.OnNavigatedFrom");
        }
    }
}