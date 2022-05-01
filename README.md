# RLC.TaskChaining

Monadic-style chaining for C# Tasks.

## Rationale

Asynchronous code (particularly in C#) typically relies on using the `async`/`await` feature introduced in C# 5.0.  This has a lot of benefits, but it unfortunately tends to push code into an imperative style.  This library aims to make writing asychronous functional code easier, cleaner, and less error-prone using extensions to `System.Threading.Tasks`.

## Installation

Install RLC.TaskChaining as a NuGet package via an IDE package manager or using the command-line instructions at [nuget.org][].

## API

### Monadic Functions

Monads are usually portrayed as a lot more complicated than they really have to be.  See [MONADS.md][] for a brief tutorial.  Note that this library contains both standard monadic functions (such as `ResultMap`(`fmap`), `Bind`, etc) and [bluebird][] Promise-style functions (`Then`, `Catch`, `Tap`, etc).  Some scenarios will lend themselves to one over the other.

### Chaining

#### Then

Once a `Task<T>` has been created, successive operations can be chained using the `Then` method.

```c#
HttpClient client;  // Assuming this is coming from an HttpClientFactory or injected or whatever

Task.FromResult("https://www.google.com")  // Task<string>
  .Then(client.GetAsync)                   // Task<HttpResponseMessage>
  .Then(response => response.StatusCode);  // Task<System.Net.HttpStatusCode>
```

#### Catch

When a `Task<T>` enters a faulted state, the `Catch` method can be used to return the `Task<T>` to a non-faulted state.

```c#
HttpClient client;  // Assuming this is coming from an HttpClientFactory or injected or whatever

Task.FromResult("not-a-url")              // Task<string>
  .Then(client.GetAsync)                  // Task<HttpResponseMessage> but FAULTED
  .Catch(exception => exception.Message)  // Task<string> and NOT FAULTED
  .Then(message => message.Length)        // Task<int>
```

Note that it's entirely possible for a `Catch` method to cause the `Task<T>` to remain in a faulted state, e.g. if you only wanted to recover into a non-faulted state if a particular exception type occurred.

```c#
HttpClient client;  // Assuming this is coming from an HttpClientFactory or injected or whatever

Task.FromResult("not-a-url")          // Task<string>
  .Then(client.GetAsync)              // Task<HttpResponseMessage> but FAULTED
  .Catch(exception => exception is NullReferenceException
    ? exception.Message
    : Task.FromException(exception))  // Task<string> but STILL FAULTED if anything other than NullReferenceException occurred
  .Then(message => message.Length)    // Task<int>
```

#### IfFulfilled/IfFaulted/Tap

The `IfFulfilled` and `IfFaulted` methods can be used to perform side effects such as logging when the `Task<T>` is in the fulfilled or faulted state, respectively.

```c#
HttpClient client;  // Assuming this is coming from an HttpClientFactory or injected or whatever

Task.FromResult("https://www.google.com/")
  .Then(client.GetAsync)
  .IfFulfilled(response => _logger.LogDebug("Got response {Response}", response)
  .Then(response => response.StatusCode);
```

```c#
HttpClient client;  // Assuming this is coming from an HttpClientFactory or injected or whatever

Task.FromResult("not-a-url")
  .Then(client.GetAsync)
  .IfFaulted(exception => _logger.LogException(exception, "Failed to get URL")
  .Catch(exception => exception.Message)
  .Then(message => message.Length);
```

The `Tap` method takes both an `onFulfilled` and `onRejected` `Action` in the event that you want to perform some side effect on both sides of the `Task` at a single time.

```c#
HttpClient client;  // Assuming this is coming from an HttpClientFactory or injected or whatever

Task.FromResult(someExternalUrl)
  .Then(client.GetAsync)
  .Tap(
    response => _logger.LogDebug("Got response {Response}", response),
    exception => _logger.LogException(exception, "Failed to get URL")
  )
```

#### Retry

`Task.Retry` can be used to automatically retry a function.  The `RetryOptions` type holds the retry interval, backoff rate, maximum attempt count, an optional `Action` to perform when a retry is about to happen, and a `Predicate<Exception>` that is used to decide whether or not a retry should be performed based on the `Exception` that occurred during the last execution.

```c#
HttpClient client;  // Assuming this is coming from an HttpClientFactory or injected or whatever
ILogger logger;

RetryOptions options = new (
  3,
  TimeSpan.FromMilliseconds(1000),
  2,
  (attemptCount, duration, exception) => logger.LogError(exception, $"Starting retry {attemptCount} after {duration.TotalMilliseconds} milliseconds"),
  exception => exception is NullReferenceException ? true : false  // Only NullReferenceExceptions will trigger retries, other exceptions will fall through
);

Task.FromResult(someExternalUrl)
  .Retry(client.GetAsync, options)
```

If the `RetryOptions` parameter is not passed, the default values (3 attempts, 1000ms duration, backoff rate of 2) are used.

### Static Methods

There are some convenience methods on `TaskExtras` that are useful when transitioning between the fulfilled and faulted states.

#### RejectIf

`RejectIf` can be used to transition into a faulted state based on some `Predicate<T>`.

```c#
Task.FromResult(1)
  .Then(TaskExtras.RejectIf(
    value => value % 2 == 0,
    value => new Exception($"{nameof(value)} was not even")
  ));
```

#### ResolveIf

`ResolveIf` can be used to transition from a faulted state to a fulfilled state based on some `Predicate<Exception>`.

```c#
Task.FromException<int>(new ArgumentException())
  .Catch(TaskExtras.ResolveIf(
    exception => exception is ArgumentException,
    exception => exception.Message.Length
  ))
```

[bluebird]: http://bluebirdjs.com/docs/getting-started.html
[MONADS.md]: ./MONADS.md
[nuget.org]: https://www.nuget.org/packages/RLC.TaskChaining/
