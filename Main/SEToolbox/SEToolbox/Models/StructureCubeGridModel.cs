namespace SEToolbox.Models
{
    using System;
    using System.Windows.Media.Media3D;
    using Sandbox.CommonLib.ObjectBuilders;
    using SEToolbox.Interop;
    using System.Linq;
    using System.Collections.Generic;

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

        public MyObjectBuilder_CubeGrid CubeGrid
        {
            get
            {
                return this.EntityBase as MyObjectBuilder_CubeGrid;
            }
        }

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

        public bool IsPiloted
        {
            get
            {
                return this.Pilots > 0;
            }
        }

        public bool IsDamaged
        {
            get
            {
                return this.CubeGrid.Skeleton.Count > 0;
            }
        }

        public int DamageCount
        {
            get
            {
                return this.CubeGrid.Skeleton.Count;
            }
        }

        public double Speed
        {
            get
            {
                return  (double)this.CubeGrid.LinearVelocity.Sum();
            }
        }

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

            foreach (var block in this.CubeGrid.CubeBlocks)
            {
                min.X = Math.Min(min.X, block.Min.X);
                min.Y = Math.Min(min.Y, block.Min.Y);
                min.Z = Math.Min(min.Z, block.Min.Z);
                max.X = Math.Max(max.X, block.Max.X);
                max.Y = Math.Max(max.Y, block.Max.Y);
                max.Z = Math.Max(max.Z, block.Max.Z);

                calcMass += SpaceEngineersAPI.FetchCubeBlockMass(block.SubtypeName, this.CubeGrid.GridSizeEnum);
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

        #endregion
    }
}
