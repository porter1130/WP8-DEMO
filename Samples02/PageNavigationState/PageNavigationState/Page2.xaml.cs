using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Navigation;

namespace PageNavigationState
{
    public partial class Page2 : PhoneApplicationPage
    {
        private bool isNewInstance;
        private const string Page2State = "TextState";

        public Page2()
        {
            InitializeComponent();
            isNewInstance = true;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            PhoneApplicationService.Current.State["TextState"] =
                textState.Text;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (isNewInstance)
            {
                if (PhoneApplicationService.Current.State.ContainsKey(Page2State))
                {
                    textState.Text = (string)
                        PhoneApplicationService.Current.State[Page2State];
                }
                isNewInstance = false;
            }
        }
    }
}