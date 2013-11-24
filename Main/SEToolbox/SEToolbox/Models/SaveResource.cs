namespace SEToolbox.Models
{
    using Sandbox.CommonLib.ObjectBuilders;
    using SEToolbox.Interop;
    using System;
    using System.IO;

    public class SaveResource : BaseModel
    {
        #region Fields

        private string username;
        private string savename;
        private string savepath;
        private MyObjectBuilder_Checkpoint content;

        #endregion

        #region Properties

        public string Username
        {
            get
            {
                return this.username;
            }

            set
            {
                if (value != this.username)
                {
                    this.username = value;
                    this.RaisePropertyChanged(() => Username);
                }
            }
        }

        public string Savename
        {
            get
            {
                return this.savename;
            }

            set
            {
                if (value != this.savename)
                {
                    this.savename = value;
                    this.RaisePropertyChanged(() => Savename);
                }
            }
        }

        public string Savepath
        {
            get
            {
                return this.savepath;
            }

            set
            {
                if (value != this.savepath)
                {
                    this.savepath = value;
                    this.RaisePropertyChanged(() => Savepath);
                }
            }
        }

        public MyObjectBuilder_Checkpoint Content
        {
            get
            {
                return this.content;
            }

            set
            {
                if (value != this.content)
                {
                    this.content = value;
                    this.RaisePropertyChanged(() => Content, () => SessionName, () => LastSaveTime, () => IsValid);
                }
            }
        }

        public string SessionName
        {
            get
            {
                if (this.content == null)
                    return " # Invalid Save # ";

                return this.content.SessionName;
            }

            set
            {
                if (value != this.content.SessionName)
                {
                    this.content.SessionName = value;
                    this.RaisePropertyChanged(() => SessionName);
                }
            }
        }

        public DateTime? LastSaveTime
        {
            get
            {
                if (this.content == null)
                    return null;

                return this.content.LastSaveTime;
            }

            set
            {
                if (value != this.content.LastSaveTime)
                {
                    this.content.LastSaveTime = value.Value;
                    this.RaisePropertyChanged(() => LastSaveTime);
                }
            }
        }


        public bool IsValid
        {
            get
            {
                return this.Content != null;
            }
        }

        public string ThumbnailImageFilename
        {
            get
            {
                return Path.Combine(this.savepath, SpaceEngineersConsts.ThumbnailImageFilename);
            }
        }

        #endregion
    }
}
