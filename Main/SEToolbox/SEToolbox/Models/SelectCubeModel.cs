namespace SEToolbox.Models
{
    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.Interop;
    using SEToolbox.Support;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;

    public class SelectCubeModel : BaseModel
    {
        #region Fields

        private ObservableCollection<ComponentItemModel> _cubeList;
        private ComponentItemModel _cubeItem;

        #endregion

        #region ctor

        public SelectCubeModel()
        {
            this._cubeList = new ObservableCollection<ComponentItemModel>();
        }

        #endregion

        #region Properties

        public ObservableCollection<ComponentItemModel> CubeList
        {
            get
            {
                return this._cubeList;
            }

            set
            {
                if (value != this._cubeList)
                {
                    this._cubeList = value;
                    this.RaisePropertyChanged(() => CubeList);
                }
            }
        }

        public ComponentItemModel CubeItem
        {
            get
            {
                return this._cubeItem;
            }

            set
            {
                if (value != this._cubeItem)
                {
                    this._cubeItem = value;
                    this.RaisePropertyChanged(() => CubeItem);
                }
            }
        }

        #endregion

        #region methods

        public void Load(MyCubeSize cubeSize, MyObjectBuilderType typeId, string subTypeId)
        {
            this.CubeList.Clear();
            var list = new SortedList<string, ComponentItemModel>();
            var contentPath = ToolboxUpdater.GetApplicationContentPath();
            var cubeDefinitions = SpaceEngineersCore.Definitions.CubeBlocks.Where(c => c.CubeSize == cubeSize);

            foreach (var cubeDefinition in cubeDefinitions)
            {
                var c = new ComponentItemModel()
                {
                    Name = cubeDefinition.DisplayName,
                    TypeId = cubeDefinition.Id.TypeId,
                    SubtypeId = cubeDefinition.Id.SubtypeId,
                    TextureFile = SpaceEngineersCore.GetDataPathOrDefault(cubeDefinition.Icon, Path.Combine(contentPath, cubeDefinition.Icon)),
                    Time = new TimeSpan((long)(TimeSpan.TicksPerSecond * cubeDefinition.BuildTimeSeconds)),
                    Accessible = cubeDefinition.Public,
                    Mass = SpaceEngineersApi.FetchCubeBlockMass(cubeDefinition.Id.TypeId, cubeDefinition.CubeSize, cubeDefinition.Id.SubtypeId),
                    CubeSize = cubeDefinition.CubeSize,
                    Size = new BindableSize3DIModel(cubeDefinition.Size),
                };

                list.Add(c.FriendlyName + c.SubtypeId, c);
            }

            foreach (var kvp in list)
            {
                this.CubeList.Add(kvp.Value);
            }

            this.CubeItem = this.CubeList.FirstOrDefault(c => c.TypeId == typeId && c.SubtypeId == subTypeId);
        }

        #endregion
    }
}
