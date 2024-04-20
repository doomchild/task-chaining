using System;
using System.Threading;
using System.Threading.Tasks;

namespace RLC.TaskChaining;

using static TaskStatics;

public static partial class TaskExtensions
{
  /// <summary>
  /// Performs an action if the <see name="Task{T}"/> is in a faulted state.
  /// </summary>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <param name="onFaulted">The action to perform if the task is faulted.</param>
  /// <returns>The task.</returns>
  public static Task<T> IfFaulted<T>(this Task<T> task, Action<Exception> onFaulted)
    => task.IfFaulted<T, T>(exception =>
    {
      onFaulted(exception);

      return Task.FromException<T>(exception);
    });

  /// <summary>
  /// Executes a function and throws away the result if the <see name="Task{T}"/> is in a faulted state.
  /// </summary>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <typeparam name="R">The output task's underlying type.</typeparam>
  /// <param name="onFaulted">The function to execute if the task is faulted.</param>
  /// <returns>The task.</returns>
  public static Task<T> IfFaulted<T, R>(this Task<T> task, Func<Exception, Task<R>> onFaulted)
    => task.ContinueWith(async continuationTask =>
    {
      if (continuationTask.IsFaulted)
      {
        Exception taskException = PotentiallyUnwindException(continuationTask.Exception!);

        return Task.FromException<R>(PotentiallyUnwindException(continuationTask.Exception!))
          .Catch<R>(ex => onFaulted(ex))
          .Then(
            _ => Task.FromException<T>(taskException),
            _ => Task.FromException<T>(taskException)
          );
      }
      else if (continuationTask.IsCanceled)
      {
        try
        {
          await continuationTask;
        }
        catch(OperationCanceledException exception)
        {
          await onFaulted(exception);

          return Task.FromException<T>(exception);
        }
      }

      return continuationTask;
    }).Unwrap().Unwrap();

  /// <summary>
  /// Executes a function and throws away the result if the <see name="Task{T}"/> is in a faulted state.
  /// </summary>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <param name="onFaulted">The function to execute if the task is faulted.</param>
  /// <returns>The task.</returns>
  public static Task<T> IfFaulted<T>(this Task<T> task, Func<Exception, Task> onFaulted)
    => task.IfFaulted<T, T>(exception => onFaulted(exception).ContinueWith(continuationTask => Task.FromException<T>(exception)).Unwrap());
}
