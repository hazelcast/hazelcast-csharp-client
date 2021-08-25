# SQL <!-- omit in toc -->

- [Example: How to Query an IHMap using SQL](#example-how-to-query-an-ihmap-using-sql)
- [Querying IHMap](#querying-ihmap)
  - [Map Names](#map-names)
  - [Field Names](#field-names)
    - [Key and Value Objects](#key-and-value-objects)
    - [Key and Value Fields](#key-and-value-fields)
    - ["SELECT *" Queries](#select--queries)
  - [Special characters in names](#special-characters-in-names)
  - [Enumerating query result](#enumerating-query-result)
  - [Disposing query/command result](#disposing-querycommand-result)
  - [Cancelling query enumeration](#cancelling-query-enumeration)
- [Data Types](#data-types)
  - [Decimal String Format](#decimal-string-format)
  - [Date String Format](#date-string-format)
  - [Time String Format](#time-string-format)
  - [Timestamp String Format](#timestamp-string-format)
  - [Timestamp with Timezone String Format](#timestamp-with-timezone-string-format)
- [Casting](#casting)
  - [How to Cast](#how-to-cast)
  - [Casting Between Types](#casting-between-types)
  - [An Example of Implicit Cast](#an-example-of-implicit-cast)
  - [An Example of Explicit Cast](#an-example-of-explicit-cast)
  - [Important Notes About Comparison and Casting](#important-notes-about-comparison-and-casting)
- [SELECT](#select)
  - [Synopsis](#synopsis)
  - [Description](#description)
  - [Sorting](#sorting)
  - [Unsupported Features](#unsupported-features)
- [Expressions](#expressions)
- [Lite Members](#lite-members)
- [More Information](#more-information)

The SQL service provided by Hazelcast .NET client allows you to query data stored in `IHMap` declaratively.

> **WARNING: The SQL feature is currently in beta. The compatibility between versions is not guaranteed. API might change between versions without notice. While in beta, SQL feature is tested against the same version of the IMDG, e.g 5.0-BETA

## Example: How to Query an IHMap using SQL

This SQL query returns map entries whose values are more than 1:

```csharp
await using var map = await client.GetMapAsync<int, string>("MyMap");
await map.SetAllAsync(Enumerable.Range(1, 5).ToDictionary(v => v, v => $"{v}"));

await using var result = client.Sql.ExecuteQuery($"SELECT __key, this FROM {map.Name} WHERE this > 2");

await foreach (var row in result.EnumerateOnceAsync())
    Console.WriteLine($"{row.GetKey<int>()}: {row.GetValue<string>()}");
```

## Querying IHMap

The following subsections describe how you can access Hazelcast map objects and perform queries on them.

### Map Names

The SQL service exposes `IHMap` objects as tables in the predefined `partitioned` schema using exact names.
This schema is in the SQL service search path so that you can access the `IHMap` objects with or without the schema name.s

Schema and table names are case-sensitive; you can access the `employee` map, for example, as employee or `partitioned.employee`,
but not as `Employee`:
```sql
SELECT * FROM employee
SELECT * FROM partitioned.employee
```

### Field Names

The SQL service resolves fields accessible from the SQL automatically. The service reads the first local entry pair of the `IHMap` to construct the list of fields. If the `IHMap` does not have local entries on the member where the query is started, then the list of fields cannot be resolved, and an exception is thrown.

Field names are case-sensitive.

#### Key and Value Objects

An `IHMap` entry consists of a key and a value. These are accessible through the `__key` and `this` aliases. The following
query returns the keys and values of all entries in a map:

```sql
SELECT __key, this FROM employee
```

#### Key and Value Fields

You may also access the nested fields of a key or value. The list of exposed fields depends on the serialization format, as described below:

* For [IdentifiedDataSerializable](serialization.md#identifieddataserializable-serialization) objects, you can use public field name or getter names.
  See [IMDG docs](https://docs.hazelcast.com/imdg/4.2/sql/querying-imap.html#key-and-value-fields) for more information.
* For [Portable](serialization.md#portable-serialization) objects, the fields written with `IPortableWriter` methods are exposed using their exact names.

> **NOTE: You cannot query JSON fields in SQL. If you want to query JSON, see [Querying with JSON Strings](distributedQuery.md#querying-with-json-strings).**

For example, consider this portable class:

```csharp
public class Employee : IPortable
{
    int IPortable.ClassId => 123;
    int IPortable.FactoryId => 345;

    public int Age { get; set; }
    public string Name { get; set; }

    public void ReadPortable(IPortableReader reader)
    {
        Age = reader.ReadInt(nameof(Age));
        Name = reader.ReadString(nameof(Name));
    }

    public void WritePortable(IPortableWriter writer)
    {
        writer.WriteInt(nameof(Age), Age);
        writer.WriteString(nameof(Name), Name);
    }
}
```

The SQL service can access the following fields:

|  Name  |  SQL Type |
|--------|-----------|
|  name  |  VARCHAR  |
|  age   |  INTEGER  |

Together with the key and value objects, you may query the following fields from `IHMap<int, Employee>`:

```sql
SELECT __key, this, Name, Age FROM employee
```

If both the key and value have fields with the same name, then the field of the value is exposed.

#### "SELECT *" Queries

You may use the `SELECT * FROM <table>` syntax to get all the table fields.

The `__key` and `this` fields are returned by the `SELECT *` queries if they do not have nested fields. For `IHMap<number, Employee>`, the following query does not return the `this` field, because the value has nested fields `Name` and `Age`:

```sql
-- Returns __key, Name, Age
SELECT * FROM employee
```

### Special characters in names

If map or field name contains non-alphanumeric characters or starts with a number, you will need to enclose it in double quotes:

```sql
SELECT * FROM "my-map"
SELECT * FROM "2map"
```

### Enumerating query result
`ISqlService.ExecuteQuery` returns `Hazelcast.Sql.ISqlQueryResult` which provides methods to manage current query:

```csharp
await using var result = client.Sql.ExecuteQuery("SELECT Name, Age FROM employee");
```

It implements `IAsyncEnumerable<SqlRow>` as one-off stream of rows and can be enumerated via regular `foreach` cycle:

```csharp
await foreach (var row in result)
    Console.WriteLine(row.GetColumn<string>("Name"));
```

Using LINQ over `IAsyncEnumerable<T>` is also possible but requires installing [System.Linq.Async](https://www.nuget.org/packages/System.Linq.Async) package. See [SqlLinqEnumerationExample](https://github.com/hazelcast/hazelcast-csharp-client/tree/master/src/Hazelcast.Net.Examples/Sql/SqlLinqEnumerationExample.cs) as an example.

> **NOTE: Obtained result is not reusable as `IAsyncEnumerable<SqlRow>`. It will never restart enumeration but continue where previous one finished.**

### Disposing query/command result

Both `ISqlQueryResult` and `ISqlCommandResult` implements `IAsyncDisposable`. Their `DisposeAsync` implementation will make sure to cancel the query and free used server resources.

Because of this, it is recommended to wrap operations with query or command into `await using` statement. This will ensure to send Cancel request in case if query is cancelled client-side or exception is thrown before it is completed or all rows are exhausted:

```csharp
await using (var result = client.Sql.ExecuteQuery("SELECT * FROM MyMap"))
{
    //...
}
```

```csharp
await using var result = Client.Sql.ExecuteCommand("INSERT INTO MyMap VALUES (1, 2)");
//...
```

### Cancelling query enumeration

You can cancel enumeration of `ISqlQueryResult`, via [WithCancellation](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.taskasyncenumerableextensions.withcancellation) extension method, see [SqlCancellationExample](https://github.com/hazelcast/hazelcast-csharp-client/tree/master/src/Hazelcast.Net.Examples/Sql/SqlCancellationExample.cs).

If you're using System.Linq.Async package, you can also pass `CancellationToken` to `ToListAsync`, `ToArrayAsync` and related methods.

> **NOTE: At the moment cancellation doesn't work during server query itself. Cancellation will stop the enumeration before fetching next page or switching to the next row of the current page, but won't stop executing request. This will be fixed in the later versions.**

## Data Types

The SQL service supports a set of SQL data types represented by `Hazelcast.Sql.SqlColumnType` enum. The table below shows SQL datatype, and corresponding .NET types:

| Column Type                  | .NET                      |
|------------------------------|---------------------------------|
| **VARCHAR**                  | `string`                        |
| **BOOLEAN**                  | `bool`                          |
| **TINYINT**                  | `byte`                          |
| **SMALLINT**                 | `short`                         |
| **INTEGER**                  | `int`                           |
| **BIGINT**                   | `long`                          |
| **DECIMAL**                  | `Hazelcast.Sql.HBigDecimal`     |
| **REAL**                     | `float`                         |
| **DOUBLE**                   | `double`                        |
| **DATE**                     | `Hazelcast.Sql.HLocalDate`      |
| **TIME**                     | `Hazelcast.Sql.HLocalTime`      |
| **TIMESTAMP**                | `Hazelcast.Sql.HLocalDateTime`  |
| **TIMESTAMP_WITH_TIME_ZONE** | `Hazelcast.Sql.HOffsetDateTime` |
| **OBJECT**                   | Any class                       |
| **NULL**                     | `null`                          |

All `Hazelcast.Sql.*` types has conversion to and from their closest built-in counterparts. Table below lists possible conversions:

| Hazelcast type                  | To .NET Type      | From .NET Type         |
----------------------------------|-------------------|------------------------|
| `Hazelcast.Sql.HBigDecimal`     | `decimal`**       | `decimal`              |
| `Hazelcast.Sql.HLocalDate`      | `DateTime`*       | `DateTime`             |
| `Hazelcast.Sql.HLocalTime`      | `TimeSpan`        | `TimeSpan`, `DateTime` |
| `Hazelcast.Sql.HLocalDateTime`  | `DateTime`*       | `DateTime`             |
| `Hazelcast.Sql.HOffsetDateTime` | `DateTimeOffset`* | `DateTimeOffset`       |
<sup>* - Possible `ArgumentOutOfRangeException` </sup>
<sup>** - Possible `OverflowException` </sup>
|

### Decimal String Format

SQL `DECIMAL` type uses dot as separator.

Examples: `12345`, `123456.789`.

### Date String Format

SQL `DATE` type uses `yyyy-mm-dd` format.

Examples: `2021-07-01`, `1990-12-31`.

### Time String Format

SQL `TIME` uses `HH:mm:ss.SSS` where `HH` is in 24-hour format and, `SSS` represents nanoseconds and can be at most 9 digits long.

Examples: `10:20:30`, `23:59:59.999999999`

### Timestamp String Format

SQL `TIMESTAMP` type uses `yyyy-mm-dd(T|t)HH:mm:ss.SSS` which is the combination of
`DATE` and `TIME` strings. There must be a `T` or `t` letter in between.

Examples: `2021-07-01T10:20:30`, `1990-12-31t23:59:59.999999999`

### Timestamp with Timezone String Format

SQL `TIMESTAMP WITH TIMEZONE` uses `yyyy-mm-dd(T|t)HH:mm:ss.SSS{timezoneString}` which is the combination of `TIMESTAMP` and timezone strings. The timezone string can be one of `Z`, `+hh:mm` or `-hh:mm` where `hh` represents hour-in-day, and `mm` represents minutes-in-hour.
The timezone must be in the range `[-18:00, +18:00]`.

`2021-07-01T10:20:30Z`, `1990-12-31t23:59:59.999999999+11:30`

## Casting

You may need to use casting when sending parameters for certain types. In general, you should try to send a parameter that has the same data type as the related column.

### How to Cast

Casting syntax: `CAST(? AS TYPE)`

Example casting:

```sql
SELECT * FROM someMap WHERE this = CAST(? AS INTEGER)
```

### Casting Between Types

When comparing a column with a parameter, your parameter must be of a compatible type. You can cast string to every SQL type.

### An Example of Implicit Cast
In the example below, Age column is of type `INTEGER`. We pass parameters as shorts (`TINYINT`) and they are automatically casted to `INTEGER` for comparison.

```csharp
await using var result = client.Sql.ExecuteQuery(
    $"SELECT Name FROM {map.Name} WHERE Age > ? AND Age < ?",
    (short)20, (short)30
);
```

### An Example of Explicit Cast

In the example below, Age column is of type `INTEGER`. We pass parameters as strings (`VARCHAR`) and cast them to `INTEGER` for comparison.

```csharp
await using var result = client.Sql.ExecuteQuery(
    $"SELECT Name FROM {map.Name} WHERE Age > CAST(? AS INTEGER) AND Age < CAST(? AS INTEGER)",
    "20", "30"
);
```

### Important Notes About Comparison and Casting

* In case of comparison operators (=, <, <>, ...), if one side is `?`, it's assumed to be exactly the other side's type, except that `TINYINT`, `SMALLINT`, `INTEGER` are all converted to `BIGINT`. Note, that reverse is not valid as it may lead to value loss.

* String parameters can be cast to any type. The cast operation may fail though.

* To send a `DECIMAL` type, use `Hazelcast.Sql.HBigDecimal` or an explicit `CAST` from string or other number type.

* To send date and time related types, use corresponding `Hazelcast.Sql.H*` type or a string with an explicit `CAST`.

## SELECT

### Synopsis

```sql
SELECT [ * | expression [ [ AS ] expression_alias ] [, ...] ]
FROM table_name [ [ AS ] table_alias ]
[WHERE condition]
```

### Description

The `SELECT` command retrieves rows from a table. A row is a sequence of expressions defined after the `SELECT` keyword. Expressions may have optional aliases.

`table_name` refers to a single `IHMap` data structure. A table may have an optional alias.

An optional `WHERE` clause defines a condition, that is any expression that evaluates to a result of type boolean. Any row that doesn’t satisfy the condition is eliminated from the result.

### Sorting

You can use the standard SQL clauses ORDER BY, LIMIT, and OFFSET to sort and limit the result set. In order to do so, you need server configuration. See [IMDG docs](https://docs.hazelcast.com/imdg/4.2/sql/select-statement.html#sorting) for more.

### Unsupported Features

The following features are **not supported** and are planned for future releases:

* set operators (`UNION`, `INTERSECT`, `MINUS`)
* subqueries (`SELECT … FROM table WHERE x = (SELECT …)`)

## Expressions

Hazelcast SQL supports logical predicates, `IS` predicates, comparison operators, mathematical functions and operators, string functions, and special functions.
Refer to [IMDG docs](https://docs.hazelcast.com/imdg/4.2/sql/expressions.html) for all possible operations.

## Lite Members

You cannot start SQL queries on lite members. This limitation will be removed in future releases.

## More Information

Please refer to [IMDG SQL docs](https://docs.hazelcast.com/imdg/4.2/sql/distributed-sql.html) for more information.

For basic usage of SQL, see `SqlBasicQueryExample` in *Hazelcast.Net.Examples* project.
