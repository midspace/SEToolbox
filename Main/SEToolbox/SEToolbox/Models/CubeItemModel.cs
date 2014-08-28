namespace SEToolbox.Models
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reflection;

    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Common.ObjectBuilders.Definitions;
    using Sandbox.Common.ObjectBuilders.VRageData;
    using SEToolbox.Interop;
    using VRageMath;

    public class CubeItemModel : BaseModel
    {
        #region fields

        private MyObjectBuilder_CubeBlock _cube;

        private MyObjectBuilderType _typeId;

        private string _subtypeId;

        private string _textureFile;

        private MyCubeSize _cubeSize;

        private string _friendlyName;

        private string _colorText;

        private float _colorHue;

        private float _colorSaturation;

        private float _colorLuminance;

        private BindablePoint3DIModel _position;

        private double _buildPercent;

        private System.Windows.Media.Brush _color;

        private ObservableCollection<InventoryEditorModel> _inventory;

        private MySessionSettings _settings;

        #endregion

        #region ctor

        public CubeItemModel(MyObjectBuilder_CubeBlock cube, MyObjectBuilder_CubeBlockDefinition definition, MySessionSettings settings)
        {
            SetProperties(cube, definition, settings);
        }

        #endregion

        #region Properties

        public bool IsSelected { get; set; }

        public MyObjectBuilder_CubeBlock Cube
        {
            get { return _cube; }

            set
            {
                if (value != _cube)
                {
                    _cube = value;
                    RaisePropertyChanged(() => Cube);
                }
            }
        }

        public long Owner
        {
            get { return _cube.Owner; }
            set
            {
                if (value != _cube.Owner)
                {
                    _cube.Owner = value;
                    RaisePropertyChanged(() => Owner);
                }
            }
        }

        public MyObjectBuilderType TypeId
        {
            get { return _typeId; }

            set
            {
                if (value != _typeId)
                {
                    _typeId = value;
                    RaisePropertyChanged(() => TypeId);
                }
            }
        }

        public string SubtypeId
        {
            get { return _subtypeId; }

            set
            {
                if (value != _subtypeId)
                {
                    _subtypeId = value;
                    RaisePropertyChanged(() => SubtypeId);
                }
            }
        }

        public string TextureFile
        {
            get { return _textureFile; }

            set
            {
                if (value != _textureFile)
                {
                    _textureFile = value;
                    RaisePropertyChanged(() => TextureFile);
                }
            }
        }

        public MyCubeSize CubeSize
        {
            get { return _cubeSize; }

            set
            {
                if (value != _cubeSize)
                {
                    _cubeSize = value;
                    RaisePropertyChanged(() => CubeSize);
                }
            }
        }

        public string FriendlyName
        {
            get { return _friendlyName; }

            set
            {
                if (value != _friendlyName)
                {
                    _friendlyName = value;
                    RaisePropertyChanged(() => FriendlyName);
                }
            }
        }

        public string ColorText
        {
            get { return _colorText; }

            set
            {
                if (value != _colorText)
                {
                    _colorText = value;
                    RaisePropertyChanged(() => ColorText);
                }
            }
        }

        public float ColorHue
        {
            get { return _colorHue; }

            set
            {
                if (value != _colorHue)
                {
                    _colorHue = value;
                    RaisePropertyChanged(() => ColorHue);
                }
            }
        }

        public float ColorSaturation
        {
            get { return _colorSaturation; }

            set
            {
                if (value != _colorSaturation)
                {
                    _colorSaturation = value;
                    RaisePropertyChanged(() => ColorSaturation);
                }
            }
        }

        public float ColorLuminance
        {
            get { return _colorLuminance; }

            set
            {
                if (value != _colorLuminance)
                {
                    _colorLuminance = value;
                    RaisePropertyChanged(() => ColorLuminance);
                }
            }
        }

        public BindablePoint3DIModel Position
        {
            get { return _position; }

            set
            {
                if (value != _position)
                {
                    _position = value;
                    RaisePropertyChanged(() => Position);
                }
            }
        }

        public override string ToString()
        {
            return FriendlyName;
        }

        public double BuildPercent
        {
            get { return _buildPercent; }

            set
            {
                if (value != _buildPercent)
                {
                    _buildPercent = value;
                    RaisePropertyChanged(() => BuildPercent);
                }
            }
        }

        public System.Windows.Media.Brush Color
        {
            get { return _color; }

            set
            {
                if (value != _color)
                {
                    _color = value;
                    RaisePropertyChanged(() => Color);
                }
            }
        }

        public ObservableCollection<InventoryEditorModel> Inventory
        {
            get { return _inventory; }

            set
            {
                if (value != _inventory)
                {
                    _inventory = value;
                    RaisePropertyChanged(() => Inventory);
                }
            }
        }

        public MySessionSettings Settings
        {
            get { return _settings; }

            set
            {
                if (!EqualityComparer<MySessionSettings>.Default.Equals(value, _settings))
                {
                    _settings = value;
                    RaisePropertyChanged(() => Settings);
                }
            }
        }

        #endregion

        public void SetColor(SerializableVector3 vector3)
        {
            Color = new System.Windows.Media.SolidColorBrush(vector3.ToSandboxMediaColor());
            ColorText = Color.ToString();
            ColorHue = vector3.X;
            ColorSaturation = vector3.Y;
            ColorLuminance = vector3.Z;

            RaisePropertyChanged(() => ColorText);
            RaisePropertyChanged(() => ColorHue);
            RaisePropertyChanged(() => ColorSaturation);
            RaisePropertyChanged(() => ColorLuminance);
        }

        public void UpdateColor(SerializableVector3 vector3)
        {
            Cube.ColorMaskHSV = vector3;
            SetColor(vector3);
        }

        public void UpdateBuildPercent(double buildPercent)
        {
            Cube.IntegrityPercent = (float)buildPercent;
            Cube.BuildPercent = (float)buildPercent;
            BuildPercent = Cube.BuildPercent;
        }

        public MyObjectBuilder_CubeBlock CreateCube(MyObjectBuilderType typeId, string subTypeId, MyObjectBuilder_CubeBlockDefinition definition, MySessionSettings settings)
        {
            var newCube = (MyObjectBuilder_CubeBlock)MyObjectBuilder_Base.CreateNewObject(typeId, subTypeId);
            newCube.BlockOrientation = Cube.BlockOrientation;
            newCube.ColorMaskHSV = Cube.ColorMaskHSV;
            newCube.BuildPercent = Cube.BuildPercent;
            newCube.EntityId = Cube.EntityId;
            newCube.IntegrityPercent = Cube.IntegrityPercent;
            newCube.Min = Cube.Min;

            SetProperties(newCube, definition, settings);

            return newCube;
        }

        private void SetProperties(MyObjectBuilder_CubeBlock cube, MyObjectBuilder_CubeBlockDefinition definition, MySessionSettings settings)
        {
            Cube = cube;
            Settings = settings;
            Position = new BindablePoint3DIModel(cube.Min);
            SetColor(cube.ColorMaskHSV);
            BuildPercent = cube.BuildPercent;

            if (definition == null)
            {
                // Obsolete block or Mod not loaded.
                return;
            }

            CubeSize = definition.CubeSize;
            FriendlyName = SpaceEngineersApi.GetResourceName(definition.DisplayName);
            TypeId = definition.Id.TypeId;
            SubtypeId = definition.Id.SubtypeId;

            if (Inventory == null)
                Inventory = new ObservableCollection<InventoryEditorModel>();

            var blockType = cube.GetType();
            if (!blockType.Equals(typeof(MyObjectBuilder_CubeBlockDefinition)))
            {
                var fields = blockType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                var inventoryFields = fields.Where(f => f.FieldType == typeof(MyObjectBuilder_Inventory) && f.Name != "ConstructionInventory").ToArray();
                foreach (var field in inventoryFields)
                {
                    var inventory = field.GetValue(cube) as MyObjectBuilder_Inventory;

                    var definitionType = definition.GetType();
                    var invSizeField = definitionType.GetField("InventorySize");
                    var inventoryMaxVolumeField = definitionType.GetField("InventoryMaxVolume");
                    float volumeMultiplier = 1f; // Unsure if there should be a default of 1 if there isn't a InventorySize defined.
                    if (invSizeField != null)
                    {
                        var invSize = (Vector3)invSizeField.GetValue(definition);
                        volumeMultiplier = invSize.X * invSize.Y * invSize.Z;
                    }
                    if (inventoryMaxVolumeField != null)
                    {
                        var maxSize = (float)inventoryMaxVolumeField.GetValue(definition);
                        volumeMultiplier = MathHelper.Min(volumeMultiplier, maxSize);
                    }

                    var iem = new InventoryEditorModel(inventory, Settings, volumeMultiplier * 1000 * Settings.InventorySizeMultiplier, null) { Name = field.Name, IsValid = true };
                    Inventory.Add(iem);
                }
            }

            while (Inventory.Count < 2)
            {
                Inventory.Add(new InventoryEditorModel(false));
            }
        }
    }
}
