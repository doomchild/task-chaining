using System;
using System.Threading.Tasks;

namespace RLC.TaskChaining;

public static partial class TaskExtensions
{
  extension<T>(Task<T> task)
  {
    /// <summary>
    /// Performs an action if the <see name="Task{T}"/> is in a faulted state.
    /// </summary>
    /// <typeparam name="T">The task's underlying type.</typeparam>
    /// <param name="onFaulted">The action to perform if the task is faulted.</param>
    /// <returns>The task.</returns>
    public Task<T> IfFaulted(Action<Exception> onFaulted)
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
    public Task<T> IfFaulted<R>(Func<Exception, Task<R>> onFaulted)
      => task.ContinueWith(async continuationTask =>
      {
        if (continuationTask.IsFaulted)
        {
          Exception taskException = PotentiallyUnwindException(continuationTask.Exception!);

          return Task.FromException<R>(PotentiallyUnwindException(continuationTask.Exception!))
            .Catch(onFaulted)
            .Then(_ => Task.FromException<T>(taskException));
        }
        
        if (continuationTask.IsCanceled)
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
    public Task<T> IfFaulted(Func<Exception, Task> onFaulted)
      => task.IfFaulted<T, T>(exception => onFaulted(exception).ContinueWith(_ => Task.FromException<T>(exception)).Unwrap());
  }
}
