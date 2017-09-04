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
    class MyVoxelConstants
    {
        //// VRage.Voxels.MyVoxelConstants
        //public const byte VOXEL_ISO_LEVEL = 127;

        //  This is the version of actually supported voxel file
        public const int VOXEL_FILE_ACTUAL_VERSION = 2;

        public const int VOXEL_LEGACY_FILE_ACTUAL_VERSION = 1;

        //  Value of voxel's content if voxel is empty
        public const byte VOXEL_CONTENT_EMPTY = 0;

        //  Value of voxel's content if voxel is full
        public const byte VOXEL_CONTENT_FULL = 255;
        

        //  Initial sum of all voxels in a cell
        public const int VOXEL_CELL_CONTENT_SUM_TOTAL = VOXEL_DATA_CELL_SIZE_IN_VOXELS * VOXEL_DATA_CELL_SIZE_IN_VOXELS * VOXEL_DATA_CELL_SIZE_IN_VOXELS * VOXEL_CONTENT_FULL;

        //  Size of a voxel data cell in voxels (count of voxels in a voxel data cell) - in one direction
        //  Assume it's a power of two!
        public const int VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS = 3;
        public const int VOXEL_DATA_CELL_SIZE_IN_VOXELS = 1 << VOXEL_DATA_CELL_SIZE_IN_VOXELS_BITS;
        public const int VOXEL_DATA_CELL_SIZE_IN_VOXELS_MASK = VOXEL_DATA_CELL_SIZE_IN_VOXELS - 1;

        public const float VOXEL_DATA_CELL_SIZE_IN_VOXELS_FLOAT = VOXEL_DATA_CELL_SIZE_IN_VOXELS;


        //  How many data cells can fit in one render cell, in one direction
        public const int VOXEL_DATA_CELLS_IN_RENDER_CELL_SIZE = 8;

        //  Size of a voxel render cell in voxels (count of voxels in a voxel render cell)
        public const int VOXEL_RENDER_CELL_SIZE_IN_VOXELS = VOXEL_DATA_CELL_SIZE_IN_VOXELS * VOXEL_DATA_CELLS_IN_RENDER_CELL_SIZE;

        //  How many voxels can have one voxel map in one direction. This const isn't really needed, we just need some 
        //  offsets for voxel cell hash code calculations, so if you need to enlarge it, do so.
        public const int MAX_VOXEL_MAP_SIZE_IN_VOXELS = 10 * 1024;
        //public const double MAX_VOXEL_MAPS_DATA_CELL_COUNT = 1.5 * (512 / VOXEL_DATA_CELL_SIZE_IN_VOXELS * 512 / VOXEL_DATA_CELL_SIZE_IN_VOXELS * 512 / VOXEL_DATA_CELL_SIZE_IN_VOXELS);
        //public const Int64 MAX_VOXEL_MAP_ID = 1024;
        //public const int MAX_SORTED_VOXEL_MAPS_COUNT = 100;

        //  Total number of voxel in one data cell
        public const int VOXEL_DATA_CELL_SIZE_IN_VOXELS_TOTAL = VOXEL_DATA_CELL_SIZE_IN_VOXELS * VOXEL_DATA_CELL_SIZE_IN_VOXELS * VOXEL_DATA_CELL_SIZE_IN_VOXELS;

    }

    //  This enum tells us if cell is 100% empty, 100% full or mixed (some voxels are full, some empty, some are something between)
    internal enum MyVoxelCellType : byte
    {
        EMPTY = 0,
        FULL = 1,
        MIXED = 2
    }
}
