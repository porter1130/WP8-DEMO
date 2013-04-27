using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Navigation;

namespace PageNavigationState
{
    public partial class MainPage : PhoneApplicationPage
    {
        private bool isNewInstance;
        private const string MainPageState = "CheckState";

        public MainPage()
        {
            InitializeComponent();
            isNewInstance = true;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            PhoneApplicationService.Current.State[MainPageState] =
                checkState.IsChecked;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (isNewInstance)
            {
                if (PhoneApplicationService.Current.State.ContainsKey(MainPageState))
                {
                    checkState.IsChecked = (bool)
                        PhoneApplicationService.Current.State[MainPageState];
                }
                isNewInstance = false;
            }
        }
    }
}