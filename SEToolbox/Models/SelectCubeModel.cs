namespace SEToolbox.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using SEToolbox.Interop;
    using SEToolbox.Support;
    using VRage.Game;
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
                    OnPropertyChanged(nameof(CubeList));
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
                    OnPropertyChanged(nameof(CubeItem));
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
            var cubeDefinitions = SpaceEngineersCore.Resources.CubeBlockDefinitions.Where(c => c.CubeSize == cubeSize);

            foreach (var cubeDefinition in cubeDefinitions)
            {
                string textureFile = null;

                if (cubeDefinition.Icons != null)
                {
                    string icon = cubeDefinition.Icons.FirstOrDefault();

                    if (icon != null)
                        textureFile = SpaceEngineersCore.GetDataPathOrDefault(icon, Path.Combine(contentPath, icon));
                }

                var buildTime = TimeSpan.Zero;

                if (cubeDefinition.IntegrityPointsPerSec != 0)
                {
                    double buildTimeSeconds = (double)cubeDefinition.MaxIntegrity / cubeDefinition.IntegrityPointsPerSec;

                    if (buildTimeSeconds <= TimeSpan.MaxValue.TotalSeconds)
                        buildTime = TimeSpan.FromSeconds(buildTimeSeconds);
                }

                var c = new ComponentItemModel {
                    Name = cubeDefinition.DisplayNameText,
                    TypeId = cubeDefinition.Id.TypeId,
                    TypeIdString = cubeDefinition.Id.TypeId.ToString(),
                    SubtypeId = cubeDefinition.Id.SubtypeName,
                    TextureFile = textureFile,
                    Time = buildTime,
                    Accessible = cubeDefinition.Public,
                    Mass = SpaceEngineersApi.FetchCubeBlockMass(cubeDefinition.Id.TypeId, cubeDefinition.CubeSize, cubeDefinition.Id.SubtypeName),
                    CubeSize = cubeDefinition.CubeSize,
                    Size = new BindableSize3DIModel(cubeDefinition.Size),
                };

                list.Add(c.FriendlyName + c.TypeIdString + c.SubtypeId, c);
            }

            ComponentItemModel cubeItem = null;

            foreach (var kvp in list)
            {
                var cube = kvp.Value;

                CubeList.Add(cube);

                if (cubeItem == null && cube.TypeId == typeId && cube.SubtypeId == subTypeId)
                    cubeItem = cube;
            }

            CubeItem = cubeItem;
        }

        #endregion
    }
}
