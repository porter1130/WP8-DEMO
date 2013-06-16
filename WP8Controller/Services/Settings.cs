using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Phone.Shell;
using YourLastOptionsDialog.Attributes;

namespace WP8Controller.Services
{
    /// <summary>
    /// A simple class that stores and handles basic application and user settings.
    /// </summary>
    public class Settings
    {
        private bool _isLockScreenDeactivated;

        /// <summary>
        /// Gets or sets a value indicating whether the lock screen is deactivated or not.
        /// </summary>
        /// <value>
        ///     <c>true</c> if the lock screen is deactivated; otherwise, <c>false</c>.
        /// </value>
        [OptionBoolean(DisplayName = "Deactivate Lock Screen",
            Description = "Use this option if you're only using sensor input (e.g. the accelerometer) to prevent the application from shutting down automatically.",
            UserMustConfirmActivation = true,
            ActivationPrompt = "Using this option may decrease battery life when you leave the phone unattended while the app is running. Continue?")]
        public bool IsLockScreenDeactivated
        {
            get { return _isLockScreenDeactivated; }
            set
            {
                if (_isLockScreenDeactivated != value)
                {
                    _isLockScreenDeactivated = value;

                    PhoneApplicationService.Current.UserIdleDetectionMode = _isLockScreenDeactivated
                                                                                ? IdleDetectionMode.Disabled
                                                                                : IdleDetectionMode.Enabled;
                }
            }
        }

        public Settings()
        {
            //these are our application defaults
            IsLockScreenDeactivated = false;
        }
    }
}
