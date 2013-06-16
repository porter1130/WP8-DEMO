
namespace UnderControl.Shared.Data
{
    /// <summary>
    /// A data message that represents a horizontal drag gesture.
    /// </summary>
    public sealed class HorizontalDragData : DragDataBase
    {
        #region Overrides of DataMessage

        /// <summary>
        /// Returns data type <c>HorizontalDrag</c>.
        /// </summary>
        public override DataType DataType
        {
            get
            {
                return DataType.HorizontalDrag;
            }
        }

        #endregion
    }
}
