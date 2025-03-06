using System;
using System.Threading.Tasks;

namespace RLC.TaskChaining;

public static partial class TaskExtensions
{
  /// <summary>
  /// Performs an action if the <see name="Task{T}"/> is in a fulfilled state.
  /// </summary>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <param name="task">The task.</param>
  /// <param name="consumer">The action to perform if the task is fulfilled.</param>
  /// <returns>The task.</returns>
  public static Task<T> IfFulfilled<T>(this Task<T> task, Action<T> consumer)
    => task.ResultMap(TaskStatics.Tap(consumer));

  /// <summary>
  /// Executes a function and throws away the result if the <see name="Task{T}"/> is in a fulfilled state.
  /// </summary>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <typeparam name="R">The type of the discarded result of <paramref name="func"/>.</typeparam>
  /// <param name="task">The task.</param>
  /// <param name="func">The function to execute if the task is fulfilled.</param>
  /// <returns>The task.</returns>
  public static Task<T> IfFulfilled<T, R>(this Task<T> task, Func<T, Task<R>> func)
    => task.ContinueWith(continuationTask =>
    {
      if (continuationTask.IsFaulted || continuationTask.IsCanceled)
      {
        return continuationTask;
      }

      return continuationTask.Then(value =>
      {
        func(value);
        return value;
      });
    }).Unwrap();

  /// <summary>
  /// Executes a function and throws away the result if the <see name="Task{T}"/> is in a fulfilled state.
  /// </summary>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <param name="task">The task.</param>
  /// <param name="func">The function to execute if the task is fulfilled.</param>
  /// <returns>The task.</returns>
  public static Task<T> IfFulfilled<T>(this Task<T> task, Func<T, Task> func)
    => task.ContinueWith(continuationTask =>
    {
      if (continuationTask.IsFaulted || continuationTask.IsCanceled)
      {
        return continuationTask;
      }

      return continuationTask.Then(async value =>
      {
        await func(value);
        return value;
      });
    }).Unwrap();
}
