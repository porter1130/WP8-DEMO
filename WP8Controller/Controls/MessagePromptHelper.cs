using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Coding4Fun.Toolkit.Controls;

namespace WP8Controller.Controls
{
    public class MessagePromptHelper
    {
        public const string ProgrammaticallyDismissedToken = "ProgrammaticallyDismissedToken";

        private CancelOnlyMessagePrompt _prompt;
        private bool _programmaticallyDismissed;

        public event EventHandler<PopUpEventArgs<string, PopUpResult>> PromptClosed;

        public bool IsActive { get; set; }

        public void Show(string message)
        {
            IsActive = true;
            _programmaticallyDismissed = false;

            //show input prompt from the coding4fun toolkit
            _prompt = new CancelOnlyMessagePrompt();
            _prompt.Message = message;
            _prompt.Title = string.Empty;
            _prompt.Completed += MessagePrompt_Completed;
            _prompt.Show();
        }

        public void Hide()
        {
            if (_prompt != null)
            {
                _programmaticallyDismissed = true;
                _prompt.Hide();
            }
        }

        void MessagePrompt_Completed(object sender, PopUpEventArgs<string, PopUpResult> popUpEventArgs)
        {
            _prompt = null;
            IsActive = false;
            RaisePromptClosedEvent(popUpEventArgs);
        }

        private void RaisePromptClosedEvent(PopUpEventArgs<string, PopUpResult> args)
        {
            var handlers = PromptClosed;
            if (handlers != null)
            {
                if (_programmaticallyDismissed)
                {
                    args.Result = ProgrammaticallyDismissedToken;
                }

                handlers(this, args);
            }
        }
    }
}
