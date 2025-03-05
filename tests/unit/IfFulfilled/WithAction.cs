using System;
using System.Threading.Tasks;
using RLC.TaskChaining;
using Xunit;

namespace RLC.TaskChainingTests.IfFulfilled;

public class WithAction
{
  [Fact]
  public async Task ItShouldPerformASideEffect()
  {
    int actualValue = 0;
    int expectedValue = 5;

    await Task.FromResult(5)
      .IfFulfilled(value => { actualValue = value; });

    Assert.Equal(expectedValue, actualValue);
  }

  [Fact]
  public async Task ItShouldPerformASideEffectWithoutAwaiting()
  {
    int actualValue = 0;
    int expectedValue = 5;

    _ = Task.FromResult(5)
      .IfFulfilled(value => { actualValue = value; });

    await Task.Delay(10);

    Assert.Equal(expectedValue, actualValue);
  }
  
  [Fact]
  public async Task ItShouldNotPerformASideEffectForAFault()
  {
    int actualValue = 0;
    int expectedValue = 0;

    try
    {
      await Task.FromException<int>(new ArgumentNullException())
        .IfFulfilled(_ => { actualValue = 5; });
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

    _ = Task.FromException<int>(new ArgumentNullException())
      .IfFulfilled(_ => { actualValue = 5; });

    await Task.Delay(10);

    Assert.Equal(expectedValue, actualValue);
  }

  [Fact]
  public async Task ItShouldNotPerformASideEffectForACancellation()
  {
    int actualValue = 0;
    int expectedValue = 0;
    Func<int, string> func = _ => throw new TaskCanceledException();

    try
    {
      await Task.FromResult<int>(0)
        .Then(func)
        .IfFulfilled(_ => { actualValue = 5; });
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

    _ = Task.FromResult<int>(0)
      .Then(func)
      .IfFulfilled(_ => { actualValue = 5; });

    await Task.Delay(10);

    Assert.Equal(expectedValue, actualValue);
  }

  [Fact]
  public async Task ItShouldResultInAFaultedTaskWithAnException()
  {
    Action<int> func = _ => throw new ArgumentNullException();

    Task<int> testTask = Task.FromResult(0)
      .IfFulfilled(func);

    await Assert.ThrowsAsync<ArgumentNullException>(() => testTask);
  }
}