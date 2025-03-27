using System;
using System.Threading;
using System.Threading.Tasks;
using RLC.TaskChaining;
using Xunit;

namespace RLC.TaskChainingTests.Tap;

public class WithBothActions
{
  [Fact]
  public async Task ItShouldPerformASideEffectOnAResolution()
  {
    int actualValue = 0;
    int expectedValue = 5;
    Action<int> onFulfilled = value => { actualValue = value; };
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
    Action<int> onFulfilled = value => { actualValue = value; };
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
    Action<int> onFulfilled = _ => { };
    Action<Exception> onFaulted = _ => { actualValue = 5; };

    try
    {
      await Task.FromException<int>(new ArgumentNullException())
        .Tap(onFulfilled, onFaulted);
    }
    catch
    {
      // ignored
    }

    Assert.Equal(expectedValue, actualValue);
  }
  
  [Fact]
  public async Task ItShouldPerformASideEffectOnAFaultWithoutAwaiting()
  {
    int actualValue = 0;
    int expectedValue = 5;
    Action<int> onFulfilled = _ => { };
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
    Action<int> onFulfilled = _ => { actualValue = 0; };
    Action<Exception> onFaulted = _ => { actualValue = 5; };
    
    cts.Cancel();

    try
    {
      await Task.Run(() => 0, cts.Token)
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
    Action<int> onFulfilled = _ => { actualValue = 0; };
    Action<Exception> onFaulted = _ => { actualValue = 5; };
    
    cts.Cancel();

    _ = Task.Run(() => 0, cts.Token)
      .Tap(onFulfilled, onFaulted);

    await Task.Delay(10);

    Assert.Equal(expectedValue, actualValue);
  }
  
  [Fact]
  public async Task ItShouldNotCallOnFaultedIfOnFulfilledThrows()
  {
    Action<int> onFulfilled = _ => throw new ArgumentException();
    Action<Exception> onFaulted = _ => { };

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
    Action<Exception> onFaulted = _ => { actualValue = 5; };

    _ = Task.FromResult(0)
      .Tap(onFulfilled, onFaulted);
    
    await Task.Delay(10);

    Assert.NotEqual(expectedValue, actualValue);
  }
}