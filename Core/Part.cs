namespace ProductionSimulation.Core
{
    public enum PartState { AtMachine, AtOperator, AtLoader, Delivered }

    public class Part
    {
        public int Id { get; }
        public PartState State { get; set; } = PartState.AtMachine;
        public bool IsDefective { get; set; }

        public Part(int id) => Id = id;
    }
}