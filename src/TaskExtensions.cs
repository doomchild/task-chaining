using System;
using System.Threading.Tasks;

namespace RLC.TaskChaining;

using static TaskStatics;

public static class TaskExtensions
{
  public static Task<T> Catch<T>(this Task<T> task, Func<Exception, T> onRejected) => task.Then(
    Task.FromResult,
    Pipe2(onRejected, Task.FromResult)
  );

  public static Task<T> Catch<T>(this Task<T> task, Func<Exception, Task<T>> onRejected) => task.Then(
    Task.FromResult,
    onRejected
  );

  public static Task<TNext> Fold<T, TNext>(this Task<T> task, Func<Exception, TNext> leftMap, Func<T, TNext> rightMap)
  {
    return task.ContinueWith(async continuationTask => continuationTask.IsFaulted
      ? continuationTask.Exception?.InnerException != null
        ? leftMap(continuationTask.Exception.InnerException)
        : leftMap(continuationTask.Exception!)
      : rightMap(await continuationTask)
    ).Unwrap();
  }

  public static Task<T> IfResolved<T>(this Task<T> task, Action<T> onFulfilled) => task.Then<T, Task<T>>(
    Pipe2(TaskStatics.Tap(onFulfilled), Task.FromResult),
    Task.FromException<T>
  ).Unwrap();

  public static Task<T> IfRejected<T>(this Task<T> task, Action<Exception> onRejected) => task.Then<T, Task<T>>(
    Task.FromResult,
    Pipe2(TaskStatics.Tap(onRejected), Task.FromException<T>)
  ).Unwrap();

  public static Task<T> Tap<T>(
    this Task<T> task,
    Action<T> onFulfilled,
    Action<Exception> onRejected
  ) => task.IfResolved(onFulfilled).IfRejected(onRejected);

  public static Task<TNext> Then<T, TNext>(this Task<T> task, Func<T, TNext> onFulfilled) => task.Then(
    Pipe2(onFulfilled, Task.FromResult),
    Task.FromException<TNext>
  );

  public static Task<TNext> Then<T, TNext>(this Task<T> task, Func<T, Task<TNext>> onFulfilled) => task.Then(
    Pipe2(onFulfilled, Task.FromResult),
    Task.FromException<TNext>
  ).Unwrap();

  public static Task<TNext> Then<T, TNext>(this Task<T> task, Func<T, TNext> onFulfilled, Func<Exception, TNext> onRejected) => task.Then(
    Pipe2(onFulfilled, Task.FromResult),
    Pipe2(onRejected, Task.FromResult)
  );

  public static Task<TNext> Then<T, TNext>(this Task<T> task, Func<T, Task<TNext>> onFulfilled, Func<Exception, TNext> onRejected) => task.Then(
    onFulfilled,
    Pipe2(onRejected, Task.FromResult)
  );

  public static Task<TNext> Then<T, TNext>(this Task<T> task, Func<T, Task<TNext>> onFulfilled, Func<Exception, Task<TNext>> onRejected)
  {
    return task.Fold(
      async exception => await onRejected(exception),
      async value => await onFulfilled(value)
    ).Unwrap();
  }
}
