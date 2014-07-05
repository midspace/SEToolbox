namespace SEToolbox.Models
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Common.ObjectBuilders.Definitions;
    using SEToolbox.ImageLibrary;
    using SEToolbox.Interfaces;
    using SEToolbox.Interop;
    using SEToolbox.Interop.Asteroids;
    using SEToolbox.Support;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Web.UI;
    using System.Xml;

    public class ResourceReportModel : BaseModel
    {
        #region Fields

        private bool _isBusy;
        private bool _isActive;
        private bool _isReportReady;
        private string _reportHtml;
        private readonly Stopwatch _timer;
        private bool _showProgress;
        private double _progress;
        private double _maximumProgress;

        private string _saveName;
        private DateTime _generatedDate;
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
        /// player controlled (inventory), measured in Kg and L.
        /// </summary>
        private List<OreAssetModel> _playerOre;

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

        public string SaveName
        {
            get { return this._saveName; }
        }

        public bool IsReportReady
        {
            get
            {
                return this._isReportReady;
            }

            set
            {
                if (value != this._isReportReady)
                {
                    this._isReportReady = value;
                    this.RaisePropertyChanged(() => IsReportReady);
                }
            }
        }

        public string ReportHtml
        {
            get
            {
                return this._reportHtml;
            }

            set
            {
                if (value != this._reportHtml)
                {
                    this._reportHtml = value;
                    this.RaisePropertyChanged(() => ReportHtml);
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

        public void Load(string saveName, IList<IStructureBase> entities)
        {
            this._saveName = saveName;
            this._entities = entities;
            this.SetActiveStatus();
        }

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
            this.IsReportReady = false;
            this._generatedDate = DateTime.Now;
            var contentPath = ToolboxUpdater.GetApplicationContentPath();
            var accumulateMaterials = new SortedDictionary<string, long>();
            var accumulateUnusedOres = new SortedDictionary<string, OreAssetModel>();
            var accumulateUsedOres = new SortedDictionary<string, OreAssetModel>();
            var accumulatePlayerOres = new SortedDictionary<string, OreAssetModel>();
            var accumulateNpcOres = new SortedDictionary<string, OreAssetModel>();
            var accumulateItems = new SortedDictionary<string, ComponentItemModel>();
            var accumulateComponents = new SortedDictionary<string, ComponentItemModel>();
            var accumulateCubes = new SortedDictionary<string, ComponentItemModel>();

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

                    if (floating.FloatingObject.Item.Content.TypeId == MyObjectBuilderTypeEnum.Ore || floating.FloatingObject.Item.Content.TypeId == MyObjectBuilderTypeEnum.Ingot)
                    {
                        TallyItems(floating.FloatingObject.Item.Content.TypeId, floating.FloatingObject.Item.Content.SubtypeName, floating.FloatingObject.Item.AmountDecimal, contentPath, accumulateUnusedOres, accumulateItems, accumulateComponents);
                    }
                    else
                    {
                        TallyItems(floating.FloatingObject.Item.Content.TypeId, floating.FloatingObject.Item.Content.SubtypeName, floating.FloatingObject.Item.AmountDecimal, contentPath, accumulateUsedOres, accumulateItems, accumulateComponents);
                    }
                }
                else if (entity is StructureCharacterModel)
                {
                    var character = entity as StructureCharacterModel;

                    // Ignore pilots, as we'll check those in the ship.
                    if (!character.IsPilot)
                    {
                        // Character inventory.
                        foreach (var item in character.Character.Inventory.Items)
                        {
                            TallyItems(item.Content.TypeId, item.Content.SubtypeName, item.AmountDecimal, contentPath, accumulatePlayerOres, accumulateItems, accumulateComponents);
                        }
                    }
                }
                else if (entity is StructureCubeGridModel)
                {
                    var ship = entity as StructureCubeGridModel;
                    var isNpc = ship.CubeGrid.CubeBlocks.Any<MyObjectBuilder_CubeBlock>(e => e is MyObjectBuilder_Cockpit && ((MyObjectBuilder_Cockpit)e).Autopilot != null);

                    foreach (MyObjectBuilder_CubeBlock block in ship.CubeGrid.CubeBlocks)
                    {
                        var blockType = block.GetType();
                        var cubeBlockDefinition = SpaceEngineersAPI.GetCubeDefinition(block.TypeId, ship.CubeGrid.GridSizeEnum, block.SubtypeName);
                        TimeSpan blockTime = TimeSpan.Zero;
                        string blockTexture = null;
                        float cubeMass = 0;

                        // Unconstructed portion.
                        if (block.ConstructionStockpile != null && block.ConstructionStockpile.Items.Count > 0)
                        {
                            foreach (MyObjectBuilder_StockpileItem item in block.ConstructionStockpile.Items)
                            {
                                var cd = SpaceEngineersAPI.GetDefinition(item.PhysicalContent.TypeId, item.PhysicalContent.SubtypeName) as MyObjectBuilder_ComponentDefinition;

                                if (isNpc)
                                {
                                    TallyItems(item.PhysicalContent.TypeId, item.PhysicalContent.SubtypeName, item.Amount, contentPath, accumulateNpcOres, null, null);
                                }
                                else
                                {
                                    TallyItems(item.PhysicalContent.TypeId, item.PhysicalContent.SubtypeName, item.Amount, contentPath, accumulateUsedOres, null, null);
                                }

                                float componentMass = cd.Mass * item.Amount;
                                cubeMass += componentMass;
                            }
                        }

                        if (cubeBlockDefinition != null)
                        {
                            if (block.BuildPercent < 1f)
                            {
                                // break down the components, to get a accurate counted on the number of components actually in the cube.
                                var componentList = new List<MyObjectBuilder_ComponentDefinition>();

                                foreach (var component in cubeBlockDefinition.Components)
                                {
                                    var cd = SpaceEngineersAPI.GetDefinition(component.Type, component.Subtype) as MyObjectBuilder_ComponentDefinition;
                                    for (var i = 0; i < component.Count; i++)
                                        componentList.Add(cd);
                                }

                                // Round up to nearest whole number.
                                var completeCount = Math.Min(componentList.Count, Math.Ceiling((double)componentList.Count * (double)block.BuildPercent));

                                // count only the components used to reach the BuildPercent, 1 component at a time.
                                for (var i = 0; i < completeCount; i++)
                                {
                                    #region used ore value

                                    if (isNpc)
                                    {
                                        TallyItems(componentList[i].Id.TypeId, componentList[i].Id.SubtypeName, 1, contentPath, accumulateNpcOres, null, null);
                                    }
                                    else
                                    {
                                        TallyItems(componentList[i].Id.TypeId, componentList[i].Id.SubtypeName, 1, contentPath, accumulateUsedOres, null, null);
                                    }

                                    #endregion

                                    float componentMass = componentList[i].Mass * 1;
                                    cubeMass += componentMass;
                                }
                            }
                            else
                            {
                                // Fully armed and operational cube.
                                foreach (var component in cubeBlockDefinition.Components)
                                {
                                    var cd = SpaceEngineersAPI.GetDefinition(component.Type, component.Subtype) as MyObjectBuilder_ComponentDefinition;

                                    #region used ore value

                                    if (isNpc)
                                    {
                                        TallyItems(component.Type, component.Subtype, component.Count, contentPath, accumulateNpcOres, null, null);
                                    }
                                    else
                                    {
                                        TallyItems(component.Type, component.Subtype, component.Count, contentPath, accumulateUsedOres, null, null);
                                    }

                                    #endregion

                                    float componentMass = cd.Mass * component.Count;
                                    cubeMass += componentMass;
                                }
                            }

                            blockTime = new TimeSpan((long)(TimeSpan.TicksPerSecond * cubeBlockDefinition.BuildTimeSeconds * block.BuildPercent));
                            blockTexture = Path.Combine(contentPath, cubeBlockDefinition.Icon + ".dds");
                        }

                        if (!blockType.Equals(typeof(MyObjectBuilder_CubeBlockDefinition)))
                        {
                            var fields = blockType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);

                            #region Inventories

                            var inventoryFields = fields.Where(f => f.FieldType == typeof(MyObjectBuilder_Inventory)).ToArray();
                            foreach (var field in inventoryFields)
                            {
                                var inventory = field.GetValue(block) as MyObjectBuilder_Inventory;
                                if (inventory != null)
                                {
                                    foreach (var item in inventory.Items)
                                    {
                                        if (isNpc)
                                        {
                                            TallyItems(item.Content.TypeId, item.Content.SubtypeName, item.AmountDecimal, contentPath, accumulateNpcOres, accumulateItems, accumulateComponents);
                                        }
                                        else
                                        {
                                            if (item.Content.TypeId == MyObjectBuilderTypeEnum.Ore || item.Content.TypeId == MyObjectBuilderTypeEnum.Ingot)
                                            {
                                                TallyItems(item.Content.TypeId, item.Content.SubtypeName, item.AmountDecimal, contentPath, accumulateUnusedOres, accumulateItems, accumulateComponents);
                                            }
                                            else
                                            {
                                                TallyItems(item.Content.TypeId, item.Content.SubtypeName, item.AmountDecimal, contentPath, accumulateUsedOres, accumulateItems, accumulateComponents);
                                            }
                                        }
                                    }
                                }
                            }

                            #endregion

                            #region Character inventories

                            var characterFields = fields.Where(f => f.FieldType == typeof(MyObjectBuilder_Character)).ToArray();
                            foreach (var field in characterFields)
                            {
                                var character = field.GetValue(block) as MyObjectBuilder_Character;
                                if (character != null)
                                {
                                    foreach (var item in character.Inventory.Items)
                                    {
                                        TallyItems(item.Content.TypeId, item.Content.SubtypeName, item.AmountDecimal, contentPath, accumulatePlayerOres, accumulateItems, accumulateComponents);
                                    }
                                }
                            }

                            #endregion
                        }

                        #region tally cubes

                        var itemsKey = cubeBlockDefinition.DisplayName;
                        if (accumulateCubes.ContainsKey(itemsKey))
                        {
                            accumulateCubes[itemsKey].Count += 1;
                            accumulateCubes[itemsKey].Mass += cubeMass;
                            accumulateCubes[itemsKey].Time += blockTime;
                        }
                        else
                        {
                            accumulateCubes.Add(itemsKey, new ComponentItemModel() { Name = cubeBlockDefinition.DisplayName, Count = 1, Mass = cubeMass, TypeId = cubeBlockDefinition.Id.TypeId, SubtypeId = cubeBlockDefinition.Id.SubtypeName, TextureFile = blockTexture, Time = blockTime });
                        }

                        #endregion
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
            _playerOre = accumulatePlayerOres.Values.ToList();
            _npcOre = accumulateNpcOres.Values.ToList();
            _allCubes = accumulateCubes.Values.ToList();
            _allComponents = accumulateComponents.Values.ToList();
            _allItems = accumulateItems.Values.ToList();

            #endregion

            #region create report

            this.IsReportReady = true;

            this.ClearProgress();

            #endregion
        }

        #region TallyItems

        private void TallyItems(MyObjectBuilderTypeEnum tallyTypeId, string tallySubTypeId, Decimal amountDecimal, string contentPath, SortedDictionary<string, OreAssetModel> accumulateOres, SortedDictionary<string, ComponentItemModel> accumulateItems, SortedDictionary<string, ComponentItemModel> accumulateComponents)
        {
            var cd = SpaceEngineersAPI.GetDefinition(tallyTypeId, tallySubTypeId) as MyObjectBuilder_PhysicalItemDefinition;

            if (cd == null)
            {
                // A component, gun, ore that doesn't exist (Depricated by KeenSH, or Mod that isn't loaded).
                return;
            }

            var componentTexture = Path.Combine(contentPath, cd.Icon + ".dds");

            if (tallyTypeId == MyObjectBuilderTypeEnum.Ore)
            {
                var mass = Math.Round((double)amountDecimal * cd.Mass, 7);
                var volume = Math.Round((double)amountDecimal * (cd.Volume.HasValue ? cd.Volume.Value : 0), 7);

                #region unused ore value

                var unusedKey = tallySubTypeId;
                if (accumulateOres.ContainsKey(unusedKey))
                {
                    accumulateOres[unusedKey].Amount += amountDecimal;
                    accumulateOres[unusedKey].Mass += mass;
                    accumulateOres[unusedKey].Volume += volume;
                }
                else
                {
                    accumulateOres.Add(unusedKey, new OreAssetModel() { Name = cd.DisplayName, Amount = amountDecimal, Mass = mass, Volume = volume, TextureFile = componentTexture });
                }

                #endregion

                #region tally items

                if (accumulateItems != null)
                {
                    var itemsKey = cd.DisplayName;
                    if (accumulateItems.ContainsKey(itemsKey))
                    {
                        accumulateItems[itemsKey].Count += amountDecimal;
                        accumulateItems[itemsKey].Mass += mass;
                        accumulateItems[itemsKey].Volume += volume;
                    }
                    else
                    {
                        accumulateItems.Add(itemsKey, new ComponentItemModel() { Name = cd.DisplayName, Count = amountDecimal, Mass = mass, Volume = volume, TypeId = tallyTypeId, SubtypeId = tallySubTypeId, TextureFile = componentTexture, Time = TimeSpan.Zero });
                    }
                }

                #endregion
            }
            else if (tallyTypeId == MyObjectBuilderTypeEnum.Ingot)
            {
                var mass = Math.Round((double)amountDecimal * cd.Mass, 7);
                var volume = Math.Round((double)amountDecimal * (cd.Volume.HasValue ? cd.Volume.Value : 0), 7);
                var bp = SpaceEngineersAPI.Definitions.Blueprints.FirstOrDefault(b => b.Result.SubtypeId == tallySubTypeId && b.Result.TypeId == tallyTypeId);
                var timeToMake = new TimeSpan((long)(TimeSpan.TicksPerSecond * (decimal)bp.BaseProductionTimeInSeconds * amountDecimal));

                #region unused ore value

                var oreRequirements = new Dictionary<string, MyObjectBuilder_BlueprintDefinition.Item>();
                TimeSpan ingotTime;
                SpaceEngineersAPI.AccumulateCubeBlueprintRequirements(tallySubTypeId, tallyTypeId, amountDecimal, oreRequirements, out ingotTime);

                foreach (var item in oreRequirements)
                {
                    TallyItems(item.Value.TypeId, item.Value.SubtypeId, item.Value.Amount, contentPath, accumulateOres, null, null);
                }

                #endregion

                #region tally items

                if (accumulateItems != null)
                {
                    var itemsKey = cd.DisplayName;
                    if (accumulateItems.ContainsKey(itemsKey))
                    {
                        accumulateItems[itemsKey].Count += amountDecimal;
                        accumulateItems[itemsKey].Mass += mass;
                        accumulateItems[itemsKey].Volume += volume;
                        accumulateItems[itemsKey].Time += timeToMake;
                    }
                    else
                    {
                        accumulateItems.Add(itemsKey, new ComponentItemModel() { Name = cd.DisplayName, Count = amountDecimal, Mass = mass, Volume = volume, TypeId = tallyTypeId, SubtypeId = tallySubTypeId, TextureFile = componentTexture, Time = timeToMake });
                    }
                }

                #endregion
            }
            else if (tallyTypeId == MyObjectBuilderTypeEnum.AmmoMagazine ||
                tallyTypeId == MyObjectBuilderTypeEnum.PhysicalGunObject)
            {
                var mass = Math.Round((double)amountDecimal * cd.Mass, 7);
                var volume = Math.Round((double)amountDecimal * (cd.Volume.HasValue ? cd.Volume.Value : 0), 7);
                var bp = SpaceEngineersAPI.Definitions.Blueprints.FirstOrDefault(b => b.Result.SubtypeId == tallySubTypeId && b.Result.TypeId == tallyTypeId);
                var timeToMake = new TimeSpan((long)(TimeSpan.TicksPerSecond * (decimal)bp.BaseProductionTimeInSeconds * amountDecimal));

                #region unused ore value

                var oreRequirements = new Dictionary<string, MyObjectBuilder_BlueprintDefinition.Item>();
                TimeSpan ingotTime;
                SpaceEngineersAPI.AccumulateCubeBlueprintRequirements(tallySubTypeId, tallyTypeId, amountDecimal, oreRequirements, out ingotTime);

                foreach (var item in oreRequirements)
                {
                    TallyItems(item.Value.TypeId, item.Value.SubtypeId, item.Value.Amount, contentPath, accumulateOres, null, accumulateComponents);
                }

                #endregion

                #region tally items

                if (accumulateItems != null)
                {
                    var itemsKey = cd.DisplayName;
                    if (accumulateItems.ContainsKey(itemsKey))
                    {
                        accumulateItems[itemsKey].Count += amountDecimal;
                        accumulateItems[itemsKey].Mass += mass;
                        accumulateItems[itemsKey].Volume += volume;
                        accumulateItems[itemsKey].Time += timeToMake;
                    }
                    else
                    {
                        accumulateItems.Add(itemsKey, new ComponentItemModel() { Name = cd.DisplayName, Count = amountDecimal, Mass = mass, Volume = volume, TypeId = tallyTypeId, SubtypeId = tallySubTypeId, TextureFile = componentTexture, Time = timeToMake });
                    }
                }

                #endregion
            }
            else if (tallyTypeId == MyObjectBuilderTypeEnum.Component)
            {
                var mass = Math.Round((double)amountDecimal * cd.Mass, 7);
                var volume = Math.Round((double)amountDecimal * (cd.Volume.HasValue ? cd.Volume.Value : 0), 7);
                var bp = SpaceEngineersAPI.Definitions.Blueprints.FirstOrDefault(b => b.Result.SubtypeId == tallySubTypeId && b.Result.TypeId == tallyTypeId);
                var timeToMake = new TimeSpan((long)(TimeSpan.TicksPerSecond * (decimal)bp.BaseProductionTimeInSeconds * amountDecimal));

                #region unused ore value

                var oreRequirements = new Dictionary<string, MyObjectBuilder_BlueprintDefinition.Item>();
                TimeSpan ingotTime;
                SpaceEngineersAPI.AccumulateCubeBlueprintRequirements(tallySubTypeId, tallyTypeId, amountDecimal, oreRequirements, out ingotTime);

                foreach (var item in oreRequirements)
                {
                    TallyItems(item.Value.TypeId, item.Value.SubtypeId, item.Value.Amount, contentPath, accumulateOres, null, null);
                }

                #endregion

                #region tally items

                if (accumulateComponents != null)
                {
                    var itemsKey = cd.DisplayName;
                    if (accumulateComponents.ContainsKey(itemsKey))
                    {
                        accumulateComponents[itemsKey].Count += amountDecimal;
                        accumulateComponents[itemsKey].Mass += mass;
                        accumulateComponents[itemsKey].Volume += volume;
                        accumulateComponents[itemsKey].Time += timeToMake;
                    }
                    else
                    {
                        accumulateComponents.Add(itemsKey, new ComponentItemModel() { Name = cd.DisplayName, Count = amountDecimal, Mass = mass, Volume = volume, TypeId = tallyTypeId, SubtypeId = tallySubTypeId, TextureFile = componentTexture, Time = timeToMake });
                    }
                }

                #endregion
            }
            //else if (typeId == MyObjectBuilderTypeEnum.CubeBlock)
            else // if (tallyItem is MyObjectBuilder_EntityBase)
            {
                // TODO: missed a new object type?
            }
        }

        #endregion

        #region CreateTextReport

        internal string CreateTextReport()
        {
            var bld = new StringBuilder();

            bld.AppendLine("Resource Report");
            bld.AppendFormat("Save World: {0}\r\n", _saveName);
            bld.AppendFormat("Date: {0}\r\n", _generatedDate);
            bld.AppendLine();

            bld.AppendLine("In game resources");
            bld.AppendLine("Everything is measured in its regressed state. Ie., how much ore was used/needed to build this item.");
            bld.AppendLine();

            bld.AppendFormat("Untouched Ore (Asteroids)\r\n");
            bld.AppendFormat("Name\tVolume m³\r\n");
            foreach (var item in _untouchedOre)
            {
                bld.AppendFormat("{0}\t{1:#,##0.000}\r\n", item.MaterialName, item.Volume);
            }

            bld.AppendLine();
            bld.AppendFormat("Unused Ore (Ore and Ingots, either floating or in containers)\r\n");
            bld.AppendFormat("Name\tMass Kg\tVolume L\r\n");
            foreach (var item in _unusedOre)
            {
                bld.AppendFormat("{0}\t{1:#,##0.000}\t{2:#,##0.000}\r\n", item.FriendlyName, item.Mass, item.Volume);
            }

            bld.AppendLine();
            bld.AppendFormat("Used Ore (tools, items, components, cubes)\r\n");
            bld.AppendFormat("Name\tMass Kg\tVolume L\r\n");
            foreach (var item in _usedOre)
            {
                bld.AppendFormat("{0}\t{1:#,##0.000}\t{2:#,##0.000}\r\n", item.FriendlyName, item.Mass, item.Volume);
            }

            bld.AppendLine();
            bld.AppendFormat("Player Ore (Player inventories)\r\n");
            bld.AppendFormat("Name\tMass Kg\tVolume L\r\n");
            foreach (var item in _playerOre)
            {
                bld.AppendFormat("{0}\t{1:#,##0.000}\t{2:#,##0.000}\r\n", item.FriendlyName, item.Mass, item.Volume);
            }

            bld.AppendLine();
            bld.AppendFormat("NPC Ore (Controlled by NPC)\r\n");
            bld.AppendFormat("Name\tMass Kg\tVolume L\r\n");
            foreach (var item in _npcOre)
            {
                bld.AppendFormat("{0}\t{1:#,##0.000}\t{2:#,##0.000}\r\n", item.FriendlyName, item.Mass, item.Volume);
            }

            bld.AppendLine();
            bld.AppendLine("In game assets");
            bld.AppendLine("Counts of all current items in game Assets. These indicate actual time spent to construct, part construct or refine.");

            bld.AppendLine();
            bld.AppendFormat("All Cubes\r\n");
            bld.AppendFormat("Name\tCount\tMass Kg\tTime\r\n");
            foreach (var item in _allCubes)
            {
                bld.AppendFormat("{0}\t{1:#,##0}\t{2:#,##0.000}\t{3}\r\n", item.FriendlyName, item.Count, item.Mass, item.Time);
            }

            bld.AppendLine();
            bld.AppendFormat("All Components\r\n");
            bld.AppendFormat("Name\tCount\tMass Kg\tVolume L\tTime\r\n");
            foreach (var item in _allComponents)
            {
                bld.AppendFormat("{0}\t{1:#,##0}\t{2:#,##0.000}\t{3:#,##0.000}\t{4}\r\n", item.FriendlyName, item.Count, item.Mass, item.Volume, item.Time);
            }

            bld.AppendLine();
            bld.AppendFormat("All Items\r\n");
            bld.AppendFormat("Name\tCount\tMass Kg\tVolume L\tTime\r\n");
            foreach (var item in _allItems)
            {
                bld.AppendFormat("{0}\t{1:#,##0}\t{2:#,##0.000}\t{3:#,##0.000}\t{4}\r\n", item.FriendlyName, item.Count, item.Mass, item.Volume, item.Time);
            }

            return bld.ToString();
        }

        #endregion

        #region CreateHtmlReport

        internal string CreateHtmlReport()
        {
            var stringWriter = new StringWriter();

            // Put HtmlTextWriter in using block because it needs to call Dispose.
            using (var writer = new HtmlTextWriter(stringWriter))
            {
                #region header

                writer.BeginDocument("Resource Report",
                    @"
body { background-color: #F6F6FA }
p { font-family: Arial, Helvetica, sans-serif; }
h1,h2,h3 { font-family: Arial, Helvetica, sans-serif; }
table { background-color: #FFFFFF; }
table tr td { font-family: Arial, Helvetica, sans-serif; font-size: small; line-height: normal; color: #000000; }
table thead td { background-color: #BABDD6; font-weight: bold; Color: #000000; }
td.right { text-align: right; }");

                #endregion

                writer.RenderElement(HtmlTextWriterTag.H1, "Resource Report");

                writer.RenderElement(HtmlTextWriterTag.P, "Save World: {0}", _saveName);
                writer.RenderElement(HtmlTextWriterTag.P, "Date: {0}", _generatedDate);

                writer.RenderElement(HtmlTextWriterTag.H2, "In game resources");
                writer.RenderElement(HtmlTextWriterTag.P, "Everything is measured in its regressed state. Ie., how much ore was used/needed to build this item.");

                writer.RenderElement(HtmlTextWriterTag.H3, "Untouched Ore (Asteroids)");
                writer.BeginTable("1", "3", "0", new String[] { "Name", "Volume m³" });
                foreach (var item in this._untouchedOre)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                    writer.RenderElement(HtmlTextWriterTag.Td, item.MaterialName);
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "right");
                    writer.RenderElement(HtmlTextWriterTag.Td, "{0:#,##0.000}", item.Volume);
                    writer.RenderEndTag(); // Tr
                }
                writer.EndTable();

                writer.RenderElement(HtmlTextWriterTag.H3, "Unused Ore (Ore and Ingots, either floating or in containers)");
                writer.BeginTable("1", "3", "0", new String[] { "Name", "Mass Kg", "Volume L" });
                foreach (var item in _unusedOre)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                    writer.RenderElement(HtmlTextWriterTag.Td, item.FriendlyName);
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "right");
                    writer.RenderElement(HtmlTextWriterTag.Td, "{0:#,##0.000}", item.Mass);
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "right");
                    writer.RenderElement(HtmlTextWriterTag.Td, "{0:#,##0.000}", item.Volume);
                    writer.RenderEndTag(); // Tr
                }
                writer.EndTable();

                writer.RenderElement(HtmlTextWriterTag.H3, "Used Ore (in tools, items, components, cubes)");
                writer.BeginTable("1", "3", "0", new String[] { "Name", "Mass Kg", "Volume L" });
                foreach (var item in _usedOre)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                    writer.RenderElement(HtmlTextWriterTag.Td, item.FriendlyName);
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "right");
                    writer.RenderElement(HtmlTextWriterTag.Td, "{0:#,##0.000}", item.Mass);
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "right");
                    writer.RenderElement(HtmlTextWriterTag.Td, "{0:#,##0.000}", item.Volume);
                    writer.RenderEndTag(); // Tr
                }
                writer.EndTable();

                writer.RenderElement(HtmlTextWriterTag.H3, "Player Ore (Player inventories)");
                writer.BeginTable("1", "3", "0", new String[] { "Name", "Mass Kg", "Volume L" });
                foreach (var item in _playerOre)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                    writer.RenderElement(HtmlTextWriterTag.Td, item.FriendlyName);
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "right");
                    writer.RenderElement(HtmlTextWriterTag.Td, "{0:#,##0.000}", item.Mass);
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "right");
                    writer.RenderElement(HtmlTextWriterTag.Td, "{0:#,##0.000}", item.Volume);
                    writer.RenderEndTag(); // Tr
                }
                writer.EndTable();

                writer.RenderElement(HtmlTextWriterTag.H3, "NPC Ore (Controlled by NPC)");
                writer.BeginTable("1", "3", "0", new String[] { "Name", "Mass Kg", "Volume L" });
                foreach (var item in _npcOre)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                    writer.RenderElement(HtmlTextWriterTag.Td, item.FriendlyName);
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "right");
                    writer.RenderElement(HtmlTextWriterTag.Td, "{0:#,##0.000}", item.Mass);
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "right");
                    writer.RenderElement(HtmlTextWriterTag.Td, "{0:#,##0.000}", item.Volume);
                    writer.RenderEndTag(); // Tr
                }
                writer.EndTable();

                writer.RenderElement(HtmlTextWriterTag.Br);
                writer.RenderElement(HtmlTextWriterTag.Hr);

                writer.RenderElement(HtmlTextWriterTag.H2, "In game assets");
                writer.RenderElement(HtmlTextWriterTag.P, "Counts of all current items in game Assets. These indicate actual time spent to construct, part construct or refine.");

                writer.RenderElement(HtmlTextWriterTag.H3, "All Cubes");
                writer.BeginTable("1", "3", "0", new String[] { "Icon", "Name", "Count", "Mass Kg", "Time" });
                foreach (var item in _allCubes)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    if (item.TextureFile != null)
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Src, "data:image/png;base64," + ImageTextureUtil.GetTextureToBase64(item.TextureFile, 32, 32));
                        writer.AddAttribute(HtmlTextWriterAttribute.Width, "32");
                        writer.AddAttribute(HtmlTextWriterAttribute.Height, "32");
                        writer.AddAttribute(HtmlTextWriterAttribute.Alt, Path.GetFileNameWithoutExtension(item.TextureFile));
                        writer.RenderBeginTag(HtmlTextWriterTag.Img);
                        writer.RenderEndTag();
                    }
                    writer.RenderEndTag(); // Td

                    writer.RenderElement(HtmlTextWriterTag.Td, item.FriendlyName);
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "right");
                    writer.RenderElement(HtmlTextWriterTag.Td, "{0:#,##0}", item.Count);
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "right");
                    writer.RenderElement(HtmlTextWriterTag.Td, "{0:#,##0.000}", item.Mass);
                    writer.RenderElement(HtmlTextWriterTag.Td, "{0}", item.Time);
                    writer.RenderEndTag(); // Tr
                }
                writer.EndTable();

                writer.RenderElement(HtmlTextWriterTag.H3, "All Components");
                writer.BeginTable("1", "3", "0", new String[] { "Icon", "Name", "Count", "Mass Kg", "Volume L", "Time" });
                foreach (var item in _allComponents)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    if (item.TextureFile != null)
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Src, "data:image/png;base64," + ImageTextureUtil.GetTextureToBase64(item.TextureFile, 32, 32));
                        writer.AddAttribute(HtmlTextWriterAttribute.Width, "32");
                        writer.AddAttribute(HtmlTextWriterAttribute.Height, "32");
                        writer.AddAttribute(HtmlTextWriterAttribute.Alt, Path.GetFileNameWithoutExtension(item.TextureFile));
                        writer.RenderBeginTag(HtmlTextWriterTag.Img);
                        writer.RenderEndTag();
                    }
                    writer.RenderEndTag(); // Td

                    writer.RenderElement(HtmlTextWriterTag.Td, item.FriendlyName);
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "right");
                    writer.RenderElement(HtmlTextWriterTag.Td, "{0:#,##0}", item.Count);
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "right");
                    writer.RenderElement(HtmlTextWriterTag.Td, "{0:#,##0.000}", item.Mass);
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "right");
                    writer.RenderElement(HtmlTextWriterTag.Td, "{0:#,##0.000}", item.Volume);
                    writer.RenderElement(HtmlTextWriterTag.Td, "{0}", item.Time);
                    writer.RenderEndTag(); // Tr
                }
                writer.EndTable();

                writer.RenderElement(HtmlTextWriterTag.H3, "All Items");
                writer.BeginTable("1", "3", "0", new String[] { "Icon", "Name", "Count", "Mass Kg", "Volume L", "Time" });
                foreach (var item in _allItems)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    if (item.TextureFile != null)
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Src, "data:image/png;base64," + ImageTextureUtil.GetTextureToBase64(item.TextureFile, 32, 32));
                        writer.AddAttribute(HtmlTextWriterAttribute.Width, "32");
                        writer.AddAttribute(HtmlTextWriterAttribute.Height, "32");
                        writer.AddAttribute(HtmlTextWriterAttribute.Alt, Path.GetFileNameWithoutExtension(item.TextureFile));
                        writer.RenderBeginTag(HtmlTextWriterTag.Img);
                        writer.RenderEndTag();
                    }
                    writer.RenderEndTag(); // Td

                    writer.RenderElement(HtmlTextWriterTag.Td, item.FriendlyName);
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "right");
                    writer.RenderElement(HtmlTextWriterTag.Td, "{0:#,##0}", item.Count);
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "right");
                    writer.RenderElement(HtmlTextWriterTag.Td, "{0:#,##0.000}", item.Mass);
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "right");
                    writer.RenderElement(HtmlTextWriterTag.Td, "{0:#,##0.000}", item.Volume);
                    writer.RenderElement(HtmlTextWriterTag.Td, "{0}", item.Time);
                    writer.RenderEndTag(); // Tr
                }
                writer.EndTable();

                writer.EndDocument();
            }

            return stringWriter.ToString();
        }

        #endregion

        #region CreateXmlReport

        internal string CreateXmlReport()
        {
            var settingsDestination = new XmlWriterSettings()
            {
                Indent = true, // Set indent to false to compress.
                Encoding = new UTF8Encoding(false)   // codepage 65001 without signature. Removes the Byte Order Mark from the start of the file.
            };

            var stringWriter = new StringWriter();

            using (var xmlWriter = XmlWriter.Create(stringWriter, settingsDestination))
            {
                xmlWriter.WriteStartElement("report");
                xmlWriter.WriteAttributeString("title", "Resource Report");
                xmlWriter.WriteAttributeString("world", _saveName);
                xmlWriter.WriteAttributeFormat("date", "{0:o}", _generatedDate);

                foreach (var item in this._untouchedOre)
                {
                    xmlWriter.WriteStartElement("untouched");
                    xmlWriter.WriteElementFormat("name", "{0}", item.MaterialName);
                    xmlWriter.WriteElementFormat("volume", "{0:0.000}", item.Volume);
                    xmlWriter.WriteEndElement();
                }

                foreach (var item in this._unusedOre)
                {
                    xmlWriter.WriteStartElement("unused");
                    xmlWriter.WriteElementFormat("name", "{0}", item.FriendlyName);
                    xmlWriter.WriteElementFormat("mass", "{0:0.000}", item.Mass);
                    xmlWriter.WriteElementFormat("volume", "{0:0.000}", item.Volume);
                    xmlWriter.WriteEndElement();
                }

                foreach (var item in _usedOre)
                {
                    xmlWriter.WriteStartElement("used");
                    xmlWriter.WriteElementFormat("name", "{0}", item.FriendlyName);
                    xmlWriter.WriteElementFormat("mass", "{0:0.000}", item.Mass);
                    xmlWriter.WriteElementFormat("volume", "{0:0.000}", item.Volume);
                    xmlWriter.WriteEndElement();
                }

                foreach (var item in _playerOre)
                {
                    xmlWriter.WriteStartElement("player");
                    xmlWriter.WriteElementFormat("name", "{0}", item.FriendlyName);
                    xmlWriter.WriteElementFormat("mass", "{0:0.000}", item.Mass);
                    xmlWriter.WriteElementFormat("volume", "{0:0.000}", item.Volume);
                    xmlWriter.WriteEndElement();
                }

                foreach (var item in _npcOre)
                {
                    xmlWriter.WriteStartElement("npc");
                    xmlWriter.WriteElementFormat("name", "{0}", item.FriendlyName);
                    xmlWriter.WriteElementFormat("mass", "{0:0.000}", item.Mass);
                    xmlWriter.WriteElementFormat("volume", "{0:0.000}", item.Volume);
                    xmlWriter.WriteEndElement();
                }

                foreach (var item in _allCubes)
                {
                    xmlWriter.WriteStartElement("cubes");
                    xmlWriter.WriteElementFormat("friendlyname", "{0}", item.FriendlyName);
                    xmlWriter.WriteElementFormat("name", "{0}", item.Name);
                    xmlWriter.WriteElementFormat("typeid", "{0}", item.TypeId);
                    xmlWriter.WriteElementFormat("subtypeid", "{0}", item.SubtypeId);
                    xmlWriter.WriteElementFormat("count", "{0:0}", item.Count);
                    xmlWriter.WriteElementFormat("mass", "{0:0.000}", item.Mass);
                    xmlWriter.WriteElementFormat("time", "{0}", item.Time);
                    xmlWriter.WriteEndElement();
                }

                foreach (var item in _allComponents)
                {
                    xmlWriter.WriteStartElement("components");
                    xmlWriter.WriteElementFormat("friendlyname", "{0}", item.FriendlyName);
                    xmlWriter.WriteElementFormat("name", "{0}", item.Name);
                    xmlWriter.WriteElementFormat("typeid", "{0}", item.TypeId);
                    xmlWriter.WriteElementFormat("subtypeid", "{0}", item.SubtypeId);
                    xmlWriter.WriteElementFormat("count", "{0:0}", item.Count);
                    xmlWriter.WriteElementFormat("mass", "{0:0.000}", item.Mass);
                    xmlWriter.WriteElementFormat("volume", "{0:0.000}", item.Volume);
                    xmlWriter.WriteElementFormat("time", "{0}", item.Time);
                    xmlWriter.WriteEndElement();
                }

                foreach (var item in _allItems)
                {
                    xmlWriter.WriteStartElement("items");
                    xmlWriter.WriteElementFormat("friendlyname", "{0}", item.FriendlyName);
                    xmlWriter.WriteElementFormat("name", "{0}", item.Name);
                    xmlWriter.WriteElementFormat("typeid", "{0}", item.TypeId);
                    xmlWriter.WriteElementFormat("subtypeid", "{0}", item.SubtypeId);
                    xmlWriter.WriteElementFormat("count", "{0:0}", item.Count);
                    xmlWriter.WriteElementFormat("mass", "{0:0.000}", item.Mass);
                    xmlWriter.WriteElementFormat("volume", "{0:0.000}", item.Volume);
                    xmlWriter.WriteElementFormat("time", "{0}", item.Time);
                    xmlWriter.WriteEndElement();
                }

                xmlWriter.WriteEndElement();
            }

            return stringWriter.ToString();
        }

        #endregion

        #endregion

        #region GenerateOfflineReport

        /// <summary>
        /// Command line driven method.
        /// <example>
        /// /WR "Easy Start Report" "c:\temp\Easy Start Report.txt"
        /// /WR "C:\Users\%USERNAME%\AppData\Roaming\SpaceEngineersDedicated\Saves\Super Excellent Map\sandbox.sbc" "c:\temp\Super Excellent Map.txt"
        /// /WR "C:\Users\%USERNAME%\AppData\Roaming\SpaceEngineersDedicated\Saves\Super Excellent Map" "c:\temp\Super Excellent Map.txt"
        /// /WR "\\SERVER\Dedicated Saves\Super Excellent Map" "\\SERVER\Reports\Super Excellent Map.txt"
        /// </example>
        /// </summary>
        /// <param name="args"></param>
        public static void GenerateOfflineReport(ExplorerModel baseModel, string[] args)
        {
            var argList = args.ToList();
            var comArgs = args.Where(a => a.ToUpper() == "/WR").Select(a => { return a; }).ToArray();
            foreach (var a in comArgs) argList.Remove(a);

            if (argList.Count < 2)
            {
                Environment.Exit(2);
                return;
            }

            var findSession = argList[0].ToUpper();
            var reportFile = argList[1];
            var reportExtension = Path.GetExtension(reportFile).ToUpper();

            if (reportExtension != ".HTM" &&
                reportExtension != ".HTML" &&
                reportExtension != ".TXT" &&
                reportExtension != ".XML")
            {
                Environment.Exit(1);
                return;
            }

            if (File.Exists(findSession))
            {
                findSession = Path.GetDirectoryName(findSession);
            }

            SaveResource world;

            if (Directory.Exists(findSession))
            {
                world = SelectWorldModel.LoadSession(findSession);
            }
            else
            {
                world = SelectWorldModel.FindSaveSession(baseModel.BaseLocalSavePath, findSession);
            }

            if (world == null)
            {
                Environment.Exit(3);
                return;
            }

            baseModel.ActiveWorld = world;
            baseModel.LoadSandBox(true);

            var model = new ResourceReportModel();
            model.Load(baseModel.ActiveWorld.Savename, baseModel.Structures);
            model.GenerateReport();
            TempfileUtil.Dispose();

            if (reportExtension == ".HTM" || reportExtension == ".HTML")
                File.WriteAllText(reportFile, model.CreateHtmlReport());

            if (reportExtension == ".TXT")
                File.WriteAllText(reportFile, model.CreateTextReport());

            if (reportExtension == ".XML")
                File.WriteAllText(reportFile, model.CreateXmlReport());

            Environment.Exit(0);
        }

        #endregion
    }
}
