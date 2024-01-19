using System.Data;
using Woose.V8;

namespace Woose.Tests
{
    public class V8DataTests
    {
        protected string connStr { get; set; } = string.Empty;

        [SetUp]
        public void Setup()
        {
            this.connStr = "Data Source=localhost;Initial Catalog=Test;User ID=tester;Password=1q2w3e4r;Application Name=Test;TrustServerCertificate=true";
        }

        [Test]
        public void Select_Base_Query()
        {
            IContext context = new DbContext(this.connStr); //의존성 주입
            var result = new List<GlobalCode>();


            Assert.IsNotNull(result);
            Assert.That(result.Count, Is.EqualTo(1));
        }


    }
}