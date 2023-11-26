using myNOC.Remootio.Services;
using myNOC.Remootio.Frames;
using myNOC.Remootio.Extensions;

namespace myNOC.Tests.Remootio.Services
{
	[TestClass]
	public class ApiCryptoTests
	{
		private ApiCrypto _apiCrypto = default!;
		private IServiceProvider _serviceProvider = default!;

		[TestInitialize]
		public void TestInit()
		{
			IServiceCollection services = new ServiceCollection();
			services.AddRemootio();
			_serviceProvider = services.BuildServiceProvider();

			_apiCrypto = new ApiCrypto(_serviceProvider, DeviceConfiguration.Get());
		}

		[TestMethod]
		public void Decrypt_EncryptedFrame_ReturnsValidQueryActionResponse()
		{
			//	Assemble
			var frameData = new FrameData("Hq9x+BpPdA+WV2ZMqPMa4Q==", "Ra1QHCP36Un6csyB7iY/SQRqpUMFWlTISWNjYo+zg0T3w46mXsn1uyOVA7PJTulPSvJlMuydwnnDWOmrPsI6PU4mK/+W2rET+yvQLLtsHmOXOTAMz4B3gWt7NYUi7QMID5oKrALLKScQqxkg6dy1r1q014S8DQ+qcIq3a6xiTvzH4LlFT2ba0TC6MYUzry6Q");
			var encryptedFrame = new Encrypted(frameData, "zch/Gk9WQ2Aucmrw5yxMOUgIgl5VqZW1lOORmXSJ5t8=");

			//	Act
			var result = _apiCrypto.Decrypt(encryptedFrame, "1f42LOneZEa3vzAnsMIPewmvHOJ6HobEPr05XEfvdTA=");

			//	Assert
			Assert.IsNotNull(result);
			Assert.IsInstanceOfType(result, typeof(Payload<myNOC.Remootio.Response.Action>));

			var response = (result as Payload<myNOC.Remootio.Response.Action>)!.Response;
			Assert.AreEqual(myNOC.Remootio.ActionTypes.QUERY, response.Type);
			Assert.AreEqual(286867086, response.Id);
			Assert.AreEqual(myNOC.Remootio.SensorStates.Closed, response.State);
			Assert.AreEqual(5564973, response.T100ms);
			Assert.IsFalse(response.RelayTriggered);
			Assert.AreEqual(string.Empty, response.ErrorCode);
		}
	}
}
