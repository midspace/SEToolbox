namespace SEToolbox.Services
{
    using System;

    interface IDropable
    {
        /// <summary>
        /// Type of the data item
        /// </summary>
        Type DataType { get; }

        /// <summary>
        /// Drop data into the collection.
        /// </summary>
        /// <param name="data">The data to be dropped</param>
        /// <param name="index">optional: The index location to insert the data</param>
        void Drop(object data, int index = -1);
    }
}
