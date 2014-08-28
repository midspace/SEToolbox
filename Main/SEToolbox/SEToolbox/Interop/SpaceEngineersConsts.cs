namespace SEToolbox.Interop
{
    using Sandbox.Common.ObjectBuilders;
    using System;

    public class SpaceEngineersConsts
    {
        /// <summary>
        /// Thumbnail image of last position in save.
        /// </summary>
        public const string ThumbnailImageFilename = "thumb.jpg";

        /// <summary>
        /// Contains summary of save content.
        /// </summary>
        public const string SandBoxCheckpointFilename = "Sandbox.sbc";

        /// <summary>
        /// Contains main content.
        /// </summary>
        public const string SandBoxSectorFilename = "SANDBOX_0_0_0_.sbs";

        /// <summary>
        /// Contains list of save 'worlds'.
        /// </summary>
        public const string LoadLoadedFilename = "LastLoaded.sbl";

        // Current set max speed m/s for Ships.
        public const float MaxShipVelocity = 104.375f;

        // Current set max speed m/s for Players - as of update 01.023.
        public const float MaxPlayerVelocity = 111.531f;

        // Estimated max speed m/s for Meteors - as of update 01.024.
        public const float MaxMeteorVelocity = 202.812f;

        public const float PlayerMass = 100f;

        public static readonly MyObjectBuilderType AmmoMagazine;
        public static readonly MyObjectBuilderType PhysicalGunObject;
        public static readonly MyObjectBuilderType Ore;
        public static readonly MyObjectBuilderType Ingot;
        public static readonly MyObjectBuilderType Component;
        public static readonly MyObjectBuilderType MedicalRoom;
        public static readonly MyObjectBuilderType Cockpit;
        public static readonly MyObjectBuilderType Thrust;

        /// <summary>
        /// The base path of the save files, minus the userid.
        /// </summary>
        public static readonly UserDataPath BaseLocalPath;

        public static readonly UserDataPath BaseDedicatedServerHostPath;

        public static readonly UserDataPath BaseDedicatedServerServicePath;

        static SpaceEngineersConsts()
        {
            // Some hopefully generic items.
            AmmoMagazine = new MyObjectBuilderType(typeof(MyObjectBuilder_AmmoMagazine));
            PhysicalGunObject = new MyObjectBuilderType(typeof(MyObjectBuilder_PhysicalGunObject));
            Ore = new MyObjectBuilderType(typeof(MyObjectBuilder_Ore));
            Ingot = new MyObjectBuilderType(typeof(MyObjectBuilder_Ingot));
            Component = new MyObjectBuilderType(typeof(MyObjectBuilder_Component));
            MedicalRoom = new MyObjectBuilderType(typeof(MyObjectBuilder_MedicalRoom));
            Cockpit = new MyObjectBuilderType(typeof(MyObjectBuilder_Cockpit));
            Thrust = new MyObjectBuilderType(typeof(MyObjectBuilder_Thrust));

            BaseLocalPath = new UserDataPath(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"SpaceEngineers\Saves", @"SpaceEngineers\Mods"); // Followed by .\%SteamuserId%\LastLoaded.sbl
            BaseDedicatedServerHostPath = new UserDataPath(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"SpaceEngineersDedicated\Saves", @"SpaceEngineersDedicated\Mods"); // Followed by .\LastLoaded.sbl
            BaseDedicatedServerServicePath = new UserDataPath(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"SpaceEngineersDedicated", ""); // Followed by .\%instancename%\Saves\LastLoaded.sbl  (.\%instancename%\Mods)
        }

        public static Version GetSEVersion()
        {
            try
            {
                return new Version(Sandbox.Common.MyFinalBuildConstants.APP_VERSION_STRING.ToString().Replace("_", "."));
            }
            catch
            {
                return new Version();
            }
        }
    }
}
