# Dynamox

Write logic, not lambdas

## Introduction

Dynamox is a fresh look at mocking for .Net unit tests. It is aimed at all levels of programmers, from those new to unit testing and mocking, to those who are frustrated with the verbocity of existing mocking tools.

Dynamox reduces the amount of code you need to write in order to generate simple or complex mocks.

* [Introduction to mocking](#introduction-to-mocking)
* [How Dynamox is different](#how-dynamox-is-different)
* [Examples](#examples)
  * [Matching arguments](#matching-arguments)

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
userRepositoryMock.GetEntityById(123).Returns(user);

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
mock.SetEntityName(123, "John");

// use all arguments
mock.SetEntityName(Dx.Any, Dx.Any);

// check arguments programatically
mock.SetEntityName(Dx.Method<int, string>((id, name) =>
{
    return id == 123 && name == "John";
}));
```

