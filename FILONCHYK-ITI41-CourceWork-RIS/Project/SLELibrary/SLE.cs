using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using MatrixLibrary;

namespace SLELibrary
{
	public class SLE
	{
		[JsonProperty("system_matrix")]
		public double[][] A { get; private set; }
		[JsonProperty("free_members_vector")]
		public double[] B { get; private set; }
		[JsonIgnore]
		public double[]? X { get; private set; }
        [JsonIgnore]
        int Rows { get; set; }
        [JsonIgnore]
        int Columns { get; set; }
        [JsonProperty("roots")]
		public double[]? Result
		{
			get
			{
				if (X == null)
					return null;

				double[] result = new double[X.Length];

				for (int i = 0; i < X.Length; i++)
					result[i] = Math.Round(X[i], 2);
				
				return result;
			}
		}

		public SLE(double[][] a, double[] b)
		{
			A = a;
			B = b;
			X = new double[A[0].Length];
            Rows = A.Length;
            Columns = A[0].Length;
        }

        // Метод LU
        public void SolveLU()
        {
            int n = A.GetLength(0);
            double[][] L = new double[n][];
            double[][] U = new double[n][];

            for (int i = 0; i < n; i++)
            {
                L[i] = new double[n];
                U[i] = new double[n];
            }

            double[] Y = new double[n];

            // Инициализация L единичной матрицей
            for (int i = 0; i < n; i++)
                L[i][i] = 1.0;

            // LU-разложение
            for (int i = 0; i < n; i++)
            {
                for (int j = i; j < n; j++)
                {
                    // Вычисление элементов U
                    double sum = 0.0;
                    for (int k = 0; k < i; k++)
                        sum += L[i][k] * U[k][j];
                    U[i][j] = A[i][j] - sum;
                }

                for (int j = i + 1; j < n; j++)
                {
                    // Вычисление элементов L
                    double sum = 0.0;
                    for (int k = 0; k < i; k++)
                        sum += L[j][k] * U[k][i];
                    L[j][i] = (A[j][i] - sum) / U[i][i];
                }
            }

            // Решение LY = B (прямой ход)
            for (int i = 0; i < n; i++)
            {
                double sum = 0.0;
                for (int j = 0; j < i; j++)
                    sum += L[i][j] * Y[j];
                Y[i] = B[i] - sum;
            }

            // Решение UX = Y (обратный ход)
            for (int i = n - 1; i >= 0; i--)
            {
                double sum = 0.0;
                for (int j = i + 1; j < n; j++)
                    sum += U[i][j] * X[j];
                X[i] = (Y[i] - sum) / U[i][i];
            }
        }

        //Распараллеленный метод LU
        public void SolveLUParallel(List<Socket> workers)
        {
            double[][] a = Matrix.Multiply(Matrix.Transpose(A), A);
            double[] b = Matrix.Multiply(Matrix.Transpose(A), B);
            X = new double[Columns];
            Array.Copy(A, a, Rows);
            Array.Copy(B, b, Rows);
            double[][] L = new double[Rows][];
            double[][] U = new double[Rows][];

            for (int i = 0; i < Rows; i++)
            {
                L[i] = new double[Rows];
                U[i] = new double[Rows];
            }

            double[] Y = new double[Rows];

            // Инициализация L единичной матрицей
            for (int i = 0; i < Rows; i++)
                L[i][i] = 1;

            int elemsPerSolver;
            Task[] tasks;
            double[] rowPart = [];
            double[][] uPart;
            double sum;
            object obj = new object();

            // LU-разложение
            for (int i = 0; i < Rows; i++)
            {
                for (int j = i; j < Rows; j++)
                {
                    sum = 0;

                    if (workers.Count > i)
                    {
                        elemsPerSolver = 1;
                        tasks = new Task[i];
                    }
                    else
                    {
                        elemsPerSolver = i / workers.Count;
                        tasks = new Task[workers.Count];
                    }

                    for (int k = 0; k < tasks.Length; k++)
                    {
                        rowPart = GetPartRow(L[i][0..i], k, elemsPerSolver, tasks.Length);

                        if (rowPart.Length == 0)
                            continue;

                        uPart = GetPartSLE(U[0..i], k, elemsPerSolver, tasks.Length);
                        TaskDataLUDecomposition taskDataLUDecomposition = new TaskDataLUDecomposition(rowPart, uPart, j);
                        NetworkStream stream = new NetworkStream(workers[k]);
                        stream.Write(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(taskDataLUDecomposition)));

                        tasks[k] = Task.Run(() =>
                        {
                            TaskDataLUDecomposition? answer = Read<TaskDataLUDecomposition>(stream);
                            lock (obj)
                            {
                                sum += answer.Sum;
                            };
                        });
                    }

                    if (rowPart.Length != 0)
                        Task.WaitAll(tasks);

                    U[i][j] = a[i][j] - sum;
                }

                for (int j = i + 1; j < Rows; j++)
                {
                    sum = 0;

                    if (workers.Count > i)
                    {
                        elemsPerSolver = 1;
                        tasks = new Task[i];
                    }
                    else
                    {
                        elemsPerSolver = i / workers.Count;
                        tasks = new Task[workers.Count];
                    }

                    for (int k = 0; k < tasks.Length; k++)
                    {
                        rowPart = GetPartRow(L[j][0..i], k, elemsPerSolver, tasks.Length);

                        if (rowPart.Length == 0)
                            continue;

                        uPart = GetPartSLE(U[0..i], k, elemsPerSolver, tasks.Length);
                        TaskDataLUDecomposition taskDataLUDecomposition = new TaskDataLUDecomposition(rowPart, uPart, i);
                        NetworkStream stream = new NetworkStream(workers[k]);
                        stream.Write(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(taskDataLUDecomposition)));

                        tasks[k] = Task.Run(() =>
                        {
                            TaskDataLUDecomposition? answer = Read<TaskDataLUDecomposition>(stream);
                            lock (obj)
                            {
                                sum += answer.Sum;
                            };
                        });
                    }

                    if (rowPart.Length != 0)
                        Task.WaitAll(tasks);

                    L[j][i] = (a[j][i] - sum) / U[i][i];
                }
            }

