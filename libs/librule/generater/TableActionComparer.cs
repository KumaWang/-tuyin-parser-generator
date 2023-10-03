namespace librule.generater
{
    class TableActionComparer : IComparer<TableAction>
    {
        public static readonly TableActionComparer Instance = new TableActionComparer();

        public int Compare(TableAction x, TableAction y)
        {
            return y.Front - x.Front;
        }
    }
}
