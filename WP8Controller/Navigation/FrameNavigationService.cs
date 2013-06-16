using System;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;

namespace WP8Controller.Navigation
{
    /// <summary>
    /// An implementation of the <c>INavigationService</c> interface that wraps a <c>PhoneApplicationFrame</c>.
    /// </summary>
    public class FrameNavigationService : INavigationService
    {
        private PhoneApplicationFrame _frame;

        public FrameNavigationService(PhoneApplicationFrame frame)
        {
            _frame = frame;

            if (_frame != null)
            {
                _frame.Navigating += Frame_Navigating;
                _frame.Navigated += Frame_Navigated;
                _frame.Unloaded += Frame_Unloaded;
            }
        }

        private void Frame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            var handlers = Navigating;
            if (handlers != null)
            {
                handlers(sender, e);
            }
        }

        private void Frame_Navigated(object sender, NavigationEventArgs e)
        {
            var handlers = Navigated;
            if (handlers != null)
            {
                handlers(sender, e);
            }
        }

        private void Frame_Unloaded(object sender, RoutedEventArgs e)
        {
            _frame.Unloaded -= Frame_Unloaded;

            if (_frame != null)
            {
                _frame.Navigating -= Frame_Navigating;
                _frame.Navigated -= Frame_Navigated;
            }

            _frame = null;
        }

        #region INavigationService implementation

        public event NavigatingCancelEventHandler Navigating;
        public event NavigatedEventHandler Navigated;

        public bool CanGoBack
        {
            get
            {
                if (_frame != null)
                {
                    return _frame.CanGoBack;
                }

                return false;
            }
        }

        public bool Navigate(Uri source)
        {
            if (_frame != null)
            {
                return _frame.Navigate(source);
            }

            return false;
        }

        public void GoBack()
        {
            if (_frame != null)
            {
                _frame.GoBack();
            }
        }

        #endregion
    }
}
