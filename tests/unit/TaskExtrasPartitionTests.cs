using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using FluentAssertions;
using RLC.TaskChaining;
using Xunit;

namespace RLC.TaskChainingTests;

public class PartitionTests
{
  [Fact]
  public async Task ItShouldPartition()
  {
    Func<Task<string>> throwFunc = () => throw new NullReferenceException();
    List<Task<string>> tasks = new()
    {
      Task.FromResult("abc"),
      Task.FromResult("def"),
      Task.FromException<string>(new InvalidOperationException()),
      Task.FromResult("ghi"),
      TaskExtras.Defer(() => Task.FromResult("jkl"), TimeSpan.FromSeconds(1)),
      TaskExtras.Defer(throwFunc, TimeSpan.FromSeconds(1))
    };
    List<string> expectedFulfillments = new()
    {
      "abc",
      "def",
      "ghi",
      "jkl"
    };
    List<Exception> expectedFaults = new()
    {
      new InvalidOperationException(),
      new NullReferenceException()
    };

    (IEnumerable<Exception> Faulted, IEnumerable<string> Fulfilled) partition = await TaskExtras.Partition(tasks);

    partition.Faulted.Should().BeEquivalentTo(
      expectedFaults,
      options => options.Excluding(ex => ex.TargetSite)
        .Excluding(ex => ex.Source)
        .Excluding(ex => ex.StackTrace)
        .Excluding(ex => ex.HResult)
    );
    partition.Fulfilled.Should().BeEquivalentTo(expectedFulfillments);
  }
}
