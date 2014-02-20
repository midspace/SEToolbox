﻿namespace SEToolbox.Interop
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Xml;
    using Microsoft.Xml.Serialization.GeneratedAssembly;
    using Sandbox.CommonLib.ObjectBuilders;
    using Sandbox.CommonLib.ObjectBuilders.Definitions;
    using SEToolbox.Support;
    using VRageMath;
using Sandbox.CommonLib.ObjectBuilders.VRageData;

    public class SpaceEngineersAPI
    {
        #region ctor

        static SpaceEngineersAPI()
        {
            // Dynamically read all definitions as soon as the SpaceEngineersAPI class is first invoked.
            ReadCubeBlockDefinitions();
        }

        #endregion

        #region Serializers

        public static T ReadSpaceEngineersFile<T, S>(Stream stream)
        where S : XmlSerializer1
        {
            var settings = new XmlReaderSettings()
            {
                IgnoreComments = true,
                IgnoreWhitespace = true,
            };

            object obj = null;

            using (var xmlReader = XmlReader.Create(stream, settings))
            {

                var serializer = (S)Activator.CreateInstance(typeof(S));
                //serializer.UnknownAttribute += serializer_UnknownAttribute;
                //serializer.UnknownElement += serializer_UnknownElement;
                //serializer.UnknownNode += serializer_UnknownNode;
                obj = serializer.Deserialize(xmlReader);
            }

            return (T)obj;
        }

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
                using (var xmlReader = XmlReader.Create(filename, settings))
                {

                    var serializer = (S)Activator.CreateInstance(typeof(S));
                    obj = serializer.Deserialize(xmlReader);
                }
            }

            return (T)obj;
        }

        public static T Deserialize<T>(string xml)
        {
            using (var textReader = new StringReader(xml))
            {
                return (T)(new XmlSerializerContract().GetSerializer(typeof(T)).Deserialize(textReader));
            }
        }

        public static string Serialize<T>(object item)
        {
            using (var textWriter = new StringWriter())
            {
                new XmlSerializerContract().GetSerializer(typeof(T)).Serialize(textWriter, item);
                return textWriter.ToString();
            }
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

            //// How they should be doing it to support Unicode.
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
            // Not the offical SE way of generating IDs, but its fast and we don't have to worry about a random seed.
            var buffer = Guid.NewGuid().ToByteArray();
            return BitConverter.ToInt64(buffer, 0);
        }

        #endregion

        #region SetCubeOrientation

        public static readonly SerializableBlockOrientation[] ValidOrientations = new SerializableBlockOrientation[] {
            new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Forward, VRageMath.Base6Directions.Direction.Up),      // no rotation
            new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Backward, VRageMath.Base6Directions.Direction.Down),// 180 around X
            new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Backward, VRageMath.Base6Directions.Direction.Up),// 180 around Y
            new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Forward, VRageMath.Base6Directions.Direction.Down), // 180 around Z

            new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Up, VRageMath.Base6Directions.Direction.Backward),// +90 around X
            new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Down, VRageMath.Base6Directions.Direction.Forward),// -90 around X
            new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Left, VRageMath.Base6Directions.Direction.Up), // +90 around Y
            new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Right, VRageMath.Base6Directions.Direction.Up),// -90 around Y
            new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Forward, VRageMath.Base6Directions.Direction.Left),// +90 around Z
            new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Forward, VRageMath.Base6Directions.Direction.Right),// -90 around Z

            new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Backward, VRageMath.Base6Directions.Direction.Right),
            new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Backward, VRageMath.Base6Directions.Direction.Left),
            new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Left, VRageMath.Base6Directions.Direction.Down),
            new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Right, VRageMath.Base6Directions.Direction.Down),
            new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Down, VRageMath.Base6Directions.Direction.Backward),
            new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Up, VRageMath.Base6Directions.Direction.Forward),
            new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Up, VRageMath.Base6Directions.Direction.Forward),

            new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Left, VRageMath.Base6Directions.Direction.Backward),
            new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Down, VRageMath.Base6Directions.Direction.Right),
            new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Up, VRageMath.Base6Directions.Direction.Right),
            new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Right, VRageMath.Base6Directions.Direction.Forward),
            new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Up, VRageMath.Base6Directions.Direction.Left),
            new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Left, VRageMath.Base6Directions.Direction.Forward),
            new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Right, VRageMath.Base6Directions.Direction.Backward),
            new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Down, VRageMath.Base6Directions.Direction.Left),
            new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Down, VRageMath.Base6Directions.Direction.Left),
            new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Right, VRageMath.Base6Directions.Direction.Backward),
            new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Left, VRageMath.Base6Directions.Direction.Forward),
            new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Up, VRageMath.Base6Directions.Direction.Left),
            new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Right, VRageMath.Base6Directions.Direction.Forward),
            new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Up, VRageMath.Base6Directions.Direction.Right),
            new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Down, VRageMath.Base6Directions.Direction.Right),
            new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Left, VRageMath.Base6Directions.Direction.Backward),
        };

        public static readonly Dictionary<CubeType, SerializableBlockOrientation> CubeOrientations = new Dictionary<CubeType, SerializableBlockOrientation>()
        {
            {CubeType.Cube, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Forward, VRageMath.Base6Directions.Direction.Up)},

            {CubeType.SlopeCenterBackTop, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Down, VRageMath.Base6Directions.Direction.Forward)}, // -90 around X
            {CubeType.SlopeRightBackCenter, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Down, VRageMath.Base6Directions.Direction.Left)},
            {CubeType.SlopeLeftBackCenter, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Down, VRageMath.Base6Directions.Direction.Right)},
            {CubeType.SlopeCenterBackBottom, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Forward, VRageMath.Base6Directions.Direction.Up)}, // no rotation
            {CubeType.SlopeRightCenterTop, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Backward, VRageMath.Base6Directions.Direction.Left)},
            {CubeType.SlopeLeftCenterTop, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Backward, VRageMath.Base6Directions.Direction.Right)},
            {CubeType.SlopeRightCenterBottom, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Forward, VRageMath.Base6Directions.Direction.Left)}, // +90 around Z
            {CubeType.SlopeLeftCenterBottom, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Forward, VRageMath.Base6Directions.Direction.Right)}, // -90 around Z
            {CubeType.SlopeCenterFrontTop, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Backward, VRageMath.Base6Directions.Direction.Down)}, // 180 around X
            {CubeType.SlopeRightFrontCenter, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Up, VRageMath.Base6Directions.Direction.Left)},
            {CubeType.SlopeLeftFrontCenter, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Up, VRageMath.Base6Directions.Direction.Right)},
            {CubeType.SlopeCenterFrontBottom, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Up, VRageMath.Base6Directions.Direction.Backward)},// +90 around X

             // Probably got the names of these all messed up in relation to their actual orientation.
            {CubeType.NormalCornerLeftFrontTop, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Backward, VRageMath.Base6Directions.Direction.Right)},
            {CubeType.NormalCornerRightFrontTop, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Backward, VRageMath.Base6Directions.Direction.Down)}, // 180 around X
            {CubeType.NormalCornerLeftBackTop, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Down, VRageMath.Base6Directions.Direction.Right)},
            {CubeType.NormalCornerRightBackTop, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Down, VRageMath.Base6Directions.Direction.Forward)}, // -90 around X
            {CubeType.NormalCornerLeftFrontBottom, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Up, VRageMath.Base6Directions.Direction.Right)},
            {CubeType.NormalCornerRightFrontBottom, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Up, VRageMath.Base6Directions.Direction.Backward)},// +90 around X 
            {CubeType.NormalCornerLeftBackBottom, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Forward, VRageMath.Base6Directions.Direction.Right)},// -90 around Z
            {CubeType.NormalCornerRightBackBottom, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Forward, VRageMath.Base6Directions.Direction.Up)},  // no rotation

            //{CubeType.LargeRamp..., new VRageMath.Quaternion(0, -0.707106769f, 0.707106769f, 0)},
            //{CubeType.LargeRamp..., new VRageMath.Quaternion(0.5f, -0.5f, 0.5f, 0.5f)},
            //{CubeType.LargeRamp..., new VRageMath.Quaternion(0.707106769f, 0, 0, 0.707106769f)},
            //{CubeType.LargeRamp..., new VRageMath.Quaternion(0.5f, 0.5f, -0.5f, 0.5f)},
        };

        public static SerializableBlockOrientation GetCubeOrientation(CubeType type)
        {
            if (CubeOrientations.ContainsKey(type))
                return CubeOrientations[type];

            throw new NotImplementedException(string.Format("SetCubeOrientation of type [{0}] not yet implmented.", type));
        }

        #endregion

        #region ReadCubeBlockDefinitions

        static MyObjectBuilder_CubeBlockDefinitions _cubeBlockDefinitions;
        static MyObjectBuilder_ComponentDefinitions _componentDefinitions;
        static MyObjectBuilder_BlueprintDefinitions _blueprintDefinitions;
        static MyObjectBuilder_PhysicalItemDefinitions _physicalItemDefinitions;
        static MyObjectBuilder_VoxelMaterialDefinitions _voxelMaterialDefinitions;
        static Dictionary<string, byte> _materialIndex;

        public static void ReadCubeBlockDefinitions()
        {
            _voxelMaterialDefinitions = LoadContentFile<MyObjectBuilder_VoxelMaterialDefinitions, MyObjectBuilder_VoxelMaterialDefinitionsSerializer>("VoxelMaterials.sbc");
            _physicalItemDefinitions = LoadContentFile<MyObjectBuilder_PhysicalItemDefinitions, MyObjectBuilder_PhysicalItemDefinitionsSerializer>("PhysicalItems.sbc");
            _componentDefinitions = LoadContentFile<MyObjectBuilder_ComponentDefinitions, MyObjectBuilder_ComponentDefinitionsSerializer>("Components.sbc");
            _cubeBlockDefinitions = LoadContentFile<MyObjectBuilder_CubeBlockDefinitions, MyObjectBuilder_CubeBlockDefinitionsSerializer>("CubeBlocks.sbc");
            _blueprintDefinitions = LoadContentFile<MyObjectBuilder_BlueprintDefinitions, MyObjectBuilder_BlueprintDefinitionsSerializer>("Blueprints.sbc");

            _materialIndex = new Dictionary<string, byte>();
        }

        private static T LoadContentFile<T, S>(string filename) where S : XmlSerializer1
        {
            object fileContent = null;

            var filePath = Path.Combine(Path.Combine(ToolboxUpdater.GetApplicationFilePath(), @"Content\Data"), filename);

            if (!File.Exists(filePath))
            {
                throw new ToolboxException(ExceptionState.MissingContentFile, filePath);
            }

            try
            {
                fileContent = SpaceEngineersAPI.ReadSpaceEngineersFile<T, S>(filePath);
            }
            catch
            {
                throw new ToolboxException(ExceptionState.CorruptContentFile, filePath);
            }

            if (fileContent == null)
            {
                throw new ToolboxException(ExceptionState.EmptyContentFile, filePath);
            }

            // TODO: set a file watch to reload the files, incase modding is occuring at the same time this is open.
            //     Lock the load during this time, in case it happens multiple times.
            // Report a friendly error if this load fails.

            return (T)fileContent;
        }

        #endregion

        #region FetchCubeBlockMass

        public static float FetchCubeBlockMass(string subTypeid, MyCubeSize cubeSize)
        {
            float mass = 0;

            var cubeBlockDefinition = _cubeBlockDefinitions.Definitions.FirstOrDefault(c => cubeSize == c.CubeSize
                && (subTypeid == c.Id.SubtypeId || (c.Variants != null && c.Variants.Any(v => subTypeid == c.Id.SubtypeId + v.Color))));

            if (cubeBlockDefinition != null)
            {
                foreach (var component in cubeBlockDefinition.Components)
                {
                    mass += _componentDefinitions.Components.Where(c => c.Id.SubtypeId == component.Subtype).Sum(c => c.Mass) * component.Count;
                }
            }

            return mass;
        }

        public static void AccumulateCubeBlueprintRequirements(string subTypeid, MyCubeSize cubeSize, decimal amount, Dictionary<string, MyObjectBuilder_BlueprintDefinition.Item> requirements, ref TimeSpan timeTaken)
        {
            var cubeBlockDefinition = _cubeBlockDefinitions.Definitions.FirstOrDefault(c => cubeSize == c.CubeSize
                && (subTypeid == c.Id.SubtypeId || (c.Variants != null && c.Variants.Any(v => subTypeid == c.Id.SubtypeId + v.Color))));

            if (cubeBlockDefinition != null)
            {
                foreach (var component in cubeBlockDefinition.Components)
                {
                    AccumulateCubeBlueprintRequirements(component.Subtype, component.Type, amount, requirements, ref timeTaken);
                }
            }
        }

        public static void AccumulateCubeBlueprintRequirements(string subType, MyObjectBuilderTypeEnum type, decimal amount, Dictionary<string, MyObjectBuilder_BlueprintDefinition.Item> requirements, ref TimeSpan timeTaken)
        {
            var bp = _blueprintDefinitions.Blueprints.FirstOrDefault(b => b.Result.SubtypeId == subType && b.Result.TypeId == type);
            if (bp != null)
            {
                foreach (var item in bp.Prerequisites)
                {
                    if (requirements.ContainsKey(item.SubtypeId))
                    {
                        // append existing
                        requirements[item.SubtypeId].Amount += (amount / bp.Result.Amount) * item.Amount;
                    }
                    else
                    {
                        // add new
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

        public static float GetItemMass(MyObjectBuilderTypeEnum typeId, string subTypeId)
        {
            var item = _physicalItemDefinitions.Definitions.FirstOrDefault(d => d.Id.TypeId == typeId && d.Id.SubtypeId == subTypeId);
            if (item != null)
            {
                return item.Mass;
            }
            return 0;
        }

        public static IList<MyObjectBuilder_VoxelMaterialDefinition> GetMaterialList()
        {
            return _voxelMaterialDefinitions.Materials;
        }

        public static byte GetMaterialIndex(string materialName)
        {
            if (_materialIndex.ContainsKey(materialName))
                return _materialIndex[materialName];
            else
            {
                var material = _voxelMaterialDefinitions.Materials.FirstOrDefault(m => m.Name == materialName);
                var index = (byte)_voxelMaterialDefinitions.Materials.ToList().IndexOf(material);
                _materialIndex.Add(materialName, index);
                return index;
            }
        }

        public static string GetMaterialName(byte materialIndex)
        {
            return _voxelMaterialDefinitions.Materials[materialIndex].Name;
        }

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
#warning resolve cubetype size.
                max.X = Math.Max(max.X, block.Min.X);       // TODO: resolve cubetype size.
                max.Y = Math.Max(max.Y, block.Min.Y);
                max.Z = Math.Max(max.Z, block.Min.Z);
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

        #region CountAssets

        public static Dictionary<string, int> CountAssets(IList<byte> materialAssets)
        {
            var assetCount = new Dictionary<byte, int>();
            for (var i = 0; i < materialAssets.Count; i++)
            {
                if (assetCount.ContainsKey(materialAssets[i]))
                {
                    assetCount[materialAssets[i]]++;
                }
                else
                {
                    assetCount.Add(materialAssets[i], 1);
                }
            }

            var assetNameCount = new Dictionary<string, int>();
            foreach (var kvp in assetCount)
            {
                assetNameCount.Add(SpaceEngineersAPI.GetMaterialName(kvp.Key), kvp.Value);
            }

            return assetNameCount;
        }

        #endregion
    }
}