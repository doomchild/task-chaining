using System;
using System.Threading.Tasks;
using RLC.TaskChaining;
using Xunit;

namespace RLC.TaskChainingTests.IfFaulted;

public class WithFullTaskFunc
{
  [Fact]
  public async Task ItShouldPerformASideEffect()
  {
    int actualValue = 0;
    int expectedValue = 5;
    Func<Exception, Task<int>> onFaulted = _ =>
    {
      actualValue = 5;
      return Task.FromResult(5);
    };

    try
    {
      await Task.FromException<int>(new ArgumentNullException())
        .IfFaulted(onFaulted);
    }
    catch (ArgumentNullException)
    {
    }

    Assert.Equal(expectedValue, actualValue);
  }
  
  [Fact]
  public async Task ItShouldPerformASideEffectWithoutAwaiting()
  {
    int actualValue = 0;
    int expectedValue = 5;
    Func<Exception, Task<int>> onFaulted = _ =>
    {
      actualValue = 5;
      return Task.FromResult(5);
    };

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
    Func<Exception, Task<int>> onFaulted = _ =>
    {
      actualValue = 5;
      return Task.FromResult(5);
    };

    await Task.FromResult(5)
      .IfFaulted(onFaulted);

    Assert.Equal(expectedValue, actualValue);
  }

  [Fact]
  public async Task ItShouldNotPerformASideEffectForAResolutionWithoutAwaiting()
  {
    int actualValue = 0;
    int expectedValue = 0;
    Func<Exception, Task<int>> onFaulted = _ =>
    {
      actualValue = 5;
      return Task.FromResult(5);
    };

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
    Func<Exception, Task<int>> onFaulted = _ =>
    {
      actualValue = 5;
      return Task.FromResult(5);
    };

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
    Func<Exception, Task<int>> onFaulted = _ =>
    {
      actualValue = 5;
      return Task.FromResult(5);
    };

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
    Func<Exception, Task<string>> onFaulted = _ => throw new NullReferenceException();

    Task<string> actualValue = Task.FromResult(5)
      .Then(func)
      .IfFaulted(onFaulted);

    await Assert.ThrowsAsync<NullReferenceException>(() => actualValue);
  }
  
  [Fact]
  public async Task ItShouldResultInAFaultedTaskWithAnException()
  {
    Func<Exception, Task<string>> func = _ => throw new ArgumentNullException();

    Task<int> testTask = Task.FromException<int>(new NullReferenceException())
      .IfFaulted(func);

    await Assert.ThrowsAsync<ArgumentNullException>(() => testTask);
  }
  
  [Fact]
  public async Task ItShouldProperlyCaptureTheTask()
  {
    int actualValue = 0;
    int expectedValue = 5;
    Func<Exception, Task<int>> onFaulted = async _ =>
    {
      await Task.Delay(TimeSpan.FromSeconds(1));
      
      actualValue = 5;
      return 5;
    };

    try
    {
      await Task.FromException<int>(new ArgumentNullException())
        .IfFaulted(onFaulted);
    }
    catch (ArgumentNullException)
    {
    }

    Assert.Equal(expectedValue, actualValue);
  }
}