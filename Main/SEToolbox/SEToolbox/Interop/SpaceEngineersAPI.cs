namespace SEToolbox.Interop
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using Microsoft.Win32;
    using Microsoft.Xml.Serialization.GeneratedAssembly;
    using Sandbox.CommonLib.ObjectBuilders;
    using Sandbox.CommonLib.ObjectBuilders.Definitions;
    using VRageMath;

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

            if (File.Exists(filename))
            {
                using (XmlReader xmlReader = XmlReader.Create(filename, settings))
                {

                    S serializer = (S)Activator.CreateInstance(typeof(S));
                    //serializer.UnknownAttribute += serializer_UnknownAttribute;
                    //serializer.UnknownElement += serializer_UnknownElement;
                    //serializer.UnknownNode += serializer_UnknownNode;
                    obj = serializer.Deserialize(xmlReader);
                }
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
        static MyObjectBuilder_BlueprintDefinitions blueprintDefinitions;
        static MyObjectBuilder_PhysicalItemDefinitions physicalItemDefinitions;
        static MyObjectBuilder_VoxelMaterialDefinitions voxelMaterialDefinitions;

        public static void ReadCubeBlockDefinitions()
        {
            var voxelMaterialsFilename = Path.Combine(SpaceEngineersAPI.GetApplicationFilePath(), @"Content\Data\VoxelMaterials.sbc");
            voxelMaterialDefinitions = SpaceEngineersAPI.ReadSpaceEngineersFile<MyObjectBuilder_VoxelMaterialDefinitions, MyObjectBuilder_VoxelMaterialDefinitionsSerializer>(voxelMaterialsFilename);

            var physicalItemsFilename = Path.Combine(SpaceEngineersAPI.GetApplicationFilePath(), @"Content\Data\PhysicalItems.sbc");
            physicalItemDefinitions = SpaceEngineersAPI.ReadSpaceEngineersFile<MyObjectBuilder_PhysicalItemDefinitions, MyObjectBuilder_PhysicalItemDefinitionsSerializer>(physicalItemsFilename);

            var componentsFilename = Path.Combine(SpaceEngineersAPI.GetApplicationFilePath(), @"Content\Data\Components.sbc");
            componentDefinitions = SpaceEngineersAPI.ReadSpaceEngineersFile<MyObjectBuilder_ComponentDefinitions, MyObjectBuilder_ComponentDefinitionsSerializer>(componentsFilename);

            var cubeblocksFilename = Path.Combine(SpaceEngineersAPI.GetApplicationFilePath(), @"Content\Data\CubeBlocks.sbc");
            cubeBlockDefinitions = SpaceEngineersAPI.ReadSpaceEngineersFile<MyObjectBuilder_CubeBlockDefinitions, MyObjectBuilder_CubeBlockDefinitionsSerializer>(cubeblocksFilename);

            var blueprintsFilename = Path.Combine(SpaceEngineersAPI.GetApplicationFilePath(), @"Content\Data\Blueprints.sbc");
            blueprintDefinitions = SpaceEngineersAPI.ReadSpaceEngineersFile<MyObjectBuilder_BlueprintDefinitions, MyObjectBuilder_BlueprintDefinitionsSerializer>(blueprintsFilename);


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

        public static void AccumulateCubeBlueprintRequirements(string subTypeid, MyCubeSize cubeSize, decimal amount, Dictionary<string, MyObjectBuilder_BlueprintDefinition.Item> requirements, ref TimeSpan timeTaken)
        {
            var cubeBlockDefinition = cubeBlockDefinitions.Definitions.FirstOrDefault(c => cubeSize == c.CubeSize
                && (subTypeid == c.Id.SubtypeId || (c.Variants != null && c.Variants.Any(v => subTypeid == c.Id.SubtypeId + v.Color))));

            if (cubeBlockDefinition != null)
            {
                foreach (var component in cubeBlockDefinition.Components)
                {
                    var bp = blueprintDefinitions.Blueprints.FirstOrDefault(b => b.Result.SubtypeId == component.Subtype && b.Result.TypeId == component.Type);
                    if (bp != null)
                    {
                        foreach (var item in bp.Prerequisites)
                        {
                            if (requirements.ContainsKey(item.SubtypeId))
                            {
                                // append
                                requirements[item.SubtypeId].Amount += (amount / bp.Result.Amount) * item.Amount;
                            }
                            else
                            {
                                // add
                                requirements.Add(item.SubtypeId, new MyObjectBuilder_BlueprintDefinition.Item()
                                {
                                    Amount = (amount / bp.Result.Amount) * item.Amount,
                                    TypeId = item.TypeId,
                                    SubtypeId = item.SubtypeId,
                                    Id = item.Id
                                });
                            }

                            var ticks = TimeSpan.TicksPerSecond * (decimal)bp.BaseProductionTimeInSeconds * amount;
                            var ts = new TimeSpan((long)ticks);
                            timeTaken += ts;
                        }
                    }
                }
            }
        }

        //public static void AccumulateCubeBlueprintRequirements(ref MyObjectBuilder_BlueprintDefinition requirements, string subTypeid, MyCubeSize cubeSize)
        //{
        //    var cubeBlockDefinition = cubeBlockDefinitions.Definitions.FirstOrDefault(c => cubeSize == c.CubeSize
        //       && (subTypeid == c.Id.SubtypeId || (c.Variants != null && c.Variants.Any(v => subTypeid == c.Id.SubtypeId + v.Color))));

        //    if (cubeBlockDefinition != null)
        //    {
        //        foreach (var component in cubeBlockDefinition.Components)
        //        {
        //            var bp = blueprintDefinitions.Blueprints.FirstOrDefault(b => b.Result.SubtypeId == component.Subtype && b.Result.TypeId == component.Type);
        //            if (bp != null)
        //            {
        //                foreach (var item in bp.Prerequisites)
        //                {
        //                    requirements.Prerequisites.FirstOrDefault(
        //                    //if (requirements.ContainsKey(item.SubtypeId))
        //                    //{
        //                    //    // append
        //                    //    requirements[item.SubtypeId].Amount += item.Amount;
        //                    //}
        //                    //else
        //                    //{
        //                    //    // add
        //                    //    requirements.Add(item.SubtypeId, new MyObjectBuilder_BlueprintDefinition.Item() { Amount = item.Amount, TypeId = item.TypeId, SubtypeId = item.SubtypeId, Id = item.Id });
        //                    //}
        //                }
        //            }
        //        }
        //    }
        //}

        #endregion

        #region GetBoundingBox

        public static BoundingBox GetBoundingBox(MyObjectBuilder_CubeGrid entity)
        {
            var min = new Vector3(int.MaxValue, int.MaxValue, int.MaxValue);
            var max = new Vector3(int.MinValue, int.MinValue, int.MinValue);

            foreach (var block in entity.CubeBlocks)
            {
                min.X = Math.Min(min.X, block.Min.X);
                min.Y = Math.Min(min.Y, block.Min.Y);
                min.Z = Math.Min(min.Z, block.Min.Z);
                max.X = Math.Max(max.X, block.Max.X);
                max.Y = Math.Max(max.Y, block.Max.Y);
                max.Z = Math.Max(max.Z, block.Max.Z);
            }

            // scale box to GridSize
            var size = max - min;
            if (entity.GridSizeEnum == MyCubeSize.Large)
            {
                size = new Vector3(size.X * 2.5f, size.Y * 2.5f, size.Z * 2.5f);
            }
            else if (entity.GridSizeEnum == MyCubeSize.Small)
            {
                size = new Vector3(size.X * 0.5f, size.Y * 0.5f, size.Z * 0.5f);
            }

            // translate box according to min/max, but reset origin.
            var bb = new BoundingBox(new Vector3(0, 0, 0), size);

            // TODO: translate for rotation.
            //bb. ????

            // translate position.
            bb.Translate(entity.PositionAndOrientation.Value.Position);


            return bb;
        }

        #endregion
    }
}