namespace SEToolbox.Models
{
    using SEToolbox.Converters;
    using SEToolbox.ImageLibrary;
    using SEToolbox.Interop;
    using SEToolbox.Support;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Web.UI;
    using VRage;
    using VRage.FileSystem;
    using VRage.Game;
    using Res = SEToolbox.Properties.Resources;

    public class ComponentListModel : BaseModel
    {
        #region Fields

        private ObservableCollection<ComponentItemModel> _cubeAssets;

        private ObservableCollection<ComponentItemModel> _componentAssets;

        private ObservableCollection<ComponentItemModel> _itemAssets;

        private ObservableCollection<ComponentItemModel> _materialAssets;

        private bool _isBusy;

        private ComponentItemModel _selectedCubeAsset;

        #endregion

        #region Properties

        /// <summary>
        /// This is detail of the breakdown of cubes in the ship.
        /// </summary>
        public ObservableCollection<ComponentItemModel> CubeAssets
        {
            get
            {
                return _cubeAssets;
            }

            set
            {
                if (value != _cubeAssets)
                {
                    _cubeAssets = value;
                    OnPropertyChanged(nameof(CubeAssets));
                }
            }
        }

        /// <summary>
        /// This is detail of the breakdown of components in the ship.
        /// </summary>
        public ObservableCollection<ComponentItemModel> ComponentAssets
        {
            get
            {
                return _componentAssets;
            }

            set
            {
                if (value != _componentAssets)
                {
                    _componentAssets = value;
                    OnPropertyChanged(nameof(ComponentAssets));
                }
            }
        }

        /// <summary>
        /// This is detail of the breakdown of items.
        /// </summary>
        public ObservableCollection<ComponentItemModel> ItemAssets
        {
            get
            {
                return _itemAssets;
            }

            set
            {
                if (value != _itemAssets)
                {
                    _itemAssets = value;
                    OnPropertyChanged(nameof(ItemAssets));
                }
            }
        }

