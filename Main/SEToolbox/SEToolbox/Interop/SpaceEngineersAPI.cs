namespace SEToolbox.Interop
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Resources;
    using System.Xml;
    using Sandbox.Definitions;
    using SEToolbox.Support;
    using VRage;
    using VRage.Game;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;

    /// <summary>
    /// Helper api for accessing and interacting with Space Engineers content.
    /// </summary>
    public static class SpaceEngineersApi
    {
        private static readonly ResourceManager CustomGameResourceManager;

        static SpaceEngineersApi()
        {
            CustomGameResourceManager = new ResourceManager("SEToolbox.Properties.MyTexts", Assembly.GetExecutingAssembly());
        }

        #region Serializers

        public static T ReadSpaceEngineersFile<T>(Stream stream) where T : MyObjectBuilder_Base
        {
            T outObject;
            MyObjectBuilderSerializer.DeserializeXML<T>(stream, out outObject);
            return outObject;
        }

        public static bool TryReadSpaceEngineersFile<T>(string filename, out T outObject, out bool isCompressed, bool snapshot = false) where T : MyObjectBuilder_Base
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

                return MyObjectBuilderSerializer.DeserializeXML<T>(tempFilename, out outObject);
            }

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
                    xmlTextWriter.WriteComment(string.Format(" Saved '{0:o}' with SEToolbox version '{1}' ", DateTime.Now, GlobalSettings.GetAppVersion()));
                    xmlTextWriter.Flush();
                }
            }

            return true;
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
                foreach (var item in bp.Prerequisites)
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
            var bpList = SpaceEngineersCore.Resources.BlueprintDefinitions.Where(b => b.Results != null && b.Results.Any(r => r.Id.TypeId == resultTypeId && r.Id.SubtypeName == resultSubTypeId));
            return bpList.FirstOrDefault();
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
            MyTexts.LoadTexts(localizationPath, maincode, subcode);

            GlobalSettings.Default.UseCustomResource = !MyTexts.Languages.Any(m => m.Value.FullCultureName.Equals(culture.IetfLanguageTag, StringComparison.OrdinalIgnoreCase)
                || m.Value.CultureName.Equals(culture.Parent.IetfLanguageTag, StringComparison.OrdinalIgnoreCase));
        }

        #endregion

        #region GetResourceName

        public static string GetResourceName(string value)
        {
            if (value == null)
                return null;

            // This will load a custom localized Satellite Resource that the game doesn't offically support.
            if (GlobalSettings.Default.UseCustomResource)
            {
                string text = CustomGameResourceManager.GetString(value);
                if (text != null)
                    return text;
            }

            var stringId = MyStringId.GetOrCompute(value);
            return MyTexts.GetString(stringId);
        }

        #endregion
    }
}