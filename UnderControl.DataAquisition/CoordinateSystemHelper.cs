using Microsoft.Phone.Controls;
using UnderControl.Shared.Data;
using Xna = Microsoft.Xna.Framework;

namespace UnderControl.DataAcquisition
{
    /// <summary>
    /// Helper functions for coordinate system conversions between the physical and logical dimensions of the device screen.
    /// </summary>
    internal static class CoordinateSystemHelper
    {
        /// <summary>
        /// Takes a vector and switches/adjusts the X and Y coordinates as required to match 
        /// the logical axis alignment of the device. The result is stored in the <paramref name="result"/> argument.
        /// </summary>
        /// <param name="result">Contains the adjusted X and Y coordinates of the physical coordinates.</param>
        /// <param name="physicalPosition">The original values, using the physical dimensions of the device screen.</param>
        public static void AdjustLogicalAxis(Vector2 result, Xna.Vector2 physicalPosition)
        {
            // determine how we need to convert the physical x and y coordinates
            // into logical ones
            if (DeviceInfo.Current.Orientation == PageOrientation.LandscapeLeft)
            {
                result.X = physicalPosition.Y;
                result.Y = -physicalPosition.X;
            }
            else
            {
                // assume PageOrientation.LandscapeRight since we don't support portrait mode at all
                result.X = -physicalPosition.Y;
                result.Y = physicalPosition.X;
            }
        }

        /// <summary>
        /// Calculates the logical position from a given physical position, using the current device orientation.
        /// </summary>
        /// <param name="result">The resulting corrected values in logical space.</param>
        /// <param name="physicalPosition">The original physical position.</param>
        public static void CalculateLogicalPosition(Vector2 result, Xna.Vector2 physicalPosition)
        {
            // get the physical screen dimensions
            var physicalScreenWidth = (float)DeviceInfo.Current.PhysicalScreenWidth;
            var physicalScreenHeight = (float)DeviceInfo.Current.PhysicalScreenHeight;

            // variables
            float x;
            float y;

            // determine how we need to convert the physical x and y coordinates
            // into logical ones
            if (DeviceInfo.Current.Orientation == PageOrientation.LandscapeLeft)
            {
                x = physicalPosition.Y;
                y = physicalScreenWidth - physicalPosition.X;
            }
            else
            {
                // assume PageOrientation.LandscapeRight since we don't support portrait mode at all
                x = physicalScreenHeight - physicalPosition.Y;
                y = physicalPosition.X;
            }

            // set values
            result.X = x;
            result.Y = y;
        }

        /// <summary>
        /// Determines whether a given logical position is within the configured active area determined by the given input margin.
        /// </summary>
        /// <param name="logicalPosition">The logical position to test.</param>
        /// <param name="inputMargin">The input margin to use to determine the active area.</param>
        /// <returns>
        ///   <c>true</c> if the logical position is within the active area; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsInActiveArea(Vector2 logicalPosition, Thickness inputMargin)
        {
            if (inputMargin == null)
            {
                return true;
            }

            return logicalPosition.X - inputMargin.Left > 0 &&
                   logicalPosition.X + inputMargin.Right < DeviceInfo.Current.LogicalScreenWidth &&
                   logicalPosition.Y - inputMargin.Top > 0 &&
                   logicalPosition.Y + inputMargin.Bottom < DeviceInfo.Current.LogicalScreenHeight;
        }

        /// <summary>
        /// Projects a logical position within the active area defined by the given margin to the full logical screen dimensions.
        /// The result is stored in the <paramref name="logicalPosition"/> that is passed into the method.
        /// </summary>
        /// <remarks>
        /// Example: 
        /// * The original logical horizontal position is 100
        /// * The left margin is 50
        /// * The right margin is 10
        /// * The logical screen width is 800.
        /// That means that the resulting active screen width is 800 - (50 + 10) = 740. The relative offset of the logical position
        /// then is (100 - 50) / 740 ~= 0.068. The resulting projected horizontal coordinate then is 800 * 0.068 = 54.4.
        /// </remarks>
        /// <param name="logicalPosition">The logical position to use.</param>
        /// <param name="inputMargin">The input margin that defines the active area.</param>
        public static void ProjectToActiveArea(Vector2 logicalPosition, Thickness inputMargin)
        {
            // get the logical screen dimensions
            var logicalScreenWidth = (float)DeviceInfo.Current.LogicalScreenWidth;
            var logicalScreenHeight = (float)DeviceInfo.Current.LogicalScreenHeight;

            // calculate the active screen dimensions
            var activeScreenWidth = logicalScreenWidth - (float)(inputMargin.Left + inputMargin.Right);
            var activeScreenHeight = logicalScreenHeight - (float)(inputMargin.Top + inputMargin.Bottom);

            // calculate the new coordinates
            var clampedX = Xna.MathHelper.Clamp(logicalPosition.X, (float)inputMargin.Left, logicalScreenWidth - (float)inputMargin.Right);
            var relativeX = (clampedX - (float)inputMargin.Left) / activeScreenWidth;
            logicalPosition.X = relativeX * logicalScreenWidth;

            var clampedY = Xna.MathHelper.Clamp(logicalPosition.Y, (float)inputMargin.Top, logicalScreenHeight - (float)inputMargin.Bottom);
            var relativeY = (clampedY - (float)inputMargin.Top) / activeScreenHeight;
            logicalPosition.Y = relativeY * logicalScreenHeight;
        }
    }
}
