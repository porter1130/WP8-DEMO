
namespace UnderControl.Shared.Data
{
    /// <summary>
    /// Represents a data message that transports custom drag data.
    /// </summary>
    /// <remarks>
    /// Custom drag data basically is the same as normal raw touch data, but the consuming side 
    /// can conveniently use this for dragging logic. The differentiation between custom drag data
    /// and the built-in drag modes like <c>FreeDrag</c> has been made to enable "relative" dragging,
    /// which means a drag mode that allows the user to lift their fingers and continue dragging in
    /// another spot of the touch area without the drag operation to be automatically completed.
    /// </remarks>
    public sealed class CustomDragData : DragDataBase
    {
        #region Overrides of DataMessage

        /// <summary>
        /// Returns data type <c>CustomDrag</c>.
        /// </summary>
        public override DataType DataType
        {
            get
            {
                return DataType.CustomDrag;
            }
        }

        /// <summary>
        /// Returns <c>true</c>.
        /// </summary>
        public override bool MustBeDelivered
        {
            get
            {
                // custom drag data basically is the same as raw touch input
                // we do not need to ensure each and every data message is delivered
                return false;
            }
        }

        #endregion
    }
}
