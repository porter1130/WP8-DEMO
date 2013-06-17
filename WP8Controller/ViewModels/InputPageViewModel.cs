using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Threading;
using Microsoft.Expression.Interactivity.Core;
using Microsoft.Phone.Net.NetworkInformation;
using UnderControl.Shared;
using UnderControl.Shared.Data;
using WP8Controller.Commands;
using WP8Controller.Controls;
using WP8Controller.Services;
using WP8Controller.TriggerActions;

namespace WP8Controller.ViewModels
{
    public class InputPageViewModel : NavigationViewModelBase
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private TextInputHelper _textInputHelper;
        private string _lastInputText;

        private bool _isCustomDragInProgress;
        private bool _isTextInputInProgress;

        private UnderControl.Shared.Data.Thickness _inputMarginThickness;
        private Visibility _textInputVisibility;
        private Visibility _customDragVisibility;

        private bool _backNavigationInProgress;
        private bool _waitForInitializedState;

        private DispatcherTimer _timer;
        private bool _isWaitingForConnection;
        private int _waitingSecondsRemaining;
        #region Properties for UI bindings





        public UnderControl.Shared.Data.Thickness InputMarginThickness
        {
            get
            {
                return _inputMarginThickness;
            }
            set
            {
                if (_inputMarginThickness != value)
                {
                    _inputMarginThickness = value;
                    RaisePropertyChanged("InputMarginThickness");
                }
            }
        }

        public Visibility TextInputVisibility
        {
            get
            {
                return _textInputVisibility;
            }
            set
            {
                if (_textInputVisibility != value)
                {
                    _textInputVisibility = value;
                    RaisePropertyChanged("TextInputVisibility");
                }
            }
        }

        public Visibility CustomDragVisibility
        {
            get
            {
                return _customDragVisibility;
            }
            set
            {
                if (_customDragVisibility != value)
                {
                    _customDragVisibility = value;
                    RaisePropertyChanged("CustomDragVisibility");
                }
            }
        }

        public bool IsCustomDragInProgress
        {
            get
            {
                return _isCustomDragInProgress;
            }
            set
            {
                if (_isCustomDragInProgress != value)
                {
                    _isCustomDragInProgress = value;
                    RaisePropertyChanged("IsCustomDragInProgress");
                    RaisePropertyChanged("IsTextInputEnabled");
                }
            }
        }

        public bool IsCustomDragEnabled
        {
            get
            {
                return !IsTextInputInProgress;
            }
        }

        public bool IsTextInputInProgress
        {
            get
            {
                return _isTextInputInProgress;
            }
            set
            {
                if (_isTextInputInProgress != value)
                {
                    _isTextInputInProgress = value;
                    RaisePropertyChanged("IsTextInputInProgress");
                    RaisePropertyChanged("IsCustomDragEnabled");
                }
            }
        }

        public bool IsTextInputEnabled
        {
            get
            {
                return !IsCustomDragInProgress;
            }
        }

        public bool IsWaitingForConnection
        {
            get
            {
                return _isWaitingForConnection;
            }
            set
            {
                if (_isWaitingForConnection != value)
                {
                    _isWaitingForConnection = value;
                    RaisePropertyChanged("IsWaitingForConnection");
                    RaisePropertyChanged("WaitingForConnectionMessageVisibility");
                }
            }
        }

