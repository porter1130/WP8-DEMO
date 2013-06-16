using System;
using System.Net;
using System.Threading;
using UnderControl.Communication.Sockets;
using UnderControl.Shared;
using UnderControl.Shared.Channels;
using UnderControl.Shared.ControlCommands;
using UnderControl.Shared.Data;

namespace UnderControl.Communication
{
    /// <summary>
    /// An implementation of the phone controller server side.
    /// </summary>
    public sealed class PhoneControllerServer
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private SynchronizationContext _synchronizationContext;
        private object _locker = new object();

        private ICommunicationFactory _communicationFactory;
        private PhoneControllerState _state;
        private MulticastListener _broadCastListener;
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
        /// Raised when the state of the controller has changed.
        /// </summary>
        /// <remarks>
        /// This event is guaranteed to be raised on the synchronization context of the thread that created the controller.
        /// </remarks>
        public event EventHandler<PhoneControllerStateEventArgs> StateChanged;

        /// <summary>
        /// Raised when a data message has been received.
        /// </summary>
        /// <remarks>
        /// This event is guaranteed to be raised on the synchronization context of the thread that created the controller.
        /// </remarks>
        public event EventHandler<DataMessageEventArgs> DataMessageReceived;

        /// <summary>
        /// Raised when a control command was sent successfully.
        /// </summary>
        /// <remarks>
        /// This event is guaranteed to be raised on the synchronization context of the thread that created the controller.
        /// </remarks>
        public event EventHandler<EventArgs> ControlCommandSent;

