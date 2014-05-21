namespace SEToolbox.Models
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Common.ObjectBuilders.Definitions;
    using Sandbox.Common.ObjectBuilders.Voxels;
    using SEToolbox.Interfaces;
    using SEToolbox.Interop;
    using SEToolbox.Interop.Asteroids;
    using SEToolbox.Support;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class ResourceReportModel : BaseModel
    {
        #region Fields

        private bool _isBusy;
        private bool _isActive;
        private string _reportText;
        private double _progress;
        private IList<IStructureBase> _entities;

        /// <summary>
        /// untouched (in asteroid), measured in m³
        /// </summary>
        private List<VoxelMaterialAssetModel> _untouchedOre;

        /// <summary>
        /// unused (ore and ingot), measured in Kg and L.
        /// </summary>
        private List<OreAssetModel> _unusedOre;

        /// <summary>
        /// used (component, tool, cube), measured in Kg and L.
        /// </summary>
        private List<OreAssetModel> _usedOre;

        // npc (currently AI cargo ships with ore, ingot, component, tool, cube), measured in Kg and L.
        // numbers of cubes, components, tools, etc could be included, as they technically indicate time spent to construct.

        #endregion

        #region ctor

        public ResourceReportModel()
        {
        }

        #endregion

        #region Properties

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
                    this.SetActiveStatus();
                    if (this._isBusy)
                    {
                        System.Windows.Forms.Application.DoEvents();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the View is available.  This is based on the IsInError and IsBusy properties
        /// </summary>
        public bool IsActive
        {
            get
            {
                return this._isActive;
            }

            set
            {
                if (value != this._isActive)
                {
                    this._isActive = value;
                    this.RaisePropertyChanged(() => IsActive);
                }
            }
        }

        public string ReportText
        {
            get
            {
                return this._reportText;
            }

            set
            {
                if (value != this._reportText)
                {
                    this._reportText = value;
                    this.RaisePropertyChanged(() => ReportText);
                }
            }
        }

        public double Progress
        {
            get
            {
                return this._progress;
            }

            set
            {
                if (value != this._progress)
                {
                    this._progress = value;
                    this.RaisePropertyChanged(() => Progress);
                }
            }
        }

        #endregion

        #region methods

        public void Load(IList<IStructureBase> entities)
        {
            this._entities = entities;
            this.SetActiveStatus();
        }

        //public void Load(List<MyObjectBuilder_EntityBase> entities)
        //{
        //    this._entities = entities;
        //    this.SetActiveStatus();
        //}

        public void SetActiveStatus()
        {
            this.IsActive = !this.IsBusy;
        }

        public void GenerateReport()
        {
            var contentPath = Path.Combine(ToolboxUpdater.GetApplicationFilePath(), "Content");

            #region untouched materials (in asteroid)

            var accumulateMaterials = new Dictionary<string, long>();

            foreach (var entity in _entities.OfType<StructureVoxelModel>())
            {
                Dictionary<string, long> details = null;
                var asteroid = entity as StructureVoxelModel;

                var filename = asteroid.SourceVoxelFilepath;
                if (string.IsNullOrEmpty(filename))
                    filename = asteroid.VoxelFilepath;

                try
                {
                    details = MyVoxelMap.GetMaterialAssetDetails(filename);
                }
                catch { }

                // Accumulate into untouched.
                if (details != null)
                {
                    foreach (var kvp in details)
                    {
                        if (accumulateMaterials.ContainsKey(kvp.Key))
                        {
                            accumulateMaterials[kvp.Key] += kvp.Value;
                        }
                        else
                        {
                            accumulateMaterials.Add(kvp.Key, kvp.Value);
                        }
                    }
                }
            }

            var sum = accumulateMaterials.Values.ToList().Sum();
            _untouchedOre = new List<VoxelMaterialAssetModel>();

            foreach (var kvp in accumulateMaterials)
            {
                _untouchedOre.Add(new VoxelMaterialAssetModel() { MaterialName = kvp.Key, Volume = Math.Round((double)kvp.Value / 255, 7), Percent = (double)kvp.Value / (double)sum });
            }

            #endregion

            #region unused (ore and ingot)

            var accumulateOres = new Dictionary<string, OreAssetModel>();

            #region Floating ore

            foreach (var entity in _entities.OfType<StructureFloatingObjectModel>())
            {
                var floating = entity as StructureFloatingObjectModel;

                if (floating.FloatingObject.Item.Content.TypeId == MyObjectBuilderTypeEnum.Ore)
                {
                    var cd = SpaceEngineersAPI.GetDefinition(floating.FloatingObject.Item.Content.TypeId, floating.FloatingObject.Item.Content.SubtypeName) as MyObjectBuilder_PhysicalItemDefinition;
                    var componentTexture = Path.Combine(contentPath, cd.Icon + ".dds");
                    var oreAsset = new OreAssetModel() { Name = cd.DisplayName, Amount = floating.FloatingObject.Item.AmountDecimal, Mass = Math.Round((double)floating.FloatingObject.Item.Amount * cd.Mass, 7), Volume = Math.Round((double)floating.FloatingObject.Item.Amount * cd.Volume.Value, 7), TextureFile = componentTexture };

                    if (accumulateOres.ContainsKey(oreAsset.Name))
                    {
                        accumulateOres[oreAsset.Name].Amount += oreAsset.Amount;
                        accumulateOres[oreAsset.Name].Mass += oreAsset.Mass;
                        accumulateOres[oreAsset.Name].Volume += oreAsset.Volume;
                    }
                    else
                    {
                        accumulateOres.Add(oreAsset.Name, oreAsset);
                    }
                }
                else if (floating.FloatingObject.Item.Content.TypeId == MyObjectBuilderTypeEnum.Ingot)
                {
                    var cd = SpaceEngineersAPI.GetDefinition(floating.FloatingObject.Item.Content.TypeId, floating.FloatingObject.Item.Content.SubtypeName) as MyObjectBuilder_PhysicalItemDefinition;
                    var bp = SpaceEngineersAPI.BlueprintDefinitions.FirstOrDefault(b => b.Result.SubtypeId == floating.FloatingObject.Item.Content.SubtypeName && b.Result.TypeId == floating.FloatingObject.Item.Content.TypeId);
                    var componentTexture = Path.Combine(contentPath, cd.Icon + ".dds");
                    TimeSpan ingotTime = new TimeSpan((long)(TimeSpan.TicksPerSecond * (decimal)bp.BaseProductionTimeInSeconds * floating.FloatingObject.Item.AmountDecimal));
                    var ingotAsset = new OreAssetModel() { Name = cd.DisplayName, Amount = floating.FloatingObject.Item.AmountDecimal, Mass = Math.Round((double)floating.FloatingObject.Item.Amount * cd.Mass, 7), Volume = Math.Round((double)floating.FloatingObject.Item.Amount * cd.Volume.Value, 7), Time = ingotTime, TextureFile = componentTexture };

                    if (accumulateOres.ContainsKey(ingotAsset.Name))
                    {
                        accumulateOres[ingotAsset.Name].Amount += ingotAsset.Amount;
                        accumulateOres[ingotAsset.Name].Mass += ingotAsset.Mass;
                        accumulateOres[ingotAsset.Name].Volume += ingotAsset.Volume;
                        accumulateOres[ingotAsset.Name].Time += ingotAsset.Time;
                    }
                    else
                    {
                        accumulateOres.Add(ingotAsset.Name, ingotAsset);
                    }
                }
            }

            #endregion

            #region Cargo ore
            
            // TODO:

            #endregion

            _unusedOre = accumulateOres.Values.ToList();

            #endregion

            #region MyRegion

            // used (component, tool, cube), measured in Kg and L.
            
            #endregion

            // npc (currently AI cargo ships with ore, ingot, component, tool, cube), measured in Kg and L.


            // In game Assets.
            // numbers of cubes, components, tools, ingots, etc could be included, as they technically indicate time spent to construct or refine.


            #region create report

            var bld = new StringBuilder();

            bld.AppendFormat("Untouched Ore\r\n");
            bld.AppendFormat("Name\tVolume m³\r\n");
            foreach (var item in _untouchedOre)
            {
                bld.AppendFormat("{0}\t{1:#,##0.000}\r\n", item.MaterialName, item.Volume);
            }

            bld.AppendLine();
            bld.AppendFormat("Unused Ore\r\n");
            bld.AppendFormat("Name\tMass Kg\tVolume L\tTime\r\n");
            foreach (var item in _unusedOre)
            {
                bld.AppendFormat("{0}\t{1:#,##0.000}\t{2:#,##0.000}\t{3}\r\n", item.FriendlyName, item.Mass, item.Volume, item.Time);
            }

            this.ReportText = bld.ToString();

            #endregion

        }

        #endregion
    }
}