            foreach (var worker in workers)
            {
                NetworkStream stream = new NetworkStream(worker);
                stream.Write(Encoding.UTF8.GetBytes("End"));
                stream.Flush();
            }

            double[] yPart;

            // Решение LY = B (прямой ход)
            for (int i = 0; i < Rows; i++)
            {
                sum = 0;

                if (workers.Count > i)
                {
                    elemsPerSolver = 1;
                    tasks = new Task[i];
                }
                else
                {
                    elemsPerSolver = i / workers.Count;
                    tasks = new Task[workers.Count];
                }

                for (int j = 0; j < tasks.Length; j++)
                {
                    rowPart = GetPartRow(L[i][0..i], j, elemsPerSolver, tasks.Length);

                    if (rowPart.Length == 0)
                        continue;

                    yPart = GetPartRow(Y[0..i], j, elemsPerSolver, tasks.Length);
                    TaskDataLUSolution taskDataLUSolution = new TaskDataLUSolution(rowPart, yPart, i);
                    NetworkStream stream = new NetworkStream(workers[j]);
                    stream.Write(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(taskDataLUSolution)));

                    tasks[j] = Task.Run(() =>
                    {
                        TaskDataLUSolution? answer = Read<TaskDataLUSolution>(stream);
                        lock (obj)
                        {
                            sum += answer.Sum;
                        };
                    });
                }

                if (rowPart.Length != 0)
                    Task.WaitAll(tasks);

                Y[i] = b[i] - sum;
            }

            foreach (var worker in workers)
            {
                NetworkStream stream = new NetworkStream(worker);
                stream.Write(Encoding.UTF8.GetBytes("End"));
            }

            // Решение UX = Y (обратный ход)
            for (int i = Rows - 1; i >= 0; i--)
            {
                sum = 0;
                for (int j = i + 1; j < Rows; j++)
                    sum += U[i][j] * X[j];
                X[i] = (Y[i] - sum) / U[i][i];
            }
        }

        public void GaussianMethod()
        {
            double[][] a = new double[A.Length][];
            double[] b = new double[A.Length];
            X = new double[A[0].Length];
            Array.Copy(A, a, A.Length);
            Array.Copy(B, b, A.Length);
            double factor;

            // Приведение расширенной матрицы к треугольному виду (прямой ход)
            for (int i = 0; i < A[0].Length - 1; i++)
            {
                for (int j = i + 1; j < A.Length; j++)
                {
                    factor = a[j][i] / a[i][i];

                    for (int k = 0; k < A[0].Length; k++)
                        a[j][k] -= a[i][k] * factor;

                    b[j] -= b[i] * factor;
                }
            }

            double sum;

            // Вычисление корней (обратный ход)
            for (int i = A.Length - 1; i >= 0; i--)
            {
                sum = 0;

                for (int j = i + 1; j < A[0].Length; j++)
                    sum += a[i][j] * X[j];

                X[i] = (b[i] - sum) / a[i][i];
            }
        }

        double[][] GetPartSLE(double[][] a, int number, int size, int length)
        {
            int leftBorder = number * size;
            int rightBorder = (number == length - 1) ? a.Length : leftBorder + size;
            double[][] aPart = a[leftBorder..rightBorder];

            return aPart;
        }

        double[] GetPartRow(double[] row, int number, int size, int length)
        {
            if (row.Length <= 1)
                return row;

            int leftBorder = number * size;
            int rightBorder = (number == length - 1) ? row.Length : leftBorder + size;
            double[] rowPart = row[leftBorder..rightBorder];

            return rowPart;
        }

        T? Read<T>(NetworkStream stream)
		{
			StringBuilder requestBuilder = new StringBuilder();
			byte[] buffer = new byte[1024];
			int length;

			do
			{
				length = stream.Read(buffer, 0, buffer.Length);
				requestBuilder.Append(Encoding.UTF8.GetString(buffer, 0, length));
			}
			while (stream.DataAvailable);

			T? answer = JsonConvert.DeserializeObject<T>(requestBuilder.ToString(), new JsonSerializerSettings { MetadataPropertyHandling = MetadataPropertyHandling.Ignore });

			return answer;
		}

		public static SLE GenerateSLE(int n)
		{
			Random random = new Random();
			double[][] a = new double[n][];
			double[] b = new double[n];
			double[] x = new double[n];

			for (int i = 0; i < n; i++)
				a[i] = new double[n];

			for (int i = 0; i < n; i++)
				for (int j = 0; j < n; j++)
					a[i][j] = Math.Round((2 * random.NextDouble() - 1) * 10, 2);

			for (int i = 0; i < n; i++)
				x[i] = Math.Round((2 * random.NextDouble() - 1) * 10, 2);

			b = Matrix.Multiply(a, x);

			Console.WriteLine("a:");
			foreach (double[] row in a)
			{
				Console.WriteLine();

				foreach (double elem in row)
					Console.Write(elem + " ");
			}

			Console.WriteLine();
			Console.WriteLine("b:");
			foreach (double elem in b)
				Console.Write(elem + " ");

			Console.WriteLine();
			Console.WriteLine("x:");
			foreach (double elem in x)
				Console.Write(elem + " ");

			return new SLE(a, b);
		}
	}
}