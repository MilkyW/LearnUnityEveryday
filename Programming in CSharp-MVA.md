# 201303_Programming in CSharp_MVA

![Programming-in-CSharp Agenda](https://github.com/MilkyW/UnityFEIntern/blob/master/Pictures/Programming-in-CSharp%20Agenda.png?raw=true)

http://blog.jerrynixon.com/2014/01/free-c-training-for-beginners-and.html

## 01-Object Oriented Programming; Managed Languages and C#
**Object Oriented Programming:** certain properties and behaviours, features supported by the language

**Object:** “is” something, “has” data, “performs” actions; don’t have data yet, but the container for the data

**Encapsulation:** wall around the code, black box, interact with interface; contract of the behaviour, not bothered by the implementation details; protected and safe inside; to create a boundary around an object to separate its external (public) behavior from its internal (private) implementation; what an object does, not how it does it

**Inheritance:** code reuse, obtain qualities(properties and methods, signatures inside the objects) through the genetic inheritance chain, whatever the parent has the children will have by default

**Polymorphism:** have many forms(Greek), objects can become other objects
What is a Managed Language? depend on services provided by a runtime environment, compile into managed code, executed by the Common Language Runtime(CLR, managed runtime environment int .NET); constantly bouncing usability, programmer, productivity, performance, constant tension make it a general purpose language 

**CLR provides features:** automatic memory management, exception handling, standard types, security
**standard types:** Metadate about space allocation and compile-time type checking(IntelliSense in VS); have common base Object, can use inherited methods like ToString.

**3 categories of types:**

**Value types:** directory store values, simple piece of data,  common->light weight(optimization)

**Reference types(Object instances):** store a reference to data, a pointer points to where the location where the object is

**Pointer types:** only available in unsafe code, can mark a block of code “unsafe” in C#, accessing memory directly for high performance (e.g. bitmap) as opposed to the safe abstractions the C# managed language provides

**Why C# for OOP?**

**C#:** General Purpose, Object Oriented, Focused on dev productivity

**JUMP:** Java User Migration Path

**C# supports Encapsulation via:** unified type system, classed and interfaces, properties, methods and events

**C# implements Inheritance in two ways:** A class may inherit from a single base class; A class may implement zero or more Interfaces

**C# & Polymorphism:** A class can be used as its own type, cast to any base types or interface types it implements. Objects may methods as virtual; virtual methods may be overridden in a derived type. These are executed instead of the base implementation.

![CSharp Evolvement](https://github.com/MilkyW/UnityFEIntern/blob/master/Pictures/CSharp%20Evolvement.png?raw=true)

**Developer Productivity:** From the outset, C# focused on making it easier for developers to solve complex tasks without compromising elegance.

**var** - simplifies variable definition while retaining strong typing

**LINQ** - language integrated query

**Lambdas** - a further refinement of anonymous methods used extensively in LINQs

**C# Syntax**

**Identifiers:** names of classes, methods, variables and so on

**Keywords:** compiler reserved words, public, class, string, get, set, void

**What is Code Decoration?**

**Attributes:** Associate additional metadata to types and members; Discoverable at runtime via reflection

**Comments:** Block/Single line/Code is Self-documenting

**XML documentation:** extracted into separate XML files during compilation, used to generate formal documentation

**Lambda Expressions:** Enhancement of anonymous methods, unnamed method written inline, alternative to a delegate; an instance of a delegate or an expression tree at compile time; validated at compile time and evaluated at run time; dynamic expression trees are supported

**Anonymous Types:** Types without a design-time definition - used extensively with LINQ; Compiler inferred types; but not portable

**Extension Methods:** extend types without altering them, especially useful for extending classes that are not yours or are sealed; declared as static methods in a static class(static: cannot manipulate the private method, thus doesn’t break the encapsulation); first parameter has the this modifier, the first parameter is the type being extended

**Dynamics:** Instructs the compiler to ignore type checking at compile-time and defers it until runtime; Simplifies interaction with COM interfaces/un-typed, external objects

## 02-Constructing Complex Types, Object Interfaces and Inheritance

**Classes and Structs:** defines the template for an object(instance); class(reference) & struct(value), imply memory strategies

**Structs:** instances of the type are small, is commonly embedded in another type,  logically represent a single value which doesn’t change (immutable), is rarely “boxed”; have performance benefits in computational intensive applications

**Classes can optionally be declared as:**

static - cannot be instantiated, stateless, only a library of activities without the cost of instantiating it

abstract - incomplete class, cannot be instantiated; must be completed in a derived class

sealed - cannot be inherited from, all classes can be inherited unless marked as sealed

partial - one class spread in more than one files

**Method can optionally be declared as:**

private - only by this class, by default

protected - only by this class and its derived classes

public - by any class

internal - only in this assembly(solution in VS)

**Properties:** shorthand get & set instructs the compiler to create an infrastructure(backing variable/store) to manipulate and store the value of the variable; can put access modifier like “private” before get & set

**Signatures:** defaults can use parameter name to bypass(doesn’t care about order)

**Events:** the way for the class or object to talk back to the developer(not invoked by the developer), raise an event and handle it; publish and subscribe pattern; EventHandler & EventArgs

**Object Interfaces and Inheritance:** Interface is a contract promising the implementation and Class implements the functionality.

**Inheritance:** virtual methods have implementations and can be overridden in derived class

**Creating Object Instances:** When a class or struct is created, a constructor is called. Unless a class is static, the compiler generates a default constructor if not supplied in code. Constructors are invoked by using the new keyword. Constructors may require parameters. More than one constructor can be defined. Constructors can be chained together. Base class constructors are always called first.

**Generics:** introduce to the .NET Framework the concept of type parameters; make it possible to design classes an methods that defer type specifications until the class or method is declared; one of the most common situations for using generics is when a strongly-typed collection is required - lists, hash tables, queues, etc.

**When creating a generic class, consider:** Which type to generalize; What constraints to apply; Whether to create generic base classes; Whether to create generic interfaces.
Generic methods may also be created within non-generic classes.

```cs
int count = 1;

// the value of count is copied and boxed
object countObject = count;

count += 1; // countObject is still 1

// the value of countObject (1) is unboxed
// and copied to count
count = (int)countObject;
```

**Boxing/Unboxing:** Boxing is the act of converting a value type to a reference type. Unboxing is the reverse, requires a cast. Boxing/Unboxing copies the value. Boxing is computationally expensive - avoid repetition. Generics help avoid these scenarios.

