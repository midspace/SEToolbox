namespace SEToolbox.Models
{
    using System.Diagnostics;
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
        private readonly Stopwatch _timer;
        private bool _showProgress;
        private double _progress;
        private double _maximumProgress;
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
        /// Everything is measured in it's regressed state. Ie., how much ore was used/needed to build this item.
        /// </summary>
        private List<OreAssetModel> _usedOre;

        /// <summary>
        /// npc (everything currently in an AI controlled ship with ore, ingot, component, tool, cube), measured in Kg and L.
        /// </summary>
        private List<OreAssetModel> _npcOre;

        /// <summary>
        /// tally of cubes to indicate time spent to construct.
        /// </summary>
        private List<ComponentItemModel> _allCubes;

        /// <summary>
        /// tally of components to indicate time spent to construct.
        /// </summary>
        private List<ComponentItemModel> _allComponents;

        /// <summary>
        /// tally of items, ingots, tools, ores, to indicate time spent to construct or mine.
        /// </summary>
        private List<ComponentItemModel> _allItems;

        #endregion

        #region ctor

        public ResourceReportModel()
        {
            this._timer = new Stopwatch();
            this.Progress = 0;
            this.MaximumProgress = 100;
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

        public bool ShowProgress
        {
            get
            {
                return this._showProgress;
            }

            set
            {
                if (value != this._showProgress)
                {
                    this._showProgress = value;
                    this.RaisePropertyChanged(() => ShowProgress);
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

                    if (!_timer.IsRunning || _timer.ElapsedMilliseconds > 100)
                    {
                        this.RaisePropertyChanged(() => Progress);
                        System.Windows.Forms.Application.DoEvents();
                        _timer.Restart();
                    }
                }
            }
        }

        public double MaximumProgress
        {
            get
            {
                return this._maximumProgress;
            }

            set
            {
                if (value != this._maximumProgress)
                {
                    this._maximumProgress = value;
                    this.RaisePropertyChanged(() => MaximumProgress);
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

        public void ResetProgress(double initial, double maximumProgress)
        {
            this.MaximumProgress = maximumProgress;
            this.Progress = initial;
            this.ShowProgress = true;
            _timer.Restart();
            System.Windows.Forms.Application.DoEvents();
        }

        public void IncrementProgress()
        {
            this.Progress++;
        }

        public void ClearProgress()
        {
            _timer.Stop();
            this.ShowProgress = false;
            this.Progress = 0;
        }

        public void GenerateReport()
        {
            var contentPath = Path.Combine(ToolboxUpdater.GetApplicationFilePath(), "Content");
            var accumulateMaterials = new Dictionary<string, long>();
            var accumulateUnusedOres = new Dictionary<string, OreAssetModel>();
            var accumulateUsedOres = new Dictionary<string, OreAssetModel>();
            var accumulateNpcOres = new Dictionary<string, OreAssetModel>();
            var accumulateItems = new Dictionary<string, ComponentItemModel>();
            var accumulateComponents = new Dictionary<string, ComponentItemModel>();
            var accumulateCubes = new Dictionary<string, ComponentItemModel>();

            this.ResetProgress(0, _entities.Count);

            foreach (var entity in _entities)
            {
                this.Progress++;

                if (entity is StructureVoxelModel)
                {
                    var asteroid = entity as StructureVoxelModel;

                    #region untouched materials (in asteroid)

                    Dictionary<string, long> details = null;

                    var filename = asteroid.SourceVoxelFilepath;
                    if (string.IsNullOrEmpty(filename))
                        filename = asteroid.VoxelFilepath;

                    try
                    {
                        details = MyVoxelMap.GetMaterialAssetDetails(filename);
                    }
                    catch
                    {
                    }

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

                    #endregion
                }
                else if (entity is StructureFloatingObjectModel)
                {
                    var floating = entity as StructureFloatingObjectModel;
                    var cd = SpaceEngineersAPI.GetDefinition(floating.FloatingObject.Item.Content.TypeId, floating.FloatingObject.Item.Content.SubtypeName) as MyObjectBuilder_PhysicalItemDefinition;
                    var componentTexture = Path.Combine(contentPath, cd.Icon + ".dds");

                    if (floating.FloatingObject.Item.Content.TypeId == MyObjectBuilderTypeEnum.Ore)
                    {
                        var mass = Math.Round((double)floating.FloatingObject.Item.Amount * cd.Mass, 7);
                        var volume = Math.Round((double)floating.FloatingObject.Item.Amount * (cd.Volume.HasValue ? cd.Volume.Value : 0), 7);

                        #region unused floating (ore and ingot)

                        var unusedKey = floating.FloatingObject.Item.Content.SubtypeName;
                        if (accumulateUnusedOres.ContainsKey(unusedKey))
                        {
                            accumulateUnusedOres[unusedKey].Amount += floating.FloatingObject.Item.AmountDecimal;
                            accumulateUnusedOres[unusedKey].Mass += mass;
                            accumulateUnusedOres[unusedKey].Volume += volume;
                        }
                        else
                        {
                            accumulateUnusedOres.Add(unusedKey, new OreAssetModel() { Name = cd.DisplayName, Amount = floating.FloatingObject.Item.AmountDecimal, Mass = mass, Volume = volume, TextureFile = componentTexture });
                        }

                        #endregion

                        #region tally items

                        var itemsKey = cd.DisplayName;
                        if (accumulateItems.ContainsKey(itemsKey))
                        {
                            accumulateItems[itemsKey].Mass += mass;
                            accumulateItems[itemsKey].Volume += volume;
                        }
                        else
                        {
                            accumulateItems.Add(itemsKey, new ComponentItemModel() { Name = cd.DisplayName, Mass = mass, Volume = volume, TypeId = floating.FloatingObject.Item.Content.TypeId, SubtypeId = floating.FloatingObject.Item.Content.SubtypeName, TextureFile = componentTexture, Time = TimeSpan.Zero });
                        }

                        #endregion
                    }
                    else if (floating.FloatingObject.Item.Content.TypeId == MyObjectBuilderTypeEnum.Ingot)
                    {
                        var bp = SpaceEngineersAPI.BlueprintDefinitions.FirstOrDefault(b => b.Result.SubtypeId == floating.FloatingObject.Item.Content.SubtypeName && b.Result.TypeId == floating.FloatingObject.Item.Content.TypeId);
                        var mass = Math.Round((double)floating.FloatingObject.Item.Amount * cd.Mass, 7);
                        var volume = Math.Round((double)floating.FloatingObject.Item.Amount * (cd.Volume.HasValue ? cd.Volume.Value : 0), 7);
                        var ingotTime = new TimeSpan((long)(TimeSpan.TicksPerSecond * (decimal)bp.BaseProductionTimeInSeconds * floating.FloatingObject.Item.AmountDecimal));

                        #region unused floating (ore and ingot)

                        var unusedKey = floating.FloatingObject.Item.Content.SubtypeName;
                        if (accumulateUnusedOres.ContainsKey(unusedKey))
                        {
                            accumulateUnusedOres[unusedKey].Amount += floating.FloatingObject.Item.AmountDecimal;
                            accumulateUnusedOres[unusedKey].Mass += mass;
                            accumulateUnusedOres[unusedKey].Volume += volume;
                            accumulateUnusedOres[unusedKey].Time += ingotTime;
                        }
                        else
                        {
                            accumulateUnusedOres.Add(unusedKey, new OreAssetModel() { Name = cd.DisplayName, Amount = floating.FloatingObject.Item.AmountDecimal, Mass = mass, Volume = volume, Time = ingotTime, TextureFile = componentTexture });
                        }

                        #endregion

                        #region tally items

                        var itemsKey = cd.DisplayName;
                        if (accumulateItems.ContainsKey(itemsKey))
                        {
                            accumulateItems[itemsKey].Mass += mass;
                            accumulateItems[itemsKey].Volume += volume;
                            accumulateItems[itemsKey].Time += ingotTime;
                        }
                        else
                        {
                            accumulateItems.Add(itemsKey, new ComponentItemModel() { Name = cd.DisplayName, Mass = mass, Volume = volume, TypeId = floating.FloatingObject.Item.Content.TypeId, SubtypeId = floating.FloatingObject.Item.Content.SubtypeName, TextureFile = componentTexture, Time = ingotTime });
                        }

                        #endregion
                    }
                    else if (floating.FloatingObject.Item.Content.TypeId == MyObjectBuilderTypeEnum.Component)
                    {
                        // TODO:
                    }
                    else if (floating.FloatingObject.Item.Content.TypeId == MyObjectBuilderTypeEnum.AmmoMagazine)
                    {
                        // TODO:
                    }
                    else if (floating.FloatingObject.Item.Content.TypeId == MyObjectBuilderTypeEnum.PhysicalGunObject)
                    {
                        // TODO:
                    }
                    else if (floating.FloatingObject.Item.Content is MyObjectBuilder_EntityBase)
                    {
                        // TODO: missed a new object type?
                    }
                }
                else if (entity is StructureCharacterModel)
                {
                    // Player inventory.
                }
                else if (entity is StructureCubeGridModel)
                {
                    var ship = entity as StructureCubeGridModel;
                    var isNpc = ship.CubeGrid.CubeBlocks.Any<MyObjectBuilder_CubeBlock>(e => e is MyObjectBuilder_Cockpit && ((MyObjectBuilder_Cockpit)e).Autopilot != null);

                    #region unused cargo (ore and ingot)

                    #endregion

                    if (isNpc)
                    {
                        #region NPC ships

                        #endregion
                    }
                    else
                    {

                    }
                }
            }

            #region build lists

            var sum = accumulateMaterials.Values.ToList().Sum();
            _untouchedOre = new List<VoxelMaterialAssetModel>();

            foreach (var kvp in accumulateMaterials)
            {
                _untouchedOre.Add(new VoxelMaterialAssetModel() { MaterialName = kvp.Key, Volume = Math.Round((double)kvp.Value / 255, 7), Percent = (double)kvp.Value / (double)sum });
            }

            _unusedOre = accumulateUnusedOres.Values.ToList();
            _usedOre = accumulateUsedOres.Values.ToList();
            _npcOre = accumulateNpcOres.Values.ToList();
            _allCubes = accumulateCubes.Values.ToList();
            _allComponents = accumulateComponents.Values.ToList();
            _allItems = accumulateItems.Values.ToList();

            #endregion

            #region create report

            var bld = new StringBuilder();

            // Everything is measured in its regressed state. Ie., how much ore was used/needed to build this item.

            bld.AppendFormat("Untouched Ore\r\n");
            bld.AppendFormat("Name\tVolume m³\r\n");
            foreach (var item in _untouchedOre)
            {
                bld.AppendFormat("{0}\t{1:#,##0.000}\r\n", item.MaterialName, item.Volume);
            }

            bld.AppendLine();
            bld.AppendFormat("Unused Ore\r\n");
            bld.AppendFormat("Name\tMass Kg\tVolume L\r\n");
            foreach (var item in _unusedOre)
            {
                bld.AppendFormat("{0}\t{1:#,##0.000}\t{2:#,##0.000}\r\n", item.FriendlyName, item.Mass, item.Volume);
            }

            bld.AppendLine();
            bld.AppendFormat("Used Ore\r\n");
            bld.AppendFormat("Name\tMass Kg\tVolume L\r\n");
            foreach (var item in _usedOre)
            {
                bld.AppendFormat("{0}\t{1:#,##0.000}\t{2:#,##0.000}\r\n", item.FriendlyName, item.Mass, item.Volume);
            }

            bld.AppendLine();
            bld.AppendFormat("NPC Ore\r\n");
            bld.AppendFormat("Name\tMass Kg\tVolume L\r\n");
            foreach (var item in _npcOre)
            {
                bld.AppendFormat("{0}\t{1:#,##0.000}\t{2:#,##0.000}\r\n", item.FriendlyName, item.Mass, item.Volume);
            }

            // Counts of all current items in game Assets.
            // numbers of cubes, components, tools, ingots, etc could be included, as they technically indicate time spent to construct or refine.

            bld.AppendLine();
            bld.AppendFormat("All Cubes\r\n");
            foreach (var item in _allCubes)
            {

            }

            bld.AppendLine();
            bld.AppendFormat("All Components\r\n");
            foreach (var item in _allComponents)
            {

            }

            bld.AppendLine();
            bld.AppendFormat("All Items\r\n");
            bld.AppendFormat("Name\tMass Kg\tVolume L\tTime\r\n");
            foreach (var item in _allItems)
            {
                bld.AppendFormat("{0}\t{1:#,##0.000}\t{2:#,##0.000}\t{3}\r\n", item.FriendlyName, item.Mass, item.Volume, item.Time);
            }

            this.ReportText = bld.ToString();

            this.ClearProgress();

            #endregion
        }


        #endregion
    }
}
