namespace SEToolbox.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Sandbox.Common.ObjectBuilders;

    [Serializable]
    public class ComonentItemModel : BaseModel
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
                    this.SetFriendlyName();
                    this.RaisePropertyChanged(() => Name);
                }
            }
        }

        public MyObjectBuilderTypeEnum TypeId { get; set; }

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

        public Dictionary<string, string> CustomProperties { get; set; }

        public string FriendlyName { get; set; }

        public override string ToString()
        {
            return this.FriendlyName;
        }

        #endregion

        private void SetFriendlyName()
        {
            if (this.Name == null)
                this.FriendlyName = null;
            else
            {
                var field = Regex.Replace(this.Name, @"^Item_", "");
                this.FriendlyName = SplitPropertText(field);
            }
        }

        private static string SplitPropertText(string content)
        {
            var replacement = content;
            replacement = Regex.Replace(replacement, @"[_:]", "", RegexOptions.Multiline);
            replacement = Regex.Replace(replacement, @"(?<first>[a-z])(?<last>[A-Z])", "${first} ${last}", RegexOptions.Multiline);
            replacement = Regex.Replace(replacement, @"(?<first>[a-zA-Z])(?<last>\d)", "${first} ${last}", RegexOptions.Multiline);
            replacement = Regex.Replace(replacement, @"(?<first>\w)\.(?<last>\w)", "${first} ${last}", RegexOptions.Multiline);
            return replacement;
        }
    }
}
