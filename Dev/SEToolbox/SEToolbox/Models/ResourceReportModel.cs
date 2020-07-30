namespace SEToolbox.Models
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using SEToolbox.ImageLibrary;
    using SEToolbox.Interfaces;
    using SEToolbox.Interop;
    using SEToolbox.Interop.Asteroids;
    using SEToolbox.Support;
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
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.ObjectBuilders;
    using VRageMath;
    using Res = SEToolbox.Properties.Resources;

    public class ResourceReportModel : BaseModel
    {
        private const string CssStyle = @"
body { background-color: #F6F6FA }
b { font-family: Arial, Helvetica, sans-serif; }
p { font-family: Arial, Helvetica, sans-serif; }
h1,h2,h3 { font-family: Arial, Helvetica, sans-serif; }
table { background-color: #FFFFFF; }
table tr td { font-family: Arial, Helvetica, sans-serif; font-size: small; line-height: normal; color: #000000; }
table thead td { background-color: #BABDD6; font-weight: bold; Color: #000000; }
td.right { text-align: right; }";

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

        private decimal _totalCubes;

        private int _totalPCU;

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
                    OnPropertyChanged(nameof(IsBusy));
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
                    OnPropertyChanged(nameof(IsActive));
                }
            }
        }

        public string SaveName => _saveName;

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
                    OnPropertyChanged(nameof(IsReportReady));
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
                    OnPropertyChanged(nameof(ReportHtml));
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
                    OnPropertyChanged(nameof(ShowProgress));
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
                        OnPropertyChanged(nameof(Progress));
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
                    OnPropertyChanged(nameof(MaximumProgress));
                }
            }
        }

        #endregion

        #region methods

        internal static ReportType GetReportType(string reportExtension)
        {
            reportExtension = reportExtension?.ToUpper() ?? string.Empty;

            if (reportExtension == ".TXT")
                return ReportType.Text;

            if (reportExtension == ".HTM" || reportExtension == ".HTML")
                return ReportType.Html;

            if (reportExtension == ".XML")
                return ReportType.Xml;

            return ReportType.Unknown;
        }

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
            _totalCubes = 0;
            _totalPCU = 0;

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
                            var bp = SpaceEngineersCore.Resources.VoxelMaterialDefinitions.FirstOrDefault(b => b.Id.SubtypeName == kvp.Key && b.Id.TypeId == SpaceEngineersTypes.VoxelMaterialDefinition);

                            if (bp != null && bp.CanBeHarvested)
                            {
                                var cd = MyDefinitionManager.Static.GetDefinition(SpaceEngineersTypes.Ore, bp.MinedOre);

                                if (cd != null)
                                {
                                    // stock ores require DisplayNameEnum. Modded ores require DisplayNameString.
                                    string key = cd.DisplayNameEnum != null ? cd.DisplayNameEnum.Value.String : cd.DisplayNameString;

                                    if (ores.ContainsKey(key))
                                        ores[key] += kvp.Value;
                                    else
                                        ores.Add(key, kvp.Value);
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

                    if (floating.FloatingObject.Item.PhysicalContent.TypeId == SpaceEngineersTypes.Ore || floating.FloatingObject.Item.PhysicalContent.TypeId == SpaceEngineersTypes.Ingot)
                    {
                        TallyItems(floating.FloatingObject.Item.PhysicalContent.TypeId, floating.FloatingObject.Item.PhysicalContent.SubtypeName, (decimal)floating.FloatingObject.Item.Amount, contentPath, accumulateUnusedOres, accumulateItems, accumulateComponents);
                    }
                    else
                    {
                        TallyItems(floating.FloatingObject.Item.PhysicalContent.TypeId, floating.FloatingObject.Item.PhysicalContent.SubtypeName, (decimal)floating.FloatingObject.Item.Amount, contentPath, accumulateUsedOres, accumulateItems, accumulateComponents);
                    }
                }
                else if (entity is StructureCharacterModel)
                {
                    var character = (StructureCharacterModel)entity;

                    // Ignore pilots, as we'll check those in the ship.
                    if (!character.IsPilot)
                    {
                        // Character inventory.
                        if (character.Character.Inventory != null)
                        {
                            foreach (var item in character.Character.Inventory.Items)
                            {
                                TallyItems(item.PhysicalContent.TypeId, item.PhysicalContent.SubtypeName, (decimal)item.Amount, contentPath, accumulatePlayerOres, accumulateItems, accumulateComponents);
                            }
                        }
                    }
                }
                else if (entity is StructureCubeGridModel)
                {
                    var ship = entity as StructureCubeGridModel;
                    var isNpc = ship.CubeGrid.CubeBlocks.Any(e => e is MyObjectBuilder_Cockpit && ((MyObjectBuilder_Cockpit)e).Autopilot != null);

                    int pcuToProduce = 0;

                    foreach (var block in ship.CubeGrid.CubeBlocks)
                    {
                        var cubeBlockDefinition = SpaceEngineersApi.GetCubeDefinition(block.TypeId, ship.CubeGrid.GridSizeEnum, block.SubtypeName);
                        if (cubeBlockDefinition != null)
                            pcuToProduce += cubeBlockDefinition.PCU;
                    }

                    var shipContent = new ShipContent()
                    {
                        DisplayName = ship.DisplayName,
                        Position = ship.PositionAndOrientation.Value.Position,
                        EntityId = ship.EntityId,
                        BlockCount = ship.BlockCount,
                        PCU = pcuToProduce
                    };

                    foreach (MyObjectBuilder_CubeBlock block in ship.CubeGrid.CubeBlocks)
                    {
                        var blockType = block.GetType();
                        var cubeBlockDefinition = SpaceEngineersApi.GetCubeDefinition(block.TypeId, ship.CubeGrid.GridSizeEnum, block.SubtypeName);
                        var blockTime = TimeSpan.Zero;
                        string blockTexture = null;
                        float cubeMass = 0;
                        int pcu = 0;

                        // Unconstructed portion.
                        if (block.ConstructionStockpile != null && block.ConstructionStockpile.Items.Length > 0)
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

                                var def = MyDefinitionManager.Static.GetDefinition(item.PhysicalContent.TypeId, item.PhysicalContent.SubtypeName);
                                float componentMass = 0;
                                var cd = def as MyComponentDefinition;
                                if (cd != null)
                                {
                                    componentMass = cd.Mass * item.Amount;
                                }
                                else
                                {
                                    var pd = def as MyPhysicalItemDefinition;
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
                                var componentList = new List<MyComponentDefinition>();

                                foreach (var component in cubeBlockDefinition.Components)
                                {
                                    for (var i = 0; i < component.Count; i++)
                                        componentList.Add(component.Definition);
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
                                    var cd = (MyComponentDefinition)MyDefinitionManager.Static.GetDefinition(component.Definition.Id);

                                    #region used ore value

                                    if (isNpc)
                                    {
                                        TallyItems(component.Definition.Id.TypeId, component.Definition.Id.SubtypeName, component.Count, contentPath, accumulateNpcOres, null, null);
                                    }
                                    else
                                    {
                                        TallyItems(component.Definition.Id.TypeId, component.Definition.Id.SubtypeName, component.Count, contentPath, accumulateUsedOres, null, null);
                                    }

                                    #endregion

                                    var componentMass = cd.Mass * component.Count;
                                    cubeMass += componentMass;
                                }
                            }

                            blockTime = TimeSpan.FromSeconds(cubeBlockDefinition.IntegrityPointsPerSec != 0 ? cubeBlockDefinition.MaxIntegrity / cubeBlockDefinition.IntegrityPointsPerSec * block.BuildPercent : 0);
                            blockTexture = (cubeBlockDefinition.Icons == null || cubeBlockDefinition.Icons.First() == null) ? null : SpaceEngineersCore.GetDataPathOrDefault(cubeBlockDefinition.Icons.First(), Path.Combine(contentPath, cubeBlockDefinition.Icons.First()));
                            pcu = cubeBlockDefinition.PCU;
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
                                            TallyItems(item.PhysicalContent.TypeId, item.PhysicalContent.SubtypeName, (decimal)item.Amount, contentPath, accumulateNpcOres, accumulateItems, accumulateComponents);
                                        }
                                        else
                                        {
                                            if (item.PhysicalContent.TypeId == SpaceEngineersTypes.Ore || item.PhysicalContent.TypeId == SpaceEngineersTypes.Ingot)
                                            {
                                                TallyItems(item.PhysicalContent.TypeId, item.PhysicalContent.SubtypeName, (decimal)item.Amount, contentPath, accumulateUnusedOres, accumulateItems, accumulateComponents);
                                            }
                                            else
                                            {
                                                TallyItems(item.PhysicalContent.TypeId, item.PhysicalContent.SubtypeName, (decimal)item.Amount, contentPath, accumulateUsedOres, accumulateItems, accumulateComponents);
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
                                        TallyItems(item.PhysicalContent.TypeId, item.PhysicalContent.SubtypeName, (decimal)item.Amount, contentPath, accumulatePlayerOres, accumulateItems, accumulateComponents);
                                    }
                                }
                            }

                            #endregion
                        }

                        #region tally cubes

                        if (cubeBlockDefinition != null)
                        {
                            var itemsKey = cubeBlockDefinition.DisplayNameText;
                            _totalCubes += 1;
                            _totalPCU += pcu;

                            if (accumulateCubes.ContainsKey(itemsKey))
                            {
                                accumulateCubes[itemsKey].Count += 1;
                                accumulateCubes[itemsKey].Mass += cubeMass;
                                accumulateCubes[itemsKey].Time += blockTime;
                                accumulateCubes[itemsKey].PCU += pcu;
                            }
                            else
                            {
                                accumulateCubes.Add(itemsKey, new ComponentItemModel { Name = cubeBlockDefinition.DisplayNameText, Count = 1, Mass = cubeMass, TypeId = cubeBlockDefinition.Id.TypeId, SubtypeId = cubeBlockDefinition.Id.SubtypeName, TextureFile = blockTexture, Time = blockTime, PCU = pcu });
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
            var cd = MyDefinitionManager.Static.GetDefinition(tallyTypeId, tallySubTypeId) as MyPhysicalItemDefinition;

            if (cd == null)
            {
                // A component, gun, ore that doesn't exist (Depricated by KeenSH, or Mod that isn't loaded).
                return;
            }

            var componentTexture = SpaceEngineersCore.GetDataPathOrDefault(cd.Icons.First(), Path.Combine(contentPath, cd.Icons.First()));

            if (tallyTypeId == SpaceEngineersTypes.Ore)
            {
                var mass = Math.Round((double)amountDecimal * cd.Mass, 7);
                var volume = Math.Round((double)amountDecimal * cd.Volume * SpaceEngineersConsts.VolumeMultiplyer, 7);

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
                    accumulateOres.Add(unusedKey, new OreContent { Name = cd.DisplayNameText, Amount = amountDecimal, Mass = mass, Volume = volume, TextureFile = componentTexture });
                }

                #endregion

                #region tally items

                if (accumulateItems != null)
                {
                    var itemsKey = cd.DisplayNameText;
                    if (accumulateItems.ContainsKey(itemsKey))
                    {
                        accumulateItems[itemsKey].Count += amountDecimal;
                        accumulateItems[itemsKey].Mass += mass;
                        accumulateItems[itemsKey].Volume += volume;
                    }
                    else
                    {
                        accumulateItems.Add(itemsKey, new ComponentItemModel { Name = cd.DisplayNameText, Count = amountDecimal, Mass = mass, Volume = volume, TypeId = tallyTypeId, SubtypeId = tallySubTypeId, TextureFile = componentTexture, Time = TimeSpan.Zero });
                    }
                }

                #endregion
            }
            else if (tallyTypeId == SpaceEngineersTypes.Ingot)
            {
                var mass = Math.Round((double)amountDecimal * cd.Mass, 7);
                var volume = Math.Round((double)amountDecimal * cd.Volume * SpaceEngineersConsts.VolumeMultiplyer, 7);
                var bp = SpaceEngineersApi.GetBlueprint(tallyTypeId, tallySubTypeId);
                var timeToMake = TimeSpan.Zero;

                // no blueprint, means the item is not built by players, but generated by the environment.
                if (bp != null && bp.Results != null && bp.Results.Length != 0)
                {
                    timeToMake = TimeSpan.FromSeconds(bp.BaseProductionTimeInSeconds * (double)amountDecimal / (double)bp.Results[0].Amount);
                }

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
                    var itemsKey = cd.DisplayNameText;
                    if (accumulateItems.ContainsKey(itemsKey))
                    {
                        accumulateItems[itemsKey].Count += amountDecimal;
                        accumulateItems[itemsKey].Mass += mass;
                        accumulateItems[itemsKey].Volume += volume;
                        accumulateItems[itemsKey].Time += timeToMake;
                    }
                    else
                    {
                        accumulateItems.Add(itemsKey, new ComponentItemModel { Name = cd.DisplayNameText, Count = amountDecimal, Mass = mass, Volume = volume, TypeId = tallyTypeId, SubtypeId = tallySubTypeId, TextureFile = componentTexture, Time = timeToMake });
                    }
                }

                #endregion
            }
            else if (tallyTypeId == SpaceEngineersTypes.AmmoMagazine ||
                tallyTypeId == SpaceEngineersTypes.PhysicalGunObject ||
                tallyTypeId == SpaceEngineersTypes.OxygenContainerObject)
            {
                var mass = Math.Round((double)amountDecimal * cd.Mass, 7);
                var volume = Math.Round((double)amountDecimal * cd.Volume * SpaceEngineersConsts.VolumeMultiplyer, 7);
                var bp = SpaceEngineersApi.GetBlueprint(tallyTypeId, tallySubTypeId);
                var timeToMake = TimeSpan.FromSeconds(bp == null ? 0 : bp.BaseProductionTimeInSeconds * (double)amountDecimal);

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
                    var itemsKey = cd.DisplayNameText;
                    if (accumulateItems.ContainsKey(itemsKey))
                    {
                        accumulateItems[itemsKey].Count += amountDecimal;
                        accumulateItems[itemsKey].Mass += mass;
                        accumulateItems[itemsKey].Volume += volume;
                        accumulateItems[itemsKey].Time += timeToMake;
                    }
                    else
                    {
                        accumulateItems.Add(itemsKey, new ComponentItemModel() { Name = cd.DisplayNameText, Count = amountDecimal, Mass = mass, Volume = volume, TypeId = tallyTypeId, SubtypeId = tallySubTypeId, TextureFile = componentTexture, Time = timeToMake });
                    }
                }

                #endregion
            }
            else if (tallyTypeId == SpaceEngineersTypes.Component)
            {
                var mass = Math.Round((double)amountDecimal * cd.Mass, 7);
                var volume = Math.Round((double)amountDecimal * cd.Volume * SpaceEngineersConsts.VolumeMultiplyer, 7);
                var bp = SpaceEngineersApi.GetBlueprint(tallyTypeId, tallySubTypeId);
                var timeToMake = new TimeSpan();

                // mod provides no blueprint for component.
                if (bp != null)
                    timeToMake = TimeSpan.FromSeconds(bp.BaseProductionTimeInSeconds * (double)amountDecimal);

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
                    var itemsKey = cd.DisplayNameText;
                    if (accumulateComponents.ContainsKey(itemsKey))
                    {
                        accumulateComponents[itemsKey].Count += amountDecimal;
                        accumulateComponents[itemsKey].Mass += mass;
                        accumulateComponents[itemsKey].Volume += volume;
                        accumulateComponents[itemsKey].Time += timeToMake;
                    }
                    else
                    {
                        accumulateComponents.Add(itemsKey, new ComponentItemModel() { Name = cd.DisplayNameText, Count = amountDecimal, Mass = mass, Volume = volume, TypeId = tallyTypeId, SubtypeId = tallySubTypeId, TextureFile = componentTexture, Time = timeToMake });
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

        #region CreateReport

        internal string CreateReport(ReportType reportType)
        {
            switch (reportType)
            {
                case ReportType.Text: return CreateTextReport();
                case ReportType.Html: return CreateHtmlReport();
                case ReportType.Xml: return CreateXmlReport();
            }
            return string.Empty;
        }

        #region CreateTextReport

        internal string CreateTextReport()
        {
            var bld = new StringBuilder();

            bld.AppendLine(Res.ClsReportTitle);
            bld.AppendFormat("{0} {1}\r\n", Res.ClsReportSaveWorld, _saveName);
            bld.AppendFormat("{0} {1}\r\n", Res.ClsReportDate, _generatedDate);
            bld.AppendLine();

            #region In game resources

            bld.AppendLine(Res.ClsReportHeaderInGameResources);
            bld.AppendLine(Res.ClsReportTextInGameResources);
            bld.AppendLine();

            bld.AppendFormat("{0}\r\n", Res.ClsReportHeadingUntouchedOre);
            bld.AppendFormat("{0}\t{1}\r\n", Res.ClsReportColMaterialName, Res.ClsReportColVolume + " " + Res.GlobalSIVolumeCubicMetre);
            foreach (var item in _untouchedOre)
            {
                bld.AppendFormat("{0}\t{1:#,##0.000}\r\n", item.MaterialName, item.Volume);
            }

            bld.AppendLine();
            bld.AppendLine(Res.ClsReportHeaderUnusedUnrefinedOre);
            bld.AppendFormat("{0}\t{1}\t{2}\r\n", Res.ClsReportColOreName, Res.ClsReportColMass + " " + Res.GlobalSIMassKilogram, Res.ClsReportColVolume + " " + Res.GlobalSIVolumeLitre);
            foreach (var item in _unusedOre)
            {
                bld.AppendFormat("{0}\t{1:#,##0.000}\t{2:#,##0.000}\r\n", item.FriendlyName, item.Mass, item.Volume);
            }

            bld.AppendLine();
            bld.AppendLine(Res.ClsReportHeaderUnusedRefinerdOre);
            bld.AppendFormat("{0}\t{1}\t{2}\r\n", Res.ClsReportColOreName, Res.ClsReportColMass + " " + Res.GlobalSIMassKilogram, Res.ClsReportColVolume + " " + Res.GlobalSIVolumeLitre);
            foreach (var item in _usedOre)
            {
                bld.AppendFormat("{0}\t{1:#,##0.000}\t{2:#,##0.000}\r\n", item.FriendlyName, item.Mass, item.Volume);
            }

            bld.AppendLine();
            bld.AppendLine(Res.ClsReportHeaderUsedPlayerOre);
            bld.AppendFormat("{0}\t{1}\t{2}\r\n", Res.ClsReportColOreName, Res.ClsReportColMass + " " + Res.GlobalSIMassKilogram, Res.ClsReportColVolume + " " + Res.GlobalSIVolumeLitre);
            foreach (var item in _playerOre)
            {
                bld.AppendFormat("{0}\t{1:#,##0.000}\t{2:#,##0.000}\r\n", item.FriendlyName, item.Mass, item.Volume);
            }

            bld.AppendLine();
            bld.AppendLine(Res.ClsReportHeaderUsedNpcOre);
            bld.AppendFormat("{0}\t{1}\t{2}\r\n", Res.ClsReportColOreName, Res.ClsReportColMass + " " + Res.GlobalSIMassKilogram, Res.ClsReportColVolume + " " + Res.GlobalSIVolumeLitre);
            foreach (var item in _npcOre)
            {
                bld.AppendFormat("{0}\t{1:#,##0.000}\t{2:#,##0.000}\r\n", item.FriendlyName, item.Mass, item.Volume);
            }

            #endregion

            #region In game assets

            bld.AppendLine();
            bld.AppendLine(Res.ClsReportHeaderInGameAssets);
            bld.AppendLine(Res.ClsReportTextInGameAssets);

            bld.AppendLine();
            bld.AppendLine(Res.ClsReportHeaderTotalCubes);
            bld.AppendLine($"{Res.ClsReportColCount}\t{Res.ClsReportColPCU}");
            bld.AppendLine($"{_totalCubes:#,##0}\t{_totalPCU:#,##0}");

            bld.AppendLine();
            bld.AppendLine(Res.ClsReportHeaderAllCubes);
            bld.AppendLine($"{Res.ClsReportColCubeName}\t{Res.ClsReportColCount}\t{Res.ClsReportColMass} {Res.GlobalSIMassKilogram}\t{Res.ClsReportColTime}\t{Res.ClsReportColPCU}");
            foreach (var item in _allCubes)
            {
                bld.AppendLine($"{item.FriendlyName}\t{item.Count:#,##0}\t{item.Mass:#,##0.000}\t{item.Time}\t{item.PCU:#,##0}");
            }

            bld.AppendLine();
            bld.AppendLine(Res.ClsReportHeaderAllComponents);
            bld.AppendFormat("{0}\t{1}\t{2}\t{3}\t{4}\r\n", Res.ClsReportColComponentName, Res.ClsReportColCount, Res.ClsReportColMass + " " + Res.GlobalSIMassKilogram, Res.ClsReportColVolume + " " + Res.GlobalSIVolumeLitre, Res.ClsReportColTime);
            foreach (var item in _allComponents)
            {
                bld.AppendFormat("{0}\t{1:#,##0}\t{2:#,##0.000}\t{3:#,##0.000}\t{4}\r\n", item.FriendlyName, item.Count, item.Mass, item.Volume, item.Time);
            }

            bld.AppendLine();
            bld.AppendLine(Res.ClsReportHeaderAllItems);
            bld.AppendFormat("{0}\t{1}\t{2}\t{3}\t{4}\r\n", Res.ClsReportColAllItemsName, Res.ClsReportColCount, Res.ClsReportColMass + " " + Res.GlobalSIMassKilogram, Res.ClsReportColVolume + " " + Res.GlobalSIVolumeLitre, Res.ClsReportColTime);
            foreach (var item in _allItems)
            {
                bld.AppendFormat("{0}\t{1:#,##0}\t{2:#,##0.000}\t{3:#,##0.000}\t{4}\r\n", item.FriendlyName, item.Count, item.Mass, item.Volume, item.Time);
            }

            #endregion

            #region Asteroid breakdown

            bld.AppendLine();
            bld.AppendFormat("{0}\r\n", Res.ClsReportHeadingUntouchedOre);
            bld.AppendFormat("{0}\t{1}\t{2}\r\n", Res.ClsReportColAsteroid, Res.ClsReportColOreName, Res.ClsReportColVolume + " " + Res.GlobalSIVolumeCubicMetre);
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

                writer.BeginDocument($"{Res.ClsReportTitle} - {_saveName}", CssStyle);

                #endregion

                writer.RenderElement(HtmlTextWriterTag.H1, Res.ClsReportTitle);

                writer.RenderElement(HtmlTextWriterTag.P, "{0} {1}", Res.ClsReportSaveWorld, _saveName);
                writer.RenderElement(HtmlTextWriterTag.P, "{0} {1}", Res.ClsReportDate, _generatedDate);

                #region In game resources

                writer.RenderElement(HtmlTextWriterTag.H2, Res.ClsReportHeaderInGameResources);
                writer.RenderElement(HtmlTextWriterTag.P, Res.ClsReportTextInGameResources);

                writer.RenderElement(HtmlTextWriterTag.H3, Res.ClsReportHeadingUntouchedOre);
                writer.BeginTable("1", "3", "0", new[] { Res.ClsReportColMaterialName, Res.ClsReportColVolume + " " + Res.GlobalSIVolumeCubicMetre });
                foreach (var item in _untouchedOre)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                    writer.RenderElement(HtmlTextWriterTag.Td, item.MaterialName);
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "right");
                    writer.RenderElement(HtmlTextWriterTag.Td, "{0:#,##0.000}", item.Volume);
                    writer.RenderEndTag(); // Tr
                }
                writer.EndTable();

                writer.RenderElement(HtmlTextWriterTag.H3, Res.ClsReportHeaderUnusedUnrefinedOre);
                writer.BeginTable("1", "3", "0", new[] { Res.ClsReportColOreName, Res.ClsReportColMass + " " + Res.GlobalSIMassKilogram, Res.ClsReportColVolume + " " + Res.GlobalSIVolumeLitre });

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

                writer.RenderElement(HtmlTextWriterTag.H3, Res.ClsReportHeaderUnusedRefinerdOre);
                writer.BeginTable("1", "3", "0", new[] { Res.ClsReportColOreName, Res.ClsReportColMass + " " + Res.GlobalSIMassKilogram, Res.ClsReportColVolume + " " + Res.GlobalSIVolumeLitre });
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

                writer.RenderElement(HtmlTextWriterTag.H3, Res.ClsReportHeaderUsedPlayerOre);
                writer.BeginTable("1", "3", "0", new[] { Res.ClsReportColOreName, Res.ClsReportColMass + " " + Res.GlobalSIMassKilogram, Res.ClsReportColVolume + " " + Res.GlobalSIVolumeLitre });
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

                writer.RenderElement(HtmlTextWriterTag.H3, Res.ClsReportHeaderUsedNpcOre);
                writer.BeginTable("1", "3", "0", new[] { Res.ClsReportColOreName, Res.ClsReportColMass + " " + Res.GlobalSIMassKilogram, Res.ClsReportColVolume + " " + Res.GlobalSIVolumeLitre });
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

                writer.RenderElement(HtmlTextWriterTag.H2, Res.ClsReportHeaderInGameAssets);
                writer.RenderElement(HtmlTextWriterTag.P, Res.ClsReportTextInGameAssets);

                writer.RenderElement(HtmlTextWriterTag.H3, Res.ClsReportHeaderTotalCubes);
                writer.BeginTable("1", "3", "0", new[] { Res.ClsReportColCount, Res.ClsReportColPCU });
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "right");
                writer.RenderElement(HtmlTextWriterTag.Td, "{0:#,##0}", _totalCubes);
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "right");
                writer.RenderElement(HtmlTextWriterTag.Td, "{0:#,##0}", _totalPCU);
                writer.RenderEndTag(); // Tr
                writer.EndTable();

                writer.RenderElement(HtmlTextWriterTag.H3, Res.ClsReportHeaderAllCubes);
                writer.BeginTable("1", "3", "0", new[] { Res.ClsReportColIcon, Res.ClsReportColCubeName, Res.ClsReportColCount, Res.ClsReportColMass + " " + Res.GlobalSIMassKilogram, Res.ClsReportColTime, Res.ClsReportColPCU });
                foreach (var item in _allCubes)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    if (item.TextureFile != null)
                    {
                        string texture = GetTextureToBase64(item.TextureFile, 32, 32);
                        if (!string.IsNullOrEmpty(texture))
                        {
                            writer.AddAttribute(HtmlTextWriterAttribute.Src, "data:image/png;base64," + texture);
                            writer.AddAttribute(HtmlTextWriterAttribute.Width, "32");
                            writer.AddAttribute(HtmlTextWriterAttribute.Height, "32");
                            writer.AddAttribute(HtmlTextWriterAttribute.Alt, Path.GetFileNameWithoutExtension(item.TextureFile));
                            writer.RenderBeginTag(HtmlTextWriterTag.Img);
                            writer.RenderEndTag();
                        }
                    }
                    writer.RenderEndTag(); // Td

                    writer.RenderElement(HtmlTextWriterTag.Td, item.FriendlyName);
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "right");
                    writer.RenderElement(HtmlTextWriterTag.Td, "{0:#,##0}", item.Count);
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "right");
                    writer.RenderElement(HtmlTextWriterTag.Td, "{0:#,##0.000}", item.Mass);
                    writer.RenderElement(HtmlTextWriterTag.Td, "{0}", item.Time);
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "right");
                    writer.RenderElement(HtmlTextWriterTag.Td, "{0:#,##0}", item.PCU);
                    writer.RenderEndTag(); // Tr
                }
                writer.EndTable();

                writer.RenderElement(HtmlTextWriterTag.H3, Res.ClsReportHeaderAllComponents);
                writer.BeginTable("1", "3", "0", new[] { Res.ClsReportColIcon, Res.ClsReportColComponentName, Res.ClsReportColCount, Res.ClsReportColMass + " " + Res.GlobalSIMassKilogram, Res.ClsReportColVolume + " " + Res.GlobalSIVolumeLitre, Res.ClsReportColTime });
                foreach (var item in _allComponents)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    if (item.TextureFile != null)
                    {
                        string texture = GetTextureToBase64(item.TextureFile, 32, 32);
                        if (!string.IsNullOrEmpty(texture))
                        {
                            writer.AddAttribute(HtmlTextWriterAttribute.Src, "data:image/png;base64," + texture);
                            writer.AddAttribute(HtmlTextWriterAttribute.Width, "32");
                            writer.AddAttribute(HtmlTextWriterAttribute.Height, "32");
                            writer.AddAttribute(HtmlTextWriterAttribute.Alt, Path.GetFileNameWithoutExtension(item.TextureFile));
                            writer.RenderBeginTag(HtmlTextWriterTag.Img);
                            writer.RenderEndTag();
                        }
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

                writer.RenderElement(HtmlTextWriterTag.H3, Res.ClsReportHeaderAllItems);
                writer.BeginTable("1", "3", "0", new[] { Res.ClsReportColIcon, Res.ClsReportColAllItemsName, Res.ClsReportColCount, Res.ClsReportColMass + " " + Res.GlobalSIMassKilogram, Res.ClsReportColVolume + " " + Res.GlobalSIVolumeLitre, Res.ClsReportColTime });
                foreach (var item in _allItems)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    if (item.TextureFile != null)
                    {
                        string texture = GetTextureToBase64(item.TextureFile, 32, 32);
                        if (!string.IsNullOrEmpty(texture))
                        {
                            writer.AddAttribute(HtmlTextWriterAttribute.Src, "data:image/png;base64," + texture);
                            writer.AddAttribute(HtmlTextWriterAttribute.Width, "32");
                            writer.AddAttribute(HtmlTextWriterAttribute.Height, "32");
                            writer.AddAttribute(HtmlTextWriterAttribute.Alt, Path.GetFileNameWithoutExtension(item.TextureFile));
                            writer.RenderBeginTag(HtmlTextWriterTag.Img);
                            writer.RenderEndTag();
                        }
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

                writer.RenderElement(HtmlTextWriterTag.H2, Res.ClsReportHeadingResourcesBreakdown);

                #region Asteroid breakdown

                writer.RenderElement(HtmlTextWriterTag.H3, Res.ClsReportHeadingUntouchedOre);
                writer.BeginTable("1", "3", "0", new[] { Res.ClsReportColAsteroid, Res.ClsReportColPosition, Res.ClsReportColOreName, Res.ClsReportColVolume + " " + Res.GlobalSIVolumeCubicMetre });
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

                writer.BeginTable("1", "3", "0", new[] { Res.ClsReportColShip, Res.ClsReportColEntityId, Res.ClsReportColPosition, Res.ClsReportColBlockCount, Res.ClsReportColPCU });
                foreach (var ship in _allShips)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                    writer.RenderElement(HtmlTextWriterTag.Td, ship.DisplayName);
                    writer.RenderElement(HtmlTextWriterTag.Td, ship.EntityId);
                    writer.RenderElement(HtmlTextWriterTag.Td, "{0},{1},{2}", ship.Position.X, ship.Position.Y, ship.Position.Z);

                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "right");
                    writer.RenderElement(HtmlTextWriterTag.Td, ship.BlockCount);

                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "right");
                    writer.RenderElement(HtmlTextWriterTag.Td, "{0:#,##0}", ship.PCU);


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
                xmlWriter.WriteAttributeString("title", Res.ClsReportTitle);
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
                    xmlWriter.WriteElementFormat("pcu", "{0}", item.PCU);
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

        #region CreateErrorReport

        internal string CreateErrorReport(ReportType reportType, string errorContent)
        {
            _generatedDate = DateTime.Now;
            switch (reportType)
            {
                case ReportType.Text: return CreateTextErrorReport(errorContent);
                case ReportType.Html: return CreateHtmlErrorReport(errorContent);
                case ReportType.Xml: return CreateXmlErrorReport(errorContent);
            }
            return string.Empty;
        }

        internal string CreateTextErrorReport(string errorContent)
        {
            var bld = new StringBuilder();

            bld.AppendLine(Res.ClsReportTitle);
            bld.AppendFormat("{0} {1}\r\n", Res.ClsReportDate, _generatedDate);
            bld.AppendFormat("{0} {1}\r\n", Res.ClsReportError, errorContent);
            bld.AppendLine();
            return bld.ToString();
        }

        internal string CreateHtmlErrorReport(string errorContent)
        {
            var stringWriter = new StringWriter();

            // Put HtmlTextWriter in using block because it needs to call Dispose.
            using (var writer = new HtmlTextWriter(stringWriter))
            {
                #region header

                writer.BeginDocument($"{Res.ClsReportTitle} - {_saveName}", CssStyle);

                #endregion

                writer.RenderElement(HtmlTextWriterTag.H1, Res.ClsReportTitle);

                writer.RenderElement(HtmlTextWriterTag.P, "{0} {1}", Res.ClsReportDate, _generatedDate);
                writer.RenderElement(HtmlTextWriterTag.P, "{0} {1}", Res.ClsReportError, errorContent);

                writer.EndDocument();
            }

            return stringWriter.ToString();
        }

        internal string CreateXmlErrorReport(string errorContent)
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
                xmlWriter.WriteAttributeString("title", Res.ClsReportTitle);
                xmlWriter.WriteAttributeFormat("date", "{0:o}", _generatedDate);
                xmlWriter.WriteAttributeString("error", errorContent);

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
                // this terminates the application.
                Environment.Exit(2);
            }

            var findSession = argList[0].ToUpper();
            var reportFile = argList[1];
            ReportType reportType = GetReportType(Path.GetExtension(reportFile));

            if (reportType == ReportType.Unknown)
            {
                // this terminates the application.
                Environment.Exit(1);
            }

            if (File.Exists(findSession))
            {
                findSession = Path.GetDirectoryName(findSession);
            }

            var model = new ResourceReportModel();
            WorldResource world;
            string errorInformation;

            if (Directory.Exists(findSession))
            {
                if (!SelectWorldModel.LoadSession(findSession, out world, out errorInformation))
                {
                    // Cannot write out to Console because app is WinForm.
                    // Known workarounds do not work with Windows 8/10.

                    File.WriteAllText(reportFile, model.CreateErrorReport(reportType, errorInformation));

                    // this terminates the application.
                    Environment.Exit(3);
                }
            }
            else
            {
                if (!SelectWorldModel.FindSaveSession(SpaceEngineersConsts.BaseLocalPath.SavesPath, findSession, out world, out errorInformation))
                {
                    // Cannot write out to Console because app is WinForm.
                    // Known workarounds do not work with Windows 8/10.

                    File.WriteAllText(reportFile, model.CreateErrorReport(reportType, errorInformation));

                    // this terminates the application.
                    Environment.Exit(3);
                }
            }

            baseModel.ActiveWorld = world;
            baseModel.ActiveWorld.LoadDefinitionsAndMods();
            if (!baseModel.ActiveWorld.LoadSector(out errorInformation, true))
            {
                File.WriteAllText(reportFile, model.CreateErrorReport(reportType, errorInformation));

                // this terminates the application.
                Environment.Exit(3);
            }
            baseModel.ParseSandBox();

            model.Load(baseModel.ActiveWorld.Savename, baseModel.Structures);
            model.GenerateReport();
            if (VRage.Plugins.MyPlugins.Loaded)
            {
                VRage.Plugins.MyPlugins.Unload();
            }
            TempfileUtil.Dispose();

            File.WriteAllText(reportFile, model.CreateReport(reportType));

            // no errors returned.
            Environment.Exit(0);
        }

        #endregion

        private static string GetTextureToBase64(string filename, int width, int height, bool ignoreAlpha = false)
        {
            using (Stream stream = MyFileSystem.OpenRead(filename))
            {
                return ImageTextureUtil.GetTextureToBase64(stream, filename, width, height, ignoreAlpha);
            }
        }

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
            public int PCU { get; set; }
            //public decimal Amount { get; set; }
            //public double Mass { get; set; }
            //public double Volume { get; set; }
            //public TimeSpan Time { get; set; }
        }

        #endregion
    }
}
