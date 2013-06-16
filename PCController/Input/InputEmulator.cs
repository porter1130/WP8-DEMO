using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using UnderControl.Shared;
using UnderControl.Shared.Data;

namespace PCController.Input
{
    /// <summary>
    /// Emulates mouse and keyboard input, based on what kind of data is 
    /// received from a Windows Phone device.
    /// </summary>
    public class InputEmulator
    {
        private double _controllerDisplayWidth;
        private double _controllerDisplayHeight;
        private float _lastTouchX = -1.0f;
        private float _lastTouchY = -1.0f;

        public bool UseRelativeTouchInput { get; set; }

        public void Process(IDataMessage data)
        {

            //simply dispatch the data to the correct method
            switch (data.DataType)
            {
                case DataType.Accelerometer:
                    Process(data as AccelerometerData);
                    break;
                case DataType.Touch:
                    Process(data as TouchData);
                    break;
                case DataType.Text:
                    Process(data as TextData);
                    break;
                case DataType.Tap:
                    Process(data as TapData);
                    break;
            }
        }

        private void Process(AccelerometerData data)
        {
            var changeX = data.X * 10.0;
            var changeY = data.Y * 10.0;

            //move mouse position
            var currentPosition = Win32Wrapper.GetCursorPosition();
            var newPositionX = (int)Math.Round(currentPosition.X + changeX);
            var newPositionY = (int)Math.Round(currentPosition.Y + changeY);
            Win32Wrapper.SetCursorPosition(newPositionX, newPositionY);
        }

        private void Process(TouchData data)
        {
            Debug.WriteLine("InputEmulator: Processing raw touch input");

            var touchPointCount = data.TouchPoints.Count;

            //should never happen
            if (touchPointCount < 1)
            {
                return;
            }

            //do not process the data if any of the points is invalid
            if (data.TouchPoints.Any(o => o.State == TouchPointState.Invalid))
            {
                return;
            }

            //was a touch point released? => reset the reference point and done
            if (data.TouchPoints.Any(o => o.State == TouchPointState.Released))
            {
                //reset the last references
                _lastTouchX = -1.0f;
                _lastTouchY = -1.0f;
                return;
            }

            //get the coordinates
            var x = data.TouchPoints.Sum(o => o.Location.X) / touchPointCount;
            var y = data.TouchPoints.Sum(o => o.Location.Y) / touchPointCount;

            //if any touch points are newly pressed, set new reference data
            if (data.TouchPoints.Any(o => o.State == TouchPointState.Pressed))
            {
                _lastTouchX = x;
                _lastTouchY = y;
                return;
            }

            // at this point we know the touch point(s) state('s) is 'Moved'

            //get current computer's virtual screen width
            var screenWidth = SystemInformation.VirtualScreen.Width;
            var screenHeight = SystemInformation.VirtualScreen.Height;

            //decide what to do
            if (touchPointCount > 1)
            {
                //the user uses more than one finger => we use this for two-finger scrolling,
                // by emulating mouse wheel data
                var changeY = y - _lastTouchY;

                //change sign because the wheel works "the other way round"
                changeY *= -1;

                //conver to wheel ticks
                var ticks = (int)Math.Round(changeY / 15.0);
                if (ticks == 0)
                {
                    //if the scrolling was too "slow" to cause any mouse wheel ticks
                    // we simply return(not storing new reference coordinates!).
                    //this allows "slow" scrolling because the next time this is invoked,
                    //the difference is computed with regards to the old reference,
                    //making a tick >=1 more likely
                    return;
                }

                Win32Wrapper.SendMouseWheel(ticks);
            }
            else
            {
                //for only one finger touch data
                //we use the data for cursor movement
                if (UseRelativeTouchInput)
                {
                    //calc the change
                    var changeX = (int)Math.Ceiling(x - _lastTouchX);
                    var changeY = (int)Math.Ceiling(y - _lastTouchY);

                    if (changeX == 0 && changeY == 0)
                    {
                        //return without setting a new reference point (below)
                        // to allow values to aggregate, but do not use small values (in particular zero)
                        //to avoid rounding problems (=> "wandering" cursor)
                        return;
                    }

                    //move mouse position
                    var currentPosition = Win32Wrapper.GetCursorPosition();
                    var newPositionX = (int)Math.Round(currentPosition.X + changeX);
                    var newPositionY = (int)Math.Round(currentPosition.Y + changeY);

                    Win32Wrapper.SetCursorPosition(newPositionX, newPositionY);
                }
                else
                {
                    //calculate new position depending on the controller's screen dimension
                    var newX = (x / _controllerDisplayWidth) * screenWidth;
                    var newY = (y / _controllerDisplayHeight) * screenHeight;

                    //set absolute mouse position
                    var newPositionX = (int)Math.Round(newX) + SystemInformation.VirtualScreen.Left;
                    var newPositionY = (int)Math.Round(newY) + SystemInformation.VirtualScreen.Top;
                    Win32Wrapper.SetCursorPosition(newPositionX, newPositionY);
                }
            }

            //set new references
            _lastTouchX = x;
            _lastTouchY = y;
        }

        private void Process(TextData data)
        {
            // this simply passes on the transmitted text data as keyboard input to the operating system
            Win32Wrapper.SendKeyboardInput(data.Text);
        }

        private void Process(TapData data)
        {
            // "click"
            Win32Wrapper.SendMouseLeftButtonClick();
        }
    }
}
