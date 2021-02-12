using System.Collections.Generic;
using Sandbox.Engine.Networking;
using VRage;
using VRage.Game;
using VRage.GameServices;

namespace SEToolbox.Interop
{
    public class SpaceEngineersWorkshop
    {
        public static IMyGameService MySteam => MyServiceManager.Instance.GetService<IMyGameService>();

        public static void GetModItems(List<MyObjectBuilder_Checkpoint.ModItem> mods, MyWorkshop.CancelToken cancelToken)
        {
            MyWorkshop.DownloadWorldModsBlocking(mods, cancelToken);
        }
    }
}
