using System;
using System.Threading.Tasks;

namespace RLC.TaskChaining;

public static partial class TaskExtensions
{
  extension<T>(Task<T> task)
  {
    /// <summary>
    /// Executes a function and discards the result on a <see name="Task{T}"/> whether it is in a fulfilled or faulted state.
    /// </summary>
    /// <remarks>This method is useful if you need to perform a side effect without altering the <see name="Task{T}"/>'s
    /// value, such as logging.</remarks>
    /// <typeparam name="T">The task's underlying type.</typeparam>
    /// <param name="onFulfilled">The function to execute if the task is fulfilled.</param>
    /// <param name="onFaulted">The function to execute if the task is faulted.</param>
    /// <returns>The task.</returns>
    public Task<T> Tap(Action<T> onFulfilled, Action<Exception> onFaulted)
      => task.ContinueWith(continuationTask =>
      {
        if (continuationTask.IsCanceled)
        {
          Exception exception = HandleCancellation(continuationTask);
    
          onFaulted(exception);
    
          return Task.FromException<T>(exception);
        }
      
        return continuationTask.IsFaulted
          ? continuationTask.IfFaulted(onFaulted)
          : continuationTask.IfFulfilled(onFulfilled);
      }).Unwrap();

    /// <summary>
    /// Executes a function and discards the result on a <see name="Task{T}"/> whether it is in a fulfilled or faulted state.
    /// </summary>
    /// <remarks>This method is useful if you need to perform a side effect without altering the <see name="Task{T}"/>'s
    /// value, such as logging.</remarks>
    /// <typeparam name="T">The task's underlying type.</typeparam>
    /// <typeparam name="R">The output type of the <paramref name="onFaulted" /> function.</typeparam>
    /// <param name="onFulfilled">The function to execute if the task is fulfilled.</param>
    /// <param name="onFaulted">The function to execute if the task is faulted.</param>
    /// <returns>The task.</returns>
    public Task<T> Tap<R>(Action<T> onFulfilled, Func<Exception, Task<R>> onFaulted)
      => task.Tap(
        value =>
        {
          onFulfilled(value);
          return Task.FromResult(value);
        },
        onFaulted
      );

    /// <summary>
    /// Executes a function and discards the result on a <see name="Task{T}"/> whether it is in a fulfilled or faulted state.
    /// </summary>
    /// <remarks>This method is useful if you need to perform a side effect without altering the <see name="Task{T}"/>'s
    /// value, such as logging.</remarks>
    /// <typeparam name="T">The task's underlying type.</typeparam>
    /// <param name="onFulfilled">The function to execute if the task is fulfilled.</param>
    /// <param name="onFaulted">The function to execute if the task is faulted.</param>
    /// <returns>The task.</returns>
    public Task<T> Tap(Action<T> onFulfilled, Func<Exception, Task> onFaulted)
      => task.ContinueWith(continuationTask =>
      {
        if (continuationTask.IsCanceled)
        {
          Exception exception = HandleCancellation(continuationTask);

          onFaulted(exception);

          return Task.FromException<T>(exception);
        }
      
        return continuationTask.IsFaulted
          ? continuationTask.IfFaulted(onFaulted)
          : continuationTask.IfFulfilled(onFulfilled);
      }).Unwrap();

    /// <summary>
    /// Executes a function and discards the result on a <see name="Task{T}"/> whether it is in a fulfilled or faulted state.
    /// </summary>
    /// <remarks>This method is useful if you need to perform a side effect without altering the <see name="Task{T}"/>'s
    /// value, such as logging.</remarks>
    /// <typeparam name="T">The task's underlying type.</typeparam>
    /// <typeparam name="R">The output type of the <paramref name="onFaulted" /> function.</typeparam>
    /// <param name="onFulfilled">The action to perform if the task is fulfilled.</param>
    /// <param name="onFaulted">The function to execute if the task is faulted.</param>
    /// <returns>The task.</returns>
    public Task<T> Tap<R>(Func<T, Task<R>> onFulfilled, Action<Exception> onFaulted)
      => task.ContinueWith(continuationTask =>
      {
        if (continuationTask.IsCanceled)
        {
          Exception exception = HandleCancellation(continuationTask);

          onFaulted(exception);

          return Task.FromException<T>(exception);
        }
      
        return continuationTask.IsFaulted
          ? continuationTask.IfFaulted(onFaulted)
          : continuationTask.IfFulfilled(onFulfilled);
      }).Unwrap();

