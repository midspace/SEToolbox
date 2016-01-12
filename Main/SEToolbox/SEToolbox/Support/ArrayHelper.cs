namespace SEToolbox.Support
{
    using System;

    public static class ArrayHelper
    {
        /// <summary>
        /// Creates a 2 dimensional jagged array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="length1"></param>
        /// <param name="length2"></param>
        /// <returns></returns>
        public static T[][] Create<T>(int length1, int length2)
        {
            var array = new T[length1][];

            for (var x = 0; x < length1; x++)
                array[x] = new T[length2];

            return array;
        }

        /// <summary>
        /// Creates a 3 dimensional jagged array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="length1"></param>
        /// <param name="length2"></param>
        /// <param name="length3"></param>
        /// <returns></returns>
        public static T[][][] Create<T>(int length1, int length2, int length3)
        {
            var array = new T[length1][][];

            for (var x = 0; x < length1; x++)
            {
                array[x] = new T[length2][];
                for (var y = 0; y < length2; y++)
                    array[x][y] = new T[length3];
            }

            return array;
        }

        /// <summary>
        /// Merges two arrays into a new array of the correct generic Type.
        /// </summary>
        /// <param name="objectArray1"></param>
        /// <param name="objectArray2"></param>
        /// <returns></returns>
        public static object MergeGenericArrays(object objectArray1, object objectArray2)
        {
            if (objectArray2 == null) return objectArray1;
            if (objectArray1 == null) return objectArray2;

            var elementType1 = objectArray1.GetType().GetElementType();
            var elementType2 = objectArray2.GetType().GetElementType();

            if (elementType1 != elementType2)
                throw new ArgumentException();

            var arrayType = elementType1.MakeArrayType();

            var array1 = (object[])Convert.ChangeType(objectArray1, arrayType);
            var array2 = (object[])Convert.ChangeType(objectArray2, arrayType);

            var arrayInstance = Array.CreateInstance(elementType1, array1.Length + array2.Length);
            Array.Copy(array1, 0, arrayInstance, 0, array1.Length);
            Array.Copy(array2, 0, arrayInstance, array1.Length, array2.Length);

            return arrayInstance;
        }
    }
}
