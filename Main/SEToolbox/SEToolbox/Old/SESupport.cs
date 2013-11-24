//namespace SEToolbox
//{
//    using SEToolbox.Enums;
//    using SEToolbox.ImageLibrary;
//    using SEToolbox.Models;
//    using SpreadsheetGear;
//    using System;
//    using System.Collections;
//    using System.Collections.Generic;
//    using System.Diagnostics;
//    using System.Drawing;
//    using System.Globalization;
//    using System.IO;
//    using System.Linq;
//    using System.Reflection;
//    using System.Windows;
//    using System.Windows.Media.Media3D;
//    using System.Xml;

//    internal class SESupport
//    {
//        #region LoadSaves

//        internal static List<SaveResource> LoadSaves()
//        {
//            List<SaveResource> list = new List<SaveResource>();
//            var filepath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"SpaceEngineers\Saves");

//            if (!Directory.Exists(filepath))
//            {
//                Console.WriteLine("Save folder location does not exist [{0}].", filepath);
//                return list;
//            }

//            var userPaths = Directory.GetDirectories(filepath);

//            foreach (var userPath in userPaths)
//            {
//                var savePaths = Directory.GetDirectories(userPath);

//                foreach (var savePath in savePaths)
//                {
//                    list.Add(new SaveResource()
//                        {
//                            Savename = Path.GetFileName(savePath),
//                            Username = Path.GetFileName(userPath),
//                            Savepath = savePath
//                        });
//                }
//            }

//            return list;
//        }

//        #endregion

//        #region GetSEDetails

//        internal static SEResource GetSEDetails(SaveResource save)
//        {
//            if (save == null)
//            {
//                return null;
//            }

//            save.Resource = new SEResource();

//            save.Resource.GlobalFile = Path.Combine(save.Savepath, "Sandbox.sbc");
//            save.Resource.SandboxFile = Path.Combine(save.Savepath, "SANDBOX_0_0_0_.sbs");

//            if (File.Exists(save.Resource.GlobalFile) && File.Exists(save.Resource.SandboxFile))
//            {
//                save.Resource.IsValid = true;
//            }

//            if (File.Exists(save.Resource.GlobalFile))
//            {
//                XmlDocument universeDoc = new XmlDocument();
//                universeDoc.Load(save.Resource.GlobalFile);
//                var nav = universeDoc.CreateNavigator();

//                save.Resource.Description = Support.GetXMLObject<string>(nav, "MyObjectBuilder_Checkpoint/Description");
//                save.Resource.LastSaved = Support.GetXMLObject<DateTimeOffset>(nav, "MyObjectBuilder_Checkpoint/LastSaveTime");
//            }

//            if (File.Exists(save.Resource.SandboxFile))
//            {
//                save.Resource.SandboxDoc = new XmlDocument();
//                save.Resource.SandboxDoc.Load(save.Resource.SandboxFile);
//                XmlNamespaceManager nsManager = Support.BuildXmlNamespaceManager(save.Resource.SandboxDoc);
//                var nav = save.Resource.SandboxDoc.CreateNavigator();

//                var nodecollection = nav.SelectSingleNode("MyObjectBuilder_Sector/SectorObjects");
//                var characterNode = nodecollection.SelectSingleNode("MyObjectBuilder_EntityBase[@xsi:type='MyObjectBuilder_Character']", nsManager);

//                if (characterNode == null)
//                {
//                    var pilotNode = nodecollection.Select("MyObjectBuilder_EntityBase[@xsi:type='MyObjectBuilder_CubeGrid']/CubeBlocks/MyObjectBuilder_CubeBlock[@xsi:type='MyObjectBuilder_Cockpit']/Pilot", nsManager);

//                    if (pilotNode.MoveNext())
//                    {
//                        characterNode = pilotNode.Current;
//                    }
//                }

//                if (characterNode != null)
//                {
//                    save.Resource.Character = new Character(characterNode);
//                    save.Resource.Character.CharacterType = Support.GetXMLObject<string>(characterNode, "CharacterModel");
//                    save.Resource.Character.Position = Support.GetXMLObject<Point3D>(characterNode, "PositionAndOrientation/Position");
//                }

//                save.Resource.Structures = new List<Structure>();
//                var objectNodes = nodecollection.Select("MyObjectBuilder_EntityBase");

//                while (objectNodes.MoveNext())
//                {
//                    var structure = new Structure(objectNodes.Current);
//                    save.Resource.Structures.Add(structure);
//                    structure.EntityId = Support.GetXMLObject<string>(objectNodes.Current, "EntityId");
//                    structure.StructureType = Support.GetXMLObject<string>(objectNodes.Current, "@xsi:type", nsManager);
//                    structure.PositionAndOrientation = new PositionAndOrientation(objectNodes.Current);
//                    structure.ClassType = ClassType.Unknown;

//                    if (structure.StructureType == "MyObjectBuilder_Character")
//                    {
//                        structure.ClassType = ClassType.Character;
//                    }
//                    else if (structure.StructureType == "MyObjectBuilder_VoxelMap")
//                    {
//                        structure.ClassType = ClassType.Voxel;
//                        structure.Filename = Support.GetXMLObject<string>(objectNodes.Current, "Filename");
//                    }
//                    else if (structure.StructureType == "MyObjectBuilder_CubeGrid")
//                    {
//                        structure.IsStatic = Support.GetXMLObject<bool>(objectNodes.Current, "IsStatic");
//                        structure.GridSize = Support.GetXMLObject<string>(objectNodes.Current, "GridSizeEnum");

//                        if (structure.IsStatic && structure.GridSize.ToUpper().Contains("LARGE"))
//                        {
//                            structure.ClassType = ClassType.Station;
//                        }
//                        else if (!structure.IsStatic && structure.GridSize.ToUpper().Contains("LARGE"))
//                        {
//                            structure.ClassType = ClassType.LargeShip;
//                        }
//                        else if (!structure.IsStatic && structure.GridSize.ToUpper().Contains("SMALL"))
//                        {
//                            structure.ClassType = ClassType.SmallShip;
//                        }

//                        structure.Cubes = new List<Cube>();

//                        var cubes = objectNodes.Current.Select("CubeBlocks/MyObjectBuilder_CubeBlock");

//                        //// SubtypeName='LargeBlockArmorBlock' or 'LargeBlockArmorSlope' or 'LargeBlockArmorBlockRed' or 'LargeBlockArmorSlopeWhite' or 'LargeBlockArmorCornerInvWhite'
//                        //var armourCount = objectNodes.Current.Select("CubeBlocks/MyObjectBuilder_CubeBlock[contains(SubtypeName, 'BlockArmor')]").Count;

//                        //// <MyObjectBuilder_CubeBlock xsi:type="MyObjectBuilder_Thrust"><SubtypeName>SmallBlockLargeThrust</SubtypeName>
//                        //var largeThrusterCount = objectNodes.Current.Select("CubeBlocks/MyObjectBuilder_CubeBlock[SubtypeName='SmallBlockLargeThrust']").Count;
//                        //var smallThrusterCount = objectNodes.Current.Select("CubeBlocks/MyObjectBuilder_CubeBlock[SubtypeName='SmallBlockSmallThrust']").Count;

//                        //var gyroCount = objectNodes.Current.Select("CubeBlocks/MyObjectBuilder_CubeBlock[SubtypeName='SmallBlockGyro']").Count;
//                        //var gravityGeneratorCount = objectNodes.Current.Select("CubeBlocks/MyObjectBuilder_CubeBlock[@xsi:type='MyObjectBuilder_GravityGenerator']", nsManager).Count;
//                        //var reactorCount = objectNodes.Current.Select("CubeBlocks/MyObjectBuilder_CubeBlock[@xsi:type='MyObjectBuilder_Reactor']", nsManager).Count;

