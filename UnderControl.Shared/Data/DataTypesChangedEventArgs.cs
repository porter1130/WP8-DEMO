using System;
using System.Collections.Generic;

namespace UnderControl.Shared.Data
{
    /// <summary>
    /// Event arguments that transport a collection of new data types when the number of acquired data types has changed.
    /// </summary>
    public class DataTypesChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the new data types that are acquired by a data source.
        /// </summary>
        public IEnumerable<DataType> NewDataTypes
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataTypesChangedEventArgs"/> class.
        /// </summary>
        /// <param name="newDataTypes">The new data types.</param>
        public DataTypesChangedEventArgs(IEnumerable<DataType> newDataTypes)
        {
            NewDataTypes = newDataTypes;
        }
    }
}