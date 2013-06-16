using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace WP8Controller.Controls
{
    public partial class GrabberControl : UserControl
    {
        private const double MinGrabberWidth = 15.0;
        private bool _ignoreManipulationCompleted;
        /// <summary>
        /// Occurs when the user started interacting with the grabber.
        /// </summary>
        public event EventHandler<EventArgs> UserInteractionStarted;

        /// <summary>
        /// Occurs when the user ended interacting with the grabber.
        /// The state of the grabber can be both <c>Activated</c> or <c>Deactivated</c>.
        /// </summary>
        public event EventHandler<EventArgs> UserInteractionEnded;

        /// <summary>
        /// Occurs when the user interaction has reached the activation threshold,
        /// or when the grabber was activated by other means (e.g. programmatically).
        /// </summary>
        public event EventHandler<EventArgs> Activated;

        /// <summary>
        /// Occurs when the user either has tapped the grabber in <c>Activated</c> state,
        /// or if the grabber was deactivated by other means (e.g. programmatically).
        /// </summary>
        public event EventHandler<EventArgs> Deactivated;

        #region Properties

        public static readonly DependencyProperty UserInteractionStartedCommandProperty =
            DependencyProperty.Register("UserInteractionStartedCommand", typeof(ICommand), typeof(GrabberControl), new PropertyMetadata(default(ICommand)));

        /// <summary>
        /// Gets or sets an <c>ICommand</c> implementation that is executed when the user starts interacting with the grabber.
        /// This is an alternative to handling the <see cref="UserInteractionStarted"/> event.
        /// </summary>
        public ICommand UserInteractionStartedCommand
        {
            get { return (ICommand)GetValue(UserInteractionStartedCommandProperty); }
            set { SetValue(UserInteractionStartedCommandProperty, value); }
        }

        public static readonly DependencyProperty UserInteractionEndedCommandProperty =
            DependencyProperty.Register("UserInteractionEndedCommand", typeof(ICommand), typeof(GrabberControl), new PropertyMetadata(default(ICommand)));

        /// <summary>
        /// Gets or sets an <c>ICommand</c> implementation that is executed when the user ends interacting with the grabber.
        /// This is an alternative to handling the <see cref="UserInteractionEnded"/> event.
        /// </summary>
        public ICommand UserInteractionEndedCommand
        {
            get { return (ICommand)GetValue(UserInteractionEndedCommandProperty); }
            set { SetValue(UserInteractionEndedCommandProperty, value); }
        }
        /// <summary>
        /// Identifies the <c>ActivatedCommand</c> dependency property.
        /// </summary>
        public static readonly DependencyProperty ActivatedCommandProperty =
            DependencyProperty.Register("ActivatedCommand", typeof(ICommand), typeof(GrabberControl), new PropertyMetadata(default(ICommand)));

        public ICommand ActivatedCommand
        {
            get { return (ICommand)GetValue(ActivatedCommandProperty); }
            set { SetValue(ActivatedCommandProperty, value); }
        }

        /// <summary>
        /// Identifies the <c>DeactivatedCommand</c> dependency property.
        /// </summary>
        public static readonly DependencyProperty DeactivatedCommandProperty =
            DependencyProperty.Register("DeactivatedCommand", typeof(ICommand), typeof(GrabberControl), new PropertyMetadata(default(ICommand)));

        public ICommand DeactivatedCommand
        {
            get { return (ICommand)GetValue(DeactivatedCommandProperty); }
            set { SetValue(DeactivatedCommandProperty, value); }
        }

        /// <summary>
        /// Identifies the <c>GrabberText</c> dependency property
        /// </summary>
        public static readonly DependencyProperty GrabberTextProperty =
            DependencyProperty.Register("GrabberText", typeof(string), typeof(GrabberControl), new PropertyMetadata("GRAB HERE", GrabberText_Changed));

        private static void GrabberText_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as GrabberControl;
            if (control != null)
            {
                control.GrabberTextBlock.Text = (string)e.NewValue;
            }
        }

        public string GrabberText
        {
            get { return (string)GetValue(GrabberTextProperty); }
            set { SetValue(GrabberTextProperty, value); }
        }

        /// <summary>
        /// Identifies the <c>ExplanationText</c> dependency property.
        /// </summary>
        public static readonly DependencyProperty ExplanationTextProperty =
            DependencyProperty.Register("ExplanationText", typeof(string), typeof(GrabberControl), new PropertyMetadata(default(string), ExplanationText_Changed));

        private static void ExplanationText_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as GrabberControl;
            if (control != null)
            {
                control.ExplanationTextBlock.Text = (string)e.NewValue;
            }
        }

        public string ExplanationText
        {
            get { return (string)GetValue(ExplanationTextProperty); }
            set { SetValue(ExplanationTextProperty, value); }
        }

        /// <summary>
        /// Identifies the <c>ActivationThreshold</c> dependency property
        /// </summary>
        public static readonly DependencyProperty ActivationThresholdProperty =
            DependencyProperty.Register("ActivationThreshold", typeof(double), typeof(GrabberControl), new PropertyMetadata(400.0, ActivationThreshold_Changed));

        private static void ActivationThreshold_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((double)e.NewValue <= MinGrabberWidth)
            {
                var control = d as GrabberControl;
                if (control != null)
                {
                    control.ActivationThreshold = MinGrabberWidth + 1.0;
                }
            }
        }

        public double ActivationThreshold
        {
            get { return (double)GetValue(ActivationThresholdProperty); }
            set { SetValue(ActivationThresholdProperty, value); }
        }

        public static readonly DependencyProperty IsActivatedProperty =
            DependencyProperty.Register("IsActivated", typeof(bool), typeof(GrabberControl), new PropertyMetadata(default(bool), IsActivated_Changed));

        private static void IsActivated_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = d as GrabberControl;
            if (obj != null)
            {
                var newValue = (bool)e.NewValue;
                if (newValue)
                {
                    //switched from false to true
                    obj.HandleActivation();
                }
                else
                {
                    //switched from true to false
                    obj.HandleDeactivation();
                }
            }
        }

        private void HandleDeactivation()
        {
            //reset the visual appearance
            DragBorder.Width = MinGrabberWidth;

            //execute command, if applicable
            ExecuteCommand(DeactivatedCommandProperty);

            //raise event
            var handlers = Deactivated;
            if (handlers != null)
            {
                handlers(this, EventArgs.Empty);
            }
        }

        private void HandleActivation()
        {
            //execute command, if applicable
            ExecuteCommand(ActivatedCommandProperty);

            //raise event
            var handlers = Activated;
            if (handlers != null)
            {
                handlers(this, EventArgs.Empty);
            }
        }

        private void ExecuteCommand(DependencyProperty property)
        {
            //get command and execute
            var command = (ICommand)GetValue(property);
            if (command != null)
            {
                if (command.CanExecute(null))
                {
                    command.Execute(null);
                }
            }
        }

        public bool IsActivated
        {
            get { return (bool)GetValue(IsActivatedProperty); }
            set { SetValue(IsActivatedProperty, value); }
        }
        #endregion
        public GrabberControl()
        {
            InitializeComponent();

            IsEnabledChanged += GrabberControl_IsEnabledChanged;
        }

        void GrabberControl_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            VisualStateManager.GoToState(this, (bool)e.NewValue ? "Enabled" : "Disabled", true);
        }

        private void DragBorder_ManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            if (_ignoreManipulationCompleted)
            {
                //reset for next time and ignore
                _ignoreManipulationCompleted = false;
                return;
            }

            //if the user has stopped interacting before the activation threshold was reached,
            //reset the visual appearance
            if (!IsActivated)
            {
                // reset the visual appearance
                DragBorder.Width = MinGrabberWidth;
            }

            //raise event and/or command
            HandleUserInteractionEnded();
        }

        private void HandleUserInteractionEnded()
        {
            //execute command, if applicable
            ExecuteCommand(UserInteractionEndedCommandProperty);

            //raise event
            var handlers = UserInteractionEnded;
            if (handlers != null)
            {
                handlers(this, EventArgs.Empty);
            }
        }

        private void DragBorder_ManipulationDelta(object sender, System.Windows.Input.ManipulationDeltaEventArgs e)
        {

        }

        private void DragBorder_ManipulationStarted(object sender, System.Windows.Input.ManipulationStartedEventArgs e)
        {
            //if we are already activated, do not process further interaction
            if (IsActivated)
            {
                _ignoreManipulationCompleted = true;
                e.Complete();
                return;
            }

            HandleUserInteractionStarted();
        }

        private void HandleUserInteractionStarted()
        {
            //execute command, if applicable
            ExecuteCommand(UserInteractionStartedCommandProperty);

            //raise event
            var handlers = UserInteractionStarted;
            if (handlers != null)
            {
                handlers(this, EventArgs.Empty);
            }
        }

        private void DragBorder_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            // a tap simply triggers deactivation (if we're activated)
            IsActivated = false;
        }
    }
}