//                        //Console.WriteLine("    Objects: {0:#,##0}", cubes.Count);
//                        //Console.WriteLine("    Armour blocks: {0:#,##0}", armourCount);
//                        //Console.WriteLine("    Reactors: {0:#,##0}", reactorCount);
//                        //Console.WriteLine("    Large Thrusters: {0:#,##0}", largeThrusterCount);
//                        //Console.WriteLine("    Small Thrusters: {0:#,##0}", smallThrusterCount);
//                        //Console.WriteLine("    Gyros: {0:#,##0}", gyroCount);
//                        //Console.WriteLine("    Gravity Generators: {0:#,##0}", gravityGeneratorCount);

//                        var min = new Point3D(double.MaxValue, double.MaxValue, double.MaxValue);
//                        var max = new Point3D(double.MinValue, double.MinValue, double.MinValue);

//                        while (cubes.MoveNext())
//                        {
//                            var subtype = Support.GetXMLObject<string>(cubes.Current, "SubtypeName");

//                            var cube = new Cube()
//                            {
//                                TypeName = Support.GetXMLObject<string>(cubes.Current, "@xsi:type", nsManager),
//                                SubtypeName = subtype == string.Empty ? SubtypeId.Empty : (SubtypeId)Enum.Parse(typeof(SubtypeId), subtype),
//                                PersistentFlags = Support.GetXMLObject<string>(cubes.Current, "PersistentFlags"),
//                                EntityId = Support.GetXMLObject<string>(cubes.Current, "EntityId"),
//                                //PositionAndOrientation = new PositionAndOrientation(cubes.Current),
//                                //Position = Support.GetXMLObject<Point3D>(cubes.Current, "Position"),
//                                Orientation = Support.GetXMLObject<Point4D>(cubes.Current, "Orientation"),
//                                Min = Support.GetXMLObject<Point3D>(cubes.Current, "Min"),
//                                Max = Support.GetXMLObject<Point3D>(cubes.Current, "Max")
//                            };

//                            min.X = Math.Min(min.X, cube.Min.X);
//                            min.Y = Math.Min(min.Y, cube.Min.Y);
//                            min.Z = Math.Min(min.Z, cube.Min.Z);
//                            max.X = Math.Max(max.X, cube.Max.X);
//                            max.Y = Math.Max(max.Y, cube.Max.Y);
//                            max.Z = Math.Max(max.Z, cube.Max.Z);

//                            structure.Cubes.Add(cube);
//                        }

//                        var size = max - min;
//                        size.X++;
//                        size.Y++;
//                        size.Z++;

//                        structure.Min = min;
//                        structure.Max = max;
//                        structure.Size = size;
//                    }
//                }
//            }

//            return save.Resource;
//        }

//        #endregion

//        #region ReadStructure

//        internal static void ReadStructure(Structure structure, string excelFilename)
//        {
//            excelFilename = Path.GetFullPath(excelFilename);
//            var isStatic = Support.GetXMLObject<bool>(structure.Node, "IsStatic");
//            var gridSize = Support.GetXMLObject<string>(structure.Node, "GridSizeEnum");
//            var entityId = Support.GetXMLObject<string>(structure.Node, "EntityId");
//            IWorkbook workbook = SpreadsheetGear.Factory.GetWorkbook(CultureInfo.CurrentCulture);

//            foreach (Cube cube in structure.Cubes)
//            {
//                // X == Columns
//                // Z == Rows
//                // Y == Sheets

//                var column = cube.Min.X - structure.Min.X;
//                var row = cube.Min.Z - structure.Min.Z;
//                var sheet = cube.Min.Y - structure.Min.Y;

//                // TODO: work out max value positioning.

//                if (workbook.Worksheets.Count < sheet + 1)
//                {
//                    while (workbook.Worksheets.Count < sheet + 1)
//                    {
//                        workbook.Worksheets.Add();
//                    }
//                };

//                var worksheet = workbook.Worksheets[(int)sheet];
//                worksheet.Cells[(int)row, (int)column].Value =
//                    string.Format("{0} O={1}", cube.SubtypeName, cube.Orientation);
//                    //string.Format("{0} F={1} U={2} O={3}", cube.SubtypeName,
//                    //cube.PositionAndOrientation.Forward,
//                    //cube.PositionAndOrientation.Up,
//                    //cube.Orientation);
//                // TODO: different font/size/color format?
//            }

//            if (File.Exists(excelFilename))
//            {
//                File.Delete(excelFilename);
//            }

//            FileFormat fileformat = FileFormat.Excel8;
//            if (Path.GetExtension(excelFilename).ToUpper() == ".XLSX")
//            {
//                fileformat = FileFormat.OpenXMLWorkbook;
//            }

//            workbook.SaveAs(excelFilename, fileformat);
//            workbook.Close();
//        }

//        #endregion

//        #region ImportStructure

//        internal static void ImportStructure(Structure structure, string polyFilename)
//        {
//            ConvertPolyToStructure(structure, polyFilename, false, false, 0);
//            // TODO:
//        }

//        #endregion

//        #region MergeStructure

//        internal static void MergeStructure(SaveResource save, Structure structure, string polyFilename)
//        {
//            var cubes = ConvertPolyToStructure(structure, polyFilename, true, false, 5);
//            // TODO:

//            structure.Cubes.Clear();
//            structure.Cubes.AddRange(cubes);
//            WriteOutCubeBlocks(save, structure);
//        }

//        #endregion

//        #region MergeTestStructure

//        internal static void MergeTestStructure(SaveResource save, Structure structure)
//        {
//            var cubes = CreatTestStructure(structure, true);
//            structure.Cubes.Clear();
//            structure.Cubes.AddRange(cubes);
//            WriteOutCubeBlocks(save, structure);
//        }

//        #endregion

//        #region MergeImage

//        internal static void MergeImage(SaveResource save, Structure structure, string imageFilename)
//        {
//            var cubes = ConvertImageToStructure(structure, imageFilename);
//            // TODO:

//            structure.Cubes.Clear();
//            structure.Cubes.AddRange(cubes);
//            WriteOutCubeBlocks(save, structure);
//        }

//        #endregion

//        #region ConvertPolyToStructure

//        private static List<Cube> ConvertPolyToStructure(Structure structure, string polyFilename, bool smoothObject, bool fillObject, int fixScale)
//        {
//            var voxFilename = ConvertPolyToVox(polyFilename, fixScale);
//            if (voxFilename == null)
//            {
//                MessageBox.Show("Unable to read the specified file. Please make sure it is a compatible format.", "Error reading file", MessageBoxButton.OK, MessageBoxImage.Error);
//                return null;
//            }

//            //double scaleFactor = 2.5;
//            SubtypeId blockType = SubtypeId.LargeBlockArmorBlock;
//            SubtypeId slopeBlockType = SubtypeId.LargeBlockArmorSlope;
//            SubtypeId cornerBlockType = SubtypeId.LargeBlockArmorCorner;
//            CubeType[, ,] ccubic;

//            if (structure.ClassType == ClassType.SmallShip)
//            {
//                //scaleFactor = 0.5;
//                blockType = SubtypeId.SmallBlockArmorBlock;
//                slopeBlockType = SubtypeId.SmallBlockArmorSlope;
//                cornerBlockType = SubtypeId.SmallBlockArmorCorner;
//            }

