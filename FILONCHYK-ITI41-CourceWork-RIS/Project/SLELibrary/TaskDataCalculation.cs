using System.Runtime.InteropServices;

namespace SLELibrary
{
    public class TaskDataCalculation
    {
        public double[] Row { get; private set; }
        public double[] X { get; private set; }
        public int TasksNumber { get; private set; }
        public double Sum { get; set; }

        public TaskDataCalculation(double[] row, double[] x)
        {
            Row = row;
            X = x;
        }
    }
}