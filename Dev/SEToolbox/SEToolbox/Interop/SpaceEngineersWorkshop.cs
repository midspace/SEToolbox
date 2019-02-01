using Sandbox;
using Sandbox.Engine.Networking;
using Sandbox.Game.Multiplayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage;
using VRage.Game;
using VRage.GameServices;

namespace SEToolbox.Interop
{
    public class SpaceEngineersWorkshop: MyWorkshop
    {
        static MySteamService MySteam { get => (MySteamService)MyServiceManager.Instance.GetService<VRage.GameServices.IMyGameService>(); }

        public new static ResultData DownloadWorldModsBlocking(List<MyObjectBuilder_Checkpoint.ModItem> mods, CancelToken cancelToken)
        {
            ResultData ret = new ResultData();
            ret.Success = true;
            //if (!MyFakes.ENABLE_WORKSHOP_MODS)
            //{
            //    if (cancelToken != null)
            //        ret.Cancel = cancelToken.Cancel;
            //    return ret;
            //}

            //MySandboxGame.Log.WriteLineAndConsole("Downloading world mods - START");
            //MySandboxGame.Log.IncreaseIndent();

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
                    else if (MySandboxGame.IsDedicated)
                    {
                        MySandboxGame.Log.WriteLineAndConsole("Local mods are not allowed in multiplayer.");
                        MySandboxGame.Log.DecreaseIndent();
                        return new ResultData();
                    }
                }

                publishedFileIds.Sort();

                if (MySandboxGame.IsDedicated)
                {
                    if (MySandboxGame.ConfigDedicated.AutodetectDependencies)
                    {
                        CheckModDependencies(mods, publishedFileIds);
                    }

                    //MyGameService.SetServerModTemporaryDirectory();

                    ret = DownloadModsBlocking(mods, ret, publishedFileIds, cancelToken);
                }
                else // client
                {
                    if (Sync.IsServer)
                    {
                        CheckModDependencies(mods, publishedFileIds);
                    }

                    ret = DownloadModsBlocking(mods, ret, publishedFileIds, cancelToken);
                }
            }

            //MySandboxGame.Log.DecreaseIndent();
            //MySandboxGame.Log.WriteLineAndConsole("Downloading world mods - END");
            if (cancelToken != null)
                ret.Cancel |= cancelToken.Cancel;
            return ret;
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
            var dependencies = GetModsDependencyHiearchy(rootMods, out hasReferenceIssue);
            foreach (var depMod in dependencies)
            {
                bool isDependency = !rootMods.Contains(depMod.Id);

                var newMod = new MyObjectBuilder_Checkpoint.ModItem(depMod.Id, isDependency);
                newMod.FriendlyName = depMod.Title;
                resultModList.Add(newMod);
                if (!publishedFileIds.Contains(depMod.Id))
                {
                    publishedFileIds.Add(depMod.Id);
                }
            }

            mods.Clear();
            mods.AddRange(resultModList);
        }

        private static ResultData DownloadModsBlocking(List<MyObjectBuilder_Checkpoint.ModItem> mods, ResultData ret, List<ulong> publishedFileIds, CancelToken cancelToken)
        {
            var toGet = new List<MyWorkshopItem>(publishedFileIds.Count);


            // TODO: we just want to fill out toGet and call SetModData(mod) down below.
            if (!GetItemsBlockingUGC(publishedFileIds, toGet))
            {
                MySandboxGame.Log.WriteLine("Could not obtain workshop item details");
                ret.Success = false;
            }
            else if (publishedFileIds.Count != toGet.Count)
            {
                MySandboxGame.Log.WriteLine(string.Format("Could not obtain all workshop item details, expected {0}, got {1}", publishedFileIds.Count, toGet.Count));
                ret.Success = false;
            }
            else
            {
                //if (m_downloadScreen != null)
                //{
                //    m_downloadScreen.MessageText = new StringBuilder(MyTexts.GetString(MyCommonTexts.ProgressTextDownloadingMods) + " 0 of " + toGet.Count.ToString());
                //}

                //ret = DownloadModsBlockingUGC(toGet, cancelToken);
                //if (ret.Success == false)
                //{
                //    MySandboxGame.Log.WriteLine("Downloading mods failed");
                //}
                //else
                //{
                    var array = mods.ToArray();

                    for (int i = 0; i < array.Length; ++i)
                    {
                        var mod = toGet.Find(x => x.Id == array[i].PublishedFileId);
                        if (mod != null)
                        {
                            array[i].FriendlyName = mod.Title;
                            array[i].SetModData(mod);
                        }
                        else
                        {
                            array[i].FriendlyName = array[i].Name;
                        }
                    }
                    mods.Clear();
                    mods.AddArray(array);
                //}
            }
            return ret;
        }


    }
}