//            #region Read in voxel and set main cube space.

//            using (BinaryReader reader = new BinaryReader(File.Open(voxFilename, FileMode.Open)))
//            {
//                int xCount = reader.ReadInt32();
//                int yCount = reader.ReadInt32();
//                int zCount = reader.ReadInt32();
//                ccubic = new CubeType[xCount, yCount, zCount];

//                for (int x = 0; x < xCount; x++)
//                {
//                    for (int y = 0; y < yCount; y++)
//                    {
//                        for (int z = 0; z < zCount; z++)
//                        {
//                            var b = reader.ReadByte();

//                            switch (b)
//                            {
//                                case 0x00: // hollow interior
//                                    if (fillObject)
//                                    {
//                                        ccubic[x, y, z] = CubeType.Cube;
//                                    }
//                                    else
//                                    {
//                                        ccubic[x, y, z] = CubeType.Interior;
//                                    }
//                                    break;

//                                case 0xFF: // space
//                                    ccubic[x, y, z] = CubeType.None;
//                                    break;

//                                case 0x12: // solid
//                                default:
//                                    ccubic[x, y, z] = CubeType.Cube;
//                                    break;
//                            }
//                        }
//                    }
//                }
//            }

//            File.Delete(voxFilename);

//            #endregion

//            /*
//                     (top)+z|   /-y(back)  
//                            |  /      
//                            | /       
//               -x(left)     |/       +x(right)
//               -------------+-----------------
//                           /|
//                          / |
//                         /  |
//               (front)+y/   |-z(bottom)
//             */

//            if (smoothObject)
//            {
//                CalculateSlopes(ccubic);
//                CalculateCorners(ccubic);
//                CalculateInverseCorners(ccubic);
//            }

//            var cubes = BuildStructureFromCubic(ccubic, blockType, slopeBlockType, cornerBlockType);

//            return cubes;
//        }

//        #endregion

//        #region CreatTestStructure

//        private static List<Cube> CreatTestStructure(Structure structure, bool smoothObject)
//        {
//            //double scaleFactor = 2.5;
//            SubtypeId blockType = SubtypeId.LargeBlockArmorBlock;
//            SubtypeId slopeBlockType = SubtypeId.LargeBlockArmorSlope;
//            SubtypeId cornerBlockType = SubtypeId.LargeBlockArmorCorner;
//            CubeType[, ,] ccubic;

//            if (structure.ClassType == ClassType.SmallShip)
//            {
//                //scaleFactor = 0.5;
//                blockType = SubtypeId.SmallBlockArmorBlock;
//                slopeBlockType = SubtypeId.SmallBlockArmorSlope;
//                cornerBlockType = SubtypeId.SmallBlockArmorCorner;
//            }

//            #region Read in voxel and set main cube space.

//            ccubic = new CubeType[5, 5, 5];

//            ccubic[2, 2, 2] = CubeType.Cube;

//            for (int i = 1; i < 4; i++)
//            {
//                for (int j = 1; j < 4; j++)
//                {
//                    ccubic[i, j, 2] = CubeType.Cube;
//                    ccubic[i, 2, j] = CubeType.Cube;
//                    ccubic[2, i, j] = CubeType.Cube;
//                }
//            }

//            for (int i = 0; i < 5; i++)
//            {
//                ccubic[i, 2, 2] = CubeType.Cube;
//                ccubic[2, i, 2] = CubeType.Cube;
//                ccubic[2, 2, i] = CubeType.Cube;
//            }

//            #endregion

//            /*
//                     (top)+z|   /-y(back)  
//                            |  /      
//                            | /       
//               -x(left)     |/       +x(right)
//               -------------+-----------------
//                           /|
//                          / |
//                         /  |
//               (front)+y/   |-z(bottom)
//             */

//            if (smoothObject)
//            {
//                CalculateSlopes(ccubic);
//                CalculateCorners(ccubic);
//                CalculateInverseCorners(ccubic);
//            }

//            var cubes = BuildStructureFromCubic(ccubic, blockType, slopeBlockType, cornerBlockType);

//            return cubes;
//        }

//        #endregion

//        #region CalculateInverseCorners

//        private static void CalculateInverseCorners(CubeType[, ,] ccubic)
//        {
//            var xCount = ccubic.GetLength(0);
//            var yCount = ccubic.GetLength(1);
//            var zCount = ccubic.GetLength(2);

//            for (int x = 0; x < xCount; x++)
//            {
//                for (int y = 0; y < yCount; y++)
//                {
//                    for (int z = 0; z < zCount; z++)
//                    {
//                        // TODO: Inverse Corner
//                    }
//                }
//            }
//        }

//        #endregion

//        #region CalculateSlopes

//        private static void CalculateSlopes(CubeType[, ,] ccubic)
//        {
//            var xCount = ccubic.GetLength(0);
//            var yCount = ccubic.GetLength(1);
//            var zCount = ccubic.GetLength(2);

//            for (int x = 0; x < xCount; x++)
//            {
//                for (int y = 0; y < yCount; y++)
//                {
//                    for (int z = 0; z < zCount; z++)
//                    {
//                        if (ccubic[x, y, z] == CubeType.None)
//                        {
//                            if (CheckAdjacentCubic(ccubic, x, y, z, xCount, yCount, zCount, 0, 1, 1))
//                            {
//                                ccubic[x, y, z] = CubeType.SlopeCenterFrontTop;
//                            }
//                            else if (CheckAdjacentCubic(ccubic, x, y, z, xCount, yCount, zCount, -1, 1, 0))
//                            {
//                                ccubic[x, y, z] = CubeType.SlopeLeftFrontCenter;
//                            }
//                            else if (CheckAdjacentCubic(ccubic, x, y, z, xCount, yCount, zCount, 1, 1, 0))
//                            {
//                                ccubic[x, y, z] = CubeType.SlopeRightFrontCenter;
//                            }
//                            else if (CheckAdjacentCubic(ccubic, x, y, z, xCount, yCount, zCount, 0, 1, -1))
//                            {
//                                ccubic[x, y, z] = CubeType.SlopeCenterFrontBottom;
//                            }
//                            else if (CheckAdjacentCubic(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 1))
//                            {
//                                ccubic[x, y, z] = CubeType.SlopeLeftCenterTop;
//                            }
//                            else if (CheckAdjacentCubic(ccubic, x, y, z, xCount, yCount, zCount, 1, 0, 1))
//                            {
//                                ccubic[x, y, z] = CubeType.SlopeRightCenterTop;
//                            }
//                            else if (CheckAdjacentCubic(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, -1))
//                            {
//                                ccubic[x, y, z] = CubeType.SlopeLeftCenterBottom;
//                            }
//                            else if (CheckAdjacentCubic(ccubic, x, y, z, xCount, yCount, zCount, 1, 0, -1))
//                            {
//                                ccubic[x, y, z] = CubeType.SlopeRightCenterBottom;
//                            }
//                            else if (CheckAdjacentCubic(ccubic, x, y, z, xCount, yCount, zCount, 0, -1, 1))
//                            {
//                                ccubic[x, y, z] = CubeType.SlopeCenterBackTop;
//                            }
//                            else if (CheckAdjacentCubic(ccubic, x, y, z, xCount, yCount, zCount, -1, -1, 0))
//                            {
//                                ccubic[x, y, z] = CubeType.SlopeLeftBackCenter;
//                            }
//                            else if (CheckAdjacentCubic(ccubic, x, y, z, xCount, yCount, zCount, 1, -1, 0))
//                            {
//                                ccubic[x, y, z] = CubeType.SlopeRightBackCenter;
//                            }
//                            else if (CheckAdjacentCubic(ccubic, x, y, z, xCount, yCount, zCount, 0, -1, -1))
//                            {
//                                ccubic[x, y, z] = CubeType.SlopeCenterBackBottom;
//                            }
//                        }
//                    }
//                }
//            }

