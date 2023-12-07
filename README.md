# Woose .net Library for mssql - beta version

Woose는 ASP.NET 라이브러리로, 좀 더 편리하게 개발을 할 수 있도록 도와주는 여러가지 라이브러리와
MSSQL 연동시 어플리케이션 의존성 없이도 동적 쿼리를 생성하여 결과물을 리턴받을 수 있습니다.
모든 기능은 DB에 중점을 두고 구현되어 있으므로, DB 의존적인 작업에서 효율적입니다.


> :warning: **주의사항:** 이 프로젝트는 코드 중심의 EntityFramework를 DB 중심 (DB First)으로 Lite하게 활용해 보고자 접근해본 토이프로젝트이므로, 실무에서 사용은 신중하시기 바랍니다.


## 설치

NuGet 패키지 관리자 콘솔에서 다음 명령어를 사용하여 Woose를 설치하세요.

```bash
Install-Package Woose.Core
Install-Package Woose.Data
Install-Package Woose.API
```

## 예제코드

다음은 엔티티 정의 예제입니다.

```csharp
public class GlobalCode : BaseEntity, IEntity
{
    [Entity("CodeIDX", System.Data.SqlDbType.BigInt, 8, true)]
    public long CodeIDX { get; set; } = -1;

    [Entity("MajorCode", System.Data.SqlDbType.VarChar, 50)]
    public string MajorCode { get; set; } = string.Empty;

    [Entity("MinorCode", System.Data.SqlDbType.VarChar, 50)]
    public string MinorCode { get; set; } = string.Empty;

    [Entity("KeyCode", System.Data.SqlDbType.VarChar, 50)]
    public string KeyCode { get; set; } = string.Empty;

    [Entity("MajorName", System.Data.SqlDbType.NVarChar, 30)]
    public string MajorName { get; set; } = string.Empty;

    [Entity("MinorName", System.Data.SqlDbType.NVarChar, 30)]
    public string MinorName { get; set; } = string.Empty;

    [Entity("KeyName", System.Data.SqlDbType.NVarChar, 30)]
    public string KeyName { get; set; } = string.Empty;

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
```


다음은 실제 사용 코드 예제입니다.


### Select (single line)

```csharp
using (var db = context.getConnection())
using (var cmd = db.CreateCommand())
{
    cmd.On<TableEntity>().Select(1).Where("ColumnA", Value1).Set();
    result = cmd.ExecuteEntity<TableEntity>();
}
```


### Select (multi line)

```csharp
using (var db = context.getConnection())
using (var cmd = db.CreateCommand())
{
	cmd.On<TableEntity>().Select().Where("ColumnA", Value1).Set();
	result = cmd.ExecuteEntities<TableEntity>();
}
```

### Paging (multi line)

```csharp
using (var db = context.getConnection())
using (var cmd = db.CreateCommand())
{
    // ({PageSize},{CurrentPage})
	cmd.On<TableEntity>().Paging(10, paramData.CurPage).Where("ColumnA", Value1).Set();
	result = cmd.ExecuteEntities<TableEntity>();
}
```


### Count

```csharp
using (var db = context.getConnection())
using (var cmd = db.CreateCommand())
{
	cmd.On<TableEntity>().Count().Where("ColumnA", Value1).Set();
	result = cmd.ExecuteCount();
}
```

### Insert

```csharp
using (var db = context.getConnection())
using (var cmd = db.CreateCommand())
{
	cmd.On<TableEntity>(tableEntityInstance).Insert().Try().Set();
	result = cmd.ExecuteResult();
}
```

### Update

```csharp
using (var db = context.getConnection())
using (var cmd = db.CreateCommand())
{
	cmd.On<TableEntity>(tableEntityInstance).Update().Try().Where("idx", tableEntityInstance.idx).Set();
	result = cmd.ExecuteResult();
}
```

### Delete

```csharp
using (var db = context.getConnection())
using (var cmd = db.CreateCommand())
{
	cmd.On<TableEntity>().Delete().Try().Where("ColumnA", Value1).Set();
	result = cmd.ExecuteResult();
}
```
