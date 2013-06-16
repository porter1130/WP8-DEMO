using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Coding4Fun.Toolkit.Controls;

namespace WP8Controller.Controls
{
    public class CancelOnlyMessagePrompt:MessagePrompt
    {
        public CancelOnlyMessagePrompt() : base()
        {
            //a bit hacky, but the base implementation
            //does not allow a different way of accessing this button
            var okButton = ActionPopUpButtons[0];
            okButton.Visibility=Visibility.Collapsed;

            IsCancelVisible = true;
        }
    }
}
