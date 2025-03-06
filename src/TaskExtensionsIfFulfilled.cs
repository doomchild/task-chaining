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
  /// <param name="onFulfilled">The action to perform if the task is fulfilled.</param>
  /// <returns>The task.</returns>
  public static Task<T> IfFulfilled<T>(this Task<T> task, Action<T> onFulfilled)
    => task.ResultMap(TaskStatics.Tap(onFulfilled));

  /// <summary>
  /// Executes a function and throws away the result if the <see name="Task{T}"/> is in a fulfilled state.
  /// </summary>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <typeparam name="R">The type of the discarded result of <paramref name="onFulfilled"/>.</typeparam>
  /// <param name="task">The task.</param>
  /// <param name="onFulfilled">The function to execute if the task is fulfilled.</param>
  /// <returns>The task.</returns>
  public static Task<T> IfFulfilled<T, R>(this Task<T> task, Func<T, Task<R>> onFulfilled)
    => task.ContinueWith(continuationTask =>
    {
      if (continuationTask.IsFaulted || continuationTask.IsCanceled)
      {
        return continuationTask;
      }

      return continuationTask.Then(value => onFulfilled(value).Then(_ => value));
    }).Unwrap();

  /// <summary>
  /// Executes a function and throws away the result if the <see name="Task{T}"/> is in a fulfilled state.
  /// </summary>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <param name="task">The task.</param>
  /// <param name="onFulfilled">The function to execute if the task is fulfilled.</param>
  /// <returns>The task.</returns>
  public static Task<T> IfFulfilled<T>(this Task<T> task, Func<T, Task> onFulfilled)
    => task.IfFulfilled(value => onFulfilled(value).ContinueWith(_ => value));
}
