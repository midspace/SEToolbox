namespace ToolboxTest
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Sandbox.Engine.Voxels;
    using SEToolbox.Support;
    using VRage.FileSystem;
    using VRage.Game;

    [TestClass]
    public class TypeLoaderTests
    {
        List<string> _spaceEngineersAssemblies;

        //[TestMethod]
        public void LoadOthe3r()
        {
            var filename = @"D:\Program Files (x86)\Steam\SteamApps\common\SpaceEngineers\Content\VoxelMaps\Arabian_Border_7.vx2";

            var storage = MyStorageBase.LoadFromFile(filename);

            //byte[] buffer;

            //using (var ms = new FileStream(filename, FileMode.Open))
            //{
            //    using (var reader = new BinaryReader(ms))
            //    {
            //        buffer = reader.ReadBytes((int)reader.BaseStream.Length);
            //    }
            //}

            //var storage = MyStorageBase.Load(buffer);

        }

        [TestMethod]
        public void FindType()
        {
            Type ivms = typeof(VRage.Game.ModAPI.IMyVoxelMaps);

            var binPath = @"D:\Program Files (x86)\Steam\steamapps\common\SpaceEngineers\Bin";

            var assemblyFiles = Directory.GetFiles(binPath /*GlobalSettings.Default.SEBinPath*/, "*.dll");
            _spaceEngineersAssemblies = assemblyFiles.Select(f => Path.GetFileName(f)).ToList();

            Type baseType = typeof(Sandbox.Definitions.MyDefinitionManager);
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += currentDomain_AssemblyResolve;
            currentDomain.ReflectionOnlyAssemblyResolve += currentDomain_ReflectionOnlyAssemblyResolve;

            currentDomain.TypeResolve += currentDomain_TypeResolve;
            try
            {
                MyFileSystem.Reset();
                MyFileSystem.Init(@"C:\Program Files (x86)\Steam\SteamApps\common\SpaceEngineers\Content", Environment.ExpandEnvironmentVariables(@"%AppData%\SpaceEngineers"));
                MyFileSystem.InitUserSpecific((string)null);

                // #########################
                // MySandboxGame (obsfcated) has to be created in memory to be able to load Voxel Material Definitions.
                // Without the Voxel Material Definitions, you cannot use the IMyStorage to load an asteroid.
                // So, this is a pointless waste of effort to try and use the in game code.
                // #########################

                Sandbox.Definitions.MyDefinitionManager.Static.LoadData(new List<MyObjectBuilder_Checkpoint.ModItem>());
                var materials = Sandbox.Definitions.MyDefinitionManager.Static.GetVoxelMaterialDefinitions();

                //DictionaryValuesReader<string, MyVoxelMaterialDefinition>;
                //var dict = materials as Dictionary<string, MyVoxelMaterialDefinition>;
                

                //var matx = Sandbox.Definitions.MyDefinitionManager.Static.m_definitions.m_voxelMaterialsByName;
                var xz = Sandbox.Definitions.MyDefinitionManager.Static.GetDefaultVoxelMaterialDefinition();

                //var tt = ass2.GetType("Sandbox.ModAPI.IMyVoxelMaps");
                //var ass = System.Reflection.Assembly.ReflectionOnlyLoad("Sandbox.Game");
                var ass = baseType.Assembly;
                
                //               ass.GetType();
                //var modules = ass.GetModules(false);
                //var types = modules[0].GetTypes();
                var types = ass.GetTypes();
             
                var myVoxelMapsType = types.Where(p => ivms.IsAssignableFrom(p)).First();
                var myVoxelMaps = Activator.CreateInstance(myVoxelMapsType) as VRage.Game.ModAPI.IMyVoxelMaps;

                //5BCAC68007431E61367F5B2CF24E2D6F.5217D2CFAB7CCD6299A3F53DAEE1DEB1
                //public static 6922E99EC72C10627AA239B8167BF7DC A109856086C45CF523B23AFCDDB82F43(byte[] 06D95B424FC4150954FF019440A547AE)
                var filename = @"C:\Program Files (x86)\Steam\SteamApps\common\SpaceEngineers\Content\VoxelMaps\Arabian_Border_7.vx2";

                byte[] buffer = File.ReadAllBytes(filename);
                var storage = myVoxelMaps.CreateStorage(buffer);
            }
            catch (Exception)
            {
                throw;
            }
        }

        Assembly currentDomain_ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var filename = args.Name.Substring(0, args.Name.IndexOf(",", StringComparison.Ordinal)) + ".dll";

            const string filter = @"^(?<assembly>(?:\w+(?:\.?\w+)+))\s*(?:,\s?Version=(?<version>\d+\.\d+\.\d+\.\d+))?(?:,\s?Culture=(?<culture>[\w-]+))?(?:,\s?PublicKeyToken=(?<token>\w+))?$";
            var match = Regex.Match(args.Name, filter);
            if (match.Success)
            {
                filename = match.Groups["assembly"].Value + ".dll";
            }

            if (_spaceEngineersAssemblies.Any(f => string.Equals(f, filename, StringComparison.OrdinalIgnoreCase)))
            {
                var assemblyPath = Path.Combine(GlobalSettings.Default.SEBinPath, filename);

                // Load the assembly from the specified path.
                // Return the loaded assembly.
                return Assembly.ReflectionOnlyLoadFrom(assemblyPath);
            }

            string missedAssemblyFullName = args.Name;
            Assembly assembly = Assembly.ReflectionOnlyLoad(missedAssemblyFullName);
            return assembly;
        }

        Assembly currentDomain_TypeResolve(object sender, ResolveEventArgs args)
        {
            return null;
        }

        Assembly currentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var filename = args.Name.Substring(0, args.Name.IndexOf(",", StringComparison.Ordinal)) + ".dll";

            const string filter = @"^(?<assembly>(?:\w+(?:\.?\w+)+))\s*(?:,\s?Version=(?<version>\d+\.\d+\.\d+\.\d+))?(?:,\s?Culture=(?<culture>[\w-]+))?(?:,\s?PublicKeyToken=(?<token>\w+))?$";
            var match = Regex.Match(args.Name, filter);
            if (match.Success)
            {
                filename = match.Groups["assembly"].Value + ".dll";
            }

            if (_spaceEngineersAssemblies.Any(f => string.Equals(f, filename, StringComparison.OrdinalIgnoreCase)))
            {
                var assemblyPath = Path.Combine(GlobalSettings.Default.SEBinPath, filename);

                // Load the assembly from the specified path.
                // Return the loaded assembly.
                return Assembly.LoadFrom(assemblyPath);
            }

            return Assembly.LoadFrom(args.Name);
        }
    }
}
