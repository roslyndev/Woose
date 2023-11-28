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
        public void ContextAndQueryHelper_TestCase_Create()
        {
            IContext context = new DbContext(this.connStr);
            List<GlobalCode> codeList = new List<GlobalCode>();

            using (var db = context.getConnection())
            using (var handler = new SqlDbOperater(db))
            {
                codeList = Entity<GlobalCode>.Query
                                            .Select(1)
                                            .Where(x => x.MajorCode == "Member")
                                            .And(x => x.MinorCode == "Status")
                                            .Execute(handler.Command)
                                            .ToList();
            }

            Assert.IsNotNull(codeList);
            Assert.That(codeList.Count, Is.EqualTo(1));
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
            using (var handler = new SqlDbOperater(db))
            {
                rst = Entity<GlobalCode>.Query
                                        .Insert(paramData)
                                        .SetResult<ExecuteResult>()
                                        .Execute(handler.Command)
                                        .ToResult() as ExecuteResult;
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
            using (var handler = new SqlDbOperater(db))
            {
                rst = Entity<GlobalCode>.Query
                                        .Update(paramData)
                                        .Where(x => x.MajorCode == "Member")
                                        .And(x => x.MinorCode == "Status")
                                        .SetResult<ExecuteResult>()
                                        .Execute(handler.Command)
                                        .ToResult() as ExecuteResult;
            }

            Assert.IsTrue(rst!.IsSuccess);
        }

        [Test, Order(4)]
        public void ContextAndQueryHelper_TestCase_Count()
        {
            IContext context = new DbContext(this.connStr);
            int cnt = 0;

            using (var db = context.getConnection())
            using (var handler = new SqlDbOperater(db))
            {
                cnt = Entity<GlobalCode>.Query
                                        .Count()
                                        .Where(x => x.MajorCode == "Member")
                                        .And(x => x.MinorCode == "Status")
                                        .SetResult<ExecuteResult>()
                                        .Execute(handler.Command)
                                        .ToCount();
            }

            Assert.That(cnt, Is.EqualTo(1));
        }

        [Test, Order(5)]
        public void ContextAndQueryHelper_TestCase_Paging()
        {
            IContext context = new DbContext(this.connStr);
            var rst = new List<GlobalCode>();

            using (var db = context.getConnection())
            using (var handler = new SqlDbOperater(db))
            {
                rst = Entity<GlobalCode>.Query
                                        .Paging(10, 1)
                                        .Where(x => x.MajorCode == "Member")
                                        .And(x => x.MinorCode == "Status")
                                        .SetResult<ExecuteResult>()
                                        .Execute(handler.Command)
                                        .ToList();
            }

            Assert.That(rst.Count(), Is.EqualTo(1));
        }

        [Test, Order(6)]
        public void ContextAndQueryHelper_TestCase_Delete()
        {
            IContext context = new DbContext(this.connStr);
            var rst = new ExecuteResult();

            using (var db = context.getConnection())
            using (var handler = new SqlDbOperater(db))
            {
                rst = Entity<GlobalCode>.Query
                                        .Delete()
                                        .Where(x => x.MajorCode == "Member")
                                        .And(x => x.MinorCode == "Status")
                                        .SetResult<ExecuteResult>()
                                        .Execute(handler.Command)
                                        .ToResult() as ExecuteResult;
            }

            Assert.IsTrue(rst!.IsSuccess);
        }

        [Test]
        public void Entity_Test_Select_Case1()
        {
            IContext context = new DbContext(this.connStr);

            List<GlobalCode> list = new List<GlobalCode>();
            int paramCount = 0;
            object paramValue = default!;

            using (var db = context.getConnection())
            using (var handler = new SqlDbOperater(db))
            {
                var rst = Entity<GlobalCode>.Query
                                            .Select()
                                            .Where(x => x.KeyCode == "test")
                                            .Execute(handler.Command)
                                            .ToList();
                
                paramCount = handler.Command!.Parameters.Count;
                paramValue = handler.Command!.Parameters[0].Value;
            }

            Assert.That(paramCount, Is.EqualTo(1));
            Assert.That(Convert.ToString(paramValue), Is.EqualTo("test"));
        }

        [Test]
        public void Entity_Test_Select_Case2()
        {
            IContext context = new DbContext(this.connStr);

            List<GlobalCode> list = new List<GlobalCode>();
            int paramCount = 0;
            object paramValue = default!;

            using (var db = context.getConnection())
            using (var handler = new SqlDbOperater(db))
            {
                var rst = Entity<GlobalCode>.Query
                                            .Select()
                                            .Where(x => x.KeyCode == "test")
                                            .And(x => x.IsEnabled)
                                            .Execute(handler.Command)
                                            .ToList();

                paramCount = handler.Command!.Parameters.Count;
                paramValue = handler.Command!.Parameters[0].Value;
            }

            Assert.That(paramCount, Is.EqualTo(2));
            Assert.That(Convert.ToString(paramValue), Is.EqualTo("test"));
        }
    }
}