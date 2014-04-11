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

        private ObservableCollection<ComonentItemModel> _cubeAssets;

        private ObservableCollection<ComonentItemModel> _componentAssets;

        private ObservableCollection<ComonentItemModel> _itemAssets;

        private ObservableCollection<ComonentItemModel> _materialAssets;

        private bool _isBusy;

        private ComonentItemModel _selectedCubeAsset;

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
        /// This is detail of the breakdown of items.
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
        /// This is detail of the breakdown of materials used by asteroids.
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

        public ComonentItemModel SelectedCubeAsset
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
            this.CubeAssets = new ObservableCollection<ComonentItemModel>();
            this.ComponentAssets = new ObservableCollection<ComonentItemModel>();
            this.ItemAssets = new ObservableCollection<ComonentItemModel>();
            this.MaterialAssets = new ObservableCollection<ComonentItemModel>();

            var contentPath = Path.Combine(ToolboxUpdater.GetGameRegistryFilePath(), "Content");

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
                    CustomProperties = props,
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
                    TextureFile = componentDefinition.Icon == null ? null : Path.Combine(contentPath, componentDefinition.Icon + ".dds"),
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
                    TextureFile = physicalItemDefinition.Icon == null ? null : Path.Combine(contentPath, physicalItemDefinition.Icon + ".dds"),
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
                    TextureFile = physicalItemDefinition.Icon == null ? null : Path.Combine(contentPath, physicalItemDefinition.Icon + ".dds"),
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

        #region GenerateHtmlReport

        public void GenerateHtmlReport(string filename)
        {
            var stringWriter = new StringWriter();

            // Put HtmlTextWriter in using block because it needs to call Dispose.
            using (var writer = new HtmlTextWriter(stringWriter))
            {
                #region header

                writer.AddAttribute("http-equiv", "Content-Type");
                writer.AddAttribute("content", "text/html;charset=UTF-8");
                writer.RenderBeginTag(HtmlTextWriterTag.Meta);
                writer.RenderEndTag();

                writer.RenderBeginTag(HtmlTextWriterTag.Html);
                writer.RenderBeginTag(HtmlTextWriterTag.Style);
                writer.Write(@"
h1 { font-family: Arial, Helvetica, sans-serif; }
table { background-color: #FFFFFF; }
table tr td { font-family: Arial, Helvetica, sans-serif; font-size: small; line-height: normal; color: #000000; }
table thead td { background-color: #BABDD6; font-weight: bold; Color: #000000; }
td.right { text-align: right; }");
                writer.RenderEndTag(); // Style

                writer.RenderBeginTag(HtmlTextWriterTag.Head);
                writer.RenderBeginTag(HtmlTextWriterTag.Title);
                writer.Write("Component Item Report");
                writer.RenderBeginTag(HtmlTextWriterTag.Title);
                writer.RenderEndTag();

                writer.AddAttribute(HtmlTextWriterAttribute.Bgcolor, "#E6E6FA");
                writer.RenderBeginTag(HtmlTextWriterTag.Body);

                #endregion

                #region Cubes

                writer.RenderBeginTag(HtmlTextWriterTag.H1);
                writer.Write("Cubes");
                writer.RenderEndTag(); // H1

                writer.AddAttribute(HtmlTextWriterAttribute.Border, "1");
                writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "3");
                writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
                writer.RenderBeginTag(HtmlTextWriterTag.Table);
                writer.RenderBeginTag(HtmlTextWriterTag.Thead);
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                foreach (var header in new String[] { "Icon", "Name", "Type Id", "Sub Type Id", "Cube Size", "Accessible", "Size (W×H×D)", "Mass (Kg)", "Build Time (h:m:s)" })
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.Write(header);
                    writer.RenderEndTag(); // Td
                }
                writer.RenderEndTag(); // Tr
                writer.RenderEndTag(); // Thead

                foreach (var asset in this.CubeAssets)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);

                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    if (asset.TextureFile != null)
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Src, "data:image/png;base64," + GetDdsImageToBase64(asset.TextureFile, 32, 32));
                        writer.AddAttribute(HtmlTextWriterAttribute.Width, "32");
                        writer.AddAttribute(HtmlTextWriterAttribute.Height, "32");
                        writer.AddAttribute(HtmlTextWriterAttribute.Alt, Path.GetFileNameWithoutExtension(asset.TextureFile));
                        writer.RenderBeginTag(HtmlTextWriterTag.Img);
                        writer.RenderEndTag();
                    }
                    writer.RenderEndTag(); // Td

                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.Write(asset.Name);
                    writer.RenderEndTag(); // Td

                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.Write(asset.TypeId);
                    writer.RenderEndTag(); // Td

                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.Write(asset.SubtypeId);
                    writer.RenderEndTag(); // Td

                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.Write(asset.CubeSize);
                    writer.RenderEndTag(); // Td

                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.Write(new EnumToResouceConverter().Convert(asset.Accessible, typeof(string), null, CultureInfo.CurrentUICulture));
                    writer.RenderEndTag(); // Td

                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.Write(string.Format("{0}×{1}×{2}", asset.Size.Width, asset.Size.Height, asset.Size.Depth));
                    writer.RenderEndTag(); // Td

                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "right");
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.Write(string.Format("{0:#,##0.00}", asset.Mass));
                    writer.RenderEndTag(); // Td

                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "right");
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.Write(string.Format("{0:hh\\:mm\\:ss\\.ff}", asset.Time));
                    writer.RenderEndTag(); // Td

                    writer.RenderEndTag(); // Tr
                }
                writer.RenderEndTag(); // Table 

                #endregion

                #region Components

                writer.RenderBeginTag(HtmlTextWriterTag.H1);
                writer.Write("Components");
                writer.RenderEndTag(); // H1

                writer.AddAttribute(HtmlTextWriterAttribute.Border, "1");
                writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "3");
                writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
                writer.RenderBeginTag(HtmlTextWriterTag.Table);
                writer.RenderBeginTag(HtmlTextWriterTag.Thead);
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                foreach (var header in new String[] { "Icon", "Name", "Type Id", "Sub Type Id", "Accessible", "Mass (Kg)", "Volume (L)", "Build Time (h:m:s)" })
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.Write(header);
                    writer.RenderEndTag(); // Td
                }
                writer.RenderEndTag(); // Tr
                writer.RenderEndTag(); // Thead

                foreach (var asset in this.ComponentAssets)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);

                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    if (asset.TextureFile != null)
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Src, "data:image/png;base64," + GetDdsImageToBase64(asset.TextureFile, 32, 32));
                        writer.AddAttribute(HtmlTextWriterAttribute.Width, "32");
                        writer.AddAttribute(HtmlTextWriterAttribute.Height, "32");
                        writer.AddAttribute(HtmlTextWriterAttribute.Alt, Path.GetFileNameWithoutExtension(asset.TextureFile));
                        writer.RenderBeginTag(HtmlTextWriterTag.Img);
                        writer.RenderEndTag();
                    }
                    writer.RenderEndTag(); // Td

                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.Write(asset.Name);
                    writer.RenderEndTag(); // Td

                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.Write(asset.TypeId);
                    writer.RenderEndTag(); // Td

                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.Write(asset.SubtypeId);
                    writer.RenderEndTag(); // Td

                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.Write(new EnumToResouceConverter().Convert(asset.Accessible, typeof(string), null, CultureInfo.CurrentUICulture));
                    writer.RenderEndTag(); // Td

                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "right");
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.Write(string.Format("{0:#,##0.00}", asset.Mass));
                    writer.RenderEndTag(); // Td

                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "right");
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.Write(string.Format("{0:#,##0.00}", asset.Volume));
                    writer.RenderEndTag(); // Td

                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "right");
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.Write(string.Format("{0:hh\\:mm\\:ss\\.ff}", asset.Time));
                    writer.RenderEndTag(); // Td

                    writer.RenderEndTag(); // Tr
                }
                writer.RenderEndTag(); // Table 

                #endregion

                #region Items

                writer.RenderBeginTag(HtmlTextWriterTag.H1);
                writer.Write("Items");
                writer.RenderEndTag(); // H1

                writer.AddAttribute(HtmlTextWriterAttribute.Border, "1");
                writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "3");
                writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
                writer.RenderBeginTag(HtmlTextWriterTag.Table);
                writer.RenderBeginTag(HtmlTextWriterTag.Thead);
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                foreach (var header in new String[] { "Icon", "Name", "Type Id", "Sub Type Id", "Accessible", "Mass (Kg)", "Volume (L)", "Build Time (h:m:s)" })
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.Write(header);
                    writer.RenderEndTag(); // Td
                }
                writer.RenderEndTag(); // Tr
                writer.RenderEndTag(); // Thead

                foreach (var asset in this.ItemAssets)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);

                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    if (asset.TextureFile != null)
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Src, "data:image/png;base64," + GetDdsImageToBase64(asset.TextureFile, 32, 32));
                        writer.AddAttribute(HtmlTextWriterAttribute.Width, "32");
                        writer.AddAttribute(HtmlTextWriterAttribute.Height, "32");
                        writer.AddAttribute(HtmlTextWriterAttribute.Alt, Path.GetFileNameWithoutExtension(asset.TextureFile));
                        writer.RenderBeginTag(HtmlTextWriterTag.Img);
                        writer.RenderEndTag();
                    }
                    writer.RenderEndTag(); // Td

                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.Write(asset.Name);
                    writer.RenderEndTag(); // Td

                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.Write(asset.TypeId);
                    writer.RenderEndTag(); // Td

                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.Write(asset.SubtypeId);
                    writer.RenderEndTag(); // Td

                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.Write(new EnumToResouceConverter().Convert(asset.Accessible, typeof(string), null, CultureInfo.CurrentUICulture));
                    writer.RenderEndTag(); // Td

                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "right");
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.Write(string.Format("{0:#,##0.00}", asset.Mass));
                    writer.RenderEndTag(); // Td

                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "right");
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.Write(string.Format("{0:#,##0.00}", asset.Volume));
                    writer.RenderEndTag(); // Td

                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "right");
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.Write(string.Format("{0:hh\\:mm\\:ss\\.ff}", asset.Time));
                    writer.RenderEndTag(); // Td

                    writer.RenderEndTag(); // Tr
                }
                writer.RenderEndTag(); // Table 

                #endregion

                #region Materials

                writer.RenderBeginTag(HtmlTextWriterTag.H1);
                writer.Write("Materials");
                writer.RenderEndTag(); // H1

                writer.AddAttribute(HtmlTextWriterAttribute.Border, "1");
                writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "3");
                writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
                writer.RenderBeginTag(HtmlTextWriterTag.Table);
                writer.RenderBeginTag(HtmlTextWriterTag.Thead);
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                foreach (var header in new String[] { "Texture", "Name", "Ore Name", "Rare", "Mined Ore Ratio" })
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.Write(header);
                    writer.RenderEndTag(); // Td
                }
                writer.RenderEndTag(); // Tr
                writer.RenderEndTag(); // Thead

                foreach (var asset in this.MaterialAssets)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);

                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    if (asset.TextureFile != null)
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Src, "data:image/png;base64," + GetDdsImageToBase64(asset.TextureFile, 32, 32));
                        writer.AddAttribute(HtmlTextWriterAttribute.Width, "32");
                        writer.AddAttribute(HtmlTextWriterAttribute.Height, "32");
                        writer.AddAttribute(HtmlTextWriterAttribute.Alt, Path.GetFileNameWithoutExtension(asset.TextureFile));
                        writer.RenderBeginTag(HtmlTextWriterTag.Img);
                        writer.RenderEndTag();
                    }
                    writer.RenderEndTag(); // Td

                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.Write(asset.Name);
                    writer.RenderEndTag(); // Td

                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.Write(asset.OreName);
                    writer.RenderEndTag(); // Td

                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.Write(new EnumToResouceConverter().Convert(asset.IsRare, typeof(string), null, CultureInfo.CurrentUICulture));
                    writer.RenderEndTag(); // Td

                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.Write(string.Format("{0:#,##0.00}", asset.MineOreRatio));
                    writer.RenderEndTag(); // Td
                 
                    writer.RenderEndTag(); // Tr
                }
                writer.RenderEndTag(); // Table 

                #endregion


                #region footer

                writer.RenderEndTag(); // Body
                writer.RenderEndTag(); // Html

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

        #region GetDdsImageToBase64

        private static string GetDdsImageToBase64(string filename, int width, int height)
        {
            var bmp = ImageTextureUtil.CreateBitmap(filename, 0, width, height);
            var converter = new ImageConverter();
            return Convert.ToBase64String((byte[])converter.ConvertTo(bmp, typeof(byte[])));
        }

        #endregion

        #endregion
    }
}
