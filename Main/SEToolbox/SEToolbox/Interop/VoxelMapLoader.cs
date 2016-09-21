namespace SEToolbox.Interop
{
    using System;
    using System.Linq;

    public static class VoxelMapLoader
    {
        static VoxelMapLoader()
        {
            //var t = typeof(IMyVoxelMap);
            //var t2 = typeof(Sandbox.Definitions.MyCubeDefinition);
            ////var t2 = typeof(Sandbox.Engine.Voxels.IMyStorage);
            //var t3 = typeof(Sandbox.ModAPI.Interfaces.IMyStorage);

            //var a = t2.Assembly;
            //Type[] ts;
            try
            {

                //ts = a.GetExportedTypes();
                //var ts = t2.Assembly.GetTypes();

                var type = typeof(VRage.ModAPI.IMyStorage);
                var types = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(s => s.GetTypes())
                    .Where(p => type.IsAssignableFrom(p));
                var c = types.Count();
            }
            catch (Exception)
            {
                // The types required to load the current asteroid files are in the Sandbox.Game.dll.
                // Trying to iterate through the types in the Sandbox.Game assembly, will practically cause it to load every other assembly in the game.
                // Unless there is another way to ignore types dependant on other assemblies I can't even progress with this 'idea' to load asteroids.
            }
            
        }

        public static void Load(string filename)
        {

        }
    }
}
