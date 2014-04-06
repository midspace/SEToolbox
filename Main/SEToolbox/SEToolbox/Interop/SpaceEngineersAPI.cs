namespace SEToolbox.Interop
{
    using Microsoft.Xml.Serialization.GeneratedAssembly;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Common.ObjectBuilders.Definitions;
    using Sandbox.Common.ObjectBuilders.VRageData;
    using SEToolbox.Support;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using VRageMath;

    public class SpaceEngineersAPI
    {
        #region ctor

        static SpaceEngineersAPI()
        {
            // Dynamically read all definitions as soon as the SpaceEngineersAPI class is first invoked.
            ReadCubeBlockDefinitions();
        }

        public static void Init()
        {
            // Placeholder to make sure ctor is called.
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
            new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Backward, VRageMath.Base6Directions.Direction.Down),   // 180 around X
            new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Backward, VRageMath.Base6Directions.Direction.Up),     // 180 around Y
            new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Forward, VRageMath.Base6Directions.Direction.Down),    // 180 around Z

            new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Up, VRageMath.Base6Directions.Direction.Backward),     // +90 around X
            new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Down, VRageMath.Base6Directions.Direction.Forward),    // -90 around X
            new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Left, VRageMath.Base6Directions.Direction.Up),         // +90 around Y
            new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Right, VRageMath.Base6Directions.Direction.Up),        // -90 around Y
            new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Forward, VRageMath.Base6Directions.Direction.Left),    // +90 around Z
            new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Forward, VRageMath.Base6Directions.Direction.Right),   // -90 around Z

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
            // TODO: Remove the Cube Armor orientation, as these appear to work fine with the Generic.
            {CubeType.Cube, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Forward, VRageMath.Base6Directions.Direction.Up)},

            // TODO: Remove the Slope Armor orientations, as these appear to work fine with the Generic.
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

            {CubeType.InverseCornerLeftFrontTop, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Backward, VRageMath.Base6Directions.Direction.Right)},
            {CubeType.InverseCornerRightFrontTop, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Backward, VRageMath.Base6Directions.Direction.Down)}, // 180 around X
            {CubeType.InverseCornerLeftBackTop, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Down, VRageMath.Base6Directions.Direction.Right)},
            {CubeType.InverseCornerRightBackTop, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Down, VRageMath.Base6Directions.Direction.Forward)},  // -90 around X
            {CubeType.InverseCornerLeftFrontBottom, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Up, VRageMath.Base6Directions.Direction.Right)},
            {CubeType.InverseCornerRightFrontBottom, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Up, VRageMath.Base6Directions.Direction.Backward)}, // +90 around X
            {CubeType.InverseCornerLeftBackBottom, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Forward, VRageMath.Base6Directions.Direction.Right)}, // -90 around Z
            {CubeType.InverseCornerRightBackBottom, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Forward, VRageMath.Base6Directions.Direction.Up)},  // no rotation

            // Generic, which seems to work for everything but Corner armor blocks.
            {CubeType.Axis24_Backward_Down, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Backward, VRageMath.Base6Directions.Direction.Down)},
            {CubeType.Axis24_Backward_Left, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Backward, VRageMath.Base6Directions.Direction.Left)},
            {CubeType.Axis24_Backward_Right, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Backward, VRageMath.Base6Directions.Direction.Right)},
            {CubeType.Axis24_Backward_Up, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Backward, VRageMath.Base6Directions.Direction.Up)},
            {CubeType.Axis24_Down_Backward, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Down, VRageMath.Base6Directions.Direction.Backward)},
            {CubeType.Axis24_Down_Forward, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Down, VRageMath.Base6Directions.Direction.Forward)},
            {CubeType.Axis24_Down_Left, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Down, VRageMath.Base6Directions.Direction.Left)},
            {CubeType.Axis24_Down_Right, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Down, VRageMath.Base6Directions.Direction.Right)},
            {CubeType.Axis24_Forward_Down, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Forward, VRageMath.Base6Directions.Direction.Down)},
            {CubeType.Axis24_Forward_Left, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Forward, VRageMath.Base6Directions.Direction.Left)},
            {CubeType.Axis24_Forward_Right, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Forward, VRageMath.Base6Directions.Direction.Right)},
            {CubeType.Axis24_Forward_Up, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Forward, VRageMath.Base6Directions.Direction.Up)},
            {CubeType.Axis24_Left_Backward, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Left, VRageMath.Base6Directions.Direction.Backward)},
            {CubeType.Axis24_Left_Down, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Left, VRageMath.Base6Directions.Direction.Down)},
            {CubeType.Axis24_Left_Forward, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Left, VRageMath.Base6Directions.Direction.Forward)},
            {CubeType.Axis24_Left_Up, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Left, VRageMath.Base6Directions.Direction.Up)},
            {CubeType.Axis24_Right_Backward, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Right, VRageMath.Base6Directions.Direction.Backward)},
            {CubeType.Axis24_Right_Down, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Right, VRageMath.Base6Directions.Direction.Down)},
            {CubeType.Axis24_Right_Forward, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Right, VRageMath.Base6Directions.Direction.Forward)},
            {CubeType.Axis24_Right_Up, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Right, VRageMath.Base6Directions.Direction.Up)},
            {CubeType.Axis24_Up_Backward, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Up, VRageMath.Base6Directions.Direction.Backward)},
            {CubeType.Axis24_Up_Forward, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Up, VRageMath.Base6Directions.Direction.Forward)},
            {CubeType.Axis24_Up_Left, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Up, VRageMath.Base6Directions.Direction.Left)},
            {CubeType.Axis24_Up_Right, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Up, VRageMath.Base6Directions.Direction.Right)},
        };

        public static SerializableBlockOrientation GetCubeOrientation(CubeType type)
        {
            if (CubeOrientations.ContainsKey(type))
                return CubeOrientations[type];

            throw new NotImplementedException(string.Format("SetCubeOrientation of type [{0}] not yet implemented.", type));
        }

        #endregion

        #region ReadCubeBlockDefinitions

        static MyObjectBuilder_AmmoMagazineDefinitions _ammoMagazineDefinitions;
        static MyObjectBuilder_CubeBlockDefinitions _cubeBlockDefinitions;
        static MyObjectBuilder_ComponentDefinitions _componentDefinitions;
        static MyObjectBuilder_BlueprintDefinitions _blueprintDefinitions;
        static MyObjectBuilder_PhysicalItemDefinitions _physicalItemDefinitions;
        static MyObjectBuilder_VoxelMaterialDefinitions _voxelMaterialDefinitions;
        static Dictionary<string, byte> _materialIndex;
        static Dictionary<Type, MyObjectBuilderTypeEnum> _myObjectBuilderTypeList;

        public static void ReadCubeBlockDefinitions()
        {
            _ammoMagazineDefinitions = LoadContentFile<MyObjectBuilder_AmmoMagazineDefinitions, MyObjectBuilder_AmmoMagazineDefinitionsSerializer>("AmmoMagazines.sbc");
            _voxelMaterialDefinitions = LoadContentFile<MyObjectBuilder_VoxelMaterialDefinitions, MyObjectBuilder_VoxelMaterialDefinitionsSerializer>("VoxelMaterials.sbc");
            _physicalItemDefinitions = LoadContentFile<MyObjectBuilder_PhysicalItemDefinitions, MyObjectBuilder_PhysicalItemDefinitionsSerializer>("PhysicalItems.sbc");
            _componentDefinitions = LoadContentFile<MyObjectBuilder_ComponentDefinitions, MyObjectBuilder_ComponentDefinitionsSerializer>("Components.sbc");
            _cubeBlockDefinitions = LoadContentFile<MyObjectBuilder_CubeBlockDefinitions, MyObjectBuilder_CubeBlockDefinitionsSerializer>("CubeBlocks.sbc");
            _blueprintDefinitions = LoadContentFile<MyObjectBuilder_BlueprintDefinitions, MyObjectBuilder_BlueprintDefinitionsSerializer>("Blueprints.sbc");

            _myObjectBuilderTypeList = new Dictionary<Type, MyObjectBuilderTypeEnum>();
            foreach (MyObjectBuilderTypeEnum e in Enum.GetValues(typeof(MyObjectBuilderTypeEnum)))
            {
                try
                {
                    var t = MyObjectBuilder_Base.GetObjectBuilderToType(e);
                    _myObjectBuilderTypeList.Add(t, e);
                }
                catch
                {
                    // Ignore any that don't have a Type defined.
                }
            }

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

        public static MyObjectBuilder_ComponentDefinition[] ComponentDefinitions
        {
            get { return _componentDefinitions.Components; }
        }

        #region FetchCubeBlockMass

        public static float FetchCubeBlockMass(Type typeId, MyCubeSize cubeSize, string subTypeid)
        {
            float mass = 0;

            var cubeBlockDefinition = GetCubeDefinition(typeId, cubeSize, subTypeid);

            if (cubeBlockDefinition != null)
            {
                foreach (var component in cubeBlockDefinition.Components)
                {
                    mass += _componentDefinitions.Components.Where(c => c.Id.SubtypeId == component.Subtype).Sum(c => c.Mass) * component.Count;
                }
            }

            return mass;
        }

        public static void AccumulateCubeBlueprintRequirements(string subType, MyObjectBuilderTypeEnum type, decimal amount, Dictionary<string, MyObjectBuilder_BlueprintDefinition.Item> requirements, out TimeSpan timeTaken)
        {
            TimeSpan time = new TimeSpan();
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
                    time += ts;
                }
            }

            timeTaken = time;
        }

        public static float GetItemMass(Type type, string subTypeId)
        {
            return GetItemMass(_myObjectBuilderTypeList[type], subTypeId);
        }

        public static float GetItemMass(MyObjectBuilderTypeEnum typeId, string subTypeId)
        {
            var item = _physicalItemDefinitions.Definitions.FirstOrDefault(d => d.Id.TypeId == typeId && d.Id.SubtypeId == subTypeId);
            if (item != null)
            {
                return item.Mass;
            }
            else
            {
                var component = _componentDefinitions.Components.FirstOrDefault(c => c.Id.TypeId == typeId && c.Id.SubtypeId == subTypeId);
                if (component != null)
                {
                    return component.Mass;
                }
            }
            return 0;
        }

        public static string GetObjectBuilderName(Type type)
        {
            if (_myObjectBuilderTypeList.ContainsKey(type))
            {
                return _myObjectBuilderTypeList[type].ToString();
            }

            return null;
        }

        public static MyObjectBuilder_DefinitionBase GetDefinition(MyObjectBuilderTypeEnum typeId, string subTypeId)
        {
            var cube = _cubeBlockDefinitions.Definitions.FirstOrDefault(d => d.Id.TypeId == typeId && d.Id.SubtypeId == subTypeId);
            if (cube != null)
            {
                return cube;
            }

            var item = _physicalItemDefinitions.Definitions.FirstOrDefault(d => d.Id.TypeId == typeId && d.Id.SubtypeId == subTypeId);
            if (item != null)
            {
                return item;
            }

            var component = _componentDefinitions.Components.FirstOrDefault(c => c.Id.TypeId == typeId && c.Id.SubtypeId == subTypeId);
            if (component != null)
            {
                return component;
            }

            var magazine = _ammoMagazineDefinitions.AmmoMagazines.FirstOrDefault(c => c.Id.TypeId == typeId && c.Id.SubtypeId == subTypeId);
            if (magazine != null)
            {
                return magazine;
            }

            return null;
        }

        public static float GetItemVolume(Type type, string subTypeId)
        {
            return GetItemVolume(_myObjectBuilderTypeList[type], subTypeId);
        }

        public static float GetItemVolume(MyObjectBuilderTypeEnum typeId, string subTypeId)
        {
            var item = _physicalItemDefinitions.Definitions.FirstOrDefault(d => d.Id.TypeId == typeId && d.Id.SubtypeId == subTypeId);
            if (item != null)
            {
                if (item.Volume.HasValue)
                    return item.Volume.Value;
            }
            else
            {
                var component = _componentDefinitions.Components.FirstOrDefault(c => c.Id.TypeId == typeId && c.Id.SubtypeId == subTypeId);
                if (component != null)
                {
                    if (component.Volume.HasValue)
                        return component.Volume.Value;
                }
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

        #region GetCubeDefinition

        public static MyObjectBuilder_CubeBlockDefinition GetCubeDefinition(Type typeId, MyCubeSize cubeSize, string subtypeId)
        {
            if (string.IsNullOrEmpty(subtypeId))
            {
                return _cubeBlockDefinitions.Definitions.FirstOrDefault(d => d.CubeSize == cubeSize && d.Id.TypeId == _myObjectBuilderTypeList[typeId]);
            }

            return _cubeBlockDefinitions.Definitions.FirstOrDefault(d => d.Id.SubtypeId == subtypeId || (d.Variants != null && d.Variants.Any(v => subtypeId == d.Id.SubtypeId + v.Color)));
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