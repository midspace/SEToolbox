namespace SEToolbox.Models
{
    using System;
    using System.Windows.Media.Media3D;
    using Sandbox.CommonLib.ObjectBuilders;
    using SEToolbox.Interop;
    using System.Linq;

    public class StructureCubeGridModel : StructureBaseModel
    {
        #region Fields

        private Point3D min;
        private Point3D max;
        private Vector3D size;
        private bool isPiloted;

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

        public bool IsPiloted
        {
            get
            {
                return this.isPiloted;
            }

            set
            {
                if (value != this.isPiloted)
                {
                    this.isPiloted = value;
                    this.RaisePropertyChanged(() => IsPiloted);
                }
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

            foreach (var block in this.CubeGrid.CubeBlocks)
            {
                min.X = Math.Min(min.X, block.Min.X);
                min.Y = Math.Min(min.Y, block.Min.Y);
                min.Z = Math.Min(min.Z, block.Min.Z);
                max.X = Math.Max(max.X, block.Max.X);
                max.Y = Math.Max(max.Y, block.Max.Y);
                max.Z = Math.Max(max.Z, block.Max.Z);
            }

            var size = max - min;
            size.X++;
            size.Y++;
            size.Z++;

            this.Min = min;
            this.Max = max;
            this.Size = size;

            this.Description = string.Format("{0}", this.Size);
        }

        public bool HasPilot()
        {
            var cube = this.CubeGrid.CubeBlocks.FirstOrDefault<MyObjectBuilder_CubeBlock>(e => e is MyObjectBuilder_Cockpit && ((MyObjectBuilder_Cockpit)e).Pilot != null);

            if (cube != null)
            {
                var cockpit = cube as MyObjectBuilder_Cockpit;
                this.IsPiloted = cockpit.Pilot != null;
                return this.IsPiloted;
            }

            return false;
        }

        public IStructureBase GetPilot()
        {
            var cube = this.CubeGrid.CubeBlocks.FirstOrDefault<MyObjectBuilder_CubeBlock>(e => e is MyObjectBuilder_Cockpit && ((MyObjectBuilder_Cockpit)e).Pilot != null);

            if (cube != null)
            {
                var cockpit = cube as MyObjectBuilder_Cockpit;

                if (cockpit.Pilot != null)
                {
                    return StructureBaseModel.Create(cockpit.Pilot);
                }
            }

            return null;
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
