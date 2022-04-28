using System;
using System.Threading.Tasks;

namespace RLC.TaskChaining;

using static TaskStatics;

public static class TaskExtensions
{
  #region Monadic Functions

  public static Task<T> Alt<T>(this Task<T> task, Task<T> other) => task.IsFaulted ? other : task;

  public static Task<T> Alt<T>(this Task<T> task, Func<Task<T>> supplier) => task.IsFaulted ? supplier() : task;

  public static Task<TNext> BiMap<T, TNext>(
    this Task<T> task,
    Func<Exception, Exception> onRejected,
    Func<T, TNext> onFulfilled
  ) => task.BiBind(
    Pipe2(onRejected, Task.FromException<TNext>),
    Pipe2(onFulfilled, Task.FromResult)
  );

  public static Task<TNext> Bind<T, TNext>(this Task<T> task, Func<T, Task<TNext>> onFulfilled) => task.BiBind(
    Task.FromException<TNext>,
    onFulfilled
  );

  public static Task<TNext> BiBind<T, TNext>(
    this Task<T> task,
    Func<Exception, Task<TNext>> faulted,
    Func<T, Task<TNext>> fulfilled
  ) => task.ContinueWith(async continuationTask => continuationTask.IsFaulted
    ? continuationTask.Exception?.InnerException != null
      ? faulted(continuationTask.Exception.InnerException)
      : faulted(continuationTask.Exception!)
    : fulfilled(await continuationTask)
  ).Unwrap().Unwrap();

  public static Task<T> ExceptionMap<T>(this Task<T> task, Func<Exception, Exception> onRejected)
    => task.BiMap(onRejected, Identity);

  public static Task<T> Filter<T>(this Task<T> task, Predicate<T> predicate, Exception exception)
    => task.Filter(predicate, Constant(exception));

  public static Task<T> Filter<T>(this Task<T> task, Predicate<T> predicate, Func<Exception> supplier)
    => task.Bind(value => predicate(value) ? Task.FromResult(value) : Task.FromException<T>(supplier()));

  public static Task<T> Filter<T>(this Task<T> task, Predicate<T> predicate, Func<T, Exception> morphism)
    => task.Bind(value => predicate(value) ? Task.FromResult(value) : Task.FromException<T>(morphism(value)));

  public static Task<TNext> ResultMap<T, TNext>(this Task<T> task, Func<T, TNext> morphism)
    => task.BiMap(Identity, morphism);

  public static Task<T> Recover<T>(this Task<T> task, Func<Exception, T> morphism)
    => task.Then(Identity, morphism);

  public static Task<T> Recover<T>(this Task<T> task, Func<Exception, Task<T>> morphism)
    => task.BiBind(morphism, Task.FromResult);

  #endregion

  public static Task<T> Catch<T>(this Task<T> task, Func<Exception, T> onRejected) => task.Recover(onRejected);

  public static Task<T> Catch<T>(this Task<T> task, Func<Exception, Task<T>> onRejected) => task.Recover(onRejected);

  public static Task<T> IfResolved<T>(this Task<T> task, Action<T> consumer)
    => task.ResultMap(TaskStatics.Tap(consumer));

  public static Task<T> IfRejected<T>(this Task<T> task, Action<Exception> onRejected)
    => task.ExceptionMap(TaskStatics.Tap(onRejected));

  public static Task<TNext> Retry<T, TNext>(
    this Task<T> task,
    Func<T, Task<TNext>> retryFunc,
    RetryOptions retryOptions
  ) => task.ResultMap(value => TaskExtras.Retry(() => retryFunc(value), retryOptions)).Unwrap();

  public static Task<TNext> Retry<T, TNext>(this Task<T> task, Func<T, TNext> retryFunc, RetryOptions retryOptions)
    => task.Retry(value => Task.FromResult(retryFunc(value)), retryOptions);

  public static Task<TNext> Retry<T, TNext>(this Task<T> task, Func<T, Task<TNext>> retryFunc)
    => task.Retry(retryFunc, RetryOptions.Default);

  public static Task<TNext> Retry<T, TNext>(this Task<T> task, Func<T, TNext> retryFunc)
    => task.Retry(retryFunc, RetryOptions.Default);

  public static Task<T> Tap<T>(this Task<T> task, Action<T> onFulfilled, Action<Exception> onRejected)
    => task.IfResolved(onFulfilled).IfRejected(onRejected);

  public static Task<TNext> Then<T, TNext>(this Task<T> task, Func<T, TNext> onFulfilled)
    => task.ResultMap(onFulfilled);

  public static Task<TNext> Then<T, TNext>(this Task<T> task, Func<T, Task<TNext>> onFulfilled)
    => task.Bind(onFulfilled);

  public static Task<TNext> Then<T, TNext>(
    this Task<T> task,
    Func<T, TNext> onFulfilled,
    Func<Exception, TNext> onRejected
  ) => task.BiBind(
    Pipe2(onRejected, Task.FromResult),
    Pipe2(onFulfilled, Task.FromResult)
  );

  public static Task<TNext> Then<T, TNext>(
    this Task<T> task,
    Func<T, TNext> onFulfilled,
    Func<Exception, Task<TNext>> onRejected
  ) => task.BiBind(onRejected, Pipe2(onFulfilled, Task.FromResult));

  public static Task<TNext> Then<T, TNext>(
    this Task<T> task,
    Func<T, Task<TNext>> onFulfilled,
    Func<Exception, TNext> onRejected
  ) => task.BiBind(Pipe2(onRejected, Task.FromResult), onFulfilled);

  public static Task<TNext> Then<T, TNext>(
    this Task<T> task,
    Func<T, Task<TNext>> onFulfilled,
    Func<Exception, Task<TNext>> onRejected
  ) => task.BiBind(onRejected, onFulfilled);
}
