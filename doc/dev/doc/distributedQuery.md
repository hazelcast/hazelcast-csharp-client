# Distributed Query

Hazelcast partitions your data and spreads it across cluster of members. You can iterate over the map entries and look for certain entries (specified by predicates) you are interested in. However, this is not very efficient because you will have to bring the entire entry set and iterate locally. Instead, Hazelcast allows you to run distributed queries on your distributed map.

## How Distributed Query Works

1. The requested predicate is sent to each member in the cluster.
2. Each member looks at its own local entries and filters them according to the predicate. At this stage, key-value pairs of the entries are deserialized and then passed to the predicate.
3. The predicate requester merges all the results coming from each member into a single set.

Distributed query is highly scalable. If you add new members to the cluster, the partition count for each member is reduced and thus the time spent by each member on iterating its entries is reduced. In addition, the pool of partition threads evaluates the entries concurrently in each member, and the network traffic is also reduced since only filtered data is sent to the requester.

**Predicates Class Operators**

There are many built-in `IPredicate` implementations for your query requirements. Some of them are explained below.

* `TruePredicate`: This predicate returns true and hence includes all the entries on the response.
* `FalsePredicate`: This predicate returns false and hence filters out all the entries in the response.
* `EqualPredicate`: Checks if the result of an expression is equal to a given value.
* `NotEqualPredicate`: Checks if the result of an expression is not equal to a given value.
* `InstanceOfPredicate`: Checks if the result of an expression has a certain type.
* `LikePredicate`: Checks if the result of an expression matches some string pattern. `%` (percentage sign) is the placeholder for many characters, `_` (underscore) is placeholder for only one character.
* `ILikePredicate`: Same as LikePredicate. Checks if the result of an expression matches some string pattern. `%` (percentage sign) is the placeholder for many characters, `_` (underscore) is placeholder for only one character.
* `GreaterLessPredicate`: Checks if the result of an expression is greater equal or less equal than a certain value.
* `BetweenPredicate`: Checks if the result of an expression is between two values (start and end values are inclusive).
* `InPredicate`: Checks if the result of an expression is an element of a certain list.
* `NotPredicate`: Negates a provided predicate result.
* `RegexPredicate`: Checks if the result of an expression matches some regular expression.
* `SqlPredicate`: Query using SQL syntax.

**Simplifying with Predicates**

You can simplify the predicate usage with the `Predicates` class, which offers simpler predicate building.
Please see the below example code.

```c#
var users = hz.GetMap<string, User>("users");
// Add some users to the Distributed Map

// Create a Predicate from a String (a SQL like Where clause)
var sqlQuery = Predicates.Sql("active AND age BETWEEN 18 AND 21)");

// Creating the same Predicate as above but with a builder
var criteriaQuery = Predicates.And(
    Predicates.IsEqual("active", true),
    Predicates.IsBetween("age", 18, 21)
);
// Get result collections using the two different Predicates
var result1 = users.Values(sqlQuery);
var result2 = users.Values(criteriaQuery);
```

### Employee Map Query Example

Assume that you have an `employee` map containing the values of `Employee`, as coded below.

```c#
public class Employee : IPortable
{
    public const int TypeId = 100;

    public string Name { get; set; }
    public int Age { get; set; }
    public bool Active { get; set; }
    public double Salary { get; set; }

    public int GetClassId()
    {
        return TypeId;
    }

    public int GetFactoryId()
    {
        return ExampleDataSerializableFactory.FactoryId;
    }

    public void ReadPortable(IPortableReader reader)
    {
        Name = reader.ReadUTF("name");
        Age = reader.ReadInt("age");
        Active = reader.ReadBoolean("active");
        Salary = reader.ReadDouble("salary");
    }

    public void WritePortable(IPortableWriter writer)
    {
        writer.WriteUTF("name", Name);
        writer.WriteInt("age", Age);
        writer.WriteBoolean("active", Active);
        writer.WriteDouble("salary", Salary);
    }
}

```

Note that `Employee` is implementing `IPortable`. As portable types are not deserialized on the server side for querying, you don't need to implement its Java equivalent on the server side.

For the non-portable types, you need to implement its Java equivalent and its serializable factory on the server side for server to reconstitute the objects from binary formats.
In this case before starting the server, you need to compile the `Employee` and related factory classes with server's `CLASSPATH` and add them to the `user-lib` directory in the extracted `hazelcast-<version>.zip` (or `tar`). See the [Adding User Library to CLASSPATH section](#1212-adding-user-library-to-classpath).

> **NOTE: Querying with `IPortable` interface is faster as compared to `IdentifiedDataSerializable`.**

### Querying by Combining Predicates with AND, OR, NOT

