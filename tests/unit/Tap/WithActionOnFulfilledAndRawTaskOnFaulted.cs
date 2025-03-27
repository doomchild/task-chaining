using System;
using System.Threading;
using System.Threading.Tasks;
using RLC.TaskChaining;
using Xunit;

namespace RLC.TaskChainingTests.Tap;

public class WithActionOnFulfilledAndRawTaskOnFaulted
{
  [Fact]
  public async Task ItShouldPerformASideEffectOnAResolution()
  {
    int actualValue = 0;
    int expectedValue = 5;
    Action<int> onFulfilled = i => { actualValue = i; };
    Func<Exception, Task> onFaulted = _ => Task.CompletedTask;

    await Task.FromResult(5)
      .Tap(onFulfilled, onFaulted);

    Assert.Equal(expectedValue, actualValue);
  }
  
  [Fact]
  public async Task ItShouldPerformASideEffectOnAResolutionWithoutAwaiting()
  {
    int actualValue = 0;
    int expectedValue = 5;
    Action<int> onFulfilled = i => { actualValue = i; };
    Func<Exception, Task> onFaulted = _ => Task.CompletedTask;

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
    Action<int> onFulfilled = _ => { };
    Func<Exception, Task> onFaulted = _ =>
    {
      actualValue = 5;
      return Task.CompletedTask;
    };

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
    Action<int> onFulfilled = _ => { };
    Func<Exception, Task> onFaulted = _ =>
    {
      actualValue = 5;
      return Task.CompletedTask;
    };

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
    Action<int> onFulfilled = _ => { actualValue = 0; };
    Func<Exception, Task> onFaulted = _ =>
    {
      actualValue = 5;
      return Task.CompletedTask;
    };
    
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
    Action<int> onFulfilled = _ => { actualValue = 0; };
    Func<Exception, Task> onFaulted = _ =>
    {
      actualValue = 5;
      return Task.CompletedTask;
    };
    
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
    Action<int> onFulfilled = _ => throw new ArgumentException();
    Func<Exception, Task> onFaulted = _ => Task.CompletedTask;

    Task testTask = Task.FromResult(0)
      .Tap(onFulfilled, onFaulted);

    await Assert.ThrowsAsync<ArgumentException>(() => testTask);
  }
  
  [Fact]
  public async Task ItShouldNotCallOnFaultedIfOnFulfilledThrowsWithoutAwaiting()
  {
    int actualValue = 0;
    int expectedValue = 5;
    Action<int> onFulfilled = _ => throw new ArgumentException();
    Func<Exception, Task> onFaulted = _ =>
    {
      actualValue = 5;
      return Task.CompletedTask;
    };

    _ = Task.FromResult(0)
      .Tap(onFulfilled, onFaulted);
    
    await Task.Delay(10);

    Assert.NotEqual(expectedValue, actualValue);
  }
}