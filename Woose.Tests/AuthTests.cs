using System.Data;
using Woose.API;
using Woose.Data;

namespace Woose.Tests
{
    public class AuthTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void CreateJWT()
        {
            CryptoHandler crypto = new CryptoHandler();
            JwtToken? token = crypto.GenerateTokens("userid", "webmaster", "test-server-token");

            Assert.IsNotNull(token);
        }

    }
}