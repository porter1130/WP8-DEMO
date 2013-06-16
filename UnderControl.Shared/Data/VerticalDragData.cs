
namespace UnderControl.Shared.Data
{
    /// <summary>
    /// A data message that represents a vertical drag gesture.
    /// </summary>
    public class VerticalDragData : DragDataBase
    {
        #region Overrides of DataMessage

        /// <summary>
        /// Returns data type <c>VerticalDrag</c>.
        /// </summary>
        public override DataType DataType
        {
            get
            {
                return DataType.VerticalDrag;
            }
        }

        #endregion
    }
}