You can combine predicates by using the `and`, `or` and `not` operators, as shown in the below example.

```c#
var criteriaQuery = Predicates.And(
    Predicates.IsEqual("active", true),
    Predicates.IsLessThan("age", 30)
);
var result2 = map.Values(criteriaQuery);
```

In the above example code, `predicate` verifies whether the entry is active and its `age` value is less than 30.
This method sends the predicate to all cluster members and merges the results coming from them.

> **NOTE: Predicates can also be applied to `keySet` and `entrySet` of the Hazelcast IMDG's distributed map.**

### Querying with SQL

`SqlPredicate` takes the regular SQL `where` clause. See the following example:

```c#
var map = hazelcastInstance.GetMap<string, Employee>( "employee" );
var employees = map.Values( new SqlPredicate( "active AND age < 30" ) );
```

#### Supported SQL Syntax

**AND/OR:** `<expression> AND <expression> AND <expression>…`

- `active AND age > 30`
- `active = false OR age = 45 OR name = 'Joe'`
- `active AND ( age > 20 OR salary < 60000 )`

**Equality:** `=, !=, <, ⇐, >, >=`

- `<expression> = value`
- `age <= 30`
- `name = 'Joe'`
- `salary != 50000`

**BETWEEN:** `<attribute> [NOT] BETWEEN <value1> AND <value2>`

- `age BETWEEN 20 AND 33 ( same as age >= 20 AND age ⇐ 33 )`
- `age NOT BETWEEN 30 AND 40 ( same as age < 30 OR age > 40 )`

**IN:** `<attribute> [NOT] IN (val1, val2,…)`

- `age IN ( 20, 30, 40 )`
- `age NOT IN ( 60, 70 )`
- `active AND ( salary >= 50000 OR ( age NOT BETWEEN 20 AND 30 ) )`
- `age IN ( 20, 30, 40 ) AND salary BETWEEN ( 50000, 80000 )`

**LIKE:** `<attribute> [NOT] LIKE 'expression'`

The `%` (percentage sign) is the placeholder for multiple characters, an `_` (underscore) is the placeholder for only one character.

- `name LIKE 'Jo%'` (true for 'Joe', 'Josh', 'Joseph' etc.)
- `name LIKE 'Jo_'` (true for 'Joe'; false for 'Josh')
- `name NOT LIKE 'Jo_'` (true for 'Josh'; false for 'Joe')
- `name LIKE 'J_s%'` (true for 'Josh', 'Joseph'; false 'John', 'Joe')

**ILIKE:** `<attribute> [NOT] ILIKE 'expression'`

ILIKE is similar to the LIKE predicate but in a case-insensitive manner.

- `name ILIKE 'Jo%'` (true for 'Joe', 'joe', 'jOe','Josh','joSH', etc.)
- `name ILIKE 'Jo_'` (true for 'Joe' or 'jOE'; false for 'Josh')

**REGEX:** `<attribute> [NOT] REGEX 'expression'`

- `name REGEX 'abc-.*'` (true for 'abc-123'; false for 'abx-123')

#### Querying Examples with Predicates

You can use the `__key` attribute to perform a predicated search for the entry keys. See the following example:

```c#
var employeeMap = client.getMap<string, Employee>("employees");
employeeMap.Put("Alice", new Employee {Name= "Alice", Age= "35"});
employeeMap.Put("Andy",  new Employee {Name= "Andy", Age= "37"});
employeeMap.Put("Bob",   new Employee {Name= "Bob", Age= "22"});
// ...
var predicate = new SqlPredicate("__key like A%");
var startingWithA = employeeMap.Values(predicate);
```

You can also use the helper class `Predicates` mentioned earlier. Here is an example:

```c#
//continued from previous example
var predicate = Predicates.Key().IsLike("A%");;
var startingWithA = personMap.values(predicate);
```

It is also possible to use a complex object as key and make query on key fields.

```c#
var employeeMap = client.getMap<Employee, int'>("employees");
employeeMap.Put(new Employee {Name= "Alice", Age= "35"}, 1);
employeeMap.Put(new Employee {Name= "Andy", Age= "37"}, 2);
employeeMap.Put(new Employee {Name= "Bob", Age= "22"}, 3);
// ...
var predicate = Predicates.Key("name").IsLike("A%");//identical to sql predicate:"__key#name LIKE A%"
var startingWithA = employeeMap.Values(predicate);
```

You can use the `this` attribute to perform a predicated search for entry values. See the following example:

```c#
//continued from previous example
var predicate=Predicates.IsGreaterThan("this", 2);
var result = employeeMap.Values(predicate);
//result will include only Bob
```

### Querying with JSON Strings

