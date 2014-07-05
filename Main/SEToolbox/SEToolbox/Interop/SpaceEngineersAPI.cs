namespace SEToolbox.Interop
{
    using Microsoft.Xml.Serialization.GeneratedAssembly;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Common.ObjectBuilders.Definitions;
    using SEToolbox.Support;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml;
    using VRage.Common.Utils;
    using VRageMath;

    public static class SpaceEngineersAPI
    {
        #region ctor

        static SpaceEngineersAPI()
        {
            // Dynamically read all definitions as soon as the SpaceEngineersAPI class is first invoked.
            ReadCubeBlockDefinitions();
            Sandbox.Common.Localization.MyTextsWrapper.Init();
        }

        public static void Init()
        {
            // Placeholder to make sure ctor is called.
        }

        #endregion

        #region Serializers

        public static T ReadSpaceEngineersFile<T, TS>(Stream stream)
        where TS : XmlSerializer1
        {
            var settings = new XmlReaderSettings
            {
                IgnoreComments = true,
                IgnoreWhitespace = true,
            };

            object obj = null;

            using (var xmlReader = XmlReader.Create(stream, settings))
            {

                var serializer = (TS)Activator.CreateInstance(typeof(TS));
                //serializer.UnknownAttribute += serializer_UnknownAttribute;
                //serializer.UnknownElement += serializer_UnknownElement;
                //serializer.UnknownNode += serializer_UnknownNode;
                obj = serializer.Deserialize(xmlReader);
            }

            return (T)obj;
        }

        public static bool TryReadSpaceEngineersFile<T, TS>(string filename, out T entity)
             where TS : XmlSerializer1
        {
            try
            {
                entity = SpaceEngineersAPI.ReadSpaceEngineersFile<T, TS>(filename);
                return true;
            }
            catch
            {
                entity = default(T);
                return false;
            }
        }

        public static T ReadSpaceEngineersFile<T, TS>(string filename)
        where TS : XmlSerializer1
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
                    var serializer = (TS)Activator.CreateInstance(typeof(TS));
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

        public static bool WriteSpaceEngineersFile<T, TS>(T sector, string filename)
            where TS : XmlSerializer1
        {
            // How they appear to be writing the files currently.
            try
            {
                using (var xmlTextWriter = new XmlTextWriter(filename, null))
                {
                    xmlTextWriter.Formatting = Formatting.Indented;
                    xmlTextWriter.Indentation = 2;
                    var serializer = (TS)Activator.CreateInstance(typeof(TS));
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

        #region ReadCubeBlockDefinitions

        private static MyObjectBuilder_Definitions _definitions;
        private static Dictionary<string, byte> _materialIndex;

        public static void ReadCubeBlockDefinitions()
        {
            // Single Player pathing.
            _definitions = LoadDefinitions(ToolboxUpdater.GetApplicationContentPath());

            // TODO: redirect pathing when prior to loading other paths.

            _materialIndex = new Dictionary<string, byte>();
        }

        private static MyObjectBuilder_Definitions LoadDefinitions(string contentPath)
        {
            var files = Directory.GetFiles(Path.Combine(contentPath, "Data"), "*.sbc");
            var definitions = new MyObjectBuilder_Definitions();

            foreach (var filePath in files)
            {
                var stockTemp = LoadContentFile<MyObjectBuilder_Definitions, MyObjectBuilder_DefinitionsSerializer>(filePath);
                var fields = stockTemp.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                foreach (var field in fields)
                {
                    var stockValues = field.GetValue(stockTemp);
                    if (stockValues != null)
                    {
                        field.SetValue(definitions, stockValues);
                    }
                }
            }

            // TODO: Read through the mod paths manually.
            // Using the MyObjectBuilder_Base.DeserializeXML() with MyFSLocationEnum.ContentWithMods does not work in the expected manner.

            return definitions;
        }

        private static T LoadContentFile<T, TS>(string filePath) where TS : XmlSerializer1
        {
            object fileContent = null;

            if (!File.Exists(filePath))
            {
                throw new ToolboxException(ExceptionState.MissingContentFile, filePath);
            }

            try
            {
                fileContent = SpaceEngineersAPI.ReadSpaceEngineersFile<T, TS>(filePath);
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

        public static float FetchCubeBlockMass(MyObjectBuilderTypeEnum typeId, MyCubeSize cubeSize, string subTypeid)
        {
            float mass = 0;

            var cubeBlockDefinition = GetCubeDefinition(typeId, cubeSize, subTypeid);

            if (cubeBlockDefinition != null)
            {
                foreach (var component in cubeBlockDefinition.Components)
                {
                    mass += _definitions.Components.Where(c => c.Id.SubtypeId == component.Subtype).Sum(c => c.Mass) * component.Count;
                }
            }

            return mass;
        }

        public static void AccumulateCubeBlueprintRequirements(string subType, MyObjectBuilderTypeEnum typeId, decimal amount, Dictionary<string, MyObjectBuilder_BlueprintDefinition.Item> requirements, out TimeSpan timeTaken)
        {
            TimeSpan time = new TimeSpan();
            var bp = _definitions.Blueprints.FirstOrDefault(b => b.Result.SubtypeId == subType && b.Result.TypeId == typeId);
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
                    time += ts;
                }
            }

            timeTaken = time;
        }

        public static MyObjectBuilder_DefinitionBase GetDefinition(MyObjectBuilderTypeEnum typeId, string subTypeId)
        {
            var cube = _definitions.CubeBlocks.FirstOrDefault(d => d.Id.TypeId == typeId && d.Id.SubtypeId == subTypeId);
            if (cube != null)
            {
                return cube;
            }

            var item = _definitions.PhysicalItems.FirstOrDefault(d => d.Id.TypeId == typeId && d.Id.SubtypeId == subTypeId);
            if (item != null)
            {
                return item;
            }

            var component = _definitions.Components.FirstOrDefault(c => c.Id.TypeId == typeId && c.Id.SubtypeId == subTypeId);
            if (component != null)
            {
                return component;
            }

            var magazine = _definitions.AmmoMagazines.FirstOrDefault(c => c.Id.TypeId == typeId && c.Id.SubtypeId == subTypeId);
            if (magazine != null)
            {
                return magazine;
            }

            return null;
        }

        public static float GetItemMass(MyObjectBuilderTypeEnum typeId, string subTypeId)
        {
            var def = GetDefinition(typeId, subTypeId);
            if (def is MyObjectBuilder_PhysicalItemDefinition)
            {
                var item2 = def as MyObjectBuilder_PhysicalItemDefinition;
                return item2.Mass;
            }

            return 0;
        }

        public static float GetItemVolume(MyObjectBuilderTypeEnum typeId, string subTypeId)
        {
            var def = GetDefinition(typeId, subTypeId);
            if (def is MyObjectBuilder_PhysicalItemDefinition)
            {
                var item2 = def as MyObjectBuilder_PhysicalItemDefinition;
                if (item2.Volume.HasValue)
                    return item2.Volume.Value;
            }

            return 0;
        }

        public static IList<MyObjectBuilder_VoxelMaterialDefinition> GetMaterialList()
        {
            return _definitions.VoxelMaterials;
        }

        public static byte GetMaterialIndex(string materialName)
        {
            if (_materialIndex.ContainsKey(materialName))
                return _materialIndex[materialName];
            else
            {
                var material = _definitions.VoxelMaterials.FirstOrDefault(m => m.Name == materialName);
                var index = (byte)_definitions.VoxelMaterials.ToList().IndexOf(material);
                _materialIndex.Add(materialName, index);
                return index;
            }
        }

        public static string GetMaterialName(byte materialIndex, byte defaultMaterialIndex)
        {
            if (materialIndex <= _definitions.VoxelMaterials.Length)
                return _definitions.VoxelMaterials[materialIndex].Name;
            else
                return _definitions.VoxelMaterials[defaultMaterialIndex].Name;
        }

        public static string GetMaterialName(byte materialIndex)
        {
            return _definitions.VoxelMaterials[materialIndex].Name;
        }

        #endregion

        #region GetCubeDefinition

        public static MyObjectBuilder_CubeBlockDefinition GetCubeDefinition(MyObjectBuilderTypeEnum typeId, MyCubeSize cubeSize, string subtypeId)
        {
            if (string.IsNullOrEmpty(subtypeId))
            {
                return _definitions.CubeBlocks.FirstOrDefault(d => d.CubeSize == cubeSize && d.Id.TypeId == typeId);
            }

            return _definitions.CubeBlocks.FirstOrDefault(d => d.Id.SubtypeId == subtypeId || (d.Variants != null && d.Variants.Any(v => subtypeId == d.Id.SubtypeId + v.Color)));
            // Returns null if it doesn't find the required SubtypeId.
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
                max.X = Math.Max(max.X, block.Min.X);       // TODO: resolve cubetype size.
                max.Y = Math.Max(max.Y, block.Min.Y);
                max.Z = Math.Max(max.Z, block.Min.Z);
            }

            // scale box to GridSize
            var size = max - min;
            var len = entity.GridSizeEnum.ToLength();
            size = new Vector3(size.X * len, size.Y * len, size.Z * len);

            // translate box according to min/max, but reset origin.
            var bb = new BoundingBox(new Vector3(0, 0, 0), size);

            // TODO: translate for rotation.
            //bb. ????

            // translate position.
            bb.Translate(entity.PositionAndOrientation.Value.Position);


            return bb;
        }

        #endregion

        public static string GetResourceName(string value)
        {
            if (value == null)
                return null;

            Sandbox.Common.Localization.MyTextsWrapperEnum myText;

            if (Enum.TryParse<Sandbox.Common.Localization.MyTextsWrapperEnum>(value, out myText))
            {
                try
                {
                    return Sandbox.Common.Localization.MyTextsWrapper.GetFormatString(myText);
                }
                catch
                {
                    return value;
                }
            }

            return value;
        }

        #region properties

        public static MyObjectBuilder_Definitions Definitions
        {
            get { return _definitions; }
        }

        #endregion
    }
}