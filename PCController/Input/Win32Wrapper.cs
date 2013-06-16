using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;

namespace PCController.Input
{
    /// <summary>
    /// A helper class that wraps several Win32 API calls to convenient methods in the managed world.
    /// </summary>
    public static class Win32Wrapper
    {
        private static void SendMouseInput(Win32.MOUSEINPUT mouseInput)
        {
            // create an input structure
            Win32.INPUT input = new Win32.INPUT();
            input.type = Win32.Constants.INPUT_MOUSE;
            input.mi = mouseInput;

            // send
            uint result = Win32.SendInput(1, new[] { input }, Marshal.SizeOf(input));

            // check for errors
            var lastError = Win32.GetLastError();
            if (lastError != 0)
            {
                throw new Exception("Error while sending mouse input: " + lastError);
            }
        }

        /// <summary>
        /// Gets the current mouse cursor position.
        /// </summary>
        /// <returns>A <c>Point</c> containing the current mouse position.</returns>
        public static Point GetCursorPosition()
        {
            Win32.POINT lpPoint;
            Win32.GetCursorPos(out lpPoint);
            return lpPoint;
        }

        /// <summary>
        /// Sets the current cursor position to the passed in coordinates on the virtual desktop.
        /// </summary>
        /// <param name="x">The X coordinate to use.</param>
        /// <param name="y">The Y coordinate to use.</param>
        public static void SetCursorPosition(int x, int y)
        {
            // get normalized values
            double normalizedY;
            double normalizedX;
            NormalizeCoordinates(x, y, out normalizedY, out normalizedX);

            // prepare mouse move input
            Win32.MOUSEINPUT mouseInput = new Win32.MOUSEINPUT();
            mouseInput.dx = (int)Math.Round(normalizedX);
            mouseInput.dy = (int)Math.Round(normalizedY);
            mouseInput.dwFlags = (int)Win32.Constants.MOUSEEVENTF_ABSOLUTE | (int)Win32.Constants.MOUSEEVENTF_VIRTUALDESK | (int)Win32.Constants.MOUSEEVENTF_MOVE;

            SendMouseInput(mouseInput);
        }

        private static void NormalizeCoordinates(int x, int y, out double normalizedY, out double normalizedX)
        {
            // virtual screen size
            var screenWidth = SystemInformation.VirtualScreen.Width;
            var screenHeight = SystemInformation.VirtualScreen.Height;

            // normalized values
            const int range = 65535;
            normalizedX = ((double)range / (double)screenWidth) * (x - SystemInformation.VirtualScreen.Left);
            normalizedY = ((double)range / (double)screenHeight) * (y - SystemInformation.VirtualScreen.Top);
        }

        /// <summary>
        /// Sends mouse wheel input to the operating system, using the given ticks amount.
        /// </summary>
        /// <param name="ticks">The ticks amount to send.</param>
        public static void SendMouseWheel(int ticks)
        {
            // prepare and send mouse wheel data
            Win32.MOUSEINPUT mouseInput = new Win32.MOUSEINPUT();
            mouseInput.dwFlags = (int)Win32.Constants.MOUSEEVENTF_WHEEL;
            mouseInput.mouseData = (uint)ticks * Win32.Constants.WHEEL_DELTA;

            SendMouseInput(mouseInput);
        }

        /// <summary>
        /// Sends mouse left button down input to the operating system.
        /// </summary>
        public static void SendMouseLeftButtonDown()
        {
            // prepare and send mouse left button down
            Win32.MOUSEINPUT mouseInput = new Win32.MOUSEINPUT();
            mouseInput.dwFlags = (int)Win32.Constants.MOUSEEVENTF_ABSOLUTE | (int)Win32.Constants.MOUSEEVENTF_VIRTUALDESK | (int)Win32.Constants.MOUSEEVENTF_LEFTDOWN;

            SendMouseInput(mouseInput);
        }

        /// <summary>
        /// Sends mouse left button up input to the operating system.
        /// </summary>
        public static void SendMouseLeftButtonUp()
        {
            // prepare and send mouse left button up
            Win32.MOUSEINPUT mouseInput = new Win32.MOUSEINPUT();
            mouseInput.dwFlags = (int)Win32.Constants.MOUSEEVENTF_ABSOLUTE | (int)Win32.Constants.MOUSEEVENTF_VIRTUALDESK | (int)Win32.Constants.MOUSEEVENTF_LEFTUP;

            SendMouseInput(mouseInput);
        }

