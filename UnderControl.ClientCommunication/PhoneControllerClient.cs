using System;
using System.Net;
using System.Threading;
using UnderControl.Communication.Sockets;
using UnderControl.Shared;
using UnderControl.Shared.Channels;
using UnderControl.Shared.ControlCommands;
using UnderControl.Shared.Data;
using UnderControl.Shared.Sockets;

namespace UnderControl.Communication
{
    /// <summary>
    /// An implementation of the phone controller client side.
    /// </summary>
    public class PhoneControllerClient
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private SynchronizationContext _synchronizationContext;
        private object _locker = new object();

        private ICommunicationFactory _communicationFactory;
        private PhoneControllerState _state;
        private ControllerConfiguration _configuration;
        private MulticastClient _broadCaster;
        private IControlChannel _controlChannel;
        private IDataChannel _dataChannel;

        /// <summary>
        /// Gets the current state of the phone controller.
        /// </summary>
        public PhoneControllerState State
        {
            get
            {
                return _state;
            }
            private set
            {
                if (_state != value)
                {
                    _state = value;
                    RaiseStateChangedEvent();
                }
            }
        }

        /// <summary>
        /// Gets the current configuration of the phone controller.
        /// </summary>
        public ControllerConfiguration Configuration
        {
            get
            {
                return _configuration;
            }
            private set
            {
                if (_configuration != value)
                {
                    _configuration = value;
                    RaiseConfigurationChangedEvent();
                }
            }
        }

        /// <summary>
        /// Gets the address of the server the controller is connected to, if applicable.
        /// </summary>
        public IPAddress ServerAddress
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the data source used by the controller to acquire data.
        /// </summary>
        public IDataSource DataSource
        {
            get;
            private set;
        }

        /// <summary>
        /// Raised when the current state of the controller has changed.
        /// </summary>
        /// <remarks>
        /// This event is guaranteed to be raised on the synchronization context of the thread that created the controller.
        /// </remarks>
        public event EventHandler<PhoneControllerStateEventArgs> StateChanged;

        /// <summary>
        /// Raised when the current configuration of the controller has changed.
        /// </summary>
        /// <remarks>
        /// This event is guaranteed to be raised on the synchronization context of the thread that created the controller.
        /// </remarks>
        public event EventHandler<EventArgs> ConfigurationChanged;

