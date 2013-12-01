namespace SEToolbox.Interop
{
    using Microsoft.Win32;
    using Microsoft.Xml.Serialization.GeneratedAssembly;
    using Sandbox.CommonLib.ObjectBuilders;
    using Sandbox.CommonLib.ObjectBuilders.Definitions;
    using System;
    using System.IO;
    using System.Linq;
    using System.Xml;

    public class SpaceEngineersAPI
    {
        #region GetApplicationFilePath

        public static bool IsSpaceEngineersInstalled()
        {
            var filePath = GetApplicationFilePath();
            if (string.IsNullOrEmpty(filePath))
                return false;
            if (!Directory.Exists(filePath))
                return false;
            if (!File.Exists(Path.Combine(filePath, "SpaceEngineers.exe")))
                return false;
            return true;
        }

        public static string GetApplicationFilePath()
        {
            // Using the [Software\Valve\Steam\SteamPath] as a base for "\steamapps\common\SpaceEngineers", is unreliable, as the Steam Library is customizable (multiple installations/locations).

            RegistryKey key;
            if (Environment.Is64BitProcess)
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 244850", false);
            else
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 244850", false);

            if (key != null)
            {
                return key.GetValue("InstallLocation") as string;
            }

            return null;
        }

        #endregion

        #region Serializers

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

        #endregion

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

        #region ReadCubeBlockDefinitions

        static MyObjectBuilder_CubeBlockDefinitions cubeBlockDefinitions;
        static MyObjectBuilder_ComponentDefinitions componentDefinitions;

        public static void ReadCubeBlockDefinitions()
        {
            var cubeblocksFilename = Path.Combine(SpaceEngineersAPI.GetApplicationFilePath(), @"Content\Data\CubeBlocks.sbc");
            var componentsFilename = Path.Combine(SpaceEngineersAPI.GetApplicationFilePath(), @"Content\Data\Components.sbc");

            cubeBlockDefinitions = SpaceEngineersAPI.ReadSpaceEngineersFile<MyObjectBuilder_CubeBlockDefinitions, MyObjectBuilder_CubeBlockDefinitionsSerializer>(cubeblocksFilename);
            componentDefinitions = SpaceEngineersAPI.ReadSpaceEngineersFile<MyObjectBuilder_ComponentDefinitions, MyObjectBuilder_ComponentDefinitionsSerializer>(componentsFilename);

            // TODO: set a file watch to reload the files, incase modding is occuring at the same time this is open.
            //     Lock the load during this time, in case it happens multiple times.
            // Report a friendly error if this load fails.
        }

        #endregion

        #region FetchCubeBlockMass

        public static float FetchCubeBlockMass(string subTypeid, MyCubeSize cubeSize)
        {
            float mass = 0;

            var cubeBlockDefinition = cubeBlockDefinitions.Definitions.FirstOrDefault(c => cubeSize == c.CubeSize
                && (subTypeid == c.Id.SubtypeId || (c.Variants != null && c.Variants.Any(v => subTypeid == c.Id.SubtypeId + v.Color))));

            if (cubeBlockDefinition != null)
            {
                foreach (var component in cubeBlockDefinition.Components)
                {
                    mass += componentDefinitions.Components.Where(c => c.Id.SubtypeId == component.Subtype).Sum(c => c.Mass) * component.Count;
                }
            }

            return mass;
        }

        #endregion
    }
}