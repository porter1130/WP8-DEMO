using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using Microsoft.Phone.Net.NetworkInformation;
using NLog;
using UnderControl.Shared;
using WP8Controller.Commands;
using WP8Controller.Controls;
using WP8Controller.Services;

namespace WP8Controller.ViewModels
{
    public class MainPageViewModel : NavigationViewModelBase
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private string _serverIPAddress;
        private bool _canConnect;
        private MessagePromptHelper _prompt;

        public string ServerIPAddress
        {
            get { return _serverIPAddress; }
            set
            {
                if (_serverIPAddress != value)
                {
                    _serverIPAddress = value;
                    RaisePropertyChanged("ServerIPAddress");
                }
            }
        }

        public bool CanConnect
        {
            get { return _canConnect; }
            set
            {
                if (_canConnect != value)
                {
                    _canConnect = value;
                    RaisePropertyChanged("CanConnect");
                }
            }
        }

        public ICommand ConnectCommand
        {
            get;
            private set;
        }

        public ICommand ConnectToAddressCommand
        {
            get;
            private set;
        }

        public MainPageViewModel()
        {
            _prompt = new MessagePromptHelper();
            _prompt.PromptClosed += _prompt_PromptClosed;

            ConnectCommand = new RelayCommand(Connect);
            ConnectToAddressCommand = new RelayCommand(ConnectToAddress);
        }

        private void ConnectToAddress()
        {
            if (!CheckNetwork())
            {
                return;
            }

            _logger.Trace("Connecting to specific IP address {0}", ServerIPAddress);

            IPAddress ipAddress;
            if (string.IsNullOrEmpty(ServerIPAddress)
                || !IPAddress.TryParse(ServerIPAddress, out ipAddress))
            {
                MessageBox.Show("Please enter a valid IP address in the format xxx.xxx.xxx.xxx");
                return;
            }

            try
            {
                ApplicationContext.Current.ControllerClient.ConnectAsync(ipAddress);
                ShowPrompt();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Connect()
        {
            if (!CheckNetwork())
            {
                return;
            }

            _logger.Trace("Auto-connecting");

            try
            {
                ApplicationContext.Current.ControllerClient.ConnectAsync();
                ShowPrompt();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ShowPrompt()
        {
            _prompt.Show("Connecting to the server. This may take a few seconds....");
        }

        private bool CheckNetwork()
        {
            if (NetworkWatchdog.Current.InterfaceType == NetworkInterfaceType.Ethernet)
            {
                MessageBox.Show("Please disconnect your phone from your PC before connecting to a remote server.");
                return false;
            }

            return true;
        }

        void _prompt_PromptClosed(object sender, Coding4Fun.Toolkit.Controls.PopUpEventArgs<string, Coding4Fun.Toolkit.Controls.PopUpResult> e)
        {
            // when the prompt has been dismissed programmatically the client is shut down already
            // => in any other case (user hit back button or confirmed the dialog) shut down
            if (e.Result != MessagePromptHelper.ProgrammaticallyDismissedToken)
            {
                try
                {
                    // shut down
                    ApplicationContext.Current.ControllerClient.Shutdown();
                }
                catch (PhoneControllerException)
                {
                    // ignore these kinds of errors
                }
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _logger.Trace("Setting up in OnNavigatedTo");

            base.OnNavigatedTo(e);

            //get controller
            var controller = ApplicationContext.Current.ControllerClient;

            //hook events
            //controller.Error += controller_Error;
            controller.StateChanged += controller_StateChanged;

            //set IP address to "last know good"
            if (controller.ServerAddress != null)
            {
                ServerIPAddress = controller.ServerAddress.ToString();
            }
            else if (ApplicationContext.Current.LastServerAddress != null)
            {
                ServerIPAddress = ApplicationContext.Current.LastServerAddress.ToString();
            }

            //hook event of network watchdog
            NetworkWatchdog.Current.NetworkChanged += Current_NetworkChanged;

            //initialize
            UpdateNetworkStatus();

            //message?
            if (!ApplicationContext.Current.InitialMessageShown)
            {
                MessageBox.Show(
                    "To use this application, you need to connect it to a desktop application that you can interact with." +
                    Environment.NewLine + "Download it for free from xx.", "UnderControl - First startup", MessageBoxButton.OK);

                //not next time
                ApplicationContext.Current.InitialMessageShown = true;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _logger.Trace("Cleaning up in OnNavigatedFrom");

            base.OnNavigatedFrom(e);

            //get controller
            var controller = ApplicationContext.Current.ControllerClient;

            //remove event handler
            controller.Error -= controller_Error;
            controller.StateChanged -= controller_StateChanged;

            //remove event handler of network watchdog
            NetworkWatchdog.Current.NetworkChanged -= Current_NetworkChanged;
        }
        private void UpdateNetworkStatus()
        {
            //determine if we can connect
            var watchdog = NetworkWatchdog.Current;

            //explicitly include the network type "Ethernet" although it does _not_allow connection
            //this allows us to check for the network type in the connect methods and show a meaningful message to the user
            //when they try to connect when they're connected to the PC (if we excluded the type here they would be stuck
            //without a clue).
            CanConnect = watchdog != null
                         && watchdog.IsNetworkAvailable
                         &&
                         (watchdog.InterfaceType == NetworkInterfaceType.Wireless80211 ||
                          watchdog.InterfaceType == NetworkInterfaceType.Ethernet);
        }

        void Current_NetworkChanged(object sender, NetworkChangedEventArgs e)
        {
            UpdateNetworkStatus();
        }

        void controller_StateChanged(object sender, UnderControl.Shared.PhoneControllerStateEventArgs e)
        {
            _logger.Trace("Received StateChanged event of controller client");

            if (e.State == PhoneControllerState.Ready)
            {
                _logger.Trace("Navigating to page InputPage");

                HidePrompt();

                ApplicationContext.Current.NavigationService.Navigate(new Uri("/InputPage.xaml", UriKind.Relative));
            }
        }

        void controller_Error(object sender, UnderControl.Shared.ErrorEventArgs e)
        {
            HidePrompt();

            try
            {
                // shut down the controller
                ApplicationContext.Current.ControllerClient.Shutdown();
            }
            catch (PhoneControllerException)
            {
                // ignore if this error happens
            }

            // give the message prompt (if applicable)
            //a chance to hide itself before we show a message box
            Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    var technicalDetails = e.Error.ErrorCode ?? e.Error.Message;
                    var message =
                        string.Format(
                            "An error occurred while communicating to the server. Please try again later.{0}Technical details: '{1}'",
                            Environment.NewLine,
                            technicalDetails);
                    MessageBox.Show(message);
                });
        }

        private void HidePrompt()
        {
            _prompt.Hide();
        }
    }
}
