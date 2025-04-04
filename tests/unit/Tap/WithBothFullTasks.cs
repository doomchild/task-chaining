using System;
using System.Threading;
using System.Threading.Tasks;
using RLC.TaskChaining;
using Xunit;

namespace RLC.TaskChainingTests.Tap;

public class WithBothFullTasks
{
  [Fact]
  public async Task ItShouldPerformASideEffectOnAResolution()
  {
    int actualValue = 0;
    int expectedValue = 5;
    Func<int, Task<string>> onFulfilled = i =>
    {
      actualValue = i;

      return Task.FromResult(i.ToString());
    };
    Func<Exception, Task<string>> onFaulted = _ => Task.FromResult("1");

    await Task.FromResult(5)
      .Tap(onFulfilled, onFaulted);

    Assert.Equal(expectedValue, actualValue);
  }
  
  [Fact]
  public async Task ItShouldPerformASideEffectOnAResolutionWithoutAwaiting()
  {
    int actualValue = 0;
    int expectedValue = 5;
    Func<int, Task<int>> onFulfilled = i =>
    {
      actualValue = i;

      return Task.FromResult(i);
    };
    Func<Exception, Task<int>> onFaulted = _ => Task.FromResult(1);

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
    Func<Exception, Task<int>> onFaulted = _ =>
    {
      actualValue = 5;
      return Task.FromResult(1);
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
    Func<int, Task<int>> onFulfilled = _ => Task.FromResult(5);
    Func<Exception, Task<int>> onFaulted = _ =>
    {
      actualValue = 5;
      return Task.FromResult(1);
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
    Func<int, Task<int>> onFulfilled = i =>
    {
      actualValue = 0;

      return Task.FromResult(i);
    };
    Func<Exception, Task<int>> onFaulted = _ =>
    {
      actualValue = 5;
      return Task.FromResult(1);
    };

    cts.Cancel();

    _ = Task.Run(() => 1, cts.Token)
      .Tap(onFulfilled, onFaulted);

    await Task.Delay(10);

    Assert.Equal(expectedValue, actualValue);
  }
  
  [Fact]
  public async Task ItShouldPerformASideEffectOnACancellationWithoutAwaiting()
  {
    int actualValue = 0;
    int expectedValue = 5;
    CancellationTokenSource cts = new();
    Func<int, Task<int>> func = _ => Task.Run(() => 1, cts.Token);
    Func<int, Task<int>> onFulfilled = i =>
    {
      actualValue = 0;

      return Task.FromResult(i);
    };
    Func<Exception, Task<int>> onFaulted = _ =>
    {
      actualValue = 5;
      return Task.FromResult(1);
    };

    cts.Cancel();

    _ = Task.Run(() => 1, cts.Token)
      .Tap(onFulfilled, onFaulted);

    await Task.Delay(10);

    Assert.Equal(expectedValue, actualValue);
  }
  
  [Fact]
  public async Task ItShouldNotCallOnFaultedIfOnFulfilledThrows()
  {
    Func<int, Task<int>> onFulfilled = _ => throw new ArgumentException();
    Func<Exception, Task<int>> onFaulted = _ => Task.FromResult(1);

    Task testTask = Task.FromResult(0)
      .Tap(onFulfilled, onFaulted);

    await Assert.ThrowsAsync<ArgumentException>(() => testTask);
  }
  
  [Fact]
  public async Task ItShouldNotCallOnFaultedIfOnFulfilledThrowsWithoutAwaiting()
  {
    int actualValue = 0;
    int expectedValue = 5;
    Func<int, Task<int>> onFulfilled = _ => throw new ArgumentException();
    Func<Exception, Task<int>> onFaulted = _ =>
    {
      actualValue = 5;
      return Task.FromResult(1);
    };

    _ = Task.FromResult(0)
      .Tap(onFulfilled, onFaulted);
    
    await Task.Delay(10);

    Assert.NotEqual(expectedValue, actualValue);
  }
}