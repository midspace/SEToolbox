namespace SEToolbox.Models
{
    using System.Drawing;

    public class BindableSizeModel : BaseModel
    {
        private Size _size;

        public BindableSizeModel()
        {
            _size = new Size();
        }

        public BindableSizeModel(int width, int height)
            : this()
        {
            Width = width;
            Height = height;
        }

        public BindableSizeModel(Size size)
            : this()
        {
            Width = size.Width;
            Height = size.Height;
        }

        #region Properties

        public int Width
        {
            get
            {
                return _size.Width;
            }

            set
            {
                if (value != _size.Width)
                {
                    _size.Width = value;
                    OnPropertyChanged(nameof(Width));
                }
            }
        }

        public int Height
        {
            get
            {
                return _size.Height;
            }

            set
            {
                if (value != _size.Height)
                {
                    _size.Height = value;
                    OnPropertyChanged(nameof(Height));
                }
            }
        }

        public Size Size
        {
            get
            {
                return _size;
            }

            set
            {
                if (value != _size)
                {
                    _size = value;
                    OnPropertyChanged(nameof(Size), nameof(Width), nameof(Height));
                }
            }
        }

        #endregion
    }
}
