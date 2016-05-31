# Dynamox

[![Build status](https://ci.appveyor.com/api/projects/status/4u4q28v9cd9qgw18?svg=true)](https://ci.appveyor.com/project/ShaneGH/dynamox)

Write logic, not lambdas

## Introduction

Dynamox is a fresh look at mocking for .Net unit tests. It is aimed at all levels of programmers, from those new to unit testing and mocking, to those who are frustrated with the verbocity of existing mocking tools.

Dynamox reduces the amount of code you need to write in order to generate simple or complex mocks.

* [Introduction to mocking](#introduction-to-mocking)
* [How Dynamox is different](#how-dynamox-is-different)
* [Philosophy](#philosophy)
* [Contribute](#contribute)
* [Examples](#examples)
  * [Creating Mocks](#creating-mocks)
  * [Matching arguments](#matching-arguments)
  * [Returning Values](#returning-values)
  * [Chaining mocks](#chaining-mocks)
  * [Mocking fields and properties](#mocking-fields-and-properties)
  * [Chaining mocks with properties](#chaining-mocks-with-properties)
  * [Ensure methods and properties are used](#ensure-methods-and-properties-are-used)
  * [Method Callbacks](#method-callbacks)
  * [Property Callbacks](#property-callbacks)
  * [Dictionaries and Indexes](#dictionaries-and-indexes)
  * [Out and Ref values](#out-and-ref-values)
  * [Partial Mocks](#partial-mocks)
  * [Constructor args](#constructor-args)
  * [Constructor args in chained mocks](#constructor-args-in-chained-mocks)
  * [Events](#events)
  * [Structs (value types) and sealed classes](#structs-value-types-and-sealed-classes)
  * [Reserved Terms](#reserved-terms)

## Introduction to mocking

Mocking is a unit testing concept, which allows you to substitue some to the objects in your application for fake objects with custom functionality. For example, within a unit test, you may wish to substitute your Database objects with something which appears like a Database or Repository, but actualy returns values specific to the particular test.

```C#
// Arrange
var userRepositoryMock = Dx.Mock();
var user = new User();

// mock the GetEntityById method, so that it returns a user
// when the id is 123
userRepositoryMock.GetEntityById(123).DxReturns(user);

var testSubject = new UserService(userRepositoryMock.DxAs<IUserRepository>());

// Act
testSubject.SetUserName(123, "John");

// Assert
Assert.AreEqual("John", user.Name);
```

## How Dynamox is different

Unit testing is a long and boring task. It is thankless and time consuming. Dynamox is a reaction to this. Rather than attempting to make unit testing sexy (it isn't..... it REALLY isn't), dynamox is built to reduce the amount of code you have to write in order to test, giving you faster turnaround time and test code which flows better an more naturally than it could within the bounds of strong typing and .Net expressions.

Dynamox is dynamic. It sacrifices strong typing, so that you can work within the bounds of the logic you wish to execute, not the classes you wish to look like.

Lets have a closer look at the example in the introduction to mocking section.

```C#
// create a dynamic mock. This mock has no type
// so we can append whatever functionality we want to it
var userRepositoryMock = Dx.Mock();
var user = new User();

// calling a method on a dynamic mock will start to build
// it's dynamic functionality. Calling the DxReturns(...) method
// specifies what to return if the method is called
userRepositoryMock.GetUserById(123).DxReturns(user);

// the actual mocking is done using the DxAs<T>() method.
// this creates a proxy type which includes the mocked functionalty
var testSubject = new UserService(userRepositoryMock.DxAs<IUserRepository>());

...
```

Mocking like this will allow us to write far more complex mocks far faster than with other mocking frameworks

## Philosophy

* Dynamox is productive. You write code which explains exactly what you want to do, no more expressions and natural language, just code. Simple code.
* Dynamox is magic! Watch properties and methods appear from nowhere.
* Dynamox is dynamic. You mock what you want to mock, you ignore everything else.
* Dynamox is expressive. You have complete power over mocks and functions.
* Dynamox is fast. Dynamox has a new proxy generation engine which is tuned for mocking only.

## Contribute

Contribute in any way you would like

1. Fork it.
2. Submit pull requests.
3. Ask questions by [logging an issue](https://github.com/ShaneGH/Dynamox/issues/new) using the "help wanted" tag.
4. Request features by [logging an issue](https://github.com/ShaneGH/Dynamox/issues/new) using the "enhancement" tag.
5. Log bugs by [logging an issue](https://github.com/ShaneGH/Dynamox/issues/new) using the "bug" tag.

## Examples

### Creating mocks
Creating mocks is a two step process
```C#
// first you create a mock builder. This
// mock builder has no type and acts as a
// container for all of your mocked logic and properties
var mock = Dx.Mock();

// second, you create a mock from the builder.
// with the DxAs<T>() method
IUserService userService = mock.DxAs<IUserService>();

// you can use a mockbuilder for as may mock types
// as you need, although 1 mock builder per mock is
// generally advisable
IDataService dataService = mock.DxAs<IDataService>();

// Created mocks are cached. This means that if you attempt
// to create a second mock of a given type, from a single builder,
// those objects will have the same object reference
IDataService userService2 = mock.DxAs<IUserService>();

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

// To return values, simply call the DxReturns(...) method
// and pass in the return value
mock.GetUser(123).DxReturns(user);
```

### Chaining mocks
Chaining mocks is one of the core concepts of dynamox. Often with legacy projects and projects which rely heavily on the factory pattern, you may have a mock within a mock. This generally leads to you having to write your tests in reverse order to how your code is written, as you create the inner most nested objects first, working outwards to the test method argument. Dynamox fixes this and removes all of the code bloat along the way.

```C#
var factoryMock = Dx.Mock();

// To chain mocks, simply call the methods expected in the chain
factoryMock.GetUserService().SetUserName(123, "John");
```
The above code is equivalent to:
```C#
var userServiceMock = Dx.Mock();

var factoryMock = Dx.Mock();
userServiceMock.SetUserName(123, "John");
factoryMock.GetUserService().DxReturns(userServiceMock);
```
And like magic, Dynamox has turned 3 lines of code into 1 

### Mocking fields and properties

Fields and properties can be mocked by setting them on the mock.
```C#
var mock = Dx.Mock();
var user = new User();
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

### Chaining mocks with properties

You can combine fields and propertes with method calls to chain anything you want!
```C#
var userContextMock = Dx.Mock();

// mock the CurrentUser property and the Logout() method with a return value of true
userContextMock.CurrentUser.Logout().DxReturns(true);
```

### Ensure methods and properties are used
If you want to ensure that a specific method was called during a test, use the DxEnsure(...) method
```C#
var databaseMock = Dx.Mock();

// mock the PersistAll method and ensure that it is called
databaseMock.PersistAll().DxEnsure();

// Create the mocked object and invoke the PersistAll method
var database = databaseMock.DxAs<IDatabase>();
database.PersistAll();

// Test that the PersistAll method was called
Dx.Ensure(database);

// you can also test the PersistAll method on the mock object instance
Dx.Ensure(databaseMock);
```

You can also ensure that properties and indexed properties are accessed.

```C#
var userContextMock = Dx.Mock();

// mock the CurrentUser property and ensure that it is accessed
userContextMock.CurrentUser = Dx.Property(new User()).DxEnsure();

// mock the Permissions["Read"] indexed property and ensure that it is accessed
userContextMock.Permissions["Read"] = Dx.Property(true).DxEnsure();

// create the mock and access it's properties
var userContext = userContextMock.DxAs<IUserContext>();
var currentUser = userContextMock.CurrentUser;
var canRead = userContextMock.Permissions["Read"];

// Test that the CurrentUser and Permissions["Read"] properties were accessed
Dx.Ensure(userContext);
```

### Method Callbacks
In order to run some code when a method is called, use the DxDo(...) method
```C#
var mock = Dx.Mock();
var user = new User();
mock.SetUserName(Dx.Any, Dx.Any).DxDo(Dx.Callback<int, string>((id, userName) =>
{
    user.UserName = userName;
}));
```
Alternatively, you do not need to include all of the arguments of the function in a Do(...) statement. You only need to include the arguments you will use.
```C#
var mock = Dx.Mock();
var user = new User();
mock.SetUserName(Dx.Any, Dx.Any).DxDo(Dx.Callback<int>((id) =>
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
mock[Dx.Any] = 456; // return 456 for any request for an indexed property
mock[Dx.AnyT<string>()] = 456; // return 456 for any request for an indexed property with an index of type string

IDictionary<string, int> dictionary = mock.DxAs<IDictionary<string, int>>();
```

###Out and Ref values
Specify `out` and `ref` values with the `DxOut(...)` method.
```C#
var mock = Dx.Mock();

// the first parameter is the index of the out parameter, the second parameter is the value
mock.GetUserName(123, Dx.Any).DxOut(1, "John");

// -OR - the first parameter is the name of the out parameter, the second parameter is the value
mock.GetUserName(123, Dx.Any).DxOut("name", "John");

string name;
mock.DxAs<IUserService>().GetUserName(123, out name);

Assert.AreEqual("John", name);
```


###Partial Mocks
Partial mocks are mocked exactly the same as interfaces.
```C#
var mock = Dx.Mock();
mock.GetUserFirstName(123).DxReturns("John");

UserService service = mock.DxAs<UserService>();
var userName = service.GetFullUserName(123);
```

###Constructor args
```C#
// specify constructor args when defining the mock builder
var mock = Dx.Mock(new object[] { "arg1", 2 });

var objectWithConstructorArgs = mock.DxAs<ObjectWithConstructorArgs>();
```

###Constructor args in chained mocks
Specify constructor args with the `DxConstructor(..)` function
```C#
var mock = Dx.Mock();

// for properties
mock.ObjectWithConstructorArgs.DxConstructor("arg1", 2).ToString().DxReturns("Hello!");
// and functions
mock.GetObjectWithConstructorArgs().DxConstructor("arg1", 2).ToString().DxReturns("Hello!");
```

###Events
```C#
var mock = Dx.Mock();

// subscribe to the OnUserAdded event
mock.OnUserAdded += Dx.EventHandler<object, UserAddedEventArgs>((sender, args) =>
{
    // event was raised
});

// create a concrete type
UserRepository repository = mock.DxAs<UserRepository>();

// raise event 1, (assuming that there is an add user method and it raises the OnUserAdded event)
repository.AddUser(new User("John"));

// raise event 2, using mock
bool eventFound2 = Dx.RaiseEvent(mock, "OnUserAdded", repository, new UserAddedEventArgs());

// raise event 3, using concrete object
bool eventFound3 = Dx.RaiseEvent(repository, "OnUserAdded", repository, new UserAddedEventArgs());
```

Events can only be raised if they are abstract, virtual or part of an interface.


###Structs (value types) and sealed classes
Structs and sealed classes can be mocked in the same way as interfaces and non sealed classes, however, the implementation will be slightly different. Rather then create a proxy for the class, a mock of a sealed class or struct will be an instance of that class or struct with the mocked properties set, if possible.

### Reserved Terms
There are several terms used by Dynamox for mocking functionality. These are:
* DxReturns(...)
* DxEnsure(...)
* DxClear(...)
* DxDo(...)
* DxAs(...)
* DxConstructor(...)
* DxOut(...)

These function names may clash with the function names of the class you are mocking. If this occurs you can temporarily change the name of the mocked function.

```C#
var mock = Dx.Mock();
var user = new User();

// Example 1: do not change mock function name
mock.GetUser(123).DxReturns(user);

// Example 2: alter the name of the returns method
mock(new { DxReturns = "Returns_New" }).GetUser(123).Returns_New(user);

// Example 3: alter the name of the returns method (strongly typed)
mock(new ReservedTerms { DxReturns = "Returns_New" }).GetUser(123).Returns_New(user);
```

Note, if two terms clash (1 from Dynamox and one from your mocked class), the Dynamox term will take precendence.

You can also permanently change these terms.
```C#
ReservedTerms.Default.DxReturns = "Returns_New";
```
