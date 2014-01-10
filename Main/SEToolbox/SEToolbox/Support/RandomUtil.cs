namespace SEToolbox.Support
{
    using System;

    public class RandomUtil
    {
        [ThreadStatic]
        static Random m_secretRandom;

        static Random random
        {
            get
            {
                if (m_secretRandom == null)
                {
                    m_secretRandom = new Random((int)DateTime.Now.Ticks);
                }
                return m_secretRandom;
            }
        }

        /// <summary>
        /// Returns a nonnegative random number less than the specified maximum.
        /// </summary>
        /// <param name="maxValue"> The exclusive upper bound of the random number to be generated. maxValue must be greater than or equal to zero.</param>
        /// <returns></returns>
        public static int GetInt(int maxValue)
        {
            return random.Next(maxValue);
        }

        /// <summary>
        /// Returns a random number within a specified range.
        /// </summary>
        /// <param name="minValue">minValue is the inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">maxValue is the exclusive upper bound of the random number returned.</param>
        /// <returns></returns>
        public static int GetInt(int minValue, int maxValue)
        {
            return random.Next(minValue, maxValue);
        }

        /// <summary>
        /// Returns a random number within a specified range.
        /// </summary>
        /// <param name="minValue">minValue is the inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">maxValue is the exclusive upper bound of the random number returned.</param>
        /// <returns></returns>
        public static float GetRandomFloat(float minValue, float maxValue)
        {
            return (float)random.NextDouble() * (maxValue - minValue) + minValue;
        }

        /// <summary>
        /// Returns a random number within a specified range.
        /// </summary>
        /// <param name="minValue">minValue is the inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">maxValue is the exclusive upper bound of the random number returned.</param>
        /// <returns></returns>
        public static double GetDouble(double minValue, double maxValue)
        {
            return random.NextDouble() * (maxValue - minValue) + minValue;
        }
    }
}
