using System;
using System.Threading.Tasks;
using RLC.TaskChaining;
using Xunit;

namespace RLC.TaskChainingTests.Filter;

public class WithRawValue
{
  [Fact]
  public async Task ItShouldNotFaultForASuccessfulPredicate()
  {
    Task<int> testTask = Task.FromResult(2).Filter(value => value % 2 == 0, new Exception("not even"));

    await Task.Delay(2);

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