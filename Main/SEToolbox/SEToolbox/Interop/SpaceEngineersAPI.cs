namespace SEToolbox.Interop
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Resources;
    using System.Text;
    using System.Xml;
    using Sandbox.Definitions;
    using SEToolbox.Support;
    using VRage;
    using VRage.Game;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;
    using VRage.FileSystem;
    using System.Xml.Serialization;
    using Res = SEToolbox.Properties.Resources;

    /// <summary>
    /// Helper api for accessing and interacting with Space Engineers content.
    /// </summary>
    public static class SpaceEngineersApi
    {
        #region Serializers

        [Obsolete("does not cater for ProtoBuf binary serialized files. Use TryReadSpaceEngineersFile<T>().")]
        public static T ReadSpaceEngineersFile<T>(Stream stream) where T : MyObjectBuilder_Base
        {
            T outObject;
            MyObjectBuilderSerializer.DeserializeXML<T>(stream, out outObject);
            return outObject;
        }

        /// <returns>True if it sucessfully deserialized the file.</returns>
        public static bool TryReadSpaceEngineersFile<T>(string filename, out T outObject, out bool isCompressed, out string errorInformation, bool snapshot = false, bool specificExtension = false) where T : MyObjectBuilder_Base
        {
            string protoBufFile = null;
            if (specificExtension)
            {
                if ((Path.GetExtension(filename) ?? string.Empty).EndsWith(SpaceEngineersConsts.ProtobuffersExtension, StringComparison.OrdinalIgnoreCase))
                    protoBufFile = filename;
            }
            else
            {
                if ((Path.GetExtension(filename) ?? string.Empty).EndsWith(SpaceEngineersConsts.ProtobuffersExtension, StringComparison.OrdinalIgnoreCase))
                    protoBufFile = filename;
                else
                    protoBufFile = filename + SpaceEngineersConsts.ProtobuffersExtension;
            }

            if (protoBufFile != null && File.Exists(protoBufFile))
            {
                var tempFilename = protoBufFile;

                if (snapshot)
                {
                    // Snapshot used for Report on Dedicated servers to prevent locking of the orginal file whilst reading it.
                    tempFilename = TempfileUtil.NewFilename();
                    File.Copy(protoBufFile, tempFilename);
                }

                using (var fileStream = new FileStream(tempFilename, FileMode.Open, FileAccess.Read))
                {
                    var b1 = fileStream.ReadByte();
                    var b2 = fileStream.ReadByte();
                    isCompressed = (b1 == 0x1f && b2 == 0x8b);
                }

                bool retCode;
                try
                {
                    // A failure to load here, will only mean it falls back to try and read the xml file instead.
                    // So a file corruption could easily have been covered up.
                    retCode = MyObjectBuilderSerializer.DeserializePB<T>(tempFilename, out outObject);
                }
                catch (InvalidCastException ex)
                {
                    outObject = null;
                    errorInformation = string.Format(Res.ErrorLoadFileError, filename, ex.AllMessages());
                    return false;
                }
                if (retCode && outObject != null)
                {
                    errorInformation = null;
                    return true;
                }
                return TryReadSpaceEngineersFileXml(filename, out outObject, out isCompressed, out errorInformation, snapshot);
            }

            return TryReadSpaceEngineersFileXml(filename, out outObject, out isCompressed, out errorInformation, snapshot);
        }

        private static bool TryReadSpaceEngineersFileXml<T>(string filename, out T outObject, out bool isCompressed, out string errorInformation, bool snapshot = false) where T : MyObjectBuilder_Base
        {
            isCompressed = false;

            if (File.Exists(filename))
            {
                var tempFilename = filename;

                if (snapshot)
                {
                    // Snapshot used for Report on Dedicated servers to prevent locking of the orginal file whilst reading it.
                    tempFilename = TempfileUtil.NewFilename();
                    File.Copy(filename, tempFilename);
                }

                using (var fileStream = new FileStream(tempFilename, FileMode.Open, FileAccess.Read))
                {
                    var b1 = fileStream.ReadByte();
                    var b2 = fileStream.ReadByte();
                    isCompressed = (b1 == 0x1f && b2 == 0x8b);
                }

                return DeserializeXml<T>(tempFilename, out outObject, out errorInformation);
            }

            errorInformation = null;
            outObject = null;
            return false;
        }

        public static T Deserialize<T>(string xml) where T : MyObjectBuilder_Base
        {
            T outObject;
            using (var stream = new MemoryStream())
            {
                StreamWriter sw = new StreamWriter(stream);
                sw.Write(xml);
                sw.Flush();
                stream.Position = 0;

                MyObjectBuilderSerializer.DeserializeXML(stream, out outObject);
            }
            return outObject;
        }

        public static string Serialize<T>(MyObjectBuilder_Base item) where T : MyObjectBuilder_Base
        {
            using (var outStream = new MemoryStream())
            {
                if (MyObjectBuilderSerializer.SerializeXML(outStream, item))
                {
                    outStream.Position = 0;

                    StreamReader sw = new StreamReader(outStream);
                    return sw.ReadToEnd();
                }
            }
            return null;
        }

        public static bool WriteSpaceEngineersFile<T>(T myObject, string filename)
            where T : MyObjectBuilder_Base
        {
            bool ret;
            using (StreamWriter sw = new StreamWriter(filename))
            {
                ret = MyObjectBuilderSerializer.SerializeXML(sw.BaseStream, myObject);
                if (ret)
                {
                    var xmlTextWriter = new XmlTextWriter(sw.BaseStream, null);
                    xmlTextWriter.WriteString("\r\n");
                    xmlTextWriter.WriteComment($" Saved '{DateTime.Now:o}' with SEToolbox version '{GlobalSettings.GetAppVersion()}' ");
                    xmlTextWriter.Flush();
                }
            }

            return true;
        }

        public static bool WriteSpaceEngineersFilePB<T>(T myObject, string filename, bool compress)
            where T : MyObjectBuilder_Base
        {
            return MyObjectBuilderSerializer.SerializePB(filename, compress, myObject);
        }

        /// <returns>True if it sucessfully deserialized the file.</returns>
        public static bool DeserializeXml<T>(string filename, out T objectBuilder, out string errorInformation) where T : MyObjectBuilder_Base
        {
            bool result = false;
            objectBuilder = null;
            errorInformation = null;

            using (var fileStream = MyFileSystem.OpenRead(filename))
            {
                if (fileStream != null)
                    using (var readStream = fileStream.UnwrapGZip())
                    {
                        if (readStream != null)
                        {
                            try
                            {
                                XmlSerializer serializer = MyXmlSerializerManager.GetSerializer(typeof(T));

                                XmlReaderSettings settings = new XmlReaderSettings { CheckCharacters = true };
                                MyXmlTextReader xmlReader = new MyXmlTextReader(readStream, settings);

                                objectBuilder = (T)serializer.Deserialize(xmlReader);
                                result = true;
                            }
                            catch (Exception ex)
                            {
                                objectBuilder = null;
                                errorInformation = string.Format(Res.ErrorLoadFileError, filename, ex.AllMessages());
                            }
                        }
                    }
            }

            return result;
        }

        #endregion

        #region GenerateEntityId

        public static long GenerateEntityId(VRage.MyEntityIdentifier.ID_OBJECT_TYPE type)
        {
            return MyEntityIdentifier.AllocateId(type);
        }

        public static bool ValidateEntityType(VRage.MyEntityIdentifier.ID_OBJECT_TYPE type, long id)
        {
            return MyEntityIdentifier.GetIdObjectType(id) == type;
        }

        //public static long GenerateEntityId()
        //{
        //    // Not the offical SE way of generating IDs, but its fast and we don't have to worry about a random seed.
        //    var buffer = Guid.NewGuid().ToByteArray();
        //    return BitConverter.ToInt64(buffer, 0);
        //}

        #endregion

        #region FetchCubeBlockMass

        public static float FetchCubeBlockMass(MyObjectBuilderType typeId, MyCubeSize cubeSize, string subTypeid)
        {
            float mass = 0;

            var cubeBlockDefinition = GetCubeDefinition(typeId, cubeSize, subTypeid);

            if (cubeBlockDefinition != null)
            {
                return cubeBlockDefinition.Mass;
            }

            return mass;
        }

        public static void AccumulateCubeBlueprintRequirements(string subType, MyObjectBuilderType typeId, decimal amount, Dictionary<string, BlueprintRequirement> requirements, out TimeSpan timeTaken)
        {
            var time = new TimeSpan();
            var bp = SpaceEngineersApi.GetBlueprint(typeId, subType);

            if (bp != null && bp.Results != null && bp.Results.Length > 0)
            {
                foreach (MyBlueprintDefinitionBase.Item item in bp.Prerequisites)
                {
                    if (requirements.ContainsKey(item.Id.SubtypeName))
                    {
                        // append existing
                        requirements[item.Id.SubtypeName].Amount = ((amount / (decimal)bp.Results[0].Amount) * (decimal)item.Amount) + requirements[item.Id.SubtypeName].Amount;
                    }
                    else
                    {
                        // add new
                        requirements.Add(item.Id.SubtypeName, new BlueprintRequirement
                        {
                            Amount = (amount / (decimal)bp.Results[0].Amount) * (decimal)item.Amount,
                            TypeId = item.Id.TypeId.ToString(),
                            SubtypeId = item.Id.SubtypeName,
                            Id = item.Id
                        });
                    }

                    double timeMassMultiplyer = 1;
                    if (typeId == typeof(MyObjectBuilder_Ore) || typeId == typeof(MyObjectBuilder_Ingot))
                        timeMassMultiplyer = (double)bp.Results[0].Amount;

                    var ts = TimeSpan.FromSeconds(bp.BaseProductionTimeInSeconds * (double)amount / timeMassMultiplyer);
                    time += ts;
                }
            }

            timeTaken = time;
        }

        public static MyBlueprintDefinitionBase GetBlueprint(MyObjectBuilderType resultTypeId, string resultSubTypeId)
        {
            // Get 'Last' item. Matches SE logic, which uese an array structure, and overrides previous found items of the same result.
            return SpaceEngineersCore.Resources.BlueprintDefinitions.LastOrDefault(b => b.Results != null && b.Results.Length == 1 && b.Results.Any(r => r.Id.TypeId == resultTypeId && r.Id.SubtypeName == resultSubTypeId));
        }

        #endregion

        #region GetCubeDefinition

        public static MyCubeBlockDefinition GetCubeDefinition(MyObjectBuilderType typeId, MyCubeSize cubeSize, string subtypeName)
        {
            if (string.IsNullOrEmpty(subtypeName))
            {
                return SpaceEngineersCore.Resources.CubeBlockDefinitions.FirstOrDefault(d => d.CubeSize == cubeSize && d.Id.TypeId == typeId);
            }

            return SpaceEngineersCore.Resources.CubeBlockDefinitions.FirstOrDefault(d => d.Id.SubtypeName == subtypeName || (d.Variants != null && d.Variants.Any(v => subtypeName == d.Id.SubtypeName + v.Color)));
            // Returns null if it doesn't find the required SubtypeId.
        }

        #endregion

        #region GetBoundingBox

        public static BoundingBoxD GetBoundingBox(MyObjectBuilder_CubeGrid entity)
        {
            var min = new Vector3D(int.MaxValue, int.MaxValue, int.MaxValue);
            var max = new Vector3D(int.MinValue, int.MinValue, int.MinValue);

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
            size = new Vector3D(size.X * len, size.Y * len, size.Z * len);

            // translate box according to min/max, but reset origin.
            var bb = new BoundingBoxD(Vector3D.Zero, size);

            // TODO: translate for rotation.
            //bb. ????

            // translate position.
            bb.Translate(entity.PositionAndOrientation.Value.Position);


            return bb;
        }

        #endregion

        #region LoadLocalization

        public static void LoadLocalization()
        {
            var culture = System.Threading.Thread.CurrentThread.CurrentUICulture;
            var languageTag = culture.IetfLanguageTag;

            var contentPath = ToolboxUpdater.GetApplicationContentPath();
            var localizationPath = Path.Combine(contentPath, @"Data\Localization");

            var codes = languageTag.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
            var maincode = codes.Length > 0 ? codes[0] : null;
            var subcode = codes.Length > 1 ? codes[1] : null;

            MyTexts.Clear();

            if (GlobalSettings.Default.UseCustomResource.HasValue && GlobalSettings.Default.UseCustomResource.Value)
            {
                // no longer required, as Chinese is now officially in game.
                // left as an example for later additional custom languages.
                //AddLanguage(MyLanguagesEnum.ChineseChina, "zh-CN", null, "Chinese", 1f, true);
            }

            MyTexts.LoadTexts(localizationPath, maincode, subcode);

            if (GlobalSettings.Default.UseCustomResource.HasValue && GlobalSettings.Default.UseCustomResource.Value)
            {
                // Load alternate localization in instead using game refined resources, as they may not yet exist.
                ResourceManager customGameResourceManager = new ResourceManager("SEToolbox.Properties.MyTexts", Assembly.GetExecutingAssembly());
                ResourceSet customResourceSet = customGameResourceManager.GetResourceSet(culture, true, false);
                if (customResourceSet != null)
                {
                    // Reflection copy of MyTexts.PatchTexts(string resourceFile)
                    foreach (DictionaryEntry dictionaryEntry in customResourceSet)
                    {
                        string text = dictionaryEntry.Key as string;
                        string text2 = dictionaryEntry.Value as string;
                        if (text != null && text2 != null)
                        {
                            MyStringId orCompute = MyStringId.GetOrCompute(text);
                            Dictionary<MyStringId, string> m_strings = typeof(MyTexts).GetStaticField<Dictionary<MyStringId, string>>("m_strings");
                            Dictionary<MyStringId, StringBuilder> m_stringBuilders = typeof(MyTexts).GetStaticField<Dictionary<MyStringId, StringBuilder>>("m_stringBuilders");

                            m_strings[orCompute] = text2;
                            m_stringBuilders[orCompute] = new StringBuilder(text2);
                        }
                    }
                }
            }
        }

        #endregion

        #region GetResourceName

        public static string GetResourceName(string value)
        {
            if (value == null)
                return null;

            MyStringId stringId = MyStringId.GetOrCompute(value);
            return MyTexts.GetString(stringId);
        }

        // Reflection copy of MyTexts.AddLanguage
        private static void AddLanguage(MyLanguagesEnum id, string cultureName, string subcultureName = null, string displayName = null, float guiTextScale = 1f, bool isCommunityLocalized = true)
        {
            // Create an empty instance of LanguageDescription.
            MyTexts.MyLanguageDescription languageDescription = ReflectionUtil.ConstructPrivateClass<MyTexts.MyLanguageDescription>(
                new Type[] { typeof(MyLanguagesEnum), typeof(string), typeof(string), typeof(string), typeof(float), typeof(bool) },
                new object[] { id, displayName, cultureName, subcultureName, guiTextScale, isCommunityLocalized });

            Dictionary<MyLanguagesEnum, MyTexts.MyLanguageDescription> m_languageIdToLanguage = typeof(MyTexts).GetStaticField<Dictionary<MyLanguagesEnum, MyTexts.MyLanguageDescription>>("m_languageIdToLanguage");
            Dictionary<string, MyLanguagesEnum> m_cultureToLanguageId = typeof(MyTexts).GetStaticField<Dictionary<string, MyLanguagesEnum>>("m_cultureToLanguageId");

            if (!m_languageIdToLanguage.ContainsKey(id))
            {
                m_languageIdToLanguage.Add(id, languageDescription);
                m_cultureToLanguageId.Add(languageDescription.FullCultureName, id);
            }
        }

        #endregion
    }
}