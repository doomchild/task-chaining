using System;
using System.Threading;
using System.Threading.Tasks;

namespace RLC.TaskChaining;

using static TaskStatics;

public static partial class TaskExtensions
{
  /// <summary>
  /// Transforms the value in a fulfilled <see name="Task{T}"/> to another type.
  /// </summary>
  /// <remarks>This method is an alias to <code>Task.ResultMap</code>.</remarks>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <typeparam name="TNext">The transformed type.</typeparam>
  /// <param name="onFulfilled">The transformation function.</param>
  /// <returns>The transformed task.</returns>
  // public static Task<TNext> Then<T, TNext>(this Task<T> task, Func<T, TNext> onFulfilled)
  //   => task.ResultMap(onFulfilled);
  public static Task<TNext> Then<T, TNext>(this Task<T> task, Func<T, TNext> onFulfilled)
    => task.ContinueWith(continuationTask =>
    {
      if (continuationTask.IsCanceled)
      {
        return Task.FromException<TNext>(HandleCancellation(task));
      }

      return continuationTask.IsFaulted
        ? Task.FromException<TNext>(PotentiallyUnwindException(continuationTask.Exception))
        : Task.FromResult(onFulfilled(continuationTask.Result));
    }).Unwrap();

  /// <summary>
  /// Transforms the value in a fulfilled <see name="Task{T}"/> to another type.
  /// </summary>
  /// <remarks>This method is an alias to <code>Task.Bind</code>.</remarks>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <typeparam name="TNext">The transformed type.</typeparam>
  /// <param name="onFulfilled">The transformation function.</param>
  /// <returns>The transformed task.</returns>
  public static Task<TNext> Then<T, TNext>(this Task<T> task, Func<T, Task<TNext>> onFulfilled)
  //=> task.Bind(onFulfilled);
    => task.ContinueWith(continuationTask =>
    {
      if (continuationTask.IsCanceled)
      {
        return Task.FromException<TNext>(HandleCancellation(task));
      }

      return continuationTask.IsFaulted
        ? Task.FromException<TNext>(PotentiallyUnwindException(continuationTask.Exception))
        : onFulfilled(continuationTask.Result);
    }).Unwrap();

  /// <summary>
  /// Transforms both sides of a <see name="Task{T}"/>.
  /// </summary>
  /// <remarks>This method is an alias to <code>Task.BiBind</code>.</remarks>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <typeparam name="TNext">The transformed type.</typeparam>
  /// <param name="onFulfilled">The transformation function for a fulfilled task.</param>
  /// <param name="onFaulted">The transformation function for a faulted task.</param>
  /// <returns>The transformed task.</returns>
  public static Task<TNext> Then<T, TNext>(
    this Task<T> task,
    Func<T, TNext> onFulfilled,
    Func<Exception, TNext> onFaulted
  ) => task.BiBind(
    Pipe2(onFaulted, Task.FromResult),
    Pipe2(onFulfilled, Task.FromResult)
  );

  /// <summary>
  /// Transforms both sides of a <see name="Task{T}"/>.
  /// </summary>
  /// <remarks>This method is an alias to <code>Task.BiBind</code>.</remarks>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <typeparam name="TNext">The transformed type.</typeparam>
  /// <param name="onFulfilled">The transformation function for a fulfilled task.</param>
  /// <param name="onFaulted">The transformation function for a faulted task.</param>
  /// <returns>The transformed task.</returns>
  public static Task<TNext> Then<T, TNext>(
    this Task<T> task,
    Func<T, TNext> onFulfilled,
    Func<Exception, Task<TNext>> onFaulted
  ) => task.BiBind(onFaulted, Pipe2(onFulfilled, Task.FromResult));

  /// <summary>
  /// Transforms both sides of a <see name="Task{T}"/>.
  /// </summary>
  /// <remarks>This method is an alias to <code>Task.BiBind</code>.</remarks>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <typeparam name="TNext">The transformed type.</typeparam>
  /// <param name="onFulfilled">The transformation function for a fulfilled task.</param>
  /// <param name="onFaulted">The transformation function for a faulted task.</param>
  /// <returns>The transformed task.</returns>
  public static Task<TNext> Then<T, TNext>(
    this Task<T> task,
    Func<T, Task<TNext>> onFulfilled,
    Func<Exception, TNext> onFaulted
  ) => task.BiBind(Pipe2(onFaulted, Task.FromResult), onFulfilled);