//        } 

//        #endregion

//        #region CalculateCorners

//        private static void CalculateCorners(CubeType[, ,] ccubic)
//        {
//            var xCount = ccubic.GetLength(0);
//            var yCount = ccubic.GetLength(1);
//            var zCount = ccubic.GetLength(2);

//            for (int x = 0; x < xCount; x++)
//            {
//                for (int y = 0; y < yCount; y++)
//                {
//                    for (int z = 0; z < zCount; z++)
//                    {
//                        if (ccubic[x, y, z] == CubeType.None)
//                        {
//                            // Red 1,3,5 CubeType.SlopeCenterFrontTop
//                            // Yellow 0,3,6 CubeType.SlopeLeftFrontCenter
//                            // Blue 0,3,5 CubeType.CornerLeftFrontTop;

//                            if ((CheckAdjacentSlope(ccubic, x, y, z, xCount, yCount, zCount, 0, 0, +1, CubeType.SlopeLeftFrontCenter, -1, 0, 0, CubeType.SlopeCenterFrontTop)) ||
//                                (CheckAdjacentSlope(ccubic, x, y, z, xCount, yCount, zCount, 0, 0, +1, CubeType.SlopeLeftFrontCenter, 0, -1, 0, CubeType.SlopeLeftCenterTop)) ||
//                                (CheckAdjacentSlope(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.SlopeCenterFrontTop, 0, -1, 0, CubeType.SlopeLeftCenterTop)))
//                            {
//                                ccubic[x, y, z] = CubeType.CornerLeftBackTop;
//                            }
//                            else if ((CheckAdjacentSlope(ccubic, x, y, z, xCount, yCount, zCount, 0, 0, +1, CubeType.SlopeRightFrontCenter, +1, 0, 0, CubeType.SlopeCenterFrontTop)) ||
//                                (CheckAdjacentSlope(ccubic, x, y, z, xCount, yCount, zCount, 0, 0, +1, CubeType.SlopeRightFrontCenter, 0, -1, 0, CubeType.SlopeRightCenterTop)) ||
//                                (CheckAdjacentSlope(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.SlopeCenterFrontTop, 0, -1, 0, CubeType.SlopeLeftCenterTop)))
//                            {
//                                ccubic[x, y, z] = CubeType.CornerRightFrontTop;
//                            }
//                            else if ((CheckAdjacentSlope(ccubic, x, y, z, xCount, yCount, zCount, 0, 0, -1, CubeType.SlopeLeftFrontCenter, -1, 0, 0, CubeType.SlopeCenterFrontBottom)) ||
//                               (CheckAdjacentSlope(ccubic, x, y, z, xCount, yCount, zCount, 0, 0, -1, CubeType.SlopeLeftFrontCenter, 0, -1, 0, CubeType.SlopeLeftCenterBottom)) ||
//                               (CheckAdjacentSlope(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.SlopeCenterFrontTop, 0, -1, 0, CubeType.SlopeLeftCenterBottom)))
//                            {
//                                ccubic[x, y, z] = CubeType.CornerLeftFrontBottom;
//                            }
//                            else if ((CheckAdjacentSlope(ccubic, x, y, z, xCount, yCount, zCount, 0, 0, -1, CubeType.SlopeRightFrontCenter, +1, 0, 0, CubeType.SlopeCenterFrontBottom)) ||
//                                (CheckAdjacentSlope(ccubic, x, y, z, xCount, yCount, zCount, 0, 0, -1, CubeType.SlopeRightFrontCenter, 0, -1, 0, CubeType.SlopeRightCenterBottom)) ||
//                                (CheckAdjacentSlope(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.SlopeCenterFrontBottom, 0, -1, 0, CubeType.SlopeLeftCenterBottom)))
//                            {
//                                ccubic[x, y, z] = CubeType.CornerRightFrontBottom;
//                            }
//                            else if ((CheckAdjacentSlope(ccubic, x, y, z, xCount, yCount, zCount, 0, 0, +1, CubeType.SlopeLeftBackCenter, -1, 0, 0, CubeType.SlopeCenterBackTop)) ||
//                                (CheckAdjacentSlope(ccubic, x, y, z, xCount, yCount, zCount, 0, 0, +1, CubeType.SlopeLeftBackCenter, 0, +1, 0, CubeType.SlopeLeftCenterTop)) ||
//                                (CheckAdjacentSlope(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.SlopeCenterBackTop, 0, +1, 0, CubeType.SlopeLeftCenterTop)))
//                            {
//                                ccubic[x, y, z] = CubeType.CornerLeftFrontTop;
//                            }
//                            else if ((CheckAdjacentSlope(ccubic, x, y, z, xCount, yCount, zCount, 0, 0, +1, CubeType.SlopeRightBackCenter, +1, 0, 0, CubeType.SlopeCenterBackTop)) ||
//                                (CheckAdjacentSlope(ccubic, x, y, z, xCount, yCount, zCount, 0, 0, +1, CubeType.SlopeRightBackCenter, 0, +1, 0, CubeType.SlopeRightCenterTop)) ||
//                                (CheckAdjacentSlope(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.SlopeCenterBackTop, 0, +1, 0, CubeType.SlopeLeftCenterTop)))
//                            {
//                                ccubic[x, y, z] = CubeType.CornerRightBackTop;
//                            }
//                            else if ((CheckAdjacentSlope(ccubic, x, y, z, xCount, yCount, zCount, 0, 0, -1, CubeType.SlopeLeftBackCenter, -1, 0, 0, CubeType.SlopeCenterBackBottom)) ||
//                               (CheckAdjacentSlope(ccubic, x, y, z, xCount, yCount, zCount, 0, 0, -1, CubeType.SlopeLeftBackCenter, 0, +1, 0, CubeType.SlopeLeftCenterBottom)) ||
//                               (CheckAdjacentSlope(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.SlopeCenterBackTop, 0, +1, 0, CubeType.SlopeLeftCenterBottom)))
//                            {
//                                ccubic[x, y, z] = CubeType.CornerLeftBackBottom;
//                            }
//                            else if ((CheckAdjacentSlope(ccubic, x, y, z, xCount, yCount, zCount, 0, 0, -1, CubeType.SlopeRightBackCenter, +1, 0, 0, CubeType.SlopeCenterBackBottom)) ||
//                                (CheckAdjacentSlope(ccubic, x, y, z, xCount, yCount, zCount, 0, 0, -1, CubeType.SlopeRightBackCenter, 0, +1, 0, CubeType.SlopeRightCenterBottom)) ||
//                                (CheckAdjacentSlope(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.SlopeCenterBackBottom, 0, +1, 0, CubeType.SlopeLeftCenterBottom)))
//                            {
//                                ccubic[x, y, z] = CubeType.CornerRightBackBottom;
//                            }
//                        }
//                    }
//                }
//            }
//        } 

//        #endregion

//        #region BuildStructureFromCubic

