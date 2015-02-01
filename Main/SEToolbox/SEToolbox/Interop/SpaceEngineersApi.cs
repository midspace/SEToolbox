namespace SEToolbox.Interop
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Xml;

    using Microsoft.Xml.Serialization.GeneratedAssembly;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Common.ObjectBuilders.Definitions;
    using SEToolbox.Support;
    using VRageMath;
    using VRage.Common;
    using VRage.Common.Utils;

    /// <summary>
    /// Helper api for accessing and interacting with Space Engineers content.
    /// </summary>
    public static class SpaceEngineersApi
    {
        #region Serializers

        public static T ReadSpaceEngineersFile<T, TS>(Stream stream)
        where TS : XmlSerializer1
        {
            var settings = new XmlReaderSettings
            {
                IgnoreComments = true,
                IgnoreWhitespace = true,
            };

            object obj;

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

        public static bool TryReadSpaceEngineersFile<T>(string filename, out T entity, out bool isCompressed)
        {
            try
            {
                entity = ReadSpaceEngineersFile<T>(filename, out isCompressed);
                return true;
            }
            catch
            {
                entity = default(T);
                isCompressed = false;
                return false;
            }
        }

        public static T ReadSpaceEngineersFile<T>(string filename)
        {
            bool isCompressed;
            return ReadSpaceEngineersFile<T>(filename, out isCompressed);
        }

        public static T ReadSpaceEngineersFile<T>(string filename, out bool isCompressed, bool snapshot = false)
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
                    fileStream.Position = 0;

                    if (isCompressed)
                    {
                        using (var outStream = new MemoryStream())
                        {
                            using (var zip = new GZipStream(fileStream, CompressionMode.Decompress))
                            {
                                zip.CopyTo(outStream);
                                Debug.WriteLine("Decompressed from {0:#,###0} bytes to {1:#,###0} bytes.", fileStream.Length, outStream.Length);
                            }
                            outStream.Position = 0;
                            return ReadSpaceEngineersFileRaw<T>(outStream);
                        }
                    }

                    return ReadSpaceEngineersFileRaw<T>(fileStream);
                }
            }

            return default(T);
        }

        public static T ReadSpaceEngineersFile<T>(Stream fileStream, out bool isCompressed)
        {
            isCompressed = false;

            if (fileStream != null)
            {
                var b1 = fileStream.ReadByte();
                var b2 = fileStream.ReadByte();
                isCompressed = (b1 == 0x1f && b2 == 0x8b);
                fileStream.Position = 0;

                if (isCompressed)
                {
                    using (var outStream = new MemoryStream())
                    {
                        using (var zip = new GZipStream(fileStream, CompressionMode.Decompress))
                        {
                            zip.CopyTo(outStream);
                            Debug.WriteLine("Decompressed from {0:#,###0} bytes to {1:#,###0} bytes.", fileStream.Length, outStream.Length);
                        }
                        outStream.Position = 0;
                        return ReadSpaceEngineersFileRaw<T>(outStream);
                    }
                }

                return ReadSpaceEngineersFileRaw<T>(fileStream);
            }

            return default(T);
        }

        public static T ReadSpaceEngineersFileRaw<T>(Stream stream)
        {
            var contract = new XmlSerializerContract();
            object obj = default(T);

            // Space Engineers is able to read partially corrupted xml files,
            // which means Keen probably aren't using any XML reader settings in general. 
            using (var xmlReader = XmlReader.Create(stream))
            {
                var serializer = contract.GetSerializer(typeof(T));
                if (serializer != null)
                    obj = serializer.Deserialize(xmlReader);
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

                    xmlTextWriter.WriteComment(string.Format(" Saved '{0:o}' with SEToolbox version '{1}' ", DateTime.Now, GlobalSettings.GetAppVersion()));
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

        #region FetchCubeBlockMass

        public static float FetchCubeBlockMass(MyObjectBuilderType typeId, MyCubeSize cubeSize, string subTypeid)
        {
            float mass = 0;

            var cubeBlockDefinition = GetCubeDefinition(typeId, cubeSize, subTypeid);

            if (cubeBlockDefinition != null)
            {
                foreach (var component in cubeBlockDefinition.Components)
                {
                    mass += SpaceEngineersCore.Resources.Definitions.Components.Where(c => c.Id.SubtypeId == component.Subtype).Sum(c => c.Mass) * component.Count;
                }
            }

            return mass;
        }

        public static void AccumulateCubeBlueprintRequirements(string subType, MyObjectBuilderType typeId, decimal amount, Dictionary<string, BlueprintRequirement> requirements, out TimeSpan timeTaken)
        {
            var time = new TimeSpan();
            var bp = SpaceEngineersApi.GetBlueprint(typeId, subType);
            if (bp != null)
            {
                foreach (var item in bp.Prerequisites)
                {
                    if (requirements.ContainsKey(item.SubtypeId))
                    {
                        // append existing
                        requirements[item.SubtypeId].Amount = ((amount / Convert.ToDecimal(bp.Result.Amount, CultureInfo.InvariantCulture)) * Convert.ToDecimal(item.Amount, CultureInfo.InvariantCulture)) + Convert.ToDecimal(requirements[item.SubtypeId].Amount, CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        // add new
                        requirements.Add(item.SubtypeId, new BlueprintRequirement
                        {
                            Amount = (amount / Convert.ToDecimal(bp.Result.Amount, CultureInfo.InvariantCulture)) * Convert.ToDecimal(item.Amount, CultureInfo.InvariantCulture),
                            TypeId = item.TypeId,
                            SubtypeId = item.SubtypeId,
                            Id = item.Id
                        });
                    }


                    decimal timeMassMultiplyer = 1;
                    if (typeId == typeof(MyObjectBuilder_Ore) || typeId == typeof(MyObjectBuilder_Ingot))
                        timeMassMultiplyer = decimal.Parse(bp.Result.Amount, CultureInfo.InvariantCulture);

                    var ticks = TimeSpan.TicksPerSecond * (decimal)bp.BaseProductionTimeInSeconds * amount / timeMassMultiplyer;
                    var ts = new TimeSpan((long)ticks);
                    time += ts;
                }
            }

            timeTaken = time;
        }

        public static MyObjectBuilder_DefinitionBase GetDefinition(MyObjectBuilderType typeId, string subTypeId)
        {
            var cube = SpaceEngineersCore.Resources.Definitions.CubeBlocks.FirstOrDefault(d => d.Id.TypeId == typeId && d.Id.SubtypeId == subTypeId);
            if (cube != null)
            {
                return cube;
            }

            var item = SpaceEngineersCore.Resources.Definitions.PhysicalItems.FirstOrDefault(d => d.Id.TypeId == typeId && d.Id.SubtypeId == subTypeId);
            if (item != null)
            {
                return item;
            }

            var component = SpaceEngineersCore.Resources.Definitions.Components.FirstOrDefault(c => c.Id.TypeId == typeId && c.Id.SubtypeId == subTypeId);
            if (component != null)
            {
                return component;
            }

            var magazine = SpaceEngineersCore.Resources.Definitions.AmmoMagazines.FirstOrDefault(c => c.Id.TypeId == typeId && c.Id.SubtypeId == subTypeId);
            if (magazine != null)
            {
                return magazine;
            }

            return null;
        }

        public static MyObjectBuilder_BlueprintDefinition GetBlueprint(MyObjectBuilderType resultTypeId, string resultSubTypeId)
        {
            var bp = SpaceEngineersCore.Resources.Definitions.Blueprints.FirstOrDefault(b => b.Result != null && b.Result.Id.TypeId == resultTypeId && b.Result.SubtypeId == resultSubTypeId);
            if (bp != null)
                return bp;

            var bpList = SpaceEngineersCore.Resources.Definitions.Blueprints.Where(b => b.Results != null && b.Results.Any(r => r.Id.TypeId == resultTypeId && r.SubtypeId == resultSubTypeId));
            return bpList.FirstOrDefault();
        }

        public static float GetItemMass(MyObjectBuilderType typeId, string subTypeId)
        {
            var def = GetDefinition(typeId, subTypeId);
            if (def is MyObjectBuilder_PhysicalItemDefinition)
            {
                var item2 = def as MyObjectBuilder_PhysicalItemDefinition;
                return item2.Mass;
            }

            return 0;
        }

        public static float GetItemVolume(MyObjectBuilderType typeId, string subTypeId)
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

        #endregion

        #region GetCubeDefinition

        public static MyObjectBuilder_CubeBlockDefinition GetCubeDefinition(MyObjectBuilderType typeId, MyCubeSize cubeSize, string subtypeId)
        {
            if (string.IsNullOrEmpty(subtypeId))
            {
                return SpaceEngineersCore.Resources.Definitions.CubeBlocks.FirstOrDefault(d => d.CubeSize == cubeSize && d.Id.TypeId == typeId);
            }

            return SpaceEngineersCore.Resources.Definitions.CubeBlocks.FirstOrDefault(d => d.Id.SubtypeId == subtypeId || (d.Variants != null && d.Variants.Any(v => subtypeId == d.Id.SubtypeId + v.Color)));
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
            var languageTag = System.Threading.Thread.CurrentThread.CurrentUICulture.IetfLanguageTag;

            var contentPath = ToolboxUpdater.GetApplicationContentPath();
            var localizationPath = Path.Combine(contentPath, @"Data\Localization");

            var codes = languageTag.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
            var maincode = codes.Length > 0 ? codes[0] : null;
            var subcode = codes.Length > 1 ? codes[1] : null;

            MyTexts.Clear();
            MyTexts.LoadTexts(localizationPath, typeof(MyTexts).Name, maincode, subcode);
        } 

        #endregion

        #region GetResourceName

        public static string GetResourceName(string value)
        {
            if (value == null)
                return null;

            var stringId = MyStringId.GetOrCompute(value);
            return MyTexts.GetString(stringId);
        }

        #endregion
    }
}