  /// <summary>
  /// Transforms both sides of a <see name="Task{T}"/>.
  /// </summary>
  /// <remarks>This method is an alias to <code>Task.BiBind</code>.</remarks>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <typeparam name="TNext">The transformed type.</typeparam>
  /// <param name="onFulfilled">The transformation function for a fulfilled task.</param>
  /// <param name="onFaulted">The transformation function for a faulted task.</param>
  /// <returns>The transformed task.</returns>
  public static Task<TNext> Then<T, TNext>(
    this Task<T> task,
    Func<T, Task<TNext>> onFulfilled,
    Func<Exception, Task<TNext>> onFaulted
  ) => task.BiBind(onFaulted, onFulfilled);

  /// <summary>
  /// Transforms the value in a fulfilled <see name="Task{T}"/> to another type.
  /// </summary>
  /// <typeparam name="T">The task's underlying type.</typeparams>
  /// <typeparam name="TNext">The transformed type.</typeparam>
  /// <param name="onFulfilled">The transformation function.</param>
  /// <param name="cancellationToken">A cancellation token</param>
  /// <returns>The transformed task</returns>
  public static Task<TNext> Then<T, TNext>(
    this Task<T> task,
    Func<T, CancellationToken, Task<TNext>> onFulfilled,
    CancellationToken cancellationToken = default
  )
  {
    return task.ContinueWith(continuationTask =>
    {
      if (continuationTask.IsCanceled)
      {
        return Task.FromException<TNext>(HandleCancellation(continuationTask));
      }

      return continuationTask.IsFaulted
        ? Task.FromException<TNext>(PotentiallyUnwindException(continuationTask.Exception))
        : onFulfilled(continuationTask.Result, cancellationToken);
    }).Unwrap();
  }

  /// <summary>
  /// Transforms both sides of a <see name="Task{T}"/>.
  /// </summary>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <typeparam name="TNext">The transformed type.</typeparam>
  /// <param name="onFulfilled">The transformation function for a fulfilled task.</param>
  /// <param name="onFaulted">The transformation function for a faulted task.</param>
  /// <param name="cancellationToken">A cancellation token.</param>
  /// <returns>The transformed task.</returns>
  public static Task<TNext> Then<T, TNext>(
    this Task<T> task,
    Func<T, CancellationToken, Task<TNext>> onFulfilled,
    Func<Exception> onFaulted,
    CancellationToken cancellationToken = default
  )
  {
    return task.ContinueWith(continuationTask =>
    {
      if (continuationTask.IsCanceled)
      {
        return Task.FromException<TNext>(HandleCancellation(task));
      }

      return continuationTask.IsFaulted
        ? Task.FromException<TNext>(onFaulted())
        : onFulfilled(continuationTask.Result, cancellationToken);
    }).Unwrap();
  }

  /// <summary>
  /// Transforms both sides of a <see name="Task{T}"/>.
  /// </summary>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <typeparam name="TNext">The transformed type.</typeparam>
  /// <param name="onFulfilled">The transformation function for a fulfilled task.</param>
  /// <param name="onFaulted">The transformation function for a faulted task.</param>
  /// <param name="cancellationToken">A cancellation token.</param>
  /// <returns>The transformed task.</returns>
  public static Task<TNext> Then<T, TNext>(
    this Task<T> task,
    Func<T, CancellationToken, Task<TNext>> onFulfilled,
    Func<TNext> onFaulted,
    CancellationToken cancellationToken = default
  )
  {
    return task.ContinueWith(continuationTask =>
    {
      if (continuationTask.IsCanceled)
      {
        return Task.FromException<TNext>(HandleCancellation(task));
      }

      return continuationTask.IsFaulted
        ? Task.FromResult(onFaulted())
        : onFulfilled(continuationTask.Result, cancellationToken);
    }).Unwrap();
  }

