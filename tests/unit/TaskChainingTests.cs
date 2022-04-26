using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Jds.TestingUtils.MockHttp;
using RLC.TaskChaining;
using Xunit;

namespace RLC.TaskChainingTests;

public class TaskChainingTests
{
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

  public class Then
  {
    public class ForTtoTNext
    {
      [Fact]
      public async void ItShouldTransition()
      {
        int expectedValue = 5;
        int actualValue = await Task.FromResult("12345")
          .Then(str => str.Length);

        Assert.Equal(expectedValue, actualValue);
      }

      [Fact]
      public async void ItShouldCompleteWithoutAwaiting()
      {
        int expectedValue = 5;
        int actualValue = 0;

        _ = Task.FromResult("12345")
          .Then(str =>
          {
            actualValue = str.Length;

            return actualValue;
          });

        await Task.Delay(100);

        Assert.Equal(expectedValue, actualValue);
      }

      [Fact]
      public async void ItShouldFaultForThrownExceptions()
      {
        Func<string, int> testFunc = _ => throw new Exception();

        Task<int> testTask = Task.FromResult("abc")
          .Then(testFunc);

        try
        {
          await testTask;
        }
        catch { }

        Assert.True(testTask.IsFaulted);
      }

      [Fact]
      public async void ItShouldRethrowForFaults()
      {
        Task<int> testTask = Task.FromException<string>(new ArgumentNullException("abcde"))
          .Then(value => value.Length);

        await Assert.ThrowsAsync<ArgumentNullException>(async () => await testTask);
      }

      [Fact]
      public async void ItShouldNotRunForAFault()
      {
        Exception testException = new(Guid.NewGuid().ToString());

        await Assert.ThrowsAsync<Exception>(
          async () => await Task.FromException<string>(testException)
            .Then(str => str.Length)
        );
      }

      [Fact]
      public async void ItShouldReportFaultedForThrownException()
      {
        Func<string, int> testFunc = _ => throw new ArgumentException();
        Task<int> testTask = Task.FromResult("12345").Then(testFunc);

        try
        {
          await testTask;
        }
        catch { }

        Assert.True(testTask.IsFaulted);
      }

      [Fact]
      public async void ItShouldThrowFaultedException()
      {
        Func<string, int> testFunc = _ => throw new ArgumentException();

        await Assert.ThrowsAsync<ArgumentException>(
          async () => await Task.FromResult("12345").Then(testFunc)
        );
      }
    }

    public class ForTtoTaskTNext
    {
      [Fact]
      public async void ItShouldTransition()
      {
        int expectedValue = 5;
        int actualValue = await Task.FromResult<string>("12345")
          .Then(str => Task.FromResult(str.Length));

        Assert.Equal(expectedValue, actualValue);
      }

      [Fact]
      public async void ItShouldCompleteWithoutAwaiting()
      {
        int expectedValue = 5;
        int actualValue = 0;

        _ = Task.FromResult<string>("12345")
          .Then(str =>
          {
            actualValue = str.Length;

            return Task.FromResult(actualValue);
          });

        await Task.Delay(100);

        Assert.Equal(expectedValue, actualValue);
      }

      [Fact]
      public async void ItShouldNotRunForAFault()
      {
        Exception testException = new(Guid.NewGuid().ToString());

        await Assert.ThrowsAsync<Exception>(
          async () => await Task.FromException<string>(testException)
            .Then(str => Task.FromResult(str.Length))
        );
      }

      [Fact]
      public async void ItShouldContinueAsyncTasks()
      {
        int expectedValue = 5;
        int actualValue = 0;

        _ = Task.FromResult("12345")
          .Then(async str =>
          {
            await Task.Delay(1);

            actualValue = str.Length;

            return Task.FromResult(str.Length);
          });

        await Task.Delay(100);

        Assert.Equal(expectedValue, actualValue);
      }

      [Fact]
      public async void ItShouldContinueAsyncTasksWithoutAwaiting()
      {
        int expectedValue = 5;
        int actualValue = 0;

        _ = Task.FromResult("12345")
          .Then(async str =>
          {
            await Task.Delay(1);

            actualValue = str.Length;

            return Task.FromResult(actualValue);
          });

        await Task.Delay(100);

        Assert.Equal(expectedValue, actualValue);
      }

      [Fact]
      public async void ItShouldReportFaultedForThrownException()
      {
        Func<string, Task<int>> testFunc = _ => throw new ArgumentException();
        Task<int> testTask = Task.FromResult("12345").Then(testFunc);

        try
        {
          await testTask;
        }
        catch { }

        Assert.True(testTask.IsFaulted);
      }