//        private static List<Cube> BuildStructureFromCubic(CubeType[, ,] ccubic, SubtypeId blockType, SubtypeId slopeBlockType, SubtypeId cornerBlockType)
//        {
//            var xCount = ccubic.GetLength(0);
//            var yCount = ccubic.GetLength(1);
//            var zCount = ccubic.GetLength(2);

//            List<Cube> cubes = new List<Cube>();
//            Cube newCube;

//            for (int x = 0; x < xCount; x++)
//            {
//                for (int y = 0; y < yCount; y++)
//                {
//                    for (int z = 0; z < zCount; z++)
//                    {
//                        if (ccubic[x, y, z] != CubeType.None && ccubic[x, y, z] != CubeType.Interior)
//                        {
//                            cubes.Add(newCube = new Cube());

//                            if (ccubic[x, y, z].ToString().StartsWith("Cube"))
//                            {
//                                newCube.SubtypeName = blockType;
//                            }
//                            else if (ccubic[x, y, z].ToString().StartsWith("Slope"))
//                            {
//                                newCube.SubtypeName = slopeBlockType;
//                            }
//                            else if (ccubic[x, y, z].ToString().StartsWith("Corner"))
//                            {
//                                newCube.SubtypeName = cornerBlockType;
//                                //if (CubeType.CornerLeftFrontTop == ccubic[x, y, z]) newCube.SubtypeName = SubtypeId.LargeBlockArmorCornerRed;
//                                //if (CubeType.CornerRightFrontTop == ccubic[x, y, z]) newCube.SubtypeName = SubtypeId.LargeBlockArmorCornerYellow;
//                                //if (CubeType.CornerLeftBackTop == ccubic[x, y, z]) newCube.SubtypeName = SubtypeId.LargeBlockArmorCornerBlue;
//                                //if (CubeType.CornerRightBackTop == ccubic[x, y, z]) newCube.SubtypeName = SubtypeId.LargeBlockArmorCornerGreen;
//                                //if (CubeType.CornerLeftFrontBottom == ccubic[x, y, z]) newCube.SubtypeName = SubtypeId.LargeBlockArmorCornerGreen;
//                                //if (CubeType.CornerRightFrontBottom == ccubic[x, y, z]) newCube.SubtypeName = SubtypeId.LargeBlockArmorCornerRed;
//                                //if (CubeType.CornerLeftBackBottom == ccubic[x, y, z]) newCube.SubtypeName = SubtypeId.LargeBlockArmorCornerWhite;
//                                //if (CubeType.CornerRightBackBottom == ccubic[x, y, z]) newCube.SubtypeName = SubtypeId.LargeBlockArmorCornerGreen;
//                                //newCube.SubtypeName = cornerBlockType;
//                            }

//                            newCube.EntityId = GenerateEntityId().ToString();
//                            newCube.PersistentFlags = "Enabled CastShadows";
//                            //newCube.PositionAndOrientation = new PositionAndOrientation();
//                            //newCube.PositionAndOrientation.Position = new Point3D(x * scaleFactor, y * scaleFactor, z * scaleFactor);
//                            SetCubeOrientation(newCube, ccubic[x, y, z]);
//                            newCube.Min = new Point3D(x, y, z);
//                            newCube.Max = new Point3D(x, y, z);
//                        }
//                    }
//                }
//            }

//            return cubes;
//        } 

//        #endregion


//        #region CheckAdjacentCubic

//        private static bool IsValidRange(int x, int y, int z, int xCount, int yCount, int zCount, int xDelta, int yDelta, int zDelta)
//        {
//            if (x + xDelta >= 0 && x + xDelta < xCount
//            && y + yDelta >= 0 && y + yDelta < yCount
//            && z + zDelta >= 0 && z + zDelta < zCount)
//            {
//                return true;
//            }

//            return false;
//        }

//        private static bool CheckAdjacentCubic(CubeType[, ,] ccubic, int x, int y, int z, int xCount, int yCount, int zCount, int xDelta, int yDelta, int zDelta)
//        {
//            if (ccubic[x, y, z] == CubeType.None && IsValidRange(x, y, z, xCount, yCount, zCount, xDelta, yDelta, zDelta))
//            {
//                if (xDelta != 0 && ccubic[x + xDelta, y, z] == CubeType.Cube &&
//                    yDelta != 0 && ccubic[x, y + yDelta, z] == CubeType.Cube &&
//                    zDelta == 0)
//                {
//                    return true;
//                }

//                if (xDelta != 0 && ccubic[x + xDelta, y, z] == CubeType.Cube &&
//                    yDelta == 0 &&
//                    zDelta != 0 && ccubic[x, y, z + zDelta] == CubeType.Cube)
//                {
//                    return true;
//                }

//                if (xDelta == 0 &&
//                    yDelta != 0 && ccubic[x, y + yDelta, z] == CubeType.Cube &&
//                    zDelta != 0 && ccubic[x, y, z + zDelta] == CubeType.Cube)
//                {
//                    return true;
//                }

//                if (xDelta != 0 && ccubic[x + xDelta, y, z] == CubeType.Cube &&
//                    yDelta != 0 && ccubic[x, y + yDelta, z] == CubeType.Cube &&
//                    zDelta != 0 && ccubic[x, y, z + zDelta] == CubeType.Cube)
//                {
//                    return true;
//                }
//            }

//            return false;
//        }

//        private static bool CheckAdjacentSlope(CubeType[, ,] ccubic, int x, int y, int z, int xCount, int yCount, int zCount, 
//            int xDelta1, int yDelta1, int zDelta1, CubeType slopeType1,
//            int xDelta2, int yDelta2, int zDelta2, CubeType slopeType2)
//        {
//            if (IsValidRange(x, y, z, xCount, yCount, zCount, xDelta1, yDelta1, zDelta1) && IsValidRange(x, y, z, xCount, yCount, zCount, xDelta2, yDelta2, zDelta2))
//            {
//                return ccubic[x + xDelta1, y + yDelta1, z + zDelta1] == slopeType1 && ccubic[x + xDelta2, y + yDelta2, z + zDelta2] == slopeType2;
//            }

//            return false;
//        }

//        #endregion

//        #region ConvertImageToStructure

//        /// <summary>
//        /// convert image to Bitmap, then index the colors, and make structure...
//        /// </summary>
//        /// <param name="imageFilename"></param>
//        /// <returns></returns>
//        private static List<Cube> ConvertImageToStructure(Structure structure, string imageFilename)
//        {
//            imageFilename = Path.GetFullPath(imageFilename);
//            Bitmap bmp = new Bitmap(imageFilename);
//            SubtypeId[,] armourArray = null;


//            List<Cube> cubes = new List<Cube>();
//            double scaleFactor = 2.5;
//            SubtypeId blockType = SubtypeId.LargeBlockArmorBlock;
//            Cube newCube;
//            int xCount;
//            int yCount;

//            if (structure.ClassType == ClassType.SmallShip)
//            {
//                scaleFactor = 0.5;
//                blockType = SubtypeId.SmallBlockArmorBlock;
//            }

//            using (Bitmap image = new Bitmap(bmp))
//            {
//                var palette = GetOptimizerPalatte();

//                //OctreeQuantizer octreeQuantizer = new OctreeQuantizer(255, 8);

//                //using (Bitmap octreeImage = octreeQuantizer.Quantize(image))
//                //{
//                ArrayList myPalette = new ArrayList(palette.Keys.ToArray());
//                PaletteQuantizer paletteQuantizer = new PaletteQuantizer(myPalette);

//                using (Bitmap palatteImage = paletteQuantizer.Quantize(image))
//                {
//                    xCount = palatteImage.Width;
//                    yCount = palatteImage.Height;

