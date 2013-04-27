using Microsoft.Phone.Controls;
using System.Windows.Navigation;

namespace LifecycleState
{
    public partial class MainPage : PhoneApplicationPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            stateValue.Text = App.IsAppInstancePreserved.ToString();
            dataValue.Text = App.ViewModel.Timestamp;
        }
    }
}