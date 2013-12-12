namespace SEToolbox.Models
{
    using Sandbox.CommonLib.ObjectBuilders;
    using Sandbox.CommonLib.ObjectBuilders.Definitions;
    using SEToolbox.Interop;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Windows.Media.Media3D;
    using System.Xml.Serialization;

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
                return (double)this.CubeGrid.LinearVelocity.Sum();
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
            Dictionary<string, MyObjectBuilder_BlueprintDefinition.Item> ingotRequirements = new Dictionary<string, MyObjectBuilder_BlueprintDefinition.Item>();
            Dictionary<string, MyObjectBuilder_BlueprintDefinition.Item> oreRequirements = new Dictionary<string, MyObjectBuilder_BlueprintDefinition.Item>();
            //MyObjectBuilder_BlueprintDefinition requirements2 = new MyObjectBuilder_BlueprintDefinition();
            TimeSpan timeTaken = new TimeSpan();

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

            StringBuilder bld = new StringBuilder();
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

        public void ResetSpeed()
        {
            this.CubeGrid.LinearVelocity = new VRageMath.Vector3(0, 0, 0);
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
                cube.DamagedComponents = new MyObjectBuilder_CubeBlock.DamagedComponent[]
                {
                    new MyObjectBuilder_CubeBlock.DamagedComponent()
                    {
                        IntegrityPercentage = 0.2f
                    }
                };
            }

            this.UpdateFromEntityBase();
        }

        public void ConvertToCompleteStructure()
        {
            foreach (var cube in this.CubeGrid.CubeBlocks)
            {
                cube.DamagedComponents = null;
            }

            this.UpdateFromEntityBase();
        }

        #endregion
    }
}
