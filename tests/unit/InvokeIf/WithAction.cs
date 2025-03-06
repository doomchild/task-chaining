using System;
using System.Threading.Tasks;
using RLC.TaskChaining;
using Xunit;

namespace RLC.TaskChainingTests.InvokeIf;

public class WithAction
{
  [Fact]
  public async Task ItShouldInvokeIfPredicateSucceeds()
  {
    int actualValue = 0;
    int expectedValue = 5;
    Predicate<int> predicate = value => value % 2 == 0;
    Action<int> action = _ => { actualValue = 5; };

    await Task.FromResult(4)
      .Then(TaskExtras.InvokeIf(predicate, action));
    
    Assert.Equal(expectedValue, actualValue);
  }
  
  [Fact]
  public async Task ItShouldNotInvokeIfPredicateFails()
  {
    int actualValue = 0;
    int expectedValue = 5;
    Predicate<int> predicate = value => value % 2 == 0;
    Action<int> action = _ => { actualValue = 5; };

    await Task.FromResult(3)
      .Then(TaskExtras.InvokeIf(predicate, action));
    
    Assert.NotEqual(expectedValue, actualValue);
  }
}