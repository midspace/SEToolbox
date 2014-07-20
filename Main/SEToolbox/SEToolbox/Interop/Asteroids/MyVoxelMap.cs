//===================================================================================
// Voxel Methods/Classes for Space Engineers.
// Based on the Miner-Wars-2081 Mod Kit.
// Copyright (c) Keen Software House a. s.
// This code is expressly licenced, and should not be used in any application without 
// the permission of Keen Software House.
// See http://www.keenswh.com/about.html
// All rights reserved.
//===================================================================================

using System.Collections;

namespace SEToolbox.Interop.Asteroids
{
    using SEToolbox.Support;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using VRageMath;

    public class MyVoxelMap
    {
        #region fields

        // Count of voxel data cells in all directions.
        private Vector3I _dataCellsCount;

        // Array of voxel cells in this voxel map.
        private MyVoxelContentCell[][][] _voxelContentCells;

        // Here we store material for each voxel; And average material for data cell too (that is used for LOD).
        private MyVoxelMaterialCell[][][] _voxelMaterialCells;

        private Vector3I _cellSize;

        private Vector3I _sizeMinusOne;

        private Vector3 _positionLeftBottomCorner;

        private BoundingBox _boundingContent;

        #endregion

        #region properties

        public Int32 FileVersion { get; private set; }

        public Vector3I Size { get; private set; }

        public Vector3I ContentSize { get { return new Vector3I(this._boundingContent.Size()) + 1; } }

        /// <summary>
        /// The center cell in the asteroid.
        /// </summary>
        public Vector3 ContentCenter { get { return this._boundingContent.Center; } }

        public byte VoxelMaterial { get; private set; }

        public string DisplayName { get; private set; }

        #endregion

        #region Init

        // Creates full voxel map, does not initialize base !!! Use only for importing voxel maps from models !!!
        public void Init(string displayName, Vector3 position, Vector3I size, string material)
        {
            InitVoxelMap(displayName, position, size, material);
        }

        private void InitVoxelMap(string displayName, Vector3 position, Vector3I size, string materialName)
        {
            this.Size = size;
            this._sizeMinusOne = new Vector3I(Size.X - 1, Size.Y - 1, Size.Z - 1);
            this.VoxelMaterial = SpaceEngineersApi.GetMaterialIndex(materialName);
            this.DisplayName = displayName;
            this._positionLeftBottomCorner = position;
            this._boundingContent = new BoundingBox(new Vector3I(Size.X, Size.Y, Size.Z), new Vector3I(0, 0, 0));

            // If you need larged voxel maps, enlarge this constant.
            Debug.Assert(Size.X <= MyVoxelConstants.MAX_VOXEL_MAP_SIZE_IN_VOXELS);
            Debug.Assert(Size.Y <= MyVoxelConstants.MAX_VOXEL_MAP_SIZE_IN_VOXELS);
            Debug.Assert(Size.Z <= MyVoxelConstants.MAX_VOXEL_MAP_SIZE_IN_VOXELS);

            // Voxel map size must be multiple of a voxel data cell size.
            Debug.Assert((Size.X & MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_MASK) == 0);
            Debug.Assert((Size.Y & MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_MASK) == 0);
            Debug.Assert((Size.Z & MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_MASK) == 0);
            this._dataCellsCount.X = Size.X >> MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS;
            this._dataCellsCount.Y = Size.Y >> MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS;
            this._dataCellsCount.Z = Size.Z >> MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS;

            // Voxel map size must be multiple of a voxel data cell size.
            Debug.Assert((Size.X % MyVoxelConstants.VOXEL_RENDER_CELL_SIZE_IN_VOXELS) == 0);
            Debug.Assert((Size.Y % MyVoxelConstants.VOXEL_RENDER_CELL_SIZE_IN_VOXELS) == 0);
            Debug.Assert((Size.Z % MyVoxelConstants.VOXEL_RENDER_CELL_SIZE_IN_VOXELS) == 0);

            // Array of voxel cells in this voxel map.
            this._voxelContentCells = new MyVoxelContentCell[this._dataCellsCount.X][][];
            for (var x = 0; x < this._voxelContentCells.Length; x++)
            {
                this._voxelContentCells[x] = new MyVoxelContentCell[this._dataCellsCount.Y][];
                for (var y = 0; y < this._voxelContentCells[x].Length; y++)
                {
                    this._voxelContentCells[x][y] = new MyVoxelContentCell[this._dataCellsCount.Z];
                }
            }

            //  Set base material.
            this._voxelMaterialCells = new MyVoxelMaterialCell[_dataCellsCount.X][][];
            for (var x = 0; x < this._dataCellsCount.X; x++)
            {
                this._voxelMaterialCells[x] = new MyVoxelMaterialCell[_dataCellsCount.Y][];
                for (var y = 0; y < this._dataCellsCount.Y; y++)
                {
                    this._voxelMaterialCells[x][y] = new MyVoxelMaterialCell[_dataCellsCount.Z];
                    for (var z = 0; z < this._dataCellsCount.Z; z++)
                    {
                        this._voxelMaterialCells[x][y][z] = new MyVoxelMaterialCell(this.VoxelMaterial, 0xFF);
                    }
                }
            }
        }

        #endregion

        #region Preview

        public static void GetPreview(string filename, out Vector3I size, out Vector3I contentSize, out Vector3 center, out long voxCells)
        {
            var map = new MyVoxelMap();
            map.Load(filename, SpaceEngineersApi.GetMaterialName(0), false);
            size = map.Size;
            contentSize = map.ContentSize;
            center = map.ContentCenter;
            voxCells = map.SumVoxelCells();
        }

