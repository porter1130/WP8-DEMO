using System;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;

namespace PCController
{
    public class Configuration : INotifyPropertyChanged
    {
        private const string ConfigurationFileName = "Config.dat";

        public const string RemoteUpdateSource = "http://www.pitorque.de/downloads/PCControllerVersionInfo.xml";

        private bool _useAccelerometer;
        private bool _useTouchInput;
        private bool _touchInputIsRelative;
        private bool _useTapGestures;
        private bool _useDoubleTapGestures;
        private bool _useHoldGestures;
        private bool _useFlickGestures;
        private bool _useDragGestures;
        private bool _useTextInput;
        private bool _enableTracing;
        private string _tracingEndpointAddress;

        public bool UseTouchInput
        {
            get
            {
                return _useTouchInput;
            }
            set
            {
                if (_useTouchInput != value)
                {
                    _useTouchInput = value;

                    RaisePropertyChanged("UseTouchInput");
                }
            }
        }

        public bool TouchInputIsRelative
        {
            get
            {
                return _touchInputIsRelative;
            }
            set
            {
                if (_touchInputIsRelative != value)
                {
                    _touchInputIsRelative = value;

                    RaisePropertyChanged("TouchInputIsRelative");
                }
            }
        }

        public bool UseTapGestures
        {
            get
            {
                return _useTapGestures;
            }
            set
            {
                if (_useTapGestures != value)
                {
                    _useTapGestures = value;

                    RaisePropertyChanged("UseTapGestures");
                }
            }
        }

        public bool UseDoubleTapGestures
        {
            get
            {
                return _useDoubleTapGestures;
            }
            set
            {
                if (_useDoubleTapGestures != value)
                {
                    _useDoubleTapGestures = value;

                    RaisePropertyChanged("UseDoubleTapGestures");
                }
            }
        }

        public bool UseHoldGestures
        {
            get
            {
                return _useHoldGestures;
            }
            set
            {
                if (_useHoldGestures != value)
                {
                    _useHoldGestures = value;

                    RaisePropertyChanged("UseHoldGestures");
                }
            }
        }

        public bool UseFlickGestures
        {
            get
            {
                return _useFlickGestures;
            }
            set
            {
                if (_useFlickGestures != value)
                {
                    _useFlickGestures = value;

                    RaisePropertyChanged("UseFlickGestures");
                }
            }
        }

        public bool UseDragGestures
        {
            get
            {
                return _useDragGestures;
            }
            set
            {
                if (_useDragGestures != value)
                {
                    _useDragGestures = value;

                    RaisePropertyChanged("UseDragGestures");
                }
            }
        }

        public bool UseAccelerometer
        {
            get
            {
                return _useAccelerometer;
            }
            set
            {
                if (_useAccelerometer != value)
                {
                    _useAccelerometer = value;

                    RaisePropertyChanged("UseAccelerometer");
                }
            }
        }

        public bool UseTextInput
        {
            get
            {
                return _useTextInput;
            }
            set
            {
                if (_useTextInput != value)
                {
                    _useTextInput = value;

                    RaisePropertyChanged("UseTextInput");
                }
            }
        }

        public bool EnableTracing
        {
            get
            {
                return _enableTracing;
            }
            set
            {
                if (_enableTracing != value)
                {
                    _enableTracing = value;

                    RaisePropertyChanged("EnableTracing");
                }
            }
        }

        public string TracingEndpointAddress
        {
            get
            {
                return _tracingEndpointAddress;
            }
            set
            {
                if (_tracingEndpointAddress != value)
                {
                    _tracingEndpointAddress = value;

                    RaisePropertyChanged("TracingEndpointAddress");
                }
            }
        }

        public Configuration()
        {
            // these are the default values
            UseAccelerometer = true;
            UseTouchInput = true;
            TouchInputIsRelative = true;
            UseTapGestures = true;
            UseDoubleTapGestures = true;
            UseHoldGestures = true;
            UseFlickGestures = false;
            UseDragGestures = true;
            UseTextInput = true;
            EnableTracing = false;
            TracingEndpointAddress = "http://192.168.2.62:5000/LogReceiver.svc";
        }

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void Load()
        {
            try
            {
                using (var store = IsolatedStorageFile.GetUserStoreForAssembly())
                {
                    if (store.FileExists(ConfigurationFileName))
                    {
                        using (var file = store.OpenFile(ConfigurationFileName, FileMode.Open))
                        {
                            using (var reader = new BinaryReader(file, Encoding.UTF8))
                            {
                                UseAccelerometer = reader.ReadBoolean();
                                UseTouchInput = reader.ReadBoolean();
                                TouchInputIsRelative = reader.ReadBoolean();
                                UseTapGestures = reader.ReadBoolean();
                                UseDoubleTapGestures = reader.ReadBoolean();
                                UseHoldGestures = reader.ReadBoolean();
                                UseFlickGestures = reader.ReadBoolean();
                                UseDragGestures = reader.ReadBoolean();
                                UseTextInput = reader.ReadBoolean();
                                EnableTracing = reader.ReadBoolean();
                                TracingEndpointAddress = reader.ReadString();
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignore loading errors
            }
        }

        public void Save()
        {
            using (var store = IsolatedStorageFile.GetUserStoreForAssembly())
            {
                if (store.FileExists(ConfigurationFileName))
                {
                    store.DeleteFile(ConfigurationFileName);
                }

                using (var file = store.CreateFile(ConfigurationFileName))
                {
                    using (var writer = new BinaryWriter(file, Encoding.UTF8))
                    {
                        writer.Write(UseAccelerometer);
                        writer.Write(UseTouchInput);
                        writer.Write(TouchInputIsRelative);
                        writer.Write(UseTapGestures);
                        writer.Write(UseDoubleTapGestures);
                        writer.Write(UseHoldGestures);
                        writer.Write(UseFlickGestures);
                        writer.Write(UseDragGestures);
                        writer.Write(UseTextInput);
                        writer.Write(EnableTracing);
                        writer.Write(TracingEndpointAddress ?? string.Empty);
                    }
                }
            }
        }
    }
}
