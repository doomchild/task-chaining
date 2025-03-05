using System;
using System.Threading.Tasks;
using RLC.TaskChaining;
using Xunit;

namespace RLC.TaskChainingTests.IfFaulted;

public class WithAction
{
  [Fact]
  public async Task ItShouldPerformASideEffectWithoutAwaiting()
  {
    int actualValue = 0;
    int expectedValue = 5;
    Action<Exception> onFaulted = _ => { actualValue = 5; };

    _ = Task.FromException<int>(new ArgumentNullException())
      .IfFaulted(onFaulted);

    await Task.Delay(10);

    Assert.Equal(expectedValue, actualValue);
  }

  [Fact]
  public async Task ItShouldNotPerformASideEffectForAResolution()
  {
    int actualValue = 0;
    int expectedValue = 0;
    Action<Exception> onFaulted = _ => { actualValue = 5; };

    await Task.FromResult(5)
      .IfFaulted(onFaulted);

    Assert.Equal(expectedValue, actualValue);
  }

  [Fact]
  public async Task ItShouldNotPerformASideEffectForAResolutionWithoutAwaiting()
  {
    int actualValue = 0;
    int expectedValue = 0;
    Action<Exception> onFaulted = _ => { actualValue = 5; };

    _ = Task.FromResult(5)
      .IfFaulted(onFaulted);

    await Task.Delay(10);

    Assert.Equal(expectedValue, actualValue);
  }

  [Fact]
  public async Task ItShouldPerformASideEffectForACancellation()
  {
    int actualValue = 0;
    int expectedValue = 5;
    Func<int, string> func = _ => throw new TaskCanceledException();
    Action<Exception> onFaulted = _ => { actualValue = 5; };

    try
    {
      await Task.FromResult(0)
        .Then(func)
        .IfFaulted(onFaulted);
    }
    catch (OperationCanceledException)
    {
    }

    Assert.Equal(expectedValue, actualValue);
  }

  [Fact]
  public async Task ItShouldPerformASideEffectForACancellationWithoutAwaiting()
  {
    int actualValue = 0;
    int expectedValue = 5;
    Func<int, string> func = _ => throw new TaskCanceledException();
    Action<Exception> onFaulted = _ => { actualValue = 5; };

    _ = Task.FromResult(0)
      .Then(func)
      .IfFaulted(onFaulted);

    await Task.Delay(10);

    Assert.Equal(expectedValue, actualValue);
  }
  
  [Fact]
  public async Task ItShouldChangeExceptionTypesForExceptionThrowingFunc()
  {
    Func<int, string> func = _ => throw new ArgumentException();
    Action<Exception> onFaulted = _ => throw new NullReferenceException();

    Task<string> actualValue = Task.FromResult(5)
      .Then(func)
      .IfFaulted(onFaulted);

    await Assert.ThrowsAsync<NullReferenceException>(() => actualValue);
  }
  
  [Fact]
  public async Task ItShouldResultInAFaultedTaskWithAnException()
  {
    Action<Exception> onFaulted = _ => throw new ArgumentNullException();

    Task<int> testTask = Task.FromException<int>(new NullReferenceException())
      .IfFaulted(onFaulted);

    await Assert.ThrowsAsync<ArgumentNullException>(() => testTask);
  }
}