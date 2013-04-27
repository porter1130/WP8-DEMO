using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Navigation;

namespace PageNavigationState
{
    public partial class Page3 : PhoneApplicationPage
    {
        private bool isNewInstance;
        private const string Page3State = "SliderState";

        public Page3()
        {
            InitializeComponent();
            isNewInstance = true;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            PhoneApplicationService.Current.State[Page3State] =
                sliderState.Value;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (isNewInstance)
            {
                if (PhoneApplicationService.Current.State.ContainsKey(Page3State))
                {
                    sliderState.Value = (double)
                        PhoneApplicationService.Current.State[Page3State];
                }
                isNewInstance = false;
            }
        }
    }
}