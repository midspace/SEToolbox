namespace SEToolbox.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Web.UI;

    using SEToolbox.Converters;
    using SEToolbox.ImageLibrary;
    using SEToolbox.Interop;
    using SEToolbox.Support;
    using Sandbox.Common.ObjectBuilders;

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
                    RaisePropertyChanged(() => CubeAssets);
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
                    RaisePropertyChanged(() => ComponentAssets);
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
                    RaisePropertyChanged(() => ItemAssets);
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
                    RaisePropertyChanged(() => MaterialAssets);
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
                    RaisePropertyChanged(() => IsBusy);
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
                    RaisePropertyChanged(() => SelectedCubeAsset);
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

            foreach (var cubeDefinition in SpaceEngineersCore.Resources.Definitions.CubeBlocks)
            {
                var props = new Dictionary<string, string>();
                var fields = cubeDefinition.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);

                foreach (var field in fields)
                {
                    props.Add(field.Name, GetValue(field, cubeDefinition));
                }

                CubeAssets.Add(new ComponentItemModel
                {
                    Name = cubeDefinition.DisplayName,
                    TypeId = cubeDefinition.Id.TypeId,
                    TypeIdString = cubeDefinition.Id.TypeIdString,
                    SubtypeId = cubeDefinition.Id.SubtypeId,
                    TextureFile = SpaceEngineersCore.GetDataPathOrDefault(cubeDefinition.Icon, Path.Combine(contentPath, cubeDefinition.Icon)),
                    Time = new TimeSpan((long)(TimeSpan.TicksPerSecond * cubeDefinition.BuildTimeSeconds)),
                    Accessible = cubeDefinition.Public,
                    Mass = SpaceEngineersApi.FetchCubeBlockMass(cubeDefinition.Id.TypeId, cubeDefinition.CubeSize, cubeDefinition.Id.SubtypeId),
                    CubeSize = cubeDefinition.CubeSize,
                    Size = new BindableSize3DIModel(cubeDefinition.Size),
                    CustomProperties = props,
                });
            }

            foreach (var componentDefinition in SpaceEngineersCore.Resources.Definitions.Components)
            {
                var bp = SpaceEngineersApi.GetBlueprint(componentDefinition.Id.TypeId, componentDefinition.Id.SubtypeId);
                ComponentAssets.Add(new ComponentItemModel
                {
                    Name = componentDefinition.DisplayName,
                    TypeId = componentDefinition.Id.TypeId,
                    TypeIdString = componentDefinition.Id.TypeIdString,
                    SubtypeId = componentDefinition.Id.SubtypeId,
                    Mass = componentDefinition.Mass,
                    TextureFile = SpaceEngineersCore.GetDataPathOrDefault(componentDefinition.Icon, Path.Combine(contentPath, componentDefinition.Icon)),
                    Volume = componentDefinition.Volume.HasValue ? componentDefinition.Volume.Value : 0f,
                    Accessible = componentDefinition.Public,
                    Time = bp != null ? new TimeSpan((long)(TimeSpan.TicksPerSecond * (bp.BaseProductionTimeInSeconds / float.Parse(bp.Result.Amount, CultureInfo.InvariantCulture)))) : (TimeSpan?)null,
                });
            }

            foreach (var physicalItemDefinition in SpaceEngineersCore.Resources.Definitions.PhysicalItems)
            {
                var bp = SpaceEngineersApi.GetBlueprint(physicalItemDefinition.Id.TypeId, physicalItemDefinition.Id.SubtypeId);

                float timeMassMultiplyer = 1f;
                if (physicalItemDefinition.Id.TypeId == typeof(MyObjectBuilder_Ore)
                    || physicalItemDefinition.Id.TypeId == typeof(MyObjectBuilder_Ingot))
                    timeMassMultiplyer = physicalItemDefinition.Mass;

                ItemAssets.Add(new ComponentItemModel
                {
                    Name = physicalItemDefinition.DisplayName,
                    TypeId = physicalItemDefinition.Id.TypeId,
                    TypeIdString = physicalItemDefinition.Id.TypeIdString,
                    SubtypeId = physicalItemDefinition.Id.SubtypeId,
                    Mass = physicalItemDefinition.Mass,
                    Volume = physicalItemDefinition.Volume.HasValue ? physicalItemDefinition.Volume.Value : 0f,
                    TextureFile = physicalItemDefinition.Icon == null ? null : SpaceEngineersCore.GetDataPathOrDefault(physicalItemDefinition.Icon, Path.Combine(contentPath, physicalItemDefinition.Icon)),
                    Accessible = physicalItemDefinition.Public,
                    Time = bp != null && bp.Result != null ? new TimeSpan((long)(TimeSpan.TicksPerSecond * (bp.BaseProductionTimeInSeconds / float.Parse(bp.Result.Amount, CultureInfo.InvariantCulture) / timeMassMultiplyer))) : (TimeSpan?)null,
                });
            }

            foreach (var physicalItemDefinition in SpaceEngineersCore.Resources.Definitions.AmmoMagazines)
            {
                var bp = SpaceEngineersApi.GetBlueprint(physicalItemDefinition.Id.TypeId, physicalItemDefinition.Id.SubtypeId);
                ItemAssets.Add(new ComponentItemModel
                {
                    Name = physicalItemDefinition.DisplayName,
                    TypeId = physicalItemDefinition.Id.TypeId,
                    TypeIdString = physicalItemDefinition.Id.TypeIdString,
                    SubtypeId = physicalItemDefinition.Id.SubtypeId,
                    Mass = physicalItemDefinition.Mass,
                    Volume = physicalItemDefinition.Volume.HasValue ? physicalItemDefinition.Volume.Value : 0f,
                    TextureFile = SpaceEngineersCore.GetDataPathOrDefault(physicalItemDefinition.Icon, Path.Combine(contentPath, physicalItemDefinition.Icon)),
                    Accessible = !string.IsNullOrEmpty(physicalItemDefinition.Model),
                    Time = bp != null ? new TimeSpan((long)(TimeSpan.TicksPerSecond * (bp.BaseProductionTimeInSeconds / float.Parse(bp.Result.Amount, CultureInfo.InvariantCulture)))) : (TimeSpan?)null,
                });
            }

            foreach (var voxelMaterialDefinition in SpaceEngineersCore.Resources.Definitions.VoxelMaterials)
            {
                var texture = voxelMaterialDefinition.DiffuseXZ;
                MaterialAssets.Add(new ComponentItemModel
                {
                    Name = voxelMaterialDefinition.Id.SubtypeId,
                    TextureFile = SpaceEngineersCore.GetDataPathOrDefault(texture, Path.Combine(contentPath, texture)),
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
                    new[] { "Icon", "Name", "Type Id", "Sub Type Id", "Cube Size", "Accessible", "Size (W×H×D)", "Mass (Kg)", "Build Time (h:m:s)" });

                foreach (var asset in CubeAssets)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);

                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    if (asset.TextureFile != null && File.Exists(asset.TextureFile))
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
                    new[] { "Icon", "Name", "Type Id", "Sub Type Id", "Accessible", "Mass (Kg)", "Volume (L)", "Build Time (h:m:s)" });

                foreach (var asset in ComponentAssets)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);

                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    if (asset.TextureFile != null && File.Exists(asset.TextureFile))
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
                    new[] { "Icon", "Name", "Type Id", "Sub Type Id", "Accessible", "Mass (Kg)", "Volume (L)", "Build Time (h:m:s)" });

                foreach (var asset in ItemAssets)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);

                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    if (asset.TextureFile != null && File.Exists(asset.TextureFile))
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
                    new[] { "Texture", "Name", "Ore Name", "Rare", "Mined Ore Ratio" });

                foreach (var asset in MaterialAssets)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);

                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    if (asset.TextureFile != null && File.Exists(asset.TextureFile))
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

        #region GetRowValue

        public static string GetValue(FieldInfo field, object objt)
        {
            var item = field.GetValue(objt);

            if (field.FieldType == typeof(Sandbox.Common.ObjectBuilders.VRageData.SerializableVector3I))
            {
                var vector = (Sandbox.Common.ObjectBuilders.VRageData.SerializableVector3I)item;
                return string.Format("{0}, {1}, {2}", vector.X, vector.Y, vector.Z);
            }

            if (field.FieldType == typeof(Sandbox.Common.ObjectBuilders.VRageData.SerializableVector3))
            {
                var vector = (Sandbox.Common.ObjectBuilders.VRageData.SerializableVector3)item;
                return string.Format("{0}, {1}, {2}", vector.X, vector.Y, vector.Z);
            }

            if (field.FieldType == typeof(Sandbox.Common.ObjectBuilders.VRageData.SerializableBounds))
            {
                var bounds = (Sandbox.Common.ObjectBuilders.VRageData.SerializableBounds)item;
                return string.Format("Default:{0}, Min:{1}, max:{2}", bounds.Default, bounds.Min, bounds.Max);
            }

            if (field.FieldType == typeof(VRageMath.Vector3))
            {
                var vector3 = (VRageMath.Vector3)item;
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
