using System;
using System.Threading.Tasks;

using RLC.TaskChaining;
using Xunit;

using static RLC.TaskChaining.TaskStatics;

public class TaskChainingIfFulfilledTests
{
  [Fact]
  public async void ItShouldPerformASideEffect()
  {
    int actualValue = 0;
    int expectedValue = 5;

    await Task.FromResult(5)
      .IfFulfilled(value =>
      {
        actualValue = value;
      });

    Assert.Equal(expectedValue, actualValue);
  }

  [Fact]
  public async void ItShouldPerformASideEffectWithoutAwaiting()
  {
    int actualValue = 0;
    int expectedValue = 5;

    _ = Task.FromResult(5)
      .IfFulfilled(value =>
      {
        actualValue = value;
      });

    await Task.Delay(2);

    Assert.Equal(expectedValue, actualValue);
  }

  [Fact]
  public async void ItShouldNotPerformASideEffectForAFault()
  {
    int actualValue = 0;
    int expectedValue = 0;

    try
    {
      await Task.FromException<int>(new ArgumentNullException())
        .IfFulfilled((int value) =>
        {
          actualValue = 5;
        });
    }
    catch (ArgumentNullException)
    {
    }

    Assert.Equal(expectedValue, actualValue);
  }

  [Fact]
  public async void ItShouldNotPerformASideEffectForAFaultWithoutAwaiting()
  {
    int actualValue = 0;
    int expectedValue = 0;

    _ = Task.FromException<int>(new ArgumentNullException())
      .IfFulfilled((int _) =>
      {
        actualValue = 5;
      });

    await Task.Delay(2);

    Assert.Equal(expectedValue, actualValue);
  }

  [Fact]
  public async void ItShouldNotPerformASideEffectForACancellation()
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
        });
    }
    catch (OperationCanceledException)
    {
    }

    Assert.Equal(expectedValue, actualValue);
  }

  [Fact]
  public async void ItShouldNotPerformASideEffectForACancellationWithoutAwaiting()
  {
    int actualValue = 0;
    int expectedValue = 0;
    Func<int, string> func = _ => throw new TaskCanceledException();

    _ = Task.FromResult<int>(0)
      .Then(func)
      .IfFulfilled(_ =>
      {
        actualValue = 5;
      });

    await Task.Delay(2);

    Assert.Equal(expectedValue, actualValue);
  }

  public class WithTaskFunc
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

  public class WithAsyncAction
  {
    [Fact]
    public async void ItShouldContinueAsyncTasks()
    {
      string testValue = "12345";
      int expectedValue = 5;
      int actualValue = 0;
      Func<string, Task> func = async value =>
      {
        await Task.Delay(1);

        actualValue = value.Length;
      };

      _ = Task.FromResult(testValue)
        .IfFulfilled(func);

      await Task.Delay(5);

      Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public async Task ItShouldAwaitAsyncSideEffects()
    {
      int coin = 5;
      int testValue = 12345;
      int expectedSideEffectValue = 12345;
      string expectedValue = "12345";
      int? sideEffectIntValue = null;
      string? sideEffectStringValue = null;
      Func<string, Task> func = value =>
      {
        return Task.FromResult(value)
          .Delay(TimeSpan.FromSeconds(1))
          .Then(int.Parse)
          .Then(v =>
          {
            sideEffectIntValue = v;
            return v;
          });
      };

      string actualValue = await Task.FromResult(testValue)
        .Then(val => val.ToString())
        .IfFulfilled(value => coin < 6
          ? func(value)
          : Task.FromResult(value)
        );

      Assert.Equal(expectedValue, actualValue);
      Assert.Equal(expectedSideEffectValue, sideEffectIntValue);
      Assert.Null(sideEffectStringValue);
    }
  }
}
