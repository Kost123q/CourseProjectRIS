namespace SLELibrary
{
    public class TaskDataNormalization
    {
        public double[][] A { get; private set; }
        public double[] B { get; private set; }
        public int Index { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }

        public TaskDataNormalization(double[][] a, double[] b, int index, int endIndex)
        {
            A = a;
            B = b;
            Index = index;
            StartIndex = index;
            EndIndex = endIndex;
        }
    }
}