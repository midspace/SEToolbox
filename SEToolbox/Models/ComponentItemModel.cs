namespace SEToolbox.Models
{
    using System;
    using System.Collections.Generic;
    using SEToolbox.Interop;
    using VRage.Game;
    using VRage.ObjectBuilders;

    [Serializable]
    public class ComponentItemModel : BaseModel
    {
        #region fields

        private string _name;

        private string _oreName;

        #endregion

        #region Properties

        public string Name
        {
            get { return _name; }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    FriendlyName = SpaceEngineersApi.GetResourceName(_name);
                    OnPropertyChanged(nameof(Name));
                    OnPropertyChanged(nameof(FriendlyName));
                }
            }
        }

        public object Definition { get; set; }

        public MyObjectBuilderType TypeId { get; set; }

        public string TypeIdString { get; set; }

        public string SubtypeId { get; set; }

        public double Mass { get; set; }

        public Decimal Count { get; set; }

        public double Volume { get; set; }

        public TimeSpan? Time { get; set; }

        public string TextureFile { get; set; }

        public MyCubeSize? CubeSize { get; set; }

        public BindableSize3DIModel Size { get; set; }

        public bool Accessible { get; set; }

        public bool IsRare { get; set; }

        public string OreName
        {
            get { return _oreName; }
            set
            {
                if (value != _oreName)
                {
                    _oreName = value;
                    FriendlyOreName = SpaceEngineersApi.GetResourceName(OreName);
                    OnPropertyChanged(nameof(OreName), nameof(FriendlyOreName));
                }
            }
        }

        public string FriendlyOreName { get; set; }

        public float MineOreRatio { get; set; }

        public Dictionary<string, string> CustomProperties { get; set; }

        public string FriendlyName { get; set; }

        public override string ToString()
        {
            return FriendlyName;
        }

        public bool IsMod { get; set; }

        public int PCU { get; set; }

        #endregion
    }
}
