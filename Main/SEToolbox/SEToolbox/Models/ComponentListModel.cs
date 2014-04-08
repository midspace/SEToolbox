namespace SEToolbox.Models
{
    using SEToolbox.Interop;
    using SEToolbox.Support;
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;

    public class ComponentListModel : BaseModel
    {
        #region Fields

        private ObservableCollection<ComonentItemModel> _cubeAssets;

        private ObservableCollection<ComonentItemModel> _componentAssets;

        private ObservableCollection<ComonentItemModel> _itemAssets;

        private ObservableCollection<ComonentItemModel> _materialAssets;

        private bool _isBusy;

        #endregion

        #region ctor

        public ComponentListModel()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// This is detail of the breakdown of cubes in the ship.
        /// </summary>
        public ObservableCollection<ComonentItemModel> CubeAssets
        {
            get
            {
                return this._cubeAssets;
            }

            set
            {
                if (value != this._cubeAssets)
                {
                    this._cubeAssets = value;
                    this.RaisePropertyChanged(() => CubeAssets);
                }
            }
        }

        /// <summary>
        /// This is detail of the breakdown of components in the ship.
        /// </summary>
        public ObservableCollection<ComonentItemModel> ComponentAssets
        {
            get
            {
                return this._componentAssets;
            }

            set
            {
                if (value != this._componentAssets)
                {
                    this._componentAssets = value;
                    this.RaisePropertyChanged(() => ComponentAssets);
                }
            }
        }

        /// <summary>
        /// This is detail of the breakdown of ingots in the ship.
        /// </summary>
        public ObservableCollection<ComonentItemModel> ItemAssets
        {
            get
            {
                return this._itemAssets;
            }

            set
            {
                if (value != this._itemAssets)
                {
                    this._itemAssets = value;
                    this.RaisePropertyChanged(() => ItemAssets);
                }
            }
        }

        /// <summary>
        /// This is detail of the breakdown of ore in the ship.
        /// </summary>
        public ObservableCollection<ComonentItemModel> MaterialAssets
        {
            get
            {
                return this._materialAssets;
            }

            set
            {
                if (value != this._materialAssets)
                {
                    this._materialAssets = value;
                    this.RaisePropertyChanged(() => MaterialAssets);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the View is currently in the middle of an asynchonise operation.
        /// </summary>
        public bool IsBusy
        {
            get
            {
                return this._isBusy;
            }

            set
            {
                if (value != this._isBusy)
                {
                    this._isBusy = value;
                    this.RaisePropertyChanged(() => IsBusy);
                    if (this._isBusy)
                    {
                        System.Windows.Forms.Application.DoEvents();
                    }
                }
            }
        }

        #endregion

        #region methods

        public void Load()
        {
            this.CubeAssets = new ObservableCollection<ComonentItemModel>();
            this.ComponentAssets = new ObservableCollection<ComonentItemModel>();
            this.ItemAssets = new ObservableCollection<ComonentItemModel>();
            this.MaterialAssets = new ObservableCollection<ComonentItemModel>();

            var contentPath = Path.Combine(ToolboxUpdater.GetApplicationFilePath(), "Content");

            foreach (var cubeDefinition in SpaceEngineersAPI.CubeBlockDefinitions)
            {
                //var cd = SpaceEngineersAPI.GetDefinition(component.Type, component.Subtype) as MyObjectBuilder_ComponentDefinition;

                var menuPosition = SpaceEngineersAPI.CubeBlockPositions.FirstOrDefault(p => p.Name == cubeDefinition.BlockPairName);
                if (menuPosition != null && (menuPosition.Position.X == -1 || menuPosition.Position.Y == -1))
                    menuPosition = null;

                this.CubeAssets.Add(new ComonentItemModel()
                {
                    Name = cubeDefinition.DisplayName,
                    TypeId = cubeDefinition.Id.TypeId.ToString(),
                    SubtypeId = cubeDefinition.Id.SubtypeId,
                    TextureFile = Path.Combine(contentPath, cubeDefinition.Icon + ".dds"),
                    Time = new TimeSpan((long)(TimeSpan.TicksPerSecond * cubeDefinition.BuildTimeSeconds)),
                    Accessible = cubeDefinition.Public,
                    Mass = SpaceEngineersAPI.FetchCubeBlockMass(cubeDefinition.Id.TypeId, cubeDefinition.CubeSize, cubeDefinition.Id.SubtypeId),
                    CubeSize = cubeDefinition.CubeSize,
                    Size = new BindableSize3DIModel(cubeDefinition.Size),
                });
            }

            foreach (var componentDefinition in SpaceEngineersAPI.ComponentDefinitions)
            {
                var bp = SpaceEngineersAPI.BlueprintDefinitions.FirstOrDefault(b => b.Result.SubtypeId == componentDefinition.Id.SubtypeId && b.Result.TypeId == componentDefinition.Id.TypeId);

                this.ComponentAssets.Add(new ComonentItemModel()
                {
                    Name = componentDefinition.DisplayName,
                    TypeId = componentDefinition.Id.TypeId.ToString(),
                    SubtypeId = componentDefinition.Id.SubtypeId,
                    Mass = componentDefinition.Mass,
                    TextureFile = Path.Combine(contentPath, componentDefinition.Icon + ".dds"),
                    Volume = componentDefinition.Volume.HasValue ? componentDefinition.Volume.Value : 0f,
                    Accessible = componentDefinition.Public,
                    Time = bp != null ? new TimeSpan((long)(TimeSpan.TicksPerSecond * bp.BaseProductionTimeInSeconds)) : (TimeSpan?)null,
                });
            }

            foreach (var physicalItemDefinition in SpaceEngineersAPI.PhysicalItemDefinitions)
            {
                var bp = SpaceEngineersAPI.BlueprintDefinitions.FirstOrDefault(b => b.Result.SubtypeId == physicalItemDefinition.Id.SubtypeId && b.Result.TypeId == physicalItemDefinition.Id.TypeId);
                this.ItemAssets.Add(new ComonentItemModel()
                {
                    Name = physicalItemDefinition.DisplayName,
                    TypeId = physicalItemDefinition.Id.TypeId.ToString(),
                    SubtypeId = physicalItemDefinition.Id.SubtypeId,
                    Mass = physicalItemDefinition.Mass,
                    Volume = physicalItemDefinition.Volume.HasValue ? physicalItemDefinition.Volume.Value : 0f,
                    TextureFile = Path.Combine(contentPath, physicalItemDefinition.Icon + ".dds"),
                    Accessible = physicalItemDefinition.Public,
                    Time = bp != null ? new TimeSpan((long)(TimeSpan.TicksPerSecond * bp.BaseProductionTimeInSeconds)) : (TimeSpan?)null,
                });
            }

            foreach (var physicalItemDefinition in SpaceEngineersAPI.AmmoMagazineDefinitions)
            {
                var bp = SpaceEngineersAPI.BlueprintDefinitions.FirstOrDefault(b => b.Result.SubtypeId == physicalItemDefinition.Id.SubtypeId && b.Result.TypeId == physicalItemDefinition.Id.TypeId);
                this.ItemAssets.Add(new ComonentItemModel()
                {
                    Name = physicalItemDefinition.DisplayName,
                    TypeId = physicalItemDefinition.Id.TypeId.ToString(),
                    SubtypeId = physicalItemDefinition.Id.SubtypeId,
                    Mass = physicalItemDefinition.Mass,
                    Volume = physicalItemDefinition.Volume.HasValue ? physicalItemDefinition.Volume.Value : 0f,
                    TextureFile = Path.Combine(contentPath, physicalItemDefinition.Icon + ".dds"),
                    Accessible = !string.IsNullOrEmpty(physicalItemDefinition.Model),
                    Time = bp != null ? new TimeSpan((long)(TimeSpan.TicksPerSecond * bp.BaseProductionTimeInSeconds)) : (TimeSpan?)null,
                });
            }

            foreach (var voxelMaterialDefinition in SpaceEngineersAPI.VoxelMaterialDefinitions)
            {
                this.MaterialAssets.Add(new ComonentItemModel()
                {
                    Name = voxelMaterialDefinition.AssetName,
                    TextureFile = Path.Combine(contentPath, @"Textures\Voxels\" + voxelMaterialDefinition.AssetName + "_ForAxisXZ_de.dds"),
                    IsRare = voxelMaterialDefinition.IsRare,
                    OreName = voxelMaterialDefinition.MinedOre,
                    MineOreRatio = voxelMaterialDefinition.MinedOreRatio,
                });
            }
        }

        #endregion
    }
}
