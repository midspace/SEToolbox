namespace SEToolbox.Models
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using SEToolbox.Converters;
    using SEToolbox.ImageLibrary;
    using SEToolbox.Interop;
    using SEToolbox.Support;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Web.UI;

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

        #region ctor

        public ComponentListModel()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// This is detail of the breakdown of cubes in the ship.
        /// </summary>
        public ObservableCollection<ComponentItemModel> CubeAssets
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
        public ObservableCollection<ComponentItemModel> ComponentAssets
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
        /// This is detail of the breakdown of items.
        /// </summary>
        public ObservableCollection<ComponentItemModel> ItemAssets
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
        /// This is detail of the breakdown of materials used by asteroids.
        /// </summary>
        public ObservableCollection<ComponentItemModel> MaterialAssets
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

        public ComponentItemModel SelectedCubeAsset
        {
            get
            {
                return this._selectedCubeAsset;
            }

            set
            {
                if (value != this._selectedCubeAsset)
                {
                    this._selectedCubeAsset = value;
                    this.RaisePropertyChanged(() => SelectedCubeAsset);
                }
            }
        }

        #endregion

        #region methods

        #region Load

        public void Load()
        {
            this.CubeAssets = new ObservableCollection<ComponentItemModel>();
            this.ComponentAssets = new ObservableCollection<ComponentItemModel>();
            this.ItemAssets = new ObservableCollection<ComponentItemModel>();
            this.MaterialAssets = new ObservableCollection<ComponentItemModel>();

            var contentPath = Path.Combine(ToolboxUpdater.GetApplicationFilePath(), @"..\Content");

            foreach (var cubeDefinition in SpaceEngineersAPI.CubeBlockDefinitions)
            {
                var props = new Dictionary<string, string>();

                if (!cubeDefinition.GetType().Equals(typeof(MyObjectBuilder_CubeBlockDefinition)))
                {
                    var fields = LoadCubeChildFields(cubeDefinition.GetType());

                    foreach (var field in fields)
                    {
                        props.Add(field.Name, GetValue(field, cubeDefinition));
                    }
                }

                this.CubeAssets.Add(new ComponentItemModel()
                {
                    Name = cubeDefinition.DisplayName,
                    TypeId = cubeDefinition.Id.TypeId,
                    SubtypeId = cubeDefinition.Id.SubtypeId,
                    TextureFile = Path.Combine(contentPath, cubeDefinition.Icon + ".dds"),
                    Time = new TimeSpan((long)(TimeSpan.TicksPerSecond * cubeDefinition.BuildTimeSeconds)),
                    Accessible = cubeDefinition.Public,
                    Mass = SpaceEngineersAPI.FetchCubeBlockMass(cubeDefinition.Id.TypeId, cubeDefinition.CubeSize, cubeDefinition.Id.SubtypeId),
                    CubeSize = cubeDefinition.CubeSize,
                    Size = new BindableSize3DIModel(cubeDefinition.Size),
                    CustomProperties = props,
                });
            }

            foreach (var componentDefinition in SpaceEngineersAPI.ComponentDefinitions)
            {
                var bp = SpaceEngineersAPI.BlueprintDefinitions.FirstOrDefault(b => b.Result.SubtypeId == componentDefinition.Id.SubtypeId && b.Result.TypeId == componentDefinition.Id.TypeId);

                this.ComponentAssets.Add(new ComponentItemModel()
                {
                    Name = componentDefinition.DisplayName,
                    TypeId = componentDefinition.Id.TypeId,
                    SubtypeId = componentDefinition.Id.SubtypeId,
                    Mass = componentDefinition.Mass,
                    TextureFile = componentDefinition.Icon == null ? null : Path.Combine(contentPath, componentDefinition.Icon + ".dds"),
                    Volume = componentDefinition.Volume.HasValue ? componentDefinition.Volume.Value : 0f,
                    Accessible = componentDefinition.Public,
                    Time = bp != null ? new TimeSpan((long)(TimeSpan.TicksPerSecond * bp.BaseProductionTimeInSeconds)) : (TimeSpan?)null,
                });
            }

            foreach (var physicalItemDefinition in SpaceEngineersAPI.PhysicalItemDefinitions)
            {
                var bp = SpaceEngineersAPI.BlueprintDefinitions.FirstOrDefault(b => b.Result.SubtypeId == physicalItemDefinition.Id.SubtypeId && b.Result.TypeId == physicalItemDefinition.Id.TypeId);
                this.ItemAssets.Add(new ComponentItemModel()
                {
                    Name = physicalItemDefinition.DisplayName,
                    TypeId = physicalItemDefinition.Id.TypeId,
                    SubtypeId = physicalItemDefinition.Id.SubtypeId,
                    Mass = physicalItemDefinition.Mass,
                    Volume = physicalItemDefinition.Volume.HasValue ? physicalItemDefinition.Volume.Value : 0f,
                    TextureFile = physicalItemDefinition.Icon == null ? null : Path.Combine(contentPath, physicalItemDefinition.Icon + ".dds"),
                    Accessible = physicalItemDefinition.Public,
                    Time = bp != null ? new TimeSpan((long)(TimeSpan.TicksPerSecond * bp.BaseProductionTimeInSeconds)) : (TimeSpan?)null,
                });
            }

            foreach (var physicalItemDefinition in SpaceEngineersAPI.AmmoMagazineDefinitions)
            {
                var bp = SpaceEngineersAPI.BlueprintDefinitions.FirstOrDefault(b => b.Result.SubtypeId == physicalItemDefinition.Id.SubtypeId && b.Result.TypeId == physicalItemDefinition.Id.TypeId);
                this.ItemAssets.Add(new ComponentItemModel()
                {
                    Name = physicalItemDefinition.DisplayName,
                    TypeId = physicalItemDefinition.Id.TypeId,
                    SubtypeId = physicalItemDefinition.Id.SubtypeId,
                    Mass = physicalItemDefinition.Mass,
                    Volume = physicalItemDefinition.Volume.HasValue ? physicalItemDefinition.Volume.Value : 0f,
                    TextureFile = physicalItemDefinition.Icon == null ? null : Path.Combine(contentPath, physicalItemDefinition.Icon + ".dds"),
                    Accessible = !string.IsNullOrEmpty(physicalItemDefinition.Model),
                    Time = bp != null ? new TimeSpan((long)(TimeSpan.TicksPerSecond * bp.BaseProductionTimeInSeconds)) : (TimeSpan?)null,
                });
            }

            foreach (var voxelMaterialDefinition in SpaceEngineersAPI.VoxelMaterialDefinitions)
            {
                this.MaterialAssets.Add(new ComponentItemModel()
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

        #region GenerateHtmlReport

        public void GenerateHtmlReport(string filename)
        {
            var stringWriter = new StringWriter();

            // Put HtmlTextWriter in using block because it needs to call Dispose.
            using (var writer = new HtmlTextWriter(stringWriter))
            {
                #region header

                writer.BeginDocument("Component Item Report",
                   @"
body { background-color: #E6E6FA }
h1 { font-family: Arial, Helvetica, sans-serif; }
table { background-color: #FFFFFF; }
table tr td { font-family: Arial, Helvetica, sans-serif; font-size: small; line-height: normal; color: #000000; }
table thead td { background-color: #BABDD6; font-weight: bold; Color: #000000; }
td.right { text-align: right; }");

                #endregion

                #region Cubes

                writer.RenderElement(HtmlTextWriterTag.H1, "Cubes");
                writer.BeginTable("1", "3", "0",
                    new String[] { "Icon", "Name", "Type Id", "Sub Type Id", "Cube Size", "Accessible", "Size (W×H×D)", "Mass (Kg)", "Build Time (h:m:s)" });

                foreach (var asset in this.CubeAssets)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);

                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    if (asset.TextureFile != null)
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Src, "data:image/png;base64," + ImageTextureUtil.GetTextureToBase64(asset.TextureFile, 32, 32));
                        writer.AddAttribute(HtmlTextWriterAttribute.Width, "32");
                        writer.AddAttribute(HtmlTextWriterAttribute.Height, "32");
                        writer.AddAttribute(HtmlTextWriterAttribute.Alt, Path.GetFileNameWithoutExtension(asset.TextureFile));
                        writer.RenderBeginTag(HtmlTextWriterTag.Img);
                        writer.RenderEndTag();
                    }
                    writer.RenderEndTag(); // Td

                    writer.RenderElement(HtmlTextWriterTag.Td, asset.FriendlyName);
                    writer.RenderElement(HtmlTextWriterTag.Td, asset.TypeId);
                    writer.RenderElement(HtmlTextWriterTag.Td, asset.SubtypeId);
                    writer.RenderElement(HtmlTextWriterTag.Td, asset.CubeSize);
                    writer.RenderElement(HtmlTextWriterTag.Td, new EnumToResouceConverter().Convert(asset.Accessible, typeof(string), null, CultureInfo.CurrentUICulture));
                    writer.RenderElement(HtmlTextWriterTag.Td, "{0}×{1}×{2}", asset.Size.Width, asset.Size.Height, asset.Size.Depth);
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "right");
                    writer.RenderElement(HtmlTextWriterTag.Td, "{0:#,##0.00}", asset.Mass);
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "right");
                    writer.RenderElement(HtmlTextWriterTag.Td, "{0:hh\\:mm\\:ss\\.ff}", asset.Time);

                    writer.RenderEndTag(); // Tr
                }
                writer.RenderEndTag(); // Table 

                #endregion

                #region Components

                writer.RenderElement(HtmlTextWriterTag.H1, "Components");
                writer.BeginTable("1", "3", "0",
                    new String[] { "Icon", "Name", "Type Id", "Sub Type Id", "Accessible", "Mass (Kg)", "Volume (L)", "Build Time (h:m:s)" });

                foreach (var asset in this.ComponentAssets)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);

                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    if (asset.TextureFile != null)
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Src, "data:image/png;base64," + ImageTextureUtil.GetTextureToBase64(asset.TextureFile, 32, 32));
                        writer.AddAttribute(HtmlTextWriterAttribute.Width, "32");
                        writer.AddAttribute(HtmlTextWriterAttribute.Height, "32");
                        writer.AddAttribute(HtmlTextWriterAttribute.Alt, Path.GetFileNameWithoutExtension(asset.TextureFile));
                        writer.RenderBeginTag(HtmlTextWriterTag.Img);
                        writer.RenderEndTag();
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

                    writer.RenderEndTag(); // Tr
                }
                writer.RenderEndTag(); // Table 

                #endregion

                #region Items

                writer.RenderElement(HtmlTextWriterTag.H1, "Items");
                writer.BeginTable("1", "3", "0",
                    new String[] { "Icon", "Name", "Type Id", "Sub Type Id", "Accessible", "Mass (Kg)", "Volume (L)", "Build Time (h:m:s)" });

                foreach (var asset in this.ItemAssets)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);

                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    if (asset.TextureFile != null)
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Src, "data:image/png;base64," + ImageTextureUtil.GetTextureToBase64(asset.TextureFile, 32, 32));
                        writer.AddAttribute(HtmlTextWriterAttribute.Width, "32");
                        writer.AddAttribute(HtmlTextWriterAttribute.Height, "32");
                        writer.AddAttribute(HtmlTextWriterAttribute.Alt, Path.GetFileNameWithoutExtension(asset.TextureFile));
                        writer.RenderBeginTag(HtmlTextWriterTag.Img);
                        writer.RenderEndTag();
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

                    writer.RenderEndTag(); // Tr
                }
                writer.RenderEndTag(); // Table 

                #endregion

                #region Materials

                writer.RenderElement(HtmlTextWriterTag.H1, "Materials");
                writer.BeginTable("1", "3", "0",
                    new String[] { "Texture", "Name", "Ore Name", "Rare", "Mined Ore Ratio" });

                foreach (var asset in this.MaterialAssets)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);

                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    if (asset.TextureFile != null)
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Src, "data:image/png;base64," + ImageTextureUtil.GetTextureToBase64(asset.TextureFile, 32, 32));
                        writer.AddAttribute(HtmlTextWriterAttribute.Width, "32");
                        writer.AddAttribute(HtmlTextWriterAttribute.Height, "32");
                        writer.AddAttribute(HtmlTextWriterAttribute.Alt, Path.GetFileNameWithoutExtension(asset.TextureFile));
                        writer.RenderBeginTag(HtmlTextWriterTag.Img);
                        writer.RenderEndTag();
                    }
                    writer.RenderEndTag(); // Td

                    writer.RenderElement(HtmlTextWriterTag.Td, asset.Name);
                    writer.RenderElement(HtmlTextWriterTag.Td, asset.OreName);
                    writer.RenderElement(HtmlTextWriterTag.Td, new EnumToResouceConverter().Convert(asset.IsRare, typeof(string), null, CultureInfo.CurrentUICulture));
                    writer.RenderElement(HtmlTextWriterTag.Td, "{0:#,##0.00}", asset.MineOreRatio);

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

        #region LoadCubeChildFields

        private List<FieldInfo> LoadCubeChildFields(Type cubeType)
        {
            var fields = new List<FieldInfo>();

            if (!cubeType.Equals(typeof(MyObjectBuilder_CubeBlockDefinition)))
            {
                fields.AddRange(cubeType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly));

                if (!cubeType.BaseType.Equals(typeof(MyObjectBuilder_CubeBlockDefinition)))
                {
                    fields.AddRange(LoadCubeChildFields(cubeType.BaseType));
                }
            }

            return fields;
        }

        #endregion

        #region GetRowValue

        public static string GetValue(FieldInfo field, object objt)
        {
            var item = field.GetValue(objt);

            if (field.FieldType == typeof(Sandbox.Common.ObjectBuilders.VRageData.SerializableBounds))
            {
                var bounds = (Sandbox.Common.ObjectBuilders.VRageData.SerializableBounds)item;
                return string.Format("Default:{0}, Min:{1}, max:{2}", bounds.Default, bounds.Min, bounds.Max);
            }
            else if (field.FieldType == typeof(VRageMath.Vector3))
            {
                var vector3 = (VRageMath.Vector3)item;
                return string.Format("X:{0}, Y:{1}, Z:{2}", vector3.X, vector3.Y, vector3.Z);
            }
            else if (field.FieldType == typeof(string))
            {
                return item as string;
            }
            else
            {
                return item.ToString();
            }
        }

        #endregion

        #endregion
    }
}
