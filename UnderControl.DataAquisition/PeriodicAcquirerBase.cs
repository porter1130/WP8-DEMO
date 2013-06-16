using System;
using System.Collections.Generic;
using System.Threading;

namespace UnderControl.DataAcquisition
{
    /// <summary>
    /// A base class that periodically invokes actions that acquire data.
    /// The benefit of using a base class for this is that a variety of actions can happen
    /// on the same timer/synchronized.
    /// </summary>
    internal abstract class PeriodicAcquirerBase
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private static readonly Timer _timer = new Timer(Timer_Tick);
        private static readonly List<Action> _notificationReceivers = new List<Action>();

        /// <summary>
        /// Initializes the <see cref="PeriodicAcquirerBase"/> class.
        /// </summary>
        static PeriodicAcquirerBase()
        {
            var timeSpan = TimeSpan.FromMilliseconds(1000.0 / 30.0);
            _timer.Change(timeSpan, timeSpan);
        }

        private static void Timer_Tick(object state)
        {
            lock (_notificationReceivers)
            {
                foreach (var receiver in _notificationReceivers)
                {
                    receiver();
                }
            }
        }

        /// <summary>
        /// Enlists the specified action to be called periodically.
        /// </summary>
        /// <param name="periodicAction">The action to be called periodically.</param>
        protected void Enlist(Action periodicAction)
        {
            _logger.Trace("Enlisting an action");

            lock (_notificationReceivers)
            {
                if (!_notificationReceivers.Contains(periodicAction))
                {
                    _notificationReceivers.Add(periodicAction);
                }
            }
        }

        /// <summary>
        /// Removes the specified periodic action.
        /// </summary>
        /// <param name="periodicAction">The periodic action to be removed.</param>
        protected void Remove(Action periodicAction)
        {
            _logger.Trace("Removing an action");

            lock (_notificationReceivers)
            {
                if (_notificationReceivers.Contains(periodicAction))
                {
                    _notificationReceivers.Remove(periodicAction);
                }
            }
        }
    }
}
