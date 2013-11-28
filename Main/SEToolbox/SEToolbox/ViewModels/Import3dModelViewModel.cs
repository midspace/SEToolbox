﻿namespace SEToolbox.ViewModels
{
    using Sandbox.CommonLib.ObjectBuilders;
    using SEToolbox.Interfaces;
    using SEToolbox.Interop;
    using SEToolbox.Models;
    using SEToolbox.Properties;
    using SEToolbox.Services;
    using SEToolbox.Support;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Windows.Media.Imaging;
    using System.Windows.Media.Media3D;

    public class Import3dModelViewModel : BaseViewModel
    {
        #region Fields

        private readonly IDialogService dialogService;
        private readonly Func<IOpenFileDialog> openFileDialogFactory;
        private Import3dModelModel dataModel;

        private bool? closeResult;
        private bool isBusy;

        #endregion

        #region Constructors

        public Import3dModelViewModel(BaseViewModel parentViewModel, Import3dModelModel dataModel)
            : this(parentViewModel, dataModel, ServiceLocator.Resolve<IDialogService>(), () => ServiceLocator.Resolve<IOpenFileDialog>())
        {
        }

        public Import3dModelViewModel(BaseViewModel parentViewModel, Import3dModelModel dataModel, IDialogService dialogService, Func<IOpenFileDialog> openFileDialogFactory)
            : base(parentViewModel)
        {
            Contract.Requires(dialogService != null);
            Contract.Requires(openFileDialogFactory != null);

            this.dialogService = dialogService;
            this.openFileDialogFactory = openFileDialogFactory;
            this.dataModel = dataModel;
            this.dataModel.PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
            {
                // Will bubble property change events from the Model to the ViewModel.
                this.OnPropertyChanged(e.PropertyName);
            };

            this.IsMultipleScale = true;
            this.MultipleScale = 1;
            this.MaxLengthScale = 100;
            this.ClassType = ImportClassType.SmallShip;
            this.ArmorType = ImportArmorType.Light;
        }

        #endregion

        #region Properties

        public ICommand BrowseImageCommand
        {
            get
            {
                return new DelegateCommand(new Action(BrowseImageExecuted), new Func<bool>(BrowseImageCanExecute));
            }
        }

        public ICommand CreateCommand
        {
            get
            {
                return new DelegateCommand(new Action(CreateExecuted), new Func<bool>(CreateCanExecute));
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                return new DelegateCommand(new Action(CancelExecuted), new Func<bool>(CancelCanExecute));
            }
        }

        /// <summary>
        /// Gets or sets the DialogResult of the View.  If True or False is passed, this initiates the Close().
        /// </summary>
        public bool? CloseResult
        {
            get
            {
                return this.closeResult;
            }

            set
            {
                this.closeResult = value;
                this.RaisePropertyChanged(() => CloseResult);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the View is currently in the middle of an asynchonise operation.
        /// </summary>
        public bool IsBusy
        {
            get
            {
                return this.isBusy;
            }

            set
            {
                if (value != this.isBusy)
                {
                    this.isBusy = value;
                    this.RaisePropertyChanged(() => IsBusy);
                    if (this.isBusy)
                    {
                        System.Windows.Forms.Application.DoEvents();
                    }
                }
            }
        }

        public string Filename
        {
            get
            {
                return this.dataModel.Filename;
            }

            set
            {
                this.dataModel.Filename = value;
            }
        }

        public bool IsValidModel
        {
            get
            {
                return this.dataModel.IsValidModel;
            }

            set
            {
                this.dataModel.IsValidModel = value;
            }
        }

        public BindableSize3DModel OriginalModelSize
        {
            get
            {
                return this.dataModel.OriginalModelSize;
            }

            set
            {
                this.dataModel.OriginalModelSize = value;
            }
        }

        public BindableSize3DModel NewModelSize
        {
            get
            {
                return this.dataModel.NewModelSize;
            }

            set
            {
                this.dataModel.NewModelSize = value;
                this.ProcessModelScale();
            }
        }

        public BindablePoint3DModel Position
        {
            get
            {
                return this.dataModel.Position;
            }

            set
            {
                this.dataModel.Position = value;
            }
        }

        public BindableVector3DModel Forward
        {
            get
            {
                return this.dataModel.Forward;
            }

            set
            {
                this.dataModel.Forward = value;
            }
        }

        public BindableVector3DModel Up
        {
            get
            {
                return this.dataModel.Up;
            }

            set
            {
                this.dataModel.Up = value;
            }
        }

        public ImportClassType ClassType
        {
            get
            {
                return this.dataModel.ClassType;
            }

            set
            {
                this.dataModel.ClassType = value;
                this.ProcessModelScale();
            }
        }

        public ImportArmorType ArmorType
        {
            get
            {
                return this.dataModel.ArmorType;
            }

            set
            {
                this.dataModel.ArmorType = value;
            }
        }


        public double MultipleScale
        {
            get
            {
                return this.dataModel.MultipleScale;
            }

            set
            {
                this.dataModel.MultipleScale = value;
                this.ProcessModelScale();
            }
        }

        public double MaxLengthScale
        {
            get
            {
                return this.dataModel.MaxLengthScale;
            }

            set
            {
                this.dataModel.MaxLengthScale = value;
                this.ProcessModelScale();
            }
        }

        public double BuildDistance
        {
            get
            {
                return this.dataModel.BuildDistance;
            }

            set
            {
                this.dataModel.BuildDistance = value;
                this.ProcessModelScale();
            }
        }

        public bool IsMultipleScale
        {
            get
            {
                return this.dataModel.IsMultipleScale;
            }

            set
            {
                this.dataModel.IsMultipleScale = value;
                this.ProcessModelScale();
            }
        }

        public bool IsMaxLengthScale
        {
            get
            {
                return this.dataModel.IsMaxLengthScale;
            }

            set
            {
                this.dataModel.IsMaxLengthScale = value;
                this.ProcessModelScale();
            }
        }

        #endregion

        #region methods

        public bool BrowseImageCanExecute()
        {
            return true;
        }

        public void BrowseImageExecuted()
        {
            this.IsValidModel = false;

            IOpenFileDialog openFileDialog = openFileDialogFactory();
            openFileDialog.Filter = Resources.ImportModelFilter;
            openFileDialog.Title = Resources.ImportModelTitle;

            // Open the dialog
            DialogResult result = dialogService.ShowOpenFileDialog(this, openFileDialog);

            if (result == DialogResult.OK)
            {
                ProcessFilename(openFileDialog.FileName);
                this.Filename = openFileDialog.FileName;
            }
        }

        public bool CreateCanExecute()
        {
            return this.IsValidModel;
        }

        public void CreateExecuted()
        {
            this.CloseResult = true;
        }

        public bool CancelCanExecute()
        {
            return true;
        }

        public void CancelExecuted()
        {
            this.CloseResult = false;
        }

        #endregion

        #region methods

        private void ProcessFilename(string filename)
        {
            this.IsValidModel = false;
            this.IsBusy = true;

            if (File.Exists(filename))
            {
                this.OriginalModelSize = new BindableSize3DModel(0, 0, 0);
                this.NewModelSize = new BindableSize3DModel(0, 0, 0);

                // validate file is a real model.
                // read model properties.
                var size = Preview3DModel(filename);

                if (size != null)
                {
                    this.OriginalModelSize = size;

                    //this.Position = new BindablePoint3DModel(0, 0, 0);
                    //this.Position = new ThreeDPointModel(0, 0, 0);
                    //this.Forward = new ThreeDPointModel(0, 0, 1);
                    //this.Up = new ThreeDPointModel(0, 1, 0);

                    this.BuildDistance = 10;

                    this.IsValidModel = true;
                    this.ProcessModelScale();
                }
            }

            this.IsBusy = false;
        }

        private BindableSize3DModel Preview3DModel(string filename)
        {
            var voxFilename = ToolboxExtensions.ConvertPolyToVox(filename, 0, false);
            BindableSize3DModel size = new BindableSize3DModel();

            if (voxFilename != null)
            {
                using (BinaryReader reader = new BinaryReader(File.Open(voxFilename, FileMode.Open)))
                {
                    size.Width = reader.ReadInt32();
                    size.Depth = reader.ReadInt32();
                    size.Height = reader.ReadInt32();
                }

                File.Delete(voxFilename);
                return size;
            }

            return null;
        }

        private void ProcessModelScale()
        {
            if (this.IsValidModel)
            {
                if (this.IsMaxLengthScale)
                {
                    var factor = this.MaxLengthScale / Math.Max(Math.Max(this.OriginalModelSize.Height, this.OriginalModelSize.Width), this.OriginalModelSize.Depth);

                    this.NewModelSize.Height = (int)(factor * this.OriginalModelSize.Height);
                    this.NewModelSize.Width = (int)(factor * this.OriginalModelSize.Width);
                    this.NewModelSize.Depth = (int)(factor * this.OriginalModelSize.Depth);
                }
                else if (this.IsMultipleScale)
                {
                    this.NewModelSize.Height = (int)(this.MultipleScale * this.OriginalModelSize.Height);
                    this.NewModelSize.Width = (int)(this.MultipleScale * this.OriginalModelSize.Width);
                    this.NewModelSize.Depth = (int)(this.MultipleScale * this.OriginalModelSize.Depth);
                }

                double vectorDistance = this.BuildDistance;

                switch (this.ClassType)
                {
                    case ImportClassType.SmallShip: vectorDistance += this.NewModelSize.Depth * 0.5; break;
                    case ImportClassType.LargeShip: vectorDistance += this.NewModelSize.Depth * 2.5; break;
                    case ImportClassType.Station: vectorDistance += this.NewModelSize.Depth * 2.5; break;
                }

                // Figure out where the Character is facing, and plant the new constrcut right in front, by "10" units, facing the Character.
                var vector = new BindableVector3DModel(this.dataModel.CharacterPosition.Forward).Vector3D;
                vector.Normalize();
                vector = Vector3D.Multiply(vector, vectorDistance);
                this.Position = new BindablePoint3DModel(Point3D.Add(new BindablePoint3DModel(this.dataModel.CharacterPosition.Position).Point3D, vector));
                this.Forward = new BindableVector3DModel(this.dataModel.CharacterPosition.Forward);
                this.Up = new BindableVector3DModel(this.dataModel.CharacterPosition.Up);
            }
        }

        public MyObjectBuilder_EntityBase BuildEntity()
        {
            MyObjectBuilder_CubeGrid entity = new MyObjectBuilder_CubeGrid();
            entity.EntityId = SpaceEngineersAPI.GenerateEntityId();
            entity.PersistentFlags = MyPersistentEntityFlags2.CastShadows | MyPersistentEntityFlags2.InScene;

            entity.Skeleton = new System.Collections.Generic.List<BoneInfo>();
            entity.LinearVelocity = new VRageMath.Vector3(0, 0, 0);
            entity.AngularVelocity = new VRageMath.Vector3(0, 0, 0);

            //double scaleFactor = 2.5;

            string blockPrefix = "";
            switch (this.ClassType)
            {
                case ImportClassType.SmallShip:
                    entity.GridSizeEnum = MyCubeSize.Small;
                    blockPrefix += "Small";
                    entity.IsStatic = false;
                    break;

                case ImportClassType.LargeShip:
                    entity.GridSizeEnum = MyCubeSize.Large;
                    blockPrefix += "Large";
                    entity.IsStatic = false;
                    break;

                case ImportClassType.Station:
                    entity.GridSizeEnum = MyCubeSize.Large;
                    blockPrefix += "Large";
                    entity.IsStatic = true;
                    this.Position = this.Position.RoundOff();
                    this.Forward = this.Forward.RoundToAxis();
                    this.Up = this.Up.RoundToAxis();
                    break;
            }

            switch (this.ArmorType)
            {
                case ImportArmorType.Heavy: blockPrefix += "HeavyBlock"; break;
                case ImportArmorType.Light: blockPrefix += "Block"; break;
            }

            entity.PositionAndOrientation = new MyPositionAndOrientation()
            {
                // TODO: reposition based scale.
                Position = this.Position.ToVector3(),
                Forward = this.Forward.ToVector3(),
                Up = this.Up.ToVector3()
            };

            // Large|Block|ArmorCorner
            // Large|HeavyBlock|ArmorBlock,
            // Small|Block|ArmorSlope,
            // Small|HeavyBlock|ArmorCorner,

            SubtypeId blockType = (SubtypeId)Enum.Parse(typeof(SubtypeId), blockPrefix + "ArmorBlock");
            SubtypeId slopeBlockType = (SubtypeId)Enum.Parse(typeof(SubtypeId), blockPrefix + "ArmorSlope");
            SubtypeId cornerBlockType = (SubtypeId)Enum.Parse(typeof(SubtypeId), blockPrefix + "ArmorCorner");

            CubeType[, ,] ccubic;
            entity.CubeBlocks = new System.Collections.Generic.List<MyObjectBuilder_CubeBlock>();

            var fixScale = 0;
            if (this.IsMultipleScale && this.MultipleScale == 1)
            {
                fixScale = 0;
            }
            else
            {
                fixScale = Math.Max(Math.Max(this.NewModelSize.Height, this.NewModelSize.Width), this.NewModelSize.Depth);
            }

            // TODO: fillobject.
            // TODO: smoothing.

            var fillObject = false;
            var smoothObject = true;

            var voxFilename = ToolboxExtensions.ConvertPolyToVox(this.Filename, fixScale, false);

            #region Read in voxel and set main cube space.

            using (BinaryReader reader = new BinaryReader(File.Open(voxFilename, FileMode.Open)))
            {
                // switch the Z and Y axis about, and reverse the Y axis to get the dimension in the right order from the Vox file.

                int xCount = reader.ReadInt32();
                int zCount = reader.ReadInt32();
                int yCount = reader.ReadInt32();
                ccubic = new CubeType[xCount, yCount, zCount];

                for (int x = 0; x < xCount; x++)
                {
                    for (int z = 0; z < zCount; z++)
                    {
                        for (int y = yCount - 1; y >= 0; y--)
                        {
                            var b = reader.ReadByte();

                            switch (b)
                            {
                                case 0x00: // hollow interior
                                    if (fillObject)
                                    {
                                        ccubic[x, y, z] = CubeType.Cube;
                                    }
                                    else
                                    {
                                        ccubic[x, y, z] = CubeType.Interior;
                                    }
                                    break;

                                case 0xFF: // space
                                    ccubic[x, y, z] = CubeType.None;
                                    break;

                                case 0x12: // solid
                                default:
                                    ccubic[x, y, z] = CubeType.Cube;
                                    break;
                            }
                        }
                    }
                }
            }

            File.Delete(voxFilename);

            #endregion


            /*
                     (top)+z|   /-y(back)  
                            |  /      
                            | /       
               -x(left)     |/       +x(right)
               -------------+-----------------
                           /|
                          / |
                         /  |
               (front)+y/   |-z(bottom)
             */

            if (smoothObject)
            {
                CalculateSlopes(ccubic);
                CalculateCorners(ccubic);
                CalculateInverseCorners(ccubic);
            }

            BuildStructureFromCubic(entity, ccubic, blockType, slopeBlockType, cornerBlockType);

            return entity;
        }

        #endregion

        #region CalculateSlopes

        private static void CalculateSlopes(CubeType[, ,] ccubic)
        {
            var xCount = ccubic.GetLength(0);
            var yCount = ccubic.GetLength(1);
            var zCount = ccubic.GetLength(2);

            for (int x = 0; x < xCount; x++)
            {
                for (int y = 0; y < yCount; y++)
                {
                    for (int z = 0; z < zCount; z++)
                    {
                        if (ccubic[x, y, z] == CubeType.None)
                        {
                            if (CheckAdjacentCubic(ccubic, x, y, z, xCount, yCount, zCount, 0, 1, 1))
                            {
                                ccubic[x, y, z] = CubeType.SlopeCenterFrontTop;
                            }
                            else if (CheckAdjacentCubic(ccubic, x, y, z, xCount, yCount, zCount, -1, 1, 0))
                            {
                                ccubic[x, y, z] = CubeType.SlopeLeftFrontCenter;
                            }
                            else if (CheckAdjacentCubic(ccubic, x, y, z, xCount, yCount, zCount, 1, 1, 0))
                            {
                                ccubic[x, y, z] = CubeType.SlopeRightFrontCenter;
                            }
                            else if (CheckAdjacentCubic(ccubic, x, y, z, xCount, yCount, zCount, 0, 1, -1))
                            {
                                ccubic[x, y, z] = CubeType.SlopeCenterFrontBottom;
                            }
                            else if (CheckAdjacentCubic(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 1))
                            {
                                ccubic[x, y, z] = CubeType.SlopeLeftCenterTop;
                            }
                            else if (CheckAdjacentCubic(ccubic, x, y, z, xCount, yCount, zCount, 1, 0, 1))
                            {
                                ccubic[x, y, z] = CubeType.SlopeRightCenterTop;
                            }
                            else if (CheckAdjacentCubic(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, -1))
                            {
                                ccubic[x, y, z] = CubeType.SlopeLeftCenterBottom;
                            }
                            else if (CheckAdjacentCubic(ccubic, x, y, z, xCount, yCount, zCount, 1, 0, -1))
                            {
                                ccubic[x, y, z] = CubeType.SlopeRightCenterBottom;
                            }
                            else if (CheckAdjacentCubic(ccubic, x, y, z, xCount, yCount, zCount, 0, -1, 1))
                            {
                                ccubic[x, y, z] = CubeType.SlopeCenterBackTop;
                            }
                            else if (CheckAdjacentCubic(ccubic, x, y, z, xCount, yCount, zCount, -1, -1, 0))
                            {
                                ccubic[x, y, z] = CubeType.SlopeLeftBackCenter;
                            }
                            else if (CheckAdjacentCubic(ccubic, x, y, z, xCount, yCount, zCount, 1, -1, 0))
                            {
                                ccubic[x, y, z] = CubeType.SlopeRightBackCenter;
                            }
                            else if (CheckAdjacentCubic(ccubic, x, y, z, xCount, yCount, zCount, 0, -1, -1))
                            {
                                ccubic[x, y, z] = CubeType.SlopeCenterBackBottom;
                            }
                        }
                    }
                }
            }

        }

        #endregion

        #region CalculateCorners

        private static void CalculateCorners(CubeType[, ,] ccubic)
        {
            var xCount = ccubic.GetLength(0);
            var yCount = ccubic.GetLength(1);
            var zCount = ccubic.GetLength(2);

            for (int x = 0; x < xCount; x++)
            {
                for (int y = 0; y < yCount; y++)
                {
                    for (int z = 0; z < zCount; z++)
                    {
                        if (ccubic[x, y, z] == CubeType.None)
                        {
                            // Red 1,3,5 CubeType.SlopeCenterFrontTop
                            // Yellow 0,3,6 CubeType.SlopeLeftFrontCenter
                            // Blue 0,3,5 CubeType.CornerLeftFrontTop;

                            if ((CheckAdjacentSlope(ccubic, x, y, z, xCount, yCount, zCount, 0, 0, +1, CubeType.SlopeLeftFrontCenter, -1, 0, 0, CubeType.SlopeCenterFrontTop)) ||
                                (CheckAdjacentSlope(ccubic, x, y, z, xCount, yCount, zCount, 0, 0, +1, CubeType.SlopeLeftFrontCenter, 0, -1, 0, CubeType.SlopeLeftCenterTop)) ||
                                (CheckAdjacentSlope(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.SlopeCenterFrontTop, 0, -1, 0, CubeType.SlopeLeftCenterTop)))
                            {
                                ccubic[x, y, z] = CubeType.CornerLeftBackTop;
                            }
                            else if ((CheckAdjacentSlope(ccubic, x, y, z, xCount, yCount, zCount, 0, 0, +1, CubeType.SlopeRightFrontCenter, +1, 0, 0, CubeType.SlopeCenterFrontTop)) ||
                                (CheckAdjacentSlope(ccubic, x, y, z, xCount, yCount, zCount, 0, 0, +1, CubeType.SlopeRightFrontCenter, 0, -1, 0, CubeType.SlopeRightCenterTop)) ||
                                (CheckAdjacentSlope(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.SlopeCenterFrontTop, 0, -1, 0, CubeType.SlopeLeftCenterTop)))
                            {
                                ccubic[x, y, z] = CubeType.CornerRightFrontTop;
                            }
                            else if ((CheckAdjacentSlope(ccubic, x, y, z, xCount, yCount, zCount, 0, 0, -1, CubeType.SlopeLeftFrontCenter, -1, 0, 0, CubeType.SlopeCenterFrontBottom)) ||
                               (CheckAdjacentSlope(ccubic, x, y, z, xCount, yCount, zCount, 0, 0, -1, CubeType.SlopeLeftFrontCenter, 0, -1, 0, CubeType.SlopeLeftCenterBottom)) ||
                               (CheckAdjacentSlope(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.SlopeCenterFrontTop, 0, -1, 0, CubeType.SlopeLeftCenterBottom)))
                            {
                                ccubic[x, y, z] = CubeType.CornerLeftFrontBottom;
                            }
                            else if ((CheckAdjacentSlope(ccubic, x, y, z, xCount, yCount, zCount, 0, 0, -1, CubeType.SlopeRightFrontCenter, +1, 0, 0, CubeType.SlopeCenterFrontBottom)) ||
                                (CheckAdjacentSlope(ccubic, x, y, z, xCount, yCount, zCount, 0, 0, -1, CubeType.SlopeRightFrontCenter, 0, -1, 0, CubeType.SlopeRightCenterBottom)) ||
                                (CheckAdjacentSlope(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.SlopeCenterFrontBottom, 0, -1, 0, CubeType.SlopeLeftCenterBottom)))
                            {
                                ccubic[x, y, z] = CubeType.CornerRightFrontBottom;
                            }
                            else if ((CheckAdjacentSlope(ccubic, x, y, z, xCount, yCount, zCount, 0, 0, +1, CubeType.SlopeLeftBackCenter, -1, 0, 0, CubeType.SlopeCenterBackTop)) ||
                                (CheckAdjacentSlope(ccubic, x, y, z, xCount, yCount, zCount, 0, 0, +1, CubeType.SlopeLeftBackCenter, 0, +1, 0, CubeType.SlopeLeftCenterTop)) ||
                                (CheckAdjacentSlope(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.SlopeCenterBackTop, 0, +1, 0, CubeType.SlopeLeftCenterTop)))
                            {
                                ccubic[x, y, z] = CubeType.CornerLeftFrontTop;
                            }
                            else if ((CheckAdjacentSlope(ccubic, x, y, z, xCount, yCount, zCount, 0, 0, +1, CubeType.SlopeRightBackCenter, +1, 0, 0, CubeType.SlopeCenterBackTop)) ||
                                (CheckAdjacentSlope(ccubic, x, y, z, xCount, yCount, zCount, 0, 0, +1, CubeType.SlopeRightBackCenter, 0, +1, 0, CubeType.SlopeRightCenterTop)) ||
                                (CheckAdjacentSlope(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.SlopeCenterBackTop, 0, +1, 0, CubeType.SlopeLeftCenterTop)))
                            {
                                ccubic[x, y, z] = CubeType.CornerRightBackTop;
                            }
                            else if ((CheckAdjacentSlope(ccubic, x, y, z, xCount, yCount, zCount, 0, 0, -1, CubeType.SlopeLeftBackCenter, -1, 0, 0, CubeType.SlopeCenterBackBottom)) ||
                               (CheckAdjacentSlope(ccubic, x, y, z, xCount, yCount, zCount, 0, 0, -1, CubeType.SlopeLeftBackCenter, 0, +1, 0, CubeType.SlopeLeftCenterBottom)) ||
                               (CheckAdjacentSlope(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.SlopeCenterBackTop, 0, +1, 0, CubeType.SlopeLeftCenterBottom)))
                            {
                                ccubic[x, y, z] = CubeType.CornerLeftBackBottom;
                            }
                            else if ((CheckAdjacentSlope(ccubic, x, y, z, xCount, yCount, zCount, 0, 0, -1, CubeType.SlopeRightBackCenter, +1, 0, 0, CubeType.SlopeCenterBackBottom)) ||
                                (CheckAdjacentSlope(ccubic, x, y, z, xCount, yCount, zCount, 0, 0, -1, CubeType.SlopeRightBackCenter, 0, +1, 0, CubeType.SlopeRightCenterBottom)) ||
                                (CheckAdjacentSlope(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.SlopeCenterBackBottom, 0, +1, 0, CubeType.SlopeLeftCenterBottom)))
                            {
                                ccubic[x, y, z] = CubeType.CornerRightBackBottom;
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region CalculateInverseCorners

        private static void CalculateInverseCorners(CubeType[, ,] ccubic)
        {
            var xCount = ccubic.GetLength(0);
            var yCount = ccubic.GetLength(1);
            var zCount = ccubic.GetLength(2);

            for (int x = 0; x < xCount; x++)
            {
                for (int y = 0; y < yCount; y++)
                {
                    for (int z = 0; z < zCount; z++)
                    {
                        // TODO: Inverse Corner
                    }
                }
            }
        }

        #endregion

        #region CheckAdjacentCubic

        private static bool IsValidRange(int x, int y, int z, int xCount, int yCount, int zCount, int xDelta, int yDelta, int zDelta)
        {
            if (x + xDelta >= 0 && x + xDelta < xCount
            && y + yDelta >= 0 && y + yDelta < yCount
            && z + zDelta >= 0 && z + zDelta < zCount)
            {
                return true;
            }

            return false;
        }

        private static bool CheckAdjacentCubic(CubeType[, ,] ccubic, int x, int y, int z, int xCount, int yCount, int zCount, int xDelta, int yDelta, int zDelta)
        {
            if (ccubic[x, y, z] == CubeType.None && IsValidRange(x, y, z, xCount, yCount, zCount, xDelta, yDelta, zDelta))
            {
                if (xDelta != 0 && ccubic[x + xDelta, y, z] == CubeType.Cube &&
                    yDelta != 0 && ccubic[x, y + yDelta, z] == CubeType.Cube &&
                    zDelta == 0)
                {
                    return true;
                }

                if (xDelta != 0 && ccubic[x + xDelta, y, z] == CubeType.Cube &&
                    yDelta == 0 &&
                    zDelta != 0 && ccubic[x, y, z + zDelta] == CubeType.Cube)
                {
                    return true;
                }

                if (xDelta == 0 &&
                    yDelta != 0 && ccubic[x, y + yDelta, z] == CubeType.Cube &&
                    zDelta != 0 && ccubic[x, y, z + zDelta] == CubeType.Cube)
                {
                    return true;
                }

                if (xDelta != 0 && ccubic[x + xDelta, y, z] == CubeType.Cube &&
                    yDelta != 0 && ccubic[x, y + yDelta, z] == CubeType.Cube &&
                    zDelta != 0 && ccubic[x, y, z + zDelta] == CubeType.Cube)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool CheckAdjacentSlope(CubeType[, ,] ccubic, int x, int y, int z, int xCount, int yCount, int zCount,
            int xDelta1, int yDelta1, int zDelta1, CubeType slopeType1,
            int xDelta2, int yDelta2, int zDelta2, CubeType slopeType2)
        {
            if (IsValidRange(x, y, z, xCount, yCount, zCount, xDelta1, yDelta1, zDelta1) && IsValidRange(x, y, z, xCount, yCount, zCount, xDelta2, yDelta2, zDelta2))
            {
                return ccubic[x + xDelta1, y + yDelta1, z + zDelta1] == slopeType1 && ccubic[x + xDelta2, y + yDelta2, z + zDelta2] == slopeType2;
            }

            return false;
        }

        #endregion

        #region BuildStructureFromCubic

        private static void BuildStructureFromCubic(MyObjectBuilder_CubeGrid entity, CubeType[, ,] ccubic, SubtypeId blockType, SubtypeId slopeBlockType, SubtypeId cornerBlockType)
        {
            var xCount = ccubic.GetLength(0);
            var yCount = ccubic.GetLength(1);
            var zCount = ccubic.GetLength(2);
            MyObjectBuilder_CubeBlock newCube;

            for (int x = 0; x < xCount; x++)
            {
                for (int y = 0; y < yCount; y++)
                {
                    for (int z = 0; z < zCount; z++)
                    {
                        if (ccubic[x, y, z] != CubeType.None && ccubic[x, y, z] != CubeType.Interior)
                        {
                            entity.CubeBlocks.Add(newCube = new MyObjectBuilder_CubeBlock());

                            if (ccubic[x, y, z].ToString().StartsWith("Cube"))
                            {
                                newCube.SubtypeName = blockType.ToString();
                            }
                            else if (ccubic[x, y, z].ToString().StartsWith("Slope"))
                            {
                                newCube.SubtypeName = slopeBlockType.ToString();
                            }
                            else if (ccubic[x, y, z].ToString().StartsWith("Corner"))
                            {
                                newCube.SubtypeName = cornerBlockType.ToString();
                            }

                            newCube.EntityId = 0;
                            newCube.PersistentFlags = MyPersistentEntityFlags2.None;
                            SpaceEngineersAPI.SetCubeOrientation(newCube, ccubic[x, y, z]);
                            newCube.Min = new VRageMath.Vector3I(x, y, z);
                            newCube.Max = new VRageMath.Vector3I(x, y, z);
                        }
                    }
                }
            }
        } 

        #endregion
    }
}
