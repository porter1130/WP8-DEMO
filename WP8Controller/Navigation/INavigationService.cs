using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace WP8Controller.Navigation
{
    public interface INavigationService
    {
        event NavigatingCancelEventHandler Navigating;
        event NavigatedEventHandler Navigated;
        bool CanGoBack { get; }
        bool Navigate(Uri source);
        void GoBack();
    }
}
