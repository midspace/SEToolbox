//namespace SEToolbox.Models
//{
//    using Sandbox.CommonLib.ObjectBuilders;
//    using Sandbox.CommonLib.ObjectBuilders.Voxels;
//    using SEToolbox.Interop;
//    using System;
//    using System.Windows.Media.Media3D;

//    public class StructureCubeGrid : StructureBase
//    {
//        #region Fields

//        //private Type structureType;
//        private string filename;
//        private ClassType classType;
//        //private PositionAndOrientation positionAndOrientation;
//        private Sandbox.CommonLib.ObjectBuilders.MyCubeSize gridSize;
//        private bool isStatic;
////        private List<Cube> cubes;
//        private Point3D min;
//        private Point3D max;
//        private Vector3D size;

//        MyObjectBuilder_EntityBase entityBase;

//        #endregion

//        #region ctor

//        public StructureCubeGrid()
//        {
//        }

//        public StructureCubeGrid(MyObjectBuilder_EntityBase entityBase)
//        {
//            this.EntityBase = entityBase;
//        }

//        #endregion

//        #region Properties


//        public MyObjectBuilder_CubeGrid CubeGrid
//        {
//            get
//            {
//                return this.EntityBase as MyObjectBuilder_CubeGrid;
//            }
//        }

//        //public Type StructureType
//        //{
//        //    get
//        //    {
//        //        return this.structureType;
//        //    }

//        //    set
//        //    {
//        //        if (value != this.structureType)
//        //        {
//        //            this.structureType = value;
//        //            this.OnPropertyChanged("StructureType");
//        //        }
//        //    }
//        //}

//        public long EntityId
//        {
//            get
//            {
//                return this.entityBase.EntityId;
//            }

//            set
//            {
//                if (value != this.entityBase.EntityId)
//                {
//                    this.entityBase.EntityId = value;
//                    this.OnPropertyChanged("EntityId");
//                }
//            }
//        }

//        public string Filename
//        {
//            get
//            {
//                return this.filename;
//            }

//            set
//            {
//                if (value != this.filename)
//                {
//                    this.filename = value;
//                    this.OnPropertyChanged("Filename");
//                }
//            }
//        }

//        public ClassType ClassType
//        {
//            get
//            {
//                return this.classType;
//            }

//            set
//            {
//                if (value != this.classType)
//                {
//                    this.classType = value;
//                    this.OnPropertyChanged("ClassType");
//                }
//            }
//        }

//        public bool IsCharacter
//        {
//            get
//            {
//                return this.ClassType == ClassType.Character;
//            }
//        }

//        public bool IsVoxel
//        {
//            get
//            {
//                return this.ClassType == ClassType.Voxel;
//            }
//        }

//        public bool IsCubes
//        {
//            get
//            {
//                return this.ClassType == ClassType.Station;
//            }
//        }

//        //public PositionAndOrientation PositionAndOrientation
//        //{
//        //    get
//        //    {
//        //        return this.positionAndOrientation;
//        //    }

//        //    set
//        //    {
//        //        this.positionAndOrientation = value;

//        //        this.OnPropertyChanged("PositionAndOrientation");
//        //    }
//        //}

//        public Sandbox.CommonLib.ObjectBuilders.MyCubeSize GridSize
//        {
//            get
//            {
//                return this.gridSize;
//            }

//            set
//            {
//                if (value != this.gridSize)
//                {
//                    this.gridSize = value;
//                    this.OnPropertyChanged("GridSize");
//                }
//            }
//        }

//        public bool IsStatic
//        {
//            get
//            {
//                return this.isStatic;
//            }

//            set
//            {
//                if (value != this.isStatic)
//                {
//                    this.isStatic = value;
//                    this.OnPropertyChanged("IsStatic");
//                }
//            }
//        }

//        //public List<Cube> Cubes
//        //{
//        //    get
//        //    {
//        //        return this.cubes;
//        //    }

//        //    set
//        //    {
//        //        this.cubes = value;

//        //        this.OnPropertyChanged("Cubes");
//        //    }
//        //}

//        public Point3D Min
//        {
//            get
//            {
//                return this.min;
//            }

//            set
//            {
//                if (value != this.min)
//                {
//                    this.min = value;
//                    this.OnPropertyChanged("Min");
//                }
//            }
//        }

//        public Point3D Max
//        {
//            get
//            {
//                return this.max;
//            }

//            set
//            {
//                if (value != this.max)
//                {
//                    this.max = value;
//                    this.OnPropertyChanged("Max");
//                }
//            }
//        }

//        public Vector3D Size
//        {
//            get
//            {
//                return this.size;
//            }

//            set
//            {
//                if (value != this.size)
//                {
//                    this.size = value;
//                    this.OnPropertyChanged("Size");
//                }
//            }
//        }

//        public MyObjectBuilder_EntityBase EntityBase
//        {
//            get
//            {
//                return this.entityBase;
//            }
//            set
//            {
//                if (value != this.entityBase)
//                {
//                    this.entityBase = value;
//                    this.UpdateFromEntityBase();
//                    this.OnPropertyChanged("Entitybase");
//                }
//            }
//        }

//        #endregion


//        private void UpdateFromEntityBase()
//        {
//            if (this.entityBase is MyObjectBuilder_VoxelMap)
//            {
//                var voxel = this.entityBase as MyObjectBuilder_VoxelMap;

//                this.ClassType = ClassType.Voxel;
//                this.Filename = voxel.Filename;
//            }
//            else if (this.entityBase is MyObjectBuilder_Character)
//            {
//                this.ClassType = ClassType.Character;
//            }
//            else if (this.entityBase is MyObjectBuilder_CubeGrid)
//            {
//                var cube = this.entityBase as MyObjectBuilder_CubeGrid;

//                this.IsStatic = cube.IsStatic;
//                this.GridSize = cube.GridSizeEnum;

//                if (this.IsStatic && cube.GridSizeEnum == MyCubeSize.Large)
//                {
//                    this.ClassType = ClassType.Station;
//                }
//                else if (!this.IsStatic && cube.GridSizeEnum == MyCubeSize.Large)
//                {
//                    this.ClassType = ClassType.LargeShip;
//                }
//                else if (!this.IsStatic && cube.GridSizeEnum == MyCubeSize.Small)
//                {
//                    this.ClassType = ClassType.SmallShip;
//                }

//                var min = new Point3D(double.MaxValue, double.MaxValue, double.MaxValue);
//                var max = new Point3D(double.MinValue, double.MinValue, double.MinValue);

//                foreach (var block in cube.CubeBlocks)
//                {
//                    min.X = Math.Min(min.X, block.Min.X);
//                    min.Y = Math.Min(min.Y, block.Min.Y);
//                    min.Z = Math.Min(min.Z, block.Min.Z);
//                    max.X = Math.Max(max.X, block.Max.X);
//                    max.Y = Math.Max(max.Y, block.Max.Y);
//                    max.Z = Math.Max(max.Z, block.Max.Z);
//                }

//                var size = max - min;
//                size.X++;
//                size.Y++;
//                size.Z++;

//                this.Min = min;
//                this.Max = max;
//                this.Size = size;
//            }
//            else
//            {
//                this.ClassType = ClassType.Unknown;
//            }
//        }
//    }
//}