        /// <summary>
        /// Raised when any kind of error occurred.
        /// </summary>
        /// <remarks>
        /// This event is guaranteed to be raised on the synchronization context of the thread that created the controller.
        /// </remarks>
        public event EventHandler<ErrorEventArgs> Error;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhoneControllerClient"/> class using the given data source.
        /// </summary>
        /// <param name="dataSource">The data source used to acquire data.</param>
        public PhoneControllerClient(IDataSource dataSource)
        {
            _communicationFactory = new DefaultCommunicationFactory();

            CreatePhoneControllerClient(dataSource);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PhoneControllerClient"/> class.
        /// </summary>
        /// <param name="dataSource">The data source used to acquire data.</param>
        /// <param name="communicationFactory">The communication factory used to create the actual communication objects.</param>
        public PhoneControllerClient(IDataSource dataSource, ICommunicationFactory communicationFactory)
        {
            if (communicationFactory == null)
            {
                throw new ArgumentNullException("communicationFactory");
            }

            _communicationFactory = communicationFactory;

            CreatePhoneControllerClient(dataSource);
        }

        private void CreatePhoneControllerClient(IDataSource dataSource)
        {
            if (dataSource == null)
            {
                throw new ArgumentNullException("dataSource");
            }

            // capture the current synchronization context
            _synchronizationContext = SynchronizationContext.Current;
            if (_synchronizationContext == null)
            {
                // create a default implementation
                _synchronizationContext = new SynchronizationContext();
            }

            // store and set up data source
            DataSource = dataSource;
            DataSource.DataAcquired += DataSource_DataAcquired;

            // create all required networking objects
            _broadCaster = new MulticastClient();
            _broadCaster.ServerDiscovered += BroadCaster_ServerDiscovered;
            _broadCaster.TimeoutElapsed += BroadCaster_TimeoutElapsed;

            _controlChannel = _communicationFactory.CreateControlChannel();
            _controlChannel.Error += ControlChannel_Error;
            _controlChannel.ControlCommandReceived += ControlChannel_ControlCommandReceived;
            _controlChannel.Connected += ControlChannel_Connected;
            _controlChannel.ConnectionClosed += ControlChannel_ConnectionClosed;

            _dataChannel = _communicationFactory.CreateDataChannel();
            _dataChannel.Error += DataChannel_Error;
            _dataChannel.Connected += DataChannel_Connected;
            _dataChannel.ConnectionClosed += DataChannel_ConnectionClosed;

            // configure some default values for the configuration here
            // these typically are overridden by a configuration sent down from the server side
            var configuration = new ControllerConfiguration();
            configuration.AutoReconnectOnActivation = false;
            configuration.TouchInputMargin = new Thickness(90, 20, 20, 20);
            configuration.MinAccelerometerDataRate = 0;
            configuration.MinCompassDataRate = 0;
            configuration.MinGyroscopeDataRate = 0;
            configuration.MinMotionDataRate = 0;
            configuration.MinMillisecondsBetweenMessages = 0; // as fast as possible
            configuration.EnableTracing = false;
            configuration.TracingEndpointAddress = null;
            Configure(configuration);

            State = PhoneControllerState.Created;
        }

        /// <summary>
        /// Asynchronously connects to a controller server, using multicast broadcasting to discover that server on the same network.
        /// </summary>
        public void ConnectAsync()
        {
            try
            {
                // lock to prevent multiple simultaneous calls to the broadcaster from different threads
                lock (_locker)
                {
                    var state = State;
                    if (state != PhoneControllerState.Created && state != PhoneControllerState.Closed)
                    {
                        throw new InvalidOperationException("Can only initialize from Created or Closed state (current state " + State + ").");
                    }

                    _logger.Trace("Trying to discover server using the broadcaster");

                    _broadCaster.DiscoverServer();
                }
            }
            catch (Exception ex)
            {
                throw new PhoneControllerException("Error while starting async connection: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Asynchronously connects to a server using the given server address.
        /// </summary>
        /// <param name="ipAddress">The IP address of the server to connect to.</param>
        public void ConnectAsync(IPAddress ipAddress)
        {
            try
            {
                // lock to prevent multiple simultaneous calls to the channels from different threads
                lock (_locker)
                {
                    var state = State;
                    if (state != PhoneControllerState.Created && state != PhoneControllerState.Closed)
                    {
                        throw new InvalidOperationException("Can only initialize from Created or Closed state (current state " + State + ").");
                    }

                    _logger.Trace("Trying to connect to server using IP address {0}", ipAddress);

                    ServerAddress = ipAddress;

                    _controlChannel.ConnectAsync(ServerAddress.ToString(), Constants.CommunicationControlChannelPort);
                    _dataChannel.ConnectAsync(ServerAddress.ToString(), Constants.CommunicationDataChannelPort);

                    State = PhoneControllerState.Initialized;
                }
            }
            catch (Exception ex)
            {
                throw new PhoneControllerException("Error while starting async connection: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Configures the controller using the given configuration.
        /// </summary>
        /// <param name="controllerConfiguration">The controller configuration used for configuration.</param>
        public void Configure(ControllerConfiguration controllerConfiguration)
        {
            try
            {
                // lock to prevent multiple simultaneous calls to the data source and channel from different threads
                lock (_locker)
                {
                    _logger.Trace("Configuring using a controller configuration");

                    // configure data source and channel
                    DataSource.Configure(controllerConfiguration);
                    _dataChannel.MinMillisecondsBetweenMessages = controllerConfiguration.MinMillisecondsBetweenMessages;

                    // configure tracing
                    LoggingConfigurator.Configure(controllerConfiguration);
                }

                // set the property and notify outside world
                Configuration = controllerConfiguration;
            }
            catch (Exception ex)
            {
                throw new PhoneControllerException("Error while configuring phone controller client: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Shuts down the controller. After a successful shutdown, the controller transitions to the <c>Closed</c>
        /// state and can be re-initialized later (e.g. re-connected by using the respective <c>ConnectAsync</c> methods).
        /// </summary>
        public void Shutdown()
        {
            try
            {
                // lock to prevent multiple simultaneous calls to any of the involved objects from different threads
                lock (_locker)
                {
                    var state = State;

                    // only proceed when valid (we allow calling shutdown when the state is "Closed" or "Created" etc.
                    // because certain actions like broadcasting are allowed in these states too)
                    if (state == PhoneControllerState.Closing)
                    {
                        return;
                    }

                    _logger.Trace("Shutting down");

                    // transition from closing to close, shutting down everything
                    State = PhoneControllerState.Closing;

                    if (DataSource != null)
                    {
                        DataSource.Shutdown();
                    }

                    if (_broadCaster != null)
                    {
                        _broadCaster.Shutdown();
                    }

                    if (_dataChannel != null)
                    {
                        _dataChannel.Shutdown();
                    }

                    if (_controlChannel != null)
                    {
                        _controlChannel.Shutdown();
                    }

                    State = PhoneControllerState.Closed;
                }
            }
            catch (Exception ex)
            {
                throw new PhoneControllerException("Error while shutting down the phone controller client: " + ex.Message, ex);
            }
        }

        private void DataSource_DataAcquired(object sender, DataMessageEventArgs e)
        {
            try
            {
                // lock to prevent multiple simultaneous calls to the data channel from different threads
                lock (_locker)
                {
                    _logger.Trace("Received data from the data source: {0}", e.DataMessage.DataType);

                    if (_dataChannel != null && _dataChannel.IsConnected)
                    {
                        // for some messages, we need to do some clean-up here.
                        // explanation: we use two separate sockets to transfer data: TCP and UDP.
                        // some data messages require a logical sequence, e.g. PinchComplete must always occur after the last Pinch.
                        // the problem is that for performance reasons, data messages like Pinch are sent using UDP, whereas
                        // all "Complete" messages are sent using TCP (because their delivery must be guaranteed). This can lead
                        // to the situation that Pinch messages arrive _after_ PinchComplete messages on the server side.
                        // we avoid this here by purging all queued messages of a certain kind if we are about to send a corresponding
                        // "Complete" message.
                        switch (e.DataMessage.DataType)
                        {
                            case DataType.CustomDragComplete:
                                _dataChannel.PurgeFromQueue(DataType.CustomDrag);
                                break;
                            case DataType.DragComplete:
                                _dataChannel.PurgeFromQueue(DataType.FreeDrag);
                                _dataChannel.PurgeFromQueue(DataType.HorizontalDrag);
                                _dataChannel.PurgeFromQueue(DataType.VerticalDrag);
                                break;
                            case DataType.PinchComplete:
                                _dataChannel.PurgeFromQueue(DataType.Pinch);
                                break;
                        }

                        // now actually send the message
                        _dataChannel.Send(e.DataMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                State = PhoneControllerState.Error;
                RaiseErrorEvent("Error while processing and sending acquired data: " + ex.Message, ex);
            }
        }

        private void BroadCaster_ServerDiscovered(object sender, ServerDiscoveredEventArgs e)
        {
            try
            {
                // lock to prevent multiple simultaneous calls to the channels from different threads
                lock (_locker)
                {
                    _logger.Trace("Discovered a server at {0}", e.RemoteAddress);

                    _controlChannel.ConnectAsync(e.RemoteAddress, Constants.CommunicationControlChannelPort);
                    _dataChannel.ConnectAsync(e.RemoteAddress, Constants.CommunicationDataChannelPort);

                    ServerAddress = IPAddress.Parse(e.RemoteAddress);

                    State = PhoneControllerState.Initialized;
                }
            }
            catch (Exception ex)
            {
                State = PhoneControllerState.Error;
                RaiseErrorEvent("Error while connecting control and data channels: " + ex.Message, ex);
            }
        }

        private void BroadCaster_TimeoutElapsed(object sender, EventArgs e)
        {
            lock (_locker)
            {
                _logger.Trace("Received TimeoutElapsed event from broadcaster");

                var state = State;
                // broadcasting is also possible from the closed state (during re-initialization)
                // so we only check for the error state here
                if (state == PhoneControllerState.Error)
                {
                    // ignore follow-up errors after we're closed or already in error state
                    return;
                }

                State = PhoneControllerState.Error;

                // create new error event args
                var eventArgs = new NetworkErrorEventArgs("Timeout while broadcasting.", "TimeOut", null);
                RaiseErrorEvent(eventArgs);
            }
        }

        private void DataChannel_Connected(object sender, EventArgs e)
        {
            try
            {
                // lock to prevent multiple simultaneous calls to the data channel from different threads
                lock (_locker)
                {
                    _logger.Trace("Received Connected event from data channel");

                    // first thing to do is to send the info about this controller
                    var controllerInfo = DataSource.GetControllerInfoData();
                    _dataChannel.Send(controllerInfo);

                    // check state
                    if (_dataChannel.IsConnected && _controlChannel.IsConnected)
                    {
                        State = PhoneControllerState.Ready;
                    }
                }
            }
            catch (Exception ex)
            {
                State = PhoneControllerState.Error;
                RaiseErrorEvent("Error while sending controller info to server: " + ex.Message, ex);
            }
        }

        private void DataChannel_ConnectionClosed(object sender, EventArgs e)
        {
            _logger.Trace("Received ConnectionClosed event from data channel");

            Shutdown();
        }

        private void DataChannel_Error(object sender, NetworkErrorEventArgs e)
        {
            _logger.Trace("Received Error event from data channel");

            var state = State;
            if (state == PhoneControllerState.Closed || state == PhoneControllerState.Closing || state == PhoneControllerState.Error)
            {
                // ignore follow-up errors after we're closed or already in error state
                return;
            }

            State = PhoneControllerState.Error;
            RaiseErrorEvent(e);
        }

        private void ControlChannel_Connected(object sender, EventArgs e)
        {
            // lock to prevent overlapping operations on the channels
            lock (_locker)
            {
                _logger.Trace("Received Connected event from the control channel");

                // check state
                if (_dataChannel.IsConnected && _controlChannel.IsConnected)
                {
                    State = PhoneControllerState.Ready;
                }
            }
        }

        private void ControlChannel_ConnectionClosed(object sender, EventArgs e)
        {
            _logger.Trace("Received ConnectionClosed event from the control channel");

            Shutdown();
        }

        private void ControlChannel_ControlCommandReceived(object sender, ControlCommandReceivedEventArgs e)
        {
            _logger.Trace("Control command received: {0}, {1}", e.ControlCommand.DataType, e.ControlCommand.Action);

            if (e.ControlCommand.DataType == DataType.Configuration)
            {
                var command = e.ControlCommand as ConfigurationControlCommand;
                if (command != null && command.Configuration != null)
                {
                    Configure(command.Configuration);
                }
            }
            else
            {
                try
                {
                    // lock to prevent multiple simultaneous calls to the data source and channel from different threads
                    lock (_locker)
                    {
                        if (e.ControlCommand.Action == ControlCommandAction.Start)
                        {
                            DataSource.StartAcquisition(e.ControlCommand.DataType);
                        }
                        else
                        {
                            DataSource.StopAcquisition(e.ControlCommand.DataType);
                            _dataChannel.PurgeFromQueue(e.ControlCommand.DataType);
                        }
                    }
                }
                catch (Exception ex)
                {
                    State = PhoneControllerState.Error;
                    RaiseErrorEvent("Error while processing start/stop control command: " + ex.Message, ex);
                }
            }
        }

        private void ControlChannel_Error(object sender, NetworkErrorEventArgs e)
        {
            _logger.Trace("Received Error event from the control channel");

            var state = State;
            if (state == PhoneControllerState.Closed || state == PhoneControllerState.Closing || state == PhoneControllerState.Error)
            {
                // ignore any follow-up errors after we're closed or after we're already in error state
                return;
            }

            State = PhoneControllerState.Error;
            RaiseErrorEvent(e);
        }

        private void RaiseErrorEvent(string message, Exception ex)
        {
            _logger.Trace("Raising Error event with message: {0}", message);

            // make sure we only raise events on the correct context
            _synchronizationContext.Post(o =>
            {
                var handlers = Error;
                if (handlers != null)
                {
                    var exception = new PhoneControllerException(message, ex);
                    handlers(this, new ErrorEventArgs(exception));
                }
            }, null);
        }

        private void RaiseErrorEvent(NetworkErrorEventArgs e)
        {
            _logger.Trace("Raising Error event with message: {0}", e.Message);

            // make sure we only raise events on the correct context
            _synchronizationContext.Post(o =>
                                             {
                                                 var handlers = Error;
                                                 if (handlers != null)
                                                 {
                                                     var exception = new PhoneControllerException("Networking error: " + e.Message, e.Error, e.ErrorCode);
                                                     handlers(this, new ErrorEventArgs(exception));
                                                 }
                                             }, null);
        }

        private void RaiseStateChangedEvent()
        {
            var state = State;

            _logger.Trace("Raising StateChanged event with state {0}", state);

            // make sure we only raise events on the correct context.
            // it's really important to use "Post" here to avoid several potential deadlock scenarios
            _synchronizationContext.Post(o =>
                                             {
                                                 var handlers = StateChanged;
                                                 if (handlers != null)
                                                 {
                                                     handlers(this, new PhoneControllerStateEventArgs(state));
                                                 }
                                             }, null);
        }

        private void RaiseConfigurationChangedEvent()
        {
            _logger.Trace("Raising ConfigurationChanged event");

            // make sure we only raise events on the correct context
            _synchronizationContext.Post(o =>
                                             {
                                                 var handlers = ConfigurationChanged;
                                                 if (handlers != null)
                                                 {
                                                     handlers(this, EventArgs.Empty);
                                                 }
                                             }, null);
        }
    }
}