        /// <summary>
        /// Sends mouse left button down input, followed by mouse left button up input, to the operating system.
        /// </summary>
        public static void SendMouseLeftButtonClick()
        {
            SendMouseLeftButtonDown();
            SendMouseLeftButtonUp();
        }

        /// <summary>
        /// Sends two mouse left button click sequences to the operating system.
        /// </summary>
        public static void SendMouseLeftButtonDoubleClick()
        {
            SendMouseLeftButtonClick();
            SendMouseLeftButtonClick();
        }

        /// <summary>
        /// Sends mouse right button down input, followed by mouse right button up input, to the operating system.
        /// </summary>
        public static void SendMouseRightButtonClick()
        {
            // prepare and send mouse left button down
            Win32.MOUSEINPUT mouseInput = new Win32.MOUSEINPUT();
            mouseInput.dwFlags = (int)Win32.Constants.MOUSEEVENTF_ABSOLUTE | (int)Win32.Constants.MOUSEEVENTF_VIRTUALDESK | (int)Win32.Constants.MOUSEEVENTF_RIGHTDOWN;

            SendMouseInput(mouseInput);

            // prepare and send mouse left button up
            mouseInput = new Win32.MOUSEINPUT();
            mouseInput.dwFlags = (int)Win32.Constants.MOUSEEVENTF_ABSOLUTE | (int)Win32.Constants.MOUSEEVENTF_VIRTUALDESK | (int)Win32.Constants.MOUSEEVENTF_RIGHTUP;

            SendMouseInput(mouseInput);
        }

        /// <summary>
        /// Sends text as keyboard input to the operating system, character by character.
        /// </summary>
        /// <remarks>
        /// Use '\r' characters to send enter key strokes. The character '\n' is ignored.
        /// </remarks>
        /// <param name="text">The text to send.</param>
        public static void SendKeyboardInput(string text)
        {
            // create input structs for normal texts
            var textInputs = new Win32.INPUT[2];

            // create two keyboard inputs
            var firstInput = new Win32.INPUT();
            firstInput.type = Win32.Constants.INPUT_KEYBOARD;
            firstInput.ki = new Win32.KEYBDINPUT();
            firstInput.ki.dwFlags = Win32.Constants.KEYEVENTF_UNICODE;
            textInputs[0] = firstInput;

            var secondInput = new Win32.INPUT();
            secondInput.type = Win32.Constants.INPUT_KEYBOARD;
            secondInput.ki = new Win32.KEYBDINPUT();
            secondInput.ki.dwFlags = Win32.Constants.KEYEVENTF_UNICODE | Win32.Constants.KEYEVENTF_KEYUP;
            textInputs[1] = secondInput;

            // create input structs for special/control chars
            var controlCharsInput = new Win32.INPUT[2];

            // create two keyboard inputs
            var firstControlCharInput = new Win32.INPUT();
            firstControlCharInput.type = Win32.Constants.INPUT_KEYBOARD;
            firstControlCharInput.ki = new Win32.KEYBDINPUT();
            firstControlCharInput.ki.dwFlags = Win32.Constants.KEYEVENTF_SCANCODE;
            controlCharsInput[0] = firstControlCharInput;

            var secondControlCharInput = new Win32.INPUT();
            secondControlCharInput.type = Win32.Constants.INPUT_KEYBOARD;
            secondControlCharInput.ki = new Win32.KEYBDINPUT();
            secondControlCharInput.ki.dwFlags = Win32.Constants.KEYEVENTF_SCANCODE | Win32.Constants.KEYEVENTF_KEYUP;
            controlCharsInput[1] = secondControlCharInput;

            // for each character, send input
            for (int i = 0; i < text.Length; i++)
            {
                // treat special characters differently: 
                // send a keyboard scan code for the enter key for each \r character
                if (text[i] == '\r')
                {
                    controlCharsInput[0].ki.wScan = 0x1c;
                    controlCharsInput[1].ki.wScan = 0x1c;
                    Win32.SendInput(2, controlCharsInput, Marshal.SizeOf(firstControlCharInput));
                    continue;
                }
                else if (text[i] == '\n')
                {
                    // ignore this, because we've already processed \r
                    // note: this logic doesn't work for *nix-like input that only sends \n
                    continue;
                }

                // send the unicode character
                textInputs[0].ki.wScan = text[i];
                textInputs[1].ki.wScan = text[i];
                Win32.SendInput(2, textInputs, Marshal.SizeOf(firstInput));
            }
        }
    }
}
