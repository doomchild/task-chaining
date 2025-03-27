using System;
using System.Threading;
using System.Threading.Tasks;
using RLC.TaskChaining;
using Xunit;

namespace RLC.TaskChainingTests.IfFulfilled;

public class WithRawTaskFunc
{
  [Fact]
  public async Task ItShouldPerformASideEffect()
  {
    int actualValue = 0;
    int expectedValue = 5;
    Func<int, Task> onFulfilled = _ =>
    {
      actualValue = 5;
      return Task.CompletedTask;
    };

    await Task.FromResult(5)
      .IfFulfilled(onFulfilled);

    Assert.Equal(expectedValue, actualValue);
  }

  [Fact]
  public async Task ItShouldPerformASideEffectWithoutAwaiting()
  {
    int actualValue = 0;
    int expectedValue = 5;
    Func<int, Task> onFulfilled = _ =>
    {
      actualValue = 5;
      return Task.CompletedTask;
    };

    _ = Task.FromResult(5)
      .IfFulfilled(onFulfilled);

    await Task.Delay(10);

    Assert.Equal(expectedValue, actualValue);
  }
  
  [Fact]
  public async Task ItShouldNotPerformASideEffectForAFault()
  {
    int actualValue = 0;
    int expectedValue = 0;
    Func<int, Task> onFulfilled = _ =>
    {
      actualValue = 5;
      return Task.CompletedTask;
    };

    try
    {
      await Task.FromException<int>(new ArgumentNullException())
        .IfFulfilled(onFulfilled);
    }
    catch (ArgumentNullException)
    {
    }

    Assert.Equal(expectedValue, actualValue);
  }
  
  [Fact]
  public async Task ItShouldNotPerformASideEffectForAFaultWithoutAwaiting()
  {
    int actualValue = 0;
    int expectedValue = 0;
    Func<int, Task> onFulfilled = _ =>
    {
      actualValue = 5;
      return Task.CompletedTask;
    };

    _ = Task.FromException<int>(new ArgumentNullException())
      .IfFulfilled(onFulfilled);

    await Task.Delay(10);

    Assert.Equal(expectedValue, actualValue);
  }

  [Fact]
  public async Task ItShouldNotPerformASideEffectForACancellation()
  {
    int actualValue = 0;
    int expectedValue = 0;
    CancellationTokenSource cts = new();
    Func<int, Task<string>> func = _ => Task.Run(() => string.Empty, cts.Token);
    Func<string, Task> onFulfilled = _ =>
    {
      actualValue = 5;
      return Task.CompletedTask;
    };

    cts.Cancel();

    try
    {
      await Task.FromResult(0)
        .Then(func)
        .IfFulfilled(onFulfilled);
    }
    catch (TaskCanceledException)
    {
    }

    Assert.Equal(expectedValue, actualValue);
  }

  [Fact]
  public async Task ItShouldNotPerformASideEffectForACancellationWithoutAwaiting()
  {
    int actualValue = 0;
    int expectedValue = 0;
    CancellationTokenSource cts = new();
    Func<int, Task<string>> func = _ => Task.Run(() => string.Empty, cts.Token);
    Func<string, Task> onFulfilled = _ =>
    {
      actualValue = 5;
      return Task.CompletedTask;
    };

    cts.Cancel();

    _ = Task.FromResult(0)
      .Then(func)
      .IfFulfilled(onFulfilled);

    await Task.Delay(10);

    Assert.Equal(expectedValue, actualValue);
  }

  [Fact]
  public async Task ItShouldResultInAFaultedTaskWithAnException()
  {
    Func<int, Task> onFulfilled = _ => throw new ArgumentNullException();

    Task<int> testTask = Task.FromResult(0)
      .IfFulfilled(onFulfilled);

    await Assert.ThrowsAsync<ArgumentNullException>(() => testTask);
  }
  
  [Fact]
  public async Task ItShouldProperlyCaptureTheTask()
  {
    int actualValue = 0;
    int expectedValue = 5;
    Func<int, Task> onFulfilled = async _ =>
    {
      await Task.Delay(TimeSpan.FromSeconds(1));
      
      actualValue = 5;
    };

    await Task.FromResult(5)
      .IfFulfilled(onFulfilled);

    Assert.Equal(expectedValue, actualValue);
  }
}