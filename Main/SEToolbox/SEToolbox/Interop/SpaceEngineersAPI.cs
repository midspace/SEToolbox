namespace SEToolbox.Interop
{
    using Microsoft.Xml.Serialization.GeneratedAssembly;
    using Sandbox.CommonLib.ObjectBuilders;
    using System;
    using System.Xml;

    public class SpaceEngineersAPI
    {
        #region SpaceEngineersHelpers

        public static T ReadSpaceEngineersFile<T, S>(string filename)
        where S : XmlSerializer1
        {
            var settings = new XmlReaderSettings()
            {
                IgnoreComments = true,
                IgnoreWhitespace = true,
            };

            object obj = null;
            using (XmlReader xmlReader = XmlReader.Create(filename, settings))
            {

                S serializer = (S)Activator.CreateInstance(typeof(S));
                //serializer.UnknownAttribute += serializer_UnknownAttribute;
                //serializer.UnknownElement += serializer_UnknownElement;
                //serializer.UnknownNode += serializer_UnknownNode;
                obj = serializer.Deserialize(xmlReader);
            }

            return (T)obj;
        }

        public static bool WriteSpaceEngineersFile<T, S>(T sector, string filename)
            where S : XmlSerializer1
        {
            // How they appear to be writing the files currently.
            try
            {
                using (var xmlTextWriter = new XmlTextWriter(filename, null))
                {
                    xmlTextWriter.Formatting = Formatting.Indented;
                    xmlTextWriter.Indentation = 2;
                    S serializer = (S)Activator.CreateInstance(typeof(S));
                    serializer.Serialize(xmlTextWriter, sector);
                }
            }
            catch
            {
                return false;
            }

            //// How they should be doing it.
            //var settingsDestination = new XmlWriterSettings()
            //{
            //    Indent = true, // Set indent to false to compress.
            //    Encoding = new UTF8Encoding(false)   // codepage 65001 without signature. Removes the Byte Order Mark from the start of the file.
            //};

            //try
            //{
            //    using (var xmlWriter = XmlWriter.Create(filename, settingsDestination))
            //    {
            //        S serializer = (S)Activator.CreateInstance(typeof(S));
            //        serializer.Serialize(xmlWriter, sector);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    return false;
            //}

            return true;
        }

        #region GenerateEntityId

        public static long GenerateEntityId()
        {
            var buffer = Guid.NewGuid().ToByteArray();
            return BitConverter.ToInt64(buffer, 0);
        }

        #endregion

        #region SetCubeOrientation

        public static void SetCubeOrientation(MyObjectBuilder_CubeBlock cube, CubeType type)
        {
            //float d45 = Math.Sqrt(1 / 2);

            switch (type)
            {
                case CubeType.Cube: cube.Orientation = new VRageMath.Quaternion(0, 0, 0, 1); break;

                case CubeType.SlopeCenterBackTop: cube.Orientation = new VRageMath.Quaternion(-0.707106769f, 0, 0, 0.707106769f); break;
                case CubeType.SlopeRightBackCenter: cube.Orientation = new VRageMath.Quaternion(0.5f, -0.5f, -0.5f, -0.5f); break;
                case CubeType.SlopeLeftBackCenter: cube.Orientation = new VRageMath.Quaternion(0.5f, 0.5f, 0.5f, -0.5f); break;
                case CubeType.SlopeCenterBackBottom: cube.Orientation = new VRageMath.Quaternion(0, 0, 0, 1); break;
                case CubeType.SlopeRightCenterTop: cube.Orientation = new VRageMath.Quaternion(0.707106769f, -0.707106769f, 0, 0); break;
                case CubeType.SlopeLeftCenterTop: cube.Orientation = new VRageMath.Quaternion(0.707106769f, 0.707106769f, 0, 0); break;
                case CubeType.SlopeRightCenterBottom: cube.Orientation = new VRageMath.Quaternion(0, 0, 0.707106769f, 0.707106769f); break;
                case CubeType.SlopeLeftCenterBottom: cube.Orientation = new VRageMath.Quaternion(0, 0, -0.707106769f, 0.707106769f); break;
                case CubeType.SlopeCenterFrontTop: cube.Orientation = new VRageMath.Quaternion(1, 0, 0, 0); break;
                case CubeType.SlopeRightFrontCenter: cube.Orientation = new VRageMath.Quaternion(0.5f, -0.5f, 0.5f, 0.5f); break;
                case CubeType.SlopeLeftFrontCenter: cube.Orientation = new VRageMath.Quaternion(0.5f, 0.5f, -0.5f, 0.5f); break;
                case CubeType.SlopeCenterFrontBottom: cube.Orientation = new VRageMath.Quaternion(0.707106769f, 0, 0, 0.707106769f); break;

                case CubeType.CornerLeftFrontTop: cube.Orientation = new VRageMath.Quaternion(0.5f, 0.5f, 0.5f, -0.5f); break;
                case CubeType.CornerRightFrontTop: cube.Orientation = new VRageMath.Quaternion(1, 0, 0, 0); break;
                case CubeType.CornerLeftBackTop: cube.Orientation = new VRageMath.Quaternion(0.707106769f, 0.707106769f, 0, 0); break;
                case CubeType.CornerRightBackTop: cube.Orientation = new VRageMath.Quaternion(-0.707106769f, 0, 0, 0.707106769f); break;
                case CubeType.CornerLeftFrontBottom: cube.Orientation = new VRageMath.Quaternion(0.5f, 0.5f, -0.5f, 0.5f); break;
                case CubeType.CornerRightFrontBottom: cube.Orientation = new VRageMath.Quaternion(0.707106769f, 0, 0, 0.707106769f); break;
                case CubeType.CornerLeftBackBottom: cube.Orientation = new VRageMath.Quaternion(0, 0, -0.707106769f, 0.707106769f); break;
                case CubeType.CornerRightBackBottom: cube.Orientation = new VRageMath.Quaternion(0, 0, 0, 1); break;

                default:
                    throw new NotImplementedException(string.Format("SetCubeOrientation of type [{0}] not yet implmented.", type));
            }
        }

        #endregion

        #endregion
    }
}