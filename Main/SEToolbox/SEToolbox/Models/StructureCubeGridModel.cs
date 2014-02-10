namespace SEToolbox.Models
{
    using Sandbox.CommonLib.ObjectBuilders;
    using Sandbox.CommonLib.ObjectBuilders.Definitions;
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
                return this.CubeGrid.LinearVelocity.LinearVector();
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
                max.X = Math.Max(max.X, block.Max.X);
                max.Y = Math.Max(max.Y, block.Max.Y);
                max.Z = Math.Max(max.Z, block.Max.Z);

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

        public void ConvertFromLightToHeavyArmor()
        {
            foreach (var cube in this.CubeGrid.CubeBlocks)
            {
                if (cube.SubtypeName.StartsWith("LargeBlockArmor"))
                {
                    cube.SubtypeName = cube.SubtypeName.Replace("LargeBlockArmor", "LargeHeavyBlockArmor");
                }
                else if (cube.SubtypeName.StartsWith("SmallBlockArmor"))
                {
                    cube.SubtypeName = cube.SubtypeName.Replace("SmallBlockArmor", "SmallHeavyBlockArmor");
                }
            }

            this.UpdateFromEntityBase();
        }

        public void ConvertFromHeavyToLightArmor()
        {
            foreach (var cube in this.CubeGrid.CubeBlocks)
            {
                if (cube.SubtypeName.StartsWith("LargeHeavyBlockArmor"))
                {
                    cube.SubtypeName = cube.SubtypeName.Replace("LargeHeavyBlockArmor", "LargeBlockArmor");
                }
                else if (cube.SubtypeName.StartsWith("SmallHeavyBlockArmor"))
                {
                    cube.SubtypeName = cube.SubtypeName.Replace("SmallHeavyBlockArmor", "SmallBlockArmor");
                }
            }

            this.UpdateFromEntityBase();
        }

        public void ConvertToFramework()
        {
            foreach (var cube in this.CubeGrid.CubeBlocks)
            {
                cube.IntegrityPercent = 0.1f;
            }

            this.UpdateFromEntityBase();
        }

        public void ConvertToCompleteStructure()
        {
            foreach (var cube in this.CubeGrid.CubeBlocks)
            {
                cube.IntegrityPercent = 1;
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
            SubtypeId.LargeBlockArmorBlockRed,
            SubtypeId.LargeBlockArmorBlockYellow,
            SubtypeId.LargeBlockArmorBlockBlue,
            SubtypeId.LargeBlockArmorBlockGreen,
            SubtypeId.LargeBlockArmorBlockBlack,
            SubtypeId.LargeBlockArmorBlockWhite,
            SubtypeId.LargeBlockArmorSlope,
            SubtypeId.LargeBlockArmorSlopeRed,
            SubtypeId.LargeBlockArmorSlopeYellow,
            SubtypeId.LargeBlockArmorSlopeBlue,
            SubtypeId.LargeBlockArmorSlopeGreen,
            SubtypeId.LargeBlockArmorSlopeBlack,
            SubtypeId.LargeBlockArmorSlopeWhite,
            SubtypeId.LargeBlockArmorCorner,
            SubtypeId.LargeBlockArmorCornerRed,
            SubtypeId.LargeBlockArmorCornerYellow,
            SubtypeId.LargeBlockArmorCornerBlue,
            SubtypeId.LargeBlockArmorCornerGreen,
            SubtypeId.LargeBlockArmorCornerBlack,
            SubtypeId.LargeBlockArmorCornerWhite,
            SubtypeId.LargeBlockArmorCornerInv,
            SubtypeId.LargeBlockArmorCornerInvRed,
            SubtypeId.LargeBlockArmorCornerInvYellow,
            SubtypeId.LargeBlockArmorCornerInvBlue,
            SubtypeId.LargeBlockArmorCornerInvGreen,
            SubtypeId.LargeBlockArmorCornerInvBlack,
            SubtypeId.LargeBlockArmorCornerInvWhite,
            SubtypeId.LargeHeavyBlockArmorBlock,
            SubtypeId.LargeHeavyBlockArmorBlockRed,
            SubtypeId.LargeHeavyBlockArmorBlockYellow,
            SubtypeId.LargeHeavyBlockArmorBlockBlue,
            SubtypeId.LargeHeavyBlockArmorBlockGreen,
            SubtypeId.LargeHeavyBlockArmorBlockBlack,
            SubtypeId.LargeHeavyBlockArmorBlockWhite,
            SubtypeId.LargeHeavyBlockArmorSlope,
            SubtypeId.LargeHeavyBlockArmorSlopeRed,
            SubtypeId.LargeHeavyBlockArmorSlopeYellow,
            SubtypeId.LargeHeavyBlockArmorSlopeBlue,
            SubtypeId.LargeHeavyBlockArmorSlopeGreen,
            SubtypeId.LargeHeavyBlockArmorSlopeBlack,
            SubtypeId.LargeHeavyBlockArmorSlopeWhite,
            SubtypeId.LargeHeavyBlockArmorCorner,
            SubtypeId.LargeHeavyBlockArmorCornerRed,
            SubtypeId.LargeHeavyBlockArmorCornerYellow,
            SubtypeId.LargeHeavyBlockArmorCornerBlue,
            SubtypeId.LargeHeavyBlockArmorCornerGreen,
            SubtypeId.LargeHeavyBlockArmorCornerBlack,
            SubtypeId.LargeHeavyBlockArmorCornerWhite,
            SubtypeId.LargeHeavyBlockArmorCornerInv,
            SubtypeId.LargeHeavyBlockArmorCornerInvRed,
            SubtypeId.LargeHeavyBlockArmorCornerInvYellow,
            SubtypeId.LargeHeavyBlockArmorCornerInvBlue,
            SubtypeId.LargeHeavyBlockArmorCornerInvGreen,
            SubtypeId.LargeHeavyBlockArmorCornerInvBlack,
            SubtypeId.LargeHeavyBlockArmorCornerInvWhite,
            SubtypeId.SmallBlockArmorBlock,
            SubtypeId.SmallBlockArmorBlockRed,
            SubtypeId.SmallBlockArmorBlockYellow,
            SubtypeId.SmallBlockArmorBlockBlue,
            SubtypeId.SmallBlockArmorBlockGreen,
            SubtypeId.SmallBlockArmorBlockBlack,
            SubtypeId.SmallBlockArmorBlockWhite,
            SubtypeId.SmallBlockArmorSlope,
            SubtypeId.SmallBlockArmorSlopeRed,
            SubtypeId.SmallBlockArmorSlopeYellow,
            SubtypeId.SmallBlockArmorSlopeBlue,
            SubtypeId.SmallBlockArmorSlopeGreen,
            SubtypeId.SmallBlockArmorSlopeBlack,
            SubtypeId.SmallBlockArmorSlopeWhite,
            SubtypeId.SmallBlockArmorCorner,
            SubtypeId.SmallBlockArmorCornerRed,
            SubtypeId.SmallBlockArmorCornerYellow,
            SubtypeId.SmallBlockArmorCornerBlue,
            SubtypeId.SmallBlockArmorCornerGreen,
            SubtypeId.SmallBlockArmorCornerBlack,
            SubtypeId.SmallBlockArmorCornerWhite,
            SubtypeId.SmallBlockArmorCornerInv,
            SubtypeId.SmallBlockArmorCornerInvRed,
            SubtypeId.SmallBlockArmorCornerInvYellow,
            SubtypeId.SmallBlockArmorCornerInvBlue,
            SubtypeId.SmallBlockArmorCornerInvGreen,
            SubtypeId.SmallBlockArmorCornerInvBlack,
            SubtypeId.SmallBlockArmorCornerInvWhite,
            SubtypeId.SmallHeavyBlockArmorBlock,
            SubtypeId.SmallHeavyBlockArmorBlockRed,
            SubtypeId.SmallHeavyBlockArmorBlockYellow,
            SubtypeId.SmallHeavyBlockArmorBlockBlue,
            SubtypeId.SmallHeavyBlockArmorBlockGreen,
            SubtypeId.SmallHeavyBlockArmorBlockBlack,
            SubtypeId.SmallHeavyBlockArmorBlockWhite,
            SubtypeId.SmallHeavyBlockArmorSlope,
            SubtypeId.SmallHeavyBlockArmorSlopeRed,
            SubtypeId.SmallHeavyBlockArmorSlopeYellow,
            SubtypeId.SmallHeavyBlockArmorSlopeBlue,
            SubtypeId.SmallHeavyBlockArmorSlopeGreen,
            SubtypeId.SmallHeavyBlockArmorSlopeBlack,
            SubtypeId.SmallHeavyBlockArmorSlopeWhite,
            SubtypeId.SmallHeavyBlockArmorCorner,
            SubtypeId.SmallHeavyBlockArmorCornerRed,
            SubtypeId.SmallHeavyBlockArmorCornerYellow,
            SubtypeId.SmallHeavyBlockArmorCornerBlue,
            SubtypeId.SmallHeavyBlockArmorCornerGreen,
            SubtypeId.SmallHeavyBlockArmorCornerBlack,
            SubtypeId.SmallHeavyBlockArmorCornerWhite,
            SubtypeId.SmallHeavyBlockArmorCornerInv,
            SubtypeId.SmallHeavyBlockArmorCornerInvRed,
            SubtypeId.SmallHeavyBlockArmorCornerInvYellow,
            SubtypeId.SmallHeavyBlockArmorCornerInvBlue,
            SubtypeId.SmallHeavyBlockArmorCornerInvGreen,
            SubtypeId.SmallHeavyBlockArmorCornerInvBlack,
            SubtypeId.SmallHeavyBlockArmorCornerInvWhite,
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
                    PersistentFlags = block.PersistentFlags,
                    Min = block.Min.Mirror(xMirror, xAxis, yMirror, yAxis, zMirror, zAxis),
                    Max = block.Max.Mirror(xMirror, xAxis, yMirror, yAxis, zMirror, zAxis),
                    Orientation = VRageMath.Quaternion.CreateFromRotationMatrix(Matrix.CreateLookAt(Vector3.Zero, Vector3.Forward, Vector3.Up))
                    //Orientation = MirrorCubeOrientation(block.SubtypeName, block.Orientation, xMirror, yMirror, zMirror);
                };
                MirrorCubeOrientation(block.SubtypeName, block.Orientation, xMirror, yMirror, zMirror, ref newBlock);

                // Don't place a block it one already exists there in the mirror.
                if (integrate && viewModel.CubeGrid.CubeBlocks.Any(b => b.Min == newBlock.Min || b.Max == newBlock.Min))
                    continue;

                if (block.PositionAndOrientation.HasValue)
                    newBlock.PositionAndOrientation = new MyPositionAndOrientation()
                    {
                        Forward = block.PositionAndOrientation.Value.Forward,
                        Position = block.PositionAndOrientation.Value.Position,
                        Up = block.PositionAndOrientation.Value.Up,
                    };

                blocks.Add(newBlock);
            }
            return blocks;
        }

        // TODO: change to a return type later when finished testing.
        private static void MirrorCubeOrientation(string subtypeName, VRageMath.Quaternion orientation, Mirror xMirror, Mirror yMirror, Mirror zMirror, ref MyObjectBuilder_CubeBlock block)
        {
            if (xMirror != Mirror.None)
            {
                if (subtypeName.Contains("ArmorSlope"))
                {
                    var cubeType = SpaceEngineersAPI.CubeOrientations.FirstOrDefault(x => x.Value == orientation && x.Key.ToString().StartsWith("Slope"));
                    switch (cubeType.Key)
                    {
                        case CubeType.SlopeCenterBackTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeCenterBackTop]; break;
                        case CubeType.SlopeRightBackCenter: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeLeftBackCenter]; break;
                        case CubeType.SlopeLeftBackCenter: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeRightBackCenter]; break;
                        case CubeType.SlopeCenterBackBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeCenterBackBottom]; break;
                        case CubeType.SlopeRightCenterTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeLeftCenterTop]; break;
                        case CubeType.SlopeLeftCenterTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeRightCenterTop]; break;
                        case CubeType.SlopeRightCenterBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeLeftCenterBottom]; break;
                        case CubeType.SlopeLeftCenterBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeRightCenterBottom]; break;
                        case CubeType.SlopeCenterFrontTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeCenterFrontTop]; break;
                        case CubeType.SlopeRightFrontCenter: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeLeftFrontCenter]; break;
                        case CubeType.SlopeLeftFrontCenter: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeRightFrontCenter]; break;
                        case CubeType.SlopeCenterFrontBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeCenterFrontBottom]; break;
                    }
                }
                else if (subtypeName.Contains("ArmorCornerInv"))
                {
                    var cubeType = SpaceEngineersAPI.CubeOrientations.FirstOrDefault(x => x.Value == orientation && x.Key.ToString().StartsWith("InverseCorner"));
                    switch (cubeType.Key)
                    {
                        case CubeType.InverseCornerLeftFrontTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.InverseCornerRightFrontTop]; break;
                        case CubeType.InverseCornerRightFrontTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.InverseCornerLeftFrontTop]; break;
                        case CubeType.InverseCornerRightBackTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.InverseCornerLeftBackTop]; break;
                        case CubeType.InverseCornerLeftBackTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.InverseCornerRightBackTop]; break;
                        case CubeType.InverseCornerLeftFrontBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.InverseCornerRightFrontBottom]; break;
                        case CubeType.InverseCornerRightFrontBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.InverseCornerLeftFrontBottom]; break;
                        case CubeType.InverseCornerLeftBackBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.InverseCornerRightBackBottom]; break;
                        case CubeType.InverseCornerRightBackBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.InverseCornerLeftBackBottom]; break;
                    }
                }
                else if (subtypeName.Contains("ArmorCorner"))
                {
                    var cubeType = SpaceEngineersAPI.CubeOrientations.FirstOrDefault(x => x.Value == orientation && x.Key.ToString().StartsWith("NormalCorner"));
                    switch (cubeType.Key)
                    {
                        case CubeType.NormalCornerLeftFrontTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerRightFrontTop]; break;
                        case CubeType.NormalCornerRightFrontTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerLeftFrontTop]; break;
                        case CubeType.NormalCornerLeftBackTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerRightBackTop]; break;
                        case CubeType.NormalCornerRightBackTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerLeftBackTop]; break;
                        case CubeType.NormalCornerLeftFrontBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerRightFrontBottom]; break;
                        case CubeType.NormalCornerRightFrontBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerLeftFrontBottom]; break;
                        case CubeType.NormalCornerLeftBackBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerRightBackBottom]; break;
                        case CubeType.NormalCornerRightBackBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerLeftBackBottom]; break;
                    }
                }
                //else if (subtypeName.Contains("LargeRamp"))
                //{
                //    var cubeType = SpaceEngineersAPI.CubeOrientations.FirstOrDefault(x => x.Value == orientation && x.Key.ToString().StartsWith("NormalCorner"));
                //    switch (cubeType.Key)
                //    {
                //        case CubeType.NormalCornerLeftFrontTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerRightFrontTop]; break;
                //        case CubeType.NormalCornerRightFrontTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerLeftFrontTop]; break;
                //        case CubeType.NormalCornerLeftBackTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerRightBackTop]; break;
                //        case CubeType.NormalCornerRightBackTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerLeftBackTop]; break;
                //        case CubeType.NormalCornerLeftFrontBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerRightFrontBottom]; break;
                //        case CubeType.NormalCornerRightFrontBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerLeftFrontBottom]; break;
                //        case CubeType.NormalCornerLeftBackBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerRightBackBottom]; break;
                //        case CubeType.NormalCornerRightBackBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerLeftBackBottom]; break;
                //    }
                //}
                // TODO: Other block types.
            }
            else if (yMirror != Mirror.None)
            {
                if (subtypeName.Contains("ArmorSlope"))
                {
                    var cubeType = SpaceEngineersAPI.CubeOrientations.FirstOrDefault(x => x.Value == orientation && x.Key.ToString().StartsWith("Slope"));
                    switch (cubeType.Key)
                    {
                        case CubeType.SlopeCenterBackTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeCenterFrontTop]; break;
                        case CubeType.SlopeRightBackCenter: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeRightFrontCenter]; break;
                        case CubeType.SlopeLeftBackCenter: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeLeftFrontCenter]; break;
                        case CubeType.SlopeCenterBackBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeCenterFrontBottom]; break;
                        case CubeType.SlopeRightCenterTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeRightCenterTop]; break;
                        case CubeType.SlopeLeftCenterTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeLeftCenterTop]; break;
                        case CubeType.SlopeRightCenterBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeRightCenterBottom]; break;
                        case CubeType.SlopeLeftCenterBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeLeftCenterBottom]; break;
                        case CubeType.SlopeCenterFrontTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeCenterBackTop]; break;
                        case CubeType.SlopeRightFrontCenter: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeRightBackCenter]; break;
                        case CubeType.SlopeLeftFrontCenter: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeLeftBackCenter]; break;
                        case CubeType.SlopeCenterFrontBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeCenterBackBottom]; break;
                    }
                }
                else if (subtypeName.Contains("ArmorCornerInv"))
                {
                    var cubeType = SpaceEngineersAPI.CubeOrientations.FirstOrDefault(x => x.Value == orientation && x.Key.ToString().StartsWith("InverseCorner"));
                    switch (cubeType.Key)
                    {
                        case CubeType.InverseCornerLeftFrontTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.InverseCornerLeftBackTop]; break;
                        case CubeType.InverseCornerRightFrontTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.InverseCornerRightBackTop]; break;
                        case CubeType.InverseCornerRightBackTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.InverseCornerRightFrontTop]; break;
                        case CubeType.InverseCornerLeftBackTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.InverseCornerLeftFrontTop]; break;
                        case CubeType.InverseCornerLeftFrontBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.InverseCornerLeftBackBottom]; break;
                        case CubeType.InverseCornerRightFrontBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.InverseCornerRightBackBottom]; break;
                        case CubeType.InverseCornerLeftBackBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.InverseCornerLeftFrontBottom]; break;
                        case CubeType.InverseCornerRightBackBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.InverseCornerRightFrontBottom]; break;
                    }
                }
                else if (subtypeName.Contains("ArmorCorner"))
                {
                    var cubeType = SpaceEngineersAPI.CubeOrientations.FirstOrDefault(x => x.Value == orientation && x.Key.ToString().StartsWith("NormalCorner"));
                    switch (cubeType.Key)
                    {
                        case CubeType.NormalCornerLeftFrontTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerLeftBackTop]; break;
                        case CubeType.NormalCornerRightFrontTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerRightBackTop]; break;
                        case CubeType.NormalCornerLeftBackTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerLeftFrontTop]; break;
                        case CubeType.NormalCornerRightBackTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerRightFrontTop]; break;
                        case CubeType.NormalCornerLeftFrontBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerLeftBackBottom]; break;
                        case CubeType.NormalCornerRightFrontBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerRightBackBottom]; break;
                        case CubeType.NormalCornerLeftBackBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerLeftFrontBottom]; break;
                        case CubeType.NormalCornerRightBackBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerRightFrontBottom]; break;
                    }
                }
                // TODO: Other block types.
            }
            else if (zMirror != Mirror.None)
            {
                if (subtypeName.Contains("ArmorSlope"))
                {
                    var cubeType = SpaceEngineersAPI.CubeOrientations.FirstOrDefault(x => x.Value == orientation && x.Key.ToString().StartsWith("Slope"));
                    switch (cubeType.Key)
                    {
                        case CubeType.SlopeCenterBackTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeCenterBackBottom]; break;
                        case CubeType.SlopeRightBackCenter: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeRightBackCenter]; break;
                        case CubeType.SlopeLeftBackCenter: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeLeftBackCenter]; break;
                        case CubeType.SlopeCenterBackBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeCenterBackTop]; break;
                        case CubeType.SlopeRightCenterTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeRightCenterBottom]; break;
                        case CubeType.SlopeLeftCenterTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeLeftCenterBottom]; break;
                        case CubeType.SlopeRightCenterBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeRightCenterTop]; break;
                        case CubeType.SlopeLeftCenterBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeLeftCenterTop]; break;
                        case CubeType.SlopeCenterFrontTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeCenterFrontBottom]; break;
                        case CubeType.SlopeRightFrontCenter: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeRightFrontCenter]; break;
                        case CubeType.SlopeLeftFrontCenter: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeLeftFrontCenter]; break;
                        case CubeType.SlopeCenterFrontBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeCenterFrontTop]; break;
                    }
                }
                else if (subtypeName.Contains("ArmorCornerInv"))
                {
                    var cubeType = SpaceEngineersAPI.CubeOrientations.FirstOrDefault(x => x.Value == orientation && x.Key.ToString().StartsWith("InverseCorner"));
                    switch (cubeType.Key)
                    {
                        case CubeType.InverseCornerLeftFrontTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.InverseCornerLeftFrontBottom]; break;
                        case CubeType.InverseCornerRightFrontTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.InverseCornerRightFrontBottom]; break;
                        case CubeType.InverseCornerRightBackTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.InverseCornerRightBackBottom]; break;
                        case CubeType.InverseCornerLeftBackTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.InverseCornerLeftBackBottom]; break;
                        case CubeType.InverseCornerLeftFrontBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.InverseCornerLeftFrontTop]; break;
                        case CubeType.InverseCornerRightFrontBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.InverseCornerRightFrontTop]; break;
                        case CubeType.InverseCornerLeftBackBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.InverseCornerLeftBackTop]; break;
                        case CubeType.InverseCornerRightBackBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.InverseCornerRightBackTop]; break;
                    }
                }
                else if (subtypeName.Contains("ArmorCorner"))
                {
                    var cubeType = SpaceEngineersAPI.CubeOrientations.FirstOrDefault(x => x.Value == orientation && x.Key.ToString().StartsWith("NormalCorner"));
                    switch (cubeType.Key)
                    {
                        case CubeType.NormalCornerLeftFrontTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerLeftFrontBottom]; break;
                        case CubeType.NormalCornerRightFrontTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerRightFrontBottom]; break;
                        case CubeType.NormalCornerLeftBackTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerLeftBackBottom]; break;
                        case CubeType.NormalCornerRightBackTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerRightBackBottom]; break;
                        case CubeType.NormalCornerLeftFrontBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerLeftFrontTop]; break;
                        case CubeType.NormalCornerRightFrontBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerRightFrontTop]; break;
                        case CubeType.NormalCornerLeftBackBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerLeftBackTop]; break;
                        case CubeType.NormalCornerRightBackBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerRightBackTop]; break;
                    }
                }
                // TODO: Other block types.
            }
        }

        
        #endregion

        #endregion
    }
}
