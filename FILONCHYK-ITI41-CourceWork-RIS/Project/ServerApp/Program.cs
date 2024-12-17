using Newtonsoft.Json;
using SLELibrary;
using SolverApp;

namespace ServerApp
{
	public class Program
	{
		const string IPADDRESS = "127.0.0.1";
		const int PORT = 8080;

		public static void Main()
		{
			Server server = new Server(IPADDRESS, PORT);
			server.Start();

           /* SLE sle = SLE.GenerateSLE(300);
            string json = JsonConvert.SerializeObject(new
            {
                system_matrix = sle.A,
                free_members_vector = sle.B

            }, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            });
            string filePath = "D:\\sle.json";
            File.WriteAllText(filePath, json);*/

            /*			Solver solver = new Solver(IPADDRESS, PORT);
                        solver.Start();*/

            Console.Read();
			server.Stop();
		}
	}
}


/*			SLE sle = SLE.GenerateSLE(50);
			string json = JsonConvert.SerializeObject(new
			{
				system_matrix = sle.A,
				free_members_vector = sle.B

			}, new JsonSerializerSettings
			{
				Formatting = Formatting.Indented
			});
			string filePath = "D:\\sle.json";
			File.WriteAllText(filePath, json);*/