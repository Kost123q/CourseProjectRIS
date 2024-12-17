using Newtonsoft.Json;
using SLELibrary;

namespace UnitTesting
{
	[TestClass]
	public class UnitTest
	{
		[TestMethod]
		public void TestLUMethod()
		{
			string json = File.ReadAllText("C:\\Users\\Asus\\Desktop\\KYRSACH\\FINALMESSI\\Project\\UnitTesting\\test_sle.json");
			SLE sle = JsonConvert.DeserializeObject<SLE>(json)!;
			sle.SolveLU();
			double[] solutionLUMethod = sle.X!;
			sle.GaussianMethod();
			double[] solutionGaussianMethod = sle.X!;

			for (int i = 0; i < solutionLUMethod.Length; i++)
				Assert.IsTrue(Math.Abs(solutionLUMethod[i] - solutionGaussianMethod[i]) < 0.1);
		}

        [TestMethod]
        public void TestGaussianMethod()
        {
            string json = File.ReadAllText("C:\\Users\\Asus\\Desktop\\KYRSACH\\FINALMESSI\\Project\\UnitTesting\\test_sle.json");
            SLE sle = JsonConvert.DeserializeObject<SLE>(json)!;
            sle.SolveLU();
            double[] solutionLUMethod = sle.X!;
            sle.GaussianMethod();
            double[] solutionGaussianMethod = sle.X!;

            for (int i = 0; i < solutionLUMethod.Length; i++)
                Assert.IsTrue(Math.Abs(solutionLUMethod[i] - solutionGaussianMethod[i]) < 0.1);
        }

    }
}