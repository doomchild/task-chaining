﻿using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Jds.TestingUtils.MockHttp;
using RLC.TaskChaining;
using Xunit;

using static RLC.TaskChaining.TaskStatics;

namespace RLC.TaskChainingTests;

public class TaskChainingTests
{
  public class Ap
  {
    public class WithRawReturningFunc
    {
      private int TestFunc(string s) => s.Length;

      [Fact]
      public async void ItShouldTransition()
      {
        string testValue = "12345";
        int expectedValue = 5;
        int actualValue = await Task.FromResult(testValue)
          .Ap(Task.FromResult(TestFunc));

        Assert.Equal(expectedValue, actualValue);
      }

      [Fact]
      public async void ItShouldContinueAsyncTasks()
      {
        string testValue = "12345";
        int expectedValue = 5;
        int actualValue = 0;

        _ = Task.FromResult(testValue)
          .Ap(Task.FromResult(TestFunc))
          .IfFulfilled(async value =>
          {
            await Task.Delay(1);

            actualValue = value;
          });

        await Task.Delay(5);

        Assert.Equal(expectedValue, actualValue);
      }

      [Fact]
      public async void ItShouldNotRunForAFault()
      {
        Exception testException = new(Guid.NewGuid().ToString());

        await Assert.ThrowsAsync<Exception>(
          async () => await Task.FromException<string>(testException)
          .Ap(Task.FromResult(TestFunc))
        );
      }

      [Fact]
      public async void ItShouldNotRunForAFaultedFunctionTask()
      {
        string testValue = "12345";
        Exception testException = new(Guid.NewGuid().ToString());

        await Assert.ThrowsAsync<Exception>(
          async () => await Task.FromResult(testValue)
          .Ap(Task.FromException<Func<string, int>>(testException))
        );
      }
    }
  }

  public class Alt
  {
    public class WithRawValue
    {
      [Fact]
      public async Task ItShouldNotReplaceForAFulfillment()
      {
        int expectedValue = 1;
        int actualValue = await Task.FromResult(1).Alt(Task.FromResult(2));

        Assert.Equal(expectedValue, actualValue);
      }

      [Fact]
      public async Task ItShouldReplaceForAFault()
      {
        int expectedValue = 2;
        int actualValue = await Task.FromException<int>(new Exception()).Alt(Task.FromResult(2));

        Assert.Equal(expectedValue, actualValue);
      }
    }

    public class WithSupplier
    {
      [Fact]
      public async Task ItShouldNotReplaceForAFulfillment()
      {
        int expectedValue = 1;
        int actualValue = await Task.FromResult(1).Alt(() => Task.FromResult(2));

        Assert.Equal(expectedValue, actualValue);
      }

      [Fact]
      public async Task ItShouldReplaceForAFault()
      {
        int expectedValue = 2;
        int actualValue = await Task.FromException<int>(new Exception()).Alt(() => Task.FromResult(2));

        Assert.Equal(expectedValue, actualValue);
      }
    }
  }

  public class Catch
  {
    public class ForExceptionTtoT
    {
      [Fact]
      public async void ItShouldReportUnfaultedAfterCatching()
      {
        Func<string, int> testFunc = _ => throw new ArgumentException();
        Task<int> testTask = Task.FromResult("abcde")
          .Then(testFunc)
          .Catch(ex => ex.Message.Length);

        await testTask;

        Assert.True(testTask.IsCompletedSuccessfully);
      }

      [Fact]
      public async void ItShouldReportUnfaultedAfterCatchingFromAsync()
      {
        Func<string, Task<int>> testFunc = async _ => { await Task.Delay(1); throw new ArgumentException(); };
        Task<int> testTask = Task.FromResult("12345")
          .Then(testFunc)
          .Catch(ex => ex.Message.Length);

        await testTask;

        Assert.True(testTask.IsCompletedSuccessfully);
      }

      [Fact]
      public async void ItShouldReturnTheFulfilledValueAfterCatching()
      {
        Func<string, int> testFunc = _ => throw new ArgumentException();
        int expectedValue = 46;
        int actualValue = await Task.FromResult("12345")
          .Then(testFunc)
          .Catch(ex => ex.Message.Length);

        Assert.Equal(expectedValue, actualValue);
      }
    }

