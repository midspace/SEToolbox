using System;
namespace SEToolbox.Interop.Asteroids
{
    // Determines offset of trace to the grid.
    public enum TraceType : byte
    {
        /// <summary>
        /// Will center the voxel on the origin, creating an odd number of voxels.
        /// </summary>
        /// <example>
        /// -1.5 to -0.5, centered on -1.0
        /// -0.5 to +0.5, centered on 0.0
        /// +0.5 to +1.5, centered on +1.0
        /// </example>
        Odd,

        /// <summary>
        /// Will center the voxel to the side of the origin, creating an even number of voxels.
        /// </summary>
        /// <example>
        /// -2.0 to -1.0, centered on -1.5
        /// -1.0 to 0.0, centered on -0.5
        /// 0.0 to +1.0, centered on +0.5
        /// +1.0 to +2.0, centered on +1.5
        /// </example>
        Even
    }

    /// <summary>
    /// Trace identifiers.
    /// </summary>
    [Flags]
    public enum TraceId : byte
    {
        Id1 = 1,
        Id2 = 2,
        Id3 = 4,
        Id4 = 8,
        Id5 = 16
    }

    /// <summary>
    /// User selectable number of Ray Traces to cast upon each voxel when converting.
    /// </summary>
    [Flags]
    public enum TraceCount : byte
    {
        Trace1 = TraceId.Id1,
        Trace2 = TraceId.Id2 + TraceId.Id3,
        Trace3 = TraceId.Id1 + TraceId.Id2 + TraceId.Id3,
        Trace4 = TraceId.Id2 + TraceId.Id3 + TraceId.Id4 + TraceId.Id5,
        Trace5 = TraceId.Id1 + TraceId.Id2 + TraceId.Id3 + TraceId.Id4 + TraceId.Id5
    }

    /// <summary>
    /// User selectable orientations of the Ray Traces.
    /// </summary>
    [Flags]
    public enum TraceDirection : byte
    {
        /// <summary>
        /// Trace along the X Axis only.
        /// </summary>
        X = 1,

        /// <summary>
        /// Trace along the Y Axis only.
        /// </summary>
        Y = 2,

        /// <summary>
        /// Trace along the Z Axis only.
        /// </summary>
        Z = 4,

        /// <summary>
        /// Trace along the X and Y Axis.
        /// </summary>
        XY = X + Y,

        /// <summary>
        /// Trace along the X and Z Axis.
        /// </summary>
        XZ = X + Z,

        /// <summary>
        /// Trace along the Y and Z Axis.
        /// </summary>
        YZ = Y + Z,

        /// <summary>
        /// Trace along all Axis.
        /// </summary>
        XYZ = X + Y + Z
    }
}
