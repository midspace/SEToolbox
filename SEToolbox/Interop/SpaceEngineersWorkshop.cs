using System.Collections.Generic;
using System.Threading;
using ParallelTasks;
using Sandbox;
using Sandbox.Engine.Networking;
using Sandbox.Game;
using VRage;
using VRage.Game;
using VRage.GameServices;
using VRage.Utils;
using static Sandbox.Engine.Networking.MyWorkshop;

namespace SEToolbox.Interop
{
    public class SpaceEngineersWorkshop
    {
        public static IMyGameService MySteam => MyServiceManager.Instance.GetService<IMyGameService>();

        static readonly int m_dependenciesRequestTimeout = 30000;

        public static void GetModItems(List<MyObjectBuilder_Checkpoint.ModItem> mods, CancelToken cancelToken)
        {
            DownloadWorldModsBlocking(mods, cancelToken);
        }

        public static ResultData DownloadWorldModsBlocking(List<MyObjectBuilder_Checkpoint.ModItem> mods, CancelToken cancelToken)
        {
            ResultData ret = default;

            Task task = Parallel.Start(() =>
            {
                ret = DownloadWorldModsBlockingInternal(mods, cancelToken);
            });

            while (!task.IsComplete)
            {
                MyGameService.Update();
                Thread.Sleep(10);
            }

            return ret;
        }

        static ResultData DownloadWorldModsBlockingInternal(List<MyObjectBuilder_Checkpoint.ModItem> mods, CancelToken cancelToken)
        {
            MySandboxGame.Log.WriteLineAndConsole("Downloading world mods - START");
            MySandboxGame.Log.IncreaseIndent();

            ResultData resultData = default;
            resultData.Success = true;

            if (mods != null && mods.Count > 0)
            {
                FixModServiceName(mods);

                var list = new List<WorkshopId>();

                foreach (MyObjectBuilder_Checkpoint.ModItem mod in mods)
                {
                    if (mod.PublishedFileId != 0L)
                    {
                        var item = new WorkshopId(mod.PublishedFileId, mod.PublishedServiceName);

                        if (!list.Contains(item))
                            list.Add(item);
                    }
                    else if (Sandbox.Engine.Platform.Game.IsDedicated)
                    {
                        MySandboxGame.Log.WriteLineAndConsole("Local mods are not allowed in multiplayer.");
                        MySandboxGame.Log.DecreaseIndent();
                        return default;
                    }
                }

                list.Sort();

                bool flag = false;

                if (MyPlatformGameSettings.CONSOLE_COMPATIBLE)
                {
                    foreach (WorkshopId item2 in list)
                    {
                        var aggregate = MyGameService.WorkshopService.GetAggregate(item2.ServiceName);

                        if (aggregate == null)
                        {
                            flag = true;
                            MySandboxGame.Log.WriteLineAndConsole(string.Format("Can't download mod {0}. Service {1} is not available", item2.Id, item2.ServiceName));
                        }
                        else if (!aggregate.IsConsoleCompatible)
                        {
                            flag = true;
                            MySandboxGame.Log.WriteLineAndConsole(string.Format("Can't download mod {0}. Service {1} is not console compatible", item2.Id, aggregate.ServiceName));
                        }
                    }
                }

                if (flag)
                {
                    resultData.Success = false;
                }
                else
                {
                    AddModDependencies(mods, list);
                    resultData = DownloadModsBlocking(mods, resultData, list, cancelToken);
                }
            }

            MySandboxGame.Log.DecreaseIndent();
            MySandboxGame.Log.WriteLineAndConsole("Downloading world mods - END");

            if (cancelToken != null)
                resultData.Cancel |= cancelToken.Cancel;

            return resultData;
        }

        static void AddModDependencies(List<MyObjectBuilder_Checkpoint.ModItem> mods, List<WorkshopId> workshopIds)
        {
            var hashSet = new HashSet<WorkshopId>();
            var hashSet2 = new HashSet<WorkshopId>();

            foreach (var mod in mods)
            {
                var item = new WorkshopId(mod.PublishedFileId, mod.PublishedServiceName);
                hashSet2.Add(item);

                if (!mod.IsDependency && mod.PublishedFileId != 0L)
                    hashSet.Add(item);
            }

            bool hasReferenceIssue;

            foreach (var item3 in GetModsDependencyHiearchy(hashSet, out hasReferenceIssue))
            {
                var item2 = new WorkshopId(item3.Id, item3.ServiceName);

                if (hashSet2.Add(item2))
                {
                    mods.Add(new MyObjectBuilder_Checkpoint.ModItem(item3.Id, item3.ServiceName, true) {
                        FriendlyName = item3.Title
                    });
                }

                if (!workshopIds.Contains(item2))
                    workshopIds.Add(item2);
            }
        }

