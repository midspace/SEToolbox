//===================================================================================
// Voxel Methods/Classes for Space Engineers.
// Based on the Miner-Wars-2081 Mod Kit.
// Copyright (c) Keen Software House a. s.
// This code is expressly licenced, and should not be used in any application without 
// the permission of Keen Software House.
// See http://www.keenswh.com/about.html
// All rights reserved.
//===================================================================================


namespace SEToolbox.Interop.Asteroids
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    //using System.Runtime.InteropServices;
    //using System.Linq;
    //using Sandbox.Definitions;
    using Sandbox.Engine.Voxels;
    using Sandbox.Game.Entities;
    //using Sandbox.Engine.Voxels.Planet;
    //using Sandbox.Game.GameSystems;
    using SEToolbox.Support;
    using SEToolbox.Interop;
    using VRage.FileSystem;
    //using VRageRender.Voxels;
    using VRageMath;
    using Res = SEToolbox.Properties.Resources;
    using VRage;
    //using VRage.Game;
    using VRage.Voxels;
    using VRage.ObjectBuilders;
    using Sandbox.Game.World;

    //using VRage.Utils;
    //using SharpDX.Toolkit.Graphics;
    //using SharpDX.DXGI;

    public class MyVoxelMap : Sandbox.Game.Entities.MyVoxelBase, IDisposable
    {
        public const string V1FileExtension = ".vox";
        public const string V2FileExtension = ".vx2";
        internal const string TagCell = "Cell";

        #region fields

        // Count of voxel data cells in all directions.
        [Obsolete]
        private Vector3I _dataCellsCount;

        // Array of voxel cells in this voxel map.
        [Obsolete]
        private MyVoxelContentCell[][][] _voxelContentCells;

        // Here we store material for each voxel; And average material for data cell too (that is used for LOD).
        [Obsolete]
        private MyVoxelMaterialCell[][][] _voxelMaterialCells;

        //[Obsolete]
        //private Vector3I _cellSize;

        [Obsolete]
        private Vector3I _sizeMinusOne;

        //[Obsolete]
        //private Vector3D _positionLeftBottomCorner;

        private BoundingBoxD _boundingContent;

        private MyStorageBase _storage;

        private Dictionary<byte, long> _assetCount;

        #endregion

        #region properties

        //[Obsolete]
        //public int FileVersion { get; private set; }

        public Vector3I Size { get; private set; }

        // TODO: WeightedCenter.  Center of mass, as opposed to center by dimension.
        // public Vector3 WeightedCenter { get; private set; }

        public BoundingBoxD BoundingContent { get { return _boundingContent; } }

        //public byte VoxelMaterial { get; private set; }

        public bool IsValid { get; private set; }

        public long VoxCells { get; private set; }

        //public Dictionary<byte, long> AssetCount { get { return _assetCount; } }

        #endregion

        #region Init

        public override void Init(MyObjectBuilder_EntityBase builder, IMyStorage storage)
        {
            _storage = (MyStorageBase)storage;
        }

        public override Sandbox.Game.Entities.MyVoxelBase RootVoxel => this;

        public override IMyStorage Storage
        {
            get { return _storage; }
            set { _storage = (MyStorageBase)value; }
        }

        public void Create(Vector3I size, string materialName)
        {
            if (_storage != null)
                _storage.Close();

            _storage = new MyOctreeStorage(null, size);
            _storage.Geometry.Init(_storage);
            byte materialIndex = SpaceEngineersCore.Resources.GetMaterialIndex(materialName);
            OverwriteAllMaterials(materialIndex);

            IsValid = true;
            Size = _storage.Size;
            _boundingContent = new BoundingBoxD();
            VoxCells = 0;
        }

        private void OverwriteAllMaterials(byte materialIndex)
        {
            // does not work
            //MyVoxelMaterialDefinition voxelMaterial = MyDefinitionManager.Static.GetVoxelMaterialDefinition(materialName);
            //Storage.OverwriteAllMaterials(voxelMaterial);

            Vector3I block;

            // read the asteroid in chunks of 64 to avoid the Arithmetic overflow issue.
            for (block.Z = 0; block.Z < _storage.Size.Z; block.Z += 64)
                for (block.Y = 0; block.Y < _storage.Size.Y; block.Y += 64)
                    for (block.X = 0; block.X < _storage.Size.X; block.X += 64)
                    {
                        var cacheSize = new Vector3I(64);
                        var cache = new MyStorageData();
                        cache.Resize(cacheSize);
                        // LOD1 is not detailed enough for content information on asteroids.
                        Vector3I maxRange = block + cacheSize - 1;
                        _storage.ReadRange(cache, MyStorageDataTypeFlags.Material, 0, ref block, ref maxRange);
                        cache.BlockFillMaterial(Vector3I.Zero, cacheSize - 1, materialIndex);
                        _storage.WriteRange(cache, MyStorageDataTypeFlags.Material, ref block, ref maxRange);
                    }
        }

        // Creates full voxel map, does not initialize base !!! Use only for importing voxel maps from models !!!
        //[Obsolete]
        //public void Init(Vector3D position, Vector3I size, string material)
        //{
        //    InitVoxelMap(position, size, material, true);
        //}

        //[Obsolete]
        //private void InitVoxelMap(Vector3D position, Vector3I size, string materialName, bool defineMemory)
        //{
        //    IsValid = true;
        //    Size = size;
        //    _sizeMinusOne = new Vector3I(Size.X - 1, Size.Y - 1, Size.Z - 1);
        //    VoxelMaterial = SpaceEngineersCore.Resources.GetMaterialIndex(materialName);
        //    _positionLeftBottomCorner = position;
        //    _boundingContent = new BoundingBoxD(new Vector3I(Size.X, Size.Y, Size.Z), new Vector3I(0, 0, 0));

        //    // this is too big for the current SEToolbox code to cope with, and is probably a planet.
        //    if (!defineMemory)
        //        return;

        //    // If you need larged voxel maps, enlarge this constant.
        //    Debug.Assert(Size.X <= MyVoxelConstants.MAX_VOXEL_MAP_SIZE_IN_VOXELS);
        //    Debug.Assert(Size.Y <= MyVoxelConstants.MAX_VOXEL_MAP_SIZE_IN_VOXELS);
        //    Debug.Assert(Size.Z <= MyVoxelConstants.MAX_VOXEL_MAP_SIZE_IN_VOXELS);

        //    // Voxel map size must be multiple of a voxel data cell size.
        //    Debug.Assert((Size.X & MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_MASK) == 0);
        //    Debug.Assert((Size.Y & MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_MASK) == 0);
        //    Debug.Assert((Size.Z & MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_MASK) == 0);
        //    _dataCellsCount.X = Size.X >> MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS;
        //    _dataCellsCount.Y = Size.Y >> MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS;
        //    _dataCellsCount.Z = Size.Z >> MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS;

        //    // Voxel map size must be multiple of a voxel data cell size.
        //    Debug.Assert((Size.X % MyVoxelConstants.VOXEL_RENDER_CELL_SIZE_IN_VOXELS) == 0);
        //    Debug.Assert((Size.Y % MyVoxelConstants.VOXEL_RENDER_CELL_SIZE_IN_VOXELS) == 0);
        //    Debug.Assert((Size.Z % MyVoxelConstants.VOXEL_RENDER_CELL_SIZE_IN_VOXELS) == 0);

        //    // Array of voxel cells in this voxel map.
        //    _voxelContentCells = new MyVoxelContentCell[_dataCellsCount.X][][];
        //    for (var x = 0; x < _voxelContentCells.Length; x++)
        //    {
        //        _voxelContentCells[x] = new MyVoxelContentCell[_dataCellsCount.Y][];
        //        for (var y = 0; y < _voxelContentCells[x].Length; y++)
        //        {
        //            _voxelContentCells[x][y] = new MyVoxelContentCell[_dataCellsCount.Z];
        //        }
        //    }

        //    //  Set base material.
        //    _voxelMaterialCells = new MyVoxelMaterialCell[_dataCellsCount.X][][];
        //    for (var x = 0; x < _dataCellsCount.X; x++)
        //    {
        //        _voxelMaterialCells[x] = new MyVoxelMaterialCell[_dataCellsCount.Y][];
        //        for (var y = 0; y < _dataCellsCount.Y; y++)
        //        {
        //            _voxelMaterialCells[x][y] = new MyVoxelMaterialCell[_dataCellsCount.Z];
        //            for (var z = 0; z < _dataCellsCount.Z; z++)
        //            {
        //                _voxelMaterialCells[x][y][z] = new MyVoxelMaterialCell(VoxelMaterial, 0xFF);
        //            }
        //        }
        //    }
        //}

        #endregion

        public static Dictionary<string, long> GetMaterialAssetDetails(string filename)
        {
            var list = new Dictionary<string, long>();
            var map = new MyVoxelMap();
            map.Load(filename);

            if (!map.IsValid)
                return list;

            //IList<byte> materialAssetList;
            //Dictionary<byte, long> materialVoxelCells;

            list = map.CountAssets();
            map.Dispose();

            return list;

            //materialAssetList = map.CalcVoxelMaterialList();
            //map.CalculateMaterialCellAssets(out materialAssetList, out materialVoxelCells);
            //return map.CountAssets(materialVoxelCells);
        }

        #region IsVoxelMapFile

        /// <summary>
        /// check for Magic Number: 1f 8b
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static bool IsVoxelMapFile(string filename)
        {
            var extension = Path.GetExtension(filename);
            if (extension != null && extension.Equals(V1FileExtension, StringComparison.InvariantCultureIgnoreCase))
            {
                using (var stream = File.OpenRead(filename))
                {
                    try
                    {
                        var msgLength1 = stream.ReadByte();
                        var msgLength2 = stream.ReadByte();
                        var msgLength3 = stream.ReadByte();
                        var msgLength4 = stream.ReadByte();
                        var b1 = stream.ReadByte();
                        var b2 = stream.ReadByte();
                        return (b1 == 0x1f && b2 == 0x8b);
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
            if (extension != null && extension.Equals(V2FileExtension, StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    return ZipTools.IsGzipedFile(filename);
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        #endregion

        #region Load

        public void Load(string filename)
        {
            try
            {
                _storage = MyStorageBase.LoadFromFile(filename);
                IsValid = true;
                //CalcVoxelCells();
                Size = _storage.Size;
            }
            catch (Exception ex)
            {
                Size = Vector3I.Zero;
                _boundingContent = new BoundingBoxD();
                VoxCells = 0;
                IsValid = false;
                DiagnosticsLogging.LogWarning(string.Format(Res.ExceptionState_CorruptAsteroidFile, filename), ex);
            }
        }

        //[Obsolete]
        //public void Load(string filename, string defaultMaterial)
        //{
        //    if (string.IsNullOrEmpty(defaultMaterial))
        //        defaultMaterial = SpaceEngineersCore.Resources.GetDefaultMaterialName();

        //    Load(filename, defaultMaterial, true);
        //}

        /// implemented from Sandbox.Engine.Voxels.MyStorageBase.UpdateFileFormat(string originalVoxFile), but we need control of the destination file.
        public static void UpdateFileFormat(string originalVoxFile, string newVoxFile)
        {
            using (MyCompressionFileLoad myCompressionFileLoad = new MyCompressionFileLoad(originalVoxFile))
            {
                using (Stream stream = MyFileSystem.OpenWrite(newVoxFile, FileMode.Create))
                {
                    using (GZipStream gZipStream = new GZipStream(stream, CompressionMode.Compress))
                    {
                        using (BufferedStream bufferedStream = new BufferedStream(gZipStream))
                        {
                            bufferedStream.WriteNoAlloc(TagCell, null);
                            bufferedStream.Write7BitEncodedInt(myCompressionFileLoad.GetInt32());
                            byte[] array = new byte[16384];
                            for (int bytes = myCompressionFileLoad.GetBytes(array.Length, array); bytes != 0; bytes = myCompressionFileLoad.GetBytes(array.Length, array))
                            {
                                bufferedStream.Write(array, 0, bytes);
                            }
                        }
                    }
                }
            }
        }

        //[Obsolete]
        //public void Load(string filename, string defaultMaterial, bool loadMaterial)
        //{
        //    _storage = MyStorageBase.LoadFromFile(filename);

        //    Size = _storage.Size;
        //    IsValid = true;

        //    var tempfilename = TempfileUtil.NewFilename();
        //    var initialVersion = Path.GetExtension(filename).Equals(V2FileExtension, StringComparison.InvariantCultureIgnoreCase) ? 1 : 0;

        //    if (initialVersion == 1)
        //        ZipTools.GZipUncompress(filename, tempfilename);
        //    else
        //        Uncompress(filename, tempfilename);

        //    using (var ms = new FileStream(tempfilename, FileMode.Open))
        //    {
        //        var reader = new BinaryReader(ms);
        //        LoadUncompressed(initialVersion, Vector3D.Zero, reader, defaultMaterial, loadMaterial);
        //    }

        //    File.Delete(tempfilename);
        //}

        //[Obsolete]
        //public void LoadUncompressed(int initialVersion, Vector3D position, BinaryReader reader, string defaultMaterial, bool loadMaterial)
        //{
        //    string voxelType = "Cell";
        //    int materialBaseCount = 0;
        //    switch (initialVersion)
        //    {
        //        case 1:
        //            // cell tag header
        //            voxelType = reader.ReadString();
        //            FileVersion = reader.Read7BitEncodedInt();
        //            break;
        //        default:
        //            FileVersion = reader.ReadInt32();
        //            break;
        //    }

        //    if (voxelType == "Cell")
        //    {
        //        var sizeX = reader.ReadInt32();
        //        var sizeY = reader.ReadInt32();
        //        var sizeZ = reader.ReadInt32();

        //        var cellSizeX = reader.ReadInt32();
        //        var cellSizeY = reader.ReadInt32();
        //        var cellSizeZ = reader.ReadInt32();

        //        _cellSize = new Vector3I(cellSizeX, cellSizeY, cellSizeZ);

        //        InitVoxelMap(position, new Vector3I(sizeX, sizeY, sizeZ), defaultMaterial, true);
        //        if (FileVersion == 2)
        //            materialBaseCount = reader.ReadByte();
        //    }
        //    else
        //    {
        //        //voxelType == "Octree"
        //        // Don't know how to read this format.
        //        var a1 = reader.ReadByte(); // no idea 
        //        var a2 = reader.ReadByte();
        //        var a3 = reader.ReadByte();
        //        var a4 = reader.ReadByte();
        //        var a5 = reader.ReadByte();
        //        var a6 = reader.ReadByte();
        //        var a7 = reader.ReadByte();

        //        var sizeX = reader.ReadInt32();
        //        var sizeY = reader.ReadInt32();
        //        var sizeZ = reader.ReadInt32();

        //        var c1 = reader.ReadByte(); // no idea 
        //        var c2 = reader.ReadByte();
        //        var c3 = reader.ReadByte();
        //        var c4 = reader.ReadByte();
        //        var c5 = reader.ReadByte();

        //        _cellSize = new Vector3I(8, 8, 8);

        //        InitVoxelMap(position, new Vector3I(sizeX, sizeY, sizeZ), defaultMaterial, false);
        //        materialBaseCount = reader.ReadInt32();

        //        //FileVersion = 2;
        //        IsValid = false;
        //        return;
        //    }

        //    switch (FileVersion)
        //    {
        //        case 0:
        //        case 1: LoadUncompressedV1(reader, loadMaterial); break;
        //        case 2: LoadUncompressedV2(reader, loadMaterial, materialBaseCount); break;
        //        default: throw new Exception("Voxel format not implmented");
        //    }
        //}

        //[Obsolete]
        //private void LoadUncompressedV1(BinaryReader reader, bool loadMaterial)
        //{
        //    var cellsCount = Size / _cellSize;

        //    for (var x = 0; x < cellsCount.X; x++)
        //    {
        //        for (var y = 0; y < cellsCount.Y; y++)
        //        {
        //            for (var z = 0; z < cellsCount.Z; z++)
        //            {
        //                var cellType = (MyVoxelCellType)reader.ReadByte();

        //                //  Cell's are FULL by default, therefore we don't need to change them
        //                if (cellType != MyVoxelCellType.FULL)
        //                {
        //                    var cellCoord = new Vector3I(x, y, z);

        //                    var newCell = new MyVoxelContentCell();
        //                    _voxelContentCells[cellCoord.X][cellCoord.Y][cellCoord.Z] = newCell;

        //                    if (cellType == MyVoxelCellType.EMPTY)
        //                    {
        //                        newCell.SetToEmpty();
        //                    }
        //                    else if (cellType == MyVoxelCellType.MIXED)
        //                    {
        //                        BoundingBoxD box;
        //                        newCell.SetAllVoxelContents(reader.ReadBytes(_cellSize.X * _cellSize.Y * _cellSize.Z), out box);
        //                        _boundingContent.Min = Vector3D.Min(_boundingContent.Min, new Vector3D((x << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS) + box.Min.X, (y << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS) + box.Min.Y, (z << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS) + box.Min.Z));
        //                        _boundingContent.Max = Vector3D.Max(_boundingContent.Max, new Vector3D((x << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS) + box.Max.X, (y << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS) + box.Max.Y, (z << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS) + box.Max.Z));
        //                    }
        //                    // ignore else condition
        //                }
        //                else
        //                {
        //                    _boundingContent.Min = Vector3D.Min(_boundingContent.Min, new Vector3D(x << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS, y << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS, z << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS));
        //                    _boundingContent.Max = Vector3D.Max(_boundingContent.Max, new Vector3D((x + 1 << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS) - 1, (y + 1 << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS) - 1, (z + 1 << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS) - 1));
        //                }
        //            }
        //        }
        //    }

        //    if (reader.PeekChar() == -1 || !loadMaterial)
        //    {
        //        return;
        //    }

        //    // Read materials and indestructible.
        //    for (var x = 0; x < cellsCount.X; x++)
        //    {
        //        for (var y = 0; y < cellsCount.Y; y++)
        //        {
        //            for (var z = 0; z < cellsCount.Z; z++)
        //            {
        //                byte indestructibleContent;
        //                var matCell = _voxelMaterialCells[x][y][z];

        //                var materialCount = reader.ReadByte();

        //                if (materialCount == 1)
        //                {
        //                    indestructibleContent = reader.ReadByte();
        //                    var materialName = reader.ReadString();
        //                    matCell.Reset(SpaceEngineersCore.Resources.GetMaterialIndex(materialName), indestructibleContent);
        //                }
        //                else
        //                {
        //                    Vector3I voxelCoordInCell;
        //                    for (voxelCoordInCell.X = 0; voxelCoordInCell.X < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.X++)
        //                    {
        //                        for (voxelCoordInCell.Y = 0; voxelCoordInCell.Y < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.Y++)
        //                        {
        //                            for (voxelCoordInCell.Z = 0; voxelCoordInCell.Z < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.Z++)
        //                            {
        //                                indestructibleContent = reader.ReadByte();
        //                                var materialName = reader.ReadString();
        //                                materialCount = reader.ReadByte();
        //                                matCell.SetMaterialAndIndestructibleContent(SpaceEngineersCore.Resources.GetMaterialIndex(materialName), indestructibleContent, ref voxelCoordInCell);
        //                            }
        //                        }
        //                    }

        //                    matCell.CalcAverageCellMaterial();
        //                }
        //            }
        //        }
        //    }
        //}

        //[Obsolete]
        //private void LoadUncompressedV2(BinaryReader reader, bool loadMaterial, int materialBaseCount)
        //{
        //    var materialIndexDict = new Dictionary<byte, byte>();
        //    for (byte i = 0; i < materialBaseCount; i++)
        //    {
        //        var idx = reader.ReadByte();
        //        var materialName = reader.ReadString();
        //        var resourceIdx = SpaceEngineersCore.Resources.GetMaterialIndex(materialName);
        //        materialIndexDict.Add(idx, resourceIdx);
        //    }

        //    var cellsCount = Size / _cellSize;

        //    for (var x = 0; x < cellsCount.X; x++)
        //    {
        //        for (var y = 0; y < cellsCount.Y; y++)
        //        {
        //            for (var z = 0; z < cellsCount.Z; z++)
        //            {
        //                var cellType = (MyVoxelCellType)reader.ReadByte();

        //                //  Cell's are FULL by default, therefore we don't need to change them
        //                if (cellType != MyVoxelCellType.FULL)
        //                {
        //                    var cellCoord = new Vector3I(x, y, z);
        //                    var newCell = new MyVoxelContentCell();
        //                    _voxelContentCells[cellCoord.X][cellCoord.Y][cellCoord.Z] = newCell;

        //                    if (cellType == MyVoxelCellType.EMPTY)
        //                    {
        //                        newCell.SetToEmpty();
        //                    }
        //                    else if (cellType == MyVoxelCellType.MIXED)
        //                    {
        //                        BoundingBoxD box;
        //                        newCell.SetAllVoxelContents(reader.ReadBytes(_cellSize.X * _cellSize.Y * _cellSize.Z), out box);
        //                        _boundingContent.Min = Vector3D.Min(_boundingContent.Min, new Vector3D((x << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS) + box.Min.X, (y << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS) + box.Min.Y, (z << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS) + box.Min.Z));
        //                        _boundingContent.Max = Vector3D.Max(_boundingContent.Max, new Vector3D((x << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS) + box.Max.X, (y << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS) + box.Max.Y, (z << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS) + box.Max.Z));
        //                    }
        //                    // ignore else condition
        //                }
        //                else
        //                {
        //                    _boundingContent.Min = Vector3D.Min(_boundingContent.Min, new Vector3D(x << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS, y << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS, z << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS));
        //                    _boundingContent.Max = Vector3D.Max(_boundingContent.Max, new Vector3D((x + 1 << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS) - 1, (y + 1 << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS) - 1, (z + 1 << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS) - 1));
        //                }
        //            }
        //        }
        //    }

        //    if (reader.PeekChar() == -1 || !loadMaterial)
        //    {
        //        return;
        //    }

        //    // Read materials.
        //    const byte indestructibleContent = 0xff;
        //    for (var x = 0; x < cellsCount.X; x++)
        //    {
        //        for (var y = 0; y < cellsCount.Y; y++)
        //        {
        //            for (var z = 0; z < cellsCount.Z; z++)
        //            {
        //                var matCell = _voxelMaterialCells[x][y][z];

        //                var materialCount = reader.ReadByte();

        //                if (materialCount == (byte)MyVoxelCellType.FULL)
        //                {
        //                    var materialIndex = reader.ReadByte();
        //                    matCell.Reset(materialIndexDict[materialIndex], indestructibleContent);
        //                }
        //                else
        //                {
        //                    Vector3I voxelCoordInCell;
        //                    for (voxelCoordInCell.X = 0; voxelCoordInCell.X < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.X++)
        //                    {
        //                        for (voxelCoordInCell.Y = 0; voxelCoordInCell.Y < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.Y++)
        //                        {
        //                            for (voxelCoordInCell.Z = 0; voxelCoordInCell.Z < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.Z++)
        //                            {
        //                                var materialIndex = reader.ReadByte();
        //                                materialCount = reader.ReadByte();
        //                                matCell.SetMaterialAndIndestructibleContent(materialIndexDict[materialIndex], indestructibleContent, ref voxelCoordInCell);
        //                            }
        //                        }
        //                    }
        //                    matCell.CalcAverageCellMaterial();
        //                }
        //            }
        //        }
        //    }
        //}

        #endregion

        #region LoadVoxelSize

        /// <summary>
        /// Loads the header details only for voxel files, without having to decompress the entire file.
        /// </summary>
        /// <param name="filename"></param>
        public static Vector3I LoadVoxelSize(string filename)
        {
            try
            {
                if (Path.GetExtension(filename).Equals(V2FileExtension, StringComparison.InvariantCultureIgnoreCase))
                {
                    var map = new MyVoxelMap();
                    map.Load(filename);
                    map.Dispose();
                    return map.Size;
                }

                // Leaving the .vox file to the old code, as we only need to interrogate it for the voxel size, not load it into memory.

                // only 29 bytes are required for the header, but I'll leave it for 32 for a bit of extra leeway.
                var buffer = Uncompress(filename, 32);

                using (var reader = new BinaryReader(new MemoryStream(buffer)))
                {
                    reader.ReadInt32();// fileVersion
                    var sizeX = reader.ReadInt32();
                    var sizeY = reader.ReadInt32();
                    var sizeZ = reader.ReadInt32();

                    return new Vector3I(sizeX, sizeY, sizeZ);
                }
            }
            catch
            {
                return Vector3I.Zero;
            }
        }

        #endregion

        #region Save

        /// <summary>
        /// Saves the asteroid to the specified filename.
        /// </summary>
        /// <param name="filename">the file extension indicates the version of file been saved.</param>
        public void Save(string filename)
        {
            Debug.Write("Saving binary.");


            //var initialVersion = Path.GetExtension(filename).Equals(V2FileExtension, StringComparison.InvariantCultureIgnoreCase) ? 1 : 0;

            //var tempfilename = TempfileUtil.NewFilename();
            //using (var ms = new FileStream(tempfilename, FileMode.Create))
            //{
            //    Save(initialVersion, new BinaryWriter(ms), true);
            //}

            //Debug.Write("Compressing.");

            byte[] array;
            _storage.Save(out array);

            File.WriteAllBytes(filename, array);

            //if (initialVersion == 1)
            //    ZipTools.GZipCompress(tempfilename, filename);
            //else
            //    Compress(tempfilename, filename);

            //File.Delete(tempfilename);
            Debug.Write("Done.");
        }

        //[Obsolete]
        //public void Save(int initialVersion, BinaryWriter writer, bool saveMaterialContent)
        //{
        //    switch (initialVersion)
        //    {
        //        case 1:
        //            writer.Write(TagCell);

        //            //  Version of a VOX file
        //            writer.Write((byte)MyVoxelConstants.VOXEL_FILE_ACTUAL_VERSION);
        //            SaveV2(writer, saveMaterialContent);
        //            break;
        //        default:
        //            //  Version of a VOX file
        //            writer.Write(MyVoxelConstants.VOXEL_LEGACY_FILE_ACTUAL_VERSION);
        //            SaveV1(writer, saveMaterialContent);
        //            break;
        //    }
        //}

        //[Obsolete]
        //private void SaveV1(BinaryWriter writer, bool saveMaterialContent)
        //{
        //    //  Size of this voxel map (in voxels)
        //    writer.Write(Size.X);
        //    writer.Write(Size.Y);
        //    writer.Write(Size.Z);

        //    //  Size of data cell in voxels, doesn't have to be same as current size specified by our constants.
        //    writer.Write(MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS);
        //    writer.Write(MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS);
        //    writer.Write(MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS);

        //    Vector3I cellCoord;
        //    for (cellCoord.X = 0; cellCoord.X < _dataCellsCount.X; cellCoord.X++)
        //    {
        //        for (cellCoord.Y = 0; cellCoord.Y < _dataCellsCount.Y; cellCoord.Y++)
        //        {
        //            for (cellCoord.Z = 0; cellCoord.Z < _dataCellsCount.Z; cellCoord.Z++)
        //            {
        //                var voxelCell = _voxelContentCells[cellCoord.X][cellCoord.Y][cellCoord.Z];

        //                if (voxelCell == null)
        //                {
        //                    //  Voxel wasn't found in cell dictionary, so cell must be full
        //                    writer.Write((byte)MyVoxelCellType.FULL);
        //                }
        //                else
        //                {
        //                    //MyCommonDebugUtils.AssertDebug((voxelCell.CellType == MyVoxelCellType.EMPTY) || (voxelCell.CellType == MyVoxelCellType.MIXED));

        //                    //  Cell type
        //                    writer.Write((byte)voxelCell.CellType);

        //                    //  Cell coordinates - otherwise we won't know which cell is this when loading a voxel map
        //                    //compressFile.Add(cellCoord.X);  // not required in SE.
        //                    //compressFile.Add(cellCoord.Y);  // not required in SE.
        //                    //compressFile.Add(cellCoord.Z);  // not required in SE.

        //                    //  If we are here, cell is empty or mixed. If empty, we don't need to save each individual voxel.
        //                    //  But if it is mixed, we will do it here.
        //                    if (voxelCell.CellType == MyVoxelCellType.MIXED)
        //                    {
        //                        Vector3I voxelCoordInCell;
        //                        for (voxelCoordInCell.X = 0; voxelCoordInCell.X < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.X++)
        //                        {
        //                            for (voxelCoordInCell.Y = 0; voxelCoordInCell.Y < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.Y++)
        //                            {
        //                                for (voxelCoordInCell.Z = 0; voxelCoordInCell.Z < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.Z++)
        //                                {
        //                                    writer.Write(voxelCell.GetVoxelContent(ref voxelCoordInCell));
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    if (saveMaterialContent)
        //    {
        //        // Save material cells
        //        for (cellCoord.X = 0; cellCoord.X < _dataCellsCount.X; cellCoord.X++)
        //        {
        //            for (cellCoord.Y = 0; cellCoord.Y < _dataCellsCount.Y; cellCoord.Y++)
        //            {
        //                for (cellCoord.Z = 0; cellCoord.Z < _dataCellsCount.Z; cellCoord.Z++)
        //                {
        //                    var matCell = _voxelMaterialCells[cellCoord.X][cellCoord.Y][cellCoord.Z];
        //                    var voxelCoordInCell = new Vector3I(0, 0, 0);
        //                    var isWholeMaterial = matCell.IsSingleMaterialForWholeCell;
        //                    writer.Write((byte)(isWholeMaterial ? 0x01 : 0x00));
        //                    if (isWholeMaterial)
        //                    {
        //                        writer.Write(matCell.GetIndestructibleContent(ref voxelCoordInCell));
        //                        writer.Write(SpaceEngineersCore.Resources.GetMaterialName(matCell.GetMaterial(ref voxelCoordInCell), VoxelMaterial));
        //                    }
        //                    else
        //                    {
        //                        for (voxelCoordInCell.X = 0; voxelCoordInCell.X < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.X++)
        //                        {
        //                            for (voxelCoordInCell.Y = 0; voxelCoordInCell.Y < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.Y++)
        //                            {
        //                                for (voxelCoordInCell.Z = 0; voxelCoordInCell.Z < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.Z++)
        //                                {
        //                                    writer.Write(matCell.GetIndestructibleContent(ref voxelCoordInCell));
        //                                    writer.Write(SpaceEngineersCore.Resources.GetMaterialName(matCell.GetMaterial(ref voxelCoordInCell), VoxelMaterial));
        //                                    writer.Write((byte)0x0);
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        //[Obsolete]
        //private void SaveV2(BinaryWriter writer, bool saveMaterialContent)
        //{
        //    //  Size of this voxel map (in voxels)
        //    writer.Write(Size.X);
        //    writer.Write(Size.Y);
        //    writer.Write(Size.Z);

        //    //  Size of data cell in voxels, doesn't have to be same as current size specified by our constants.
        //    writer.Write(MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS);
        //    writer.Write(MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS);
        //    writer.Write(MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS);

        //    var materials = SpaceEngineersCore.Resources.VoxelMaterialDefinitions;
        //    writer.Write((byte)materials.Count);
        //    for (byte idx = 0; idx < materials.Count; idx++)
        //    {
        //        writer.Write(idx);
        //        writer.Write(materials[idx].Id.SubtypeName);
        //    }

        //    Vector3I cellCoord;
        //    for (cellCoord.X = 0; cellCoord.X < _dataCellsCount.X; cellCoord.X++)
        //    {
        //        for (cellCoord.Y = 0; cellCoord.Y < _dataCellsCount.Y; cellCoord.Y++)
        //        {
        //            for (cellCoord.Z = 0; cellCoord.Z < _dataCellsCount.Z; cellCoord.Z++)
        //            {
        //                var voxelCell = _voxelContentCells[cellCoord.X][cellCoord.Y][cellCoord.Z];

        //                if (voxelCell == null)
        //                {
        //                    //  Voxel wasn't found in cell dictionary, so cell must be full
        //                    writer.Write((byte)MyVoxelCellType.FULL);
        //                }
        //                else
        //                {
        //                    //  Cell type
        //                    writer.Write((byte)voxelCell.CellType);

        //                    //  If we are here, cell is empty or mixed. If empty, we don't need to save each individual voxel.
        //                    //  But if it is mixed, we will do it here.
        //                    if (voxelCell.CellType == MyVoxelCellType.MIXED)
        //                    {
        //                        Vector3I voxelCoordInCell;
        //                        for (voxelCoordInCell.X = 0; voxelCoordInCell.X < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.X++)
        //                        {
        //                            for (voxelCoordInCell.Y = 0; voxelCoordInCell.Y < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.Y++)
        //                            {
        //                                for (voxelCoordInCell.Z = 0; voxelCoordInCell.Z < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.Z++)
        //                                {
        //                                    writer.Write(voxelCell.GetVoxelContent(ref voxelCoordInCell));
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    if (saveMaterialContent)
        //    {
        //        // Save material cells
        //        for (cellCoord.X = 0; cellCoord.X < _dataCellsCount.X; cellCoord.X++)
        //        {
        //            for (cellCoord.Y = 0; cellCoord.Y < _dataCellsCount.Y; cellCoord.Y++)
        //            {
        //                for (cellCoord.Z = 0; cellCoord.Z < _dataCellsCount.Z; cellCoord.Z++)
        //                {
        //                    var matCell = _voxelMaterialCells[cellCoord.X][cellCoord.Y][cellCoord.Z];
        //                    var voxelCoordInCell = new Vector3I(0, 0, 0);
        //                    var isWholeMaterial = matCell.IsSingleMaterialForWholeCell;
        //                    writer.Write((byte)(isWholeMaterial ? 0x01 : 0x00));
        //                    if (isWholeMaterial)
        //                    {
        //                        //writer.Write(matCell.GetIndestructibleContent(ref voxelCoordInCell));
        //                        //writer.Write(SpaceEngineersCore.Resources.GetMaterialName(matCell.GetMaterial(ref voxelCoordInCell), VoxelMaterial));
        //                        writer.Write(matCell.GetMaterial(ref voxelCoordInCell));
        //                    }
        //                    else
        //                    {
        //                        for (voxelCoordInCell.X = 0; voxelCoordInCell.X < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.X++)
        //                        {
        //                            for (voxelCoordInCell.Y = 0; voxelCoordInCell.Y < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.Y++)
        //                            {
        //                                for (voxelCoordInCell.Z = 0; voxelCoordInCell.Z < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.Z++)
        //                                {
        //                                    //writer.Write(matCell.GetIndestructibleContent(ref voxelCoordInCell));
        //                                    //writer.Write(SpaceEngineersCore.Resources.GetMaterialName(matCell.GetMaterial(ref voxelCoordInCell), VoxelMaterial));
        //                                    writer.Write(matCell.GetMaterial(ref voxelCoordInCell));
        //                                    writer.Write((byte)0x0);
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        #endregion

        #region Un/Compress

        [Obsolete]
        public static void Compress(string sourceFilename, string destinationFilename)
        {
            // Low memory, fast compress.
            using (var originalByteStream = new FileStream(sourceFilename, FileMode.Open))
            {
                if (File.Exists(destinationFilename))
                    File.Delete(destinationFilename);

                using (var compressedByteStream = new FileStream(destinationFilename, FileMode.CreateNew))
                {
                    compressedByteStream.Write(BitConverter.GetBytes(originalByteStream.Length), 0, 4);

                    // GZipStream requires using. Do not optimize the stream.
                    using (var compressionStream = new GZipStream(compressedByteStream, CompressionMode.Compress, true))
                    {
                        originalByteStream.CopyTo(compressionStream);
                    }

                    Debug.WriteLine("Compressed from {0:#,###0} bytes to {1:#,###0} bytes.", originalByteStream.Length, compressedByteStream.Length);
                }
            }
        }

        [Obsolete]
        public static void Uncompress(string sourceFilename, string destinationFilename)
        {
            // Low memory, fast extract.
            using (var compressedByteStream = new FileStream(sourceFilename, FileMode.Open, FileAccess.Read))
            {
                var reader = new BinaryReader(compressedByteStream);
                // message Length.
                reader.ReadInt32();

                if (File.Exists(destinationFilename))
                    File.Delete(destinationFilename);

                using (var outStream = new FileStream(destinationFilename, FileMode.CreateNew))
                {
                    // GZipStream requires using. Do not optimize the stream.
                    using (var zip = new GZipStream(compressedByteStream, CompressionMode.Decompress))
                    {
                        zip.CopyTo(outStream);
                        Debug.WriteLine("Decompressed from {0:#,###0} bytes to {1:#,###0} bytes.", compressedByteStream.Length, outStream.Length);
                    }
                }
            }
        }

        /// <summary>
        /// Used for loading older format voxel file streams, like .vox and first version of .vx2
        /// This is kept for legacy purposes, nothing more.
        /// </summary>
        public static byte[] Uncompress(string sourceFilename, int numberBytes)
        {
            using (var compressedByteStream = new FileStream(sourceFilename, FileMode.Open, FileAccess.Read))
            {
                var reader = new BinaryReader(compressedByteStream);
                // message Length.
                reader.ReadInt32();

                // GZipStream requires using. Do not optimize the stream.
                using (var zip = new GZipStream(compressedByteStream, CompressionMode.Decompress))
                {
                    var arr = new byte[numberBytes];
                    zip.Read(arr, 0, numberBytes);
                    return arr;
                }
            }
        }

        #endregion

        #region methods

        #region MergeVoxelMaterials

        // MyMwcVoxelFilesEnum is obsoleted in SE version 01.60.xx.

        ////  Merges specified materials (from file) into our actual voxel map - overwriting materials only.
        ////  We are using a regular voxel map to define areas where we want to set a specified material. Empty voxels are ignored and 
        ////  only mixed/full voxels are used to tell us that that voxel will contain new material - 'materialToSet'.
        ////  If we are seting indestructible material, voxel content values from merged voxel map will be used to define indestructible content.
        ////  Parameter 'voxelPosition' - place where we will place merged voxel map withing actual voxel map. It's in voxel coords.
        ////  IMPORTANT: THIS METHOD WILL WORK ONLY IF WE PLACE THE MAP THAT WE TRY TO MERGE FROM IN VOXEL COORDINATES THAT ARE MULTIPLY OF DATA CELL SIZE
        ////  This method is used to load small material areas, overwriting actual material only if value from file is 1. Zeros are ignored (it's empty space).
        ////  This method is quite fast, even on large maps - 512x512x512, so we can do more overwrites.
        ////  Parameter 'materialToSet' tells us what material to set at places which are full in file. Empty are ignored - so stay as they were before this method was called.
        ////  IMPORTANT: THIS MERGE MATERIAL CAN BE CALLED ONLY AFTER ALL VOXEL CONTENTS ARE LOADED. THAT'S BECAUSE WE NEED TO KNOW THEM FOR MIN CONTENT / INDESTRUCTIBLE CONTENT.
        ////  Voxel map we are trying to merge into existing voxel map can be bigger or outside of area of existing voxel map. This method will just ignore those parts.
        //public void MergeVoxelMaterials(MyMwcVoxelFilesEnum voxelFile, Vector3I voxelPosition, string materialToSet)
        //{
        //    var filename = Path.Combine(ToolboxUpdater.GetApplicationFilePath(), voxelFile + V2FileExtension);
        //    var dataStore = new MyVoxelMap();
        //    dataStore.Load(filename, SpaceEngineersCore.Resources.GetDefaultMaterialName(), false);

        //    var cellsCountX = dataStore.Size.X / dataStore._cellSize.X;
        //    var cellsCountY = dataStore.Size.Y / dataStore._cellSize.Y;
        //    var cellsCountZ = dataStore.Size.Z / dataStore._cellSize.Z;

        //    //  This method will work only if we place the map that we try to merge from in voxel coordinates that are multiply of data cell size
        //    Debug.Assert((voxelPosition.X & MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_MASK) == 0);
        //    Debug.Assert((voxelPosition.Y & MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_MASK) == 0);
        //    Debug.Assert((voxelPosition.Z & MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_MASK) == 0);
        //    Vector3I cellFullForVoxelPosition = dataStore.GetDataCellCoordinate(ref voxelPosition);

        //    Vector3I cellCoord;
        //    for (cellCoord.X = 0; cellCoord.X < cellsCountX; cellCoord.X++)
        //    {
        //        for (cellCoord.Y = 0; cellCoord.Y < cellsCountY; cellCoord.Y++)
        //        {
        //            for (cellCoord.Z = 0; cellCoord.Z < cellsCountZ; cellCoord.Z++)
        //            {
        //                // TODO:
        //            }
        //        }
        //    }
        //}

        #endregion

        #region SetVoxelContent

        //  Voxel at specified coordinate 'x, y, z' sets to value 'content'
        //  Coordinates are relative to voxel map
        [Obsolete]
        internal void SetVoxelContent(byte content, ref Vector3I voxelCoord, bool needLock = true)
        {
            //  We don't change voxel if it's a border voxel and it would be an empty voxel (not full). Because that would make voxel map with wrong/missing edges.
            if ((content > 0) && (IsVoxelAtBorder(ref voxelCoord))) return;

            var cellCoord = GetDataCellCoordinate(ref voxelCoord);
            var voxelCell = GetCell(ref cellCoord);

            if (voxelCell == null)
            {
                //  Voxel wasn't found in cell dictionary, therefore cell must be FULL

                if (content == MyVoxelConstants.VOXEL_CONTENT_FULL)
                {
                    //  Cell is full and we are seting voxel to full, so nothing needs to be changed
                    return;
                }
                else
                {
                    //  We are switching cell from type FULL to EMPTY or MIXED, therefore we need to allocate new cell
                    var newCell = AddCell(ref cellCoord);
                    var voxelCoordInCell = GetVoxelCoordinatesInDataCell(ref voxelCoord);
                    newCell.SetVoxelContent(content, ref voxelCoordInCell);
                }
            }
            else if (voxelCell.CellType == MyVoxelCellType.FULL)
            {
                if (content == MyVoxelConstants.VOXEL_CONTENT_FULL)
                {
                    //  Cell is full and we are seting voxel to full, so nothing needs to be changed
                    return;
                }
                else
                {
                    var voxelCoordInCell = GetVoxelCoordinatesInDataCell(ref voxelCoord);
                    voxelCell.SetVoxelContent(content, ref voxelCoordInCell);
                    CheckIfCellChangedToFull(voxelCell, ref cellCoord);
                }
            }
            else if (voxelCell.CellType == MyVoxelCellType.EMPTY)
            {
                if (content == MyVoxelConstants.VOXEL_CONTENT_EMPTY)
                {
                    //  Cell is empty and we are seting voxel to empty, so nothing needs to be changed
                    return;
                }
                else
                {
                    var voxelCoordInCell = GetVoxelCoordinatesInDataCell(ref voxelCoord);
                    voxelCell.SetVoxelContent(content, ref voxelCoordInCell);
                    CheckIfCellChangedToFull(voxelCell, ref cellCoord);
                }
            }
            else if (voxelCell.CellType == MyVoxelCellType.MIXED)
            {
                var voxelCoordInCell = GetVoxelCoordinatesInDataCell(ref voxelCoord);
                var oldContent = voxelCell.GetVoxelContent(ref voxelCoordInCell);
                voxelCell.SetVoxelContent(content, ref voxelCoordInCell);
                CheckIfCellChangedToFull(voxelCell, ref cellCoord);
            }
            // ignore else condition.
        }

        #endregion

        #region SetVoxelContentRegion

        [Obsolete]
        public void SetVoxelContentRegion(byte content, int? xMin, int? xMax, int? yMin, int? yMax, int? zMin, int? zMax)
        {
            for (var x = 0; x < Size.X; x++)
            {
                for (var y = 0; y < Size.Y; y++)
                {
                    for (var z = 0; z < Size.Z; z++)
                    {
                        var coords = new Vector3I(x, y, z);

                        if (xMin.HasValue && xMax.HasValue && yMin.HasValue && yMax.HasValue && zMin.HasValue && zMax.HasValue)
                        {
                            if (xMin.Value <= x && x <= xMax.Value && yMin.Value <= y && y <= yMax.Value && zMin.Value <= z && z <= zMax.Value)
                            {
                                SetVoxelContent(content, ref coords);
                            }
                        }
                        else if (xMin.HasValue && xMax.HasValue && yMin.HasValue && yMax.HasValue)
                        {
                            if (xMin.Value <= x && x <= xMax.Value && yMin.Value <= y && y <= yMax.Value)
                            {
                                SetVoxelContent(content, ref coords);
                            }
                        }
                        else if (xMin.HasValue && xMax.HasValue && zMin.HasValue && zMax.HasValue)
                        {
                            if (xMin.Value <= x && x <= xMax.Value && zMin.Value <= z && z <= zMax.Value)
                            {
                                SetVoxelContent(content, ref coords);
                            }
                        }
                        else if (yMin.HasValue && yMax.HasValue && zMin.HasValue && zMax.HasValue)
                        {
                            if (yMin.Value <= y && y <= yMax.Value && zMin.Value <= z && z <= zMax.Value)
                            {
                                SetVoxelContent(content, ref coords);
                            }
                        }
                        else if (xMin.HasValue && xMax.HasValue)
                        {
                            if (xMin.Value <= x && x <= xMax.Value)
                            {
                                SetVoxelContent(content, ref coords);
                            }
                        }
                        else if (yMin.HasValue && yMax.HasValue)
                        {
                            if (yMin.Value <= y && y <= yMax.Value)
                            {
                                SetVoxelContent(content, ref coords);
                            }
                        }
                        else if (zMin.HasValue && zMax.HasValue)
                        {
                            if (zMin.Value <= z && z <= zMax.Value)
                            {
                                SetVoxelContent(content, ref coords);
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region SetVoxelMaterialAndIndestructibleContent

        [Obsolete]
        public void SetVoxelMaterialAndIndestructibleContent(string materialName, byte indestructibleContent, ref Vector3I voxelCoord)
        {
            var cellCoord = GetDataCellCoordinate(ref voxelCoord);
            var voxelCoordInCell = GetVoxelCoordinatesInDataCell(ref voxelCoord);
            var oldMaterial = _voxelMaterialCells[cellCoord.X][cellCoord.Y][cellCoord.Z].GetMaterial(ref voxelCoordInCell);
            _voxelMaterialCells[cellCoord.X][cellCoord.Y][cellCoord.Z].SetMaterialAndIndestructibleContent(SpaceEngineersCore.Resources.GetMaterialIndex(materialName), indestructibleContent, ref voxelCoordInCell);
        }

        #endregion

        #region SetVoxelMaterialRegion

        /// <summary>
        /// Change the material of the voxel cell with the given coordinates
        /// </summary>
        /// <param name="materialName">The material name to set the cell to</param>
        /// <param name="cellCoord">Cell coordinates vector (internal)</param>
        [Obsolete]
        public void SetVoxelMaterialRegion(string materialName, ref Vector3I cellCoord)
        {
            if (!(CheckVoxelCoord(ref cellCoord)))
                return;
            _voxelMaterialCells[cellCoord.X][cellCoord.Y][cellCoord.Z].ForceReplaceMaterial(SpaceEngineersCore.Resources.GetMaterialIndex(materialName));
        }

        #endregion

        #region GetVoxelMaterialContent

        [Obsolete]
        public void GetVoxelMaterialContent(ref Vector3I voxelCoord, out string materialName, out byte content)
        {
            var cellCoord = GetDataCellCoordinate(ref voxelCoord);
            var voxelCoordInCell = GetVoxelCoordinatesInDataCell(ref voxelCoord);

            var oldMaterial = _voxelMaterialCells[cellCoord.X][cellCoord.Y][cellCoord.Z].GetMaterial(ref voxelCoordInCell);
            materialName = SpaceEngineersCore.Resources.GetMaterialName(oldMaterial);
            content = GetVoxelContent(ref voxelCoord);
        }

        #endregion

        #region SeedMaterialSphere

        /// <summary>
        /// Set a material for a random voxel cell and possibly nearest ones to it.
        /// </summary>
        /// <param name="materialName">material name</param>
        /// <param name="radius">radius in voxels, defaults to zero, meaning only a random grid.</param>
        [Obsolete]
        public void SeedMaterialSphere(string materialName, byte radius = 0)
        {
            var fullCells = new List<Vector3I>();
            Vector3I cellCoord;
            // Collect the non-empty cell coordinates
            for (cellCoord.X = 0; cellCoord.X < _dataCellsCount.X; cellCoord.X++)
                for (cellCoord.Y = 0; cellCoord.Y < _dataCellsCount.Y; cellCoord.Y++)
                    for (cellCoord.Z = 0; cellCoord.Z < _dataCellsCount.Z; cellCoord.Z++)
                        if (!CheckCellType(ref _voxelContentCells[cellCoord.X][cellCoord.Y][cellCoord.Z], MyVoxelCellType.EMPTY))
                            fullCells.Add(cellCoord);

            // Choose random cell and switch material there
            fullCells.Shuffle();
            int cellCount = fullCells.Count;
            Vector3I cell, vlen;
            for (int i = 0; i < cellCount; i++)
            {
                cell = fullCells[i];
                if (i == 0)
                {
                    SetVoxelMaterialRegion(materialName, ref cell);
                    continue;
                }
                // Optionally seek adjanced cells and set their material too.
                if (radius == 0)
                    return;
                vlen = fullCells[0] - cell;
                if (vlen.RectangularLength() <= radius)
                {
                    SetVoxelMaterialRegion(materialName, ref cell);
                }
            }
        }

        #endregion

        #region ForceBaseMaterial

        /// <summary>
        /// This will replace all the materials inside the asteroid with specified material.
        /// </summary>
        /// <param name="defaultMaterial"></param>
        /// <param name="materialName"></param>
        public void ForceBaseMaterial(string defaultMaterial, string materialName)
        {
            var materialIndex = SpaceEngineersCore.Resources.GetMaterialIndex(materialName);
            Vector3I block;

            // read the asteroid in chunks of 64 to avoid the Arithmetic overflow issue.
            for (block.Z = 0; block.Z < _storage.Size.Z; block.Z += 64)
                for (block.Y = 0; block.Y < _storage.Size.Y; block.Y += 64)
                    for (block.X = 0; block.X < _storage.Size.X; block.X += 64)
                    {
                        var cacheSize = new Vector3I(64);
                        var cache = new MyStorageData();
                        cache.Resize(cacheSize);
                        // LOD1 is not detailed enough for content information on asteroids.
                        Vector3I maxRange = block + cacheSize - 1;
                        _storage.ReadRange(cache, MyStorageDataTypeFlags.Material, 0, ref block, ref maxRange);

                        Vector3I p;
                        for (p.Z = 0; p.Z < cacheSize.Z; ++p.Z)
                            for (p.Y = 0; p.Y < cacheSize.Y; ++p.Y)
                                for (p.X = 0; p.X < cacheSize.X; ++p.X)
                                {
                                    cache.Material(ref p, materialIndex);
                                }

                        _storage.WriteRange(cache, MyStorageDataTypeFlags.Material, ref block, ref maxRange);
                    }
        }

        #endregion

        #region ForceVoxelFaceMaterial

        /// <summary>
        /// Changes all the min and max face materials to a default to overcome the the hiding rare ore inside of nonrare ore.
        /// </summary>
        /// <param name="materialName"></param>
        [Obsolete("This is no longer required, as the voxel's no longer take their 'surface' texture from the outer most cell.")]
        public void ForceVoxelFaceMaterial(string materialName)
        {
            var materialIndex = SpaceEngineersCore.Resources.GetMaterialIndex(materialName);
            Vector3I block;

            // read the asteroid in chunks of 64 to avoid the Arithmetic overflow issue.
            for (block.Z = 0; block.Z < _storage.Size.Z; block.Z += 64)
                for (block.Y = 0; block.Y < _storage.Size.Y; block.Y += 64)
                    for (block.X = 0; block.X < _storage.Size.X; block.X += 64)
                    {
                        var cacheSize = new Vector3I(64);

                        if (block.X == 0 || block.Y == 0 || block.Z == 0 ||
                            block.X + cacheSize.X == _storage.Size.X - 1 || block.Z + cacheSize.Y == _storage.Size.Y - 1 ||
                            block.Z + cacheSize.Z == _storage.Size.Z - 1)
                        {
                            var cache = new MyStorageData();
                            cache.Resize(cacheSize);
                            // LOD1 is not detailed enough for content information on asteroids.
                            Vector3I maxRange = block + cacheSize - 1;
                            _storage.ReadRange(cache, MyStorageDataTypeFlags.ContentAndMaterial, 0, ref block, ref maxRange);

                            bool changed = false;
                            Vector3I p;
                            for (p.Z = 0; p.Z < cacheSize.Z; ++p.Z)
                                for (p.Y = 0; p.Y < cacheSize.Y; ++p.Y)
                                    for (p.X = 0; p.X < cacheSize.X; ++p.X)
                                    {
                                        var min = p + block;
                                        if (min.X == 0 || min.Y == 0 || min.Z == 0 ||
                                            min.X == _storage.Size.X - 1 || min.Y == _storage.Size.Y - 1 || min.Z == _storage.Size.Z - 1)
                                        {
                                            if (cache.Material(ref p) != materialIndex)
                                            {
                                                cache.Material(ref p, materialIndex);
                                                changed = true;
                                            }
                                        }
                                    }

                            if (changed)
                                _storage.WriteRange(cache, MyStorageDataTypeFlags.ContentAndMaterial, ref block, ref maxRange);
                        }
                    }
        }

        #endregion

        #region ForceShellFaceMaterials

        /// <summary>
        /// Force the material of the outermost mixed voxcells to the given material
        /// </summary>
        /// <param name="materialName"></param>
        /// <param name="targtThickness"></param>
        public void ForceShellMaterial(string materialName, byte targtThickness = 0)
        {
            byte curThickness;
            var materialIndex = SpaceEngineersCore.Resources.GetMaterialIndex(materialName);

            // read the asteroid in chunks of 8 to simulate datacells.
            var cacheSize = new Vector3I(_storage.Size.X >> 3);
            var cache = new MyStorageData();
            cache.Resize(cacheSize);
            _storage.ReadRange(cache, MyStorageDataTypeFlags.ContentAndMaterial, 3, ref Vector3I.Zero, ref cacheSize);

            var writebufferSize = new Vector3I(8);
            var writebuffer = new MyStorageData();
            writebuffer.Resize(writebufferSize);

            Vector3I dataCell;
            for (dataCell.X = 0; dataCell.X < cacheSize.X; dataCell.X ++)
            {
                for (dataCell.Y = 0; dataCell.Y < cacheSize.Y; dataCell.Y ++)
                {
                    for (curThickness = 0, dataCell.Z = 0; dataCell.Z < cacheSize.Z - 1; dataCell.Z++)
                    {
                        Vector3I nextCell = dataCell + new Vector3I(0, 0, 1);
                        if (cache.Content(ref dataCell) != 0 && cache.Content(ref nextCell) != 0)
                        {
                            // read the dataCell location in the storage, and update the material at a byte level.
                            Vector3I bufferPosition = new Vector3I(dataCell.X << 3, dataCell.Y << 3, dataCell.Z << 3);
                            var maxRange = bufferPosition + writebufferSize - 1;
                            writebuffer.ClearMaterials(0);
                            _storage.ReadRange(writebuffer, MyStorageDataTypeFlags.Material, 0, ref bufferPosition, ref maxRange);
                            writebuffer.BlockFillMaterial(Vector3I.Zero, writebufferSize - 1, materialIndex);
                            _storage.WriteRange(writebuffer, MyStorageDataTypeFlags.Material, ref bufferPosition, ref maxRange);

                            if ((targtThickness > 0 && ++curThickness >= targtThickness) || cache.Content(ref nextCell) == 255)
                                break;
                        }
                    }
                    for (curThickness = 0, dataCell.Z = cacheSize.Z - 1; dataCell.Z > 0; dataCell.Z--)
                    {
                        Vector3I nextCell = dataCell + new Vector3I(0, 0, -1);
                        if (cache.Content(ref dataCell) != 0 && cache.Content(ref nextCell) != 0)
                        {
                            Vector3I bufferPosition = new Vector3I(dataCell.X << 3, dataCell.Y << 3, dataCell.Z << 3);
                            var maxRange = bufferPosition + writebufferSize - 1;
                            writebuffer.ClearMaterials(0);
                            _storage.ReadRange(writebuffer, MyStorageDataTypeFlags.Material, 0, ref bufferPosition, ref maxRange);
                            writebuffer.BlockFillMaterial(Vector3I.Zero, writebufferSize - 1, materialIndex);
                            _storage.WriteRange(writebuffer, MyStorageDataTypeFlags.Material, ref bufferPosition, ref maxRange);

                            if ((targtThickness > 0 && ++curThickness >= targtThickness) || cache.Content(ref nextCell) == 255)
                                break;
                        }
                    }
                }
            }

            for (dataCell.X = 0; dataCell.X < cacheSize.X; dataCell.X++)
            {
                for (dataCell.Z = 0; dataCell.Z < cacheSize.Z; dataCell.Z++)
                {
                    for (curThickness = 0, dataCell.Y = 0; dataCell.Y < cacheSize.Y - 1; dataCell.Y++)
                    {
                        Vector3I nextCell = dataCell + new Vector3I(0, 0, 1);
                        if (cache.Content(ref dataCell) != 0 && cache.Content(ref nextCell) != 0)
                        {
                            Vector3I bufferPosition = new Vector3I(dataCell.X << 3, dataCell.Y << 3, dataCell.Z << 3);
                            var maxRange = bufferPosition + writebufferSize - 1;
                            writebuffer.ClearMaterials(0);
                            _storage.ReadRange(writebuffer, MyStorageDataTypeFlags.Material, 0, ref bufferPosition, ref maxRange);
                            writebuffer.BlockFillMaterial(Vector3I.Zero, writebufferSize - 1, materialIndex);
                            _storage.WriteRange(writebuffer, MyStorageDataTypeFlags.Material, ref bufferPosition, ref maxRange);
                            if ((targtThickness > 0 && ++curThickness >= targtThickness) || cache.Content(ref nextCell) == 255)
                                break;
                        }
                    }
                    for (curThickness = 0, dataCell.Y = cacheSize.Y - 1; dataCell.Y > 0; dataCell.Y--)
                    {
                        Vector3I nextCell = dataCell + new Vector3I(0, 0, -1);
                        if (cache.Content(ref dataCell) != 0 && cache.Content(ref nextCell) != 0)
                        {
                            Vector3I bufferPosition = new Vector3I(dataCell.X << 3, dataCell.Y << 3, dataCell.Z << 3);
                            var maxRange = bufferPosition + writebufferSize - 1;
                            writebuffer.ClearMaterials(0);
                            _storage.ReadRange(writebuffer, MyStorageDataTypeFlags.Material, 0, ref bufferPosition, ref maxRange);
                            writebuffer.BlockFillMaterial(Vector3I.Zero, writebufferSize - 1, materialIndex);
                            _storage.WriteRange(writebuffer, MyStorageDataTypeFlags.Material, ref bufferPosition, ref maxRange);
                            if ((targtThickness > 0 && ++curThickness >= targtThickness) || cache.Content(ref nextCell) == 255)
                                break;
                        }
                    }
                }
            }

            for (dataCell.Z = 0; dataCell.Z < cacheSize.Z; dataCell.Z++)
            {
                for (dataCell.Y = 0; dataCell.Y < cacheSize.Y; dataCell.Y++)
                {
                    for (curThickness = 0, dataCell.X = 0; dataCell.X < cacheSize.X - 1; dataCell.X++)
                    {
                        Vector3I nextCell = dataCell + new Vector3I(1, 0, 0);
                        if (cache.Content(ref dataCell) != 0 && cache.Content(ref nextCell) != 0)
                        {
                            Vector3I bufferPosition = new Vector3I(dataCell.X << 3, dataCell.Y << 3, dataCell.Z << 3);
                            var maxRange = bufferPosition + writebufferSize - 1;
                            writebuffer.ClearMaterials(0);
                            _storage.ReadRange(writebuffer, MyStorageDataTypeFlags.Material, 0, ref bufferPosition, ref maxRange);
                            writebuffer.BlockFillMaterial(Vector3I.Zero, writebufferSize - 1, materialIndex);
                            _storage.WriteRange(writebuffer, MyStorageDataTypeFlags.Material, ref bufferPosition, ref maxRange);
                            if ((targtThickness > 0 && ++curThickness >= targtThickness) || cache.Content(ref nextCell) == 255)
                                break;
                        }
                    }
                    for (curThickness = 0, dataCell.X = cacheSize.X - 1; dataCell.X > 0; dataCell.X--)
                    {
                        Vector3I nextCell = dataCell + new Vector3I(-1, 0, 0);
                        if (cache.Content(ref dataCell) != 0 && cache.Content(ref nextCell) != 0)
                        {
                            Vector3I bufferPosition = new Vector3I(dataCell.X << 3, dataCell.Y << 3, dataCell.Z << 3);
                            var maxRange = bufferPosition + writebufferSize - 1;
                            writebuffer.ClearMaterials(0);
                            _storage.ReadRange(writebuffer, MyStorageDataTypeFlags.Material, 0, ref bufferPosition, ref maxRange);
                            writebuffer.BlockFillMaterial(Vector3I.Zero, writebufferSize - 1, materialIndex);
                            _storage.WriteRange(writebuffer, MyStorageDataTypeFlags.Material, ref bufferPosition, ref maxRange);
                            if ((targtThickness > 0 && ++curThickness >= targtThickness) || cache.Content(ref nextCell) == 255)
                                break;
                        }
                    }
                }
            }
        }

        #endregion

        #region RemoveContent

        [Obsolete]
        public void RemoveContent()
        {
            Vector3I cellCoord;

            for (cellCoord.X = 0; cellCoord.X < _dataCellsCount.X; cellCoord.X++)
            {
                for (cellCoord.Y = 0; cellCoord.Y < _dataCellsCount.Y; cellCoord.Y++)
                {
                    for (cellCoord.Z = 0; cellCoord.Z < _dataCellsCount.Z; cellCoord.Z++)
                    {
                        var voxelCell = _voxelContentCells[cellCoord.X][cellCoord.Y][cellCoord.Z];

                        if (voxelCell == null)
                        {
                            var newCell = new MyVoxelContentCell();
                            _voxelContentCells[cellCoord.X][cellCoord.Y][cellCoord.Z] = newCell;
                            newCell.SetToEmpty();
                        }
                        else if (voxelCell.CellType == MyVoxelCellType.MIXED)
                        {
                            // A mixed cell.
                            Vector3I voxelCoordInCell;
                            for (voxelCoordInCell.X = 0; voxelCoordInCell.X < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.X++)
                            {
                                for (voxelCoordInCell.Y = 0; voxelCoordInCell.Y < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.Y++)
                                {
                                    for (voxelCoordInCell.Z = 0; voxelCoordInCell.Z < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.Z++)
                                    {
                                        voxelCell.SetVoxelContent(0x00, ref voxelCoordInCell);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void RemoveContent(string materialName, string replaceFillMaterial)
        {
            var materialIndex = SpaceEngineersCore.Resources.GetMaterialIndex(materialName);
            var replaceMaterialIndex = materialIndex;
            if (!string.IsNullOrEmpty(replaceFillMaterial))
                replaceMaterialIndex = SpaceEngineersCore.Resources.GetMaterialIndex(replaceFillMaterial);
            Vector3I block;

            // read the asteroid in chunks of 64 to avoid the Arithmetic overflow issue.
            for (block.Z = 0; block.Z < _storage.Size.Z; block.Z += 64)
                for (block.Y = 0; block.Y < _storage.Size.Y; block.Y += 64)
                    for (block.X = 0; block.X < _storage.Size.X; block.X += 64)
                    {
                        var cacheSize = new Vector3I(64);
                        var cache = new MyStorageData();
                        cache.Resize(cacheSize);
                        // LOD1 is not detailed enough for content information on asteroids.
                        Vector3I maxRange = block + cacheSize - 1;
                        _storage.ReadRange(cache, MyStorageDataTypeFlags.ContentAndMaterial, 0, ref block, ref maxRange);

                        bool changed = false;
                        Vector3I p;
                        for (p.Z = 0; p.Z < cacheSize.Z; ++p.Z)
                            for (p.Y = 0; p.Y < cacheSize.Y; ++p.Y)
                                for (p.X = 0; p.X < cacheSize.X; ++p.X)
                                {
                                    if (cache.Material(ref p) == materialIndex)
                                    {
                                        cache.Content(ref p, 0);
                                        if (replaceMaterialIndex != materialIndex)
                                            cache.Material(ref p, replaceMaterialIndex);
                                        changed = true;
                                    }
                                }

                        if (changed)
                            _storage.WriteRange(cache, MyStorageDataTypeFlags.ContentAndMaterial, ref block, ref maxRange);
                    }
        }

        [Obsolete]
        public void RemoveMaterial(int? xMin, int? xMax, int? yMin, int? yMax, int? zMin, int? zMax)
        {
            SetVoxelContentRegion(0x00, xMin, xMax, yMin, yMax, zMin, zMax);
        }

        #endregion

        #region ReplaceMaterial

        public void ReplaceMaterial(string materialName, string replaceFillMaterial)
        {
            var materialIndex = SpaceEngineersCore.Resources.GetMaterialIndex(materialName);
            var replaceMaterialIndex = SpaceEngineersCore.Resources.GetMaterialIndex(replaceFillMaterial);

            Vector3I block;

            // read the asteroid in chunks of 64 to avoid the Arithmetic overflow issue.
            for (block.Z = 0; block.Z < _storage.Size.Z; block.Z += 64)
                for (block.Y = 0; block.Y < _storage.Size.Y; block.Y += 64)
                    for (block.X = 0; block.X < _storage.Size.X; block.X += 64)
                    {
                        var cacheSize = new Vector3I(64);
                        var cache = new MyStorageData();
                        cache.Resize(cacheSize);
                        // LOD1 is not detailed enough for content information on asteroids.
                        Vector3I maxRange = block + cacheSize - 1;
                        _storage.ReadRange(cache, MyStorageDataTypeFlags.Material, 0, ref block, ref maxRange);

                        bool changed = false;
                        Vector3I p;
                        for (p.Z = 0; p.Z < cacheSize.Z; ++p.Z)
                            for (p.Y = 0; p.Y < cacheSize.Y; ++p.Y)
                                for (p.X = 0; p.X < cacheSize.X; ++p.X)
                                {
                                    if (cache.Material(ref p) == materialIndex)
                                    {
                                        cache.Material(ref p, replaceMaterialIndex);
                                        changed = true;
                                    }
                                }

                        if (changed)
                            _storage.WriteRange(cache, MyStorageDataTypeFlags.Material, ref block, ref maxRange);
                    }
        }

        #endregion

        #region SumVoxelCells

        private void CalcVoxelCells()
        {
            long sum = 0;

            if (!IsValid)
            {
                _assetCount = new Dictionary<byte, long>();
                _boundingContent = new BoundingBoxD();
                VoxCells = sum;
                return;
            }

            if (_storage.DataProvider is MyPlanetStorageProvider)
            {
                _assetCount = new Dictionary<byte, long>();
                _boundingContent = new BoundingBoxD(Vector3D.Zero, _storage.Size.ToVector3D());
                VoxCells = sum;
                return;
            }

            Vector3I min = Vector3I.MaxValue;
            Vector3I max = Vector3I.MinValue;
            Vector3I block;
            Dictionary<byte, long> assetCount = new Dictionary<byte, long>();

            // read the asteroid in chunks of 64 to avoid the Arithmetic overflow issue.
            for (block.Z = 0; block.Z < _storage.Size.Z; block.Z += 64)
                for (block.Y = 0; block.Y < _storage.Size.Y; block.Y += 64)
                    for (block.X = 0; block.X < _storage.Size.X; block.X += 64)
                    {
                        var cacheSize = new Vector3I(64);
                        var cache = new MyStorageData();
                        cache.Resize(cacheSize);
                        // LOD1 is not detailed enough for content information on asteroids.
                        Vector3I maxRange = block + cacheSize - 1;
                        _storage.ReadRange(cache, MyStorageDataTypeFlags.ContentAndMaterial, 0, ref block, ref maxRange);

                        Vector3I p;
                        for (p.Z = 0; p.Z < cacheSize.Z; ++p.Z)
                            for (p.Y = 0; p.Y < cacheSize.Y; ++p.Y)
                                for (p.X = 0; p.X < cacheSize.X; ++p.X)
                                {
                                    var content = cache.Content(ref p);
                                    if (content > 0)
                                    {
                                        min = Vector3I.Min(min, p + block);
                                        max = Vector3I.Max(max, p + block + 1);

                                        var material = cache.Material(ref p);
                                        if (assetCount.ContainsKey(material))
                                            assetCount[material] += content;
                                        else
                                            assetCount.Add(material, content);
                                        sum += content;
                                    }
                                }
                    }

            _assetCount = assetCount;

            if (min == Vector3I.MaxValue && max == Vector3I.MinValue)
                _boundingContent = new BoundingBoxD();
            else
                _boundingContent = new BoundingBoxD(min, max - 1);
            VoxCells = sum;

            //new MyOctreeStorage()

            //Storage.Geometry.Init(Storage);

            //for (block.Z = 0; block.Z < Storage.Geometry.CellsCount.Z; block.Z ++)
            //    for (block.Y = 0; block.Y < Storage.Geometry.CellsCount.Y; block.Y ++)
            //        for (block.X = 0; block.X < Storage.Geometry.CellsCount.X; block.X ++)
            //        {
            //            MyCellCoord cell = new MyCellCoord(0, ref block);
            //            bool isEmpty;
            //            MyIsoMesh mesh;
            //            if (Storage.Geometry.TryGetMesh(cell, out isEmpty, out mesh))
            //            {
            //            }
            //        }

        }

        //[Obsolete]
        //public long SumVoxelCells()
        //{
        //    long sum = 0;

        //    if (!IsValid)
        //        return sum;

        //    for (var x = 0; x < _voxelContentCells.Length; x++)
        //    {
        //        for (var y = 0; y < _voxelContentCells[x].Length; y++)
        //        {
        //            for (var z = 0; z < _voxelContentCells[x][y].Length; z++)
        //            {
        //                if (_voxelContentCells[x][y][z] != null)
        //                {
        //                    sum += _voxelContentCells[x][y][z].VoxelSum;
        //                }
        //                else
        //                {
        //                    sum += MyVoxelConstants.VOXEL_CELL_CONTENT_SUM_TOTAL;
        //                }
        //            }
        //        }
        //    }

        //    return sum;
        //}

        #endregion

        #region SumFullCells

        //[Obsolete]
        //public long SumFullCells()
        //{
        //    long sum = 0;

        //    for (var x = 0; x < _voxelContentCells.Length; x++)
        //    {
        //        for (var y = 0; y < _voxelContentCells[x].Length; y++)
        //        {
        //            for (var z = 0; z < _voxelContentCells[x][y].Length; z++)
        //            {
        //                if (_voxelContentCells[x][y][z] != null)
        //                {
        //                    sum += _voxelContentCells[x][y][z].VoxelFullCells;
        //                }
        //                else
        //                {
        //                    sum += MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_TOTAL;
        //                }
        //            }
        //        }
        //    }

        //    return sum;
        //}

        #endregion

        #region SumPartCells

        //[Obsolete]
        //public long SumPartCells()
        //{
        //    long sum = 0;

        //    for (var x = 0; x < _voxelContentCells.Length; x++)
        //    {
        //        for (var y = 0; y < _voxelContentCells[x].Length; y++)
        //        {
        //            for (var z = 0; z < _voxelContentCells[x][y].Length; z++)
        //            {
        //                if (_voxelContentCells[x][y][z] != null)
        //                {
        //                    sum += _voxelContentCells[x][y][z].VoxelPartCells;
        //                }
        //                else
        //                {
        //                    sum += MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_TOTAL;
        //                }
        //            }
        //        }
        //    }

        //    return sum;
        //}

        #endregion

        #region CalculateMaterialCellAssets

        public IList<byte> CalcVoxelMaterialList()
        {
            if (!IsValid)
                return null;

            Vector3I block;
            var voxelMaterialList = new List<byte>();

            // read the asteroid in chunks of 64 to avoid the Arithmetic overflow issue.
            for (block.Z = 0; block.Z < _storage.Size.Z; block.Z += 64)
                for (block.Y = 0; block.Y < _storage.Size.Y; block.Y += 64)
                    for (block.X = 0; block.X < _storage.Size.X; block.X += 64)
                    {
                        var cacheSize = new Vector3I(64);
                        var cache = new MyStorageData();
                        cache.Resize(cacheSize);
                        // LOD1 is not detailed enough for content information on asteroids.
                        Vector3I maxRange = block + cacheSize - 1;
                        _storage.ReadRange(cache, MyStorageDataTypeFlags.ContentAndMaterial, 0, ref block, ref maxRange);

                        Vector3I p;
                        for (p.Z = 0; p.Z < cacheSize.Z; ++p.Z)
                            for (p.Y = 0; p.Y < cacheSize.Y; ++p.Y)
                                for (p.X = 0; p.X < cacheSize.X; ++p.X)
                                {
                                    var content = cache.Content(ref p);
                                    if (content > 0)
                                    {
                                        voxelMaterialList.Add(cache.Material(ref p));
                                    }
                                }
                    }

            return voxelMaterialList;
        }

        public void SetVoxelMaterialList(IList<byte> materials)
        {
            if (!IsValid)
                return;

            Vector3I block;
            int index = 0;

            // read the asteroid in chunks of 64 to avoid the Arithmetic overflow issue.
            for (block.Z = 0; block.Z < _storage.Size.Z; block.Z += 64)
                for (block.Y = 0; block.Y < _storage.Size.Y; block.Y += 64)
                    for (block.X = 0; block.X < _storage.Size.X; block.X += 64)
                    {
                        var cacheSize = new Vector3I(64);
                        var cache = new MyStorageData();
                        cache.Resize(cacheSize);
                        // LOD1 is not detailed enough for content information on asteroids.
                        Vector3I maxRange = block + cacheSize - 1;
                        _storage.ReadRange(cache, MyStorageDataTypeFlags.ContentAndMaterial, 0, ref block, ref maxRange);

                        Vector3I p;
                        for (p.Z = 0; p.Z < cacheSize.Z; ++p.Z)
                            for (p.Y = 0; p.Y < cacheSize.Y; ++p.Y)
                                for (p.X = 0; p.X < cacheSize.X; ++p.X)
                                {
                                    var content = cache.Content(ref p);
                                    if (content > 0)
                                    {
                                        cache.Material(ref p, materials[index]);
                                        index++;
                                    }
                                }

                        _storage.WriteRange(cache, MyStorageDataTypeFlags.Material, ref block, ref maxRange);
                    }
        }

        //[Obsolete]
        //public void CalculateMaterialCellAssets(out IList<byte> materialAssetList, out Dictionary<byte, long> materialVoxelCells)
        //{
        //    materialAssetList = new List<byte>();
        //    materialVoxelCells = new Dictionary<byte, long>();
        //    Vector3I cellCoord;

        //    for (cellCoord.X = 0; cellCoord.X < _dataCellsCount.X; cellCoord.X++)
        //    {
        //        for (cellCoord.Y = 0; cellCoord.Y < _dataCellsCount.Y; cellCoord.Y++)
        //        {
        //            for (cellCoord.Z = 0; cellCoord.Z < _dataCellsCount.Z; cellCoord.Z++)
        //            {
        //                var voxelCell = _voxelContentCells[cellCoord.X][cellCoord.Y][cellCoord.Z];
        //                var matCell = _voxelMaterialCells[cellCoord.X][cellCoord.Y][cellCoord.Z];

        //                if (voxelCell == null)
        //                {
        //                    //  Voxel wasn't found in cell dictionary, so cell must be FULL
        //                    if (matCell.IsSingleMaterialForWholeCell)
        //                    {
        //                        for (var i = 0; i < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_TOTAL; i++)
        //                        {
        //                            materialAssetList.Add(matCell.SingleMaterial);

        //                            if (materialVoxelCells.ContainsKey(matCell.SingleMaterial))
        //                                materialVoxelCells[matCell.SingleMaterial] += MyVoxelConstants.VOXEL_CONTENT_FULL;
        //                            else
        //                                materialVoxelCells.Add(matCell.SingleMaterial, MyVoxelConstants.VOXEL_CONTENT_FULL);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        // A full cell, with mixed materials.
        //                        Vector3I voxelCoordInCell;
        //                        for (voxelCoordInCell.X = 0; voxelCoordInCell.X < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.X++)
        //                        {
        //                            for (voxelCoordInCell.Y = 0; voxelCoordInCell.Y < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.Y++)
        //                            {
        //                                for (voxelCoordInCell.Z = 0; voxelCoordInCell.Z < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.Z++)
        //                                {
        //                                    var material = matCell.GetMaterial(ref voxelCoordInCell);
        //                                    materialAssetList.Add(material);

        //                                    if (materialVoxelCells.ContainsKey(material))
        //                                        materialVoxelCells[material] += MyVoxelConstants.VOXEL_CONTENT_FULL;
        //                                    else
        //                                        materialVoxelCells.Add(material, MyVoxelConstants.VOXEL_CONTENT_FULL);
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //                else if (voxelCell.CellType == MyVoxelCellType.MIXED)
        //                {
        //                    if (matCell.IsSingleMaterialForWholeCell)
        //                    {
        //                        // a mixed cell, with one material.
        //                        Vector3I voxelCoordInCell;
        //                        for (voxelCoordInCell.X = 0; voxelCoordInCell.X < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.X++)
        //                        {
        //                            for (voxelCoordInCell.Y = 0; voxelCoordInCell.Y < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.Y++)
        //                            {
        //                                for (voxelCoordInCell.Z = 0; voxelCoordInCell.Z < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.Z++)
        //                                {
        //                                    var content = voxelCell.GetVoxelContent(ref voxelCoordInCell);

        //                                    if (content != MyVoxelConstants.VOXEL_CONTENT_EMPTY)
        //                                    {
        //                                        materialAssetList.Add(matCell.SingleMaterial);

        //                                        if (materialVoxelCells.ContainsKey(matCell.SingleMaterial))
        //                                            materialVoxelCells[matCell.SingleMaterial] += content;
        //                                        else
        //                                            materialVoxelCells.Add(matCell.SingleMaterial, content);
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        // A mixed cell, with mixed materials.
        //                        Vector3I voxelCoordInCell;
        //                        for (voxelCoordInCell.X = 0; voxelCoordInCell.X < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.X++)
        //                        {
        //                            for (voxelCoordInCell.Y = 0; voxelCoordInCell.Y < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.Y++)
        //                            {
        //                                for (voxelCoordInCell.Z = 0; voxelCoordInCell.Z < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.Z++)
        //                                {
        //                                    var content = voxelCell.GetVoxelContent(ref voxelCoordInCell);

        //                                    if (content != MyVoxelConstants.VOXEL_CONTENT_EMPTY)
        //                                    {
        //                                        var material = matCell.GetMaterial(ref voxelCoordInCell);
        //                                        materialAssetList.Add(material);

        //                                        if (materialVoxelCells.ContainsKey(material))
        //                                            materialVoxelCells[material] += content;
        //                                        else
        //                                            materialVoxelCells.Add(material, content);
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        #endregion

        #region SetMaterialAssets

        //[Obsolete]
        //public void SetMaterialAssets(IList<byte> materialsList)
        //{
        //    var materialsIndex = 0;
        //    Vector3I cellCoord;
        //    for (cellCoord.X = 0; cellCoord.X < _dataCellsCount.X; cellCoord.X++)
        //    {
        //        for (cellCoord.Y = 0; cellCoord.Y < _dataCellsCount.Y; cellCoord.Y++)
        //        {
        //            for (cellCoord.Z = 0; cellCoord.Z < _dataCellsCount.Z; cellCoord.Z++)
        //            {
        //                var voxelCell = _voxelContentCells[cellCoord.X][cellCoord.Y][cellCoord.Z];
        //                var matCell = _voxelMaterialCells[cellCoord.X][cellCoord.Y][cellCoord.Z];

        //                // A mixed cell, with mixed materials.
        //                Vector3I voxelCoordInCell;
        //                for (voxelCoordInCell.X = 0; voxelCoordInCell.X < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.X++)
        //                {
        //                    for (voxelCoordInCell.Y = 0; voxelCoordInCell.Y < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.Y++)
        //                    {
        //                        for (voxelCoordInCell.Z = 0; voxelCoordInCell.Z < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.Z++)
        //                        {
        //                            if (voxelCell == null) // Cell is FULL.
        //                            {
        //                                matCell.SetMaterialAndIndestructibleContent(materialsList[materialsIndex++], 0xff, ref voxelCoordInCell);
        //                            }
        //                            else
        //                            {
        //                                // Cell is Mixed.
        //                                var content = voxelCell.GetVoxelContent(ref voxelCoordInCell);

        //                                if (content != MyVoxelConstants.VOXEL_CONTENT_EMPTY) // Only working with cells that aren't empty.
        //                                {
        //                                    matCell.SetMaterialAndIndestructibleContent(materialsList[materialsIndex++], 0xff, ref voxelCoordInCell);
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        #endregion

        #endregion

        #region UpdateContentBounds

        //[Obsolete]
        //public void UpdateContentBounds()
        //{
        //    _boundingContent = new BoundingBoxD(new Vector3I(Size.X, Size.Y, Size.Z), new Vector3I(0, 0, 0));
        //    Vector3I cellCoord;

        //    for (cellCoord.X = 0; cellCoord.X < _dataCellsCount.X; cellCoord.X++)
        //    {
        //        for (cellCoord.Y = 0; cellCoord.Y < _dataCellsCount.Y; cellCoord.Y++)
        //        {
        //            for (cellCoord.Z = 0; cellCoord.Z < _dataCellsCount.Z; cellCoord.Z++)
        //            {
        //                var voxelCell = _voxelContentCells[cellCoord.X][cellCoord.Y][cellCoord.Z];

        //                if (voxelCell == null)
        //                {
        //                    _boundingContent.Min = Vector3D.Min(_boundingContent.Min, new Vector3D(cellCoord.X << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS, cellCoord.Y << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS, cellCoord.Z << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS));
        //                    _boundingContent.Max = Vector3D.Max(_boundingContent.Max, new Vector3D((cellCoord.X + 1 << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS) - 1, (cellCoord.Y + 1 << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS) - 1, (cellCoord.Z + 1 << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS) - 1));
        //                }
        //                //  Cell's are FULL by default, therefore we don't need to change them
        //                else if (voxelCell.CellType == MyVoxelCellType.MIXED)
        //                {

        //                    Vector3I voxelCoordInCell;
        //                    for (voxelCoordInCell.X = 0; voxelCoordInCell.X < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.X++)
        //                    {
        //                        for (voxelCoordInCell.Y = 0; voxelCoordInCell.Y < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.Y++)
        //                        {
        //                            for (voxelCoordInCell.Z = 0; voxelCoordInCell.Z < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.Z++)
        //                            {
        //                                var content = voxelCell.GetVoxelContent(ref voxelCoordInCell);
        //                                if (content > 0)
        //                                {
        //                                    _boundingContent.Min = Vector3D.Min(_boundingContent.Min, new Vector3D((cellCoord.X << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS) + voxelCoordInCell.X, (cellCoord.Y << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS) + voxelCoordInCell.Y, (cellCoord.Z << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS) + voxelCoordInCell.Z));
        //                                    _boundingContent.Max = Vector3D.Max(_boundingContent.Max, new Vector3D((cellCoord.X << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS) + voxelCoordInCell.X, (cellCoord.Y << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS) + voxelCoordInCell.Y, (cellCoord.Z << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS) + voxelCoordInCell.Z));
        //                                }

        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        #endregion

        #region CountAssets

        //[Obsolete]
        //public Dictionary<string, long> CountAssets(IList<byte> materialAssets)
        //{
        //    var assetCount = new Dictionary<byte, long>();
        //    for (var i = 0; i < materialAssets.Count; i++)
        //    {
        //        if (assetCount.ContainsKey(materialAssets[i]))
        //        {
        //            assetCount[materialAssets[i]]++;
        //        }
        //        else
        //        {
        //            assetCount.Add(materialAssets[i], 1);
        //        }
        //    }

        //    var materialDefinitions = SpaceEngineersCore.Resources.VoxelMaterialDefinitions;
        //    var assetNameCount = new Dictionary<string, long>();

        //    foreach (var kvp in assetCount)
        //    {
        //        string name;

        //        if (kvp.Key >= materialDefinitions.Count)
        //            name = materialDefinitions[VoxelMaterial].Id.SubtypeName;
        //        else
        //            name = materialDefinitions[kvp.Key].Id.SubtypeName;

        //        if (assetNameCount.ContainsKey(name))
        //        {
        //            assetNameCount[name] += kvp.Value;
        //        }
        //        else
        //        {
        //            assetNameCount.Add(name, kvp.Value);
        //        }
        //    }

        //    return assetNameCount;
        //}

        public Dictionary<string, long> RefreshAssets()
        {
            CalcVoxelCells();
            return CountAssets();
        }

        public Dictionary<string, long> CountAssets()
        {
            var materialDefinitions = SpaceEngineersCore.Resources.VoxelMaterialDefinitions;
            var assetNameCount = new Dictionary<string, long>();
            var defaultMaterial = SpaceEngineersCore.Resources.GetMaterialIndex(SpaceEngineersCore.Resources.GetDefaultMaterialName());

            foreach (var kvp in _assetCount)
            {
                string name;

                if (kvp.Key >= materialDefinitions.Count)
                    name = materialDefinitions[defaultMaterial].Id.SubtypeName;
                else
                    name = materialDefinitions[kvp.Key].Id.SubtypeName;

                if (assetNameCount.ContainsKey(name))
                {
                    assetNameCount[name] += kvp.Value;
                }
                else
                {
                    assetNameCount.Add(name, kvp.Value);
                }
            }

            return assetNameCount;
        }

        //[Obsolete]
        //public Dictionary<string, long> CountAssets(Dictionary<byte, long> assetCount)
        //{
        //    var materialDefinitions = SpaceEngineersCore.Resources.VoxelMaterialDefinitions;
        //    var assetNameCount = new Dictionary<string, long>();

        //    foreach (var kvp in assetCount)
        //    {
        //        string name;

        //        if (kvp.Key >= materialDefinitions.Count)
        //            name = materialDefinitions[VoxelMaterial].Id.SubtypeName;
        //        else
        //            name = materialDefinitions[kvp.Key].Id.SubtypeName;

        //        if (assetNameCount.ContainsKey(name))
        //        {
        //            assetNameCount[name] += kvp.Value;
        //        }
        //        else
        //        {
        //            assetNameCount.Add(name, kvp.Value);
        //        }
        //    }

        //    return assetNameCount;
        //}

        #endregion

        #region helper methods

        //  Coordinates are relative to voxel map
        [Obsolete]
        private byte GetVoxelContent(ref Vector3I voxelCoord)
        {
            var cellCoord = GetDataCellCoordinate(ref voxelCoord);
            var voxelCell = GetCell(ref cellCoord);

            if (voxelCell == null)
            {
                //  Voxel wasn't found in cell dictionary, therefore cell must be full
                return MyVoxelConstants.VOXEL_CONTENT_FULL;
            }

            var voxelCoordInCell = GetVoxelCoordinatesInDataCell(ref voxelCoord);
            var ret = voxelCell.GetVoxelContent(ref voxelCoordInCell);
            return ret;
        }

        //  Return data cell to which belongs specified voxel (data cell)
        //  IMPORTANT: Input variable 'tempVoxelCoord' is 'ref' only for optimization. Never change its value in the method!!!
        [Obsolete]
        private Vector3I GetDataCellCoordinate(ref Vector3I voxelCoord)
        {
            return new Vector3I(voxelCoord.X >> MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS, voxelCoord.Y >> MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS, voxelCoord.Z >> MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS);
        }

        //  Get cell. If not found (cell is full), null is returned.
        //  IMPORTANT: This method doesn't check if input cell coord0 is inside of the voxel map.
        //  IMPORTANT: This method has overloaded version that is sometimes needed too.
        [Obsolete]
        private MyVoxelContentCell GetCell(ref Vector3I cellCoord)
        {
            if (!CheckVoxelCoord(ref cellCoord)) return null;
            return _voxelContentCells[cellCoord.X][cellCoord.Y][cellCoord.Z];
        }

        //  Return voxel's coordinates relative to cell (in voxel space)
        //  IMPORTANT: Input variable 'tempVoxelCoord' is 'ref' only for optimization. Never change its value in the method!!!
        [Obsolete]
        private Vector3I GetVoxelCoordinatesInDataCell(ref Vector3I voxelCoord)
        {
            return new Vector3I(voxelCoord.X & MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_MASK, voxelCoord.Y & MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_MASK, voxelCoord.Z & MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_MASK);
        }

        [Obsolete]
        private bool CheckVoxelCoord(ref Vector3I cellCoord)
        {
            if (cellCoord.X >= 0 && cellCoord.Y >= 0 && cellCoord.Z >= 0)
            {
                if (cellCoord.X < _voxelContentCells.Length &&
                    cellCoord.Y < _voxelContentCells[cellCoord.X].Length &&
                    cellCoord.Z < _voxelContentCells[cellCoord.X][cellCoord.Y].Length)
                {
                    return true;
                }
            }

            return false;
        }

        //  Checks if cell didn't change to FULL and if is, we set it to null
        [Obsolete]
        private void CheckIfCellChangedToFull(MyVoxelContentCell voxelCell, ref Vector3I cellCoord)
        {
            if (voxelCell.CellType == MyVoxelCellType.FULL)
            {
                _voxelContentCells[cellCoord.X][cellCoord.Y][cellCoord.Z] = null;
            }
        }

        //  Return true if this voxel is on voxel map border
        [Obsolete]
        private bool IsVoxelAtBorder(ref Vector3I voxelCoord)
        {
            if (voxelCoord.X <= 0) return true;
            if (voxelCoord.Y <= 0) return true;
            if (voxelCoord.Z <= 0) return true;
            if (voxelCoord.X >= _sizeMinusOne.X - 1) return true;
            if (voxelCoord.Y >= _sizeMinusOne.Y - 1) return true;
            if (voxelCoord.Z >= _sizeMinusOne.Z - 1) return true;
            return false;
        }

        //  Allocates cell from a buffer, store reference to dictionary and return reference to the cell
        //  Use it when changing cell type from full to empty or mixed.
        [Obsolete]
        private MyVoxelContentCell AddCell(ref Vector3I cellCoord)
        {
            //  Adding or creating cell can be made only once
            Debug.Assert(_voxelContentCells[cellCoord.X][cellCoord.Y][cellCoord.Z] == null);

            var ret = new MyVoxelContentCell();
            _voxelContentCells[cellCoord.X][cellCoord.Y][cellCoord.Z] = ret;
            return ret;
        }

        /// <summary>
        /// Check the given cell type against a possible cell contents state (full, empty, mixed)
        /// </summary>
        /// <param name="cell">MyVoxelContentCell object to check</param>
        /// <param name="type">A cell state to check against (MyVoxelCellType enum)</param>
        /// <returns>whether the cell has the given type or not.</returns>
        [Obsolete]
        private bool CheckCellType(ref MyVoxelContentCell cell, MyVoxelCellType type)
        {
            if (cell == null)
                return type == MyVoxelCellType.FULL;
            return cell.CellType == type;
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~MyVoxelMap()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_storage != null)
                    _storage.Close();
            }
        }

        #endregion

        #region Sandbox.Game.Entities.MyVoxelBase
        // Sandbox.Game.Entities.MyVoxelBase
        public ulong UpdateVoxelShape(OperationType type, MyShape shape, byte material)
        {
            ulong result = 0uL;
            switch (type)
            {
                case OperationType.Fill:
                    result = MyVoxelGenerator.FillInShape(this, shape, material);
                    //result = FillInShape(this, shape, material);
                    break;
                case OperationType.Paint:
                    MyVoxelGenerator.PaintInShape(this, shape, material);
                    break;
                case OperationType.Cut:
                    //result = CutOutShape(this, shape);
                    result = MyVoxelGenerator.CutOutShape(this, shape);
                    break;
            }
            return result;
        }

        #endregion

        #region Sandbox.Engine.Voxels.MyVoxelGenerator

        //public static ulong FillInShape(MyVoxelBase voxelMap, MyShape shape, byte materialIdx)
        //{
        //    Vector3I minCorner, maxCorner, numCells;
        //    ulong retValue = 0;
        //    GetVoxelShapeDimensions(voxelMap, shape, out minCorner, out maxCorner, out numCells);

        //    //voxel must be at least 1 m from side to be closed (e.g. without holes in it)
        //    minCorner = Vector3I.Max(Vector3I.One, minCorner);
        //    maxCorner = Vector3I.Max(minCorner, maxCorner);

        //    for (var itCells = new Vector3I_RangeIterator(ref Vector3I.Zero, ref numCells); itCells.IsValid(); itCells.MoveNext())
        //    {
        //        Vector3I cellMinCorner, cellMaxCorner;
        //        GetCellCorners(ref minCorner, ref maxCorner, ref itCells, out cellMinCorner, out cellMaxCorner);

        //        Vector3I originalMinCorner = cellMinCorner;
        //        Vector3I originalMaxCorner = cellMaxCorner;

        //        voxelMap.Storage.ClampVoxelCoord(ref cellMinCorner, 0);
        //        voxelMap.Storage.ClampVoxelCoord(ref cellMaxCorner, 0);

        //        ClampingInfo minCornerClamping = CheckForClamping(originalMinCorner, cellMinCorner);
        //        ClampingInfo maxCornerClamping = CheckForClamping(originalMaxCorner, cellMaxCorner);

        //        m_cache.Resize(cellMinCorner, cellMaxCorner);
        //        voxelMap.Storage.ReadRange(m_cache, MyStorageDataTypeFlags.ContentAndMaterial, 0, ref cellMinCorner, ref cellMaxCorner);

        //        ulong filledSum = 0;

        //        for (var it = new Vector3I_RangeIterator(ref cellMinCorner, ref cellMaxCorner); it.IsValid(); it.MoveNext())
        //        {
        //            var relPos = it.Current - cellMinCorner; // get original amount
        //            var original = m_cache.Content(ref relPos);

        //            if (original == MyVoxelConstants.VOXEL_CONTENT_FULL) // if there is nothing to add
        //                continue;

        //            //if there was some claping, fill the clamp region with material
        //            if ((it.Current.X == cellMinCorner.X && minCornerClamping.X) || (it.Current.X == cellMaxCorner.X && maxCornerClamping.X) ||
        //                (it.Current.Y == cellMinCorner.Y && minCornerClamping.Y) || (it.Current.Y == cellMaxCorner.Y && maxCornerClamping.Y) ||
        //                (it.Current.Z == cellMinCorner.Z && minCornerClamping.Z) || (it.Current.Z == cellMaxCorner.Z && maxCornerClamping.Z))
        //            {
        //                m_cache.Material(ref relPos, materialIdx);
        //                continue;
        //            }

        //            Vector3D vpos;
        //            MyVoxelCoordSystems.VoxelCoordToWorldPosition(voxelMap.PositionLeftBottomCorner, ref it.Current, out vpos);
        //            var volume = shape.GetVolume(ref vpos);


        //            if (volume <= 0f) // there is nothing to fill
        //                continue;

        //            m_cache.Material(ref relPos, materialIdx); // set material

        //            var toFill = (int)(volume * MyVoxelConstants.VOXEL_CONTENT_FULL);
        //            long newVal = MathHelper.Clamp(original + toFill, 0, Math.Max(original, toFill));

        //            m_cache.Content(ref relPos, (byte)newVal);
        //            filledSum += (ulong)(newVal - original);
        //        }
        //        if (filledSum > 0)
        //        {
        //            RemoveSmallVoxelsUsingChachedVoxels();
        //            voxelMap.Storage.WriteRange(m_cache, MyStorageDataTypeFlags.ContentAndMaterial, ref cellMinCorner, ref cellMaxCorner);
        //        }

        //        retValue += filledSum;
        //    }

        //    //if (retValue > 0)
        //    //    OnVoxelChanged(OperationType.Fill, voxelMap, shape);

        //    return retValue;
        //}

        //private static MyStorageData m_cache = new MyStorageData();
        //const int CELL_SIZE = 16;

        //private static void GetVoxelShapeDimensions(MyVoxelBase voxelMap, MyShape shape, out Vector3I minCorner, out Vector3I maxCorner, out Vector3I numCells)
        //{
        //    {
        //        var bbox = shape.GetWorldBoundaries();
        //        ComputeShapeBounds(voxelMap, ref bbox, voxelMap.PositionLeftBottomCorner, voxelMap.Storage.Size, out minCorner, out maxCorner);
        //    }
        //    numCells = new Vector3I((maxCorner.X - minCorner.X) / CELL_SIZE, (maxCorner.Y - minCorner.Y) / CELL_SIZE, (maxCorner.Z - minCorner.Z) / CELL_SIZE);
        //}

        //private static void ComputeShapeBounds(MyVoxelBase voxelMap,
        //    ref BoundingBoxD shapeAabb, Vector3D voxelMapMinCorner, Vector3I storageSize,
        //    out Vector3I voxelMin, out Vector3I voxelMax)
        //{
        //    MyVoxelCoordSystems.WorldPositionToVoxelCoord(voxelMapMinCorner, ref shapeAabb.Min, out voxelMin);
        //    MyVoxelCoordSystems.WorldPositionToVoxelCoord(voxelMapMinCorner, ref shapeAabb.Max, out voxelMax);
        //    voxelMin += voxelMap.StorageMin;
        //    voxelMax += voxelMap.StorageMin;

        //    voxelMax += 1; //what? why? Another hack of MK?

        //    storageSize -= 1;
        //    Vector3I.Clamp(ref voxelMin, ref Vector3I.Zero, ref storageSize, out voxelMin);
        //    Vector3I.Clamp(ref voxelMax, ref Vector3I.Zero, ref storageSize, out voxelMax);
        //}

        //private static void GetCellCorners(ref Vector3I minCorner, ref Vector3I maxCorner, ref Vector3I_RangeIterator it, out Vector3I cellMinCorner, out Vector3I cellMaxCorner)
        //{
        //    cellMinCorner = new Vector3I(minCorner.X + it.Current.X * CELL_SIZE, minCorner.Y + it.Current.Y * CELL_SIZE, minCorner.Z + it.Current.Z * CELL_SIZE);
        //    cellMaxCorner = new Vector3I(Math.Min(maxCorner.X, cellMinCorner.X + CELL_SIZE), Math.Min(maxCorner.Y, cellMinCorner.Y + CELL_SIZE), Math.Min(maxCorner.Z, cellMinCorner.Z + CELL_SIZE));
        //}

        //private static ClampingInfo CheckForClamping(Vector3I originalValue, Vector3I clampedValue)
        //{
        //    ClampingInfo ret = new ClampingInfo(false, false, false);
        //    if (originalValue.X != clampedValue.X)
        //    {
        //        ret.X = true;
        //    }

        //    if (originalValue.Y != clampedValue.Y)
        //    {
        //        ret.Y = true;
        //    }
        //    if (originalValue.Z != clampedValue.Z)
        //    {
        //        ret.Z = true;
        //    }
        //    return ret;
        //}

        //struct ClampingInfo
        //{
        //    public bool X;
        //    public bool Y;
        //    public bool Z;

        //    public ClampingInfo(bool X, bool Y, bool Z)
        //    {
        //        this.X = X;
        //        this.Y = Y;
        //        this.Z = Z;
        //    }
        //}

        //private static void RemoveSmallVoxelsUsingChachedVoxels()
        //{
        //    // ProfilerShort.Begin("MyVoxelGenerator::RemoveSmallVoxelsUsingChachedVoxels()");
        //    Vector3I voxel;
        //    var cacheSize = m_cache.Size3D;
        //    var sizeMinusOne = cacheSize - 1;
        //    for (voxel.X = 0; voxel.X < cacheSize.X; voxel.X++)
        //    {
        //        for (voxel.Y = 0; voxel.Y < cacheSize.Y; voxel.Y++)
        //        {
        //            for (voxel.Z = 0; voxel.Z < cacheSize.Z; voxel.Z++)
        //            {
        //                //  IMPORTANT: When doing transformations on 'content' value, cast it to int always!!!
        //                //  It's because you can easily forget that result will be negative and if you put negative into byte, it will
        //                //  be overflown and you will be surprised by results!!
        //                int content = m_cache.Content(ref voxel);

        //                //  Check if this is small/invisible voxel (less than 127), but still not empty (more than 0)
        //                if ((content > 0) && (content < MyVoxelConstants.VOXEL_ISO_LEVEL))
        //                {
        //                    Vector3I neighborVoxel;
        //                    Vector3I neighborVoxelMin = voxel - 1;
        //                    Vector3I neighborVoxelMax = voxel + 1;
        //                    Vector3I.Clamp(ref neighborVoxelMin, ref Vector3I.Zero, ref sizeMinusOne, out neighborVoxelMin);
        //                    Vector3I.Clamp(ref neighborVoxelMax, ref Vector3I.Zero, ref sizeMinusOne, out neighborVoxelMax);

        //                    bool foundNonEmptyVoxel = false;
        //                    for (neighborVoxel.X = neighborVoxelMin.X; neighborVoxel.X <= neighborVoxelMax.X; neighborVoxel.X++)
        //                    {
        //                        for (neighborVoxel.Y = neighborVoxelMin.Y; neighborVoxel.Y <= neighborVoxelMax.Y; neighborVoxel.Y++)
        //                        {
        //                            for (neighborVoxel.Z = neighborVoxelMin.Z; neighborVoxel.Z <= neighborVoxelMax.Z; neighborVoxel.Z++)
        //                            {
        //                                int neighborContent = m_cache.Content(ref neighborVoxel);

        //                                //  Check if this is small/invisible voxel
        //                                if (neighborContent >= MyVoxelConstants.VOXEL_ISO_LEVEL)
        //                                {
        //                                    foundNonEmptyVoxel = true;
        //                                    goto END_NEIGHBOR_LOOP;
        //                                }
        //                            }
        //                        }
        //                    }

        //                END_NEIGHBOR_LOOP:

        //                    if (foundNonEmptyVoxel == false)
        //                    {
        //                        m_cache.Content(ref voxel, MyVoxelConstants.VOXEL_CONTENT_EMPTY);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    //   ProfilerShort.End();
        //}

        //public static ulong CutOutShape(MyVoxelBase voxelMap, MyShape shape)
        //{
        //    if (MySession.Static.EnableVoxelDestruction == false)
        //    {
        //        return 0;
        //    }

        //    Vector3I minCorner, maxCorner, numCells;
        //    GetVoxelShapeDimensions(voxelMap, shape, out minCorner, out maxCorner, out numCells);
        //    ulong changedVolumeAmount = 0;

        //    for (var itCells = new Vector3I_RangeIterator(ref Vector3I.Zero, ref numCells); itCells.IsValid(); itCells.MoveNext())
        //    {
        //        Vector3I cellMinCorner, cellMaxCorner;
        //        GetCellCorners(ref minCorner, ref maxCorner, ref itCells, out cellMinCorner, out cellMaxCorner);

        //        var cacheMin = cellMinCorner - 1;
        //        var cacheMax = cellMaxCorner + 1;
        //        voxelMap.Storage.ClampVoxelCoord(ref cacheMin);
        //        voxelMap.Storage.ClampVoxelCoord(ref cacheMax);

        //        ulong removedSum = 0;
        //        m_cache.Resize(cacheMin, cacheMax);
        //        voxelMap.Storage.ReadRange(m_cache, MyStorageDataTypeFlags.Content, 0, ref cacheMin, ref cacheMax);

        //        for (var it = new Vector3I_RangeIterator(ref cellMinCorner, ref cellMaxCorner); it.IsValid(); it.MoveNext())
        //        {
        //            var relPos = it.Current - cacheMin; // get original amount
        //            var original = m_cache.Content(ref relPos);

        //            if (original == MyVoxelConstants.VOXEL_CONTENT_EMPTY) // if there is nothing to remove
        //                continue;

        //            Vector3D vpos;
        //            MyVoxelCoordSystems.VoxelCoordToWorldPosition(voxelMap.PositionLeftBottomCorner, ref it.Current, out vpos);
        //            var volume = shape.GetVolume(ref vpos);

        //            if (volume == 0f) // if there is no intersection
        //                continue;

        //            var toRemove = (int)(MyVoxelConstants.VOXEL_CONTENT_FULL - (volume * MyVoxelConstants.VOXEL_CONTENT_FULL));
        //            var newVal = Math.Min(toRemove, original);
        //            ulong removed = (ulong)Math.Abs(original - newVal);

        //            m_cache.Content(ref relPos, (byte)newVal);
        //            removedSum += removed;
        //        }

        //        if (removedSum > 0)
        //        {
        //            RemoveSmallVoxelsUsingChachedVoxels(); // must stay because of the around when filling voxels
        //            voxelMap.Storage.WriteRange(m_cache, MyStorageDataTypeFlags.Content, ref cacheMin, ref cacheMax);
        //        }

        //        changedVolumeAmount += removedSum;
        //    }

        //    //if (changedVolumeAmount > 0)
        //    //    OnVoxelChanged(OperationType.Cut, voxelMap, shape);

        //    return changedVolumeAmount;
        //}

        #endregion

    }
}