        /// <summary>
        /// This is detail of the breakdown of materials used by asteroids.
        /// </summary>
        public ObservableCollection<ComponentItemModel> MaterialAssets
        {
            get
            {
                return _materialAssets;
            }

            set
            {
                if (value != _materialAssets)
                {
                    _materialAssets = value;
                    OnPropertyChanged(nameof(MaterialAssets));
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
                return _isBusy;
            }

            set
            {
                if (value != _isBusy)
                {
                    _isBusy = value;
                    OnPropertyChanged(nameof(IsBusy));
                    if (_isBusy)
                    {
                        System.Windows.Forms.Application.DoEvents();
                    }
                }
            }
        }

        public ComponentItemModel SelectedCubeAsset
        {
            get
            {
                return _selectedCubeAsset;
            }

            set
            {
                if (value != _selectedCubeAsset)
                {
                    _selectedCubeAsset = value;
                    OnPropertyChanged(nameof(SelectedCubeAsset));
                }
            }
        }

        #endregion

        #region methods

        #region Load

        public void Load()
        {
            CubeAssets = new ObservableCollection<ComponentItemModel>();
            ComponentAssets = new ObservableCollection<ComponentItemModel>();
            ItemAssets = new ObservableCollection<ComponentItemModel>();
            MaterialAssets = new ObservableCollection<ComponentItemModel>();

            var contentPath = ToolboxUpdater.GetApplicationContentPath();

            foreach (var cubeDefinition in SpaceEngineersCore.Resources.CubeBlockDefinitions)
            {
                var props = new Dictionary<string, string>();
                var fields = cubeDefinition.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);

                foreach (var field in fields)
                {
                    props.Add(field.Name, GetValue(field, cubeDefinition));
                }

                CubeAssets.Add(new ComponentItemModel
                {
                    Name = cubeDefinition.DisplayNameText,
                    TypeId = cubeDefinition.Id.TypeId,
                    TypeIdString = cubeDefinition.Id.TypeId.ToString(),
                    SubtypeId = cubeDefinition.Id.SubtypeName,
                    TextureFile = (cubeDefinition.Icons == null || cubeDefinition.Icons.First() == null) ? null : SpaceEngineersCore.GetDataPathOrDefault(cubeDefinition.Icons.First(), Path.Combine(contentPath, cubeDefinition.Icons.First())),
                    Time = TimeSpan.FromSeconds(cubeDefinition.MaxIntegrity / cubeDefinition.IntegrityPointsPerSec),
                    Accessible = cubeDefinition.Public,
                    PCU = cubeDefinition.PCU,
                    Mass = SpaceEngineersApi.FetchCubeBlockMass(cubeDefinition.Id.TypeId, cubeDefinition.CubeSize, cubeDefinition.Id.SubtypeName),
                    CubeSize = cubeDefinition.CubeSize,
                    Size = new BindableSize3DIModel(cubeDefinition.Size),
                    CustomProperties = props,
                    IsMod = !cubeDefinition.Context.IsBaseGame,
                });
            }

            foreach (var componentDefinition in SpaceEngineersCore.Resources.ComponentDefinitions)
            {
                var bp = SpaceEngineersApi.GetBlueprint(componentDefinition.Id.TypeId, componentDefinition.Id.SubtypeName);
                float amount = 0;
                if (bp != null && bp.Results.Length > 0)
                    amount = (float)bp.Results[0].Amount;

                ComponentAssets.Add(new ComponentItemModel
                {
                    Name = componentDefinition.DisplayNameText,
                    TypeId = componentDefinition.Id.TypeId,
                    TypeIdString = componentDefinition.Id.TypeId.ToString(),
                    SubtypeId = componentDefinition.Id.SubtypeName,
                    Mass = componentDefinition.Mass,
                    TextureFile = (componentDefinition.Icons == null || componentDefinition.Icons.First() == null) ? null : SpaceEngineersCore.GetDataPathOrDefault(componentDefinition.Icons.First(), Path.Combine(contentPath, componentDefinition.Icons.First())),
                    Volume = componentDefinition.Volume * SpaceEngineersConsts.VolumeMultiplyer,
                    Accessible = componentDefinition.Public,
                    Time = bp != null ? TimeSpan.FromSeconds(bp.BaseProductionTimeInSeconds / amount) : (TimeSpan?)null,
                    IsMod = !componentDefinition.Context.IsBaseGame,
                });
            }

            foreach (var physicalItemDefinition in SpaceEngineersCore.Resources.PhysicalItemDefinitions)
            {
                var bp = SpaceEngineersApi.GetBlueprint(physicalItemDefinition.Id.TypeId, physicalItemDefinition.Id.SubtypeName);
                float amount = 0;
                if (bp != null)
                {
                    if (bp.Results != null && bp.Results.Length > 0)
                        amount = (float)bp.Results[0].Amount;
                }

                float timeMassMultiplyer = 1f;
                if (physicalItemDefinition.Id.TypeId == typeof(MyObjectBuilder_Ore)
                    || physicalItemDefinition.Id.TypeId == typeof(MyObjectBuilder_Ingot))
                    timeMassMultiplyer = physicalItemDefinition.Mass;

                ItemAssets.Add(new ComponentItemModel
                {
                    Name = physicalItemDefinition.DisplayNameText,
                    TypeId = physicalItemDefinition.Id.TypeId,
                    TypeIdString = physicalItemDefinition.Id.TypeId.ToString(),
                    SubtypeId = physicalItemDefinition.Id.SubtypeName,
                    Mass = physicalItemDefinition.Mass,
                    Volume = physicalItemDefinition.Volume * SpaceEngineersConsts.VolumeMultiplyer,
                    TextureFile = physicalItemDefinition.Icons == null ? null : SpaceEngineersCore.GetDataPathOrDefault(physicalItemDefinition.Icons.First(), Path.Combine(contentPath, physicalItemDefinition.Icons.First())),
                    Accessible = physicalItemDefinition.Public,
                    Time = bp != null ? TimeSpan.FromSeconds(bp.BaseProductionTimeInSeconds / amount / timeMassMultiplyer) : (TimeSpan?)null,
                    IsMod = !physicalItemDefinition.Context.IsBaseGame,
                });
            }

            foreach (MyVoxelMaterialDefinition voxelMaterialDefinition in SpaceEngineersCore.Resources.VoxelMaterialDefinitions)
            {
                string texture = voxelMaterialDefinition.GetVoxelDisplayTexture();

                MaterialAssets.Add(new ComponentItemModel
                {
                    Name = voxelMaterialDefinition.Id.SubtypeName,
                    TextureFile = texture == null ? null : SpaceEngineersCore.GetDataPathOrDefault(texture, Path.Combine(contentPath, texture)),
                    IsRare = voxelMaterialDefinition.IsRare,
                    OreName = voxelMaterialDefinition.MinedOre,
                    MineOreRatio = voxelMaterialDefinition.MinedOreRatio,
                    IsMod = !voxelMaterialDefinition.Context.IsBaseGame,
                });
            }
        }

