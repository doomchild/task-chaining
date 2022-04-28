using System;
using System.Diagnostics;
using System.Threading.Tasks;

using RLC.TaskChaining;
using Xunit;

namespace RLC.TaskChainingTests;

public class TaskExtrasTests
{
	public class RejectIf
  {
    [Fact]
    public async void ItShouldRejectForASuccessfulPredicate()
    {
      Task<int> testTask = TaskExtras.RejectIf(
        (int value) => value % 2 != 0,
        value => new ArgumentException()
      )(1);

      await Task.Delay(100);

      Assert.True(testTask.IsFaulted);
    }

    [Fact]
    public async void ItShouldThrowTheExpectedExceptionForASuccessfulPredicate()
    {
      Task<int> testTask = TaskExtras.RejectIf(
        (int value) => value % 2 != 0,
        value => new ArgumentException()
      )(1);

      await Task.Delay(100);

      await Assert.ThrowsAsync<ArgumentException>(async () => await testTask);
    }

    [Fact]
    public async void ItShouldResolveForAFailedPredicate()
    {
      Task<int> testTask = TaskExtras.RejectIf(
        (int value) => value % 2 != 0,
        value => new ArgumentException()
      )(2);

      await Task.Delay(100);

      Assert.True(testTask.IsCompletedSuccessfully);
    }
  }

  public class ReRejectIf
  {
    [Fact]
    public async Task ItShouldReRejectForSuccessfulPredicate()
    {
      string expectedMessage = Guid.NewGuid().ToString();
      Task<int> testTask = Task.FromException<int>(new ArgumentNullException())
        .Catch(TaskExtras.ReRejectIf<int>(
          exception => exception is ArgumentNullException,
          exception => new ArgumentException(expectedMessage, exception)
        ));
      Exception thrownException = new();

      try
      {
        await testTask;
      }
      catch(ArgumentException exception)
      {
        thrownException = exception;
      }

      Assert.Equal(expectedMessage, thrownException.Message);
    }

    [Fact]
    public async Task ItShouldNotReRejectForFailedPredicate()
    {
      string expectedMessage = Guid.NewGuid().ToString();
      Task<int> testTask = Task.FromException<int>(new ArgumentException(expectedMessage))
        .Catch(TaskExtras.ReRejectIf<int>(
          exception => exception is StackOverflowException,
          exception => new ArgumentNullException(Guid.NewGuid().ToString(), exception)
        ));
      Exception thrownException = new();

      try
      {
        await testTask;
      }
      catch (ArgumentException exception)
      {
        thrownException = exception;
      }

      Assert.Equal(expectedMessage, thrownException.Message);
    }
  }

  public class ResolveIf
  {
    public class WithRawResolutionSupplier
    {
      [Fact]
      public async void ItShouldResolveForASuccessfulPredicate()
      {
        Task<int> testTask = TaskExtras.ResolveIf(
          (Exception value) => value is ArgumentException,
          value => value.Message.Length
        )(new ArgumentException());

        await Task.Delay(100);

        Assert.True(testTask.IsCompletedSuccessfully);
      }

      [Fact]
      public async void ItShouldRejectForAFailedPredicate()
      {
        Task<int> testTask = TaskExtras.ResolveIf(
          (Exception value) => value is ArgumentException,
          value => value.Message.Length
        )(new NullReferenceException());

        await Task.Delay(100);

        Assert.True(testTask.IsFaulted);
      }
    }

    public class WithTaskResolutionSupplier
    {
      [Fact]
      public async void ItShouldResolveForASuccessfulPredicate()
      {
        Task<int> testTask = TaskExtras.ResolveIf(
          (Exception value) => value is ArgumentException,
          value => Task.FromResult(value.Message.Length)
        )(new ArgumentException());

        await Task.Delay(100);

        Assert.True(testTask.IsCompletedSuccessfully);
      }

      [Fact]
      public async void ItShouldRejectForAFailedPredicate()
      {
        Task<int> testTask = TaskExtras.ResolveIf(
          (Exception value) => value is ArgumentException,
          value => Task.FromResult(value.Message.Length)
        )(new NullReferenceException());

        await Task.Delay(100);

        Assert.True(testTask.IsFaulted);
      }
    }
  }

  public class Defer
  {
    [Fact]
    public async Task ItShouldWaitTheConfiguredTime()
    {
      Stopwatch testStopWatch = new();
      int testDeferTimeMilliseconds = 100;

      testStopWatch.Start();

      await TaskExtras.Defer(() => 1, TimeSpan.FromMilliseconds(testDeferTimeMilliseconds));

      testStopWatch.Stop();

      Assert.InRange(
        testStopWatch.ElapsedMilliseconds,
        testDeferTimeMilliseconds - (testDeferTimeMilliseconds * 0.1),
        testDeferTimeMilliseconds + (testDeferTimeMilliseconds * 0.1)
      );
    }
  }

  public class Retry
  {
    [Fact]
    public async Task ItShouldMakeTheConfiguredNumberOfAttempts()
    {
      int actualValue = 0;
      Func<int> testFunc = () =>
      {
        actualValue += 1;

        throw new Exception();
      };
      int expectedValue = 3;

      try
      {
        await TaskExtras.Retry(testFunc);
      }
      catch(RetryException)
      {
      }

      Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public async Task ItShouldThrowARetryExceptionAfterTheConfiguredNumberOfAttempts()
    {
      string expectedMessage = "Retries exhausted after 3 attempts";

      Func<int> testFunc = () =>
      {
        throw new Exception();
      };

      Exception thrownException = await Assert.ThrowsAsync<RetryException>(async () => await TaskExtras.Retry(testFunc));

      Assert.Equal(expectedMessage, thrownException.Message);
    }

    [Fact]
    public async Task ItShouldPassIfARetrySucceeds()
    {
      int actualValue = 0;
      Func<int> testFunc = () =>
      {
        actualValue += 1;

        if (actualValue < 3)
        {
          throw new Exception();
        }
        else
        {
          return actualValue;
        }
      };
      int expectedValue = 3;

      await TaskExtras.Retry(testFunc);

      Assert.Equal(expectedValue, actualValue);
    }
  }
}
