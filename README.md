# AnonymousInterfaces

A fluent API for dynamically creating an anonymous type implementing a specific interface.

## Supported Platforms

.NET 4.0 and higher. Silverlight can be added if someone wants it.

Unfortunately, Windows Store and Windows Phone platforms are not currently supported.

## Quick Start

Download via NuGet: [Nito.AnonymousInterfaces](http://nuget.org/List/Packages/Nito.AnonymousInterfaces).

The namespace:

````C#
using AnonymousInterfaces;
````

Start with:

````C#
IMyInterface instance = Anonymous.Implement<IMyInterface>()
    ... // cool stuff goes here
    .Create();
````

Then stick in the "cool stuff" you need (examples are below; let the XML documentation guide you).

The `Create` method will create a new _type_ and then create and return an _instance_ of that type.

You can pass an `IMyInterface` object in to the `Implement` method, and any methods you don't override with your "cool stuff" will get forwarded to that object.

If you don't supply an implementation for an interface member, it will be implemented as throwing `NotImplementedException`. So if you just do `Anonymous.Implement<IMyInterface>().Create()`, you'll get an implementation that throws `NotImplementedException` for everything.

## Property getters and setters

````C#
interface IMyInterface
{
    int Value { get; set; }
}

IMyInterface instance = Anonymous.Implement<IMyInterface>()
    .PropertyGet(x => x.Value, () => { return ...; })
    .PropertySet(x => x.Value, value => { ... })
    .Create();
````

This works as long as there is a property getter.

## Methods

````C#
interface IMyInterface
{
    void Test();
}

IMyInterface instance = Anonymous.Implement<IMyInterface>()
    .Method<Action>(x => x.Test, () => { ... })
    .Create();
````

This works for most methods, including `params` methods and parameters with default values; if you need to return a value, just use `Func` instead of `Action`. For complex scenarios, see below.

## Index getters and setters

````C#
interface IMyInterface
{
    string this[int index] { get; set; }
}

IMyInterface instance = Anonymous.Implement<IMyInterface>()
    .IndexGet<Func<int, string>>(index => { return ...; })
    .IndexSet<Action<int, string>>((index, value) => { ... })
    .Create();
````

## Events

````C#
interface IMyInterface
{
    event Action OnWhatever;
}

IMyInterface instance = Anonymous.Implement<IMyInterface>()
    .EventSubscribe<Action>("OnWhatever", value => { ... })
    .EventUnsubscribe<Action>("OnWhatever", value => { ... })
    .Create();
````

## Set-only properties (no getters)

````C#
interface IMyInterface
{
    int Value { set; }
}

IMyInterface instance = Anonymous.Implement<IMyInterface>()
    .PropertySet<int>("Value", value => { ... })
    .Create();
````

## Methods with `out` or `ref` parameters

````C#
interface IMyInterface
{
    void Test(out int arg);
}

// Have to define your own matching delegate type
private delegate void TestDelegate(out int arg);

IMyInterface instance = Anonymous.Implement<IMyInterface>()
    .Method<TestDelegate>(x => x.Test, (out int arg) => { arg = ... })
    .Create();
````

## Methods hidden in base interfaces

````C#
interface IMyBaseInterface
{
    void Test();
}

interface IMyInterface : IMyBaseInterface
{
    new void Test();
}

IMyInterface instance = Anonymous.Implement<IMyInterface>()
    .Method<Action>(x => ((IMyBaseInterface)x).Test, () => { ... })
    .Create();
````

## Generic methods

Generic methods require an implementation of the interface, because there's no way to construct an open generic delegate type:

````C#
interface IMyInterface
{
    void Test<T>();
    // other members
}

private sealed class MyImplementation : IMyInterface
{
    void Test<T>() { ... }
    // usually just throws NotImplementedException for other members
}

IMyInterface implementation = new MyImplementation();
IMyInterface instance = Anonymous.Implement<IMyInterface>(implementation)
    ... // other interface members overridden here
    .Create();
````