You can query JSON strings stored inside your Hazelcast clusters. To query the JSON string,
you first need to create a `HazelcastJsonValue` from the JSON string using the `HazelcastJsonValue(string jsonString)` constructor.
You can use ``HazelcastJsonValue``s both as keys and values in the distributed data structures.
Then, it is possible to query these objects using the Hazelcast query methods explained in this section.

```c#
    var person1 = new HazelcastJsonValue("{ \"age\": 35 }");
    var person2 = new HazelcastJsonValue("{ \"age\": 24 }");
    var person3 = new HazelcastJsonValue("{ \"age\": 17 }");

    var idPersonMap = client.GetMap<int, HazelcastJsonValue>("jsonValues");

    idPersonMap.Put(1, person1);
    idPersonMap.Put(2, person2);
    idPersonMap.Put(3, person3);

    var peopleUnder21 = idPersonMap.Values(Predicates.IsLessThan("age", 21));
```

When running the queries, Hazelcast treats values extracted from the JSON documents as Java types so they
can be compared with the query attribute. JSON specification defines five primitive types to be used in the JSON
documents: `number`,`string`, `true`, `false` and `null`. The `string`, `true/false` and `null` types are treated
as `String`, `boolean` and `null`, respectively. We treat the extracted `number` values as ``long``s if they
can be represented by a `long`. Otherwise, ``number``s are treated as ``double``s.

It is possible to query nested attributes and arrays in the JSON documents. The query syntax is the same
as querying other Hazelcast objects using the ``Predicate``s.

```c#
/**
 * Sample JSON object
 *
 * {
 *     "departmentId": 1,
 *     "room": "alpha",
 *     "people": [
 *         {
 *             "name": "Peter",
 *             "age": 26,
 *             "salary": 50000
 *         },
 *         {
 *             "name": "Jonah",
 *             "age": 50,
 *             "salary": 140000
 *         }
 *     ]
 * }
 *
 *
 * The following query finds all the departments that have a person named "Peter" working in them.
 */

var departmentWithPeter = departments.Values(Predicates.IsEqual("people[any].name", "Peter"));

```

`HazelcastJsonValue` is a lightweight wrapper around your JSON strings. It is used merely as a way to indicate
that the contained string should be treated as a valid JSON value. Hazelcast does not check the validity of JSON
strings put into to the maps. Putting an invalid JSON string into a map is permissible. However, in that case
whether such an entry is going to be returned or not from a query is not defined.

### Filtering with Paging Predicates

The .NET client provides paging for defined predicates. With its `PagingPredicate` class, you can get a list of keys, values or entries page
by page by filtering them with predicates and giving the size of the pages. Also, you can sort the entries by specifying comparators.

```c#
var map = hazelcastInstance.GetMap<int, Student>( "students" );
var greaterEqual = Predicates.greaterEqual( "age", 18 );
var pagingPredicate = new PagingPredicate(pageSize:5, predicate:greaterEqual);
// Retrieve the first page
var values = map.Values( pagingPredicate );
//...
// Set up next page
pagingPredicate.NextPage();
// Retrieve next page
var values = map.Values( pagingPredicate );

//...
```

If you want to sort the result before paging, you need to specify a comparator object that implements the `System.Collections.Generic.IComparer<KeyValuePair<object, object>>` interface.
Also, this comparator class should implement' one of `IIdentifiedDataSerializable` or `IPortable`. After implementing this class in .NET,
you need to implement the Java equivalent of it and its factory. The Java equivalent of the comparator should implement `java.util.Comparator`.
Note that the `Compare` function of `Comparator` on the Java side is the equivalent of the `sort` function of `Comparator` on the .NET side.
When you implement the `Comparator` and its factory, you can add them to the `CLASSPATH` of the server side.
See the [Adding User Library to CLASSPATH section](#1212-adding-user-library-to-classpath).

Also, you can access a specific page more easily with the help of the `Page` property. This way, if you make a query for the 100th page,
for example, it will get all 100 pages at once instead of reaching the 100th page one by one using the `NextPage` function.

## Fast-Aggregations

Fast-Aggregations feature provides some aggregate functions, such as `sum`, `average`, `max`, and `min`, on top of Hazelcast `IMap` entries.
Their performance is perfect since they run in parallel for each partition and are highly optimized for speed and low memory consumption.

The `Aggregators` class provides a wide variety of built-in aggregators. The full list is presented below:

* count
* bigInteger sum/avg/min/max
* double sum/avg/min/max
* integer sum/avg/min/max
* long sum/avg/min/max
* number avg
* fixedPointSum, floatingPointSum

You can use these aggregators with the `IMap.Aggregate(aggregator)` and `IMap.Aggregate(aggregator, predicate)` methods.