    /// <summary>
    /// Executes a function and discards the result on a <see name="Task{T}"/> whether it is in a fulfilled or faulted state.
    /// </summary>
    /// <remarks>This method is useful if you need to perform a side effect without altering the <see name="Task{T}"/>'s
    /// value, such as logging.</remarks>
    /// <typeparam name="T">The task's underlying type.</typeparam>
    /// <param name="onFulfilled">The function to execute if the task is fulfilled.</param>
    /// <param name="onFaulted">The function to execute if the task is faulted.</param>
    /// <returns>The task.</returns>
    public Task<T> Tap(Func<T, Task> onFulfilled, Action<Exception> onFaulted)
      => task.ContinueWith(continuationTask =>
      {
        if (continuationTask.IsCanceled)
        {
          Exception exception = HandleCancellation(continuationTask);

          onFaulted(exception);

          return Task.FromException<T>(exception);
        }
      
        return continuationTask.IsFaulted
          ? continuationTask.IfFaulted(onFaulted)
          : continuationTask.IfFulfilled(onFulfilled);
      }).Unwrap();

    /// <summary>
    /// Executes a function and discards the result on a <see name="Task{T}"/> whether it is in a fulfilled or faulted state.
    /// </summary>
    /// <remarks>This method is useful if you need to perform a side effect without altering the <see name="Task{T}"/>'s
    /// value, such as logging.</remarks>
    /// <typeparam name="T">The task's underlying type.</typeparam>
    /// <typeparam name="R">The output type of the <paramref name="onFulfilled" /> function.</typeparam>
    /// <typeparam name="S">The output type of the <paramref name="onFaulted" /> function.</typeparam>
    /// <param name="onFulfilled">The function to execute if the task is fulfilled.</param>
    /// <param name="onFaulted">The function to execute if the task is faulted.</param>
    /// <returns>The task.</returns>
    public Task<T> Tap<R, S>(Func<T, Task<R>> onFulfilled, Func<Exception, Task<S>> onFaulted)
      => task.ContinueWith(async continuationTask =>
      {
        if (continuationTask.IsCanceled)
        {
          Exception exception = HandleCancellation(continuationTask);

          await onFaulted(exception);

          return Task.FromException<T>(exception);
        }
      
        return continuationTask.IsFaulted
          ? continuationTask.IfFaulted(onFaulted)
          : continuationTask.IfFulfilled(onFulfilled);
      }).Unwrap().Unwrap();

    /// <summary>
    /// Executes a function and discards the result on a <see name="Task{T}"/> whether it is in a fulfilled or faulted state.
    /// </summary>
    /// <remarks>This method is useful if you need to perform a side effect without altering the <see name="Task{T}"/>'s
    /// value, such as logging.</remarks>
    /// <typeparam name="T">The task's underlying type.</typeparam>
    /// <param name="onFulfilled">The function to execute if the task is fulfilled.</param>
    /// <param name="onFaulted">The function to execute if the task is faulted.</param>
    /// <returns>The task.</returns>
    public Task<T> Tap(Func<T, Task> onFulfilled, Func<Exception, Task> onFaulted)
      => task.ContinueWith(async continuationTask =>
      {
        if (continuationTask.IsCanceled)
        {
          Exception exception = HandleCancellation(continuationTask);

          await onFaulted(exception);

          return Task.FromException<T>(exception);
        }
        
        if (continuationTask is { IsFaulted: false, Result: Task { IsCanceled: true } resultTask })
        {
          try
          {
            await resultTask;
          }
          catch (Exception exception)
          {
            var resultException = PotentiallyUnwindException(exception);
          
            await onFaulted(resultException);
      
            return continuationTask;
          }
        }
      
        return continuationTask.IsFaulted
          ? continuationTask.IfFaulted(onFaulted)
          : continuationTask.IfFulfilled(onFulfilled);
      }).Unwrap().Unwrap();

    /// <summary>
    /// Executes a function and discards the result on a <see name="Task{T}"/> whether it is in a fulfilled or faulted state.
    /// </summary>
    /// <remarks>This method is useful if you need to perform a side effect without altering the <see name="Task{T}"/>'s
    /// value, such as logging.</remarks>
    /// <typeparam name="T">The task's underlying type.</typeparam>
    /// <typeparam name="R">The output type of the <paramref name="onFaulted" /> function.</typeparam>
    /// <param name="onFulfilled">The function to execute if the task is fulfilled.</param>
    /// <param name="onFaulted">The function to execute if the task is faulted.</param>
    /// <returns>The task.</returns>
    public Task<T> Tap<R>(Func<T, Task> onFulfilled, Func<Exception, Task<R>> onFaulted)
      => task.Tap(
        value =>
        {
          onFulfilled(value);
          return Task.FromResult(value);
        },
        onFaulted
      );
  }
}
