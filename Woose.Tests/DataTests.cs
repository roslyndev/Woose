using Woose.Data;

namespace Woose.Tests
{
    public class InputCode
    {
        public long CodeIDX { get; set; } = -1;
        public string MajorCode { get; set; } = string.Empty;
        public string MinorCode { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string MajorName { get; set; } = string.Empty;
        public string MinorName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class DataTests
    {
        protected string connStr { get; set; } = string.Empty;

        [SetUp]
        public void Setup()
        {
            this.connStr = "Data Source=localhost;Initial Catalog=Test;User ID=tester;Password=1q2w3e4r;Application Name=Test;TrustServerCertificate=true";
        }

        [Test, Order(2)]
        public void ContextAndQueryHelper_TestCase_Create()
        {
            IContext context = new DbContext(this.connStr);
            List<GlobalCode> codeList = new List<GlobalCode>();

            using (var db = context.getConnection())
            using (var handler = new SqlDbOperater(db))
            {
                handler.Command.CreateQuery<GlobalCode>(true).Select()
                               .Where("MajorCode='Member'")
                               .Where("MinorCode='Status'")
                               .Set();
                codeList = handler.Command.ExecuteEntities<GlobalCode>();
            }

            Assert.IsNotNull(codeList);
            Assert.AreEqual(codeList.Count, 1);
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
            paramData.Name = "정상";
            paramData.Code = "Active";

            using (var db = context.getConnection())
            using (var handler = new SqlDbOperater(db))
            {
                handler.Command.CreateQuery<GlobalCode>(true)
                               .InsertAll(paramData)
                               .NotExists()
                               .Where("MajorCode='Member'")
                               .Where("MinorCode='Status'")
                               .ToResult<ExecuteResult>();
                rst = handler.Command.ExecuteResult();
            }

            Assert.AreEqual(rst.IsSuccess, true);
        }

        [Test, Order(3)]
        public void ContextAndQueryHelper_TestCase_Update()
        {
            IContext context = new DbContext(this.connStr);
            var rst = new ExecuteResult();

            using (var db = context.getConnection())
            using (var handler = new SqlDbOperater(db))
            {
                handler.Command.CreateQuery<GlobalCode>().Select(1).Where("MajorCode='Member'").Where("MinorCode='Status'").Set();
                GlobalCode target = handler.Command.ExecuteEntity<GlobalCode>();
                target.Code = "Absolute";

                handler.Command.CreateQuery<GlobalCode>(true)
                               .UpdateAll(target)
                               .Where("MajorCode='Member'")
                               .Where("MinorCode='Status'")
                               .ToResult<ExecuteResult>();
                rst = handler.Command.ExecuteResult();
            }

            Assert.AreEqual(rst.IsSuccess, true);
        }

        [Test, Order(4)]
        public void ContextAndQueryHelper_TestCase_Delete()
        {
            IContext context = new DbContext(this.connStr);
            int num = 0;

            using (var db = context.getConnection())
            using (var handler = new SqlDbOperater(db))
            {
                handler.Command.CreateQuery<GlobalCode>(true)
                               .Delete()
                               .Where("MajorCode='Member'")
                               .Where("MinorCode='Status'")
                               .Set();
                num = handler.Command.ExecuteNonQuery();
            }

            Assert.AreEqual(num, 1);
        }
    }
}