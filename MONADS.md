# Monads

They're just monoids in the category of endofunctors, what's the big deal?

If anyone ever uses the above sentence with a straight face, flee the vicinity on foot and alert the authorities.  The person who said it is not your friend.  They mean you harm.

## A Brief Refresher on Data Structures and Abstraction

For anyone who doesn't remember, a data structure is any collection of data that moves as one, along with any attached methods.  In C#, there are tons of data structures of all different shapes and sizes.  `int` is a data structure (albeit a very small one).  So is `string`.  So is `Task<T>`, `List<T>`, `Exception`, `CancellationTokenSource`, and `HttpClient`.

Data structures exist to provide abstractions over various operations.  The primitive numerical types provide abstractions over mathematical operations (meaning you don't have to handle the complexity of two's complement math in binary or know about the specifics of carry flags and all the other weird optimizations that modern processors perform to do math quickly), you just write math the way humans are used to reading it and the compiler/runtime handle things from there.  Collection types (like `List<T>`, `Dictionary<TKey, TValue>` and the others) provide abstractions over storage and searching operations (meaning you don't have to worry about how entries are held in memory or have to write search algorithms yourself every time you need a different collection).

Abstractions are important for hiding unnecessary complexity.  Everything about a computer is built on one kind of abstraction or another, both physically and in code.  After all, when was the last time you built your own transistors and fashioned a fully-functional CPU?

A monad is just another data structure, and it abstracts function composition.  That's it.  You don't need category theory, you don't need to understand sets, and you need absolutely zero weird math to make productive use of monads.  If you're really interested, by all means go learn about it, but in the realm of Getting Shit Done With Computers, the theoretical underpinnings are not in any way important.  Do you spend a thought on the resistance of the various circuits inside your CPU?  Of course not, they're immaterial to how you make things work.

## A Monad's Contract

Different data structures have different contracts.  In C#, an interface can be thought of as a contract.  Any type that implements an interface is guaranteed to have the methods specified in the contract.  The `System.Collections.Generic.List<T>` class, for example, implements the `ICollection<T>`, `IEnumerable<T>`, `IList<T>`, `IReadOnlyCollection<T>`, `IReadOnlyList<T>`, and `IList` interfaces, meaning that it is guaranteed to have the functions specified in those interfaces.

A monad is the same way.  It's any type that implements a specific set of contracts.  At the most basic level, every monad has to have a `bind` function and a `map` function.  The names aren't necessarily important, but what they does is.  In C#, they would look something like this:

```c#
public interface IMonad<T>
{
  IMonad<TNext> Ap<T, TNext>(IMonad<Func<T, TNext>> morphism);
  IMonad<TNext> Bind<T, TNext>(Func<T, IMonad<TNext>> morphism);
  IMonad<TNext> Map<T, TNext>(Func<T, TNext> morphism);
}
```

The `ap` function applies the wrapped function (which transforms a `T` to a `TNext`) to the value inside the monad.  We're not going to use it in this primer, but it's a required part of the contract.

The `bind` function applies the value inside the monad to a function that returns another monad (this time of type `TNext`), then hands that new monad back out.  This is the important part.  A wrapped value is unwrapped, transformed into a new wrapped value, and returned.

The `map` function applies the value inside the monad  to a function that transforms the value from a `T` to a `TNext`, and then boxes that new value up on behalf of the caller.  This is the important thing.  A wrapped value is unwrapped, transformed into a new unwrapped type, then rewrapped and returned.

## The Simplest Monad

The simplest monad is (in my opinion) is the `Maybe` monad (if you've ever seen the `Option` type in F#, you've seen a `Maybe`).  It either has a value or it doesn't.  It's a better way to represent a missing value than `null`, which can throw exceptions and break the flow of your program.  A `Maybe` monad is disjunctive, meaning it can be in one of two states:  A `Some` or a `Nothing`.  If it's a `Some` (usually called a `Some` or a `Just` in other languages), it is wrapping some value (of type `T` in our case).

Here's a simple example.  It's contrived, but hopefully it'll be illustrative.

```c#
public static string? GetFirstLongString(List<string> stringList)
{
  return stringList.Find(s => s.Length > 100);
}
```

This has a problem.  `List<T>` returns a nullable `T`, which means this method might return `null` if no string with a length greater than 100 characters is found.  That might blow up down the line if someone doesn't account for that possibility.  Nullable types and the associated compiler warnings were an easy way to provide some feedback to the programmer while not having to break backwards compatibility (and all of the existing .NET code out there), but they're not really a _solution_.

Let's try the same thing with a `Maybe`.

```c#
public static Maybe<string> GetFirstLongString(List<string> stringList)
{
  return Maybe.Of(stringList)
    .Map(list => list.Find(s => s.Length > 100));
}
```

`Maybe.Map` takes a `Func<T, U>` and watches for `null` on our behalf.  Now there's no way for a `null` to sneak out and possibly blow up later.  It will either return a `Maybe<string>` with the first long string, or it will return a `Nothing<string>`, which is a kind of sentinel that contains no value.

You've probably noticed that we didn't use the `bind` method in that example.  The `bind` method would probably come in when we _call_ that method, since it returns a monad:

```c#
public static List<string> GetStrings() { ... } // how we get those strings isn't particularly important.

Maybe.Of(GetStrings())
  .Bind(list => GetFirstLongString(list))  // you can shorten this to .Bind(GetFirstLongString)
  .Map(s => s.Length) // this only executes the lambda if we're in a `Some` state
  .Tap(
    length => Console.WriteLine($"the longest string was {length} characters"),  // this lambda is called if we're in a Some state
    () => Console.WriteLine("no string longer than 100 characters was found")    // this lambda is called if we're in a Nothing state
  )
```

So here, we started off by making a `Maybe` of the list of strings we wanted to search, then we called `Maybe.Bind` which handed that list off to the `GetFirstLongString` function.  When that function returned a `Maybe`, `Maybe.Bind` made that the new state of the monad.  And if it returned `Nothing<string>`, the next `Map` would be ignored (because you can't transform nothing!), and the `Nothing` side of the `Maybe.Tap` method (which is a way to hoist side effects into the monadic world) will execute the second function, printing out our message.

It's longer, for sure, but it's safer.  There's no risk of a `null` causing your program to go kaboom, and it allows you to define your workflow as a linear sequence of function calls.  This is what we meant when we said that monads abstract function composition.

## Other monads

There are as many other monads as people want to create.  Some of the canonical ones are `Either<TLeft, TRight>`, `Maybe<T>`, `IO<T>`, and `State<T>`.  They're all useful in different contexts.

## Making `Task` into a monad

This is probably a contentious statement, but as it is, `Task<T>` is not a monad.  There's an argument to be made that because it can be _turned into_ a monad, it must already be one, but that seems like suggesting that because a tree can be turned into planks of wood, it must already be a house.  It's a structural vs functionality argument, and the kind of back-and-forth that programmers can easily fight about until the sun becomes a small, warm object the size of a child's bowling ball.  By the definition we're using, `Task<T>` is not a monad because it doesn't have `ap`, `bind`, or `map` methods.

But we can make them.

This is outside the scope of this primer, but `Task<T>` has a method `ContinueWith` that allows a callback to be attached to the end of a `Task<T>` such that when the `Task<T>` has finished its current asynchronous processing (or if it's already done), another function can be executed asynchronously with the output of the first asychronous operation.  This is a style of programming called continuation passing style (sometimes abbreviated CPS), and _well_ outside the scope of this document.

But the thing about the various programming styles (imperative, functional, CPS), is that they are more or less equivalent.  You can write two programs that do the same thing in two different styles, and they can both perform the same operations, even if the code looks _wildly_ different.  In a sense, they are just syntactic and structural paradigms, not categories of different levels of computational power.

Long story short, you can use C# extension methods to hide contination passing code behind a functional monadic facade.  If you want to see how this is done, go look at [TaskExtensions.cs][].

What kind of code does this allow us to write?  Let's say we have a function that makes an HTTP GET request and returns the content.

-- NOTE:  I'm eliding a lot here.  I know that `HttpClient` instances should come from an `HttpClientFactory`, I know that you should `System.Uri` instead of a `string`, and I know about `System.UriBuilder`.  This is all for the purposes of demonstration. --

```c#
public static async Task<string> MakeGetRequest(string url, CancellationToken cancellationToken = default)
{
  HttpClient client = new();
  Uri getUri = new(url);

  HttpResponseMessage response = await client.GetAsync(getUri, cancellationToken);

  return await response.Content.ReadAsStringAsync(cancellationToken);
}
```

There are several places where an exception could occur here:

1. If `url` is not correctly formatted, the `Uri` constructor could throw.
2. `client.GetAsync` can throw for network connectivity issues or if `cancellationToken` is cancelled.
3. `response.Content.ReadAsStringAsync` can throw if `cancellationToken` is cancelled.

That's not awesome.  And yeah, we could wrap things in `try/catch` blocks, but let's try something different.

```c#
public static async Task<string> MakeGetRequest(string url, CancellationToken cancellationToken = default)
{
  HttpClient client = new();

  return Task.FromResult(url)
    .ResultMap(s => new Uri(s))
    .Bind(uri = client.GetAsync(uri, cancellationToken))
    .ResultMap(response => response.Content)
    .Bind(content => content.ReadAsStringAsyc(cancellationToken))
}
```

-- NOTE:  Because `Task<T>` is disjunctive (that is, it either has a value or an `Exception`, which are colloquially referred to as its right and left sides, respectively), `map` is split into two methods:  `ResultMap`, which only operates on the right side (the same way `map` does) and `ExceptionMap`, which operates on the left side. --

There are some pros and cons to this.  The biggest pro is that any exceptions are trapped and returned as a faulted `Task<T>`.  The cons are that it's longer, and there are a lot of lambdas being created, which might be slightly less performant if the compiler and runtime can't do fancy tricks to them (it often can).  However, while the code may be longer, it follows explicit rules that help prevent some really common errors (e.g., uncaught exceptions and `null` propagation), and if a couple of small changes are made, all of the code becomes much more testable:

```c#
public static Task<Uri> BuildUri(string url) => Task.FromResult(url).ResultMap(s => new Uri(s));

public static Func<Uri, Task<HttpResponseMessage>> PerformGetAsync(HttpClient httpClient, CancellationToken cancellationToken = default)
  => uri => Task.FromResult(httpClient).Bind(client => client.GetAsync(uri, cancellationToken));

public static Func<HttpResponseMessage, Task<string>> GetContentAsync(CancellationToken cancellationToken = default)
  => response => Task.FromResult(response).Bind(r => r.Content.ReadAsStringAsyc(cancellationToken));

public static async Task<string> MakeGetRequest(string url, HttpClient httpClient, CancellationToken cancellationToken = default)
   => Task.FromResult(url)
     .Bind(BuildUri)
     .Bind(PerformGetAsyc(httpClient, cancellationToken))
     .Bind(GetContentAsync(cancellationToken));
```

Now we have three functions that do exactly one thing, all of which are pure (meaning they don't mutate any state and only depend on their input arguments for their outputs).  Pure functions are great for testing, because their inputs can be easily mocked.  The fourth function is also pure, and is the combination of the previous three.  This setup also has the benefit that if we later need to add another step (say, parsing the content's body as JSON), we just add another `.Bind` or `.ResultMap` to the end of the chain.

-- NOTE:  _Technically_, pure functions only take a single input.  To pass multiple arguments currying is used to create nested functions, but that's not particularly important here.  It's really just kind of a mathematical trick, and we don't need to worry about it here. --

## Ending

Functional programming certainly isn't the best way to write every single program.  There are absolutely times when imperative or procedural code is a much better idea (object-oriented code is really just imperative code where some of the operations are nested inside objects).  However, making functional programming more readily available doesn't take anything away from those paradigms, and it provides greater freedom to build programs in a way that makes sense.  Monads are a big part of making that work seamlessly, and I hope that this document has both made that clear and provided a useful foundation for understanding how to use them.

[TaskExtensions.cs]: ./src/TaskExtensions.cs
