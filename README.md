# Dynamox

Write logic, not lambdas

## Introduction

Dynamox is a fresh look at mocking for .Net unit tests. It is aimed at all levels of programmers, from those new to unit testing and mocking, to those who are frustrated with the verbocity of existing mocking tools.

Dynamox reduces the amount of code you need to write in order to generate simple or complex mocks.

* [Introduction to mocking](#introduction-to-mocking)
* [How Dynamox is different](#how-dynamox-is-different)
* [Examples](#examples)
  * [Creating Mocks](#creating-mocks)
  * [Matching arguments](#matching-arguments)
  * [Returning Values](#returning-values)
  * [Chaining mocks](#chaining-mocks)
  * [Mocking fields and properties](#mocking-fields-and-properties)
  * [Ensure methods are called](#ensure-methods-are-called)
  * [Method Callbacks](#method-callbacks)
  * [Property Callbacks](#property-callbacks)
  * [Dictionaries and Indexes](#dictionaries-and-indexes)
  * [Out and Ref values](#out-and-ref-values)
  * [Partial Mocks](#partial-mocks)
  * [Constructor args](#constructor-args)
  * [Constructor args in chained mocks](#constructor-args-in-chained-mocks)
  * [Structs (value types) and sealed classes](#structs-value-types-and-sealed-classes)
  * [Reserved Terms](#reserved-terms)
* [Contribute](#contribute)

## Introduction to mocking

Mocking is a unit testing concept, which allows you to substitue some to the objects in your application for fake objects with custom functionality. For example, within a unit test, you may wish to substitute your Database objects with something which appears like a Database or Repository, but actualy returns values specific to the particular test.

```C#
// Arrange
var userRepositoryMock = Dx.Mock();
var user = new User();

// mock the GetEntityById method, so that it returns a user
// when the id is 123
userRepositoryMock.GetEntityById(123).Returns(user);

var testSubject = new UserService(userRepositoryMock.As<IUserRepository>());

// Act
testSubject.SetUserName(123, "John");

// Assert
Assert.AreEqual("John", user.Name);
```

## How Dynamox is different
Unit testing is a long and often boring task. It is thankless and time consuming. Dynamox is a reaction to this. Rather than attempting to make unit testing sexy (it isn't..... it REALLY isn't), dynamox is built to reduce the amount of code you have to write in order to test, giving you faster turnaround time and tests which look more similar to the code which they are testing.

Dynamox does this by sacrificing strong typing, so that you can work within the bounds of the logic you wish to execute, not the classes you wish to look like.

Lets have a closer look at the example in the introduction to mocking section.

```C#
// create a dynamic mock. This mock has no type
// so we can append whatever functionality we want to it
var userRepositoryMock = Dx.Mock();
var user = new User();

// calling a method on a dynamic mock will start to build
// it's dynamic functionality. Calling the Returns(...) method
// specifies what to return if the method is called
userRepositoryMock.GetUserById(123).Returns(user);

// the actual mocking is done using the As<T>() method.
// this creates a proxy type which includes the mocked functionalty
var testSubject = new UserService(userRepositoryMock.As<IUserRepository>());

...
```

Mocking like this will allow us to write far more complex mocks far faster than with other mocking frameworks

## Examples

### Creating mocks
Creating mocks is a two step process
```C#
// first you create a mock builder. This
// mock builder has no type and acts as a
// container for all of your mocked logic and properties
var mock = Dx.Mock();

// second, you create a mock from the builder.
// with the As<T>() method
IUserService userService = mock.As<IUserService>();

// you can use a mockbuilder for as may mock types
// as you need, although 1 mock builder per mock is
// generally advisable
IDataService dataService = mock.As<IDataService>();

// Created mocks are cached. This means that if you attempt
// to create a second mock of a given type, from a single builder,
// those objects will have the same object reference
IDataService userService2 = mock.As<IUserService>();

Assert.AreEqual(userService, userService2);	// true
Assert.AreNotEqual(userService, dataService);	// true
```

### Matching arguments

```C#
var mock = Dx.Mock();

// specify arguments explicitly
mock.SetUserName(123, "John");

// use any arguments
mock.SetUserName(Dx.Any, Dx.Any);

// use any arguments with a given type
mock.SetUserName(Dx.AnyT<int>(), Dx.AnyT<string>());

// check arguments programatically
mock.SetUserName(Dx.Args<int, string>((id, name) =>
{
    return id == 123 && name == "John";
}));
```

### Returning Values
```C#
var mock = Dx.Mock();
var user = new User();

// To return values, simply call the Returns(...) method 
// and pass in the return value
mock.GetUser(123).Returns(user);
```

### Chaining mocks
Often with legacy projects and projects which rely heavily on the factory pattern, you may have a mock within a mock. This is very easy to do with dynamox

```C#
var factoryMock = Dx.Mock();

// To chain mocks, simply call the methods expected in the chain
factoryMock.GetUserService().SetUserName(123, "John");

// this code will create 2 mocks, one for the factory and one for the user service.
// it will then set the GetUserService() method of the factory mock to return the user service mock.
```
Alternatively you can write the above code like so
```C#
var userServiceMock = Dx.Mock();
userServiceMock.SetUserName(123, "John");

var factoryMock = Dx.Mock();
factoryMock.GetUserService().Returns(userServiceMock);
```

### Mocking fields and properties

Fields and properties can be mocked by setting them on the mock.
```C#
var mock = Dx.Mock();
var user = new user();
mock.CurrentUser = user;
```
Nested fields and properties are also allowed
```C#
var mock = Dx.Mock();
var userId = 123;
mock.CurrentUser.UserId = userId;
```
If a property is `abstract`, `virtual` or belongs to an `interface`, it will be mocked. Other properties or fields will have their values set in the constructor of the Mock.

`internal` and `private` values cannot be mocked.

### Ensure methods are called
If you want to ensure that a specific method was called during a test, use the Ensure(...) method
```C#
var databaseMock = Dx.Mock();

// mock the PersistAll method and ensure that it is called
databaseMock.PersistAll().Ensure();

// Test that the PersistAll method was called
Dx.Ensure(databaseMock);
```

### Method Callbacks
In order to run some code when a method is called, use the Do(...) method
```C#
var mock = Dx.Mock();
var user = new User();
mock.SetUserName(Dx.Any, Dx.Any).Do(Dx.Callback<int, string>((id, userName) =>
{
    user.UserName = userName;
}));
```
Alternatively, you do not need to include all of the arguments of the function in a Do(...) statement. You only need to include the arguments you will use.
```C#
var mock = Dx.Mock();
var user = new User();
mock.SetUserName(Dx.Any, Dx.Any).Do(Dx.Callback<int>((id) =>
{
    Console.WriteLine("Edit username for user " + id);
}));
```
### Property Callbacks
You can attatch functionalty to a property also. You may only attatch functionality to properties which are `virtual`, `abstract` or part of an `interface`.
```C#
var mock = Dx.Mock();
var user = new User();

// get user value dynamically
mock.CurrentUser = Dx.Property(() => user);

// user get and set callbacks
mock.CurrentUser = Dx.Property(user)
    .OnGet(u =>
    {
        Console.WriteLine("Getting user");
    })
    .OnSet((oldValue, newValue) =>
    {
        Console.WriteLine("Setting user");
    });
```

### Dictionaries and Indexes
Dictionaries and indexes behave in much the same way as properties
```C#
var mock = Dx.Mock();

mock["Val1"] = 123;
mock["Val2"] = Dx.Property(() => 234); // see the Property Callbacks section

IDictionary<string, int> dictionary = mock.As<IDictionary<string, int>>();
```

###Out and Ref values
Specify out and ref values with the `Out(...)` method.
```C#
var mock = Dx.Mock();

// the first parameter is the index of the out parameter, the second parameter is the value
mock.GetUserName(123, Dx.Any).Out(1, "John");

// -OR - the first parameter is the name of the out parameter, the second parameter is the value
mock.GetUserName(123, Dx.Any).Out("name", "John");

string name;
mock.As<IUserService>().GetUserName(123, out name);

Assert.AreEqual("John", name);
```


###Partial Mocks
Partial mocks are mocked exactly the same as interfaces.

###Constructor args
```C#
// specify constructor args when defining the mock builder
var mock = Dx.Mock(new object[] { "arg1", 2 });

var objectWithConstructorArgs = mock.As<ObjectWithConstructorArgs>();
```

###Constructor args in chained mocks
Specify constructor args with the `Constructor(..)` function
```C#
var mock = Dx.Mock();

// for properties
mock.ObjectWithConstructorArgs.Constructor(new object[] { "arg1", 2 }).ToString().Returns("Hello!");
// and functions
mock.GetObjectWithConstructorArgs().Constructor(new object[] { "arg1", 2 }).ToString().Returns("Hello!");
```

###Structs (value types) and sealed classes
Structs and sealed classes can be mocked in the same way as interfaces and non sealed classes, however, the implementation will be slightly different. Rather then create a proxy for the class, a mock of a sealed class or struct will be an instance of that class or struct with the mocked properties set, if possible.

### Reserved Terms
There are several terms used by Dynamox for mocking functionality. These are:
* Returns(...)
* Ensure(...)
* Clear(...)
* Do(...)
* As(...)
* Constructor(...)
* Out(...)

These function names may clash with the function names of the class you are mocking. If this occurs you can temporarily change the name of the mocked function.

```C#
var mock = Dx.Mock();
var user = new User();

// Example 1: do not change mock function name
mock.GetUser(123).Retuns(user);

// Example 2: alter the name of the returns method
mock(new { Returns = "Returns_New" }).GetUser(123).Retuns_New(user);

// Example 3: alter the name of the returns method (strongly typed)
mock(new MockSettings { Returns = "Returns_New" }).GetUser(123).Retuns_New(user);
```

## Contribute

Contribute in (almost) any way you would like
1. Fork it.
2. Ask questions by [logging an issue](https://github.com/ShaneGH/Dynamox/issues/new) using the "help wanted" tag.
3. Request features by [logging an issue](https://github.com/ShaneGH/Dynamox/issues/new) using the "enhancement" tag.
4. Log bugs by [logging an issue](https://github.com/ShaneGH/Dynamox/issues/new) using the "bug" tag.
5. Coming soon: code contributions and pull requests. Once we get an alpha release out we can begin to accept pull requests