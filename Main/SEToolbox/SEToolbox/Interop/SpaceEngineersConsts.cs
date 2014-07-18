namespace SEToolbox.Interop
{
    using Sandbox.Common.ObjectBuilders;

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

        public static readonly MyObjectBuilderType AmmoMagazine;
        public static readonly MyObjectBuilderType PhysicalGunObject;
        public static readonly MyObjectBuilderType Ore;
        public static readonly MyObjectBuilderType Ingot;
        public static readonly MyObjectBuilderType Component;
        public static readonly MyObjectBuilderType MedicalRoom;
        public static readonly MyObjectBuilderType Cockpit;

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
        }

    }
}
