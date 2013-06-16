using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Navigation;
using PCController.ViewModels;
using MessageBox = System.Windows.MessageBox;

namespace PCController
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();            
//#if DEBUG
//            EnableTracingCheckBox.Visibility = Visibility.Visible;
//            TracingEndpointAddressTextBox.Visibility = Visibility.Visible;
//#endif

            //SetupSystemTrayIcon();
        }

        #region System Tray Icon Handling

        private NotifyIcon _notifyIcon;

        private void SetupSystemTrayIcon()
        {
            _notifyIcon = new NotifyIcon();
            var sri = App.GetResourceStream(new Uri("App.ico", UriKind.Relative));
            _notifyIcon.Icon = new Icon(sri.Stream);
            _notifyIcon.Visible = false;
            _notifyIcon.Click += NotifyIcon_Click;
        }

        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            Show();
            _notifyIcon.Visible = false;
            WindowState = WindowState.Normal;
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                _notifyIcon.Visible = true;
                Hide();
            }
        }

        #endregion

        #region Initializiation and shutdown of view model

        private void Window_Loaded(object sender, EventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                var vm = DataContext as MainWindowViewModel;
                if (vm != null)
                {
                    vm.Initialize();
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
           // _notifyIcon.Dispose();

            var vm = DataContext as MainWindowViewModel;
            if (vm != null)
            {
                vm.Shutdown();
            }
        }

        #endregion

        #region Misc view-related event handling

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                var url = e.Uri.ToString();
                var psi = new ProcessStartInfo(url);
                Process.Start(psi);
                e.Handled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while launching browser: " + ex.Message);
            }
        }

        #endregion
    }
}
