using System;
using System.Threading;
using System.Threading.Tasks;
using RLC.TaskChaining;
using Xunit;

namespace RLC.TaskChainingTests.Tap;

public class WithFullTaskOnFulfilledAndActionOnFaulted
{
  [Fact]
  public async Task ItShouldPerformASideEffectOnAResolution()
  {
    int actualValue = 0;
    int expectedValue = 5;
    Func<int, Task<int>> onFulfilled = value =>
    {
      actualValue = value;
      return Task.FromResult(5);
    };
    Action<Exception> onFaulted = _ => { };

    await Task.FromResult(5)
      .Tap(onFulfilled, onFaulted);

    Assert.Equal(expectedValue, actualValue);
  }

  [Fact]
  public async Task ItShouldPerformASideEffectOnAResolutionWithoutAwaiting()
  {
    int actualValue = 0;
    int expectedValue = 5;
    Func<int, Task<int>> onFulfilled = value =>
    {
      actualValue = value;
      return Task.FromResult(5);
    };
    Action<Exception> onFaulted = _ => { };

    _ = Task.FromResult(5)
      .Tap(onFulfilled, onFaulted);

    await Task.Delay(10);

    Assert.Equal(expectedValue, actualValue);
  }

  [Fact]
  public async Task ItShouldPerformASideEffectOnAFault()
  {
    int actualValue = 0;
    int expectedValue = 5;
    Func<int, Task<int>> onFulfilled = _ => Task.FromResult(5);
    Action<Exception> onFaulted = _ => { actualValue = 5; };

    try
    {
      await Task.FromException<int>(new ArgumentNullException())
        .Tap(onFulfilled, onFaulted);
    }
    catch (ArgumentNullException)
    {
    }

    Assert.Equal(expectedValue, actualValue);
  }
  
  [Fact]
  public async Task ItShouldPerformASideEffectOnAFaultWithoutAwaiting()
  {
    int actualValue = 0;
    int expectedValue = 5;
    Func<int, Task<int>> onFulfilled = value => Task.FromResult(5);
    Action<Exception> onFaulted = _ => { actualValue = 5; };

    _ = Task.FromException<int>(new ArgumentNullException())
      .Tap(onFulfilled, onFaulted);

    await Task.Delay(10);

    Assert.Equal(expectedValue, actualValue);
  }

  [Fact]
  public async Task ItShouldPerformASideEffectOnACancellation()
  {
    int actualValue = 0;
    int expectedValue = 5;
    CancellationTokenSource cts = new();
    Func<int, Task<int>> func = _ => Task.Run(() => 1, cts.Token);
    Func<int, Task<int>> onFulfilled = _ =>
    {
      actualValue = 0;
      return Task.FromResult(5);
    };
    Action<Exception> onFaulted = _ => { actualValue = 5; };

    cts.Cancel();

    try
    {
      await Task.FromResult(0)
        .Then(func)
        .Tap(onFulfilled, onFaulted);
    }
    catch (TaskCanceledException)
    {
    }

    Assert.Equal(expectedValue, actualValue);
  }

  [Fact]
  public async Task ItShouldPerformASideEffectOnACancellationWithoutAwaiting()
  {
    int actualValue = 0;
    int expectedValue = 5;
    CancellationTokenSource cts = new();
    Func<int, Task<int>> func = _ => Task.Run(() => 1, cts.Token);
    Func<int, Task<int>> onFulfilled = _ =>
    {
      actualValue = 0;
      return Task.FromResult(5);
    };
    Action<Exception> onFaulted = _ => { actualValue = 5; };

    cts.Cancel();

    _ = Task.FromResult(0)
      .Then(func)
      .Tap(onFulfilled, onFaulted);

    await Task.Delay(10);

    Assert.Equal(expectedValue, actualValue);
  }
  
  [Fact]
  public async Task ItShouldNotCallOnFaultedIfOnFulfilledThrows()
  {
    Func<int, Task<int>> onFulfilled = _ => throw new ArgumentNullException();
    Action<Exception> onFaulted = _ => { };

    Task testTask = Task.FromResult(0)
      .Tap(onFulfilled, onFaulted);

    await Assert.ThrowsAsync<ArgumentNullException>(() => testTask);
  }
  
  [Fact]
  public async Task ItShouldNotCallOnFaultedIfOnFulfilledThrowsWithoutAwaiting()
  {
    int actualValue = 0;
    int expectedValue = 5;
    Func<int, Task<int>> onFulfilled = _ => throw new ArgumentNullException();
    Action<Exception> onFaulted = _ => { actualValue = 5; };

    _ = Task.FromResult(0)
      .Tap(onFulfilled, onFaulted);
    
    await Task.Delay(10);

    Assert.NotEqual(expectedValue, actualValue);
  }
}