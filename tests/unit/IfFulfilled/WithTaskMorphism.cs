using System;
using System.Threading.Tasks;

using RLC.TaskChaining;
using Xunit;

namespace RLC.TaskChainingTests.IfFulfilled;

public class WithTaskMorphism
{
  [Fact]
  public async void ItShouldPerformASideEffect()
  {
    int actualValue = 0;
    int expectedValue = 5;
    Func<int, Task<string>> func = i =>
    {
      actualValue = i;

      return Task.FromResult(i.ToString());
    };

    await Task.FromResult(5)
      .IfFulfilled(func);

    Assert.Equal(expectedValue, actualValue);
  }
  
  [Fact]
  public async Task ItShouldPerformASideEffectWithoutAwaiting()
  {
    int actualValue = 0;
    int expectedValue = 5;

    _ = Task.FromResult(5)
      .IfFulfilled(value =>
      {
        actualValue = value;

        return Task.FromResult(value.ToString());
      });

    await Task.Delay(2);

    Assert.Equal(expectedValue, actualValue);
  }

  [Fact]
  public async void ItShouldNotPerformASideEffectForAFault()
  {
    int actualValue = 0;
    int expectedValue = 0;
    Func<int, Task<string>> func = i =>
    {
      actualValue = i;

      return Task.FromResult(i.ToString());
    };

    try
    {
      await Task.FromException<int>(new ArgumentNullException())
        .IfFulfilled(func);
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
      .IfFulfilled(_ =>
      {
        actualValue = 5;

        return Task.FromResult(actualValue);
      });

    await Task.Delay(2);

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
        .IfFulfilled(_ =>
        {
          actualValue = 5;

          return Task.FromResult(actualValue);
        });
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
      .IfFulfilled(_ =>
      {
        actualValue = 5;

        return Task.FromResult(actualValue);
      });

    await Task.Delay(2);

    Assert.Equal(expectedValue, actualValue);
  }

  [Fact]
  public async void ItShouldNotResultInAFaultedTaskWithAnException()
  {
    Func<int, Task<string>> func = i =>
    {
      throw new ArgumentException();
    };

    int expectedValue = 5;
    int actualValue = await Task.FromResult(5)
      .IfFulfilled(func);

    Assert.Equal(expectedValue, actualValue);
  }
}