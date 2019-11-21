using Sandbox;
using Sandbox.Engine.Networking;
using System.Collections.Generic;
using VRage;
using VRage.Game;
using VRage.GameServices;

namespace SEToolbox.Interop
{
    public class SpaceEngineersWorkshop : MyWorkshop
    {
        public static IMyGameService MySteam => MyServiceManager.Instance.GetService<IMyGameService>();

        public static void GetModItems(List<MyObjectBuilder_Checkpoint.ModItem> mods)
        {
            ResultData ret = new ResultData();
            ret.Success = true;

            if (mods != null && mods.Count > 0)
            {
                var publishedFileIds = new List<ulong>();
                foreach (var mod in mods)
                {
                    if (mod.PublishedFileId != 0)
                    {
                        if (!publishedFileIds.Contains(mod.PublishedFileId))
                        {
                            publishedFileIds.Add(mod.PublishedFileId);
                        }
                    }
                }

                publishedFileIds.Sort();
                CheckModDependencies(mods, publishedFileIds);
            }
        }

        private static void CheckModDependencies(List<MyObjectBuilder_Checkpoint.ModItem> mods, List<ulong> publishedFileIds)
        {
            List<MyObjectBuilder_Checkpoint.ModItem> resultModList = new List<MyObjectBuilder_Checkpoint.ModItem>();
            HashSet<ulong> rootMods = new HashSet<ulong>();
            foreach (var mod in mods)
            {
                if (mod.IsDependency)
                {
                    continue;
                }

                if (mod.PublishedFileId == 0)
                {
                    resultModList.Add(mod);
                }
                else
                {
                    rootMods.Add(mod.PublishedFileId);
                }
            }

            bool hasReferenceIssue;
            if (MySteam.IsActive)
            {
                MySandboxGame.IsDedicated = true;
                var dependencies = GetModsDependencyHiearchy(rootMods, out hasReferenceIssue);
                MySandboxGame.IsDedicated = false;
                foreach (var depMod in dependencies)
                {
                    bool isDependency = !rootMods.Contains(depMod.Id);

                    var newMod = new MyObjectBuilder_Checkpoint.ModItem(depMod.Id, isDependency);
                    newMod.FriendlyName = depMod.Title;
                    depMod.UpdateState();

                    // Exclude mods that have not been downloaded.
                    if (depMod.Folder != null)
                    {
                        newMod.SetModData(depMod);
                    }

                    resultModList.Add(newMod);
                    if (!publishedFileIds.Contains(depMod.Id))
                    {
                        publishedFileIds.Add(depMod.Id);
                    }
                }
            }

            mods.Clear();
            mods.AddRange(resultModList);
        }
    }
}
