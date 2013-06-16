
using System;
using System.Windows;
using Microsoft.Phone.Controls;

namespace UnderControl.DataAcquisition
{
    /// <summary>
    /// A helper class that collects and exposes certain device information from a Windows Phone device.
    /// For this class to be used correctly, it's <c>Initialize</c> method needs to be called once,
    /// for example during the phone application's initialization.
    /// </summary>
    public sealed class DeviceInfo
    {
        /// <summary>
        /// The singleton instance of the class; <c>null</c> until the <c>Initialize</c> method has been called.
        /// </summary>
        public static DeviceInfo Current;

        private static object _locker = new object();

        private PageOrientation _orientation;
        private double _physicalScreenWidth;
        private double _physicalScreenHeight;

        /// <summary>
        /// Gets the current device and screen orientation.
        /// </summary>
        public PageOrientation Orientation
        {
            get
            {
                lock (_locker)
                {
                    return _orientation;
                }
            }
            private set
            {
                // the locking is done outside the setter for performance reasons, see below
                _orientation = value;
            }
        }

        /// <summary>
        /// Gets the physical width of the screen. The physical width always is smaller than the physical height
        /// and typically has a value of 480.0.
        /// </summary>
        /// <value>
        /// The physical width of the screen.
        /// </value>
        public double PhysicalScreenWidth
        {
            get
            {
                lock (_locker)
                {
                    return _physicalScreenWidth;
                }
            }
            private set
            {
                // the locking is done outside the setter for performance reasons, see below
                _physicalScreenWidth = value;

                // width/height are switched because of portrait/landscape mode
                LogicalScreenHeight = value;
            }
        }

        /// <summary>
        /// Gets the physical height of the screen. The physical height always is larger than the physical width
        /// and typically has a value of 800.0.
        /// </summary>
        /// <value>
        /// The physical height of the screen.
        /// </value>
        public double PhysicalScreenHeight
        {
            get
            {
                lock (_locker)
                {
                    return _physicalScreenHeight;
                }
            }
            private set
            {
                // the locking is done outside the setter for performance and consistency reasons, see below
                _physicalScreenHeight = value;

                // width/height are switched because of portrait/landscape mode
                LogicalScreenWidth = value;
            }
        }

        /// <summary>
        /// Gets the logical width of the screen. Since the library only supports landscape orientation,
        /// this returns the value of <c>PhysicalScreenHeight</c>.
        /// </summary>
        /// <value>
        /// The logical width of the screen.
        /// </value>
        public double LogicalScreenWidth
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the logical height of the screen. Since the library only supports landscape orientation,
        /// this returns the value of <c>PhysicalScreenWidth</c>.
        /// </summary>
        /// <value>
        /// The logical height of the screen.
        /// </value>
        public double LogicalScreenHeight
        {
            get;
            private set;
        }

        private DeviceInfo()
        {
        }

        /// <summary>
        /// Initializes the class, and in particular the singleton instance.
        /// If this method is not called from the UI thread of the application, the invocation
        /// is automatically scheduled to that thread using the dispatcher.
        /// </summary>
        public static void Initialize()
        {
            // we need a separate initialization method that is invoked manually to avoid issues
            // with cross-threading and also the order of initialization of objects on the phone.
            // This initialize method allows the object to be created and set up
            // correctly e.g. from the initialization of the application.
            if (!Application.Current.RootVisual.CheckAccess())
            {
                Application.Current.RootVisual.Dispatcher.BeginInvoke(Initialize);
                return;
            }

            lock (_locker)
            {
                if (Current == null)
                {
                    Current = new DeviceInfo();
                }
                else
                {
                    // no second initialization
                    return;
                }
            }

            // hook the orientation changed event
            var frame = Application.Current.RootVisual as PhoneApplicationFrame;
            if (frame == null)
            {
                throw new InvalidOperationException("The root visual of the application is not a PhoneApplicationFrame");
            }

            // hook the event
            frame.OrientationChanged += (o, e) => Current.UpdateProperties(); // always happens on the correct thread

            // initial update of the properties
            Current.UpdateProperties();
        }

        private void UpdateProperties()
        {
            // this is the only place where we update the properties
            // => lock "all at once" to make sure all are updated consistently and performant
            lock (_locker)
            {
                var frame = Application.Current.RootVisual as PhoneApplicationFrame;
                if (frame != null)
                {
                    Orientation = frame.Orientation;
                }

                PhysicalScreenWidth = Application.Current.Host.Content.ActualWidth;
                PhysicalScreenHeight = Application.Current.Host.Content.ActualHeight;
            }
        }
    }
}
