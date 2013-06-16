using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace WP8Controller.ViewModels
{
    /// <summary>
    /// A view model base class that abstracts the navigation features of Silverlight's page model. 
    /// </summary>
    public class NavigationViewModelBase: ViewModelBase
    {
        protected virtual void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            
        }
        protected virtual void OnNavigatedFrom(NavigationEventArgs e)
        {
            
        }
        protected virtual void OnNavigatedTo(NavigationEventArgs e)
        {
            
        }
        internal void InternalNavigatingFrom(NavigatingCancelEventArgs e)
        {
            OnNavigatingFrom(e);
        }
        internal void InternalNavigatedFrom(NavigationEventArgs e)
        {
            OnNavigatedFrom(e);
        }
        internal void InternalNavigatedTo(NavigationEventArgs e)
        {
            OnNavigatedTo(e);
        }
    }
}