//                    armourArray = new SubtypeId[palatteImage.Width, palatteImage.Height];

//                    for (int x = 0; x < palatteImage.Width; x++)
//                    {
//                        for (int y = 0; y < palatteImage.Height; y++)
//                        {
//                            int z = 0;
//                            var color = palatteImage.GetPixel(x, y);
//                            SubtypeId armor = (SubtypeId)Enum.Parse(typeof(SubtypeId), blockType.ToString() + palette[color]);

//                            cubes.Add(newCube = new Cube());
//                            newCube.SubtypeName = armor;
//                            newCube.EntityId = GenerateEntityId().ToString();
//                            newCube.PersistentFlags = "Enabled CastShadows";
//                            //newCube.PositionAndOrientation = new PositionAndOrientation();
//                            //newCube.PositionAndOrientation.Position = new Point3D(x * scaleFactor, y * scaleFactor, z * scaleFactor);
//                            SetCubeOrientation(newCube, CubeType.Cube);
//                            newCube.Min = new Point3D(x, y, z);
//                            newCube.Max = new Point3D(x, y, z);
//                        }
//                    }

//                    //var outputFileTest = Path.Combine(Path.GetDirectoryName(imageFilename), Path.GetFileNameWithoutExtension(imageFilename) + "_up" + ".png");
//                    //palatteImage.Save(outputFileTest, ImageFormat.Png);
//                }
//                //}
//            }

//            bmp.Dispose();

//            return cubes;
//        } 

//        #endregion

//        #region ConvertPolyToVox

//        private static string ConvertPolyToVox(string polyFilename, int fixScale)
//        {
//            string voxFilename = null;

//            string tempfilename = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".vox");

//            Process p = new Process();
//            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
//            p.StartInfo.FileName = Path.Combine(directory, "poly2vox.exe");
//            p.StartInfo.WorkingDirectory = directory;

//            if (fixScale <= 0)
//            {
//                p.StartInfo.Arguments = string.Format("\"{0}\" \"{1}\"", polyFilename, tempfilename);
//            }
//            else
//            {
//                p.StartInfo.Arguments = string.Format("\"{0}\" \"{1}\" /v{2}", polyFilename, tempfilename, fixScale);
//            }

//            p.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
//            var ret = p.Start();
//            p.WaitForExit();

//            if (ret && File.Exists(tempfilename))
//            {
//                voxFilename = tempfilename;
//            }

//            return voxFilename;
//        }

//        #endregion

//        #region GenerateEntityId

//        private static long GenerateEntityId()
//        {
//            var buffer = Guid.NewGuid().ToByteArray();
//            return BitConverter.ToInt64(buffer, 0);
//        }

//        #endregion

//        #region WriteOutCubeBlocks

//        private static void WriteOutCubeBlocks(SaveResource save, Structure structure)
//        {
//            XmlNamespaceManager nsManager = Support.BuildXmlNamespaceManager(save.Resource.SandboxDoc);
//            var nav = save.Resource.SandboxDoc.CreateNavigator();
//            var nodecollection = nav.SelectSingleNode("MyObjectBuilder_Sector/SectorObjects");
//            var blocksNode = structure.Node.SelectSingleNode("CubeBlocks");

//            if (blocksNode.HasChildren)
//            {
//                blocksNode.InnerXml = string.Empty;

//                //while (blocksNode.HasChildren)
//                //{
//                //    var firstChild = blocksNode.Clone();
//                //    firstChild.MoveToFirstChild();
//                //    firstChild.DeleteSelf();
//                //}
//            }

//            foreach (var cube in structure.Cubes)
//            {
//                var writer = blocksNode.AppendChild();

//                writer.WriteStartElement("MyObjectBuilder_CubeBlock");
//                writer.WriteElementString("SubtypeName", cube.SubtypeName.ToString());
//                writer.WriteElementString("EntityId", cube.EntityId);
//                writer.WriteElementString("PersistentFlags", cube.PersistentFlags);
//                //writer.WriteStartElement("PositionAndOrientation");

//                //writer.WriteStartElement("Position");
//                //writer.WriteElementString("X", cube.PositionAndOrientation.Position.X.ToString());
//                //writer.WriteElementString("Y", cube.PositionAndOrientation.Position.Y.ToString());
//                //writer.WriteElementString("Z", cube.PositionAndOrientation.Position.Z.ToString());
//                //writer.WriteEndElement();

//                //writer.WriteStartElement("Forward");
//                //writer.WriteElementString("X", cube.PositionAndOrientation.Forward.X.ToString());
//                //writer.WriteElementString("Y", cube.PositionAndOrientation.Forward.Y.ToString());
//                //writer.WriteElementString("Z", cube.PositionAndOrientation.Forward.Z.ToString());
//                //writer.WriteEndElement();

//                //writer.WriteStartElement("Up");
//                //writer.WriteElementString("X", cube.PositionAndOrientation.Up.X.ToString());
//                //writer.WriteElementString("Y", cube.PositionAndOrientation.Up.Y.ToString());
//                //writer.WriteElementString("Z", cube.PositionAndOrientation.Up.Z.ToString());
//                //writer.WriteEndElement();

//                //writer.WriteEndElement();

//                writer.WriteStartElement("Min");
//                writer.WriteElementString("X", cube.Min.X.ToString());
//                writer.WriteElementString("Y", cube.Min.Y.ToString());
//                writer.WriteElementString("Z", cube.Min.Z.ToString());
//                writer.WriteEndElement();

//                writer.WriteStartElement("Max");
//                writer.WriteElementString("X", cube.Max.X.ToString());
//                writer.WriteElementString("Y", cube.Max.Y.ToString());
//                writer.WriteElementString("Z", cube.Max.Z.ToString());
//                writer.WriteEndElement();

//                writer.WriteStartElement("Orientation");
//                writer.WriteElementString("X", cube.Orientation.X.ToString());
//                writer.WriteElementString("Y", cube.Orientation.Y.ToString());
//                writer.WriteElementString("Z", cube.Orientation.Z.ToString());
//                writer.WriteElementString("W", cube.Orientation.W.ToString());
//                writer.WriteEndElement();

//                writer.WriteEndElement();
//                writer.Close();
//            }

//            save.Resource.SandboxDoc.Save(save.Resource.SandboxFile);


//            //var settings = new XmlReaderSettings()
//            //{
//            //    IgnoreComments = true,
//            //    IgnoreWhitespace = true
//            //};


//            //using (XmlReader myReader = XmlReader.Create(save.Resource.SandboxFile, settings))
//            //{
//            //    while (myReader.Read())
//            //    {
//            //        // Process each node (myReader.Value) here
//            //        // ...

//            //        if (myReader.Name == "CubeBlocks")
//            //        {
//            //        }
//            //    }
//            //}


//            //XDocument xmlFile = XDocument.Load("books.xml");
//            //var query = from c in xmlFile.Elements("catalog").Elements("book")
//            //            select c;
//            //foreach (XElement book in query)
//            //{
//            //    book.Attribute("attr1").Value = "MyNewValue";
//            //}
//            //xmlFile.Save("books.xml");


//            //foreach (var cube in structure.Cubes)
//            //{
//            //    var writer = blocksNode.AppendChild();

//            //    writer.WriteStartElement("MyObjectBuilder_CubeBlock");
//            //    writer.WriteElementString("SubtypeName", cube.SubtypeName.ToString());
//            //    writer.WriteElementString("EntityId", cube.EntityId);
//            //    writer.WriteElementString("PersistentFlags", cube.PersistentFlags);
//            //    writer.WriteStartElement("PositionAndOrientation");