        /// <summary>
        /// This is a shortcut event for using the <c>StateChanged</c> event 
        /// and checking the <c>State</c> property for a <c>PhoneControllerState</c> of <c>Error</c>.
        /// In contrast to the <c>StateChanged</c> event, this event provides detailed technical
        /// information on the actual error that occurred.
        /// </summary>
        /// <remarks>
        /// This event is guaranteed to be raised on the synchronization context of the thread that created the controller.
        /// </remarks>
        public event EventHandler<ErrorEventArgs> Error;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhoneControllerServer"/> class.
        /// </summary>
        public PhoneControllerServer()
        {
            _communicationFactory = new DefaultCommunicationFactory();

            CreatePhoneControllerServer();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PhoneControllerServer"/> class using the given communication factory.
        /// </summary>
        /// <param name="communicationFactory">The communication factory used to create the interal communication objects.</param>
        public PhoneControllerServer(ICommunicationFactory communicationFactory)
        {
            if (communicationFactory == null)
            {
                throw new ArgumentNullException("communicationFactory");
            }

            _communicationFactory = communicationFactory;

            CreatePhoneControllerServer();
        }

        private void CreatePhoneControllerServer()
        {
            // capture the current synchronization context
            _synchronizationContext = SynchronizationContext.Current;
            if (_synchronizationContext == null)
            {
                // create a default implementation
                _synchronizationContext = new SynchronizationContext();
            }

            // create all required networking objects
            _broadCastListener = new MulticastListener();

            _controlChannel = _communicationFactory.CreateControlChannel();
            _controlChannel.ClientAccepted += ControlChannel_ClientAccepted;
            _controlChannel.ConnectionClosed += ControlChannel_ConnectionClosed;
            _controlChannel.ControlCommandSent += ControlChannel_ControlCommandSent;
            _controlChannel.Error += ControlChannel_Error;

            _dataChannel = _communicationFactory.CreateDataChannel();
            _dataChannel.ClientAccepted += DataChannel_ClientAccepted;
            _dataChannel.ConnectionClosed += DataChannel_ConnectionClosed;
            _dataChannel.Error += DataChannel_Error;
            _dataChannel.DataMessageReceived += DataChannel_DataMessageReceived;

            // set initial state
            State = PhoneControllerState.Created;
        }

        /// <summary>
        /// Initializes the controller with the given local IP address and starts waiting for incoming client connections.
        /// </summary>
        /// <param name="localAddress">The local IP address to use for the controller.</param>
        public void Initialize(IPAddress localAddress)
        {
            try
            {
                // make sure the state transitioning happens atomically
                lock (_locker)
                {
                    var state = State;
                    if (state != PhoneControllerState.Created && state != PhoneControllerState.Closed)
                    {
                        throw new InvalidOperationException("Can only initialize from Created or Closed state (current state " + State + ").");
                    }

                    _logger.Trace("Initializing with local address {0}", localAddress);

                    _broadCastListener.StartListening();

                    _controlChannel.ListenAsync(localAddress.ToString(), Constants.CommunicationControlChannelPort);
                    _dataChannel.ListenAsync(localAddress.ToString(), Constants.CommunicationDataChannelPort);

                    State = PhoneControllerState.Initialized;
                }
            }
            catch (Exception ex)
            {
                throw new PhoneControllerException("Error while starting the control and data channels: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Asynchronously sends a control command to the phone client.
        /// </summary>
        /// <param name="command">The control command to send.</param>
        public void SendCommandAsync(IControlCommand command)
        {
            try
            {
                // let the control channel handle send requests one at a time
                lock (_locker)
                {
                    Guard();

                    _logger.Trace("Sending control command: {0}, {1}", command.DataType, command.Action);

                    _controlChannel.Send(command);
                }
            }
            catch (Exception ex)
            {
                throw new PhoneControllerException("Error while send control command: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Shuts down the controller. After a successful shutdown, the controller transitions to the <c>Closed</c>
        /// state and can be re-initialized later.
        /// </summary>
        public void Shutdown()
        {
            try
            {
                // avoid that multiple threads invoke this in parallel
                lock (_locker)
                {
                    var state = State;

                    // shortcut
                    if (state == PhoneControllerState.Created)
                    {
                        State = PhoneControllerState.Closed;
                        return;
                    }

                    // only proceed when valid
                    if (state == PhoneControllerState.Closing || state == PhoneControllerState.Closed)
                    {
                        return;
                    }

                    _logger.Trace("Shutting down");

                    State = PhoneControllerState.Closing;

                    if (_broadCastListener != null)
                    {
                        _broadCastListener.Shutdown();
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

        private void DataChannel_ClientAccepted(object sender, EventArgs e)
        {
            // avoid multiple threads accessing the channels
            lock (_locker)
            {
                _logger.Trace("Received ClientAccepted event from data channel");

                if (_dataChannel.IsConnected && _controlChannel.IsConnected)
                {
                    State = PhoneControllerState.Ready;
                }
            }
        }

        private void DataChannel_ConnectionClosed(object sender, EventArgs e)
        {
            _logger.Trace("Received ConnectionClosed event from data channel");

            Shutdown();
        }

        private void DataChannel_DataMessageReceived(object sender, DataMessageEventArgs e)
        {
            // do not report data if we are not ready
            // (e.g. data may still drop in if the client version does not match the 
            // expected version and we've transitioned to Error state already,
            // and until the consumer of the library does something like shutting down).
            if (State != PhoneControllerState.Ready)
            {
                return;
            }

            _logger.Trace("Received DataMessageReceived event from data channel");

            // check if we have a controller info data message
            if (e.DataMessage.DataType == DataType.ControllerInfo)
            {
                // make sure that the version information fits what we expect
                var controllerInfoData = e.DataMessage as ControllerInfoData;
                if (controllerInfoData.ClientVersion != Constants.ControllerVersion)
                {
                    _logger.Trace("Client controller version {0} does not match expected version {1}.", controllerInfoData.ClientVersion, Constants.ControllerVersion);

                    State = PhoneControllerState.Error;
                    var error = new ControllerVersionMismatchException(Constants.ControllerVersion, controllerInfoData.ClientVersion);
                    RaiseErrorEvent(error);
                    return;
                }
            }

            // simply pass through
            RaiseDataMessageReceivedEvent(e.DataMessage);
        }

        private void DataChannel_Error(object sender, NetworkErrorEventArgs e)
        {
            var state = State;
            if (state == PhoneControllerState.Closed || state == PhoneControllerState.Closing || state == PhoneControllerState.Error)
            {
                // ignore follow-up errors after we're closed or already in error state
                return;
            }

            _logger.Trace("Received Error event from data channel");

            State = PhoneControllerState.Error;
            RaiseErrorEvent(e);
        }

        private void ControlChannel_ClientAccepted(object sender, EventArgs e)
        {
            lock (_locker)
            {
                _logger.Trace("Received ClientAccepted event from control channel");

                if (_dataChannel.IsConnected && _controlChannel.IsConnected)
                {
                    State = PhoneControllerState.Ready;
                }
            }
        }

        private void ControlChannel_ConnectionClosed(object sender, EventArgs e)
        {
            _logger.Trace("Received ConnectionClosed event from control channel");

            Shutdown();
        }

        private void ControlChannel_ControlCommandSent(object sender, EventArgs e)
        {
            _logger.Trace("Received ControlCommandSent event from control channel");

            RaiseControlCommandSentEvent();
        }

        private void ControlChannel_Error(object sender, NetworkErrorEventArgs e)
        {
            var state = State;
            if (state == PhoneControllerState.Closed || state == PhoneControllerState.Closing || state == PhoneControllerState.Error)
            {
                // ignore follow-up errors after we're closed or already in error state
                return;
            }

            _logger.Trace("Received Error event from control channel");

            State = PhoneControllerState.Error;
            RaiseErrorEvent(e);
        }

        private void RaiseStateChangedEvent()
        {
            _logger.Trace("Raising event StateChanged with {0}", State);

            // it's important to "Post" here, to avoid multiple potential deadlock scenarios
            _synchronizationContext.Post(o =>
                                             {
                                                 var handlers = StateChanged;
                                                 if (handlers != null)
                                                 {
                                                     handlers(this, new PhoneControllerStateEventArgs(State));
                                                 }
                                             }, null);
        }

        private void RaiseControlCommandSentEvent()
        {
            _logger.Trace("Raising event ControlCommandSent");

            _synchronizationContext.Post(o =>
                                             {
                                                 var handlers = ControlCommandSent;
                                                 if (handlers != null)
                                                 {
                                                     handlers(this, EventArgs.Empty);
                                                 }
                                             }, null);
        }

        private void RaiseDataMessageReceivedEvent(IDataMessage dataMessage)
        {
            _logger.Trace("Raising event DataMessageReceived");

            _synchronizationContext.Post(o =>
                                             {
                                                 var handlers = DataMessageReceived;
                                                 if (handlers != null)
                                                 {
                                                     handlers(this, new DataMessageEventArgs(dataMessage));
                                                 }
                                             }, null);
        }

        private void RaiseErrorEvent(PhoneControllerException ex)
        {
            _logger.Trace("Raising Error event with message: {0}", ex.Message);

            // make sure we only raise events on the correct context
            _synchronizationContext.Post(o =>
            {
                var handlers = Error;
                if (handlers != null)
                {
                    handlers(this, new ErrorEventArgs(ex));
                }
            }, null);
        }

        private void RaiseErrorEvent(NetworkErrorEventArgs e)
        {
            _logger.Trace("Raising event Error");

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

        private void Guard()
        {
            if (State != PhoneControllerState.Ready)
            {
                throw new InvalidOperationException("Not connected to the client (control channel missing).");
            }
        }
    }
}
