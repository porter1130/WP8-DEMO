using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WP8Controller.Controls
{
    public class TextInputHelper
    {
        public event EventHandler<TextInputEventArgs> TextInputFinished;

        public void RaiseTextInputFinishedEvent(string text)
        {
            var handlers = TextInputFinished;
            if (handlers != null)
            {
                handlers(this, new TextInputEventArgs(text));
            }
        }
    }
}
