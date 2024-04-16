using System;
using System.Threading.Tasks;

using RLC.TaskChaining;
using Xunit;

using static RLC.TaskChaining.TaskStatics;

public class TaskChainingIfFaultedTests
{
  [Fact]
  public async void ItShouldPerformASideEffectWithoutAwaiting()
  {
    int actualValue = 0;
    int expectedValue = 5;

    _ = Task.FromException<int>(new ArgumentNullException())
      .IfFaulted((Exception _) =>
      {
        actualValue = 5;
      });

    await Task.Delay(2);

    Assert.Equal(expectedValue, actualValue);
  }

  [Fact]
  public async void ItShouldNotPerformASideEffectForAResolution()
  {
    int actualValue = 0;
    int expectedValue = 0;

    await Task.FromResult(5)
      .IfFaulted((Exception _) =>
      {
        actualValue = 5;
      });

    Assert.Equal(expectedValue, actualValue);
  }

  [Fact]
  public async void ItShouldNotPerformASideEffectForAResolutionWithoutAwaiting()
  {
    int actualValue = 0;
    int expectedValue = 0;

    _ = Task.FromResult(5)
      .IfFaulted((Exception _) =>
      {
        actualValue = 5;
      });

    await Task.Delay(2);

    Assert.Equal(expectedValue, actualValue);
  }

  [Fact]
  public async void ItShouldPerformASideEffectForACancellation()
  {
    int actualValue = 0;
    int expectedValue = 5;
    Func<int, string> func = _ => throw new TaskCanceledException();

    try
    {
      await Task.FromResult<int>(0)
        .Then(func)
        .IfFaulted(_ =>
        {
          actualValue = 5;
        });
    }
    catch (OperationCanceledException)
    {
    }

    Assert.Equal(expectedValue, actualValue);
  }

  [Fact]
  public async void ItShouldPerformASideEffectForACancellationWithoutAwaiting()
  {
    int actualValue = 0;
    int expectedValue = 5;
    Func<int, string> func = _ => throw new TaskCanceledException();

    _ = Task.FromResult<int>(0)
      .Then(func)
      .IfFaulted(_ =>
      {
        actualValue = 5;
      });

    await Task.Delay(2);

    Assert.Equal(expectedValue, actualValue);
  }

  public class WithTaskFunc
  {
    [Fact]
    public async void ItShouldPerformASideEffectWithoutAwaiting()
    {
      int actualValue = 0;
      int expectedValue = 5;
      Func<Exception, Task<string>> func = exception =>
      {
        actualValue = 5;

        return Task.FromResult(Guid.NewGuid().ToString());
      };

      _ = Task.FromException<int>(new ArgumentNullException())
        .IfFaulted(func);

      await Task.Delay(2);

      Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public async void ItShouldNotPerformASideEffectForAResolution()
    {
      int actualValue = 0;
      int expectedValue = 0;
      Func<Exception, Task<string>> func = exception =>
      {
        actualValue = 5;

        return Task.FromResult(Guid.NewGuid().ToString());
      };

      await Task.FromResult(5)
        .IfFaulted(func);

      Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public async void ItShouldNotChangeExceptionTypesForExceptionThrowingFunc()
    {
      Func<Exception, Task<string>> func = exception =>
      {
        throw new InvalidOperationException();
      };

      await Assert.ThrowsAsync<ArgumentNullException>(async () => await Task.FromException<int>(new ArgumentNullException())
      .IfFaulted(func));
    }
  }

  public class WithAsyncAction
  {
    [Fact]
    public async void ItShouldContinueAsyncTasks()
    {
      int actualValue = 0;
      int expectedValue = 5;
      Func<Exception, Task> func = async exception =>
      {
        await Task.Delay(1);

        actualValue = 5;
      };

      _ = Task.FromException<int>(new Exception())
        .IfFaulted(func);

      await Task.Delay(5);

      Assert.Equal(expectedValue, actualValue);
    }
  }
}
