using System;
using System.Threading;
using System.Threading.Tasks;

namespace RLC.TaskChaining;

using static TaskStatics;

public static partial class TaskExtensions
{
  /// <summary>
  /// Executes a function and discards the result on a <see name="Task{T}"/> whether it is in a fulfilled or faulted state.
  /// </summary>
  /// <remarks>This method is useful if you need to perform a side effect without altering the <see name"Task{T}"/>'s
  /// value, such as logging.</remarks>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <param name="task">The task.</param>
  /// <param name="onFulfilled">The function to execute if the task is fulfilled.</param>
  /// <param name="onFaulted">The function to execute if the task is faulted.</param>
  /// <returns>The task.</returns>
  public static Task<T> Tap<T>(this Task<T> task, Action<T> onFulfilled, Action<Exception> onFaulted)
    => task.IfFulfilled(onFulfilled).IfFaulted(onFaulted);

  /// <summary>
  /// Executes a function and discards the result on a <see name="Task{T}"/> whether it is in a fulfilled or faulted state.
  /// </summary>
  /// <remarks>This method is useful if you need to perform a side effect without altering the <see name"Task{T}"/>'s
  /// value, such as logging.</remarks>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <typeparam name="R">The output type of the <paramref name="onFaulted" /> function.</typeparam>
  /// <param name="task">The task.</param>
  /// <param name="onFulfilled">The function to execute if the task is fulfilled.</param>
  /// <param name="onFaulted">The function to execute if the task is faulted.</param>
  /// <returns>The task.</returns>
  public static Task<T> Tap<T, R>(this Task<T> task, Action<T> onFulfilled, Func<Exception, Task<R>> onFaulted)
    => task.IfFulfilled(onFulfilled).IfFaulted(onFaulted);

  /// <summary>
  /// Executes a function and discards the result on a <see name="Task{T}"/> whether it is in a fulfilled or faulted state.
  /// </summary>
  /// <remarks>This method is useful if you need to perform a side effect without altering the <see name"Task{T}"/>'s
  /// value, such as logging.</remarks>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <param name="task">The task.</param>
  /// <param name="onFulfilled">The function to execute if the task is fulfilled.</param>
  /// <param name="onFaulted">The function to execute if the task is faulted.</param>
  /// <returns>The task.</returns>
  public static Task<T> Tap<T>(this Task<T> task, Action<T> onFulfilled, Func<Exception, Task> onFaulted)
    => task.IfFulfilled(onFulfilled).IfFaulted(onFaulted);

  /// <summary>
  /// Executes a function and discards the result on a <see name="Task{T}"/> whether it is in a fulfilled or faulted state.
  /// </summary>
  /// <remarks>This method is useful if you need to perform a side effect without altering the <see name"Task{T}"/>'s
  /// value, such as logging.</remarks>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <typeparam name="R">The output type of the <paramref name="onFaulted" /> function.</typeparam>
  /// <param name="task">The task.</param>
  /// <param name="onFulfilled">The action to perform if the task is fulfilled.</param>
  /// <param name="onFaulted">The function to execute if the task is faulted.</param>
  /// <returns>The task.</returns>
  public static Task<T> Tap<T, R>(this Task<T> task, Func<T, Task<R>> onFulfilled, Action<Exception> onFaulted)
    => task.IfFulfilled(onFulfilled).IfFaulted(onFaulted);

  /// <summary>
  /// Executes a function and discards the result on a <see name="Task{T}"/> whether it is in a fulfilled or faulted state.
  /// </summary>
  /// <remarks>This method is useful if you need to perform a side effect without altering the <see name"Task{T}"/>'s
  /// value, such as logging.</remarks>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <param name="task">The task.</param>
  /// <param name="onFulfilled">The function to execute if the task is fulfilled.</param>
  /// <param name="onFaulted">The function to execute if the task is faulted.</param>
  /// <returns>The task.</returns>
  public static Task<T> Tap<T>(this Task<T> task, Func<T, Task> onFulfilled, Action<Exception> onFaulted)
    => task.IfFulfilled(onFulfilled).IfFaulted(onFaulted);

  /// <summary>
  /// Executes a function and discards the result on a <see name="Task{T}"/> whether it is in a fulfilled or faulted state.
  /// </summary>
  /// <remarks>This method is useful if you need to perform a side effect without altering the <see name"Task{T}"/>'s
  /// value, such as logging.</remarks>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <typeparam name="R">The output type of the <paramref name="onFaulted" /> function.</typeparam>
  /// <param name="task">The task.</param>
  /// <param name="onFulfilled">The function to execute if the task is fulfilled.</param>
  /// <param name="onFaulted">The function to execute if the task is faulted.</param>
  /// <returns>The task.</returns>
  public static Task<T> Tap<T, R, S>(this Task<T> task, Func<T, Task<R>> onFulfilled, Func<Exception, Task<S>> onFaulted)
    => task.IfFulfilled(onFulfilled).IfFaulted(onFaulted);

  /// <summary>
  /// Executes a function and discards the result on a <see name="Task{T}"/> whether it is in a fulfilled or faulted state.
  /// </summary>
  /// <remarks>This method is useful if you need to perform a side effect without altering the <see name"Task{T}"/>'s
  /// value, such as logging.</remarks>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <param name="task">The task.</param>
  /// <param name="onFulfilled">The function to execute if the task is fulfilled.</param>
  /// <param name="onFaulted">The function to execute if the task is faulted.</param>
  /// <returns>The task.</returns>
  public static Task<T> Tap<T>(this Task<T> task, Func<T, Task> onFulfilled, Func<Exception, Task> onFaulted)
    => task.IfFulfilled(onFulfilled).IfFaulted(onFaulted);

  /// <summary>
  /// Executes a function and discards the result on a <see name="Task{T}"/> whether it is in a fulfilled or faulted state.
  /// </summary>
  /// <remarks>This method is useful if you need to perform a side effect without altering the <see name"Task{T}"/>'s
  /// value, such as logging.</remarks>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <param name="task">The task.</param>
  /// <param name="onFulfilled">The function to execute if the task is fulfilled.</param>
  /// <param name="onFaulted">The function to execute if the task is faulted.</param>
  /// <returns>The task.</returns>
  public static Task<T> Tap<T, R>(this Task<T> task, Func<T, Task> onFulfilled, Func<Exception, Task<R>> onFaulted)
    => task.IfFulfilled(onFulfilled).IfFaulted(onFaulted);
}
