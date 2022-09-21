using System;
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

        await Task.Delay(10);

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

        await Task.Delay(10);

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

        await Task.Delay(10);

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

  public class Filter
  {
    public class WithRawValue
    {
      [Fact]
      public async Task ItShouldNotFaultForASuccessfulPredicate()
      {
        Task<int> testTask = Task.FromResult(2).Filter(value => value % 2 == 0, new Exception("not even"));

        await Task.Delay(10);

        Assert.True(testTask.IsCompletedSuccessfully);
      }

      [Fact]
      public async Task ItShouldNotChangeTheValueForASuccessfulPredicate()
      {
        int expectedValue = 2;
        int actualValue = await Task.FromResult(2).Filter(value => value % 2 == 0, new Exception("not even"));

        Assert.Equal(expectedValue, actualValue);
      }

      [Fact]
      public async Task ItShouldTransitionForAFailedPredicate()
      {
        string expectedMessage = Guid.NewGuid().ToString();
        Task<int> testTask = Task.FromResult(1).Filter(value => value % 2 == 0, new ArgumentException(expectedMessage));
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
    }

    public class WithMorphism
    {
      public class WithRawMorphism
      {
        [Fact]
        public async Task ItShouldNotFaultForASuccessfulPredicate()
        {
          Task<int> testTask = Task.FromResult(2).Filter(value => value % 2 == 0, value => new Exception("not even"));

          await Task.Delay(10);

          Assert.True(testTask.IsCompletedSuccessfully);
        }

        [Fact]
        public async Task ItShouldNotChangeTheValueForASuccessfulPredicate()
        {
          int expectedValue = 2;
          int actualValue = await Task.FromResult(2).Filter(value => value % 2 == 0, value => new Exception("not even"));

          Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public async Task ItShouldTransitionForAFailedPredicate()
        {
          string expectedMessage = Guid.NewGuid().ToString();
          Task<int> testTask = Task.FromResult(1).Filter(value => value % 2 == 0, value => new ArgumentException(expectedMessage));
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

      public class WithTaskSupplier
      {
        [Fact]
        public async Task ItShouldNotFaultForASuccessfulPredicate()
        {
          Task<int> testTask = Task.FromResult(2).Filter(
            value => value % 2 == 0,
            () => Task.FromResult(new Exception("not even"))
          );

          await Task.Delay(10);

          Assert.True(testTask.IsCompletedSuccessfully);
        }

        [Fact]
        public async Task ItShouldNotChangeTheValueForASuccessfulPredicate()
        {
          int expectedValue = 2;
          int actualValue = await Task.FromResult(2).Filter(
            value => value % 2 == 0,
            () => Task.FromResult(new Exception("not even"))
          );

          Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public async Task ItShouldTransitionForAFailedPredicate()
        {
          string expectedMessage = Guid.NewGuid().ToString();
          Task<int> testTask = Task.FromResult(1).Filter(
            value => value % 2 == 0,
            () => Task.FromResult(new ArgumentException(expectedMessage))
          );
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

      public class WithTaskMorphism
      {
        [Fact]
        public async Task ItShouldNotFaultForASuccessfulPredicate()
        {
          Task<int> testTask = Task.FromResult(2).Filter(
            value => value % 2 == 0,
            _ => Task.FromResult(new Exception("not even"))
          );

          await Task.Delay(10);

          Assert.True(testTask.IsCompletedSuccessfully);
        }

        [Fact]
        public async Task ItShouldNotChangeTheValueForASuccessfulPredicate()
        {
          int expectedValue = 2;
          int actualValue = await Task.FromResult(2).Filter(
            value => value % 2 == 0,
            _ => Task.FromResult(new Exception("not even"))
          );

          Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public async Task ItShouldTransitionForAFailedPredicate()
        {
          string expectedMessage = Guid.NewGuid().ToString();
          Task<int> testTask = Task.FromResult(1).Filter(
            value => value % 2 == 0,
            _ => Task.FromResult(new ArgumentException(expectedMessage))
          );
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
    }
  }

  public class Then
  {
    public class ForOnlyFulfilled
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

          await Task.Delay(10);

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

          await Task.Delay(10);

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

          await Task.Delay(50);

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

          await Task.Delay(10);

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

    public class ForBoth
    {
      public class ForTtoTNext
      {
        [Fact]
        public async void ItShouldTransitionAFulfilledTask()
        {
          int expectedValue = 5;
          int actualValue = await Task.FromResult("12345")
            .Then(str => str.Length, Constant<Exception, int>(0));

          Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public async void ItShouldTransitionAFaultBackIntoAFulfillment()
        {
          int expectedValue = 5;
          int actualValue = await Task.FromException<string>(new Exception())
            .Then(s => s.Length, Constant<Exception, int>(5));

          Assert.Equal(expectedValue, actualValue);
        }
      }

      public class ForTtoTaskTNext
      {
        [Fact]
        public async void ItShouldTransitionAFulfilledTask()
        {
          int expectedValue = 5;
          int actualValue = await Task.FromResult("12345")
            .Then(str => Task.FromResult(str.Length), Constant<Exception, Task<int>>(Task.FromResult(0)));

          Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public async void ItShouldTransitionAFaultBackIntoAFulfillment()
        {
          int expectedValue = 5;
          int actualValue = await Task.FromException<string>(new Exception())
            .Then(s => Task.FromResult(s.Length), Constant<Exception, Task<int>>(Task.FromResult(5)));

          Assert.Equal(expectedValue, actualValue);
        }
      }

      public class ForTaskOnlyOnFulfillment
      {
        [Fact]
        public async void ItShouldTransitionAFulfilledTask()
        {
          int expectedValue = 5;
          int actualValue = await Task.FromResult("12345")
            .Then(str => Task.FromResult(str.Length), Constant<Exception, int>(0));

          Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public async void ItShouldTransitionAFaultBackIntoAFulfillment()
        {
          int expectedValue = 5;
          int actualValue = await Task.FromException<string>(new Exception())
            .Then(s => Task.FromResult(s.Length), Constant<Exception, int>(5));

          Assert.Equal(expectedValue, actualValue);
        }
      }

      public class ForTaskOnlyOnFaulted
      {
        [Fact]
        public async void ItShouldTransitionAFulfilledTask()
        {
          int expectedValue = 5;
          int actualValue = await Task.FromResult("12345")
            .Then(str => str.Length, Constant<Exception, Task<int>>(Task.FromResult(0)));

          Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public async void ItShouldTransitionAFaultBackIntoAFulfillment()
        {
          int expectedValue = 5;
          int actualValue = await Task.FromException<string>(new Exception())
            .Then(s => s.Length, Constant<Exception, Task<int>>(Task.FromResult(5)));

          Assert.Equal(expectedValue, actualValue);
        }
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
    }
  }

  public class IfFulfilled
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

      await Task.Delay(10);

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

      await Task.Delay(10);

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
        .IfFaulted((Exception _) =>
        {
          actualValue = 5;
        });

      await Task.Delay(10);

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
          .IfFaulted((Exception _) =>
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
        .IfFaulted((Exception _) =>
        {
          actualValue = 5;
        });

      await Task.Delay(10);

      Assert.Equal(expectedValue, actualValue);
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
