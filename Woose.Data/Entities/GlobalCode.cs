using Woose.Core;

namespace Woose.Data
{
    public class GlobalCode : BaseEntity, IEntity
    {
        [Entity("CodeIDX", System.Data.SqlDbType.BigInt, 8, true)]
        public long CodeIDX { get; set; } = -1;

        [Entity("MajorCode", System.Data.SqlDbType.VarChar, 50)]
        public string MajorCode { get; set; } = string.Empty;

        [Entity("MinorCode", System.Data.SqlDbType.VarChar, 50)]
        public string MinorCode { get; set; } = string.Empty;

        [Entity("Code", System.Data.SqlDbType.VarChar, 50)]
        public string Code { get; set; } = string.Empty;

        [Entity("MajorName", System.Data.SqlDbType.NVarChar, 30)]
        public string MajorName { get; set; } = string.Empty;

        [Entity("MinorName", System.Data.SqlDbType.NVarChar, 30)]
        public string MinorName { get; set; } = string.Empty;

        [Entity("Name", System.Data.SqlDbType.NVarChar, 30)]
        public string Name { get; set; } = string.Empty;

        [Entity("MappingKey", System.Data.SqlDbType.BigInt, 8)]
        public long MappingKey { get; set; } = -1;

        [Entity("IsEnabled", System.Data.SqlDbType.Bit, 1)]
        public bool IsEnabled { get; set; } = true;


        public GlobalCode()
        {
            this.TableName = "GlobalCode";
            this.PrimaryColumn = "CodeIDX";
        }
    }
}
