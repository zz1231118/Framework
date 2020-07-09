namespace Framework.Data.Entry
{
    internal class RowEntry
    {
        public RowEntry(object row, EntityState state)
        {
            Row = row;
            State = state;
        }

        public object Row { get; }

        public EntityState State { get; set; }

        public void AcceptChanges()
        {
            State = EntityState.Unchanged;
        }
    }
}
