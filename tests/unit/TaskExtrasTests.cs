using System;
using System.Threading.Tasks;

using RLC.TaskChaining;
using Xunit;

namespace RLC.TaskChainingTests;

public class TaskExtrasTests
{
	public class RejectIf
  {
    [Fact]
    public async void ItShouldRejectForASuccessfulPredicate()
    {
      Task<int> testTask = TaskExtras.RejectIf(
        (int value) => value % 2 != 0,
        value => new ArgumentException()
      )(1);

      await Task.Delay(100);

      Assert.True(testTask.IsFaulted);
    }

    [Fact]
    public async void ItShouldThrowTheExpectedExceptionForASuccessfulPredicate()
    {
      Task<int> testTask = TaskExtras.RejectIf(
        (int value) => value % 2 != 0,
        value => new ArgumentException()
      )(1);

      await Task.Delay(100);

      await Assert.ThrowsAsync<ArgumentException>(async () => await testTask);
    }

    [Fact]
    public async void ItShouldResolveForAFailedPredicate()
    {
      Task<int> testTask = TaskExtras.RejectIf(
        (int value) => value % 2 != 0,
        value => new ArgumentException()
      )(2);

      await Task.Delay(100);

      Assert.True(testTask.IsCompletedSuccessfully);
    }
  }

  public class ResolveIf
  {
    public class WithRawResolutionSupplier
    {
      [Fact]
      public async void ItShouldResolveForASuccessfulPredicate()
      {
        Task<int> testTask = TaskExtras.ResolveIf(
          (Exception value) => value is ArgumentException,
          value => value.Message.Length
        )(new ArgumentException());

        await Task.Delay(100);

        Assert.True(testTask.IsCompletedSuccessfully);
      }

      [Fact]
      public async void ItShouldRejectForAFailedPredicate()
      {
        Task<int> testTask = TaskExtras.ResolveIf(
          (Exception value) => value is ArgumentException,
          value => value.Message.Length
        )(new NullReferenceException());

        await Task.Delay(100);

        Assert.True(testTask.IsFaulted);
      }
    }

    public class WithTaskResolutionSupplier
    {
      [Fact]
      public async void ItShouldResolveForASuccessfulPredicate()
      {
        Task<int> testTask = TaskExtras.ResolveIf(
          (Exception value) => value is ArgumentException,
          value => Task.FromResult(value.Message.Length)
        )(new ArgumentException());

        await Task.Delay(100);

        Assert.True(testTask.IsCompletedSuccessfully);
      }

      [Fact]
      public async void ItShouldRejectForAFailedPredicate()
      {
        Task<int> testTask = TaskExtras.ResolveIf(
          (Exception value) => value is ArgumentException,
          value => Task.FromResult(value.Message.Length)
        )(new NullReferenceException());

        await Task.Delay(100);

        Assert.True(testTask.IsFaulted);
      }
    }
  }
}