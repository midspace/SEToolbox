namespace SEToolbox.Interop
{
    using VRage.ObjectBuilders;

    public class BlueprintRequirement
    {
        public decimal Amount { get; set; }

        public SerializableDefinitionId Id { get; set; }

        public string SubtypeId { get; set; }

        public string TypeId { get; set; }
    }
}
