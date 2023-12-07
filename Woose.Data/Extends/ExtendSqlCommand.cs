using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Woose.Core;

namespace Woose.Data
{
    public static class ExtendSqlCommand
    {
        public static DataTable ExecuteTable(this SqlCommand cmd)
        {
            var result = new DataTable();
            using (var adp = new SqlDataAdapter(cmd))
            {
                adp.Fill(result);
            }
            return result;
        }

        public static T ExecuteEntity<T>(this SqlCommand cmd) where T : new()
        {
            var result = new T();
            var dt = cmd.ExecuteTable();
            if (dt != null && dt.Rows.Count > 0)
            {
                result = EntityHelper.ColumnToEntity<T>(dt);
            }
            return result;
        }

        public static List<T> ExecuteEntities<T>(this SqlCommand cmd) where T : new()
        {
            var result = new List<T>();
            var dt = cmd.ExecuteTable();
            if (dt != null && dt.Rows.Count > 0)
            {
                result = EntityHelper.ColumnToEntities<T>(dt);
            }
            return result;
        }

        public static ExecuteResult NoneExecuteResult(this SqlCommand cmd)
        {
            var result = new ExecuteResult();
            int cnt = cmd.ExecuteNonQuery();
            if (cnt > 0)
            {
                result.Success(cnt);
            }
            else
            {
                result.Error("Regist Fail");
            }
            return result;
        }

        public static int ExecuteCount(this SqlCommand cmd)
        {
            int result = 0;
            
            try
            {
                result = Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch
            {
                result = 0;
            }

            return result;
        }

        public static void Set(this SqlParameterCollection Parameter, string Name, SqlDbType type, object? Value)
        {
            Parameter.Add(Name, type);
            Parameter[Name].Value = Value;
        }

        public static void Set(this SqlParameterCollection Parameter, string Name, SqlDbType type, object? Value, int size)
        {
            Parameter.Add(Name, type, size);
            Parameter[Name].Value = Value;
        }

        public static void Set(this SqlParameterCollection Parameter, string Name, decimal Value, byte Scale)
        {
            Parameter.Add(Name, SqlDbType.Decimal, 18);
            Parameter[Name].Precision = 18;
            Parameter[Name].Scale = Scale;
            Parameter[Name].Value = Value;
        }

        public static void SetOutput(this SqlParameterCollection Parameter, string Name, SqlDbType type)
        {
            Parameter.Add(Name, type);
            Parameter[Name].Direction = ParameterDirection.Output;
        }

        public static void SetOutput(this SqlParameterCollection Parameter, string Name, SqlDbType type, int size)
        {
            Parameter.Add(Name, type, size);
            Parameter[Name].Direction = ParameterDirection.Output;
        }

        public static void SetReturnValue(this SqlParameterCollection Parameter)
        {
            Parameter.Add("@Code", SqlDbType.BigInt);
            Parameter["@Code"].Direction = ParameterDirection.Output;
            Parameter.Add("@Value", SqlDbType.VarChar, 100);
            Parameter["@Value"].Direction = ParameterDirection.Output;
            Parameter.Add("@Msg", SqlDbType.NVarChar, 100);
            Parameter["@Msg"].Direction = ParameterDirection.Output;
        }

        public static object GetOutParameterValue(this SqlCommand cmd, string Name)
        {
            return cmd.Parameters[Name].Value;
        }

        public static ReturnValue ExecuteReturnValue(this SqlCommand cmd)
        {
            var result = new ReturnValue();

            cmd.Parameters.SetReturnValue();
            cmd.ExecuteNonQuery();
            result.Code = Convert.ToInt64(cmd.GetOutParameterValue("@Code"));
            result.Value = Convert.ToString(cmd.GetOutParameterValue("@Value"));
            result.Message = Convert.ToString(cmd.GetOutParameterValue("@Msg"));
            result.Check = (result.Code > 0);

            return result;
        }

        public static ReturnValues<List<T>> ExecuteReturnValues<T>(this SqlCommand cmd) where T : new()
        {
            var result = new ReturnValues<List<T>>();

            cmd.Parameters.SetReturnValue();
            var tmp = new DataTable();
            using (var adp = new SqlDataAdapter(cmd))
            {
                adp.Fill(tmp);
            }
            result.Code = Convert.ToInt64(cmd.GetOutParameterValue("@Code"));
            result.Value = Convert.ToString(cmd.GetOutParameterValue("@Value"));
            result.Message = Convert.ToString(cmd.GetOutParameterValue("@Msg"));
            result.Check = (result.Code > 0);
            result.Data = EntityHelper.ColumnToEntities<T>(tmp);

            return result;
        }

        public static ExecuteResult ExecuteResult(this SqlCommand cmd)
        {
            var result = new ExecuteResult();
            result = cmd.ExecuteEntity<ExecuteResult>();
            return result;
        }
    }
}
