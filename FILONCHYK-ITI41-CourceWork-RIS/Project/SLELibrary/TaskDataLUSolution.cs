using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLELibrary
{
    public class TaskDataLUSolution
    {

        public double[] Row { get; private set; }
        public double[] Y { get; private set; }
        public int Index { get; private set; }
        public double Sum { get; set; }

        public TaskDataLUSolution(double[] row, double[] y, int index)
        {
            Row = row;
            Y = y;
            Index = index;
        }

    }
}
