namespace SEToolbox.Models
{
    using Sandbox.Common.ObjectBuilders;
    using System;

    [Serializable]
    public class ComonentItemModel : BaseModel
    {
        #region Properties

        public string Name { get; set; }

        public string TypeId { get; set; }

        public string SubtypeId { get; set; }

        public double Mass { get; set; }

        public double Volume { get; set; }

        public TimeSpan? Time { get; set; }

        public string TextureFile { get; set; }

        public MyCubeSize CubeSize { get; set; }

        public BindableSize3DIModel Size { get; set; }

        public bool Accessible { get; set; }

        public bool IsRare { get; set; }

        public string OreName { get; set; }

        public float MineOreRatio { get; set; }

        #endregion
    }
}
