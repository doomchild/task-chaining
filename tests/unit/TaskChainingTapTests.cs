using System;
using System.Threading.Tasks;

using RLC.TaskChaining;
using Xunit;

public class TaskChainingTapTests
{
  [Fact]
  public async void ItShouldPerformASideEffectOnAResolution()
  {
    int actualValue = 0;
    int expectedValue = 5;

    await Task.FromResult(5)
      .Tap(value =>
      {
        actualValue = value;
      },
        _ => { }
      );

    Assert.Equal(expectedValue, actualValue);
    }

  [Fact]
  public async void ItShouldPerformASideEffectOnAResolutionWithoutAwaiting()
  {
    int actualValue = 0;
    int expectedValue = 5;

    _ = Task.FromResult(5)
      .Tap(value =>
      {
        actualValue = value;
      },
        _ => { }
      );

    await Task.Delay(10);

    Assert.Equal(expectedValue, actualValue);
  }

  [Fact]
  public async void ItShouldPerformASideEffectOnAFaultWithoutAwaiting()
  {
    int actualValue = 0;
    int expectedValue = 5;

    _ = Task.FromException<int>(new ArgumentNullException())
      .Tap(
        _ => { },
        _ =>
        {
          actualValue = 5;
        }
      );

    await Task.Delay(10);

    Assert.Equal(expectedValue, actualValue);
  }

  [Fact]
  public async void ItShouldPerformASideEffectOnACancellationWithoutAwaiting()
  {
    int actualValue = 0;
    int expectedValue = 5;
    Func<int, int> func = i =>
    {
      throw new TaskCanceledException();
    };

    _ = Task.FromResult(0)
      .Then(func)
      .Tap(
        i => { actualValue = 0; },
        ex => { actualValue = 5; }
      );

    await Task.Delay(10);

    Assert.Equal(expectedValue, actualValue);
  }

  public class WithTaskReturningFunc
  {
    [Fact]
    public async void ItShouldPerformASideEffectOnAResolution()
    {
      int actualValue = 0;
      int expectedValue = 5;
      Func<int, Task<string>> func = i =>
      {
        actualValue = i;

        return Task.FromResult(i.ToString());
      };

      await Task.FromResult(5)
        .Tap(func, _ => { });

      Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public async void ItShouldPerformASideEffectOnACancellation()
    {
      int actualValue = 0;
      int expectedValue = 5;
      Func<int, Task<int>> func = i =>
      {
        throw new TaskCanceledException();
      };

      _ = Task.FromResult(0)
        .Then(func)
        .Tap(
          i => { actualValue = 0; },
          ex => { actualValue = 5; }
        );

      await Task.Delay(10);

      Assert.Equal(expectedValue, actualValue);
    }
  }

  public class WithAsyncAction
  {
    [Fact]
    public async void ItShouldPerformASideEffectOnAResolution()
    {
      int actualValue = 0;
      int expectedValue = 5;
      Func<int, Task> func = async i =>
      {
        await Task.Delay(1);

        actualValue = i;
      };

      _ = Task.FromResult(5)
        .Tap(func, ex => Task.FromException<int>(ex));

      await Task.Delay(10);

      Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public async void ItShouldPerformASideEffectOnAFault()
    {
      int actualValue = 0;
      int expectedValue = 5;
      Func<Exception, Task> func = async i =>
      {
        await Task.Delay(1);

        actualValue = 5;
      };

      _ = Task.FromException<int>(new Exception())
        .Tap(i => Task.FromResult(i), func);

      await Task.Delay(10);

      Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public async void ItShouldPerformASideEffectOnACancellation()
    {
      int actualValue = 0;
      int expectedValue = 5;
      Func<int, Task<int>> func = async i =>
      {
        await Task.Delay(1);

        throw new TaskCanceledException();
      };

      _ = Task.FromResult(0)
        .Then(func)
        .Tap(
          i => { actualValue = 0; },
          ex => { actualValue = 5; }
        );

      await Task.Delay(10);

      Assert.Equal(expectedValue, actualValue);
    }
  }
}
