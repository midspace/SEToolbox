namespace SEToolbox.Models
{
    using System.Drawing;

    public class BindableSizeModel : BaseModel
    {
        private Size size;

        public BindableSizeModel()
        {
            this.size = new Size();
        }

        public BindableSizeModel(int width, int height)
            : this()
        {
            this.Width = width;
            this.Height = height;
        }

        public BindableSizeModel(Size size)
            : this()
        {
            this.Width = size.Width;
            this.Height = size.Height;
        }

        #region Properties

        public int Width
        {
            get
            {
                return this.size.Width;
            }

            set
            {
                if (value != this.size.Width)
                {
                    this.size.Width = value;
                    this.RaisePropertyChanged(() => Width);
                }
            }
        }

        public int Height
        {
            get
            {
                return this.size.Height;
            }

            set
            {
                if (value != this.size.Height)
                {
                    this.size.Height = value;
                    this.RaisePropertyChanged(() => Height);
                }
            }
        }

        public Size Size
        {
            get
            {
                return this.size;
            }

            set
            {
                if (value != this.size)
                {
                    this.size = value;
                    this.RaisePropertyChanged(() => Size, () => Width, () => Height);
                }
            }
        }

        #endregion
    }
}
