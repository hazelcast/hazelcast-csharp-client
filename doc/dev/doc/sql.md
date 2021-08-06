# SQL

The SQL service provided by Hazelcast .NET client allows you to query data stored in `IHMap` declaratively.

> **WARNING: The SQL feature is currently in beta. The compatibility between versions is not guaranteed. API might change between versions without notice. While in beta, SQL feature is tested against the same version of the IMDG, e.g 5.0-BETA

## Example: How to Query an IHMap using SQL

This SQL query returns map entries whose values are more than 1:

```csharp
await using var map = await client.GetMapAsync<int, string>("my-map");
await map.SetAllAsync(Enumerable.Range(1, 5).ToDictionary(v => v, v => $"{v}"));

await using var result = client.Sql.ExecuteQuery($"SELECT __key, this FROM {map.Name} WHERE this > 2");

await foreach (var row in result.EnumerateOnceAsync())
    Console.WriteLine($"{row.GetKey<int>()}: {row.GetValue<string>()}");
```

## Querying IHMap

The following subsections describe how you can access Hazelcast map objects and perform queries on them.

### Names

The SQL service exposes `IHMap` objects as tables in the predefined `partitioned` schema using exact names.
This schema is in the SQL service search path so that you can access the `IHMap` objects with or without the schema name.s

Schema and table names are case-sensitive; you can access the `employee` map, for example, as employee or `partitioned.employee`,
but not as `Employee`:
```sql
SELECT * FROM employee
SELECT * FROM partitioned.employee
```

### Fields

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

The `__key` and `this` fields are returned by the `SELECT *` queries if they do not have nested fields. For `IHMap<number, Employee>`,
the following query does not return the `this` field, because the value has nested fields `Name` and `Age`:

```sql
-- Returns __key, Name, Age
SELECT * FROM employee
```

## Data Types

The SQL service supports a set of SQL data types represented by `Hazelcast.Sql.SqlColumnType` enum. The table below shows SQL datatype, and corresponding .NET types:

| Column Type                  | Javascript                      |
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

SQL `TIMESTAMP` type uses `yyyy-mm-dd(T|t)HH:mm:ss.SSS` format which is the combination of
`DATE` and `TIME` strings. There must be a `T` or `t` letter in between.

Examples: `2021-07-01T10:20:30`, `1990-12-31t23:59:59.999999999`

### Timestamp with Timezone String Format

SQL `TIMESTAMP WITH TIMEZONE` type is sent and received as a string with the `yyyy-mm-dd(T|t)HH:mm:ss.SSS{timezoneString}` format which is the combination of
`TIMESTAMP` and timezone strings. The timezone string can be one of `Z`, `+hh:mm` or `-hh:mm` where `hh` represents hour-in-day, and `mm` represents minutes-in-hour.
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

#### Using Long

In the example below, age column is of type `INTEGER`. Since long objects are sent as `BIGINT` and `BIGINT` is comparable with `INTEGER`, the query is valid without an explicit `CAST`.

```javascript
const result = client.getSqlService().execute(
    'SELECT * FROM myMap WHERE age > ? AND age < ?',
    [long.fromNumber(13), long.fromNumber(18)]
);
```

### An Example of Casting

In the example below, age column is of type `INTEGER`. The default number type is `double` in Node.js client. We cast
doubles as `BIGINT`, and `BIGINT` is comparable with `INTEGER` the query is valid. Note that we can also cast to other types that are
comparable with `INTEGER`.

```javascript
const result = client.getSqlService().execute(
    'SELECT * FROM myMap WHERE age > CAST(? AS BIGINT) AND age < CAST(? AS BIGINT)',
    [13, 18]
);
```

#### Important Notes About Comparison and Casting

* In case of comparison operators (=, <, <>, ...), if one side is `?`, it's assumed to be exactly the other side's type, except that `TINYINT`, `SMALLINT`, `INTEGER` are all converted to `BIGINT`.

* String parameters can be cast to any type. The cast operation may fail though.

* To send a `DECIMAL` type, use a string with an explicit `CAST`.

* To send date and time related types, use a string with an explicit `CAST`.

* See [SQL data types code samples](code_samples/sql-data-types.js) for example usage of all data types.

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

* `GROUP BY`/`HAVING`
* `JOIN`
* set operators (`UNION`, `INTERSECT`, `MINUS`)
* subqueries (`SELECT … FROM table WHERE x = (SELECT …)`)

## Expressions

Hazelcast SQL supports logical predicates, `IS` predicates, comparison operators, mathematical functions and operators, string functions, and special functions.
Refer to [IMDG docs](https://docs.hazelcast.com/imdg/4.2/sql/expressions.html) for all possible operations.

## Lite Members

You cannot start SQL queries on lite members. This limitation will be removed in future releases.

## More Information

Please refer to [IMDG SQL docs](https://docs.hazelcast.com/imdg/4.2/sql/distributed-sql.html) for more information.

For basic usage of SQL, see `SqlBasicQueryExample` in `Hazelcast.Net.Examples` project.
