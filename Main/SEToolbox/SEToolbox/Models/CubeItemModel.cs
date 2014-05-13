namespace SEToolbox.Models
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Common.ObjectBuilders.Definitions;
    using Sandbox.Common.ObjectBuilders.VRageData;
    using SEToolbox.Interop;

    public class CubeItemModel : BaseModel
    {
        #region fields

        private MyObjectBuilder_CubeBlock _cube;

        private MyObjectBuilderTypeEnum _typeId;

        private string _subtypeId;

        private string _textureFile;

        private MyCubeSize _cubeSize;

        private string _friendlyName;

        private string _colorText;

        private float _colorHue;

        private float _colorSaturation;

        private BindablePoint3DIModel _position;

        private double _buildPercent;

        private System.Windows.Media.Brush _color;

        #endregion

        #region ctor

        public CubeItemModel(MyObjectBuilder_CubeBlock cube, MyCubeSize cubeSize, MyObjectBuilder_CubeBlockDefinition definition)
        {
            this.SetProperties(cube, cubeSize, definition);
        }

        #endregion

        #region Properties

        public bool IsSelected { get; set; }

        public MyObjectBuilder_CubeBlock Cube
        {
            get
            {
                return this._cube;
            }

            set
            {
                if (value != this._cube)
                {
                    this._cube = value;
                    this.RaisePropertyChanged(() => Cube);
                }
            }
        }

        public MyObjectBuilderTypeEnum TypeId
        {
            get
            {
                return this._typeId;
            }

            set
            {
                if (value != this._typeId)
                {
                    this._typeId = value;
                    this.RaisePropertyChanged(() => TypeId);
                }
            }
        }

        public string SubtypeId
        {
            get
            {
                return this._subtypeId;
            }

            set
            {
                if (value != this._subtypeId)
                {
                    this._subtypeId = value;
                    this.RaisePropertyChanged(() => SubtypeId);
                }
            }
        }

        public string TextureFile
        {
            get
            {
                return this._textureFile;
            }

            set
            {
                if (value != this._textureFile)
                {
                    this._textureFile = value;
                    this.RaisePropertyChanged(() => TextureFile);
                }
            }
        }

        public MyCubeSize CubeSize
        {
            get
            {
                return this._cubeSize;
            }

            set
            {
                if (value != this._cubeSize)
                {
                    this._cubeSize = value;
                    this.RaisePropertyChanged(() => CubeSize);
                }
            }
        }

        public string FriendlyName
        {
            get
            {
                return this._friendlyName;
            }

            set
            {
                if (value != this._friendlyName)
                {
                    this._friendlyName = value;
                    this.RaisePropertyChanged(() => FriendlyName);
                }
            }
        }

        public string ColorText
        {
            get
            {
                return this._colorText;
            }

            set
            {
                if (value != this._colorText)
                {
                    this._colorText = value;
                    this.RaisePropertyChanged(() => ColorText);
                }
            }
        }

        public float ColorHue
        {
            get
            {
                return this._colorHue;
            }

            set
            {
                if (value != this._colorHue)
                {
                    this._colorHue = value;
                    this.RaisePropertyChanged(() => ColorHue);
                }
            }
        }

        public float ColorSaturation
        {
            get
            {
                return this._colorSaturation;
            }

            set
            {
                if (value != this._colorSaturation)
                {
                    this._colorSaturation = value;
                    this.RaisePropertyChanged(() => ColorSaturation);
                }
            }
        }

        public BindablePoint3DIModel Position
        {
            get
            {
                return this._position;
            }

            set
            {
                if (value != this._position)
                {
                    this._position = value;
                    this.RaisePropertyChanged(() => Position);
                }
            }
        }

        public override string ToString()
        {
            return this.FriendlyName;
        }

        public double BuildPercent
        {
            get
            {
                return this._buildPercent;
            }

            set
            {
                if (value != this._buildPercent)
                {
                    this._buildPercent = value;
                    this.RaisePropertyChanged(() => BuildPercent);
                }
            }
        }

        public System.Windows.Media.Brush Color
        {
            get
            {
                return this._color;
            }

            set
            {
                if (value != this._color)
                {
                    this._color = value;
                    this.RaisePropertyChanged(() => Color);
                }
            }
        }

        #endregion

        public void SetColor(SerializableVector3 vector3)
        {
            this.Color = new System.Windows.Media.SolidColorBrush(vector3.ToSandboxMediaColor());
            this.ColorText = this.Color.ToString();
            this.ColorHue = vector3.X;
            this.ColorSaturation = vector3.Y;

            this.RaisePropertyChanged(() => ColorText);
            this.RaisePropertyChanged(() => ColorHue);
            this.RaisePropertyChanged(() => ColorSaturation);
        }

        public void UpdateColor(SerializableVector3 vector3)
        {
            this.Cube.ColorMaskHSV = vector3;
            SetColor(vector3);
        }

        public void UpdateBuildPercent(double buildPercent)
        {
            this.Cube.IntegrityPercent = (float)buildPercent;
            this.Cube.BuildPercent = (float)buildPercent;
            this.BuildPercent = this.Cube.BuildPercent;
        }

        public MyObjectBuilder_CubeBlock CreateCube(MyObjectBuilderTypeEnum typeId, string subTypeId, MyObjectBuilder_CubeBlockDefinition definition)
        {
            var newCube = (MyObjectBuilder_CubeBlock)MyObjectBuilder_Base.CreateNewObject(typeId, subTypeId);
            newCube.BlockOrientation = this.Cube.BlockOrientation;
            newCube.ColorMaskHSV = this.Cube.ColorMaskHSV;
            newCube.BuildPercent = this.Cube.BuildPercent;
            newCube.EntityId = this.Cube.EntityId;
            newCube.IntegrityPercent = this.Cube.IntegrityPercent;
            newCube.Min = this.Cube.Min;

            this.SetProperties(newCube, this.CubeSize, definition);

            return newCube;
        }

        private void SetProperties(MyObjectBuilder_CubeBlock cube, MyCubeSize cubeSize, MyObjectBuilder_CubeBlockDefinition definition)
        {
            this.Cube = cube;
            this.CubeSize = cubeSize;
            this.FriendlyName = SpaceEngineersAPI.GetResourceName(definition.DisplayName);
            this.TypeId = definition.Id.TypeId;
            this.SubtypeId = definition.Id.SubtypeId;
            this.Position = new BindablePoint3DIModel(cube.Min);
            this.SetColor(cube.ColorMaskHSV);
            this.BuildPercent = cube.BuildPercent;
        }
    }
}