//            //    writer.WriteStartElement("Position");
//            //    writer.WriteElementString("X", cube.PositionAndOrientation.Position.X.ToString());
//            //    writer.WriteElementString("Y", cube.PositionAndOrientation.Position.Y.ToString());
//            //    writer.WriteElementString("Z", cube.PositionAndOrientation.Position.Z.ToString());
//            //    writer.WriteEndElement();

//            //    writer.WriteStartElement("Forward");
//            //    writer.WriteElementString("X", cube.PositionAndOrientation.Forward.X.ToString());
//            //    writer.WriteElementString("Y", cube.PositionAndOrientation.Forward.Y.ToString());
//            //    writer.WriteElementString("Z", cube.PositionAndOrientation.Forward.Z.ToString());
//            //    writer.WriteEndElement();

//            //    writer.WriteStartElement("Up");
//            //    writer.WriteElementString("X", cube.PositionAndOrientation.Up.X.ToString());
//            //    writer.WriteElementString("Y", cube.PositionAndOrientation.Up.Y.ToString());
//            //    writer.WriteElementString("Z", cube.PositionAndOrientation.Up.Z.ToString());
//            //    writer.WriteEndElement();

//            //    writer.WriteEndElement();

//            //    writer.WriteStartElement("Min");
//            //    writer.WriteElementString("X", cube.Min.X.ToString());
//            //    writer.WriteElementString("Y", cube.Min.Y.ToString());
//            //    writer.WriteElementString("Z", cube.Min.Z.ToString());
//            //    writer.WriteEndElement();

//            //    writer.WriteStartElement("Max");
//            //    writer.WriteElementString("X", cube.Max.X.ToString());
//            //    writer.WriteElementString("Y", cube.Max.Y.ToString());
//            //    writer.WriteElementString("Z", cube.Max.Z.ToString());
//            //    writer.WriteEndElement();

//            //    writer.WriteStartElement("Orientation");
//            //    writer.WriteElementString("X", cube.Orientation.X.ToString());
//            //    writer.WriteElementString("Y", cube.Orientation.Y.ToString());
//            //    writer.WriteElementString("Z", cube.Orientation.Z.ToString());
//            //    writer.WriteElementString("W", cube.Orientation.W.ToString());
//            //    writer.WriteEndElement();

//            //    writer.WriteEndElement();
//            //    writer.Close();
//            //}
            
//        }

//        #endregion

//        #region SetCubeOrientation

//        private static Cube SetCubeOrientation(Cube cube, CubeType type)
//        {
//            var d45 = Math.Sqrt(1 / 2);

//            switch (type)
//            {
//                case CubeType.Cube:
//                    //cube.PositionAndOrientation.Forward = new Point3D(0, 0, -1);
//                    //cube.PositionAndOrientation.Up = new Point3D(0, 1, 0);
//                    cube.Orientation = new Point4D(0, 0, 0, 1);
//                    break;

//                case CubeType.SlopeCenterBackTop:
//                    //cube.PositionAndOrientation.Forward = new Point3D(0, -1, 0);
//                    //cube.PositionAndOrientation.Up = new Point3D(0, 0, -1);
//                    cube.Orientation = new Point4D(-0.707106769, 0, 0, 0.707106769);
//                    break;

//                case CubeType.SlopeRightBackCenter:
//                    //cube.PositionAndOrientation.Forward = new Point3D(0, -1, 0);
//                    //cube.PositionAndOrientation.Up = new Point3D(-1, 0, 0);
//                    cube.Orientation = new Point4D(0.5, -0.5, -0.5, -0.5);
//                    break;

//                case CubeType.SlopeLeftBackCenter:
//                    //cube.PositionAndOrientation.Forward = new Point3D(0, -1, 0);
//                    //cube.PositionAndOrientation.Up = new Point3D(1, 0, 0);
//                    cube.Orientation = new Point4D(0.5, 0.5, 0.5, -0.5);
//                    break;

//                case CubeType.SlopeCenterBackBottom:
//                    //cube.PositionAndOrientation.Forward = new Point3D(0, 0, -1);
//                    //cube.PositionAndOrientation.Up = new Point3D(0, 1, 0);
//                    cube.Orientation = new Point4D(0, 0, 0, 1);
//                    break;

//                case CubeType.SlopeRightCenterTop:
//                    //cube.PositionAndOrientation.Forward = new Point3D(0, 0, 1);
//                    //cube.PositionAndOrientation.Up = new Point3D(-1, 0, 0);
//                    cube.Orientation = new Point4D(0.707106769, -0.707106769, 0, 0);
//                    break;

//                case CubeType.SlopeLeftCenterTop:
//                    //cube.PositionAndOrientation.Forward = new Point3D(0, 0, 1);
//                    //cube.PositionAndOrientation.Up = new Point3D(1, 0, 0);
//                    cube.Orientation = new Point4D(0.707106769, 0.707106769, 0, 0);
//                    break;

//                case CubeType.SlopeRightCenterBottom:
//                    //cube.PositionAndOrientation.Forward = new Point3D(0, 0, -1);
//                    //cube.PositionAndOrientation.Up = new Point3D(-1, 0, 0);
//                    cube.Orientation = new Point4D(0, 0, 0.707106769, 0.707106769);
//                    break;

//                case CubeType.SlopeLeftCenterBottom:
//                    //cube.PositionAndOrientation.Forward = new Point3D(0, 0, -1);
//                    //cube.PositionAndOrientation.Up = new Point3D(1, 0, 0);
//                    cube.Orientation = new Point4D(0, 0, -0.707106769, 0.707106769);
//                    break;

//                case CubeType.SlopeCenterFrontTop:
//                    //cube.PositionAndOrientation.Forward = new Point3D(0, 0, 1);
//                    //cube.PositionAndOrientation.Up = new Point3D(0, -1, 0);
//                    cube.Orientation = new Point4D(1, 0, 0, 0);
//                    break;

//                case CubeType.SlopeRightFrontCenter:
//                    //cube.PositionAndOrientation.Forward = new Point3D(0, 1, 0);
//                    //cube.PositionAndOrientation.Up = new Point3D(-1, 0, 0);
//                    cube.Orientation = new Point4D(0.5, -0.5, 0.5, 0.5);
//                    break;

//                case CubeType.SlopeLeftFrontCenter:
//                    //cube.PositionAndOrientation.Forward = new Point3D(0, 1, 0);
//                    //cube.PositionAndOrientation.Up = new Point3D(1, 0, 0);
//                    cube.Orientation = new Point4D(0.5, 0.5, -0.5, 0.5);
//                    break;

//                case CubeType.SlopeCenterFrontBottom:
//                    //cube.PositionAndOrientation.Forward = new Point3D(0, 1, 0);
//                    //cube.PositionAndOrientation.Up = new Point3D(0, 0, 1);
//                    cube.Orientation = new Point4D(0.707106769, 0, 0, 0.707106769);
//                    break;

//                // -----------------------------

//                case CubeType.CornerLeftFrontTop:
//                    //cube.PositionAndOrientation.Forward = new Point3D(0, -1, 0);
//                    //cube.PositionAndOrientation.Up = new Point3D(1, 0, 0);
//                    cube.Orientation = new Point4D(0.5, 0.5, 0.5, -0.5);
//                    break;

