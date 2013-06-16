using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WP8Controller.Controls
{
    public class TextInputEventArgs : EventArgs
    {
        /// <summary>
        /// The text that is the result of the input, or <c>null</c> if there
        /// was no result
        /// </summary>
        public string Text { get; private set; }

        public TextInputEventArgs(string text)
        {
            Text = text;
        }
    }
}
