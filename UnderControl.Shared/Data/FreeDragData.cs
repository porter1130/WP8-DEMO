
namespace UnderControl.Shared.Data
{
    /// <summary>
    /// A data message that represents a free drag gesture.
    /// </summary>
    public sealed class FreeDragData : DragDataBase
    {
        #region Overrides of DataMessage

        /// <summary>
        /// Returns data type <c>FreeDrag</c>.
        /// </summary>
        public override DataType DataType
        {
            get
            {
                return DataType.FreeDrag;
            }
        }

        #endregion
    }
}