        #endregion

        #region GenerateHtmlReport

        public void GenerateHtmlReport(string filename)
        {
            var stringWriter = new StringWriter();

            // Put HtmlTextWriter in using block because it needs to call Dispose.
            using (var writer = new HtmlTextWriter(stringWriter))
            {
                #region header

                writer.BeginDocument(Res.CtlComponentTitleReport,
                   @"
body { background-color: #E6E6FA }
h1 { font-family: Arial, Helvetica, sans-serif; }
table { background-color: #FFFFFF; }
table tr td { font-family: Arial, Helvetica, sans-serif; font-size: small; line-height: normal; color: #000000; }
table thead td { background-color: #BABDD6; font-weight: bold; Color: #000000; }
td.right { text-align: right; }");

                #endregion

                #region Cubes

                writer.RenderElement(HtmlTextWriterTag.H1, Res.CtlComponentTitleCubes);
                writer.BeginTable("1", "3", "0",
                    new[] { Res.CtlComponentColIcon, Res.CtlComponentColName, Res.CtlComponentColType, Res.CtlComponentColSubType, Res.CtlComponentColCubeSize, Res.CtlComponentColPCU, Res.CtlComponentColAccessible, Res.CtlComponentColSize, Res.CtlComponentColMass, Res.CtlComponentColBuildTime, Res.CtlComponentColMod });

                foreach (var asset in CubeAssets)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);

                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    if (asset.TextureFile != null)
                    {
                        string texture = GetTextureToBase64(asset.TextureFile, 32, 32);
                        if (!string.IsNullOrEmpty(texture))
                        {
                            writer.AddAttribute(HtmlTextWriterAttribute.Src, "data:image/png;base64," + texture);
                            writer.AddAttribute(HtmlTextWriterAttribute.Width, "32");
                            writer.AddAttribute(HtmlTextWriterAttribute.Height, "32");
                            writer.AddAttribute(HtmlTextWriterAttribute.Alt, Path.GetFileNameWithoutExtension(asset.TextureFile));
                            writer.RenderBeginTag(HtmlTextWriterTag.Img);
                            writer.RenderEndTag();
                        }
                    }
                    writer.RenderEndTag(); // Td

                    writer.RenderElement(HtmlTextWriterTag.Td, asset.FriendlyName);
                    writer.RenderElement(HtmlTextWriterTag.Td, asset.TypeId);
                    writer.RenderElement(HtmlTextWriterTag.Td, asset.SubtypeId);
                    writer.RenderElement(HtmlTextWriterTag.Td, asset.CubeSize);
                    writer.RenderElement(HtmlTextWriterTag.Td, asset.PCU);
                    writer.RenderElement(HtmlTextWriterTag.Td, new EnumToResouceConverter().Convert(asset.Accessible, typeof(string), null, CultureInfo.CurrentUICulture));
                    writer.RenderElement(HtmlTextWriterTag.Td, "{0}×{1}×{2}", asset.Size.Width, asset.Size.Height, asset.Size.Depth);
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "right");
                    writer.RenderElement(HtmlTextWriterTag.Td, "{0:#,##0.00}", asset.Mass);
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "right");
                    writer.RenderElement(HtmlTextWriterTag.Td, "{0:hh\\:mm\\:ss\\.ff}", asset.Time);
                    writer.RenderElement(HtmlTextWriterTag.Td, new EnumToResouceConverter().Convert(asset.IsMod, typeof(string), null, CultureInfo.CurrentUICulture));

                    writer.RenderEndTag(); // Tr
                }
                writer.RenderEndTag(); // Table 

                #endregion

                #region Components

                writer.RenderElement(HtmlTextWriterTag.H1, Res.CtlComponentTitleComponents);
                writer.BeginTable("1", "3", "0",
                    new[] { Res.CtlComponentColIcon, Res.CtlComponentColName, Res.CtlComponentColType, Res.CtlComponentColSubType, Res.CtlComponentColAccessible, Res.CtlComponentColMass, Res.CtlComponentColVolume, Res.CtlComponentColBuildTime, Res.CtlComponentColMod });

                foreach (var asset in ComponentAssets)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);

                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    if (asset.TextureFile != null)
                    {
                        string texture = GetTextureToBase64(asset.TextureFile, 32, 32);
                        if (!string.IsNullOrEmpty(texture))
                        {
                            writer.AddAttribute(HtmlTextWriterAttribute.Src, "data:image/png;base64," + texture);
                            writer.AddAttribute(HtmlTextWriterAttribute.Width, "32");
                            writer.AddAttribute(HtmlTextWriterAttribute.Height, "32");
                            writer.AddAttribute(HtmlTextWriterAttribute.Alt, Path.GetFileNameWithoutExtension(asset.TextureFile));
                            writer.RenderBeginTag(HtmlTextWriterTag.Img);
                            writer.RenderEndTag();
                        }
                    }
                    writer.RenderEndTag(); // Td

                    writer.RenderElement(HtmlTextWriterTag.Td, asset.FriendlyName);
                    writer.RenderElement(HtmlTextWriterTag.Td, asset.TypeId);
                    writer.RenderElement(HtmlTextWriterTag.Td, asset.SubtypeId);
                    writer.RenderElement(HtmlTextWriterTag.Td, new EnumToResouceConverter().Convert(asset.Accessible, typeof(string), null, CultureInfo.CurrentUICulture));
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "right");
                    writer.RenderElement(HtmlTextWriterTag.Td, "{0:#,##0.00}", asset.Mass);
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "right");
                    writer.RenderElement(HtmlTextWriterTag.Td, "{0:#,##0.00}", asset.Volume);
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "right");
                    writer.RenderElement(HtmlTextWriterTag.Td, "{0:hh\\:mm\\:ss\\.ff}", asset.Time);
                    writer.RenderElement(HtmlTextWriterTag.Td, new EnumToResouceConverter().Convert(asset.IsMod, typeof(string), null, CultureInfo.CurrentUICulture));

