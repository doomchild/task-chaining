using System;
using System.Threading.Tasks;
using RLC.TaskChaining;
using Xunit;

namespace RLC.TaskChainingTests.InvokeIf;

public class WithRawTaskFunc
{
  [Fact]
  public async Task ItShouldInvokeIfPredicateSucceeds()
  {
    int actualValue = 0;
    int expectedValue = 5;
    Predicate<int> predicate = value => value % 2 == 0;
    Func<int, Task> func = value =>
    {
      actualValue = 5;
      return Task.CompletedTask;
    };

    await Task.FromResult(4)
      .Then(TaskExtras.InvokeIf(predicate, func));
    
    Assert.Equal(expectedValue, actualValue);
  }
  
  [Fact]
  public async Task ItShouldNotInvokeIfPredicateFails()
  {
    int actualValue = 0;
    int expectedValue = 5;
    Predicate<int> predicate = value => value % 2 == 0;
    Func<int, Task> func = value =>
    {
      actualValue = 5;
      return Task.CompletedTask;
    };

    await Task.FromResult(3)
      .Then(TaskExtras.InvokeIf(predicate, func));
    
    Assert.NotEqual(expectedValue, actualValue);
  }
}