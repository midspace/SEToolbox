namespace SEToolbox.ViewModels
{
    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows.Input;

    public class StructureCharacterViewModel : StructureBaseViewModel<StructureCharacterModel>
    {
        #region ctor

        public StructureCharacterViewModel(BaseViewModel parentViewModel, StructureCharacterModel dataModel)
            : base(parentViewModel, dataModel)
        {
            this.DataModel.PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
            {
                // Will bubble property change events from the Model to the ViewModel.
                this.OnPropertyChanged(e.PropertyName);
            };
        }

        #endregion

        #region Properties

        #region command Properties

        public ICommand ResetVelocityCommand
        {
            get
            {
                return new DelegateCommand(new Action(ResetVelocityExecuted), new Func<bool>(ResetVelocityCanExecute));
            }
        }

        public ICommand ReverseVelocityCommand
        {
            get
            {
                return new DelegateCommand(new Action(ReverseVelocityExecuted), new Func<bool>(ReverseVelocityCanExecute));
            }
        }

        #endregion

        protected new StructureCharacterModel DataModel
        {
            get
            {
                return base.DataModel as StructureCharacterModel;
            }
        }

        public bool IsPilot
        {
            get
            {
                return this.DataModel.IsPilot;
            }

            set
            {
                this.DataModel.IsPilot = value;
            }
        }

        public bool IsPlayer
        {
            get
            {
                return this.DataModel.IsPlayer;
            }

            set
            {
                this.DataModel.IsPlayer = value;
            }
        }

        public string CharacterModel
        {
            get
            {
                return this.DataModel.CharacterModel.ToString();
            }

            set
            {
                this.DataModel.CharacterModel = (MyCharacterModelEnum)Enum.Parse(typeof(MyCharacterModelEnum), value);
                this.MainViewModel.IsModified = true;
            }
        }

        public bool Light
        {
            get
            {
                return this.DataModel.Light;
            }

            set
            {
                this.DataModel.Light = value;
                this.MainViewModel.IsModified = true;
            }
        }

        public bool JetPack
        {
            get
            {
                return this.DataModel.JetPack;
            }

            set
            {
                this.DataModel.JetPack = value;
                this.MainViewModel.IsModified = true;
            }
        }

        public bool Dampeners
        {
            get
            {
                return this.DataModel.Dampeners;
            }

            set
            {
                this.DataModel.Dampeners = value;
                this.MainViewModel.IsModified = true;
            }
        }

        public List<string> CharacterModels
        {
            get
            {
                return this.DataModel.CharacterModels;
            }

            set
            {
                this.DataModel.CharacterModels = value;
            }
        }

        public double Speed
        {
            get
            {
                return this.DataModel.Speed;
            }
        }

        public double PositionX
        {
            get
            {
                return this.DataModel.PositionX;
            }

            set
            {
                this.DataModel.PositionX = value;
                this.MainViewModel.IsModified = true;
                this.MainViewModel.CalcDistances();
            }
        }

        public double PositionY
        {
            get
            {
                return this.DataModel.PositionY;
            }

            set
            {
                this.DataModel.PositionY = value;
                this.MainViewModel.IsModified = true;
                this.MainViewModel.CalcDistances();
            }
        }

        public double PositionZ
        {
            get
            {
                return this.DataModel.PositionZ;
            }

            set
            {
                this.DataModel.PositionZ = value;
                this.MainViewModel.IsModified = true;
                this.MainViewModel.CalcDistances();
            }
        }

        //public BindablePoint3DModel Position
        //{
        //    get
        //    {
        //        return this.DataModel.Position;
        //    }

        //    set
        //    {
        //        this.DataModel.Position = value;
        //    }
        //}

        #endregion

        #region methods

        public bool ResetVelocityCanExecute()
        {
            return this.DataModel.Speed != 0f;
        }

        public void ResetVelocityExecuted()
        {
            this.DataModel.ResetVelocity();
            this.MainViewModel.IsModified = true;
        }

        public bool ReverseVelocityCanExecute()
        {
            return this.DataModel.Speed != 0f;
        }

        public void ReverseVelocityExecuted()
        {
            this.DataModel.ReverseVelocity();
            this.MainViewModel.IsModified = true;
        }

        #endregion
    }
}
