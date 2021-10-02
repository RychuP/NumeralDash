namespace NumeralDash.World
{
    interface IConnectable
    {
        public int ID { get; init; }

        public bool HasConnectionTo(Room other);

        public string GetInfo();
    }
}
