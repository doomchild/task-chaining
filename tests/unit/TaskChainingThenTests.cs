using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Jds.TestingUtils.MockHttp;
using RLC.TaskChaining;
using Xunit;

using static RLC.TaskChaining.TaskStatics;

namespace RLC.TaskChainingTests;

public class Then
{
  private static async Task<int> TestFunc(string s, CancellationToken cancellationToken)
  {
    return await Task<string>.Run(() => s.Length);
  }

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

        await Task.Delay(2);

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

        await Task.Delay(2);

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

        await Task.Delay(5);

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

        await Task.Delay(5);

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

        CancellationTokenSource testTokenSource = new();
        testTokenSource.Cancel();

        Task<string> testTask = Task.FromResult<string>("https://www.google.com")
          .Then(async url => await testHttpClient.GetStringAsync(url, testTokenSource.Token))
          .Then(_ => Guid.NewGuid().ToString());

        await Task.Delay(50);

        Assert.True(testTask.IsFaulted);
      }
    }

    public class ForCancellationTokenOnFulfilled
    {
      [Fact]
      public async void ItShouldTransition()
      {
        int expectedValue = 5;
        int actualValue = await Task.FromResult<string>("12345")
          .Then(TestFunc, CancellationToken.None);

        Assert.Equal(expectedValue, actualValue);
      }

      [Fact]
      public async void ItShouldNotRunForAFault()
      {
        Exception testException = new(Guid.NewGuid().ToString());

        await Assert.ThrowsAsync<Exception>(
          async () => await Task.FromException<string>(testException)
          .Then(TestFunc, CancellationToken.None)
        );
      }

      [Fact]
      public async void ItShouldReportFaultedForThrownException()
      {
        Func<string, CancellationToken, Task<int>> testFunc = (_, _) => throw new ArgumentException();
        Task<int> testTask = Task.FromResult("12345").Then(testFunc, CancellationToken.None);

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
        Func<string, CancellationToken, Task<int>> testFunc = (_, _) => throw new ArgumentException();

        await Assert.ThrowsAsync<ArgumentException>(
          async () => await Task.FromResult("12345").Then(testFunc, CancellationToken.None)
        );
      }

      [Fact]
      public async void ItShouldReportFaultedForThrownExceptionFromAsync()
      {
        Func<string, CancellationToken, Task<int>> testFunc = async (_, _) =>
        {
          await Task.Delay(1);
          throw new ArgumentException();
        };
        Task<int> testTask = Task.FromResult("12345").Then(testFunc, CancellationToken.None);

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
        Func<string, CancellationToken, Task<int>> testFunc = async (_, _) =>
        {
          await Task.Delay(1);
          throw new ArgumentException();
        };

        await Assert.ThrowsAsync<ArgumentException>(
          async () => await Task.FromResult("12345").Then(testFunc, CancellationToken.None)
        );
      }

      [Fact]
      public async void ItShouldFaultForThrownExceptions()
      {
        Func<string, CancellationToken, Task<int>> testFunc = (_, _) => throw new Exception();

        Task<int> testTask = Task.FromResult("abc")
          .Then(testFunc, CancellationToken.None);

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

        CancellationTokenSource testTokenSource = new();
        testTokenSource.Cancel();

        Task<string> testTask = Task.Run(() => "https://www.google.com", testTokenSource.Token)
          .Then(testHttpClient.GetStringAsync, CancellationToken.None)
          .Then(_ => Guid.NewGuid().ToString());

        await Task.Delay(5);

        Assert.True(testTask.IsFaulted);
      }
    }

    public async void ItShouldCaptureTaskCancellationException()
    {
      HttpClient testHttpClient = new MockHttpBuilder()
        .WithHandler(messageCaseBuilder => messageCaseBuilder.AcceptAll()
        .RespondWith((responseBuilder, _) => responseBuilder.WithStatusCode(HttpStatusCode.OK))
        )
        .BuildHttpClient();

      CancellationTokenSource testTokenSource = new();
      testTokenSource.Cancel();

      Task<string> testTask = Task.FromResult<string>("https://www.google.com")
        .Then(testHttpClient.GetStringAsync, testTokenSource.Token)
        .Then(_ => Guid.NewGuid().ToString());

      await Task.Delay(50);

      await Assert.ThrowsAsync<TaskCanceledException>(async () => await testTask);
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

    public class ForCancellationTokenFuncException
    {
      [Fact]
      public async void ItShouldTransitionAFulfilledTask()
      {
        int expectedValue = 5;
        int actualValue = await Task.FromResult("12345")
          .Then(TestFunc, () => new NullReferenceException(), CancellationToken.None);

        Assert.Equal(expectedValue, actualValue);
      }

      [Fact]
      public async void ItShouldTransitionAFaultToAnotherExceptionType()
      {
        Task<int> testTask = Task.FromException<string>(new ArgumentException())
          .Then(TestFunc, () => new NullReferenceException(), CancellationToken.None);

        await Assert.ThrowsAsync<NullReferenceException>(async () => await testTask);
      }

      [Fact]
      public async void ItShouldCaptureTaskCancellation()
      {
        HttpClient testHttpClient = new MockHttpBuilder()
          .WithHandler(messageCaseBuilder => messageCaseBuilder.AcceptAll()
            .RespondWith((responseBuilder, _) => responseBuilder.WithStatusCode(HttpStatusCode.OK))
          )
          .BuildHttpClient();

        CancellationTokenSource testTokenSource = new();
        testTokenSource.Cancel();

        Task<string> testTask = Task.Run(() => Task.FromException<string>(new Exception()), testTokenSource.Token)
          .Then(testHttpClient.GetStringAsync, () => new NullReferenceException(), CancellationToken.None)
          .Then(_ => Guid.NewGuid().ToString());

        await Task.Delay(5);

        Assert.True(testTask.IsFaulted);
      }
    }

    public class ForCancellationTokenTNextOnFaulted
    {
      [Fact]
      public async void ItShouldTransitionAFulfilledTask()
      {
        int expectedValue = 5;
        int actualValue = await Task.FromResult("12345")
          .Then(TestFunc, () => 100, CancellationToken.None);

        Assert.Equal(expectedValue, actualValue);
      }

      [Fact]
      public async void ItShouldTransitionAFaultToASuccessfulValue()
      {
        int expectedValue = 10;
        int actualValue = await Task.FromException<string>(new ArgumentException())
          .Then(TestFunc, () => expectedValue, CancellationToken.None);

        Assert.Equal(expectedValue, actualValue);
      }

      [Fact]
      public async void ItShouldCaptureTaskCancellation()
      {
        HttpClient testHttpClient = new MockHttpBuilder()
          .WithHandler(messageCaseBuilder => messageCaseBuilder.AcceptAll()
            .RespondWith((responseBuilder, _) => responseBuilder.WithStatusCode(HttpStatusCode.OK))
          )
          .BuildHttpClient();

        CancellationTokenSource testTokenSource = new();
        testTokenSource.Cancel();

        Task<string> testTask = Task.Run(() => Task.FromException<string>(new Exception()), testTokenSource.Token)
          .Then(testHttpClient.GetStringAsync, () => string.Empty, CancellationToken.None)
          .Then(_ => Guid.NewGuid().ToString());

        await Task.Delay(5);

        Assert.True(testTask.IsFaulted);
      }
    }

    public class ForCancellationTokenTaskTNextOnFaulted
    {
      [Fact]
      public async void ItShouldTransitionAFulfilledTask()
      {
        int expectedValue = 5;
        int actualValue = await Task.FromResult("12345")
          .Then(TestFunc, () => Task.FromResult(100), CancellationToken.None);

        Assert.Equal(expectedValue, actualValue);
      }

      [Fact]
      public async void ItShouldTransitionAFaultToASuccessfulValue()
      {
        int expectedValue = 10;
        int actualValue = await Task.FromException<string>(new ArgumentException())
          .Then(TestFunc, () => Task.FromResult(expectedValue), CancellationToken.None);

        Assert.Equal(expectedValue, actualValue);
      }

      [Fact]
      public async void ItShouldCaptureTaskCancellation()
      {
        HttpClient testHttpClient = new MockHttpBuilder()
          .WithHandler(messageCaseBuilder => messageCaseBuilder.AcceptAll()
            .RespondWith((responseBuilder, _) => responseBuilder.WithStatusCode(HttpStatusCode.OK))
          )
          .BuildHttpClient();

        CancellationTokenSource testTokenSource = new();
        testTokenSource.Cancel();

        Task<string> testTask = Task.Run(() => Task.FromException<string>(new Exception()), testTokenSource.Token)
          .Then(testHttpClient.GetStringAsync, () => Task.FromResult(string.Empty), CancellationToken.None)
          .Then(_ => Guid.NewGuid().ToString());

        await Task.Delay(5);

        Assert.True(testTask.IsFaulted);
      }
    }

    public class ForCancellationTokenExceptionToExceptionOnFaulted
    {
      [Fact]
      public async void ItShouldTransitionAFulfilledTask()
      {
        int expectedValue = 5;
        int actualValue = await Task.FromResult("12345")
          .Then(TestFunc, ex => new Exception("nested exception", ex), CancellationToken.None);

        Assert.Equal(expectedValue, actualValue);
      }

      [Fact]
      public async void ItShouldTransitionAFaultToASuccessfulValue()
      {

        Task<int> testTask = Task.FromException<string>(new ArgumentException())
          .Then(TestFunc, ex => new InvalidOperationException("nested exception", ex), CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(async () => await testTask);
      }

      [Fact]
      public async void ItShouldCaptureTaskCancellation()
      {
        HttpClient testHttpClient = new MockHttpBuilder()
          .WithHandler(messageCaseBuilder => messageCaseBuilder.AcceptAll()
            .RespondWith((responseBuilder, _) => responseBuilder.WithStatusCode(HttpStatusCode.OK))
          )
          .BuildHttpClient();

        CancellationTokenSource testTokenSource = new();
        testTokenSource.Cancel();

        Task<string> testTask = Task.Run(() => Task.FromException<string>(new Exception()), testTokenSource.Token)
          .Then(testHttpClient.GetStringAsync, ex => new InvalidOperationException(), CancellationToken.None)
          .Then(_ => Guid.NewGuid().ToString());

        await Task.Delay(5);

        Assert.True(testTask.IsFaulted);
      }
    }

    public class ForCancellationTokenExceptionToTNextOnFaulted
    {
      [Fact]
      public async void ItShouldTransitionAFulfilledTask()
      {
        int expectedValue = 5;
        int actualValue = await Task.FromResult("12345")
          .Then(TestFunc, ex => 100, CancellationToken.None);

        Assert.Equal(expectedValue, actualValue);
      }

      [Fact]
      public async void ItShouldTransitionAFaultToASuccessfulValue()
      {
        int expectedValue = 10;
        int actualValue = await Task.FromException<string>(new ArgumentException())
          .Then(TestFunc, ex => expectedValue, CancellationToken.None);

        Assert.Equal(expectedValue, actualValue);
      }

      [Fact]
      public async void ItShouldCaptureTaskCancellation()
      {
        HttpClient testHttpClient = new MockHttpBuilder()
          .WithHandler(messageCaseBuilder => messageCaseBuilder.AcceptAll()
            .RespondWith((responseBuilder, _) => responseBuilder.WithStatusCode(HttpStatusCode.OK))
          )
          .BuildHttpClient();

        CancellationTokenSource testTokenSource = new();
        testTokenSource.Cancel();

        Task<string> testTask = Task.Run(() => Task.FromException<string>(new Exception()), testTokenSource.Token)
          .Then(testHttpClient.GetStringAsync, ex => string.Empty, CancellationToken.None)
          .Then(_ => Guid.NewGuid().ToString());

        await Task.Delay(5);

        Assert.True(testTask.IsFaulted);
      }
    }

    public class ForCancellationTokenExceptionToTaskTNextOnFaulted
    {
      [Fact]
      public async void ItShouldTransitionAFulfilledTask()
      {
        int expectedValue = 5;
        int actualValue = await Task.FromResult("12345")
          .Then(TestFunc, ex => Task.FromResult(100), CancellationToken.None);

        Assert.Equal(expectedValue, actualValue);
      }

      [Fact]
      public async void ItShouldTransitionAFaultToASuccessfulValue()
      {
        int expectedValue = 10;
        int actualValue = await Task.FromException<string>(new ArgumentException())
          .Then(TestFunc, ex => Task.FromResult(expectedValue), CancellationToken.None);

        Assert.Equal(expectedValue, actualValue);
      }

      [Fact]
      public async void ItShouldCaptureTaskCancellation()
      {
        HttpClient testHttpClient = new MockHttpBuilder()
          .WithHandler(messageCaseBuilder => messageCaseBuilder.AcceptAll()
            .RespondWith((responseBuilder, _) => responseBuilder.WithStatusCode(HttpStatusCode.OK))
          )
          .BuildHttpClient();

        CancellationTokenSource testTokenSource = new();
        testTokenSource.Cancel();

        Task<string> testTask = Task.Run(() => Task.FromException<string>(new Exception()), testTokenSource.Token)
          .Then(testHttpClient.GetStringAsync, ex => Task.FromResult(string.Empty), CancellationToken.None)
          .Then(_ => Guid.NewGuid().ToString());

        await Task.Delay(5);

        Assert.True(testTask.IsFaulted);
      }
    }
  }
}
