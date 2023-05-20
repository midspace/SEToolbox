namespace SEToolbox.Interfaces
{
    using SEToolbox.Interop;
    using VRage;
    using VRage.ObjectBuilders;
    using VRageMath;

    public interface IStructureBase
    {
        MyObjectBuilder_EntityBase EntityBase { get; set; }

        long EntityId { get; set; }

        MyPositionAndOrientation? PositionAndOrientation { get; set; }

        ClassType ClassType { get; set; }

        string DisplayName { get; set; }

        string Description { get; set; }

        double PlayerDistance { get; set; }

        double Mass { get; set; }

        int BlockCount { get; set; }

        Vector3D Center { get; set; }

        BoundingBoxD WorldAABB { get; set; }

        string SerializedEntity { get; set; }

        void UpdateGeneralFromEntityBase();

        bool IsBusy { get; set; }

        bool IsValid { get; set; }

        void InitializeAsync();
        
        void CancelAsync();

        double PositionX { get; set; }

        double PositionY { get; set; }

        double PositionZ { get; set; }

        double LinearVelocity { get; set; }

        void RecalcPosition(Vector3D playerPosition);
    }
}
