using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json;
using SLELibrary;

namespace SolverApp
{
    public class Solver
    {
        public IPAddress IpAddress { get; set; }
        public int Port { get; set; }
        Socket Socket { get; set; }
        NetworkStream Stream { get; set; }

        public Solver(string ipAddress = "0.0.0.0", int port = 8080)
        {
            IpAddress = IPAddress.Parse(ipAddress);
            Port = port;
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Start()
        {
            Socket.Connect(new IPEndPoint(IpAddress, Port));
            Stream = new NetworkStream(Socket);
            Console.WriteLine($"Клиент подключен к серверу по адресу {IpAddress}:{Port}");

            byte[] message = Encoding.UTF8.GetBytes("Worker client");
            Stream.Write(message);

            GetTask();
        }

        void GetTask()
        {
            while (true)
            {
                StringBuilder requestBuilder = new StringBuilder();
                byte[] buffer = new byte[1024];
                int length;
                byte[] message;

                while (true)
                {
                    requestBuilder = new StringBuilder();
                    buffer = new byte[1024];

                    do
                    {
                        length = Stream.Read(buffer, 0, buffer.Length);
                        requestBuilder.Append(Encoding.UTF8.GetString(buffer, 0, length));
                    }
                    while (Stream.DataAvailable);

                    if (requestBuilder.ToString() == "End")
                    {
                        break;
                    }

                    try
                    {
                        TaskDataLUDecomposition taskDataLUDecomposition = JsonConvert.DeserializeObject<TaskDataLUDecomposition>(requestBuilder.ToString(), new JsonSerializerSettings { MetadataPropertyHandling = MetadataPropertyHandling.Ignore });
                        CalcSum(taskDataLUDecomposition);

                        message = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(taskDataLUDecomposition));
                        Stream.Write(message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }

                while (true)
                {
                    requestBuilder = new StringBuilder();
                    buffer = new byte[1024];

                    do
                    {
                        length = Stream.Read(buffer, 0, buffer.Length);
                        requestBuilder.Append(Encoding.UTF8.GetString(buffer, 0, length));
                    }
                    while (Stream.DataAvailable);

                    if (requestBuilder.ToString() == "End")
                    {
                        break;
                    }

                    try
                    {
                        TaskDataLUSolution taskDataLUSolution = JsonConvert.DeserializeObject<TaskDataLUSolution>(requestBuilder.ToString(), new JsonSerializerSettings { MetadataPropertyHandling = MetadataPropertyHandling.Ignore });
                        CalcSolution(taskDataLUSolution);

                        message = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(taskDataLUSolution));
                        Stream.Write(message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        public void CalcSum(TaskDataLUDecomposition taskData)
        {
            for (int i = 0; i < taskData.Row.Length; i++)
                taskData.Sum += taskData.Row[i] * taskData.U[i][taskData.Index];
        }

        public void CalcSolution(TaskDataLUSolution taskData)
        {
            for (int i = 0; i < taskData.Row.Length; i++)
                taskData.Sum += taskData.Row[i] * taskData.Y[i];
        }

        public void Stop()
        {
            Socket.Shutdown(SocketShutdown.Both);
            Socket.Close();
        }
    }
}