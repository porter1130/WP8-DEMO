using System;
using System.Collections.Generic;

namespace UnderControl.Shared.Data
{
    /// <summary>
    /// Describes a data source that is able to acquire various data types.
    /// </summary>
    public interface IDataSource
    {
        /// <summary>
        /// Occurs when new data was acquired.
        /// </summary>
        event EventHandler<DataMessageEventArgs> DataAcquired;

        /// <summary>
        /// Occurs when the active data types have changed.
        /// </summary>
        event EventHandler<DataTypesChangedEventArgs> DataTypesChanged;

        /// <summary>
        /// Starts acquisition of a certain data type.
        /// </summary>
        /// <param name="dataType">The type of data to start acquisition for.</param>
        void StartAcquisition(DataType dataType);

        /// <summary>
        /// Stops acquisition of a certain data type.
        /// </summary>
        /// <param name="dataType">The type of the data to stop acquisition for.</param>
        void StopAcquisition(DataType dataType);

        /// <summary>
        /// Pauses acquisition of a certain data type.
        /// </summary>
        /// <param name="dataType">The type of the data to pause the acquisition for.</param>
        void PauseAcquisition(DataType dataType);

        /// <summary>
        /// Resumes acquisition of a certain data type.
        /// </summary>
        /// <param name="dataType">The type of data to resume the acquisition for.</param>
        void ResumeAcquisition(DataType dataType);

        /// <summary>
        /// Shuts down the data source and cleans up all resources.
        /// </summary>
        void Shutdown();

        /// <summary>
        /// Gets the currently active data types, including the paused data types.
        /// </summary>
        /// <returns>A collection of the data types that are currently set to be acquired, including the paused data types.</returns>
        IEnumerable<DataType> GetActiveDataTypes();

        /// <summary>
        /// Gets the paused data types only.
        /// </summary>
        /// <returns>A collection containing the currently paused data types.</returns>
        IEnumerable<DataType> GetPausedDataTypes();

        /// <summary>
        /// Gets capabilities information about the device used for data acquisition.
        /// </summary>
        /// <returns>A <c>ControllerInfoData</c> object that contains the capabilities information of the device.</returns>
        ControllerInfoData GetControllerInfoData();

        /// <summary>
        /// Configures the data acquisition with the specified configuration.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        void Configure(ControllerConfiguration configuration);

        /// <summary>
        /// Manually adds new data to the data source to be reported to consumers.
        /// This is mandatory for certain data types like <c>Text</c> which cannot be acquired automatically.
        /// </summary>
        /// <param name="dataMessage">The data message to be reported to consumers of the data source.</param>
        void AddData(DataMessage dataMessage);
    }
}