        #endregion

        public static Dictionary<string, long> GetMaterialAssetDetails(string filename)
        {
            var map = new MyVoxelMap();
            map.Load(filename, SpaceEngineersApi.GetMaterialName(0), true);

            IList<byte> materialAssetList;
            Dictionary<byte, long> materialVoxelCells;

            map.CalculateMaterialCellAssets(out materialAssetList, out materialVoxelCells);
            return map.CountAssets(materialVoxelCells);
        }

        #region IsVoxelMapFile

        /// <summary>
        /// check for Magic Number: 1f 8b
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static bool IsVoxelMapFile(string filename)
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
                finally
                {
                    stream.Close();
                }
            }
        }

        #endregion

        #region Load

        public void Load(string filename, string defaultMaterial)
        {
            if (defaultMaterial == null)
                defaultMaterial = SpaceEngineersApi.GetMaterialName(0);
            this.Load(filename, defaultMaterial, true);
        }

        public void Load(string filename, string defaultMaterial, bool loadMaterial)
        {
            var tempfilename = TempfileUtil.NewFilename();
            Uncompress(filename, tempfilename);

            using (var ms = new FileStream(tempfilename, FileMode.Open))
            {
                using (var reader = new BinaryReader(ms))
                {
                    this.LoadUncompressed(Vector3.Zero, Path.GetFileNameWithoutExtension(filename), reader, defaultMaterial, loadMaterial);
                }
            }

            File.Delete(tempfilename);
        }

        public void LoadUncompressed(Vector3 position, string displayName, BinaryReader reader, string defaultMaterial, bool loadMaterial)
        {
            Debug.WriteLine(string.Format("Load: '{0}'", displayName));

            this.FileVersion = reader.ReadInt32();

            var sizeX = reader.ReadInt32();
            var sizeY = reader.ReadInt32();
            var sizeZ = reader.ReadInt32();

            var cellSizeX = reader.ReadInt32();
            var cellSizeY = reader.ReadInt32();
            var cellSizeZ = reader.ReadInt32();

            this._cellSize = new Vector3I(cellSizeX, cellSizeY, cellSizeZ);

            this.InitVoxelMap(displayName, position, new Vector3I(sizeX, sizeY, sizeZ), defaultMaterial);
            var cellsCount = this.Size / this._cellSize;

            for (var x = 0; x < cellsCount.X; x++)
            {
                for (var y = 0; y < cellsCount.Y; y++)
                {
                    for (var z = 0; z < cellsCount.Z; z++)
                    {
                        var cellType = (MyVoxelCellType)reader.ReadByte();

                        //  Cell's are FULL by default, therefore we don't need to change them
                        if (cellType != MyVoxelCellType.FULL)
                        {
                            var cellCoord = new Vector3I(x, y, z);

                            var newCell = new MyVoxelContentCell();
                            this._voxelContentCells[cellCoord.X][cellCoord.Y][cellCoord.Z] = newCell;

                            if (cellType == MyVoxelCellType.EMPTY)
                            {
                                newCell.SetToEmpty();
                            }
                            else if (cellType == MyVoxelCellType.MIXED)
                            {
                                BoundingBox box;
                                newCell.SetAllVoxelContents(reader.ReadBytes(this._cellSize.X * this._cellSize.Y * this._cellSize.Z), out box);
                                this._boundingContent.Min = Vector3.Min(this._boundingContent.Min, new Vector3((x << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS) + box.Min.X, (y << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS) + box.Min.Y, (z << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS) + box.Min.Z));
                                this._boundingContent.Max = Vector3.Max(this._boundingContent.Max, new Vector3((x << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS) + box.Max.X, (y << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS) + box.Max.Y, (z << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS) + box.Max.Z));
                            }
                            // ignore else condition
                        }
                        else
                        {
                            this._boundingContent.Min = Vector3.Min(this._boundingContent.Min, new Vector3(x << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS, y << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS, z << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS));
                            this._boundingContent.Max = Vector3.Max(this._boundingContent.Max, new Vector3((x + 1 << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS) - 1, (y + 1 << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS) - 1, (z + 1 << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS) - 1));
                        }
                    }
                }
            }

            if (reader.PeekChar() == -1 || !loadMaterial)
            {
                return;
            }

            // Read materials and indestructible.
            for (var x = 0; x < cellsCount.X; x++)
            {
                for (var y = 0; y < cellsCount.Y; y++)
                {
                    for (var z = 0; z < cellsCount.Z; z++)
                    {
                        byte indestructibleContent;
                        var matCell = this._voxelMaterialCells[x][y][z];

                        var materialCount = reader.ReadByte();

                        if (materialCount == 1)
                        {
                            indestructibleContent = reader.ReadByte();
                            var materialName = reader.ReadString();
                            matCell.Reset(SpaceEngineersApi.GetMaterialIndex(materialName), indestructibleContent);
                        }
                        else
                        {
                            //Vector3I voxelCoordInCell;
                            for (var voxelCoordInCellX = 0; voxelCoordInCellX < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCellX++)
                            {
                                for (var voxelCoordInCellY = 0; voxelCoordInCellY < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCellY++)
                                {
                                    for (var voxelCoordInCellZ = 0; voxelCoordInCellZ < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCellZ++)
                                    {
                                        indestructibleContent = reader.ReadByte();
                                        var materialName = reader.ReadString();
                                        materialCount = reader.ReadByte();
                                        var coord = new Vector3I(voxelCoordInCellX, voxelCoordInCellY, voxelCoordInCellZ);
                                        matCell.SetMaterialAndIndestructibleContent(SpaceEngineersApi.GetMaterialIndex(materialName), indestructibleContent, ref coord);
                                    }
                                }
                            }

                            matCell.CalcAverageCellMaterial();
                        }
                    }
                }
            }
        }

        #endregion

        #region Save

        public void Save(string filename)
        {
            Debug.Write("Saving binary.");
            var tempfilename = TempfileUtil.NewFilename();
            using (var ms = new FileStream(tempfilename, FileMode.Create))
            {
                this.Save(new BinaryWriter(ms), true);
            }

            Debug.Write("Compressing.");
            Compress(tempfilename, filename);
            File.Delete(tempfilename);
            Debug.Write("Done.");
        }

        public void Save(BinaryWriter writer, bool saveMaterialContent)
        {
            //  Version of a VOX file
            writer.Write(MyVoxelConstants.VOXEL_FILE_ACTUAL_VERSION);

            //  Size of this voxel map (in voxels)
            writer.Write(this.Size.X);
            writer.Write(this.Size.Y);
            writer.Write(this.Size.Z);

            //  Size of data cell in voxels, doesn't have to be same as current size specified by our constants.
            writer.Write(MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS);
            writer.Write(MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS);
            writer.Write(MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS);

            Vector3I cellCoord;
            for (cellCoord.X = 0; cellCoord.X < this._dataCellsCount.X; cellCoord.X++)
            {
                for (cellCoord.Y = 0; cellCoord.Y < this._dataCellsCount.Y; cellCoord.Y++)
                {
                    for (cellCoord.Z = 0; cellCoord.Z < this._dataCellsCount.Z; cellCoord.Z++)
                    {
                        var voxelCell = this._voxelContentCells[cellCoord.X][cellCoord.Y][cellCoord.Z];

                        if (voxelCell == null)
                        {
                            //  Voxel wasn't found in cell dictionary, so cell must be full
                            writer.Write((byte)MyVoxelCellType.FULL);
                        }
                        else
                        {
                            //MyCommonDebugUtils.AssertDebug((voxelCell.CellType == MyVoxelCellType.EMPTY) || (voxelCell.CellType == MyVoxelCellType.MIXED));

                            //  Cell type
                            writer.Write((byte)voxelCell.CellType);

                            //  Cell coordinates - otherwise we won't know which cell is this when loading a voxel map
                            //compressFile.Add(cellCoord.X);  // not required in SE.
                            //compressFile.Add(cellCoord.Y);  // not required in SE.
                            //compressFile.Add(cellCoord.Z);  // not required in SE.

                            //  If we are here, cell is empty or mixed. If empty, we don't need to save each individual voxel.
                            //  But if it is mixed, we will do it here.
                            if (voxelCell.CellType == MyVoxelCellType.MIXED)
                            {
                                Vector3I voxelCoordInCell;
                                for (voxelCoordInCell.X = 0; voxelCoordInCell.X < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.X++)
                                {
                                    for (voxelCoordInCell.Y = 0; voxelCoordInCell.Y < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.Y++)
                                    {
                                        for (voxelCoordInCell.Z = 0; voxelCoordInCell.Z < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.Z++)
                                        {
                                            writer.Write(voxelCell.GetVoxelContent(ref voxelCoordInCell));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (saveMaterialContent)
            {
                // Save material cells
                for (cellCoord.X = 0; cellCoord.X < this._dataCellsCount.X; cellCoord.X++)
                {
                    for (cellCoord.Y = 0; cellCoord.Y < this._dataCellsCount.Y; cellCoord.Y++)
                    {
                        for (cellCoord.Z = 0; cellCoord.Z < this._dataCellsCount.Z; cellCoord.Z++)
                        {
                            var matCell = this._voxelMaterialCells[cellCoord.X][cellCoord.Y][cellCoord.Z];
                            var voxelCoordInCell = new Vector3I(0, 0, 0);
                            var isWholeMaterial = matCell.IsSingleMaterialForWholeCell;
                            writer.Write((byte)(isWholeMaterial ? 0x01 : 0x00));
                            if (isWholeMaterial)
                            {
                                writer.Write(matCell.GetIndestructibleContent(ref voxelCoordInCell));
                                writer.Write(SpaceEngineersApi.GetMaterialName(matCell.GetMaterial(ref voxelCoordInCell), this.VoxelMaterial));
                            }
                            else
                            {
                                for (voxelCoordInCell.X = 0; voxelCoordInCell.X < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.X++)
                                {
                                    for (voxelCoordInCell.Y = 0; voxelCoordInCell.Y < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.Y++)
                                    {
                                        for (voxelCoordInCell.Z = 0; voxelCoordInCell.Z < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.Z++)
                                        {
                                            writer.Write(matCell.GetIndestructibleContent(ref voxelCoordInCell));
                                            writer.Write(SpaceEngineersApi.GetMaterialName(matCell.GetMaterial(ref voxelCoordInCell), this.VoxelMaterial));
                                            writer.Write((byte)0x0);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Un/Compress

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

                    using (var compressionStream = new GZipStream(compressedByteStream, CompressionMode.Compress, true))
                    {
                        originalByteStream.CopyTo(compressionStream);
                    }

                    Debug.WriteLine("Compressed from {0:#,###0} bytes to {1:#,###0} bytes.", originalByteStream.Length, compressedByteStream.Length);
                }
            }
        }

        public static void Uncompress(string sourceFilename, string destinationFilename)
        {
            // Low memory, fast extract.
            using (var compressedByteStream = new FileStream(sourceFilename, FileMode.Open, FileAccess.Read))
            {
                var reader = new BinaryReader(compressedByteStream);
                var msgLength = reader.ReadInt32();

                if (File.Exists(destinationFilename))
                    File.Delete(destinationFilename);

                using (var outStream = new FileStream(destinationFilename, FileMode.CreateNew))
                {
                    using (var zip = new GZipStream(compressedByteStream, CompressionMode.Decompress))
                    {
                        zip.CopyTo(outStream);

                        Debug.WriteLine("Decompressed from {0:#,###0} bytes to {1:#,###0} bytes.", compressedByteStream.Length, outStream.Length);
                    }
                }
            }
        }

        #endregion

        #region methods

        #region SetVoxelContent

        //  Voxel at specified coordinate 'x, y, z' sets to value 'content'
        //  Coordinates are relative to voxel map
        internal void SetVoxelContent(byte content, ref Vector3I voxelCoord, bool needLock = true)
        {
            //  We don't change voxel if it's a border voxel and it would be an empty voxel (not full). Because that would make voxel map with wrong/missing edges.
            if ((content > 0) && (this.IsVoxelAtBorder(ref voxelCoord))) return;

            var cellCoord = this.GetDataCellCoordinate(ref voxelCoord);
            var voxelCell = this.GetCell(ref cellCoord);

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
                    var newCell = this.AddCell(ref cellCoord);
                    var voxelCoordInCell = this.GetVoxelCoordinatesInDataCell(ref voxelCoord);
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
                    var voxelCoordInCell = this.GetVoxelCoordinatesInDataCell(ref voxelCoord);
                    voxelCell.SetVoxelContent(content, ref voxelCoordInCell);
                    this.CheckIfCellChangedToFull(voxelCell, ref cellCoord);
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
                    var voxelCoordInCell = this.GetVoxelCoordinatesInDataCell(ref voxelCoord);
                    voxelCell.SetVoxelContent(content, ref voxelCoordInCell);
                    this.CheckIfCellChangedToFull(voxelCell, ref cellCoord);
                }
            }
            else if (voxelCell.CellType == MyVoxelCellType.MIXED)
            {
                var voxelCoordInCell = this.GetVoxelCoordinatesInDataCell(ref voxelCoord);
                var oldContent = voxelCell.GetVoxelContent(ref voxelCoordInCell);
                voxelCell.SetVoxelContent(content, ref voxelCoordInCell);
                this.CheckIfCellChangedToFull(voxelCell, ref cellCoord);
            }
            // ignore else condition.
        }

        #endregion

        #region SetVoxelMaterialAndIndestructibleContent

        public void SetVoxelMaterialAndIndestructibleContent(string materialName, byte indestructibleContent, ref Vector3I voxelCoord)
        {
            var cellCoord = this.GetDataCellCoordinate(ref voxelCoord);
            var voxelCoordInCell = this.GetVoxelCoordinatesInDataCell(ref voxelCoord);
            var oldMaterial = this._voxelMaterialCells[cellCoord.X][cellCoord.Y][cellCoord.Z].GetMaterial(ref voxelCoordInCell);
            this._voxelMaterialCells[cellCoord.X][cellCoord.Y][cellCoord.Z].SetMaterialAndIndestructibleContent(SpaceEngineersApi.GetMaterialIndex(materialName), indestructibleContent, ref voxelCoordInCell);
        }

        #endregion

        #region ForceBaseMaterial

        /// <summary>
        /// This will replace all the materials inside the asteroid with specified material.
        /// </summary>
        /// <param name="materialName"></param>
        public void ForceBaseMaterial(string defaultMaterial, string materialName)
        {
            var materialIndex = SpaceEngineersApi.GetMaterialIndex(materialName);

            for (var x = 0; x < this._voxelMaterialCells.Length; x++)
            {
                for (var y = 0; y < this._voxelMaterialCells[x].Length; y++)
                {
                    for (var z = 0; z < this._voxelMaterialCells[x][y].Length; z++)
                    {
                        this._voxelMaterialCells[x][y][z].ForceReplaceMaterial(materialIndex);
                    }
                }
            }

            this.ForceVoxelFaceMaterial(defaultMaterial);
        }

        #endregion

        #region ForceVoxelFaceMaterial

        /// <summary>
        /// Changes all the min and max face materials to a default to overcome the the hiding rare ore inside of nonrare ore.
        /// </summary>
        /// <param name="materialName"></param>
        public void ForceVoxelFaceMaterial(string materialName)
        {
            Vector3I coords;

            for (var y = 0; y < this.Size.Y; y++)
            {
                for (var z = 0; z < this.Size.Z; z++)
                {
                    coords = new Vector3I(0, y, z);
                    this.SetVoxelMaterialAndIndestructibleContent(materialName, 0xff, ref coords);

                    coords = new Vector3I(this.Size.X - 1, y, z);
                    this.SetVoxelMaterialAndIndestructibleContent(materialName, 0xff, ref coords);
                }
            }

            for (var x = 0; x < this.Size.X; x++)
            {
                for (var z = 0; z < this.Size.Z; z++)
                {
                    coords = new Vector3I(x, 0, z);
                    this.SetVoxelMaterialAndIndestructibleContent(materialName, 0xff, ref coords);

                    coords = new Vector3I(x, this.Size.Y - 1, z);
                    this.SetVoxelMaterialAndIndestructibleContent(materialName, 0xff, ref coords);
                }
            }

            for (var x = 0; x < this.Size.X; x++)
            {
                for (var y = 0; y < this.Size.Y; y++)
                {
                    coords = new Vector3I(x, y, 0);
                    this.SetVoxelMaterialAndIndestructibleContent(materialName, 0xff, ref coords);

                    coords = new Vector3I(x, y, this.Size.Z - 1);
                    this.SetVoxelMaterialAndIndestructibleContent(materialName, 0xff, ref coords);
                }
            }
        }

        #endregion

        #region RemoveMaterial

        public void RemoveMaterial(string materialName, string replaceFillMaterial)
        {
            var materialIndex = SpaceEngineersApi.GetMaterialIndex(materialName);
            var replaceMaterialIndex = SpaceEngineersApi.GetMaterialIndex(replaceFillMaterial);
            Vector3I cellCoord;

            for (cellCoord.X = 0; cellCoord.X < this._dataCellsCount.X; cellCoord.X++)
            {
                for (cellCoord.Y = 0; cellCoord.Y < this._dataCellsCount.Y; cellCoord.Y++)
                {
                    for (cellCoord.Z = 0; cellCoord.Z < this._dataCellsCount.Z; cellCoord.Z++)
                    {
                        var voxelCell = this._voxelContentCells[cellCoord.X][cellCoord.Y][cellCoord.Z];
                        var matCell = this._voxelMaterialCells[cellCoord.X][cellCoord.Y][cellCoord.Z];

                        if (voxelCell == null)
                        {
                            //  Voxel wasn't found in cell dictionary, so cell must be FULL
                            if (matCell.IsSingleMaterialForWholeCell)
                            {
                                if (matCell.SingleMaterial == materialIndex)
                                {
                                    var newCell = new MyVoxelContentCell();
                                    this._voxelContentCells[cellCoord.X][cellCoord.Y][cellCoord.Z] = newCell;
                                    newCell.SetToEmpty();
                                    matCell.Reset(replaceMaterialIndex, 0xff);
                                }
                            }
                            else
                            {
                                // A full cell, with mixed materials.
                                Vector3I voxelCoordInCell;
                                MyVoxelContentCell newCell = null;

                                for (voxelCoordInCell.X = 0; voxelCoordInCell.X < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.X++)
                                {
                                    for (voxelCoordInCell.Y = 0; voxelCoordInCell.Y < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.Y++)
                                    {
                                        for (voxelCoordInCell.Z = 0; voxelCoordInCell.Z < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.Z++)
                                        {
                                            var material = matCell.GetMaterial(ref voxelCoordInCell);

                                            if (material == materialIndex)
                                            {
                                                if (newCell == null)
                                                {
                                                    newCell = new MyVoxelContentCell();
                                                    this._voxelContentCells[cellCoord.X][cellCoord.Y][cellCoord.Z] = newCell;
                                                }

                                                newCell.SetVoxelContent(0x00, ref voxelCoordInCell);
                                                matCell.SetMaterialAndIndestructibleContent(replaceMaterialIndex, 0xff, ref voxelCoordInCell);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else if (voxelCell.CellType == MyVoxelCellType.MIXED)
                        {
                            if (matCell.IsSingleMaterialForWholeCell)
                            {
                                if (matCell.SingleMaterial == materialIndex)
                                {
                                    voxelCell.SetToEmpty();
                                    matCell.Reset(replaceMaterialIndex, 0xff);
                                }
                            }
                            else
                            {
                                // A mixed cell, with mixed materials.
                                Vector3I voxelCoordInCell;
                                for (voxelCoordInCell.X = 0; voxelCoordInCell.X < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.X++)
                                {
                                    for (voxelCoordInCell.Y = 0; voxelCoordInCell.Y < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.Y++)
                                    {
                                        for (voxelCoordInCell.Z = 0; voxelCoordInCell.Z < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.Z++)
                                        {
                                            var material = matCell.GetMaterial(ref voxelCoordInCell);

                                            if (material == materialIndex)
                                            {
                                                voxelCell.SetVoxelContent(0x00, ref voxelCoordInCell);
                                                matCell.SetMaterialAndIndestructibleContent(replaceMaterialIndex, 0xff, ref voxelCoordInCell);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void RemoveMaterial(int? xMin, int? xMax, int? yMin, int? yMax, int? zMin, int? zMax)
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
                                SetVoxelContent(0x00, ref coords);
                            }
                        }
                        else if (xMin.HasValue && xMax.HasValue && yMin.HasValue && yMax.HasValue)
                        {
                            if (xMin.Value <= x && x <= xMax.Value && yMin.Value <= y && y <= yMax.Value)
                            {
                                SetVoxelContent(0x00, ref coords);
                            }
                        }
                        else if (xMin.HasValue && xMax.HasValue && zMin.HasValue && zMax.HasValue)
                        {
                            if (xMin.Value <= x && x <= xMax.Value && zMin.Value <= z && z <= zMax.Value)
                            {
                                SetVoxelContent(0x00, ref coords);
                            }
                        }
                        else if (yMin.HasValue && yMax.HasValue && zMin.HasValue && zMax.HasValue)
                        {
                            if (yMin.Value <= y && y <= yMax.Value && zMin.Value <= z && z <= zMax.Value)
                            {
                                SetVoxelContent(0x00, ref coords);
                            }
                        }
                        else if (xMin.HasValue && xMax.HasValue)
                        {
                            if (xMin.Value <= x && x <= xMax.Value)
                            {
                                SetVoxelContent(0x00, ref coords);
                            }
                        }
                        else if (yMin.HasValue && yMax.HasValue)
                        {
                            if (yMin.Value <= y && y <= yMax.Value)
                            {
                                SetVoxelContent(0x00, ref coords);
                            }
                        }
                        else if (zMin.HasValue && zMax.HasValue)
                        {
                            if (zMin.Value <= z && z <= zMax.Value)
                            {
                                SetVoxelContent(0x00, ref coords);
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region SumVoxelCells

        public long SumVoxelCells()
        {
            long sum = 0;

            for (var x = 0; x < this._voxelContentCells.Length; x++)
            {
                for (var y = 0; y < this._voxelContentCells[x].Length; y++)
                {
                    for (var z = 0; z < this._voxelContentCells[x][y].Length; z++)
                    {
                        if (this._voxelContentCells[x][y][z] != null)
                        {
                            sum += this._voxelContentCells[x][y][z].VoxelSum;
                        }
                        else
                        {
                            sum += MyVoxelConstants.VOXEL_CELL_CONTENT_SUM_TOTAL;
                        }
                    }
                }
            }

            return sum;
        }

        #endregion

        #region SumFullCells

        public long SumFullCells()
        {
            long sum = 0;

            for (var x = 0; x < this._voxelContentCells.Length; x++)
            {
                for (var y = 0; y < this._voxelContentCells[x].Length; y++)
                {
                    for (var z = 0; z < this._voxelContentCells[x][y].Length; z++)
                    {
                        if (this._voxelContentCells[x][y][z] != null)
                        {
                            sum += this._voxelContentCells[x][y][z].VoxelFullCells;
                        }
                        else
                        {
                            sum += MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_TOTAL;
                        }
                    }
                }
            }

            return sum;
        }

        #endregion

        #region SumPartCells

        public long SumPartCells()
        {
            long sum = 0;

            for (var x = 0; x < this._voxelContentCells.Length; x++)
            {
                for (var y = 0; y < this._voxelContentCells[x].Length; y++)
                {
                    for (var z = 0; z < this._voxelContentCells[x][y].Length; z++)
                    {
                        if (this._voxelContentCells[x][y][z] != null)
                        {
                            sum += this._voxelContentCells[x][y][z].VoxelPartCells;
                        }
                        else
                        {
                            sum += MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_TOTAL;
                        }
                    }
                }
            }

            return sum;
        }

        #endregion

        #region CalculateMaterialCellAssets

        public void CalculateMaterialCellAssets(out IList<byte> materialAssetList, out Dictionary<byte, long> materialVoxelCells)
        {
            materialAssetList = new List<byte>();
            materialVoxelCells = new Dictionary<byte, long>();
            Vector3I cellCoord;

            for (cellCoord.X = 0; cellCoord.X < this._dataCellsCount.X; cellCoord.X++)
            {
                for (cellCoord.Y = 0; cellCoord.Y < this._dataCellsCount.Y; cellCoord.Y++)
                {
                    for (cellCoord.Z = 0; cellCoord.Z < this._dataCellsCount.Z; cellCoord.Z++)
                    {
                        var voxelCell = this._voxelContentCells[cellCoord.X][cellCoord.Y][cellCoord.Z];
                        var matCell = this._voxelMaterialCells[cellCoord.X][cellCoord.Y][cellCoord.Z];

                        if (voxelCell == null)
                        {
                            //  Voxel wasn't found in cell dictionary, so cell must be FULL
                            if (matCell.IsSingleMaterialForWholeCell)
                            {
                                for (var i = 0; i < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_TOTAL; i++)
                                {
                                    materialAssetList.Add(matCell.SingleMaterial);

                                    if (materialVoxelCells.ContainsKey(matCell.SingleMaterial))
                                        materialVoxelCells[matCell.SingleMaterial] += MyVoxelConstants.VOXEL_CONTENT_FULL;
                                    else
                                        materialVoxelCells.Add(matCell.SingleMaterial, MyVoxelConstants.VOXEL_CONTENT_FULL);
                                }
                            }
                            else
                            {
                                // A full cell, with mixed materials.
                                Vector3I voxelCoordInCell;
                                for (voxelCoordInCell.X = 0; voxelCoordInCell.X < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.X++)
                                {
                                    for (voxelCoordInCell.Y = 0; voxelCoordInCell.Y < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.Y++)
                                    {
                                        for (voxelCoordInCell.Z = 0; voxelCoordInCell.Z < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.Z++)
                                        {
                                            var material = matCell.GetMaterial(ref voxelCoordInCell);
                                            materialAssetList.Add(material);

                                            if (materialVoxelCells.ContainsKey(material))
                                                materialVoxelCells[material] += MyVoxelConstants.VOXEL_CONTENT_FULL;
                                            else
                                                materialVoxelCells.Add(material, MyVoxelConstants.VOXEL_CONTENT_FULL);
                                        }
                                    }
                                }
                            }
                        }
                        else if (voxelCell.CellType == MyVoxelCellType.MIXED)
                        {
                            if (matCell.IsSingleMaterialForWholeCell)
                            {
                                // a mixed cell, with one material.
                                Vector3I voxelCoordInCell;
                                for (voxelCoordInCell.X = 0; voxelCoordInCell.X < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.X++)
                                {
                                    for (voxelCoordInCell.Y = 0; voxelCoordInCell.Y < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.Y++)
                                    {
                                        for (voxelCoordInCell.Z = 0; voxelCoordInCell.Z < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.Z++)
                                        {
                                            var content = voxelCell.GetVoxelContent(ref voxelCoordInCell);

                                            if (content != MyVoxelConstants.VOXEL_CONTENT_EMPTY)
                                            {
                                                materialAssetList.Add(matCell.SingleMaterial);

                                                if (materialVoxelCells.ContainsKey(matCell.SingleMaterial))
                                                    materialVoxelCells[matCell.SingleMaterial] += content;
                                                else
                                                    materialVoxelCells.Add(matCell.SingleMaterial, content);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // A mixed cell, with mixed materials.
                                Vector3I voxelCoordInCell;
                                for (voxelCoordInCell.X = 0; voxelCoordInCell.X < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.X++)
                                {
                                    for (voxelCoordInCell.Y = 0; voxelCoordInCell.Y < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.Y++)
                                    {
                                        for (voxelCoordInCell.Z = 0; voxelCoordInCell.Z < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.Z++)
                                        {
                                            var content = voxelCell.GetVoxelContent(ref voxelCoordInCell);

                                            if (content != MyVoxelConstants.VOXEL_CONTENT_EMPTY)
                                            {
                                                var material = matCell.GetMaterial(ref voxelCoordInCell);
                                                materialAssetList.Add(material);

                                                if (materialVoxelCells.ContainsKey(material))
                                                    materialVoxelCells[material] += content;
                                                else
                                                    materialVoxelCells.Add(material, content);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region SetMaterialAssets

        public void SetMaterialAssets(IList<byte> materialsList)
        {
            var materialsIndex = 0;
            Vector3I cellCoord;
            for (cellCoord.X = 0; cellCoord.X < this._dataCellsCount.X; cellCoord.X++)
            {
                for (cellCoord.Y = 0; cellCoord.Y < this._dataCellsCount.Y; cellCoord.Y++)
                {
                    for (cellCoord.Z = 0; cellCoord.Z < this._dataCellsCount.Z; cellCoord.Z++)
                    {
                        var voxelCell = this._voxelContentCells[cellCoord.X][cellCoord.Y][cellCoord.Z];
                        var matCell = this._voxelMaterialCells[cellCoord.X][cellCoord.Y][cellCoord.Z];

                        // A mixed cell, with mixed materials.
                        Vector3I voxelCoordInCell;
                        for (voxelCoordInCell.X = 0; voxelCoordInCell.X < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.X++)
                        {
                            for (voxelCoordInCell.Y = 0; voxelCoordInCell.Y < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.Y++)
                            {
                                for (voxelCoordInCell.Z = 0; voxelCoordInCell.Z < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.Z++)
                                {
                                    if (voxelCell == null) // Cell is FULL.
                                    {
                                        matCell.SetMaterialAndIndestructibleContent(materialsList[materialsIndex++], 0xff, ref voxelCoordInCell);
                                    }
                                    else
                                    {
                                        // Cell is Mixed.
                                        var content = voxelCell.GetVoxelContent(ref voxelCoordInCell);

                                        if (content != MyVoxelConstants.VOXEL_CONTENT_EMPTY) // Only working with cells that aren't empty.
                                        {
                                            matCell.SetMaterialAndIndestructibleContent(materialsList[materialsIndex++], 0xff, ref voxelCoordInCell);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #endregion

        #region UpdateContentBounds

        public void UpdateContentBounds()
        {
            this._boundingContent = new BoundingBox(new Vector3I(Size.X, Size.Y, Size.Z), new Vector3I(0, 0, 0));
            Vector3I cellCoord;

            for (cellCoord.X = 0; cellCoord.X < this._dataCellsCount.X; cellCoord.X++)
            {
                for (cellCoord.Y = 0; cellCoord.Y < this._dataCellsCount.Y; cellCoord.Y++)
                {
                    for (cellCoord.Z = 0; cellCoord.Z < this._dataCellsCount.Z; cellCoord.Z++)
                    {
                        var voxelCell = this._voxelContentCells[cellCoord.X][cellCoord.Y][cellCoord.Z];

                        if (voxelCell == null)
                        {
                            this._boundingContent.Min = Vector3.Min(this._boundingContent.Min, new Vector3(cellCoord.X << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS, cellCoord.Y << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS, cellCoord.Z << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS));
                            this._boundingContent.Max = Vector3.Max(this._boundingContent.Max, new Vector3((cellCoord.X + 1 << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS) - 1, (cellCoord.Y + 1 << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS) - 1, (cellCoord.Z + 1 << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS) - 1));
                        }
                        //  Cell's are FULL by default, therefore we don't need to change them
                        else if (voxelCell.CellType == MyVoxelCellType.MIXED)
                        {

                            Vector3I voxelCoordInCell;
                            for (voxelCoordInCell.X = 0; voxelCoordInCell.X < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.X++)
                            {
                                for (voxelCoordInCell.Y = 0; voxelCoordInCell.Y < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.Y++)
                                {
                                    for (voxelCoordInCell.Z = 0; voxelCoordInCell.Z < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; voxelCoordInCell.Z++)
                                    {
                                        var content = voxelCell.GetVoxelContent(ref voxelCoordInCell);
                                        if (content > 0)
                                        {
                                            this._boundingContent.Min = Vector3.Min(this._boundingContent.Min, new Vector3((cellCoord.X << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS) + voxelCoordInCell.X, (cellCoord.Y << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS) + voxelCoordInCell.Y, (cellCoord.Z << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS) + voxelCoordInCell.Z));
                                            this._boundingContent.Max = Vector3.Max(this._boundingContent.Max, new Vector3((cellCoord.X << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS) + voxelCoordInCell.X, (cellCoord.Y << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS) + voxelCoordInCell.Y, (cellCoord.Z << MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS) + voxelCoordInCell.Z));
                                        }

                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region CountAssets

        public Dictionary<string, long> CountAssets(IList<byte> materialAssets)
        {
            var assetCount = new Dictionary<byte, long>();
            for (var i = 0; i < materialAssets.Count; i++)
            {
                if (assetCount.ContainsKey(materialAssets[i]))
                {
                    assetCount[materialAssets[i]]++;
                }
                else
                {
                    assetCount.Add(materialAssets[i], 1);
                }
            }

            var materialDefinitions = SpaceEngineersApi.GetMaterialList();
            var assetNameCount = new Dictionary<string, long>();

            foreach (var kvp in assetCount)
            {
                string name;

                if (kvp.Key >= materialDefinitions.Count)
                    name = materialDefinitions[this.VoxelMaterial].Name;
                else
                    name = materialDefinitions[kvp.Key].Name;

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

        public Dictionary<string, long> CountAssets(Dictionary<byte, long> assetCount)
        {
            var materialDefinitions = SpaceEngineersApi.GetMaterialList();
            var assetNameCount = new Dictionary<string, long>();

            foreach (var kvp in assetCount)
            {
                string name;

                if (kvp.Key >= materialDefinitions.Count)
                    name = materialDefinitions[this.VoxelMaterial].Name;
                else
                    name = materialDefinitions[kvp.Key].Name;

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

        #endregion

        #region helper methods

        //  Coordinates are relative to voxel map
        private byte GetVoxelContent(ref Vector3I voxelCoord)
        {
            var cellCoord = this.GetDataCellCoordinate(ref voxelCoord);
            var voxelCell = this.GetCell(ref cellCoord);

            if (voxelCell == null)
            {
                //  Voxel wasn't found in cell dictionary, therefore cell must be full
                return MyVoxelConstants.VOXEL_CONTENT_FULL;
            }
            else
            {
                var voxelCoordInCell = this.GetVoxelCoordinatesInDataCell(ref voxelCoord);
                var ret = voxelCell.GetVoxelContent(ref voxelCoordInCell);
                return ret;
            }
        }

        //  Return data cell to which belongs specified voxel (data cell)
        //  IMPORTANT: Input variable 'tempVoxelCoord' is 'ref' only for optimization. Never change its value in the method!!!
        private Vector3I GetDataCellCoordinate(ref Vector3I voxelCoord)
        {
            return new Vector3I(voxelCoord.X >> MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS, voxelCoord.Y >> MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS, voxelCoord.Z >> MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS);
        }

        //  Get cell. If not found (cell is full), null is returned.
        //  IMPORTANT: This method doesn't check if input cell coord0 is inside of the voxel map.
        //  IMPORTANT: This method has overloaded version that is sometimes needed too.
        private MyVoxelContentCell GetCell(ref Vector3I cellCoord)
        {
            if (!this.CheckVoxelCoord(ref cellCoord)) return null;
            return this._voxelContentCells[cellCoord.X][cellCoord.Y][cellCoord.Z];
        }

        //  Return voxel's coordinates relative to cell (in voxel space)
        //  IMPORTANT: Input variable 'tempVoxelCoord' is 'ref' only for optimization. Never change its value in the method!!!
        private Vector3I GetVoxelCoordinatesInDataCell(ref Vector3I voxelCoord)
        {
            return new Vector3I(voxelCoord.X & MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_MASK, voxelCoord.Y & MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_MASK, voxelCoord.Z & MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_MASK);
        }

        private bool CheckVoxelCoord(ref Vector3I cellCoord)
        {
            if (cellCoord.X >= 0 && cellCoord.Y >= 0 && cellCoord.Z >= 0)
            {
                if (cellCoord.X < this._voxelContentCells.Length &&
                    cellCoord.Y < this._voxelContentCells[cellCoord.X].Length &&
                    cellCoord.Z < this._voxelContentCells[cellCoord.X][cellCoord.Y].Length)
                {
                    return true;
                }
            }

            return false;
        }

        //  Checks if cell didn't change to FULL and if is, we set it to null
        private void CheckIfCellChangedToFull(MyVoxelContentCell voxelCell, ref Vector3I cellCoord)
        {
            if (voxelCell.CellType == MyVoxelCellType.FULL)
            {
                this._voxelContentCells[cellCoord.X][cellCoord.Y][cellCoord.Z] = null;
            }
        }

        //  Return true if this voxel is on voxel map border
        private bool IsVoxelAtBorder(ref Vector3I voxelCoord)
        {
            if (voxelCoord.X <= 0) return true;
            if (voxelCoord.Y <= 0) return true;
            if (voxelCoord.Z <= 0) return true;
            if (voxelCoord.X >= this._sizeMinusOne.X - 1) return true;
            if (voxelCoord.Y >= this._sizeMinusOne.Y - 1) return true;
            if (voxelCoord.Z >= this._sizeMinusOne.Z - 1) return true;
            return false;
        }

        //  Allocates cell from a buffer, store reference to dictionary and return reference to the cell
        //  Use it when changing cell type from full to empty or mixed.
        private MyVoxelContentCell AddCell(ref Vector3I cellCoord)
        {
            //  Adding or creating cell can be made only once
            Debug.Assert(this._voxelContentCells[cellCoord.X][cellCoord.Y][cellCoord.Z] == null);

            var ret = new MyVoxelContentCell();
            this._voxelContentCells[cellCoord.X][cellCoord.Y][cellCoord.Z] = ret;
            return ret;
        }

        #endregion
    }
}