  /// <summary>
  /// Transforms both sides of a <see name="Task{T}"/>.
  /// </summary>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <typeparam name="TNext">The transformed type.</typeparam>
  /// <param name="onFulfilled">The transformation function for a fulfilled task.</param>
  /// <param name="onFaulted">The transformation function for a faulted task.</param>
  /// <param name="cancellationToken">A cancellation token.</param>
  /// <returns>The transformed task.</returns>
  public static Task<TNext> Then<T, TNext>(
    this Task<T> task,
    Func<T, CancellationToken, Task<TNext>> onFulfilled,
    Func<Task<TNext>> onFaulted,
    CancellationToken cancellationToken = default
  )
  {
    return task.ContinueWith(continuationTask =>
    {
      if (continuationTask.IsCanceled)
      {
        return Task.FromException<TNext>(HandleCancellation(task));
      }

      return continuationTask.IsFaulted
        ? onFaulted()
        : onFulfilled(continuationTask.Result, cancellationToken);
    }).Unwrap();
  }

  /// <summary>
  /// Transforms both sides of a <see name="Task{T}"/>.
  /// </summary>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <typeparam name="TNext">The transformed type.</typeparam>
  /// <param name="onFulfilled">The transformation function for a fulfilled task.</param>
  /// <param name="onFaulted">The transformation function for a faulted task.</param>
  /// <param name="cancellationToken">A cancellation token.</param>
  /// <returns>The transformed task.</returns>
  public static Task<TNext> Then<T, TNext>(
    this Task<T> task,
    Func<T, CancellationToken, Task<TNext>> onFulfilled,
    Func<Exception, Exception> onFaulted,
    CancellationToken cancellationToken = default
  )
  {
    return task.ContinueWith(continuationTask =>
    {
      if (continuationTask.IsCanceled)
      {
        return Task.FromException<TNext>(HandleCancellation(task));
      }

      return continuationTask.IsFaulted
        ? Task.FromException<TNext>(onFaulted(PotentiallyUnwindException(continuationTask.Exception)))
        : onFulfilled(continuationTask.Result, cancellationToken);
    }).Unwrap();
  }

  /// <summary>
  /// Transforms both sides of a <see name="Task{T}"/>.
  /// </summary>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <typeparam name="TNext">The transformed type.</typeparam>
  /// <param name="onFulfilled">The transformation function for a fulfilled task.</param>
  /// <param name="onFaulted">The transformation function for a faulted task.</param>
  /// <param name="cancellationToken">A cancellation token.</param>
  /// <returns>The transformed task.</returns>
  public static Task<TNext> Then<T, TNext>(
    this Task<T> task,
    Func<T, CancellationToken, Task<TNext>> onFulfilled,
    Func<Exception, TNext> onFaulted,
    CancellationToken cancellationToken = default
  )
  {
    return task.ContinueWith(continuationTask =>
    {
      if (continuationTask.IsCanceled)
      {
        return Task.FromException<TNext>(HandleCancellation(task));
      }

      return continuationTask.IsFaulted
        ? Task.FromResult(onFaulted(PotentiallyUnwindException(continuationTask.Exception)))
        : onFulfilled(continuationTask.Result, cancellationToken);
    }).Unwrap();
  }

  /// <summary>
  /// Transforms both sides of a <see name="Task{T}"/>.
  /// </summary>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <typeparam name="TNext">The transformed type.</typeparam>
  /// <param name="onFulfilled">The transformation function for a fulfilled task.</param>
  /// <param name="onFaulted">The transformation function for a faulted task.</param>
  /// <param name="cancellationToken">A cancellation token.</param>
  /// <returns>The transformed task.</returns>
  public static Task<TNext> Then<T, TNext>(
    this Task<T> task,
    Func<T, CancellationToken, Task<TNext>> onFulfilled,
    Func<Exception, Task<TNext>> onFaulted,
    CancellationToken cancellationToken = default
  )
  {
    return task.ContinueWith(continuationTask =>
    {
      if (continuationTask.IsCanceled)
      {
        return Task.FromException<TNext>(HandleCancellation(continuationTask));
      }

      return continuationTask.IsFaulted
        ? onFaulted(PotentiallyUnwindException(continuationTask.Exception))
        : onFulfilled(continuationTask.Result, cancellationToken);
    }).Unwrap();
  }
}
