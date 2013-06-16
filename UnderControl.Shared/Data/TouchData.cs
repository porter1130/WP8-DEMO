using System.Collections.Generic;
using System.IO;

namespace UnderControl.Shared.Data
{
    /// <summary>
    /// A data message that represents raw touch input.
    /// </summary>
    public class TouchData : DataMessage
    {
        /// <summary>
        /// Gets the list of touch points associated with this raw touch input reading.
        /// Typically, this can be up to four touch points on today's devices.
        /// </summary>
        public IList<TouchPoint> TouchPoints
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TouchData"/> class.
        /// </summary>
        public TouchData()
        {
            TouchPoints = new List<TouchPoint>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TouchData"/> class using a given set of touch points.
        /// </summary>
        /// <param name="touchPoints">The touch points to use.</param>
        public TouchData(IEnumerable<TouchPoint> touchPoints)
        {
            TouchPoints = new List<TouchPoint>(touchPoints);
        }

        #region Overrides of DataMessage

        /// <summary>
        /// Returns data type <c>Touch</c>.
        /// </summary>
        public override DataType DataType
        {
            get
            {
                return DataType.Touch;
            }
        }

        /// <summary>
        /// Returns <c>false</c>.
        /// </summary>
        public override bool MustBeDelivered
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Adds custom data to the raw representation of the data message.
        /// </summary>
        /// <param name="writer">The binary writer used to create the raw representation of the data message.</param>
        protected override void WriteData(BinaryWriter writer)
        {
            writer.Write(TouchPoints.Count);
            foreach (var point in TouchPoints)
            {
                writer.Write(point.Id);
                writer.Write(point.Location.X);
                writer.Write(point.Location.Y);
                writer.Write((int)point.State);
            }
        }

        /// <summary>
        /// Reads back custom data from a raw representation of the data message.
        /// </summary>
        /// <param name="reader">The binary reader used to read from the raw representation of the original data message.</param>
        protected override void ReadData(BinaryReader reader)
        {
            TouchPoints.Clear();

            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var point = new TouchPoint()
                                {
                                    Id = reader.ReadInt32(),
                                    Location = new Vector2(reader.ReadSingle(), reader.ReadSingle()),
                                    State = (TouchPointState)reader.ReadInt32()
                                };
                TouchPoints.Add(point);
            }
        }

        #endregion
    }
}
