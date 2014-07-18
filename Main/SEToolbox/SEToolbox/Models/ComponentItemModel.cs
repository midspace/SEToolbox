namespace SEToolbox.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
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
            get { return this._name; }
            set
            {
                if (value != this._name)
                {
                    this._name = value;
                    this.FriendlyName = SpaceEngineersAPI.GetResourceName(this.Name);
                    this.RaisePropertyChanged(() => Name);
                }
            }
        }

        public MyObjectBuilderType TypeId { get; set; }

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
            return this.FriendlyName;
        }

        #endregion
    }
}
