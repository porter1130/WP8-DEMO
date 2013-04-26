using Microsoft.Phone.Controls;
using System.Diagnostics;
using System.Windows.Navigation;

namespace TestLifecycle
{
    public partial class MainPage : PhoneApplicationPage
    {
        public MainPage()
        {
            Debug.WriteLine("MainPage ctor");

            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Debug.WriteLine("OnNavigatedTo");
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Debug.WriteLine("OnNavigatedFrom");
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            Debug.WriteLine("OnNavigatingFrom");
        }
    }
}