using System;
using System.Data.SqlClient;
using Woose.Core;
using Woose.Data;

namespace Woose.API
{
    public interface IRepository
    {
        SqlTransaction BeginTransaction(SqlConnection conn);
        void Commit(SqlTransaction tran);
        void Rollback(SqlTransaction tran);
        T Single<T>(long idx) where T : IEntity, new();
        T Single<T>(string whereStr) where T : IEntity, new();
        List<T> Select<T>(string whereStr) where T : IEntity, new();
        List<T> Select<T>(string whereStr, int TopCount) where T : IEntity, new();
        List<T> Select<T>(string whereStr, QueryOption.Sequence OrderBy, int TopCount) where T : IEntity, new();
        List<T> Select<T>(string whereStr, QueryOption.Sequence OrderBy, string OrderColumn, int TopCount) where T : IEntity, new();
        int Count<T>(string whereStr) where T : IEntity, new();
        List<T> Paging<T>(IPagingParameter paramData) where T : IEntity, new();
        List<T> Execute<T>(string query) where T : IEntity, new();
        ReturnValue InsertOut<T>(T target) where T : IEntity, new();
        ReturnValue InsertOut<T>(T target, params string[] Columns) where T : IEntity, new();
        ReturnValue UpdateOut<T>(T target) where T : IEntity, new();
        ReturnValue UpdateOut<T>(T target, params string[] Columns) where T : IEntity, new();
        ReturnValue DeleteOut<T>(T target, string whereStr) where T : IEntity, new();
        ExecuteResult InsertIn<T>(T target) where T : IEntity, new();
        ExecuteResult InsertIn<T>(T target, params string[] Columns) where T : IEntity, new();
        ExecuteResult UpdateIn<T>(T target) where T : IEntity, new();
        ExecuteResult UpdateIn<T>(T target, params string[] Columns) where T : IEntity, new();
        ExecuteResult DeleteIn<T>(T target, string whereStr) where T : IEntity, new();

    }
}
