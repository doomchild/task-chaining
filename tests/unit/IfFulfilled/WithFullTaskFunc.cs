using System;
using System.Threading.Tasks;
using RLC.TaskChaining;
using Xunit;

namespace RLC.TaskChainingTests.IfFulfilled;

public class WithFullTaskFunc
{
  [Fact]
  public async Task ItShouldPerformASideEffect()
  {
    int actualValue = 0;
    int expectedValue = 5;
    Func<int, Task<int>> onFulfilled = _ =>
    {
      actualValue = 5;
      return Task.FromResult(5);
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
    Func<int, Task<int>> onFulfilled = _ =>
    {
      actualValue = 5;
      return Task.FromResult(5);
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
    Func<int, Task<int>> onFulfilled = _ =>
    {
      actualValue = 5;
      return Task.FromResult(5);
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
    Func<int, Task<int>> onFulfilled = _ =>
    {
      actualValue = 5;
      return Task.FromResult(5);
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
    Func<int, string> func = _ => throw new TaskCanceledException();
    Func<string, Task<int>> onFulfilled = _ =>
    {
      actualValue = 5;
      return Task.FromResult(5);
    };

    try
    {
      await Task.FromResult(0)
        .Then(func)
        .IfFulfilled(onFulfilled);
    }
    catch (OperationCanceledException)
    {
    }

    Assert.Equal(expectedValue, actualValue);
  }

  [Fact]
  public async Task ItShouldNotPerformASideEffectForACancellationWithoutAwaiting()
  {
    int actualValue = 0;
    int expectedValue = 0;
    Func<int, string> func = _ => throw new TaskCanceledException();
    Func<string, Task<int>> onFulfilled = _ =>
    {
      actualValue = 5;
      return Task.FromResult(5);
    };

    _ = Task.FromResult(0)
      .Then(func)
      .IfFulfilled(onFulfilled);

    await Task.Delay(10);

    Assert.Equal(expectedValue, actualValue);
  }

  [Fact]
  public async Task ItShouldResultInAFaultedTaskWithAnException()
  {
    Func<int, Task<int>> onFulfilled = _ => throw new ArgumentNullException();

    Task<int> testTask = Task.FromResult(0)
      .IfFulfilled(onFulfilled);

    await Assert.ThrowsAsync<ArgumentNullException>(() => testTask);
  }
}