namespace SEToolbox.Interop
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Common.ObjectBuilders.Definitions;
    using VRage.Game;
    using VRage.ObjectBuilders;

    /// <summary>
    /// Some hopefully generic items.
    /// </summary>
    public class SpaceEngineersTypes
    {
        public static readonly MyObjectBuilderType AmmoMagazine;
        public static readonly MyObjectBuilderType PhysicalGunObject;
        public static readonly MyObjectBuilderType OxygenContainerObject;
        public static readonly MyObjectBuilderType Ore;
        public static readonly MyObjectBuilderType Ingot;
        public static readonly MyObjectBuilderType Component;
        public static readonly MyObjectBuilderType VoxelMaterialDefinition;
        public static readonly MyObjectBuilderType MedicalRoom;
        public static readonly MyObjectBuilderType Cockpit;
        public static readonly MyObjectBuilderType Thrust;

        /// <summary>
        /// The base path of the save files, minus the userid.
        /// </summary>
        public static readonly UserDataPath BaseLocalPath;

        public static readonly UserDataPath BaseDedicatedServerHostPath;

        public static readonly UserDataPath BaseDedicatedServerServicePath;

        static SpaceEngineersTypes()
        {
            AmmoMagazine = new MyObjectBuilderType(typeof(MyObjectBuilder_AmmoMagazine));
            PhysicalGunObject = new MyObjectBuilderType(typeof(MyObjectBuilder_PhysicalGunObject));
            OxygenContainerObject = new MyObjectBuilderType(typeof(MyObjectBuilder_OxygenContainerObject));
            Ore = new MyObjectBuilderType(typeof(MyObjectBuilder_Ore));
            Ingot = new MyObjectBuilderType(typeof(MyObjectBuilder_Ingot));
            Component = new MyObjectBuilderType(typeof(MyObjectBuilder_Component));
            VoxelMaterialDefinition = new MyObjectBuilderType(typeof(MyObjectBuilder_VoxelMaterialDefinition));
            MedicalRoom = new MyObjectBuilderType(typeof(MyObjectBuilder_MedicalRoom));
            Cockpit = new MyObjectBuilderType(typeof(MyObjectBuilder_Cockpit));
            Thrust = new MyObjectBuilderType(typeof(MyObjectBuilder_Thrust));
        }
    }
}