//                case CubeType.CornerRightFrontTop:
//                    //cube.PositionAndOrientation.Forward = new Point3D(0, 0, 1);
//                    //cube.PositionAndOrientation.Up = new Point3D(0, -1, 0);
//                    cube.Orientation = new Point4D(1, 0, 0, 0);
//                    break;

//                case CubeType.CornerLeftBackTop:
//                    //cube.PositionAndOrientation.Forward = new Point3D(0, 0, 1);
//                    //cube.PositionAndOrientation.Up = new Point3D(1, 0, 0);
//                    cube.Orientation = new Point4D(0.707106769, 0.707106769, 0, 0);
//                    break;

//                case CubeType.CornerRightBackTop:
//                    //cube.PositionAndOrientation.Forward = new Point3D(0, -1, 0);
//                    //cube.PositionAndOrientation.Up = new Point3D(0, 0, -1);
//                    cube.Orientation = new Point4D(-0.707106769, 0, 0, 0.707106769);
//                    break;

//                case CubeType.CornerLeftFrontBottom:
//                    //cube.PositionAndOrientation.Forward = new Point3D(0, 1, 0);
//                    //cube.PositionAndOrientation.Up = new Point3D(1, 0, 0);
//                    cube.Orientation = new Point4D(0.5, 0.5, -0.5, 0.5);
//                    break;

//                case CubeType.CornerRightFrontBottom:
//                    //cube.PositionAndOrientation.Forward = new Point3D(0, 1, 0);
//                    //cube.PositionAndOrientation.Up = new Point3D(0, 0, 1);
//                    cube.Orientation = new Point4D(0.707106769, 0, 0, 0.707106769);
//                    break;

//                case CubeType.CornerLeftBackBottom:
//                    //cube.PositionAndOrientation.Forward = new Point3D(0, 0, -1);
//                    //cube.PositionAndOrientation.Up = new Point3D(1, 0, 0);
//                    cube.Orientation = new Point4D(0, 0, -0.707106769, 0.707106769);
//                    break;

//                case CubeType.CornerRightBackBottom:
//                    //cube.PositionAndOrientation.Forward = new Point3D(0, 0, -1);
//                    //cube.PositionAndOrientation.Up = new Point3D(0, 1, 0);
//                    cube.Orientation = new Point4D(0, 0, 0, 1);
//                    break;

//                default:
//                    throw new NotImplementedException(string.Format("SetCubeOrientation of type [{0}] not yet implmented.", type));
//            }

//            return cube;
//        }

//        #endregion

//        #region GetOptimizerPalatte

//        internal static Dictionary<Color, string> GetOptimizerPalatte()
//        {
//            Dictionary<Color, string> palette = new Dictionary<Color, string>()
//                {
//                    {Color.FromArgb(255, 0, 0, 0), "Black"},
//                    {Color.FromArgb(255, 20, 20, 20), "Black"},
//                    {Color.FromArgb(255, 40, 40, 40), "Black"},

//                    {Color.FromArgb(255, 128, 128, 128), ""}, // grey
//                    {Color.FromArgb(255, 92, 92, 92), ""}, // grey

//                    {Color.FromArgb(255, 255, 255, 255), "White"},
//                    {Color.FromArgb(255, 192, 192, 192), "White"},

//                    {Color.FromArgb(255, 255, 0, 0), "Red"},
//                    {Color.FromArgb(255, 192, 110, 110), "Red"},
//                    {Color.FromArgb(255, 160, 110, 110), "Red"},
//                    {Color.FromArgb(255, 120, 80, 80), "Red"},
//                    {Color.FromArgb(255, 148, 40, 40), "Red"},
//                    {Color.FromArgb(255, 148, 0, 0), "Red"},
//                    {Color.FromArgb(255, 128, 0, 0), "Red"},
//                    {Color.FromArgb(255, 92, 20, 20), "Red"},
//                    {Color.FromArgb(255, 64, 0, 0), "Red"},
//                    {Color.FromArgb(255, 64, 32, 32), "Red"},

//                    {Color.FromArgb(255, 0, 255, 0), "Green"},
//                    {Color.FromArgb(255, 110, 192, 110), "Green"},
//                    {Color.FromArgb(255, 110, 160, 110), "Green"},
//                    {Color.FromArgb(255, 80, 120, 80), "Green"},
//                    {Color.FromArgb(255, 40, 148, 40), "Green"},
//                    {Color.FromArgb(255, 0, 148, 0), "Green"},
//                    {Color.FromArgb(255, 0, 128, 0), "Green"},
//                    {Color.FromArgb(255, 0, 64, 0), "Green"},
//                    {Color.FromArgb(255, 32, 64, 32), "Green"},

//                    {Color.FromArgb(255, 0, 0, 255), "Blue"},
//                    {Color.FromArgb(255, 110, 110, 192), "Blue"},
//                    {Color.FromArgb(255, 110, 110, 160), "Blue"},
//                    {Color.FromArgb(255, 80, 90, 120), "Blue"},
//                    {Color.FromArgb(255, 40, 40, 148), "Blue"},
//                    {Color.FromArgb(255, 0, 0, 148), "Blue"},
//                    {Color.FromArgb(255, 0, 0, 128), "Blue"},
//                    {Color.FromArgb(255, 0, 0, 64), "Blue"},
//                    {Color.FromArgb(255, 32, 32, 64), "Blue"},

//                    {Color.FromArgb(255, 128, 128, 0), "Yellow"},
//                    {Color.FromArgb(255, 215, 128, 0), "Yellow"},
//                    {Color.FromArgb(255, 192, 128, 0), "Yellow"},
//                    {Color.FromArgb(255, 215, 64, 0), "Yellow"},
//                    {Color.FromArgb(255, 192, 64, 0), "Yellow"},
//                    {Color.FromArgb(255, 215, 192, 128), "Yellow"},
//                    {Color.FromArgb(255, 215, 192, 96), "Yellow"},
//                    {Color.FromArgb(255, 215, 192, 64), "Yellow"},
//                    {Color.FromArgb(255, 192, 172, 96), "Yellow"},
//                    {Color.FromArgb(255, 192, 160, 96), "Yellow"},
//                    {Color.FromArgb(255, 192, 160, 64), "Yellow"},
//                    {Color.FromArgb(255, 192, 160, 32), "Yellow"},
//                    {Color.FromArgb(255, 128, 96, 48), "Yellow"},
//                    {Color.FromArgb(255, 128, 96, 32), "Yellow"},
//                    {Color.FromArgb(255, 128, 96, 16), "Yellow"},
//                    {Color.FromArgb(255, 92, 64, 16), "Yellow"},

//                };

//            return palette;
//        }

//        #endregion

//        #region OptimizeImagePalette

//        public static Bitmap OptimizeImagePalette(string imageFilename)
//        {
//            imageFilename = Path.GetFullPath(imageFilename);
//            Bitmap bmp = new Bitmap(imageFilename);
//            Bitmap palatteImage;

//            using (Bitmap image = new Bitmap(bmp))
//            {
//                var palette = GetOptimizerPalatte();

//                //OctreeQuantizer octreeQuantizer = new OctreeQuantizer(255, 8);

//                //using (Bitmap octreeImage = octreeQuantizer.Quantize(image))
//                //{
//                ArrayList myPalette = new ArrayList(palette.Keys.ToArray());
//                PaletteQuantizer paletteQuantizer = new PaletteQuantizer(myPalette);

//                palatteImage = paletteQuantizer.Quantize(image);
//            }

//            bmp.Dispose();

//            return palatteImage;
//        }

//        #endregion

//    }
//}