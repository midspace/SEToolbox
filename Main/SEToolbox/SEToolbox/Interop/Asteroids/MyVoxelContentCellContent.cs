﻿//===================================================================================
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
    using VRageMath;

    class MyVoxelContentCellContent
    {
        #region fields

        const int xStep = MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS * MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS;
        const int yStep = MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS;
        const int zStep = 1;
        const int TOTAL_VOXEL_COUNT = MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS * MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS * MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS;

        private const int QUANTIZATION_BITS = 3;                   // number of bits kept

        const int THROWAWAY_BITS = 8 - QUANTIZATION_BITS;  // number of bits thrown away


        private readonly byte[] _packed;

        static readonly uint[] Bitmask = {
            ~((255u >> THROWAWAY_BITS) << 0), ~((255u >> THROWAWAY_BITS) << 1), ~((255u >> THROWAWAY_BITS) << 2), ~((255u >> THROWAWAY_BITS) << 3),
            ~((255u >> THROWAWAY_BITS) << 4), ~((255u >> THROWAWAY_BITS) << 5), ~((255u >> THROWAWAY_BITS) << 6), ~((255u >> THROWAWAY_BITS) << 7),
        };

        // Values quantized to (8 - QUANTIZATION_BITS) with correct smearing of significant bits.
        // Example: 3 significant bits
        //   000xxxxx -> 0000000   001xxxxx -> 0010010   010xxxxx -> 0100100   011xxxxx -> 0110110
        //   100xxxxx -> 1001001   101xxxxx -> 1011011   110xxxxx -> 1101101   111xxxxx -> 1111111
        // It's important to return 255 for max value and 0 for min value.
        static readonly byte[] SmearBits;

        #endregion

        #region ctor

        static MyVoxelContentCellContent()
        {
            SmearBits = new byte[1 << QUANTIZATION_BITS];
            for (uint i = 0; i < 1 << QUANTIZATION_BITS; i++)
            {
                uint value = i << THROWAWAY_BITS;

                // smear bits
                value = value + (value >> QUANTIZATION_BITS);
                if (QUANTIZATION_BITS < 4)
                {
                    value = value + (value >> QUANTIZATION_BITS * 2);
                    if (QUANTIZATION_BITS < 2)
                        value = value + (value >> QUANTIZATION_BITS * 4);
                }

                SmearBits[i] = (byte)value;
            }
        }

        public MyVoxelContentCellContent()
        {
            // round number of bytes up, add 1 for quantizations with bits split into different bytes
            this._packed = new byte[(TOTAL_VOXEL_COUNT * QUANTIZATION_BITS + 7) / 8 + 1];
            this.Reset(MyVoxelConstants.VOXEL_CONTENT_FULL);
        }

        #endregion

        #region methods

        public static byte QuantizedValue(byte content)
        {
            unchecked
            {
                return SmearBits[content >> THROWAWAY_BITS];
            }
        }

        //  Reset all voxels in this content to specified value. Original version was reseting to full only, but now we need reseting to empty too.
        //  Old: By default all voxels are full
        //      This method must be called in constructor and then everytime we allocate this content after it was deallocated before.
        //      So, when this content is used first time, it's freshly reseted by constructor. If later we deallocate it and then
        //      more later allocate again, we have to reset it so it contains only full voxels again.
        public void Reset(byte resetToContent)
        {
            if (resetToContent == MyVoxelConstants.VOXEL_CONTENT_FULL)
                for (var i = 0; i < this._packed.Length; i++)
                    this._packed[i] = 0xFF;
            else if (resetToContent == MyVoxelConstants.VOXEL_CONTENT_EMPTY)
                Array.Clear(this._packed, 0, this._packed.Length);
            else
            {
                Vector3I position;
                for (position.X = 0; position.X < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; position.X++)
                    for (position.Y = 0; position.Y < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; position.Y++)
                        for (position.Z = 0; position.Z < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS; position.Z++)
                            SetVoxelContent(resetToContent, ref position);
            }
        }

        public void SetAddVoxelContents(byte[] contents)
        {
            unchecked
            {
                // for QUANTIZATION_BITS == 8 we can just do System.Buffer.BlockCopy
                Array.Clear(this._packed, 0, this._packed.Length);

                for (int bitadr = 0, adr = 0; bitadr < MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_TOTAL * QUANTIZATION_BITS; bitadr += QUANTIZATION_BITS, adr++)
                {
                    var byteadr = bitadr >> 3;
                    var c = ((uint)contents[adr] >> THROWAWAY_BITS) << (bitadr & 7);
                    this._packed[byteadr] |= (byte)c;
                    this._packed[byteadr + 1] |= (byte)(c >> 8);  // this needs to be done only for QUANTIZATION_BITS == 1,2,4,8
                }
            }
        }

        //  Voxel at specified coordinate 'x, y, z' sets to value 'content'
        //  Coordinates are relative to voxel cell
        public void SetVoxelContent(byte content, ref Vector3I voxelCoordInCell)
        {
            if (!CheckVoxelCoord(ref voxelCoordInCell)) return;

            unchecked
            {
                // for QUANTIZATION_BITS == 8: m_packed[voxelCoordInCell.X * xStep + voxelCoordInCell.Y * yStep + voxelCoordInCell.Z * zStep] = content;
                var bitadr = (voxelCoordInCell.X * xStep + voxelCoordInCell.Y * yStep + voxelCoordInCell.Z * zStep) * QUANTIZATION_BITS;
                var bit = bitadr & 7;
                var byteadr = bitadr >> 3;
                var c = ((uint)content >> THROWAWAY_BITS) << bit;
                this._packed[byteadr] = (byte)(this._packed[byteadr] & Bitmask[bit] | c);
                this._packed[byteadr + 1] = (byte)(this._packed[byteadr + 1] & Bitmask[bit] >> 8 | c >> 8);   // this needs to be done only for QUANTIZATION_BITS == 1,2,4,8
            }
        }

        //  Coordinates are relative to voxel cell
        //  IMPORTANT: Input variable 'voxelCoordInCell' is 'ref' only for optimization. Never change its value in the method!!!
        public byte GetVoxelContent(ref Vector3I voxelCoordInCell)
        {
            if (!CheckVoxelCoord(ref voxelCoordInCell)) return 0x00;

            unchecked
            {
                // for QUANTIZATION_BITS == 8: return m_packed[voxelCoordInCell.X * xStep + voxelCoordInCell.Y * yStep + voxelCoordInCell.Z * zStep];
                var bitadr = (voxelCoordInCell.X * xStep + voxelCoordInCell.Y * yStep + voxelCoordInCell.Z * zStep) * QUANTIZATION_BITS;
                var byteadr = bitadr >> 3;
                var value = this._packed[byteadr] + ((uint)this._packed[byteadr + 1] << 8);  // QUANTIZATION_BITS == 1,2,4,8: value = (uint)m_packed[bitadr >> 3];
                return SmearBits[(value >> (bitadr & 7)) & (255 >> THROWAWAY_BITS)];
            }
        }

        private static bool CheckVoxelCoord(ref Vector3I cellCoord)
        {
            return (uint)(cellCoord.X | cellCoord.Y | cellCoord.Z) < (uint)MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS;  // VOXEL_DATA_CELL_SIZE_IN_VOXELS must be a power of 2
        }

        #endregion
    }
}