        static List<MyWorkshopItem> GetModsDependencyHiearchy(HashSet<WorkshopId> workshopIds, out bool hasReferenceIssue)
        {
            hasReferenceIssue = false;

            var list = new List<MyWorkshopItem>();
            var hashSet = new HashSet<WorkshopId>();
            var list2 = new List<WorkshopId>();
            var stack = new Stack<WorkshopId>();

            foreach (var workshopId in workshopIds)
                stack.Push(workshopId);

            while (stack.Count > 0)
            {
                while (stack.Count > 0)
                {
                    var item = stack.Pop();

                    if (!hashSet.Contains(item))
                    {
                        hashSet.Add(item);
                        list2.Add(item);
                    }
                    else
                    {
                        hasReferenceIssue = true;
                        MyLog.Default.WriteLineAndConsole(string.Format("Reference issue detected (circular reference or wrong order) for mod {0}:{1}", item.ServiceName, item.Id));
                    }
                }

                if (list2.Count == 0)
                    continue;

                var modsInfo = GetModsInfo(list2);

                if (modsInfo != null)
                {
                    foreach (MyWorkshopItem item2 in modsInfo)
                    {
                        list.Insert(0, item2);
                        item2.UpdateDependencyBlocking();

                        for (int num = item2.Dependencies.Count - 1; num >= 0; num--)
                        {
                            ulong id = item2.Dependencies[num];
                            stack.Push(new WorkshopId(id, item2.ServiceName));
                        }
                    }
                }

                list2.Clear();
            }

            return list;
        }

        static List<MyWorkshopItem> GetModsInfo(List<WorkshopId> workshopIds)
        {
            var dictionary = ToDictionary(workshopIds);
            List<MyWorkshopItem> list = null;

            foreach (var item in dictionary)
            {
                if (list == null)
                {
                    list = GetModsInfo(item.Key, item.Value);
                    continue;
                }

                var modsInfo = GetModsInfo(item.Key, item.Value);

                if (modsInfo != null)
                    list.AddRange(modsInfo);
            }

            return list;
        }

        static List<MyWorkshopItem> GetModsInfo(string serviceName, List<ulong> workshopIds)
        {
            var myWorkshopQuery = MyGameService.CreateWorkshopQuery(serviceName);

            if (myWorkshopQuery == null)
                return null;

            myWorkshopQuery.ItemIds = workshopIds;

            using (var resetEvent = new AutoResetEvent(false))
            {
                myWorkshopQuery.QueryCompleted += result =>
                {
                    if (result == MyGameServiceCallResult.OK)
                        MySandboxGame.Log.WriteLine("Mod dependencies query successful");
                    else
                        MySandboxGame.Log.WriteLine(string.Format("Error during mod dependencies query: {0}", result));

                    resetEvent.Set();
                };

                myWorkshopQuery.Run();

                if (!resetEvent.WaitOne(m_dependenciesRequestTimeout))
                {
                    myWorkshopQuery.Dispose();
                    return null;
                }
            }

            var items = myWorkshopQuery.Items;
            myWorkshopQuery.Dispose();

            return items;
        }

        static ResultData DownloadModsBlocking(List<MyObjectBuilder_Checkpoint.ModItem> mods, ResultData ret, List<WorkshopId> workshopIds, CancelToken cancelToken)
        {
            var toGet = new List<MyWorkshopItem>(workshopIds.Count);

            if (!GetItemsBlockingUGC(workshopIds, toGet))
            {
                MySandboxGame.Log.WriteLine("Could not obtain workshop item details");
                ret.Success = false;
            }
            //else if (workshopIds.Count != toGet.Count)
            //{
            //    MySandboxGame.Log.WriteLine(string.Format("Could not obtain all workshop item details, expected {0}, got {1}", workshopIds.Count, toGet.Count));
            //    ret.Success = false;
            //}
            else
            {
                ret = DownloadModsBlockingUGC(toGet, cancelToken);

                if (!ret.Success)
                {
                    MySandboxGame.Log.WriteLine("Downloading mods failed");
                }
                else
                {
                    var array = mods.ToArray();

                    for (int i = 0; i < array.Length; i++)
                    {
                        var myWorkshopItem = toGet.Find((MyWorkshopItem x) => x.Id == array[i].PublishedFileId);

                        if (myWorkshopItem != null)
                        {
                            array[i].FriendlyName = myWorkshopItem.Title;
                            array[i].SetModData(myWorkshopItem);
                        }
                        else
                        {
                            array[i].FriendlyName = array[i].Name;
                        }
                    }

                    mods.Clear();
                    mods.AddRange(array);
                }
            }

            return ret;
        }

        static void FixModServiceName(List<MyObjectBuilder_Checkpoint.ModItem> mods)
        {
            for (int i = 0; i < mods.Count; i++)
            {
                var value = mods[i];

                if (string.IsNullOrEmpty(value.PublishedServiceName))
                {
                    value.PublishedServiceName = MyGameService.GetDefaultUGC().ServiceName;
                    mods[i] = value;
                }
            }
        }

        static Dictionary<string, List<ulong>> ToDictionary(IEnumerable<WorkshopId> workshopIds)
        {
            var dictionary = new Dictionary<string, List<ulong>>();

            foreach (WorkshopId workshopId in workshopIds)
            {
                string key = workshopId.ServiceName ?? MyGameService.GetDefaultUGC().ServiceName;

                if (!dictionary.TryGetValue(key, out var value))
                    dictionary.Add(key, value = new List<ulong>());

                value.Add(workshopId.Id);
            }

            return dictionary;
        }
    }
}
