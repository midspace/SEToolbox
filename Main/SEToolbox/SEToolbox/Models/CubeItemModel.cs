namespace SEToolbox.Models
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Common.ObjectBuilders.Definitions;
    using Sandbox.Common.ObjectBuilders.VRageData;
    using SEToolbox.Interop;

    public class CubeItemModel : BaseModel
    {
        #region fields

        private System.Windows.Media.Brush _color;

        #endregion

        #region ctor

        public CubeItemModel(MyObjectBuilder_CubeBlock cube, MyCubeSize cubeSize, MyObjectBuilder_CubeBlockDefinition definition)
        {
            this.Cube = cube;
            this.CubeSize = cubeSize;
            this.FriendlyName = SpaceEngineersAPI.GetResourceName(definition.DisplayName);
            this.TypeId = definition.TypeId;
            this.SubtypeId = definition.SubtypeName;
            this.Position = new BindablePoint3DIModel(cube.Min);
            this.SetColor(cube.ColorMaskHSV);
            this.Build = cube.BuildPercent;
        }

        #endregion

        #region Properties

        public bool IsSelected { get; set; }

        public MyObjectBuilder_CubeBlock Cube { get; set; }

        public MyObjectBuilderTypeEnum TypeId { get; set; }

        public string SubtypeId { get; set; }

        public string TextureFile { get; set; }

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

        public double Build { get; set; }

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
    }
}
