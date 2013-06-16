using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace WP8Controller.Controls
{
    public partial class LightIndicator : UserControl
    {
        public static readonly DependencyProperty OnDescriptionProperty =
            DependencyProperty.Register("OnDescription", typeof(string), typeof(LightIndicator), new PropertyMetadata(default(string), OnDescription_Changed));

        private static void OnDescription_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var lightIndicator = d as LightIndicator;
            if (lightIndicator != null)
            {
                lightIndicator.OnText.Text = (string)e.NewValue;
            }
        }

        public string OnDescription
        {
            get { return (string)GetValue(OnDescriptionProperty); }
            set { SetValue(OnDescriptionProperty, value); }
        }

        public static readonly DependencyProperty OffDescriptionProperty =
            DependencyProperty.Register("OffDescription", typeof(string), typeof(LightIndicator), new PropertyMetadata(default(string), OffDescription_Changed));

        private static void OffDescription_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var lightIndicator = d as LightIndicator;
            if (lightIndicator != null)
            {
                lightIndicator.OffText.Text = (string)e.NewValue;
            }
        }

        public string OffDescription
        {
            get { return (string)GetValue(OffDescriptionProperty); }
            set { SetValue(OffDescriptionProperty, value); }
        }
        public static readonly DependencyProperty IsOnProperty =
            DependencyProperty.Register("IsOn", typeof(bool), typeof(LightIndicator), new PropertyMetadata(default(bool), IsOn_Changed));

        private static void IsOn_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var lightIndicator = d as LightIndicator;
            if (lightIndicator != null)
            {
                var isOn = (bool)e.NewValue;
                var newState = isOn ? "On" : "Off";
                VisualStateManager.GoToState(lightIndicator, newState, true);
            }
        }

        public bool IsOn
        {
            get { return (bool)GetValue(IsOnProperty); }
            set { SetValue(IsOnProperty, value); }
        }

        public LightIndicator()
        {
            //Required to initialize variables
            InitializeComponent();

            var newState = IsOn ? "On" : "Off";
            VisualStateManager.GoToState(this, newState, false);
        }


        private void LayoutRoot_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            IndicatorEllipse.Height = LayoutRoot.ActualHeight;
            IndicatorEllipse.Width = LayoutRoot.ActualHeight;
        }
    }
}
