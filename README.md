# Woose .net Library for mssql  

:beta: 버전

Woose는 ASP.NET 라이브러리로, 좀 더 편리하게 개발을 할 수 있도록 도와주는 여러가지 라이브러리와
MSSQL 연동시 어플리케이션 의존성 없이도 동적 쿼리를 생성하여 결과물을 리턴받을 수 있습니다.
모든 기능은 DB에 중점을 두고 구현되어 있으므로, DB 의존적인 작업에서 효율적입니다.


## 설치

NuGet 패키지 관리자 콘솔에서 다음 명령어를 사용하여 Woose를 설치하세요.

```bash
Install-Package Woose.Core
Install-Package Woose.Data
```

## 예제코드

다음은 실제 사용 코드 예제입니다.


### Select (single line)

```csharp
  using (var db = context.getConnection())
  using (var handler = new SqlDbOperater(db))
  {
      var instance = Entity<TableEntity>.Query
                                        .Select(1)
                                        .Where(x => x.ColumnA == "Value1")
                                        .And(x => x.ColumnB == "Value2")
                                        .Execute(handler.Command)
                                        .ToEntity();
  }
```


### Select (multi line)

```csharp
  using (var db = context.getConnection())
  using (var handler = new SqlDbOperater(db))
  {
      var list = Entity<TableEntity>.Query
                                    .Select()
                                    .Where(x => x.ColumnA == "Value1")
                                    .And(x => x.ColumnB == "Value2")
                                    .Execute(handler.Command)
                                    .ToList();
  }
```


### Paging (multi line)

```csharp
  using (var db = context.getConnection())
  using (var handler = new SqlDbOperater(db))
  {
      var list = Entity<TableEntity>.Query
                                    .Paging(10, 1)  // ({PageSize},{CurrentPage})
                                    .Where(x => x.ColumnA == "Value1")
                                    .And(x => x.ColumnB == "Value2")
                                    .Execute(handler.Command)
                                    .ToList();
  }
```


### Count

```csharp
  using (var db = context.getConnection())
  using (var handler = new SqlDbOperater(db))
  {
      int count = Entity<TableEntity>.Query
                                     .Count()
                                     .Where(x => x.ColumnA == "Value1")
                                     .And(x => x.ColumnB == "Value2")
                                     .Execute(handler.Command)
                                     .ToCount();
  }
```

### Insert

```csharp
  using (var db = context.getConnection())
  using (var handler = new SqlDbOperater(db))
  {
      var rst = Entity<TableEntity>.Query
                                   .Insert(tableEntityInstance)
                                   .SetResult<ExecuteResult>()
                                   .Execute(handler.Command)
                                   .ToResult() as ExecuteResult;   //ExecuteResult : IFeedback
  }
```

### Update

```csharp
  using (var db = context.getConnection())
  using (var handler = new SqlDbOperater(db))
  {
      var rst = Entity<TableEntity>.Query
                                   .Update(tableEntityInstance)
                                   .Where(x => x.ColumnA == "Value1")
                                   .And(x => x.ColumnB == "Value2")
                                   .SetResult<ExecuteResult>()
                                   .Execute(handler.Command)
                                   .ToResult() as ExecuteResult;   //ExecuteResult : IFeedback
  }
```

### Delete

```csharp
  using (var db = context.getConnection())
  using (var handler = new SqlDbOperater(db))
  {
      var rst = Entity<TableEntity>.Query
                                   .Delete()
                                   .Where(x => x.ColumnA == "Value1")
                                   .And(x => x.ColumnB == "Value2")
                                   .SetResult<ReturnValue>()
                                   .Execute(handler.Command)
                                   .ToResult() as ReturnValue;   //ReturnValue : IFeedback
  }
```
