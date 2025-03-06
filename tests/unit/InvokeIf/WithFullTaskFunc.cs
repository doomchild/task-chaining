using System;
using System.Threading.Tasks;
using RLC.TaskChaining;
using Xunit;

namespace RLC.TaskChainingTests.InvokeIf;

public class WithFullTaskFunc
{
  [Fact]
  public async Task ItShouldInvokeIfPredicateSucceeds()
  {
    int expectedValue = 5;
    Predicate<int> predicate = value => value % 2 == 0;
    Func<int, Task<int>> func = _ => Task.FromResult(5);

    int actualValue = await Task.FromResult(4)
      .Then(TaskExtras.InvokeIf(predicate, func));
    
    Assert.Equal(expectedValue, actualValue);
  }
  
  [Fact]
  public async Task ItShouldNotInvokeIfPredicateFails()
  {
    int expectedValue = 3;
    Predicate<int> predicate = value => value % 2 == 0;
    Func<int, Task<int>> func = _ => Task.FromResult(5);

    int actualValue = await Task.FromResult(3)
      .Then(TaskExtras.InvokeIf(predicate, func));
    
    Assert.Equal(expectedValue, actualValue);
  }
}