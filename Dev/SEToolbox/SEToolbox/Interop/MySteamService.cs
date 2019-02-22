using System;
using Steamworks;
using VRage.GameServices;
using MySteamServiceBase = VRage.Steam.MySteamService;

namespace SEToolbox.Interop
{
    /// <summary>
    /// Keen's steam service calls RestartIfNecessary, which triggers steam to think the game was launched
    /// outside of Steam, which causes this process to exit, and the game to launch instead with an arguments warning.
    /// We have to override the default behavior, then forcibly set the correct options.
    /// </summary>
    public class MySteamService : MySteamServiceBase
    {
        public MySteamService(bool isDedicated, uint appId)
            : base(true, appId)
        {
            // TODO: Add protection for this mess... somewhere
            GameServer.Shutdown();
            var steam = typeof(MySteamServiceBase);
            steam.GetField("m_gameServer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(this, null);

            steam.GetProperty("AppId").GetSetMethod(true).Invoke(this, new object[] { appId });
            if (isDedicated)
            {
                steam.GetProperty("SteamServerAPI").GetSetMethod(true).Invoke(this, new object[] { null });
                steam.GetField("m_gameServer").SetValue(this, new VRage.Steam.MySteamGameServer());

                var method = typeof(MySteamServiceBase).GetMethod("OnModServerDownloaded", System.Reflection.BindingFlags.NonPublic);
                var del = method.CreateDelegate<Callback<DownloadItemResult_t>.DispatchDelegate>(this);
                steam.GetField("m_modServerDownload").SetValue(this, Callback<DownloadItemResult_t>.CreateGameServer(new Callback<DownloadItemResult_t>.DispatchDelegate(del)));
            }
            else
            {
                steam.GetProperty("IsActive").GetSetMethod(true).Invoke(this, new object[] { SteamAPI.Init() });

                if (IsActive)
                {
                    steam.GetField("SteamUserId", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(this, SteamUser.GetSteamID());
                    steam.GetProperty("UserId").GetSetMethod(true).Invoke(this, new object[] { (ulong)SteamUser.GetSteamID() });
                    steam.GetProperty("UserName").GetSetMethod(true).Invoke(this, new object[] { SteamFriends.GetPersonaName() });
                    steam.GetProperty("OwnsGame").GetSetMethod(true).Invoke(this, new object[] {
                        SteamUser.UserHasLicenseForApp(SteamUser.GetSteamID(), (AppId_t)appId) == EUserHasLicenseForAppResult.k_EUserHasLicenseResultHasLicense
                    });
                    steam.GetProperty("UserUniverse").GetSetMethod(true).Invoke(this, new object[] { (MyGameServiceUniverse)SteamUtils.GetConnectedUniverse() });

                    string pchName;
                    steam.GetProperty("BranchName").GetSetMethod(true).Invoke(this, new object[] { SteamApps.GetCurrentBetaName(out pchName, 512) ? pchName : "default" });

                    SteamUserStats.RequestCurrentStats();

                    steam.GetProperty("InventoryAPI").GetSetMethod(true).Invoke(this, new object[] { new VRage.Steam.MySteamInventory() });

                    steam.GetMethod("RegisterCallbacks",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        .Invoke(this, null);

//#if SE
                    steam.GetField("m_remoteStorage", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(this, new VRage.Steam.MySteamRemoteStorage());
//#endif
                }
            }

            steam.GetProperty("Peer2Peer").GetSetMethod(true).Invoke(this, new object[] { new VRage.Steam.MySteamPeer2Peer() });
        }
    }
}
