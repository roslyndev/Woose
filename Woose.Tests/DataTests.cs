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
            List<GlobalCode> codeList = new List<GlobalCode>();

            using (var db = context.getConnection())
            using (var handler = new SqlDbOperater(db))
            {
                codeList = await Entity<GlobalCode>.Run.On(handler)
                                                   .Select(1)
                                                   .Where(x => x.MajorCode == "Member")
                                                   .And(x => x.MinorCode == "Status")
                                                   .ToListAsync();
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
                rst = Entity<GlobalCode>.Run.On(handler)
                                        .Insert(paramData)
                                        .SetResult<ExecuteResult>()
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
                rst = Entity<GlobalCode>.Run.On(handler)
                                        .Update(paramData)
                                        .Where(x => x.MajorCode == "Member")
                                        .And(x => x.MinorCode == "Status")
                                        .SetResult<ExecuteResult>()
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
                cnt = Entity<GlobalCode>.Run.On(handler)
                                        .Count()
                                        .Where(x => x.MajorCode == "Member")
                                        .And(x => x.MinorCode == "Status")
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
                rst = Entity<GlobalCode>.Run.On(handler)
                                        .Paging(10, 1)
                                        .Where(x => x.MajorCode == "Member")
                                        .And(x => x.MinorCode == "Status")
                                        .OrderBy(x => x.CodeIDX, QueryOption.Sequence.DESC)
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
                rst = Entity<GlobalCode>.Run.On(handler)
                                        .Delete()
                                        .Where(x => x.MajorCode == "Member")
                                        .And(x => x.MinorCode == "Status")
                                        .SetResult<ExecuteResult>()
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
                var rst = Entity<GlobalCode>.Run.On(handler)
                                            .Select()
                                            .Where(x => x.KeyCode == "test")
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
                var rst = Entity<GlobalCode>.Run.On(handler)
                                            .Select()
                                            .Where(x => x.KeyCode == "test")
                                            .And(x => x.IsEnabled)
                                            .ToList();

                paramCount = handler.Command!.Parameters.Count;
                paramValue = handler.Command!.Parameters[0].Value;
            }

            Assert.That(paramCount, Is.EqualTo(2));
            Assert.That(Convert.ToString(paramValue), Is.EqualTo("test"));
        }

        [Test]
        public void Entity_Test_Select_Case3()
        {
            IContext context = new DbContext(this.connStr);

            int cnt = 0;
            string strValue = string.Empty;

            using (var db = context.getConnection())
            using (var handler = new SqlDbOperater(db))
            {
                var dt = Entity.Run.On(handler)
                                     .Query("select 1 as [idx], 'Test' as [title] union select 2, 'sample'")
                                     .ToList();

                if (dt != null && dt.Rows.Count > 0)
                {
                    cnt = dt.Rows.Count;
                    foreach(DataRow row in dt.Rows)
                    {
                        strValue = row["title"].ToString();
                        break;
                    }
                }
            }

            Assert.That(cnt, Is.EqualTo(2));
            Assert.That(Convert.ToString(strValue), Is.EqualTo("Test"));
        }

        [Test]
        public void Entity_Test_Select_Case4()
        {
            IContext context = new DbContext(this.connStr);

            int idx = 0;
            string strValue = string.Empty;

            using (var db = context.getConnection())
            using (var handler = new SqlDbOperater(db))
            {
                var recode = Entity.Run.On(handler)
                                       .Query("select 1 as [idx], 'Test' as [title] union select 2, 'sample'")
                                       .ToEntity();

                if (recode != null)
                {
                    idx = Convert.ToInt32(recode["idx"]);
                    strValue = Convert.ToString(recode["title"]);
                }
            }

            Assert.That(idx, Is.EqualTo(1));
            Assert.That(Convert.ToString(strValue), Is.EqualTo("Test"));
        }

        [Test]
        public void Entity_Test_Select_Case5()
        {
            IContext context = new DbContext(this.connStr);

            int cnt = 0;
            string strValue = string.Empty;

            using (var db = context.getConnection())
            using (var handler = new SqlDbOperater(db))
            {
                var dt = Entity.Run.On(handler)
                                   .StoredProcedure("sp_server_info")
                                   //.AddParameter("@name", SqlDbType.VarChar, "test", 50)
                                   .ToList();

                if (dt != null && dt.Rows.Count > 0)
                {
                    cnt = dt.Rows.Count;
                    foreach (DataRow row in dt.Rows)
                    {
                        strValue = row[1].ToString();
                        break;
                    }
                }
            }

            Assert.That(cnt, Is.EqualTo(29));
            Assert.That(Convert.ToString(strValue), Is.EqualTo("DBMS_NAME"));
        }
    }
}