      [Fact]
      public async void ItShouldThrowFaultedException()
      {
        Func<string, Task<int>> testFunc = _ => throw new ArgumentException();

        await Assert.ThrowsAsync<ArgumentException>(
          async () => await Task.FromResult("12345").Then(testFunc)
        );
      }

      [Fact]
      public async void ItShouldReportFaultedForThrownExceptionFromAsync()
      {
        Func<string, Task<int>> testFunc = async _ =>
        {
          await Task.Delay(1);
          throw new ArgumentException();
        };
        Task<int> testTask = Task.FromResult("12345").Then(testFunc);

        try
        {
          await testTask;
        }
        catch { }

        Assert.True(testTask.IsFaulted);
      }

      [Fact]
      public async void ItShouldThrowFaultedExceptionFromAsync()
      {
        Func<string, Task<int>> testFunc = async _ =>
        {
          await Task.Delay(1);
          throw new ArgumentException();
        };

        await Assert.ThrowsAsync<ArgumentException>(
          async () => await Task.FromResult("12345").Then(testFunc)
        );
      }

      [Fact]
      public async void ItShouldFaultForThrownExceptions()
      {
        Func<string, Task<int>> testFunc = _ => throw new Exception();

        Task<int> testTask = Task.FromResult("abc")
          .Then(testFunc);

        try
        {
          await testTask;
        }
        catch { }

        Assert.True(testTask.IsFaulted);
      }

      [Fact]
      public async void ItShouldCaptureTaskCancellation()
      {
        HttpClient testHttpClient = new MockHttpBuilder()
          .WithHandler(messageCaseBuilder => messageCaseBuilder.AcceptAll()
            .RespondWith((responseBuilder, _) => responseBuilder.WithStatusCode(HttpStatusCode.OK))
          )
          .BuildHttpClient();
        ;
        CancellationTokenSource testTokenSource = new();
        testTokenSource.Cancel();

        Task<string> testTask = Task.FromResult<string>("http://anything.anywhere")
          .Then(async url => await testHttpClient.GetStringAsync(url, testTokenSource.Token));

        await Assert.ThrowsAsync<TaskCanceledException>(async () => await testTask);
      }
    }
  }

  public class Tap
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

      await Task.Delay(100);

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

      await Task.Delay(100);

      Assert.Equal(expectedValue, actualValue);
    }
  }

  public class IfResolved
  {
    [Fact]
    public async void ItShouldPerformASideEffect()
    {
      int actualValue = 0;
      int expectedValue = 5;

      await Task.FromResult(5)
        .IfResolved(value =>
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
        .IfResolved(value =>
        {
          actualValue = value;
        });

      await Task.Delay(100);

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
          .IfResolved(value =>
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
        .IfResolved(_ =>
        {
          actualValue = 5;
        });

      await Task.Delay(100);

      Assert.Equal(expectedValue, actualValue);
    }
  }

  public class IfRejected
  {
    [Fact]
    public async void ItShouldPerformASideEffectWithoutAwaiting()
    {
      int actualValue = 0;
      int expectedValue = 5;

      _ = Task.FromException<int>(new ArgumentNullException())
        .IfRejected(_ =>
        {
          actualValue = 5;
        });

      await Task.Delay(100);

      Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public async void ItShouldNotPerformASideEffectForAResolution()
    {
      int actualValue = 0;
      int expectedValue = 0;

      try
      {
        await Task.FromResult(5)
          .IfRejected(_ =>
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
    public async void ItShouldNotPerformASideEffectForAResolutionWithoutAwaiting()
    {
      int actualValue = 0;
      int expectedValue = 0;

      _ = Task.FromResult(5)
        .IfRejected(_ =>
        {
          actualValue = 5;
        });

      await Task.Delay(100);

      Assert.Equal(expectedValue, actualValue);
    }
  }

  public class Retry
  {
    public static RetryOptions TestRetryOptions = new(3, 100, 2, (_, _, _) => { }, exception => true);

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

          if (count < 3)
          {
            throw new Exception();
          }
          else
          {
            return s.Length;
          }
        };
        int expectedValue = 5;

        int actualValue = await Task.FromResult("abcde").Retry(testFunc, Retry.TestRetryOptions);

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

          if (count < 3)
          {
            throw new Exception();
          }
          else
          {
            return Task.FromResult(s.Length);
          }
        };
        int expectedValue = 5;

        int actualValue = await Task.FromResult("abcde").Retry(testFunc, Retry.TestRetryOptions);

        Assert.Equal(expectedValue, actualValue);
      }
    }
  }
}
