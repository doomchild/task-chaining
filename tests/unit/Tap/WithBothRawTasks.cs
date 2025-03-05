using System;
using System.Threading.Tasks;
using RLC.TaskChaining;
using Xunit;

namespace RLC.TaskChainingTests.Tap;

public class WithBothRawTasks
{
  [Fact]
  public async Task ItShouldPerformASideEffectOnAResolution()
  {
    int actualValue = 0;
    int expectedValue = 5;
    Func<int, Task> onFulfilled = i =>
    {
      actualValue = i;

      return Task.CompletedTask;
    };
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
    Func<int, Task> onFulfilled = value =>
    {
      actualValue = value;
      return Task.CompletedTask;
    };
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
    Func<int, Task> onFulfilled = _ => Task.CompletedTask;
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
    Func<int, Task> onFulfilled = _ => Task.CompletedTask;
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
    Func<int, Task> func = _ => throw new TaskCanceledException();
    Func<Task, Task> onFulfilled = _ =>
    {
      actualValue = 0;
      return Task.CompletedTask;
    };
    Func<Exception, Task> onFaulted = _ =>
    {
      actualValue = 5;
      return Task.CompletedTask;
    };

    _ = Task.FromResult(0)
      .Then(func)
      .Tap(onFulfilled, onFaulted);

    await Task.Delay(10);

    Assert.Equal(expectedValue, actualValue);
  }
  
  [Fact]
  public async Task ItShouldPerformASideEffectOnACancellationWithoutAwaiting()
  {
    int actualValue = 0;
    int expectedValue = 5;
    Func<int, Task> func = _ => throw new TaskCanceledException();
    Func<Task, Task> onFulfilled = _ =>
    {
      actualValue = 0;
      return Task.CompletedTask;
    };
    Func<Exception, Task> onFaulted = _ =>
    {
      actualValue = 5;
      return Task.CompletedTask;
    };

    _ = Task.FromResult(0)
      .Then(func)
      .Tap(onFulfilled, onFaulted);

    await Task.Delay(10);

    Assert.Equal(expectedValue, actualValue);
  }
  
  [Fact]
  public async Task ItShouldNotCallOnFaultedIfOnFulfilledThrows()
  {
    Func<int, Task> onFulfilled = _ => throw new ArgumentException();
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
    Func<int, Task> onFulfilled = _ => throw new ArgumentException();
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