using SEToolbox.Support;
using Steamworks;
using MySteamServiceBase = VRage.Steam.MySteamService;

namespace SEToolbox.Interop
{
    /// <summary>
    /// Wrapper Keen's MySteamService, so we have to override the default behavior.
    /// </summary>
    public class MySteamService : MySteamServiceBase
    {
        internal CSteamID SteamUserId;

        public MySteamService(bool isDedicated, uint appId)
            : base(true, appId)
        {
            bool isActive = SteamAPI.Init();
            ReflectionUtil.SetObjectFieldValue(this, "IsActive", isActive);

            if (IsActive)
            {
                SteamUserId = SteamUser.GetSteamID();
                UserId = (ulong)SteamUserId;
                ReflectionUtil.SetObjectFieldValue(this, "m_remoteStorage", new VRage.Steam.MySteamRemoteStorage());
            }
        }
    }
}
