using Microsoft.Phone.Controls;
using System.Diagnostics;
using System.Windows.Navigation;

namespace PageCreationOrder
{
    public partial class SecondPage : PhoneApplicationPage
    {
        public SecondPage()
        {
            Debug.WriteLine("SecondPage.ctor");
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Debug.WriteLine("SecondPage.OnNavigatedTo");
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Debug.WriteLine("SecondPage.OnNavigatedFrom");
        }
    }
}