        public Visibility WaitingForConnectionMessageVisibility
        {
            get
            {
                return IsWaitingForConnection ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public string WaitingForConnectionMessage
        {
            get
            {
                return string.Format("Trying to reconnect...{0}{1}{2} seconds left{3}{4}Use the back button to prematurely abort and return to the main menu.",
                                     Environment.NewLine,
                                     Environment.NewLine,
                                     _waitingSecondsRemaining,
                                     Environment.NewLine,
                                     Environment.NewLine);
            }
        }

        public ICommand TextInputUserInteractionStartedCommand
        {
            get;
            private set;
        }

        public ICommand TextInputUserInteractionEndedCommand
        {
            get;
            private set;
        }

        public ICommand TextInputActivatedCommand
        {
            get;
            private set;
        }

        public ICommand CustomDragUserInteractionStartedCommand
        {
            get;
            private set;
        }

        public ICommand CustomDragUserInteractionEndedCommand
        {
            get;
            private set;
        }

        public ICommand CustomDragActivatedCommand
        {
            get;
            private set;
        }

        public ICommand CustomDragDeactivatedCommand
        {
            get;
            private set;
        }


        public ICommand InputTextKeyDownCommand { get; private set; }

        public ICommand InputTextGotFocusCommand { get; private set; }

        public ICommand InputTextLostFocusCommand { get; private set; }
        #endregion

        #region Navigation Handling

        public InputPageViewModel()
        {
            _textInputHelper = new TextInputHelper();
            _textInputHelper.TextInputFinished += _textInputHelper_TextInputFinished;

            InputTextKeyDownCommand = new RelayCommand<EventInformation<KeyEventArgs>>(InputTextData_KeyDown);
            //InputTextGotFocusCommand = new RelayCommand(InputTextGotFocus);
            //InputTextLostFocusCommand = new RelayCommand(InputTextLostFocus);
        }

        private void InputTextData_KeyDown(EventInformation<KeyEventArgs> ei)
        {
            var input = ei.Sender as TextBox;
            if (ei.EventArgs.Key == Key.Enter)
            {
                _textInputHelper.RaiseTextInputFinishedEvent(input.Text);
            }
        }



        private void InputTextLostFocus()
        {
            ResumeDataAcquisition();
        }

        private void InputTextGotFocus()
        {
            PauseAllDataAcquisition();
        }




        private void PauseAllDataAcquisition()
        {
            // get active data acquisitions and pause all
            var dataSource = ApplicationContext.Current.ControllerClient.DataSource;
            var activeDataTypes = dataSource.GetActiveDataTypes();
            foreach (var dataType in activeDataTypes)
            {
                // stop all others
                dataSource.PauseAcquisition(dataType);
            }
        }

        private void ResumeDataAcquisition()
        {
            var dataSource = ApplicationContext.Current.ControllerClient.DataSource;
            var pausedDataTypes = dataSource.GetPausedDataTypes();
            foreach (var dataType in pausedDataTypes)
            {
                // ignore the custom drag inputs, these are supposed to be paused until explicitly activated above
                if (dataType == DataType.CustomDrag || dataType == DataType.CustomDragComplete)
                {
                    continue;
                }

                dataSource.ResumeAcquisition(dataType);
            }
        }

        void _textInputHelper_TextInputFinished(object sender, TextInputEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Text))
            {
                _logger.Trace("No text entered, nothing to do");
                return;
            }

            _logger.Trace("Sending manually entered text");

            try
            {
                var textData = new TextData();
                textData.Text = e.Text;
                ApplicationContext.Current.ControllerClient.DataSource.AddData(textData);
            }
            catch (PhoneControllerException ex)
            {
                MessageBox.Show("An error occurred while sending the text: " + ex.Message);
            }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            _logger.Trace("Setting up in OnNavigatedTo");

            base.OnNavigatedTo(e);

            // get controller and set up
            var controllerClient = ApplicationContext.Current.ControllerClient;
            controllerClient.StateChanged += controllerClient_StateChanged;
            controllerClient.Error += controllerClient_Error;
            controllerClient.ConfigurationChanged += controllerClient_ConfigurationChanged;
            controllerClient.DataSource.DataTypesChanged += DataSource_DataTypesChanged;

            //let's see how we got here
            if (e.NavigationMode == NavigationMode.Back)
            {
                // => some one navigated back to us, which can only mean that we returned from deactivation
                //=> check to see if we should try to reconnect
                var lastIpAddress = ApplicationContext.Current.LastServerAddress;
                if (controllerClient.Configuration.AutoReconnectOnActivation && lastIpAddress != null)
                {
                    InitiateReconnect();
                }
                else
                {
                    //no reconnect => navigate back to the main menu
                    _logger.Trace("Cannot or not supposed to reconnect => navigating back");

                    // we cannot or are not supposed to reconnect
                    NavigateBackToMainPage();
                }
            }
            else
            {
                //if we came here from the main page
                //we are connected
                Debug.Assert(controllerClient.State == PhoneControllerState.Ready);

                AdjustInputMarginThickness();

                //initial setup
                var activeDataTypes = controllerClient.DataSource.GetActiveDataTypes();
                AdjustDataTypesVisibility(activeDataTypes);
                InitializeDataSource(activeDataTypes);
            }
        }

        private void InitializeDataSource(IEnumerable<DataType> activeDataTypes)
        {
            if (activeDataTypes.Contains(DataType.CustomDrag))
            {
                //pause this, because we manually trigger it when the user wants to explicitly input that type of gesture
                ApplicationContext.Current.ControllerClient.DataSource.PauseAcquisition(DataType.CustomDrag);
            }
        }

