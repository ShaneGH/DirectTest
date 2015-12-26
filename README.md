# Dynamox

Write logic, not lambdas

## Introduction

Dynamox is a fresh look at mocking for .Net unit tests. It is aimed at all levels of programmers, from those new to unit testing and mocking, to those who are frustrated with the verbocity of existing mocking tools.

Dynamox reduces the amount of code you need to write in order to generate simple or complex mocks.

* [Introduction to mocking](#introduction-to-mocking)
* [How Dynamox is different](#how-dynamox-is-different)
* [Examples](#examples)
  * [Matching arguments](#matching-arguments)
  * [Returning Values](#returning-values)
  * [Mocking fields and properties](#mocking-fields-and-properties)

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

// the actual mocking is done with the As<T>() method.
// this creates a proxy type which includes the mocked functionalty
var testSubject = new UserService(userRepositoryMock.As<IUserRepository>());

...
```

Mocking like this will allow us to write far more complex mocks far faster than with other mocking frameworks

## Examples

## Matching arguments

```C#
var mock = Dx.Mock();

// specify arguments explicitly
mock.SetUserName(123, "John");

// use all arguments
mock.SetUserName(Dx.Any, Dx.Any);

// check arguments programatically
mock.SetUserName(Dx.Method<int, string>((id, name) =>
{
    return id == 123 && name == "John";
}));
```

##Returning Values
```C#
var mock = Dx.Mock();
var user = new User();

// To return values, simply call the Returns(...) method 
// and pass in the return value
mock.GetUser(123).Returns(user);
```

##Chaining mocks
Often with legacy projects and projects wich rely heavily on the factory pattern, you may have a mock within a mock. This is very easy to do with dynamox

```C#
var factoryMock = Dx.Mock();

// To chain mocks, simply call the methods expected in the chain
factoryMock.GetUserService().SetUserName(123, "John");

// this code will create 2 mocks, one for the factory and one for the user service.
// it will then set the GetUserService() method of the factory mock to return the user service mock.
```
Alternatively you can write the above code like so
```C#
var factoryMock = Dx.Mock();
var userServiceMock = Dx.Mock();

factoryMock.GetUserService().Returns(userServiceMock);
userServiceMock.SetUserName(123, "John");
```

## Mocking fields and properties

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
If a property is `abstract`, `virtual` or belongs to an interface, it will be mocked. Other properties or fields will have their values set in the constructor of the Mock.

`internal` and `private` values cannot be mocked.
