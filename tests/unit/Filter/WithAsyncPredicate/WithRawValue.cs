using System;
using System.Threading.Tasks;
using RLC.TaskChaining;
using Xunit;

namespace RLC.TaskChainingTests.Filter.WithAsyncPredicate;

public class WithRawValue
{
  [Fact]
  public async Task ItShouldNotFaultForASuccessfulPredicate()
  {
    Task<int> testTask = Task.FromResult(2)
      .Filter(
        AsyncPredicate,
        new Exception("not even")
      );

    await Task.Delay(10);

    Assert.True(testTask.IsCompletedSuccessfully);
  }

  [Fact]
  public async Task ItShouldNotChangeTheValueForASuccessfulPredicate()
  {
    int expectedValue = 2;
    int actualValue = await Task.FromResult(2)
      .Filter(
        AsyncPredicate,
        new Exception("not even")
      );

    Assert.Equal(expectedValue, actualValue);
  }

  [Fact]
  public async Task ItShouldTransitionForAFailedPredicate()
  {
    string expectedMessage = Guid.NewGuid().ToString();
    Task<int> testTask = Task.FromResult(1)
      .Filter(
        AsyncPredicate,
        new ArgumentException(expectedMessage)
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

  [Fact]
  public async Task ItShouldNotDoubleWrapAnExistingException()
  {
    Task<int> testTask = Task.FromException<int>(new ArithmeticException())
      .Filter(
        AsyncPredicate,
        new ArgumentException()
      );

    await Assert.ThrowsAsync<ArithmeticException>(() => testTask);
  }

  private async Task<bool> AsyncPredicate(int value)
  {
    await Task.Delay(1);

    return value % 2 == 0;
  }
}