                    writer.RenderEndTag(); // Tr
                }
                writer.RenderEndTag(); // Table 

                #endregion

                #region Items

                writer.RenderElement(HtmlTextWriterTag.H1, Res.CtlComponentTitleItems);
                writer.BeginTable("1", "3", "0",
                    new[] { Res.CtlComponentColIcon, Res.CtlComponentColName, Res.CtlComponentColType, Res.CtlComponentColSubType, Res.CtlComponentColAccessible, Res.CtlComponentColMass, Res.CtlComponentColVolume, Res.CtlComponentColBuildTime, Res.CtlComponentColMod });

                foreach (var asset in ItemAssets)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);

                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    if (asset.TextureFile != null)
                    {
                        string texture = GetTextureToBase64(asset.TextureFile, 32, 32);
                        if (!string.IsNullOrEmpty(texture))
                        {
                            writer.AddAttribute(HtmlTextWriterAttribute.Src, "data:image/png;base64," + texture);
                            writer.AddAttribute(HtmlTextWriterAttribute.Width, "32");
                            writer.AddAttribute(HtmlTextWriterAttribute.Height, "32");
                            writer.AddAttribute(HtmlTextWriterAttribute.Alt, Path.GetFileNameWithoutExtension(asset.TextureFile));
                            writer.RenderBeginTag(HtmlTextWriterTag.Img);
                            writer.RenderEndTag();
                        }
                    }
                    writer.RenderEndTag(); // Td

                    writer.RenderElement(HtmlTextWriterTag.Td, asset.FriendlyName);
                    writer.RenderElement(HtmlTextWriterTag.Td, asset.TypeId);
                    writer.RenderElement(HtmlTextWriterTag.Td, asset.SubtypeId);
                    writer.RenderElement(HtmlTextWriterTag.Td, new EnumToResouceConverter().Convert(asset.Accessible, typeof(string), null, CultureInfo.CurrentUICulture));
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "right");
                    writer.RenderElement(HtmlTextWriterTag.Td, "{0:#,##0.00}", asset.Mass);
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "right");
                    writer.RenderElement(HtmlTextWriterTag.Td, "{0:#,##0.00}", asset.Volume);
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "right");
                    writer.RenderElement(HtmlTextWriterTag.Td, "{0:hh\\:mm\\:ss\\.ff}", asset.Time);
                    writer.RenderElement(HtmlTextWriterTag.Td, new EnumToResouceConverter().Convert(asset.IsMod, typeof(string), null, CultureInfo.CurrentUICulture));

                    writer.RenderEndTag(); // Tr
                }
                writer.RenderEndTag(); // Table 

                #endregion

                #region Materials

                writer.RenderElement(HtmlTextWriterTag.H1, Res.CtlComponentTitleMaterials);
                writer.BeginTable("1", "3", "0",
                    new[] { Res.CtlComponentColTexture, Res.CtlComponentColName, Res.CtlComponentColOreName, Res.CtlComponentColRare, Res.CtlComponentColMinedOreRatio, Res.CtlComponentColMod });

                foreach (var asset in MaterialAssets)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);

                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    if (asset.TextureFile != null)
                    {
                        string texture = GetTextureToBase64(asset.TextureFile, 32, 32, true);
                        if (!string.IsNullOrEmpty(texture))
                        {
                            writer.AddAttribute(HtmlTextWriterAttribute.Src, "data:image/png;base64," + texture);
                            writer.AddAttribute(HtmlTextWriterAttribute.Width, "32");
                            writer.AddAttribute(HtmlTextWriterAttribute.Height, "32");
                            writer.AddAttribute(HtmlTextWriterAttribute.Alt, Path.GetFileNameWithoutExtension(asset.TextureFile));
                            writer.RenderBeginTag(HtmlTextWriterTag.Img);
                            writer.RenderEndTag();
                        }
                    }
                    writer.RenderEndTag(); // Td

                    writer.RenderElement(HtmlTextWriterTag.Td, asset.Name);
                    writer.RenderElement(HtmlTextWriterTag.Td, asset.OreName);
                    writer.RenderElement(HtmlTextWriterTag.Td, new EnumToResouceConverter().Convert(asset.IsRare, typeof(string), null, CultureInfo.CurrentUICulture));
                    writer.RenderElement(HtmlTextWriterTag.Td, "{0:#,##0.00}", asset.MineOreRatio);
                    writer.RenderElement(HtmlTextWriterTag.Td, new EnumToResouceConverter().Convert(asset.IsMod, typeof(string), null, CultureInfo.CurrentUICulture));

                    writer.RenderEndTag(); // Tr
                }
                writer.RenderEndTag(); // Table 

                #endregion

                #region footer

                writer.EndDocument();

                #endregion
            }

            // Write to disk.
            File.WriteAllText(filename, stringWriter.ToString());
        }

        #endregion

        private static string GetTextureToBase64(string filename, int width, int height, bool ignoreAlpha = false)
        {
            using (Stream stream = MyFileSystem.OpenRead(filename))
            {
                return ImageTextureUtil.GetTextureToBase64(stream, filename, width, height, ignoreAlpha);
            }
        }

        #region GetRowValue

        public static string GetValue(FieldInfo field, object objt)
        {
            var item = field.GetValue(objt);

            if (field.FieldType == typeof(SerializableVector3I))
            {
                var vector = (SerializableVector3I)item;
                return string.Format("{0}, {1}, {2}", vector.X, vector.Y, vector.Z);
            }

            if (field.FieldType == typeof(SerializableVector3))
            {
                var vector = (SerializableVector3)item;
                return string.Format("{0}, {1}, {2}", vector.X, vector.Y, vector.Z);
            }

            if (field.FieldType == typeof(SerializableBounds))
            {
                var bounds = (SerializableBounds)item;
                // TODO: #21 localize
                return string.Format("Default:{0}, Min:{1}, max:{2}", bounds.Default, bounds.Min, bounds.Max);
            }

            if (field.FieldType == typeof(VRageMath.Vector3))
            {
                var vector3 = (VRageMath.Vector3)item;
                // TODO: #21 localize
                return string.Format("X:{0}, Y:{1}, Z:{2}", vector3.X, vector3.Y, vector3.Z);
            }

            if (field.FieldType == typeof(string))
            {
                return item as string;
            }

            if (item == null)
            {
                return string.Empty;
            }

            return item.ToString();
        }

        #endregion

        #endregion
    }
}
