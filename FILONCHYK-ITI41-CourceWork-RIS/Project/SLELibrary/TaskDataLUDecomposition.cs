namespace SLELibrary
{
    public class TaskDataLUDecomposition
    {
        public double[] Row { get; private set; }
        public double[][] U { get; private set; }
        public int Index { get; private set; }
        public double Sum { get; set; }

        public TaskDataLUDecomposition(double[] row, double[][] u, int index)
        {
            Row = row;
            U = u;
            Index = index;
        }
    }
}
