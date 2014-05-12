namespace SEToolbox.Models
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Common.ObjectBuilders.Definitions;
    using Sandbox.Common.ObjectBuilders.VRageData;
    using SEToolbox.Interop;

    public class CubeItemModel : BaseModel
    {
        #region fields

        private double _buildPercent;
        private System.Windows.Media.Brush _color;
        private string _textureFile;

        #endregion

        #region ctor

        public CubeItemModel(MyObjectBuilder_CubeBlock cube, MyCubeSize cubeSize, MyObjectBuilder_CubeBlockDefinition definition)
        {
            this.SetProperties(cube, cubeSize, definition);
        }

        #endregion

        #region Properties

        // TODO: make these field backed properties.

        public bool IsSelected { get; set; }

        public MyObjectBuilder_CubeBlock Cube { get; set; }

        public MyObjectBuilderTypeEnum TypeId { get; set; }

        public string SubtypeId { get; set; }

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

        public MyCubeSize CubeSize { get; set; }

        public string FriendlyName { get; set; }

        public string ColorText { get; set; }

        public float ColorHue { get; set; }

        public float ColorSaturation { get; set; }

        public BindablePoint3DIModel Position { get; set; }

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

            this.RaisePropertyChanged(() => Cube);
            this.RaisePropertyChanged(() => CubeSize);
            this.RaisePropertyChanged(() => FriendlyName);
            this.RaisePropertyChanged(() => TypeId);
            this.RaisePropertyChanged(() => SubtypeId);
            this.RaisePropertyChanged(() => Position);
            this.RaisePropertyChanged(() => BuildPercent);
        }
    }
}
