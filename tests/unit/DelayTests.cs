using System;
using System.Diagnostics;
using System.Threading.Tasks;
using RLC.TaskChaining;
using Xunit;

namespace RLC.TaskChainingTests;

public class DelayTests
{
  [Fact]
  public async Task ItShouldWaitTheConfiguredTime()
  {
    Stopwatch testStopWatch = new();
    int testDelayTimeMilliseconds = 15;

    testStopWatch.Start();

    await Task.FromResult(1).Delay(TimeSpan.FromMilliseconds(testDelayTimeMilliseconds));

    testStopWatch.Stop();

    Assert.True(testStopWatch.ElapsedMilliseconds >= testDelayTimeMilliseconds);
  }
  
  [Fact]
  public async Task ItShouldEnsureRetryExceptionsAreNotWrapped()
  {
    Func<int, Task<int>> testFunc = _ => Task.FromException<int>(new RetryException(1, new ArgumentNullException()));
    Exception? actualValue = null;

    try
    {
      await testFunc(1)
        .Delay(TimeSpan.Zero);
    }
    catch (RetryException exception)
    {
      actualValue = exception.InnerException!;
    }

    Assert.IsType<ArgumentNullException>(actualValue);
  }
}