    public class ForExceptionToTaskT
    {
      [Fact]
      public async void ItShouldReportUnfaultedAfterCatching()
      {
        Func<string, int> testFunc = _ => throw new ArgumentException();
        Task<int> testTask = Task.FromResult("12345")
          .Then(testFunc)
          .Catch(ex => Task.FromResult(ex.Message.Length));

        await testTask;

        Assert.True(testTask.IsCompletedSuccessfully);
      }

      [Fact]
      public async void ItShouldReportUnfaultedAfterCatchingFromAsync()
      {
        Func<string, Task<int>> testFunc = async _ => { await Task.Delay(1); throw new ArgumentException(); };
        Task<int> testTask = Task.FromResult("12345")
          .Then(testFunc)
          .Catch(ex => Task.FromResult(ex.Message.Length));

        await testTask;

        Assert.True(testTask.IsCompletedSuccessfully);
      }

      [Fact]
      public async void ItShouldReturnTheFulfilledValueAfterCatching()
      {
        Func<string, int> testFunc = _ => throw new ArgumentException();
        int expectedValue = 46;
        int actualValue = await Task.FromResult("12345")
          .Then(testFunc)
          .Catch(ex => Task.FromResult(ex.Message.Length));

        Assert.Equal(expectedValue, actualValue);
      }

      [Fact]
      public async void ItShouldReturnTheFulfilledValueAfterCatchingFromAsync()
      {
        ArgumentException testException = new();
        Func<string, int> testFunc = _ => throw testException;
        int expectedValue = 46;
        int actualValue = await Task.FromResult("12345")
          .Then(testFunc)
          .Catch(async ex =>
          {
            await Task.Delay(1);
            return ex.Message.Length;
          });

        Assert.Equal(expectedValue, actualValue);
      }
    }
  }

  public class CatchWhen
  {
    public class ForExceptionToT
    {
      [Fact]
      public async void ItShouldReportUnfaultedForSpecificExceptionType()
      {
        Func<string, int> testFunc = _ => throw new ArgumentException();
        Task<int> testTask = Task.FromResult("abcde")
          .Then(testFunc)
          .CatchWhen<int, ArgumentException>(ex => ex.Message.Length);

        await testTask;

        Assert.True(testTask.IsCompletedSuccessfully);
      }

      [Fact]
      public async void ItShouldNotCatchForNonMatchingExceptionType()
      {
        Func<string, int> testFunc = _ => throw new ArgumentException();
        Task<int> testTask = Task.FromResult("abcde")
          .Then(testFunc)
          .CatchWhen<int, NullReferenceException>(ex => ex.Message.Length);

        await Assert.ThrowsAsync<ArgumentException>(async () => await testTask);
      }
    }

    public class ForExceptionToTaskT
    {
      [Fact]
      public async void ItShouldReportUnfaultedForSpecificExceptionType()
      {
        Func<string, int> testFunc = _ => throw new ArgumentException();
        Task<int> testTask = Task.FromResult("abcde")
          .Then(testFunc)
          .CatchWhen<int, ArgumentException>(ex => Task.FromResult(ex.Message.Length));

        await testTask;

        Assert.True(testTask.IsCompletedSuccessfully);
      }

      [Fact]
      public async void ItShouldNotCatchForNonMatchingExceptionType()
      {
        Func<string, int> testFunc = _ => throw new ArgumentException();
        Task<int> testTask = Task.FromResult("abcde")
          .Then(testFunc)
          .CatchWhen<int, NullReferenceException>(ex => Task.FromResult(ex.Message.Length));

        await Assert.ThrowsAsync<ArgumentException>(async () => await testTask);
      }
    }
  }

  public class Delay
  {
    [Fact]
    public async Task ItShouldWaitTheConfiguredTime()
    {
      Stopwatch testStopWatch = new();
      int testDelayIntervalMilliseconds = 50;

      testStopWatch.Start();

      await Task.FromResult(1).Delay(TimeSpan.FromMilliseconds(testDelayIntervalMilliseconds));

      testStopWatch.Stop();

      Assert.True(testStopWatch.ElapsedMilliseconds >= testDelayIntervalMilliseconds);
    }
  }

