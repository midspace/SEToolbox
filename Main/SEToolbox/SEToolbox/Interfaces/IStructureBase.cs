namespace SEToolbox.Interfaces
{
    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.Interop;
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

        Vector3 Center { get; set; }

        string SerializedEntity { get; set; }

        void UpdateGeneralFromEntityBase();

        bool IsBusy { get; set; }

        void InitializeAsync();

        float PositionX { get; set; }

        float PositionY { get; set; }

        float PositionZ { get; set; }

        void RecalcPosition(Vector3 playerPosition);
    }
}
