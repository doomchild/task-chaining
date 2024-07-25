using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using RLC.TaskChaining;
using Xunit;

namespace RLC.TaskChainingTests;

public class PartitionTests
{
  [Fact]
  public async Task ItShouldPartition()
  {
    int expectedFulfills = 4;
    int expectedFaults = 1;
    List<Task<string>> tasks = new()
    {
      Task.FromResult("abc"),
      Task.FromResult("def"),
      Task.FromException<string>(new InvalidOperationException()),
      Task.FromResult("ghi"),
      TaskExtras.Defer(() => Task.FromResult("jkl"), TimeSpan.FromSeconds(1))
    };

    (IEnumerable<Task<string>> Faulted, IEnumerable<Task<string>> Fulfilled) partition = await TaskExtras.Partition(tasks);

    Assert.Equal(expectedFaults, partition.Faulted.Count());
    Assert.Equal(expectedFulfills, partition.Fulfilled.Count());
  }
}