        private void AdjustDataTypesVisibility(IEnumerable<DataType> activeDataTypes)
        {
            TextInputVisibility = activeDataTypes.Contains(DataType.Text) ? Visibility.Visible : Visibility.Collapsed;
            CustomDragVisibility = activeDataTypes.Contains(DataType.CustomDrag)
                                       ? Visibility.Visible
                                       : Visibility.Collapsed;
        }

        private void AdjustInputMarginThickness()
        {
            var inputMargin = ApplicationContext.Current.ControllerClient.Configuration.TouchInputMargin;

            //depending on the screen orientation, we switch the left and right margins

        }

        private void NavigateBackToMainPage()
        {
            // => navigate back to the main page
            var navigationService = ApplicationContext.Current.NavigationService;
            if (navigationService.CanGoBack && !_backNavigationInProgress)
            {
                _backNavigationInProgress = true;
                navigationService.GoBack();
            }
        }

        private void InitiateReconnect()
        {
            _waitForInitializedState = true;

            //start the timer to give some visual feedback
            if (_timer == null)
            {
                _timer = new DispatcherTimer();
                _timer.Interval = TimeSpan.FromSeconds(1.0);
                _timer.Tick += _timer_Tick;
            }
        }

        void _timer_Tick(object sender, EventArgs e)
        {
            //update UI
            _waitingSecondsRemaining--;
            RaisePropertyChanged("WaitingForConnectionMessage");

            //decide what to do
            if (_waitingSecondsRemaining == 0)
            {
                //we were not successful
                _timer.Stop();
                IsWaitingForConnection = false;

                MessageBox.Show(
                    "No suitable network connection available to automatically reconnect. Navigating back to the main menu...",
                    "Reconnect failed", MessageBoxButton.OK);
                NavigateBackToMainPage();
            }
            else
            {
                //check whether we are already trying to reconnect or not
                if (ApplicationContext.Current.ControllerClient.State == PhoneControllerState.Closed)
                {
                    //we are not yet trying to reconnect => check whether we have a suitable network
                    var watchdog = NetworkWatchdog.Current;
                    var canConnect = watchdog != null && watchdog.IsNetworkAvailable &&
                                     watchdog.InterfaceType == NetworkInterfaceType.Wireless80211;

                    if (canConnect)
                    {
                        Reconnect();
                    }
                }
            }
        }

        private void Reconnect()
        {
            var controllerClient = ApplicationContext.Current.ControllerClient;

            //get ip and try to reconnect
            var lastIpAddress = ApplicationContext.Current.LastServerAddress;

            try
            {
                _logger.Trace("Trying to reconnect automatically");

                //start connection
                controllerClient.ConnectAsync(lastIpAddress);
            }
            catch (Exception)
            {
                //shut down and out of here
                controllerClient.Shutdown();
                NavigateBackToMainPage();
            }
        }

        void DataSource_DataTypesChanged(object sender, UnderControl.Shared.Data.DataTypesChangedEventArgs e)
        {
            _logger.Trace("Received DataTypesChanged event from controller client");

            AdjustDataTypesVisibility(e.NewDataTypes);
            InitializeDataSource(e.NewDataTypes);
        }

        void controllerClient_ConfigurationChanged(object sender, EventArgs e)
        {
            _logger.Trace("Received ConfigurationChanged event from controller client");

            AdjustInputMarginThickness();
        }

        void controllerClient_Error(object sender, UnderControl.Shared.ErrorEventArgs e)
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer = null;
            }
            IsWaitingForConnection = false;

            //show to the user
            MessageBox.Show("An unexpected error occurrred:" + e.Error.Message);

            NavigateBackToMainPage();
        }

        void controllerClient_StateChanged(object sender, UnderControl.Shared.PhoneControllerStateEventArgs e)
        {
            if (_waitForInitializedState && e.State != PhoneControllerState.Initialized)
            {
                return;
            }
            _waitForInitializedState = false;

            //let's see what we should do
            switch (e.State)
            {
                case PhoneControllerState.Closed:
                    NavigateBackToMainPage();
                    break;
                case PhoneControllerState.Ready:
                    // stop timer if applicable
                    if (_timer != null)
                    {
                        _timer.Stop();
                        _timer = null;
                    }
                    IsWaitingForConnection = false;

                    //initial setup
                    AdjustInputMarginThickness();
                    var activeDataTypes = ApplicationContext.Current.ControllerClient.DataSource.GetActiveDataTypes();
                    AdjustDataTypesVisibility(activeDataTypes);
                    InitializeDataSource(activeDataTypes);
                    break;
            }
        }
        #endregion
    }
}
