namespace SEToolbox.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;

    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.Interop;
    using SEToolbox.Support;
    using VRage.ObjectBuilders;

    public class SelectCubeModel : BaseModel
    {
        #region Fields

        private ObservableCollection<ComponentItemModel> _cubeList;
        private ComponentItemModel _cubeItem;

        #endregion

        #region ctor

        public SelectCubeModel()
        {
            _cubeList = new ObservableCollection<ComponentItemModel>();
        }

        #endregion

        #region Properties

        public ObservableCollection<ComponentItemModel> CubeList
        {
            get
            {
                return _cubeList;
            }

            set
            {
                if (value != _cubeList)
                {
                    _cubeList = value;
                    RaisePropertyChanged(() => CubeList);
                }
            }
        }

        public ComponentItemModel CubeItem
        {
            get
            {
                return _cubeItem;
            }

            set
            {
                if (value != _cubeItem)
                {
                    _cubeItem = value;
                    RaisePropertyChanged(() => CubeItem);
                }
            }
        }

        #endregion

        #region methods

        public void Load(MyCubeSize cubeSize, MyObjectBuilderType typeId, string subTypeId)
        {
            CubeList.Clear();
            var list = new SortedList<string, ComponentItemModel>();
            var contentPath = ToolboxUpdater.GetApplicationContentPath();
            var cubeDefinitions = SpaceEngineersCore.Resources.Definitions.CubeBlocks.Where(c => c.CubeSize == cubeSize);

            foreach (var cubeDefinition in cubeDefinitions)
            {
                var c = new ComponentItemModel
                {
                    Name = cubeDefinition.DisplayName,
                    TypeId = cubeDefinition.Id.TypeId,
                    TypeIdString = cubeDefinition.Id.TypeIdString,
                    SubtypeId = cubeDefinition.Id.SubtypeId,
                    TextureFile = SpaceEngineersCore.GetDataPathOrDefault(cubeDefinition.Icon, Path.Combine(contentPath, cubeDefinition.Icon)),
                    Time = new TimeSpan((long)(TimeSpan.TicksPerSecond * cubeDefinition.BuildTimeSeconds)),
                    Accessible = cubeDefinition.Public,
                    Mass = SpaceEngineersApi.FetchCubeBlockMass(cubeDefinition.Id.TypeId, cubeDefinition.CubeSize, cubeDefinition.Id.SubtypeId),
                    CubeSize = cubeDefinition.CubeSize,
                    Size = new BindableSize3DIModel(cubeDefinition.Size),
                };

                list.Add(c.FriendlyName + c.TypeIdString + c.SubtypeId, c);
            }

            foreach (var kvp in list)
            {
                CubeList.Add(kvp.Value);
            }

            CubeItem = CubeList.FirstOrDefault(c => c.TypeId == typeId && c.SubtypeId == subTypeId);
        }

        #endregion
    }
}
