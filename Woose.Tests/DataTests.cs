using System.Data;
using Woose.Data;

namespace Woose.Tests
{
    public class DataTests
    {
        protected string connStr { get; set; } = string.Empty;

        [SetUp]
        public void Setup()
        {
            this.connStr = "Data Source=localhost;Initial Catalog=Test;User ID=tester;Password=1q2w3e4r;Application Name=Test;TrustServerCertificate=true";
        }

        [Test, Order(2)]
        public async Task ContextAndQueryHelper_TestCase_Create()
        {
            IContext context = new DbContext(this.connStr);
            var result = new GlobalCode();

            using (var db = context.getConnection())
            using (var cmd = db.CreateCommand())
            {
                cmd.On<GlobalCode>().Select(1).Where("KeyCode", "Active").Set();
                result = cmd.ExecuteEntity<GlobalCode>();
            }

            Assert.IsNotNull(result);
            Assert.That(result.KeyName, Is.EqualTo("정상"));
        }

        [Test, Order(1)]
        public void ContextAndQueryHelper_TestCase_Insert()
        {
            IContext context = new DbContext(this.connStr);
            var rst = new ExecuteResult();

            GlobalCode paramData = new GlobalCode();
            paramData.MajorName = "회원";
            paramData.MajorCode = "Member";
            paramData.MinorName = "상태";
            paramData.MinorCode = "Status";
            paramData.KeyName = "정상";
            paramData.KeyCode = "Active";


            using (var db = context.getConnection())
            using (var cmd = db.CreateCommand())
            {
                cmd.On<GlobalCode>(paramData).Insert().Try().Set();
                rst = cmd.ExecuteResult();
            }

            Assert.IsTrue(rst!.IsSuccess);
        }

        [Test, Order(3)]
        public void ContextAndQueryHelper_TestCase_Update()
        {
            IContext context = new DbContext(this.connStr);
            var rst = new ExecuteResult();

            GlobalCode paramData = new GlobalCode();
            paramData.MajorName = "회원";
            paramData.MajorCode = "Member";
            paramData.MinorName = "상태";
            paramData.MinorCode = "Status";
            paramData.KeyName = "정상";
            paramData.KeyCode = "Absolute";

            using (var db = context.getConnection())
            using (var cmd = db.CreateCommand())
            {
                cmd.On<GlobalCode>(paramData).Update().Try().Where("KeyCode", "Active").Set();
                rst = cmd.ExecuteResult();
            }

            Assert.IsTrue(rst!.IsSuccess);
        }

        [Test, Order(4)]
        public void ContextAndQueryHelper_TestCase_Count()
        {
            IContext context = new DbContext(this.connStr);
            int cnt = 0;

            using (var db = context.getConnection())
            using (var cmd = db.CreateCommand())
            {
                cmd.On<GlobalCode>().Count().Where("MajorCode", "Member").Set();
                cnt = Convert.ToInt32(cmd.ExecuteScalar());
            }

            Assert.That(cnt, Is.EqualTo(1));
        }

        [Test, Order(5)]
        public void ContextAndQueryHelper_TestCase_Paging()
        {
            IContext context = new DbContext(this.connStr);
            var rst = new List<GlobalCode>();

            using (var db = context.getConnection())
            using (var cmd = db.CreateCommand())
            {
                cmd.On<GlobalCode>().Paging(10, 1).Where("MajorCode", "Member").Desc("CodeIDX").Set();
                rst = cmd.ExecuteEntities<GlobalCode>();
            }

            Assert.That(rst.Count(), Is.EqualTo(1));
        }

        [Test, Order(6)]
        public void ContextAndQueryHelper_TestCase_Delete()
        {
            IContext context = new DbContext(this.connStr);
            var rst = new ExecuteResult();

            using (var db = context.getConnection())
            using (var cmd = db.CreateCommand())
            {
                cmd.On<GlobalCode>().Delete().Try().Where("MajorCode", "Member").Set();
                rst = cmd.ExecuteResult();
            }

            Assert.IsTrue(rst!.IsSuccess);
        }


        [Test, Order(7)]
        public void ContextAndQueryHelper_TestCase_SP()
        {
            IContext context = new DbContext(this.connStr);
            var rst = new ExecuteResult();

            using (var db = context.getConnection())
            using (var cmd = db.CreateCommand())
            {
                cmd.On("spname").Set();
                rst = cmd.ExecuteResult();
                
            }

            Assert.IsTrue(rst!.IsSuccess);
        }

    }
}