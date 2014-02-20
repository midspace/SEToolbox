namespace SEToolbox.Models
{
    using Sandbox.CommonLib.ObjectBuilders;
    using Sandbox.CommonLib.ObjectBuilders.Definitions;
    using Sandbox.CommonLib.ObjectBuilders.VRageData;
    using SEToolbox.Interop;
    using SEToolbox.Support;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Windows.Media.Media3D;
    using System.Xml.Serialization;
    using VRageMath;

    [Serializable]
    public class StructureCubeGridModel : StructureBaseModel
    {
        #region Fields

        private Point3D min;
        private Point3D max;
        private Vector3D size;
        private int pilots;
        private string report;
        private float mass;

        #endregion

        #region ctor

        public StructureCubeGridModel(MyObjectBuilder_EntityBase entityBase)
            : base(entityBase)
        {
        }

        #endregion

        #region Properties

        [XmlIgnore]
        public MyObjectBuilder_CubeGrid CubeGrid
        {
            get
            {
                return this.EntityBase as MyObjectBuilder_CubeGrid;
            }
        }

        [XmlIgnore]
        public Sandbox.CommonLib.ObjectBuilders.MyCubeSize GridSize
        {
            get
            {
                return this.CubeGrid.GridSizeEnum;
            }

            set
            {
                if (value != this.CubeGrid.GridSizeEnum)
                {
                    this.CubeGrid.GridSizeEnum = value;
                    this.RaisePropertyChanged(() => GridSize);
                }
            }
        }

        [XmlIgnore]
        public bool IsStatic
        {
            get
            {
                return this.CubeGrid.IsStatic;
            }

            set
            {
                if (value != this.CubeGrid.IsStatic)
                {
                    this.CubeGrid.IsStatic = value;
                    this.RaisePropertyChanged(() => IsStatic);
                }
            }
        }

        [XmlIgnore]
        public bool Dampeners
        {
            get
            {
                return this.CubeGrid.DampenersEnabled;
            }

            set
            {
                if (value != this.CubeGrid.DampenersEnabled)
                {
                    this.CubeGrid.DampenersEnabled = value;
                    this.RaisePropertyChanged(() => Dampeners);
                }
            }
        }

        [XmlIgnore]
        public Point3D Min
        {
            get
            {
                return this.min;
            }

            set
            {
                if (value != this.min)
                {
                    this.min = value;
                    this.RaisePropertyChanged(() => Min);
                }
            }
        }

        [XmlIgnore]
        public Point3D Max
        {
            get
            {
                return this.max;
            }

            set
            {
                if (value != this.max)
                {
                    this.max = value;
                    this.RaisePropertyChanged(() => Max);
                }
            }
        }

        [XmlIgnore]
        public Vector3D Size
        {
            get
            {
                return this.size;
            }

            set
            {
                if (value != this.size)
                {
                    this.size = value;
                    this.RaisePropertyChanged(() => Size);
                }
            }
        }

        [XmlIgnore]
        public int Pilots
        {
            get
            {
                return this.pilots;
            }

            set
            {
                if (value != this.pilots)
                {
                    this.pilots = value;
                    this.RaisePropertyChanged(() => Pilots);
                }
            }
        }

        [XmlIgnore]
        public bool IsPiloted
        {
            get
            {
                return this.Pilots > 0;
            }
        }

        [XmlIgnore]
        public bool IsDamaged
        {
            get
            {
                return this.CubeGrid.Skeleton.Count > 0;
            }
        }

        [XmlIgnore]
        public int DamageCount
        {
            get
            {
                return this.CubeGrid.Skeleton.Count;
            }
        }

        [XmlIgnore]
        public double Speed
        {
            get
            {
                return this.CubeGrid.LinearVelocity.ToVector3().LinearVector();
            }
        }

        [XmlIgnore]
        public string Report
        {
            get
            {
                return this.report;
            }

            set
            {
                if (value != this.report)
                {
                    this.report = value;
                    this.RaisePropertyChanged(() => Report);
                }
            }
        }

        [XmlIgnore]
        public float Mass
        {
            get
            {
                return this.mass;
            }

            set
            {
                if (value != this.mass)
                {
                    this.mass = value;
                    this.RaisePropertyChanged(() => Mass);
                }
            }
        }

        #endregion

        #region methods

        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context)
        {
            this.SerializedEntity = SpaceEngineersAPI.Serialize<MyObjectBuilder_CubeGrid>(this.CubeGrid);
        }
        
        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            this.EntityBase = SpaceEngineersAPI.Deserialize<MyObjectBuilder_CubeGrid>(this.SerializedEntity);
        }

        public override void UpdateFromEntityBase()
        {
            if (this.IsStatic && this.CubeGrid.GridSizeEnum == MyCubeSize.Large)
            {
                this.ClassType = ClassType.Station;
            }
            else if (!this.IsStatic && this.CubeGrid.GridSizeEnum == MyCubeSize.Large)
            {
                this.ClassType = ClassType.LargeShip;
            }
            else if (!this.IsStatic && this.CubeGrid.GridSizeEnum == MyCubeSize.Small)
            {
                this.ClassType = ClassType.SmallShip;
            }

            var min = new Point3D(int.MaxValue, int.MaxValue, int.MaxValue);
            var max = new Point3D(int.MinValue, int.MinValue, int.MinValue);
            float calcMass = 0;
            var ingotRequirements = new Dictionary<string, MyObjectBuilder_BlueprintDefinition.Item>();
            var oreRequirements = new Dictionary<string, MyObjectBuilder_BlueprintDefinition.Item>();
            //MyObjectBuilder_BlueprintDefinition requirements2 = new MyObjectBuilder_BlueprintDefinition();
            var timeTaken = new TimeSpan();

            foreach (var block in this.CubeGrid.CubeBlocks)
            {
                min.X = Math.Min(min.X, block.Min.X);
                min.Y = Math.Min(min.Y, block.Min.Y);
                min.Z = Math.Min(min.Z, block.Min.Z);
#warning resolve cubetype size.
                max.X = Math.Max(max.X, block.Min.X);       // TODO: resolve cubetype size.
                max.Y = Math.Max(max.Y, block.Min.Y);
                max.Z = Math.Max(max.Z, block.Min.Z);

                calcMass += SpaceEngineersAPI.FetchCubeBlockMass(block.SubtypeName, this.CubeGrid.GridSizeEnum);

                SpaceEngineersAPI.AccumulateCubeBlueprintRequirements(block.SubtypeName, this.CubeGrid.GridSizeEnum, 1, ingotRequirements, ref timeTaken);
            }

            foreach (var kvp in ingotRequirements)
            {
                SpaceEngineersAPI.AccumulateCubeBlueprintRequirements(kvp.Value.SubtypeId, kvp.Value.TypeId, kvp.Value.Amount, oreRequirements, ref timeTaken);
            }

            var size = max - min;
            size.X++;
            size.Y++;
            size.Z++;

            this.Min = min;
            this.Max = max;
            this.Size = size;
            this.Mass = calcMass;

            this.Name = null;
            // Substitue Beacon detail for the name.
            var beacons = this.CubeGrid.CubeBlocks.Where(b => b.SubtypeName == SubtypeId.LargeBlockBeacon.ToString() || b.SubtypeName == SubtypeId.SmallBlockBeacon.ToString()).ToArray();
            if (beacons.Length > 0)
            {
                var a = beacons.Select(b => ((MyObjectBuilder_Beacon)b).CustomName).ToArray();
                this.Name = String.Join("|", a);
            }
            
            this.Description = string.Format("{0} | {1:#,##0}Kg", this.Size, this.Mass);

            var bld = new StringBuilder();
            bld.AppendLine("Construction Requirements:");
            foreach (var kvp in oreRequirements)
            {
                var mass = SpaceEngineersAPI.GetItemMass(kvp.Value.TypeId, kvp.Value.SubtypeId);
                bld.AppendFormat("{0} {1}: {2:###,##0.000} L or {3:###,##0.000} Kg\r\n", kvp.Value.SubtypeId, kvp.Value.TypeId, kvp.Value.Amount * 1000, kvp.Value.Amount * (decimal)mass);
            }
            bld.AppendLine();
            bld.AppendFormat("Time to produce: {0:hh\\:mm\\:ss}\r\n", timeTaken);

            this.Report = bld.ToString();

            // Report:
            // Reflectors On
            // Mass:      9,999,999 Kg
            // Speed:          0.0 m/s
            // Power Usage:      0.05%
            // Reactors:     12,999 GW
            // Thrusts:            999
            // Gyros:              999
            // Fuel Time:        0 sec
        }

        /// <summary>
        /// Find any Cockpits that have player character/s in them.
        /// </summary>
        /// <returns></returns>
        public List<MyObjectBuilder_Cockpit> GetActiveCockpits()
        {
            var cubes = this.CubeGrid.CubeBlocks.Where<MyObjectBuilder_CubeBlock>(e => e is MyObjectBuilder_Cockpit && ((MyObjectBuilder_Cockpit)e).Pilot != null);
            return new List<MyObjectBuilder_Cockpit>(cubes.Cast<MyObjectBuilder_Cockpit>());
        }

        public void RepairAllDamage()
        {
            if (this.CubeGrid.Skeleton == null)
                this.CubeGrid.Skeleton = new System.Collections.Generic.List<BoneInfo>();
            else
                this.CubeGrid.Skeleton.Clear();
            this.RaisePropertyChanged(() => IsDamaged);
            this.RaisePropertyChanged(() => DamageCount);
        }

        public void ResetVelocity()
        {
            this.CubeGrid.LinearVelocity = new VRageMath.Vector3(0, 0, 0);
            this.CubeGrid.AngularVelocity = new VRageMath.Vector3(0, 0, 0);
            this.RaisePropertyChanged(() => Speed);
        }

        public void ReverseVelocity()
        {
            this.CubeGrid.LinearVelocity = new VRageMath.Vector3(this.CubeGrid.LinearVelocity.X * -1, this.CubeGrid.LinearVelocity.Y * -1, this.CubeGrid.LinearVelocity.Z * -1);
            this.CubeGrid.AngularVelocity = new VRageMath.Vector3(this.CubeGrid.AngularVelocity.X * -1, this.CubeGrid.AngularVelocity.Y * -1, this.CubeGrid.AngularVelocity.Z * -1);
            this.RaisePropertyChanged(() => Speed);
        }

        public void MaxVelocityAtPlayer(Vector3 playerPosition)
        {
            var v = playerPosition - this.CubeGrid.PositionAndOrientation.Value.Position;
            v.Normalize();
            v = Vector3.Multiply(v, 104.375f);

            this.CubeGrid.LinearVelocity = v;
            this.CubeGrid.AngularVelocity = new VRageMath.Vector3(0, 0, 0);
            this.RaisePropertyChanged(() => Speed);
        }

        public bool ConvertFromLightToHeavyArmor()
        {
            var count = 0;
            foreach (var cube in this.CubeGrid.CubeBlocks)
            {
                if (cube.SubtypeName.StartsWith("LargeBlockArmor"))
                {
                    cube.SubtypeName = cube.SubtypeName.Replace("LargeBlockArmor", "LargeHeavyBlockArmor");
                    count++;
                }
                else if (cube.SubtypeName.StartsWith("SmallBlockArmor"))
                {
                    cube.SubtypeName = cube.SubtypeName.Replace("SmallBlockArmor", "SmallHeavyBlockArmor");
                    count++;
                }
            }

            this.UpdateFromEntityBase();
            return count > 0;
        }

        public bool ConvertFromHeavyToLightArmor()
        {
            var count = 0;
            foreach (var cube in this.CubeGrid.CubeBlocks)
            {
                if (cube.SubtypeName.StartsWith("LargeHeavyBlockArmor"))
                {
                    cube.SubtypeName = cube.SubtypeName.Replace("LargeHeavyBlockArmor", "LargeBlockArmor");
                    count++;
                }
                else if (cube.SubtypeName.StartsWith("SmallHeavyBlockArmor"))
                {
                    cube.SubtypeName = cube.SubtypeName.Replace("SmallHeavyBlockArmor", "SmallBlockArmor");
                    count++;
                }
            }

            this.UpdateFromEntityBase();
            return count > 0;
        }

        public void ConvertToFramework()
        {
            foreach (var cube in this.CubeGrid.CubeBlocks)
            {
                cube.BuildPercent = 0.0f;
            }

            this.UpdateFromEntityBase();
        }

        public void ConvertToCompleteStructure()
        {
            foreach (var cube in this.CubeGrid.CubeBlocks)
            {
                cube.BuildPercent = 1;
            }

            this.UpdateFromEntityBase();
        }

        public void ConvertToStation()
        {
            this.ResetVelocity();
            this.CubeGrid.IsStatic = true;
            this.UpdateFromEntityBase();
        }

        public void ConvertToShip()
        {
            this.CubeGrid.IsStatic = false;
            this.UpdateFromEntityBase();
        }

        public bool ConvertToCornerArmor()
        {
            var count = 0;
            count += this.CubeGrid.CubeBlocks.Where(c => c.SubtypeName == SubtypeId.LargeRoundArmor_Corner.ToString()).Select(c => { c.SubtypeName = SubtypeId.LargeBlockArmorCorner.ToString(); return c; }).ToList().Count;
            count += this.CubeGrid.CubeBlocks.Where(c => c.SubtypeName == SubtypeId.LargeRoundArmor_Slope.ToString()).Select(c => { c.SubtypeName = SubtypeId.LargeBlockArmorSlope.ToString(); return c; }).ToList().Count;
            count += this.CubeGrid.CubeBlocks.Where(c => c.SubtypeName == SubtypeId.LargeRoundArmor_CornerInv.ToString()).Select(c => { c.SubtypeName = SubtypeId.LargeBlockArmorCornerInv.ToString(); return c; }).ToList().Count;
            return count > 0;
        }

        public bool ConvertToRoundArmor()
        {
            var count = 0;
            count += this.CubeGrid.CubeBlocks.Where(c => c.SubtypeName == SubtypeId.LargeBlockArmorCorner.ToString()).Select(c => { c.SubtypeName = SubtypeId.LargeRoundArmor_Corner.ToString(); return c; }).ToList().Count;
            count += this.CubeGrid.CubeBlocks.Where(c => c.SubtypeName == SubtypeId.LargeBlockArmorSlope.ToString()).Select(c => { c.SubtypeName = SubtypeId.LargeRoundArmor_Slope.ToString(); return c; }).ToList().Count;
            count += this.CubeGrid.CubeBlocks.Where(c => c.SubtypeName == SubtypeId.LargeBlockArmorCornerInv.ToString()).Select(c => { c.SubtypeName = SubtypeId.LargeRoundArmor_CornerInv.ToString(); return c; }).ToList().Count;
            return count > 0;
        }

        public bool ConvertLadderToPassage()
        {
            var list = this.CubeGrid.CubeBlocks.Where(c => c is MyObjectBuilder_Ladder).ToArray();

            for (var i = 0; i < list.Length; i++)
            {
                var c = new MyObjectBuilder_Passage()
                {
                    EntityId = list[i].EntityId,
                    BlockOrientation = list[i].BlockOrientation,
                    BuildPercent = list[i].BuildPercent,
                    ColorMaskHSV = list[i].ColorMaskHSV,
                    IntegrityPercent = list[i].IntegrityPercent,
                    Min = list[i].Min,
                    SubtypeName = list[i].SubtypeName
                };
                this.CubeGrid.CubeBlocks.Remove(list[i]);
                this.CubeGrid.CubeBlocks.Add(c);
            }

            return list.Length > 0;
        }

        #region Mirror

        public void MirrorModel(bool usePlane, bool oddMirror)
        {
            var xMirror = Mirror.None;
            var yMirror = Mirror.None;
            var zMirror = Mirror.None;
            var xAxis = 0;
            var yAxis = 0;
            var zAxis = 0;

            if (!usePlane)
            // Find mirror Axis.
            //if (!this.CubeGrid.XMirroxPlane.HasValue && !this.CubeGrid.YMirroxPlane.HasValue && !this.CubeGrid.ZMirroxPlane.HasValue)
            {
                // Find the largest contigious exterior surface to use as the mirror.
                var minX = this.CubeGrid.CubeBlocks.Min(c => c.Min.X);
                var maxX = this.CubeGrid.CubeBlocks.Max(c => c.Min.X);
                var minY = this.CubeGrid.CubeBlocks.Min(c => c.Min.Y);
                var maxY = this.CubeGrid.CubeBlocks.Max(c => c.Min.Y);
                var minZ = this.CubeGrid.CubeBlocks.Min(c => c.Min.Z);
                var maxZ = this.CubeGrid.CubeBlocks.Max(c => c.Min.Z);

                var countMinX = this.CubeGrid.CubeBlocks.Count(c => c.Min.X == minX);
                var countMinY = this.CubeGrid.CubeBlocks.Count(c => c.Min.Y == minY);
                var countMinZ = this.CubeGrid.CubeBlocks.Count(c => c.Min.Z == minZ);
                var countMaxX = this.CubeGrid.CubeBlocks.Count(c => c.Min.X == maxX);
                var countMaxY = this.CubeGrid.CubeBlocks.Count(c => c.Min.Y == maxY);
                var countMaxZ = this.CubeGrid.CubeBlocks.Count(c => c.Min.Z == maxZ);

                if (countMinX > countMinY && countMinX > countMinZ && countMinX > countMaxX && countMinX > countMaxY && countMinX > countMaxZ)
                {
                    xMirror = oddMirror ? Mirror.Odd : Mirror.EvenDown;
                    xAxis = minX;
                }
                else if (countMinY > countMinX && countMinY > countMinZ && countMinY > countMaxX && countMinY > countMaxY && countMinY > countMaxZ)
                {
                    yMirror = oddMirror ? Mirror.Odd : Mirror.EvenDown;
                    yAxis = minY;
                }
                else if (countMinZ > countMinX && countMinZ > countMinY && countMinZ > countMaxX && countMinZ > countMaxY && countMinZ > countMaxZ)
                {
                    zMirror = oddMirror ? Mirror.Odd : Mirror.EvenDown;
                    zAxis = minZ;
                }
                else if (countMaxX > countMinX && countMaxX > countMinY && countMaxX > countMinZ && countMaxX > countMaxY && countMaxX > countMaxZ)
                {
                    xMirror = oddMirror ? Mirror.Odd : Mirror.EvenUp;
                    xAxis = maxX;
                }
                else if (countMaxY > countMinX && countMaxY > countMinY && countMaxY > countMinZ && countMaxY > countMaxX && countMaxY > countMaxZ)
                {
                    yMirror = oddMirror ? Mirror.Odd : Mirror.EvenUp;
                    yAxis = maxY;
                }
                else if (countMaxZ > countMinX && countMaxZ > countMinY && countMaxZ > countMinZ && countMaxZ > countMaxX && countMaxZ > countMaxY)
                {
                    zMirror = oddMirror ? Mirror.Odd : Mirror.EvenUp;
                    zAxis = maxZ;
                }

                this.CubeGrid.CubeBlocks.AddRange(MirrorCubes(this, false, xMirror, xAxis, yMirror, yAxis, zMirror, zAxis));
            }
            else
            {
                // Use the built in Mirror plane defined in game.
                if (this.CubeGrid.XMirroxPlane.HasValue)
                {
                    xMirror = this.CubeGrid.XMirroxOdd ? Mirror.EvenDown : Mirror.Odd; // Meaning is back to front? Or is it my reasoning?
                    xAxis = this.CubeGrid.XMirroxPlane.Value.X;
                    this.CubeGrid.CubeBlocks.AddRange(MirrorCubes(this, true, xMirror, xAxis, Mirror.None, 0, Mirror.None, 0));
                }
                if (this.CubeGrid.YMirroxPlane.HasValue)
                {
                    yMirror = this.CubeGrid.YMirroxOdd ? Mirror.EvenDown : Mirror.Odd;
                    yAxis = this.CubeGrid.YMirroxPlane.Value.Y;
                    this.CubeGrid.CubeBlocks.AddRange(MirrorCubes(this, true, Mirror.None, 0, yMirror, yAxis, Mirror.None, 0));
                }
                if (this.CubeGrid.ZMirroxPlane.HasValue)
                {
                    zMirror = this.CubeGrid.ZMirroxOdd ? Mirror.EvenUp : Mirror.Odd;
                    zAxis = this.CubeGrid.ZMirroxPlane.Value.Z;
                    this.CubeGrid.CubeBlocks.AddRange(MirrorCubes(this, true, Mirror.None, 0, Mirror.None, 0, zMirror, zAxis));
                }
            }

            this.UpdateFromEntityBase();
        }

        #region ValidMirrorBlocks

        private static readonly SubtypeId[] ValidMirrorBlocks = new SubtypeId[] {
            SubtypeId.LargeBlockArmorBlock,
            SubtypeId.LargeBlockArmorSlope,
            SubtypeId.LargeBlockArmorCorner,
            SubtypeId.LargeBlockArmorCornerInv,
            SubtypeId.LargeHeavyBlockArmorBlock,
            SubtypeId.LargeHeavyBlockArmorSlope,
            SubtypeId.LargeHeavyBlockArmorCorner,
            SubtypeId.LargeHeavyBlockArmorCornerInv,
            SubtypeId.LargeRoundArmor_Slope,
            SubtypeId.LargeRoundArmor_Corner,
            SubtypeId.LargeRoundArmor_CornerInv,
            SubtypeId.SmallBlockArmorBlock,
            SubtypeId.SmallBlockArmorSlope,
            SubtypeId.SmallBlockArmorCorner,
            SubtypeId.SmallBlockArmorCornerInv,
            SubtypeId.SmallHeavyBlockArmorBlock,
            SubtypeId.SmallHeavyBlockArmorSlope,
            SubtypeId.SmallHeavyBlockArmorCorner,
            SubtypeId.SmallHeavyBlockArmorCornerInv,
            //SubtypeId.LargeRamp,
        };

        #endregion

        private static IEnumerable<MyObjectBuilder_CubeBlock> MirrorCubes(StructureCubeGridModel viewModel, bool integrate, Mirror xMirror, int xAxis, Mirror yMirror, int yAxis, Mirror zMirror, int zAxis)
        {
            var blocks = new List<MyObjectBuilder_CubeBlock>();
            SubtypeId outVal;

            if (xMirror == Mirror.None && yMirror == Mirror.None && zMirror == Mirror.None)
                return blocks;

            foreach (var block in viewModel.CubeGrid.CubeBlocks.Where(b => Enum.TryParse<SubtypeId>(b.SubtypeName, out outVal) && ValidMirrorBlocks.Contains(outVal)))
            {
                var newBlock = new MyObjectBuilder_CubeBlock()
                {
                    SubtypeName = block.SubtypeName,
                    EntityId = block.EntityId == 0 ? 0 : SpaceEngineersAPI.GenerateEntityId(),
                    Min = block.Min.Mirror(xMirror, xAxis, yMirror, yAxis, zMirror, zAxis),
                    ColorMaskHSV = block.ColorMaskHSV,
                    BuildPercent = block.BuildPercent,
                    IntegrityPercent = block.IntegrityPercent,
                    //Orientation = VRageMath.Quaternion.CreateFromRotationMatrix(Matrix.CreateLookAt(Vector3.Zero, Vector3.Forward, Vector3.Up))
                    //Orientation = MirrorCubeOrientation(block.SubtypeName, block.Orientation, xMirror, yMirror, zMirror);
                    BlockOrientation = block.BlockOrientation
                };
                MirrorCubeOrientation(block.SubtypeName, block.BlockOrientation, xMirror, yMirror, zMirror, ref newBlock);

                // Don't place a block it one already exists there in the mirror.
                if (integrate && viewModel.CubeGrid.CubeBlocks.Any(b => b.Min.X == newBlock.Min.X && b.Min.Y == newBlock.Min.Y && b.Min.Z == newBlock.Min.Z /*|| b.Max == newBlock.Min*/))  // TODO: check cubeblock size.
                    continue;

                blocks.Add(newBlock);
            }
            return blocks;
        }

        // TODO: change to a return type later when finished testing.
        private static void MirrorCubeOrientation(string subtypeName, SerializableBlockOrientation orientation, Mirror xMirror, Mirror yMirror, Mirror zMirror, ref MyObjectBuilder_CubeBlock block)
        {
            if (xMirror != Mirror.None)
            {
                if (subtypeName.Contains("ArmorSlope") || subtypeName.Contains("Armor_Slope"))
                {
                    var cubeType = SpaceEngineersAPI.CubeOrientations.FirstOrDefault(x => x.Value.Forward == orientation.Forward && x.Value.Up == orientation.Up && x.Key.ToString().StartsWith("Slope"));
                    switch (cubeType.Key)
                    {
                        case CubeType.SlopeCenterBackTop: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeCenterBackTop]; break;
                        case CubeType.SlopeRightBackCenter: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeLeftBackCenter]; break;
                        case CubeType.SlopeLeftBackCenter: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeRightBackCenter]; break;
                        case CubeType.SlopeCenterBackBottom: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeCenterBackBottom]; break;
                        case CubeType.SlopeRightCenterTop: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeLeftCenterTop]; break;
                        case CubeType.SlopeLeftCenterTop: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeRightCenterTop]; break;
                        case CubeType.SlopeRightCenterBottom: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeLeftCenterBottom]; break;
                        case CubeType.SlopeLeftCenterBottom: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeRightCenterBottom]; break;
                        case CubeType.SlopeCenterFrontTop: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeCenterFrontTop]; break;
                        case CubeType.SlopeRightFrontCenter: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeLeftFrontCenter]; break;
                        case CubeType.SlopeLeftFrontCenter: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeRightFrontCenter]; break;
                        case CubeType.SlopeCenterFrontBottom: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeCenterFrontBottom]; break;
                    }
                }
                else if (subtypeName.Contains("ArmorCorner") || subtypeName.Contains("Armor_Corner"))
                {
                    var cubeType = SpaceEngineersAPI.CubeOrientations.FirstOrDefault(x => x.Value.Forward == orientation.Forward && x.Value.Up == orientation.Up && x.Key.ToString().StartsWith("NormalCorner"));
                    switch (cubeType.Key)
                    {
                        case CubeType.NormalCornerLeftFrontTop: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerRightFrontTop]; break;
                        case CubeType.NormalCornerRightFrontTop: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerLeftFrontTop]; break;
                        case CubeType.NormalCornerLeftBackTop: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerRightBackTop]; break;
                        case CubeType.NormalCornerRightBackTop: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerLeftBackTop]; break;
                        case CubeType.NormalCornerLeftFrontBottom: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerRightFrontBottom]; break;
                        case CubeType.NormalCornerRightFrontBottom: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerLeftFrontBottom]; break;
                        case CubeType.NormalCornerLeftBackBottom: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerRightBackBottom]; break;
                        case CubeType.NormalCornerRightBackBottom: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerLeftBackBottom]; break;
                    }
                }


                //else if (subtypeName.Contains("LargeRamp"))
                //{
                //    var cubeType = SpaceEngineersAPI.CubeOrientations.FirstOrDefault(x => x.Value.Forward == orientation.Forward && x.Value.Up == orientation.Up && x.Key.ToString().StartsWith("NormalCorner"));
                //    switch (cubeType.Key)
                //    {
                //        case CubeType.NormalCornerLeftFrontTop: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerRightFrontTop]; break;
                //        case CubeType.NormalCornerRightFrontTop: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerLeftFrontTop]; break;
                //        case CubeType.NormalCornerLeftBackTop: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerRightBackTop]; break;
                //        case CubeType.NormalCornerRightBackTop: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerLeftBackTop]; break;
                //        case CubeType.NormalCornerLeftFrontBottom: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerRightFrontBottom]; break;
                //        case CubeType.NormalCornerRightFrontBottom: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerLeftFrontBottom]; break;
                //        case CubeType.NormalCornerLeftBackBottom: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerRightBackBottom]; break;
                //        case CubeType.NormalCornerRightBackBottom: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerLeftBackBottom]; break;
                //    }
                //}
                // TODO: Other block types.
            }
            else if (yMirror != Mirror.None)
            {
                if (subtypeName.Contains("ArmorSlope") || subtypeName.Contains("Armor_Slope"))
                {
                    var cubeType = SpaceEngineersAPI.CubeOrientations.FirstOrDefault(x => x.Value.Forward == orientation.Forward && x.Value.Up == orientation.Up && x.Key.ToString().StartsWith("Slope"));
                    switch (cubeType.Key)
                    {
                        case CubeType.SlopeCenterBackTop: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeCenterFrontTop]; break;
                        case CubeType.SlopeRightBackCenter: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeRightFrontCenter]; break;
                        case CubeType.SlopeLeftBackCenter: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeLeftFrontCenter]; break;
                        case CubeType.SlopeCenterBackBottom: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeCenterFrontBottom]; break;
                        case CubeType.SlopeRightCenterTop: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeRightCenterTop]; break;
                        case CubeType.SlopeLeftCenterTop: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeLeftCenterTop]; break;
                        case CubeType.SlopeRightCenterBottom: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeRightCenterBottom]; break;
                        case CubeType.SlopeLeftCenterBottom: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeLeftCenterBottom]; break;
                        case CubeType.SlopeCenterFrontTop: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeCenterBackTop]; break;
                        case CubeType.SlopeRightFrontCenter: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeRightBackCenter]; break;
                        case CubeType.SlopeLeftFrontCenter: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeLeftBackCenter]; break;
                        case CubeType.SlopeCenterFrontBottom: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeCenterBackBottom]; break;
                    }
                }
                else if (subtypeName.Contains("ArmorCorner") || subtypeName.Contains("Armor_Corner"))
                {
                    var cubeType = SpaceEngineersAPI.CubeOrientations.FirstOrDefault(x => x.Value.Forward == orientation.Forward && x.Value.Up == orientation.Up && x.Key.ToString().StartsWith("NormalCorner"));
                    switch (cubeType.Key)
                    {
                        case CubeType.NormalCornerLeftFrontTop: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerLeftBackTop]; break;
                        case CubeType.NormalCornerRightFrontTop: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerRightBackTop]; break;
                        case CubeType.NormalCornerLeftBackTop: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerLeftFrontTop]; break;
                        case CubeType.NormalCornerRightBackTop: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerRightFrontTop]; break;
                        case CubeType.NormalCornerLeftFrontBottom: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerLeftBackBottom]; break;
                        case CubeType.NormalCornerRightFrontBottom: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerRightBackBottom]; break;
                        case CubeType.NormalCornerLeftBackBottom: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerLeftFrontBottom]; break;
                        case CubeType.NormalCornerRightBackBottom: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerRightFrontBottom]; break;
                    }
                }
                // TODO: Other block types.
            }
            else if (zMirror != Mirror.None)
            {
                if (subtypeName.Contains("ArmorSlope") || subtypeName.Contains("Armor_Slope"))
                {
                    var cubeType = SpaceEngineersAPI.CubeOrientations.FirstOrDefault(x => x.Value.Forward == orientation.Forward && x.Value.Up == orientation.Up && x.Key.ToString().StartsWith("Slope"));
                    switch (cubeType.Key)
                    {
                        case CubeType.SlopeCenterBackTop: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeCenterBackBottom]; break;
                        case CubeType.SlopeRightBackCenter: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeRightBackCenter]; break;
                        case CubeType.SlopeLeftBackCenter: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeLeftBackCenter]; break;
                        case CubeType.SlopeCenterBackBottom: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeCenterBackTop]; break;
                        case CubeType.SlopeRightCenterTop: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeRightCenterBottom]; break;
                        case CubeType.SlopeLeftCenterTop: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeLeftCenterBottom]; break;
                        case CubeType.SlopeRightCenterBottom: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeRightCenterTop]; break;
                        case CubeType.SlopeLeftCenterBottom: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeLeftCenterTop]; break;
                        case CubeType.SlopeCenterFrontTop: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeCenterFrontBottom]; break;
                        case CubeType.SlopeRightFrontCenter: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeRightFrontCenter]; break;
                        case CubeType.SlopeLeftFrontCenter: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeLeftFrontCenter]; break;
                        case CubeType.SlopeCenterFrontBottom: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeCenterFrontTop]; break;
                    }
                }
                else if (subtypeName.Contains("ArmorCorner") || subtypeName.Contains("Armor_Corner"))
                {
                    var cubeType = SpaceEngineersAPI.CubeOrientations.FirstOrDefault(x => x.Value.Forward == orientation.Forward && x.Value.Up == orientation.Up && x.Key.ToString().StartsWith("NormalCorner"));
                    switch (cubeType.Key)
                    {
                        case CubeType.NormalCornerLeftFrontTop: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerLeftFrontBottom]; break;
                        case CubeType.NormalCornerRightFrontTop: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerRightFrontBottom]; break;
                        case CubeType.NormalCornerLeftBackTop: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerLeftBackBottom]; break;
                        case CubeType.NormalCornerRightBackTop: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerRightBackBottom]; break;
                        case CubeType.NormalCornerLeftFrontBottom: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerLeftFrontTop]; break;
                        case CubeType.NormalCornerRightFrontBottom: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerRightFrontTop]; break;
                        case CubeType.NormalCornerLeftBackBottom: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerLeftBackTop]; break;
                        case CubeType.NormalCornerRightBackBottom: block.BlockOrientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerRightBackTop]; break;
                    }
                }
                // TODO: Other block types.
            }
        }

        
        #endregion

        #endregion
    }
}
