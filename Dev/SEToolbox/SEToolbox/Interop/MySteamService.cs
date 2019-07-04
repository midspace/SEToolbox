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

        public new bool IsActive { get; private set; }

        public new ulong UserId { get; set; }

        public MySteamService(bool isDedicated, uint appId)
            : base(true, appId)
        {
            IsActive = SteamAPI.Init();

            if (IsActive)
            {
                SteamUserId = SteamUser.GetSteamID();
                UserId = (ulong)SteamUserId;
                ReflectionUtil.SetObjectFieldValue(this, "m_remoteStorage", new VRage.Steam.MySteamRemoteStorage());
            }
        }
    }
}
