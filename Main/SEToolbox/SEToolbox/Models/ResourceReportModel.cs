﻿namespace SEToolbox.Models
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Web.UI;
    using System.Xml;

    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Common.ObjectBuilders.Definitions;
    using SEToolbox.ImageLibrary;
    using SEToolbox.Interfaces;
    using SEToolbox.Interop;
    using SEToolbox.Interop.Asteroids;
    using SEToolbox.Support;
    using VRageMath;

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
        /// untouched (in all asteroids), measured in m³.
        /// </summary>
        private List<VoxelMaterialAssetModel> _untouchedOre;

        /// <summary>
        /// untouched (by asteroid), measured in m³.
        /// </summary>
        private List<AsteroidContent> _untouchedOreByAsteroid;

        /// <summary>
        /// unused (ore and ingot), measured in Kg and L.
        /// </summary>
        private List<OreContent> _unusedOre;

        /// <summary>
        /// used (component, tool, cube), measured in Kg and L.
        /// Everything is measured in it's regressed state. Ie., how much ore was used/needed to build this item.
        /// </summary>
        private List<OreContent> _usedOre;

        /// <summary>
        /// player controlled (inventory), measured in Kg and L.
        /// </summary>
        private List<OreContent> _playerOre;

        /// <summary>
        /// npc (everything currently in an AI controlled ship with ore, ingot, component, tool, cube), measured in Kg and L.
        /// </summary>
        private List<OreContent> _npcOre;

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

        private List<ShipContent> _allShips;

        #endregion

        #region ctor

        public ResourceReportModel()
        {
            _timer = new Stopwatch();
            Progress = 0;
            MaximumProgress = 100;
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
                return _isBusy;
            }

            set
            {
                if (value != _isBusy)
                {
                    _isBusy = value;
                    RaisePropertyChanged(() => IsBusy);
                    SetActiveStatus();
                    if (_isBusy)
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
                return _isActive;
            }

            set
            {
                if (value != _isActive)
                {
                    _isActive = value;
                    RaisePropertyChanged(() => IsActive);
                }
            }
        }

        public string SaveName
        {
            get { return _saveName; }
        }

        public bool IsReportReady
        {
            get
            {
                return _isReportReady;
            }

            set
            {
                if (value != _isReportReady)
                {
                    _isReportReady = value;
                    RaisePropertyChanged(() => IsReportReady);
                }
            }
        }

        public string ReportHtml
        {
            get
            {
                return _reportHtml;
            }

            set
            {
                if (value != _reportHtml)
                {
                    _reportHtml = value;
                    RaisePropertyChanged(() => ReportHtml);
                }
            }
        }

        public bool ShowProgress
        {
            get
            {
                return _showProgress;
            }

            set
            {
                if (value != _showProgress)
                {
                    _showProgress = value;
                    RaisePropertyChanged(() => ShowProgress);
                }
            }
        }

        public double Progress
        {
            get
            {
                return _progress;
            }

            set
            {
                if (value != _progress)
                {
                    _progress = value;

                    if (!_timer.IsRunning || _timer.ElapsedMilliseconds > 100)
                    {
                        RaisePropertyChanged(() => Progress);
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
                return _maximumProgress;
            }

            set
            {
                if (value != _maximumProgress)
                {
                    _maximumProgress = value;
                    RaisePropertyChanged(() => MaximumProgress);
                }
            }
        }

        #endregion

        #region methods

        public void Load(string saveName, IList<IStructureBase> entities)
        {
            _saveName = saveName;
            _entities = entities;
            SetActiveStatus();
        }

        public void SetActiveStatus()
        {
            IsActive = !IsBusy;
        }

        public void ResetProgress(double initial, double maximumProgress)
        {
            MaximumProgress = maximumProgress;
            Progress = initial;
            ShowProgress = true;
            _timer.Restart();
            System.Windows.Forms.Application.DoEvents();
        }

        public void IncrementProgress()
        {
            Progress++;
        }

        public void ClearProgress()
        {
            _timer.Stop();
            ShowProgress = false;
            Progress = 0;
        }

        public void GenerateReport()
        {
            IsReportReady = false;
            _generatedDate = DateTime.Now;
            var contentPath = ToolboxUpdater.GetApplicationContentPath();
            var accumulateOres = new SortedDictionary<string, long>();
            var accumulateAsteroidOres = new List<AsteroidContent>();
            var accumulateUnusedOres = new SortedDictionary<string, OreContent>();
            var accumulateUsedOres = new SortedDictionary<string, OreContent>();
            var accumulatePlayerOres = new SortedDictionary<string, OreContent>();
            var accumulateNpcOres = new SortedDictionary<string, OreContent>();
            var accumulateItems = new SortedDictionary<string, ComponentItemModel>();
            var accumulateComponents = new SortedDictionary<string, ComponentItemModel>();
            var accumulateCubes = new SortedDictionary<string, ComponentItemModel>();
            var accumulateShips = new List<ShipContent>();

            ResetProgress(0, _entities.Count);

            foreach (var entity in _entities)
            {
                Progress++;

                if (entity is StructureVoxelModel)
                {
                    var asteroid = (StructureVoxelModel)entity;

                    #region untouched ores (in asteroid)

                    Dictionary<string, long> details;

                    var filename = asteroid.SourceVoxelFilepath;
                    if (string.IsNullOrEmpty(filename))
                        filename = asteroid.VoxelFilepath;

                    try
                    {
                        details = MyVoxelMap.GetMaterialAssetDetails(filename);
                    }
                    catch
                    {
                        details = null;
                    }

                    // Accumulate into untouched.
                    if (details != null)
                    {
                        var ores = new Dictionary<string, long>();
                        foreach (var kvp in details)
                        {
                            var bp = SpaceEngineersCore.Resources.Definitions.VoxelMaterials.FirstOrDefault(b => b.Id.SubtypeId == kvp.Key && b.Id.TypeId == SpaceEngineersConsts.VoxelMaterialDefinition);

                            if (bp != null && bp.CanBeHarvested)
                            {
                                var cd = SpaceEngineersApi.GetDefinition(SpaceEngineersConsts.Ore, bp.MinedOre) as MyObjectBuilder_PhysicalItemDefinition;

                                if (cd != null)
                                {
                                    if (ores.ContainsKey(cd.DisplayName))
                                        ores[cd.DisplayName] += kvp.Value;
                                    else
                                        ores.Add(cd.DisplayName, kvp.Value);
                                }
                            }
                        }

                        foreach (var kvp in ores)
                        {
                            if (accumulateOres.ContainsKey(kvp.Key))
                            {
                                accumulateOres[kvp.Key] += kvp.Value;
                            }
                            else
                            {
                                accumulateOres.Add(kvp.Key, kvp.Value);
                            }
                        }


                        var oreSum = ores.Values.ToList().Sum();
                        accumulateAsteroidOres.Add(new AsteroidContent()
                        {
                            Name = Path.GetFileNameWithoutExtension(filename),
                            Position = asteroid.PositionAndOrientation.Value.Position,
                            UntouchedOreList = ores.Select(kvp => new VoxelMaterialAssetModel { MaterialName = SpaceEngineersApi.GetResourceName(kvp.Key), Volume = Math.Round((double)kvp.Value / 255, 7), Percent = kvp.Value / (double)oreSum }).ToList()
                        });
                    }

                    #endregion
                }
                else if (entity is StructureFloatingObjectModel)
                {
                    var floating = (StructureFloatingObjectModel)entity;

                    if (floating.FloatingObject.Item.Content.TypeId == SpaceEngineersConsts.Ore || floating.FloatingObject.Item.Content.TypeId == SpaceEngineersConsts.Ingot)
                    {
                        TallyItems(floating.FloatingObject.Item.Content.TypeId, floating.FloatingObject.Item.Content.SubtypeName, (decimal)floating.FloatingObject.Item.Amount, contentPath, accumulateUnusedOres, accumulateItems, accumulateComponents);
                    }
                    else
                    {
                        TallyItems(floating.FloatingObject.Item.Content.TypeId, floating.FloatingObject.Item.Content.SubtypeName, (decimal)floating.FloatingObject.Item.Amount, contentPath, accumulateUsedOres, accumulateItems, accumulateComponents);
                    }
                }
                else if (entity is StructureCharacterModel)
                {
                    var character = (StructureCharacterModel)entity;

                    // Ignore pilots, as we'll check those in the ship.
                    if (!character.IsPilot)
                    {
                        // Character inventory.
                        foreach (var item in character.Character.Inventory.Items)
                        {
                            TallyItems(item.Content.TypeId, item.Content.SubtypeName, (decimal)item.Amount, contentPath, accumulatePlayerOres, accumulateItems, accumulateComponents);
                        }
                    }
                }
                else if (entity is StructureCubeGridModel)
                {
                    var ship = entity as StructureCubeGridModel;
                    var isNpc = ship.CubeGrid.CubeBlocks.Any(e => e is MyObjectBuilder_Cockpit && ((MyObjectBuilder_Cockpit)e).Autopilot != null);

                    var shipContent = new ShipContent()
                    {
                        DisplayName = ship.DisplayName,
                        Position = ship.PositionAndOrientation.Value.Position,
                        EntityId = ship.EntityId,
                        BlockCount = ship.BlockCount
                    };

                    foreach (MyObjectBuilder_CubeBlock block in ship.CubeGrid.CubeBlocks)
                    {
                        var blockType = block.GetType();
                        var cubeBlockDefinition = SpaceEngineersApi.GetCubeDefinition(block.TypeId, ship.CubeGrid.GridSizeEnum, block.SubtypeName);
                        var blockTime = TimeSpan.Zero;
                        string blockTexture = null;
                        float cubeMass = 0;

                        // Unconstructed portion.
                        if (block.ConstructionStockpile != null && block.ConstructionStockpile.Items.Count > 0)
                        {
                            foreach (var item in block.ConstructionStockpile.Items)
                            {
                                if (isNpc)
                                {
                                    TallyItems(item.PhysicalContent.TypeId, item.PhysicalContent.SubtypeName, item.Amount, contentPath, accumulateNpcOres, null, null);
                                }
                                else
                                {
                                    TallyItems(item.PhysicalContent.TypeId, item.PhysicalContent.SubtypeName, item.Amount, contentPath, accumulateUsedOres, null, null);
                                }

                                var def = SpaceEngineersApi.GetDefinition(item.PhysicalContent.TypeId, item.PhysicalContent.SubtypeName);
                                float componentMass = 0;
                                var cd = def as MyObjectBuilder_ComponentDefinition;
                                if (cd != null)
                                {
                                    componentMass = cd.Mass * item.Amount;
                                }
                                else
                                {
                                    var pd = def as MyObjectBuilder_PhysicalItemDefinition;
                                    if (pd != null)
                                    {
                                        componentMass = pd.Mass * item.Amount;
                                    }
                                }
                                
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
                                    var cd = SpaceEngineersApi.GetDefinition(component.Type, component.Subtype) as MyObjectBuilder_ComponentDefinition;
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
                                    var cd = (MyObjectBuilder_ComponentDefinition)SpaceEngineersApi.GetDefinition(component.Type, component.Subtype);

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

                                    var componentMass = cd.Mass * component.Count;
                                    cubeMass += componentMass;
                                }
                            }

                            blockTime = new TimeSpan((long)(TimeSpan.TicksPerSecond * cubeBlockDefinition.BuildTimeSeconds * block.BuildPercent));
                            blockTexture = SpaceEngineersCore.GetDataPathOrDefault(cubeBlockDefinition.Icon, Path.Combine(contentPath, cubeBlockDefinition.Icon));
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
                                            TallyItems(item.Content.TypeId, item.Content.SubtypeName, (decimal)item.Amount, contentPath, accumulateNpcOres, accumulateItems, accumulateComponents);
                                        }
                                        else
                                        {
                                            if (item.Content.TypeId == SpaceEngineersConsts.Ore || item.Content.TypeId == SpaceEngineersConsts.Ingot)
                                            {
                                                TallyItems(item.Content.TypeId, item.Content.SubtypeName, (decimal)item.Amount, contentPath, accumulateUnusedOres, accumulateItems, accumulateComponents);
                                            }
                                            else
                                            {
                                                TallyItems(item.Content.TypeId, item.Content.SubtypeName, (decimal)item.Amount, contentPath, accumulateUsedOres, accumulateItems, accumulateComponents);
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
                                        TallyItems(item.Content.TypeId, item.Content.SubtypeName, (decimal)item.Amount, contentPath, accumulatePlayerOres, accumulateItems, accumulateComponents);
                                    }
                                }
                            }

                            #endregion
                        }

                        #region tally cubes

                        if (cubeBlockDefinition != null)
                        {
                            var itemsKey = cubeBlockDefinition.DisplayName;

                            if (accumulateCubes.ContainsKey(itemsKey))
                            {
                                accumulateCubes[itemsKey].Count += 1;
                                accumulateCubes[itemsKey].Mass += cubeMass;
                                accumulateCubes[itemsKey].Time += blockTime;
                            }
                            else
                            {
                                accumulateCubes.Add(itemsKey, new ComponentItemModel { Name = cubeBlockDefinition.DisplayName, Count = 1, Mass = cubeMass, TypeId = cubeBlockDefinition.Id.TypeId, SubtypeId = cubeBlockDefinition.Id.SubtypeName, TextureFile = blockTexture, Time = blockTime });
                            }
                        }

                        #endregion
                    }

                    accumulateShips.Add(shipContent);
                }
            }

            #region build lists

            var sum = accumulateOres.Values.ToList().Sum();
            _untouchedOre = new List<VoxelMaterialAssetModel>();

            foreach (var kvp in accumulateOres)
            {
                _untouchedOre.Add(new VoxelMaterialAssetModel { MaterialName = SpaceEngineersApi.GetResourceName(kvp.Key), Volume = Math.Round((double)kvp.Value / 255, 7), Percent = kvp.Value / (double)sum });
            }

            _untouchedOreByAsteroid = accumulateAsteroidOres;

            _unusedOre = accumulateUnusedOres.Values.ToList();
            _usedOre = accumulateUsedOres.Values.ToList();
            _playerOre = accumulatePlayerOres.Values.ToList();
            _npcOre = accumulateNpcOres.Values.ToList();
            _allCubes = accumulateCubes.Values.ToList();
            _allComponents = accumulateComponents.Values.ToList();
            _allItems = accumulateItems.Values.ToList();
            _allShips = accumulateShips;

            #endregion

            #region create report

            IsReportReady = true;

            ClearProgress();

            #endregion
        }

        #region TallyItems

        private void TallyItems(MyObjectBuilderType tallyTypeId, string tallySubTypeId, Decimal amountDecimal, string contentPath, SortedDictionary<string, OreContent> accumulateOres, SortedDictionary<string, ComponentItemModel> accumulateItems, SortedDictionary<string, ComponentItemModel> accumulateComponents)
        {
            var cd = SpaceEngineersApi.GetDefinition(tallyTypeId, tallySubTypeId) as MyObjectBuilder_PhysicalItemDefinition;

            if (cd == null)
            {
                // A component, gun, ore that doesn't exist (Depricated by KeenSH, or Mod that isn't loaded).
                return;
            }

            var componentTexture = SpaceEngineersCore.GetDataPathOrDefault(cd.Icon, Path.Combine(contentPath, cd.Icon));

            if (tallyTypeId == SpaceEngineersConsts.Ore)
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
                    accumulateOres.Add(unusedKey, new OreContent { Name = cd.DisplayName, Amount = amountDecimal, Mass = mass, Volume = volume, TextureFile = componentTexture });
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
                        accumulateItems.Add(itemsKey, new ComponentItemModel { Name = cd.DisplayName, Count = amountDecimal, Mass = mass, Volume = volume, TypeId = tallyTypeId, SubtypeId = tallySubTypeId, TextureFile = componentTexture, Time = TimeSpan.Zero });
                    }
                }

                #endregion
            }
            else if (tallyTypeId == SpaceEngineersConsts.Ingot)
            {
                var mass = Math.Round((double)amountDecimal * cd.Mass, 7);
                var volume = Math.Round((double)amountDecimal * (cd.Volume.HasValue ? cd.Volume.Value : 0), 7);
                var bp = SpaceEngineersApi.GetBlueprint(tallyTypeId, tallySubTypeId);
                var timeToMake = TimeSpan.Zero;
                
                // no blueprint, means the item is not built by players, but generated by the environment.
                if (bp != null)
                    timeToMake = new TimeSpan((long)(TimeSpan.TicksPerSecond * (decimal)bp.BaseProductionTimeInSeconds * amountDecimal));

                #region unused ore value

                var oreRequirements = new Dictionary<string, BlueprintRequirement>();
                TimeSpan ingotTime;
                SpaceEngineersApi.AccumulateCubeBlueprintRequirements(tallySubTypeId, tallyTypeId, amountDecimal, oreRequirements, out ingotTime);

                foreach (var item in oreRequirements)
                {
                    TallyItems(item.Value.Id.TypeId, item.Value.SubtypeId, item.Value.Amount, contentPath, accumulateOres, null, null);
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
                        accumulateItems.Add(itemsKey, new ComponentItemModel { Name = cd.DisplayName, Count = amountDecimal, Mass = mass, Volume = volume, TypeId = tallyTypeId, SubtypeId = tallySubTypeId, TextureFile = componentTexture, Time = timeToMake });
                    }
                }

                #endregion
            }
            else if (tallyTypeId == SpaceEngineersConsts.AmmoMagazine ||
                tallyTypeId == SpaceEngineersConsts.PhysicalGunObject)
            {
                var mass = Math.Round((double)amountDecimal * cd.Mass, 7);
                var volume = Math.Round((double)amountDecimal * (cd.Volume.HasValue ? cd.Volume.Value : 0), 7);
                var bp = SpaceEngineersApi.GetBlueprint(tallyTypeId, tallySubTypeId);
                var timeToMake = new TimeSpan((long)(TimeSpan.TicksPerSecond * (decimal)bp.BaseProductionTimeInSeconds * amountDecimal));

                #region unused ore value

                var oreRequirements = new Dictionary<string, BlueprintRequirement>();
                TimeSpan ingotTime;
                SpaceEngineersApi.AccumulateCubeBlueprintRequirements(tallySubTypeId, tallyTypeId, amountDecimal, oreRequirements, out ingotTime);

                foreach (var item in oreRequirements)
                {
                    TallyItems(item.Value.Id.TypeId, item.Value.SubtypeId, item.Value.Amount, contentPath, accumulateOres, null, accumulateComponents);
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
            else if (tallyTypeId == SpaceEngineersConsts.Component)
            {
                var mass = Math.Round((double)amountDecimal * cd.Mass, 7);
                var volume = Math.Round((double)amountDecimal * (cd.Volume.HasValue ? cd.Volume.Value : 0), 7);
                var bp = SpaceEngineersApi.GetBlueprint(tallyTypeId, tallySubTypeId);
                var timeToMake = new TimeSpan((long)(TimeSpan.TicksPerSecond * (decimal)bp.BaseProductionTimeInSeconds * amountDecimal));

                #region unused ore value

                var oreRequirements = new Dictionary<string, BlueprintRequirement>();
                TimeSpan ingotTime;
                SpaceEngineersApi.AccumulateCubeBlueprintRequirements(tallySubTypeId, tallyTypeId, amountDecimal, oreRequirements, out ingotTime);

                foreach (var item in oreRequirements)
                {
                    TallyItems(item.Value.Id.TypeId, item.Value.SubtypeId, item.Value.Amount, contentPath, accumulateOres, null, null);
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
            //else if (typeId == SpaceEngineersConsts.CubeBlock)
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

            #region In game resources

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

            #endregion

            #region In game assets

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

            #endregion

            #region Asteroid breakdown

            bld.AppendLine();
            bld.AppendFormat("Untouched Ores (Asteroids)\r\n");
            bld.AppendFormat("Asteroid\tOre Name\tVolume m³\r\n");
            foreach (var asteroid in _untouchedOreByAsteroid)
            {
                foreach (var item in asteroid.UntouchedOreList)
                {
                    bld.AppendFormat("{0}\t{1}\t{2:#,##0.000}\r\n", asteroid.Name, item.MaterialName, item.Volume);
                }
            }

            #endregion

            #region Ship breakdown

            // TODO:

            #endregion

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

                writer.BeginDocument(string.Format("Resource Report - {0}", _saveName),
                    @"
body { background-color: #F6F6FA }
b { font-family: Arial, Helvetica, sans-serif; }
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

                #region In game resources

                writer.RenderElement(HtmlTextWriterTag.H2, "In game resources");
                writer.RenderElement(HtmlTextWriterTag.P, "Everything is measured in its regressed state. Ie., how much ore was used/needed to build this item.");

                writer.RenderElement(HtmlTextWriterTag.H3, "Untouched Ore (Asteroids)");
                writer.BeginTable("1", "3", "0", new[] { "Name", "Volume m³" });
                foreach (var item in _untouchedOre)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                    writer.RenderElement(HtmlTextWriterTag.Td, item.MaterialName);
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "right");
                    writer.RenderElement(HtmlTextWriterTag.Td, "{0:#,##0.000}", item.Volume);
                    writer.RenderEndTag(); // Tr
                }
                writer.EndTable();

                writer.RenderElement(HtmlTextWriterTag.H3, "Unused Ore (Ore and Ingots, either floating or in containers)");
                writer.BeginTable("1", "3", "0", new[] { "Name", "Mass Kg", "Volume L" });
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
                writer.BeginTable("1", "3", "0", new[] { "Name", "Mass Kg", "Volume L" });
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
                writer.BeginTable("1", "3", "0", new[] { "Name", "Mass Kg", "Volume L" });
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
                writer.BeginTable("1", "3", "0", new[] { "Name", "Mass Kg", "Volume L" });
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

                #endregion

                writer.RenderElement(HtmlTextWriterTag.Br);
                writer.RenderElement(HtmlTextWriterTag.Hr);

                #region In game assets

                writer.RenderElement(HtmlTextWriterTag.H2, "In game assets");
                writer.RenderElement(HtmlTextWriterTag.P, "Counts of all current items in game Assets. These indicate actual time spent to construct, part construct or refine.");

                writer.RenderElement(HtmlTextWriterTag.H3, "All Cubes");
                writer.BeginTable("1", "3", "0", new[] { "Icon", "Name", "Count", "Mass Kg", "Time" });
                foreach (var item in _allCubes)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    if (item.TextureFile != null && File.Exists(item.TextureFile))
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
                writer.BeginTable("1", "3", "0", new[] { "Icon", "Name", "Count", "Mass Kg", "Volume L", "Time" });
                foreach (var item in _allComponents)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    if (item.TextureFile != null && File.Exists(item.TextureFile))
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
                writer.BeginTable("1", "3", "0", new[] { "Icon", "Name", "Count", "Mass Kg", "Volume L", "Time" });
                foreach (var item in _allItems)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    if (item.TextureFile != null && File.Exists(item.TextureFile))
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

                #endregion

                writer.RenderElement(HtmlTextWriterTag.Br);
                writer.RenderElement(HtmlTextWriterTag.Hr);

                writer.RenderElement(HtmlTextWriterTag.H2, "Resources breakdown");

                #region Asteroid breakdown

                writer.RenderElement(HtmlTextWriterTag.H3, "Untouched Ores (Asteroids)");
                writer.BeginTable("1", "3", "0", new[] { "Asteroid", "Position", "Ore name", "Volume m³" });
                foreach (var asteroid in _untouchedOreByAsteroid)
                {
                    var inx = 0;
                    foreach (var item in asteroid.UntouchedOreList)
                    {
                        writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                        if (inx == 0)
                        {
                            writer.AddAttribute(HtmlTextWriterAttribute.Rowspan, asteroid.UntouchedOreList.Count.ToString(CultureInfo.InvariantCulture));
                            writer.RenderElement(HtmlTextWriterTag.Td, asteroid.Name);

                            writer.AddAttribute(HtmlTextWriterAttribute.Rowspan, asteroid.UntouchedOreList.Count.ToString(CultureInfo.InvariantCulture));
                            writer.RenderElement(HtmlTextWriterTag.Td, "{0},{1},{2}", asteroid.Position.X, asteroid.Position.Y, asteroid.Position.Z);
                        }
                        writer.RenderElement(HtmlTextWriterTag.Td, item.MaterialName);
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "right");
                        writer.RenderElement(HtmlTextWriterTag.Td, "{0:#,##0.000}", item.Volume);
                        writer.RenderEndTag(); // Tr
                        inx++;
                    }
                }
                writer.EndTable();

                #endregion

                writer.RenderElement(HtmlTextWriterTag.Br);
                writer.RenderElement(HtmlTextWriterTag.Hr);

                #region Ship breakdown

                writer.BeginTable("1", "3", "0", new[] { "Ship", "EntityId", "Position", "Block Count" });
                foreach (var ship in _allShips)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                    writer.RenderElement(HtmlTextWriterTag.Td, ship.DisplayName);
                    writer.RenderElement(HtmlTextWriterTag.Td, ship.EntityId);
                    writer.RenderElement(HtmlTextWriterTag.Td, "{0},{1},{2}", ship.Position.X, ship.Position.Y, ship.Position.Z);
                    writer.RenderElement(HtmlTextWriterTag.Td, ship.BlockCount);

                    // TODO: more detail.

                    writer.RenderEndTag(); // Tr
                }
                writer.EndTable();

                #endregion

                writer.EndDocument();
            }

            return stringWriter.ToString();
        }

        #endregion

        #region CreateXmlReport

        internal string CreateXmlReport()
        {
            var settingsDestination = new XmlWriterSettings
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

                #region In game resources

                foreach (var item in _untouchedOre)
                {
                    xmlWriter.WriteStartElement("untouched");
                    xmlWriter.WriteElementFormat("orename", "{0}", item.MaterialName);
                    xmlWriter.WriteElementFormat("volume", "{0:0.000}", item.Volume);
                    xmlWriter.WriteEndElement();
                }

                foreach (var item in _unusedOre)
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

                #endregion

                #region In game assets

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

                #endregion

                #region Asteroid breakdown

                foreach (var asteroid in _untouchedOreByAsteroid)
                {
                    xmlWriter.WriteStartElement("asteroids");
                    xmlWriter.WriteAttributeString("name", asteroid.Name);

                    foreach (var item in asteroid.UntouchedOreList)
                    {
                        xmlWriter.WriteStartElement("untouched");
                        xmlWriter.WriteElementFormat("orename", "{0}", item.MaterialName);
                        xmlWriter.WriteElementFormat("volume", "{0:0.000}", item.Volume);
                        xmlWriter.WriteEndElement();
                    }

                    xmlWriter.WriteEndElement();
                }

                #endregion

                #region Ship breakdown

                // TODO:

                #endregion

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
        /// <param name="baseModel"></param>
        /// <param name="args"></param>
        public static void GenerateOfflineReport(ExplorerModel baseModel, string[] args)
        {
            var argList = args.ToList();
            var comArgs = args.Where(a => a.ToUpper() == "/WR" || a.ToUpper() == "-WR").Select(a => { return a; }).ToArray();
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

            WorldResource world;

            if (Directory.Exists(findSession))
            {
                world = SelectWorldModel.LoadSession(findSession);
            }
            else
            {
                world = SelectWorldModel.FindSaveSession(SpaceEngineersConsts.BaseLocalPath.SavesPath, findSession);
            }

            if (world == null)
            {
                Environment.Exit(3);
                return;
            }

            baseModel.ActiveWorld = world;
            baseModel.ActiveWorld.LoadDefinitionsAndMods();
            baseModel.ActiveWorld.LoadSector(true);
            baseModel.ParseSandBox();

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

        #region helper classes

        public class AsteroidContent
        {
            public string Name { get; set; }
            public Vector3D Position { get; set; }
            public long Empty { get; set; }
            public List<VoxelMaterialAssetModel> UntouchedOreList { get; set; }
        }

        public class OreContent
        {
            private string _name;

            public string Name
            {
                get { return _name; }

                set
                {
                    if (value != _name)
                    {
                        _name = value;
                        FriendlyName = SpaceEngineersApi.GetResourceName(Name);
                    }
                }
            }

            public string FriendlyName { get; set; }
            public decimal Amount { get; set; }
            public double Mass { get; set; }
            public double Volume { get; set; }
            public string TextureFile { get; set; }
        }

        public class ShipContent
        {
            public string DisplayName { get; set; }
            public Vector3D Position { get; set; }
            public long EntityId { get; set; }
            public int BlockCount { get; set; }
            //public decimal Amount { get; set; }
            //public double Mass { get; set; }
            //public double Volume { get; set; }
            //public TimeSpan Time { get; set; }
        }

        #endregion
    }
}
