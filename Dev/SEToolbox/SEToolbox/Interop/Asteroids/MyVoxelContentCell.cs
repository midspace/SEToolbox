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
    using System.Diagnostics;
    using VRageMath;

    class MyVoxelContentCell
    {
        #region fields

        //  Reference to cell's content (array of voxel values). Only if cell type is MIXED.
        private MyVoxelContentCellContent _cellContent = null;

        //  Sums all voxel values. Default is summ of all full voxel in cell, so by subtracting we can switch cell from MIXED to EMPTY.
        long _voxelContentSum;
        long _voxelFullCells;
        long _voxelPartCells;
        long _voxelEmptyCells;


        #endregion

        #region ctor

        public MyVoxelContentCell()
        {
            //  Default cell is FULL
            this.CellType = MyVoxelCellType.FULL;

            //  Sums all voxel values. Default is sum of all full voxel in cell, so be subtracting we can switch cell from MIXED to EMPTY.
            this._voxelContentSum = MyVoxelConstants.VOXEL_CELL_CONTENT_SUM_TOTAL;
            this._voxelFullCells = MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_TOTAL;
            this._voxelPartCells = 0;
            this._voxelEmptyCells = 0;
        }
        #endregion

        #region properties

        //  Cell type. Default is FULL.
        public MyVoxelCellType CellType { get; private set; }

        public long VoxelSum
        {
            get { return this._voxelContentSum; }
        }

        public long VoxelFullCells
        {
            get { return this._voxelFullCells; }
        }

        public long VoxelPartCells
        {
            get { return this._voxelPartCells; }
        }

        public long VoxelEmptyCells
        {
            get { return this._voxelEmptyCells; }
        }

        #endregion

        #region methods

        public void SetToEmpty()
        {
            this.CellType = MyVoxelCellType.EMPTY;
            this._voxelContentSum = 0;
            this._voxelFullCells = 0;
            this._voxelPartCells = 0;
            this._voxelEmptyCells = MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_TOTAL;
            this.CheckCellType();
        }

        public void SetAllVoxelContents(byte[] buffer, out BoundingBoxD bounding)
        {
            // quantize the buffer and compute sum
            this._voxelContentSum = 0;
            for (var i = 0; i < buffer.Length; i++)
            {
                buffer[i] = MyVoxelContentCellContent.QuantizedValue(buffer[i]);
                this._voxelContentSum += buffer[i];
            }

            bounding = new BoundingBoxD(Vector3I.MaxValue, Vector3I.MinValue);

            // mixed-->empty/full: deallocate
            // empty/full-->mixed: allocate
            // mixed: fill with values from buffer
            if (_voxelContentSum == 0)
            {
                if (this.CellType == MyVoxelCellType.MIXED) this.Deallocate();
                this.CellType = MyVoxelCellType.EMPTY;
            }
            else if (_voxelContentSum == MyVoxelConstants.VOXEL_CELL_CONTENT_SUM_TOTAL)
            {
                if (this.CellType == MyVoxelCellType.MIXED) this.Deallocate();
                this.CellType = MyVoxelCellType.FULL;
            }
            else
            {
                if (this.CellType == MyVoxelCellType.FULL || this.CellType == MyVoxelCellType.EMPTY)
                {
                    this._cellContent = new MyVoxelContentCellContent();
                }
                if (this._cellContent != null)
                {
                    this._cellContent.SetAddVoxelContents(buffer, ref bounding);
                }
                this.CellType = MyVoxelCellType.MIXED;
            }
        }

        //  Coordinates are relative to voxel cell
        //  IMPORTANT: Input variable 'voxelCoordInCell' is 'ref' only for optimization. Never change its value in the method!!!
        public byte GetVoxelContent(ref Vector3I voxelCoordInCell)
        {
            if (this.CellType == MyVoxelCellType.EMPTY)
            {
                //  Cell is empty, therefore voxel must be empty too.
                return MyVoxelConstants.VOXEL_CONTENT_EMPTY;
            }
            else if (this.CellType == MyVoxelCellType.FULL)
            {
                //  Cell is full, therefore voxel must be full too.
                return MyVoxelConstants.VOXEL_CONTENT_FULL;
            }
            else
            {
                //  If cell is mixed, get voxel's content from the cell's content.
                //  Content was allocated before, we don't need to do it now (or even check it).
                if (this._cellContent != null)
                {
                    return this._cellContent.GetVoxelContent(ref voxelCoordInCell);
                }

                return 0;
            }
        }

        //  This method helps us to maintain correct cell type even after removing or adding voxels from cell
        //  If all voxels were removed from this cell, we change its type to from MIXED to EMPTY.
        //  If voxels were added, we change its type to from EMPTY to MIXED.
        //  If voxels were added to full, we change its type to FULL.
        void CheckCellType()
        {
            //  Voxel cell content sum isn't in allowed range. Probably increased or descreased too much.
            Debug.Assert((this._voxelContentSum >= 0) && (this._voxelContentSum <= MyVoxelConstants.VOXEL_CELL_CONTENT_SUM_TOTAL));

            if (this._voxelContentSum == 0)
            {
                this.CellType = MyVoxelCellType.EMPTY;
                this._voxelFullCells = 0;
                this._voxelPartCells = 0;
                this._voxelEmptyCells = MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_TOTAL;
            }
            else if (_voxelContentSum == MyVoxelConstants.VOXEL_CELL_CONTENT_SUM_TOTAL)
            {
                this.CellType = MyVoxelCellType.FULL;
                this._voxelFullCells = MyVoxelConstants.VOXEL_DATA_CELL_SIZE_IN_VOXELS_TOTAL;
                this._voxelPartCells = 0;
                this._voxelEmptyCells = 0;
            }
            else
            {
                this.CellType = MyVoxelCellType.MIXED;
            }

            //  If cell changed from MIXED to EMPTY or FULL, we will release it's cell content because it's not needed any more
            if (this.CellType == MyVoxelCellType.EMPTY || this.CellType == MyVoxelCellType.FULL)
            {
                this.Deallocate();
            }
        }

        public void Deallocate()
        {
            this._cellContent = null;
        }

        //  Voxel at specified coordinate 'x, y, z' sets to value 'content'. Coordinates are relative to voxel cell
        //  IMPORTANT: Do not call this method directly! Always call it through MyVoxelMap.SetVoxelContent()
        public void SetVoxelContent(byte content, ref Vector3I voxelCoordInCell)
        {
            content = MyVoxelContentCellContent.QuantizedValue(content);

            if (this.CellType == MyVoxelCellType.FULL)
            {
                if (content == MyVoxelConstants.VOXEL_CONTENT_FULL)
                {
                    //  Nothing is changing
                    return;
                }
                else
                {
                    this._voxelContentSum -= (MyVoxelConstants.VOXEL_CONTENT_FULL - content);
                    this.CheckCellType();

                    //  If this cell is mixed, we change voxel's value in the cell content array, but first allocate the array
                    if (this.CellType == MyVoxelCellType.MIXED)
                    {
                        this._cellContent = new MyVoxelContentCellContent(); // MyVoxelContentCellContents.Allocate();
                        if (this._cellContent != null)
                        {
                            this._cellContent.Reset(MyVoxelConstants.VOXEL_CONTENT_FULL);
                            this._cellContent.SetVoxelContent(content, ref voxelCoordInCell);
                        }
                    }
                }
            }
            else if (this.CellType == MyVoxelCellType.EMPTY)
            {
                if (content == MyVoxelConstants.VOXEL_CONTENT_EMPTY)
                {
                    //  Nothing is changing
                    return;
                }
                else
                {
                    this._voxelContentSum += content;
                    this.CheckCellType();

                    //  If this cell is mixed, we change voxel's value in the cell content array, but first allocate the array
                    if (this.CellType == MyVoxelCellType.MIXED)
                    {
                        this._cellContent = new MyVoxelContentCellContent(); //MyVoxelContentCellContents.Allocate();
                        if (this._cellContent != null)
                        {
                            this._cellContent.Reset(MyVoxelConstants.VOXEL_CONTENT_EMPTY);
                            this._cellContent.SetVoxelContent(content, ref voxelCoordInCell);
                        }
                    }
                }
            }
            else if (this.CellType == MyVoxelCellType.MIXED)
            {
                if (this._cellContent == null)
                {
                    return;
                }
                //  Check for previous content value not only for optimisation, but because we need to know how much it changed
                //  for calculating whole cell content summary.
                var previousContent = this._cellContent.GetVoxelContent(ref voxelCoordInCell);

                if (previousContent == content)
                {
                    //  New value is same as current, so nothing needs to be changed
                    return;
                }

                this._voxelContentSum -= previousContent - content;
                this.CheckCellType();

                //  If this cell is still mixed, we change voxel's value in the cell content array
                if (this.CellType == MyVoxelCellType.MIXED)
                {
                    this._cellContent.SetVoxelContent(content, ref voxelCoordInCell);
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}
