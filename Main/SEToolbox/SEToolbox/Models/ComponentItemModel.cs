namespace SEToolbox.Models
{
    using System;
    using System.Collections.Generic;

    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.Interop;

    [Serializable]
    public class ComponentItemModel : BaseModel
    {
        #region fields

        private string _name;

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
                    FriendlyName = SpaceEngineersApi.GetResourceName(Name);
                    RaisePropertyChanged(() => Name);
                }
            }
        }

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

        public string OreName { get; set; }

        public float MineOreRatio { get; set; }

        public Dictionary<string, string> CustomProperties { get; set; }

        public string FriendlyName { get; set; }

        public override string ToString()
        {
            return FriendlyName;
        }

        #endregion
    }
}
