namespace SEToolbox.Models
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using SEToolbox.Interop;
    using VRage;
    using VRage.Game;
    using VRage.ObjectBuilders;
    using Res = SEToolbox.Properties.Resources;

    public class CubeItemModel : BaseModel
    {
        #region fields

        private MyObjectBuilder_CubeBlock _cube;

        private MyObjectBuilderType _typeId;

        private string _subtypeId;

        private string _textureFile;

        private MyCubeSize _cubeSize;

        private string _friendlyName;

        private string _ownerName;

        private string _builtByName;

        private string _colorText;

        private float _colorHue;

        private float _colorSaturation;

        private float _colorLuminance;

        private BindablePoint3DIModel _position;

        private double _buildPercent;

        private System.Windows.Media.Brush _color;

        private int _pcu;

        private ObservableCollection<InventoryEditorModel> _inventory;

        #endregion

        #region ctor

        public CubeItemModel(MyObjectBuilder_CubeBlock cube, MyCubeBlockDefinition definition)
        {
            SetProperties(cube, definition);
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
                    OnPropertyChanged(nameof(Cube));
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
                    OnPropertyChanged(nameof(Owner));
                }
            }
        }

        public long BuiltBy
        {
            get { return _cube.BuiltBy; }
            set
            {
                if (value != _cube.BuiltBy)
                {
                    _cube.BuiltBy = value;
                    OnPropertyChanged(nameof(BuiltBy));
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
                    OnPropertyChanged(nameof(TypeId));
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
                    OnPropertyChanged(nameof(SubtypeId));
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
                    OnPropertyChanged(nameof(TextureFile));
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
                    OnPropertyChanged(nameof(CubeSize));
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
                    OnPropertyChanged(nameof(FriendlyName));
                }
            }
        }

        public string OwnerName
        {
            get { return _ownerName; }

            set
            {
                if (value != _ownerName)
                {
                    _ownerName = value;
                    OnPropertyChanged(nameof(OwnerName));
                }
            }
        }

        public string BuiltByName
        {
            get { return _builtByName; }

            set
            {
                if (value != _builtByName)
                {
                    _builtByName = value;
                    OnPropertyChanged(nameof(BuiltByName));
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
                    OnPropertyChanged(nameof(ColorText));
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
                    OnPropertyChanged(nameof(ColorHue));
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
                    OnPropertyChanged(nameof(ColorSaturation));
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
                    OnPropertyChanged(nameof(ColorLuminance));
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
                    OnPropertyChanged(nameof(Position));
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
                    OnPropertyChanged(nameof(BuildPercent));
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
                    OnPropertyChanged(nameof(Color));
                }
            }
        }

        public int PCU
        {
            get { return _pcu; }

            set
            {
                if (value != _pcu)
                {
                    _pcu = value;
                    OnPropertyChanged(nameof(PCU));
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
                    OnPropertyChanged(nameof(Inventory));
                }
            }
        }

        #endregion

        public void SetColor(SerializableVector3 vector3)
        {
            Color = new System.Windows.Media.SolidColorBrush(vector3.FromHsvMaskToPaletteMediaColor());
            ColorText = Color.ToString();
            ColorHue = vector3.X;
            ColorSaturation = vector3.Y;
            ColorLuminance = vector3.Z;

            OnPropertyChanged(nameof(ColorText), nameof(ColorHue), nameof(ColorSaturation), nameof(ColorLuminance));
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

        public static bool ConvertFromLightToHeavyArmor(MyObjectBuilder_CubeBlock cube)
        {
            if (cube.SubtypeName.StartsWith("LargeBlockArmor"))
            {
                var newSubTypeName = cube.SubtypeName.Replace("LargeBlockArmor", "LargeHeavyBlockArmor");
                if (cube.SubtypeName != newSubTypeName && SpaceEngineersCore.Resources.CubeBlockDefinitions.Any(b => b.Id.TypeId == cube.TypeId && b.Id.SubtypeName == newSubTypeName))
                {
                    cube.SubtypeName = newSubTypeName;
                    return true;
                }
            }
            else if (cube.SubtypeName.StartsWith("Large") && (cube.SubtypeName.EndsWith("HalfArmorBlock") || cube.SubtypeName.EndsWith("HalfSlopeArmorBlock")))
            {
                var newSubTypeName = cube.SubtypeName.Replace("LargeHalf", "LargeHeavyHalf");
                if (cube.SubtypeName != newSubTypeName && SpaceEngineersCore.Resources.CubeBlockDefinitions.Any(b => b.Id.TypeId == cube.TypeId && b.Id.SubtypeName == newSubTypeName))
                {
                    cube.SubtypeName = newSubTypeName;
                    return true;
                }
            }
            else if (cube.SubtypeName.StartsWith("SmallBlockArmor"))
            {
                var newSubTypeName = cube.SubtypeName.Replace("SmallBlockArmor", "SmallHeavyBlockArmor");
                if (cube.SubtypeName != newSubTypeName && SpaceEngineersCore.Resources.CubeBlockDefinitions.Any(b => b.Id.TypeId == cube.TypeId && b.Id.SubtypeName == newSubTypeName))
                {
                    cube.SubtypeName = newSubTypeName;
                    return true;
                }
            }
            else if (!cube.SubtypeName.StartsWith("Large") && (cube.SubtypeName.EndsWith("HalfArmorBlock") || cube.SubtypeName.EndsWith("HalfSlopeArmorBlock")))
            {
                var newSubTypeName = Regex.Replace(cube.SubtypeName, "^(Half)(.*)", "HeavyHalf$2", RegexOptions.IgnoreCase);
                if (cube.SubtypeName != newSubTypeName && SpaceEngineersCore.Resources.CubeBlockDefinitions.Any(b => b.Id.TypeId == cube.TypeId && b.Id.SubtypeName == newSubTypeName))
                {
                    cube.SubtypeName = newSubTypeName;
                    return true;
                }
            }

            return false;
        }

        public static bool ConvertFromHeavyToLightArmor(MyObjectBuilder_CubeBlock cube)
        {
            if (cube.SubtypeName.StartsWith("LargeHeavyBlockArmor"))
            {
                var newSubTypeName = cube.SubtypeName.Replace("LargeHeavyBlockArmor", "LargeBlockArmor");
                if (cube.SubtypeName != newSubTypeName && SpaceEngineersCore.Resources.CubeBlockDefinitions.Any(b => b.Id.TypeId == cube.TypeId && b.Id.SubtypeName == newSubTypeName))
                {
                    cube.SubtypeName = newSubTypeName;
                    return true;
                }
            }
            else if (cube.SubtypeName.StartsWith("Large") && (cube.SubtypeName.EndsWith("HalfArmorBlock") || cube.SubtypeName.EndsWith("HalfSlopeArmorBlock")))
            {
                var newSubTypeName = cube.SubtypeName.Replace("LargeHeavyHalf", "LargeHalf");
                if (cube.SubtypeName != newSubTypeName && SpaceEngineersCore.Resources.CubeBlockDefinitions.Any(b => b.Id.TypeId == cube.TypeId && b.Id.SubtypeName == newSubTypeName))
                {
                    cube.SubtypeName = newSubTypeName;
                    return true;
                }
            }
            else if (cube.SubtypeName.StartsWith("SmallHeavyBlockArmor"))
            {
                var newSubTypeName = cube.SubtypeName.Replace("SmallHeavyBlockArmor", "SmallBlockArmor");
                if (cube.SubtypeName != newSubTypeName && SpaceEngineersCore.Resources.CubeBlockDefinitions.Any(b => b.Id.TypeId == cube.TypeId && b.Id.SubtypeName == newSubTypeName))
                {
                    cube.SubtypeName = newSubTypeName;
                    return true;
                }
            }
            else if (!cube.SubtypeName.StartsWith("Large") && (cube.SubtypeName.EndsWith("HalfArmorBlock") || cube.SubtypeName.EndsWith("HalfSlopeArmorBlock")))
            {
                var newSubTypeName = Regex.Replace(cube.SubtypeName, "^(HeavyHalf)(.*)", "Half$2", RegexOptions.IgnoreCase);
                if (cube.SubtypeName != newSubTypeName && SpaceEngineersCore.Resources.CubeBlockDefinitions.Any(b => b.Id.TypeId == cube.TypeId && b.Id.SubtypeName == newSubTypeName))
                {
                    cube.SubtypeName = newSubTypeName;
                    return true;
                }
            }

            return false;
        }

        public MyObjectBuilder_CubeBlock CreateCube(MyObjectBuilderType typeId, string subTypeId, MyCubeBlockDefinition definition)
        {
            var newCube = SpaceEngineersCore.Resources.CreateNewObject<MyObjectBuilder_CubeBlock>(typeId, subTypeId);
            newCube.BlockOrientation = Cube.BlockOrientation;
            newCube.ColorMaskHSV = Cube.ColorMaskHSV;
            newCube.BuildPercent = Cube.BuildPercent;
            newCube.EntityId = Cube.EntityId;
            newCube.IntegrityPercent = Cube.IntegrityPercent;
            newCube.Min = Cube.Min;
            newCube.BuiltBy = Cube.BuiltBy;
            newCube.Owner = Cube.Owner;
            newCube.ShareMode = Cube.ShareMode;
            newCube.DeformationRatio = Cube.DeformationRatio;
            newCube.BlockGeneralDamageModifier = Cube.BlockGeneralDamageModifier;

            SetProperties(newCube, definition);

            return newCube;
        }

        public bool ChangeOwner(long newOwnerId)
        {
            // There appear to be quite a few exceptions, blocks that inherit from MyObjectBuilder_TerminalBlock but SE doesn't allow setting of Owner.
            if (Cube is MyObjectBuilder_InteriorLight
                || Cube is MyObjectBuilder_ReflectorLight
                || Cube is MyObjectBuilder_LandingGear
                || (Cube is MyObjectBuilder_Cockpit && SubtypeId == "PassengerSeatLarge")
                || Cube is MyObjectBuilder_Thrust)
                return false;

            if (Cube is MyObjectBuilder_TerminalBlock)
            {
                this.Owner = newOwnerId;

                var identity = SpaceEngineersCore.WorldResource.Checkpoint.Identities.FirstOrDefault(p => p.PlayerId == Owner);
                var dead = $" ({Res.ClsCharacterDead})";
                if (SpaceEngineersCore.WorldResource.Checkpoint.AllPlayersData != null)
                {
                    var player = SpaceEngineersCore.WorldResource.Checkpoint.AllPlayersData.Dictionary.FirstOrDefault(kvp => kvp.Value.IdentityId == Owner);
                    dead = player.Value == null ? $" ({Res.ClsCharacterDead})" : "";
                }
                OwnerName = identity == null ? null : identity.DisplayName + dead;
                return true;
            }

            return false;
        }

        public bool ChangeBuiltBy(long newOwnerId)
        {
            this.BuiltBy = newOwnerId;

            var identity = SpaceEngineersCore.WorldResource.Checkpoint.Identities.FirstOrDefault(p => p.PlayerId == BuiltBy);
            var dead = $" ({Res.ClsCharacterDead})";
            if (SpaceEngineersCore.WorldResource.Checkpoint.AllPlayersData != null)
            {
                var player = SpaceEngineersCore.WorldResource.Checkpoint.AllPlayersData.Dictionary.FirstOrDefault(kvp => kvp.Value.IdentityId == BuiltBy);
                dead = player.Value == null ? $" ({Res.ClsCharacterDead})" : "";
            }
            BuiltByName = identity == null ? null : identity.DisplayName + dead;
            return true;
        }

        private void SetProperties(MyObjectBuilder_CubeBlock cube, MyCubeBlockDefinition definition)
        {
            Cube = cube;
            Position = new BindablePoint3DIModel(cube.Min);
            SetColor(cube.ColorMaskHSV);
            BuildPercent = cube.BuildPercent;

            if (definition == null)
            {
                // Obsolete block or Mod not loaded.
                return;
            }

            CubeSize = definition.CubeSize;
            FriendlyName = SpaceEngineersApi.GetResourceName(definition.DisplayNameText);

            var ownerIdentity = SpaceEngineersCore.WorldResource.Checkpoint.Identities.FirstOrDefault(p => p.PlayerId == Owner);
            var buyiltByIdentity = SpaceEngineersCore.WorldResource.Checkpoint.Identities.FirstOrDefault(p => p.PlayerId == BuiltBy);
            var ownerDead = $" ({Res.ClsCharacterDead})";
            var builtByDead = $" ({Res.ClsCharacterDead})";
            if (SpaceEngineersCore.WorldResource.Checkpoint.AllPlayersData != null)
            {
                var ownerPlayer = SpaceEngineersCore.WorldResource.Checkpoint.AllPlayersData.Dictionary.FirstOrDefault(kvp => kvp.Value.IdentityId == Owner);
                ownerDead = ownerPlayer.Value == null ? $" ({Res.ClsCharacterDead})" : "";

                var builtByPlayer = SpaceEngineersCore.WorldResource.Checkpoint.AllPlayersData.Dictionary.FirstOrDefault(kvp => kvp.Value.IdentityId == BuiltBy);
                builtByDead = builtByPlayer.Value == null ? $" ({Res.ClsCharacterDead})" : "";
            }
            OwnerName = ownerIdentity == null ? null : ownerIdentity.DisplayName + ownerDead;
            BuiltByName = buyiltByIdentity == null ? null : buyiltByIdentity.DisplayName + builtByDead;

            TypeId = definition.Id.TypeId;
            SubtypeId = definition.Id.SubtypeName;
            PCU = definition.PCU;

            if (Inventory == null)
                Inventory = new ObservableCollection<InventoryEditorModel>();

            foreach (var item in cube.ComponentContainer.GetInventory(definition))
                Inventory.Add(item);

            while (Inventory.Count < 2)
            {
                Inventory.Add(new InventoryEditorModel(false));
            }
        }

    }
}