  public class Fault
  {
    public class ForTtoTo
    {
      [Fact]
      public async Task ItShouldFaultAFulfilledTask()
      {
        Task<int> testTask = Task.FromResult(2).Fault(new Exception());

        await Task.Delay(2);

        Assert.True(testTask.IsFaulted);
      }

      [Fact]
      public async Task ItShouldNotFaultAFaultedTask()
      {
        Task<int> testTask = Task.FromException<int>(new NullReferenceException()).Fault(new ArgumentException());
        Exception actualValue = new Exception();

        try
        {
          await testTask;
        }
        catch (Exception exception)
        {
          actualValue = exception;
        }

        Assert.IsType<NullReferenceException>(actualValue);
      }
    }

    public class ForTtoTNext
    {
      [Fact]
      public async Task ItShouldFaultAFulfilledTask()
      {
        Task<string> testTask = Task.FromResult(2).Fault<int, string>(new Exception());

        await Task.Delay(2);

        Assert.True(testTask.IsFaulted);
      }

      [Fact]
      public async Task ItShouldNotFaultAFaultedTask()
      {
        Task<string> testTask = Task.FromException<int>(new NullReferenceException())
          .Fault<int, string>(new ArgumentException());
        Exception actualValue = new Exception();

        try
        {
          await testTask;
        }
        catch (Exception exception)
        {
          actualValue = exception;
        }

        Assert.IsType<NullReferenceException>(actualValue);
      }
    }
  }

  public class Retry
  {
    public static RetryParams TestRetryOptions = new(3, TimeSpan.FromMilliseconds(10), 2, (_, _, _) => { }, exception => true);

    public class ForTtoTNext
    {
      [Fact]
      public async Task ItShouldMakeTheConfiguredNumberOfAttempts()
      {
        int actualValue = 0;
        Func<string, int> testFunc = s =>
        {
          actualValue++;

          throw new Exception();
        };
        int expectedValue = 3;

        try
        {
          await Task.FromResult("abc")
            .Retry(testFunc, Retry.TestRetryOptions);
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
        Func<string, int> testFunc = _ =>
        {
          throw new Exception();
        };

        Exception thrownException = await Assert.ThrowsAsync<RetryException>(async () => await Task.FromResult("abc").Retry(testFunc, Retry.TestRetryOptions));

        Assert.Equal(expectedMessage, thrownException.Message);
      }

      [Fact]
      public async Task ItShouldPassIfARetrySucceeds()
      {
        int count = 0;
        Func<string, int> testFunc = s =>
        {
          count++;

          if (count < 2)
          {
            throw new Exception();
          }
          else
          {
            return s.Length;
          }
        };
        int expectedValue = 5;

        int actualValue = await Task.FromResult("abcde").Retry(testFunc);

        Assert.Equal(expectedValue, actualValue);
      }
    }

    public class ForTtoTaskTNext
    {
      [Fact]
      public async Task ItShouldMakeTheConfiguredNumberOfAttempts()
      {
        int actualValue = 0;
        Func<string, Task<int>> testFunc = s =>
        {
          actualValue++;

          throw new Exception();
        };
        int expectedValue = 3;

        try
        {
          await Task.FromResult("abc")
            .Retry(testFunc, Retry.TestRetryOptions);
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
        Func<string, Task<int>> testFunc = _ =>
        {
          throw new Exception();
        };

        Exception thrownException = await Assert.ThrowsAsync<RetryException>(async () => await Task.FromResult("abc").Retry(testFunc, Retry.TestRetryOptions));

        Assert.Equal(expectedMessage, thrownException.Message);
      }

      [Fact]
      public async Task ItShouldPassIfARetrySucceeds()
      {
        int count = 0;
        Func<string, Task<int>> testFunc = s =>
        {
          count++;

          if (count < 2)
          {
            throw new Exception();
          }
          else
          {
            return Task.FromResult(s.Length);
          }
        };
        int expectedValue = 5;

        int actualValue = await Task.FromResult("abcde").Retry(testFunc);

        Assert.Equal(expectedValue, actualValue);
      }
    }
  }
}
