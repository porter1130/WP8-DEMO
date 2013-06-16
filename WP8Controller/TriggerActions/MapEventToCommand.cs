using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace WP8Controller.TriggerActions
{
    public class MapEventToCommand : MapEventToCommandBase<EventArgs>
    {
    } 

    public class MapKeyEventToCommand:MapEventToCommandBase<KeyEventArgs>
    {
        
    }

    public abstract class MapEventToCommandBase<TEventArgsType> : TriggerAction<FrameworkElement>
        where TEventArgsType : EventArgs
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(MapEventToCommandBase<TEventArgsType>), new PropertyMetadata(null, OnCommandPropertyChanged));

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(MapEventToCommandBase<TEventArgsType>), new PropertyMetadata(null, OnCommandParameterPropertyChanged));

        private static void OnCommandParameterPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var invokeCommand = d as MapEventToCommand;
            if (invokeCommand != null)
            {
                invokeCommand.SetValue(CommandParameterProperty, e.NewValue);
            }  
        }

        public object CommandParameter
        {
            get { return (object) GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        private static void OnCommandPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var invokeCommand = d as MapEventToCommand;
            if (invokeCommand != null)
            {
                invokeCommand.SetValue(CommandProperty, e.NewValue);
            }  
        }

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }


        protected override void Invoke(object parameter)
        {
            if (this.Command == null)
            {
                return;
            }

            var eventInfo = new EventInformation<TEventArgsType>
            {
                EventArgs = parameter as TEventArgsType,
                Sender = this.AssociatedObject,
                CommandArgument = GetValue(CommandParameterProperty)
            };

            if (this.Command.CanExecute(eventInfo))
            {
                this.Command.Execute(eventInfo);
            }  
        }
    }
}
