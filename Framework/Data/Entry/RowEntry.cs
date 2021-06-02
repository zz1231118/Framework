namespace Framework.Data.Entry
{
    public class RowEntry
    {
        internal RowEntry(object value, EntityState state)
        {
            Value = value;
            State = state;
        }

        public object Value { get; }

        public EntityState State { get; internal set; }

        public object? Tag { get; set; }

        internal void AcceptChanges()
        {
            State = EntityState.Unchanged;
        }
    }
}
