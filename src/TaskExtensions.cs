using System;
using System.Threading;
using System.Threading.Tasks;

namespace RLC.TaskChaining;

using static TaskStatics;

public static class TaskExtensions
{
  #region Monadic Functions

  private static Exception PotentiallyUnwindException(AggregateException exception) => exception.InnerException != null
    ? exception.InnerException
    : exception;

  /// <summary>
  /// Monadic 'alt'.
  /// </summary>
  /// <remarks>This method allows you to swap a faulted <see name="Task{T}"/> with an alternate
  /// <see name="Task{T}"/>.</remarks>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <param name="other">The replacement <see name="Task{T}"/>.</param>
  /// <returns>The alternative value if the original <see name="Task{T}"/> was faulted, otherwise the original
  /// <see name="Task{T}"/>.</returns>
  public static Task<T> Alt<T>(this Task<T> task, Task<T> other) => task.IsFaulted ? other : task;

  /// <summary>
  /// Monadic 'alt'.
  /// </summary>
  /// <remarks>This method allows you to swap a faulted <see name="Task{T}"/> with an alternate <see name="Task{T}"/>
  /// produced by the supplier function.</remarks>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <param name="supplier">A supplier function.</param>
  /// <returns>The alternative value if the original <see name="Task{T}"/> was faulted, otherwise the original
  /// <see name="Task{T}"/>.</returns>
  public static Task<T> Alt<T>(this Task<T> task, Func<Task<T>> supplier) => task.IsFaulted ? supplier() : task;

  /// <summary>
  /// Monadic 'ap'.
  /// </summary>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <typeparams name="TR">The type of the other input value to <paramref name="morphismTask"/>.</typeparam>
  /// <typeparam name="TNext">The transformed type.</typeparam>
  /// <param name="morphismTask">A <see cref="Task{Func{T, TR, TNext}}"/> containing the transformation function. </param>
  /// <returns>The transformed task.</returns>
  public static Task<TNext> Ap<T, TNext>(
    this Task<T> task,
    Task<Func<T, TNext>> morphismTask
  ) => morphismTask.IsFaulted
    ? Task.FromException<TNext>(PotentiallyUnwindException(morphismTask.Exception!))
    : task.ResultMap(async value => (await morphismTask)(value)).Unwrap();

  /// <summary>
  /// Monadic 'bimap'.
  /// </summary>
  /// <remarks>This method allows you to specify a transformation for both sides (fulfilled and faulted) of a
  /// <see name="Task{T}"/>.  If the input <see name="Task{T}"/> is fulfilled, the <code>onFulfilled</code> function
  /// will be executed, otherwise the <code>onFaulted</code> function will be executed.  Note that this method
  /// does NOT allow a faulted <see name="Task{T}"/> to be transitioned back into a fulfilled one.</remarks>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <typeparam name="TNext">The transformed type.</typeparam>
  /// <param name="onFaulted">The function to run if the task is faulted.</param>
  /// <param name="onFulfilled">The function to run if the task is fulfilled.</param>
  /// <returns>The transformed task.</returns>
  public static Task<TNext> BiMap<T, TNext>(
    this Task<T> task,
    Func<Exception, Exception> onFaulted,
    Func<T, TNext> onFulfilled
  ) => task.BiBind(
    Pipe2(onFaulted, Task.FromException<TNext>),
    Pipe2(onFulfilled, Task.FromResult)
  );

  /// <summary>
  /// Monadic 'bind'.
  /// </summary>
  /// <remarks>This is the normal monadic 'bind' in which a function transforms the <see name="Task{T}"/>'s unwrapped
  /// value into a wrapped <see name="Task{T}"/>.</remarks>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <typeparam name="TNext">The transformed type.</typeparam>
  /// <param name="onFulfilled">The function to run if the task is fulfilled.</param>
  /// <returns>The transformed task.</returns>
  public static Task<TNext> Bind<T, TNext>(this Task<T> task, Func<T, Task<TNext>> onFulfilled) => task.BiBind(
    Task.FromException<TNext>,
    onFulfilled
  );

  /// <summary>
  /// Monadic 'bind' on both sides of a <see name="Task{T}"/>.
  /// </summary>
  /// <remarks>This method allows either side of a <see name="Task{T}"/> (faulted or fulfilled) to be bound to a new
  /// type.  Note that this method requires a faulted <see name="Task{T}"/> to be transitioned back into a fulfilled
  /// one.</remarks>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <typeparam name="TNext">The transformed type.</typeparam>
  /// <returns>The transformed task.</returns>
  public static Task<TNext> BiBind<T, TNext>(
    this Task<T> task,
    Func<Exception, Task<TNext>> onFaulted,
    Func<T, Task<TNext>> onFulfilled
  ) => task.ContinueWith(async continuationTask => continuationTask.IsFaulted
    ? onFaulted(PotentiallyUnwindException(continuationTask.Exception!))
    : onFulfilled(await continuationTask)
  ).Unwrap().Unwrap();

  /// <summary>
  /// Disjunctive `leftMap`.
  /// </summary>
  /// <remarks>This method allows a faulted <see name="Task{T}"/>'s <see name="Exception"/> to be transformed into
  /// another type of <see name="Exception"/> (<see name="Task{T}"/>'s left side is pinned to <see name="Exception"/>).
  /// This can be used to transform an <see name="Exception"/> to add context or to use a custom exception
  /// type.</remarks>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <param name="onFaulted">The transformation function.</param>
  /// <returns>The transformated task.</returns>
  public static Task<T> ExceptionMap<T>(this Task<T> task, Func<Exception, Exception> onFaulted)
    => task.BiMap(onFaulted, Identity);

  /// <summary>
  /// Allows a fulfilled <see name="Task{T}"/> to be transitioned to a faulted one if the <paramref name="predicate"/>
  /// returns <code>false</code>.
  /// </summary>
  /// <remarks>This method provides another way to perform validation on the value contained within the
  /// <see name="Task{T}"/>.</remarks>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <param name="predicate">The predicate to run on the <see name="Task{T}"/>'s value.</param>
  /// <param name="exception">The exception to fault with if the <paramref name="predicate"/> returns
  /// <code>false</code>.</param>
  /// <returns>The transformed task.</returns>
  public static Task<T> Filter<T>(this Task<T> task, Predicate<T> predicate, Exception exception)
    => task.Filter(predicate, Constant(exception));

  /// <summary>
  /// Allows a fulfilled <see name="Task{T}"/> to be transitioned to a faulted one if the <paramref name="predicate"/>
  /// returns <code>false</code>.
  /// </summary>
  /// <remarks>This method provides another way to perform validation on the value contained within the
  /// <see name="Task{T}"/>.</remarks>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <param name="predicate">The predicate to run on the <see name="Task{T}"/>'s value.</param>
  /// <param name="supplier">A supplier function that produces the exception to fault with if the
  /// <paramref name="predicate"/> returns <code>false</code>.</param>
  /// <returns>The transformed task.</returns>
  public static Task<T> Filter<T>(this Task<T> task, Predicate<T> predicate, Func<Exception> supplier)
    => task.Filter(predicate, _ => supplier());

  /// <summary>
  /// Allows a fulfilled <see cref="Task{T}"/> to be transitioned to a faulted one if the <paramref name="predicate"/>
  /// returns <code>false</code>.
  /// </summary>
  /// <remarks>This method provides another way to perform validation on the value contained within the
  /// <see name="Task{T}"/>.</remarks>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <param name="predicate">The predicate to run on the <see cref="Task{T}"/>'s value.</param>
  /// <param name="morphism">A function that produces the exception to fault with if the <paramref name="predicate"/>
  /// returns <code>false</code>.</param>
  /// <returns>The transformed task.</returns>
  public static Task<T> Filter<T>(this Task<T> task, Predicate<T> predicate, Func<T, Exception> morphism)
    => task.Bind(value => predicate(value) ? Task.FromResult(value) : Task.FromException<T>(morphism(value)));

  /// <summary>
  /// Allows a fulfilled <see cref="Task{T}"/> to be transitioned to a faulted one if the <paramref name="predicate"/>
  /// return <code>false</code>.
  /// </summary>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <typeparam name="E">The type of <see cref="Exception"/> that <paramref name="morphism"/> returns.</typeparam>
  /// <param name="task">The task.</param>
  /// <param name="predicate">The predicate to run on the <see cref="Task{T}"/>'s value.</param>
  /// <param name="morphism">A function that produces a <see cref="Task{E}"/> to fault with if the
  /// <paramref name="predicate"/>returns <code>false</code>.</param>
  /// <returns>The transformed task.</returns>
  public static Task<T> Filter<T, E>(
    this Task<T> task,
    Predicate<T> predicate,
    Func<Task<E>> morphism
  ) where E : Exception => task.Filter(predicate, _ => morphism());

  /// <summary>
  /// Allows a fulfilled <see cref="Task{T}"/> to be transitioned to a faulted one if the <paramref name="predicate"/>
  /// return <code>false</code>.
  /// </summary>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <typeparam name="E">The type of <see cref="Exception"/> that <paramref name="morphism"/> returns.</typeparam>
  /// <param name="task">The task.</param>
  /// <param name="predicate">The predicate to run on the <see cref="Task{T}"/>'s value.</param>
  /// <param name="morphism">A function that produces a <see cref="Task{E}"/> to fault with if the
  /// <paramref name="predicate"/>returns <code>false</code>.</param>
  /// <returns>The transformed task.</returns>
  public static Task<T> Filter<T, E>(
    this Task<T> task,
    Predicate<T> predicate,
    Func<T, Task<E>> morphism
  ) where E : Exception => task.Bind(
    async value => predicate(value)
      ? Task.FromResult(value)
      : Task.FromException<T>(await morphism(value))
    ).Unwrap();

  /// <summary>
  /// Disjunctive 'rightMap'.  Can be thought of as 'fmap' for <see name="Task{T}"/>s.
  /// </summary>
  /// <remarks>This method allows the value in a fulfilled <see name="Task{T}"/> to be transformed into another
  /// type.</remarks>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <typeparam name="TNext">The transformed type.</typeparam>
  /// <param name="morphism">The transformation function.</param>
  /// <returns>The transformed task.</returns>
  public static Task<TNext> ResultMap<T, TNext>(this Task<T> task, Func<T, TNext> morphism)
    => task.BiMap(Identity, morphism);

  /// <summary>
  /// Allows a faulted <see name="Task{T}"/> to be transitioned to a fulfilled one.
  /// </summary>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <param name="morphism">The function to convert an <see name="Exception"/> into a <typeparamref name="T"/>.</param>
  /// <returns>The transformed task.</returns>
  public static Task<T> Recover<T>(this Task<T> task, Func<Exception, T> morphism)
    => task.Then(Identity, morphism);

  /// <summary>
  /// Allows a faulted <see name="Task{T}"/> to be transitioned to a fulfilled one.
  /// </summary>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <param name="morphism">The function to convert an <see name="Exception"/> into a <typeparamref name="T"/>.</param>
  /// <returns>The transformed task.</returns>
  public static Task<T> Recover<T>(this Task<T> task, Func<Exception, Task<T>> morphism)
    => task.BiBind(morphism, Task.FromResult);

  #endregion

  /// <summary>
  /// Allows a faulted <see name="Task{T}"/> to be transitioned to a fulfilled one.
  /// </summary>
  /// <remarks>This method is an alias to <code>Task.Recover</code>.</remarks>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <param name="onFaulted">The function to convert an <see name="Exception"/> into a
  /// <typeparamref name="T"/>.</param>
  /// <returns>The transformed task.</returns>
  public static Task<T> Catch<T>(this Task<T> task, Func<Exception, T> onFaulted) => task.Recover(onFaulted);

  /// <summary>
  /// Allows a faulted <see name="Task{T}"/> to be transitioned to a fulfilled one.
  /// </summary>
  /// <remarks>This method is an alias to <code>Task.Recover</code> in order to line up with the expected Promise
  /// API.</remarks>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <param name="onFaulted">The function to convert an <see name="Exception"/> into a
  /// <typeparamref name="T"/>.</param>
  /// <returns>The transformed task.</returns>
  public static Task<T> Catch<T>(this Task<T> task, Func<Exception, Task<T>> onFaulted) => task.Recover(onFaulted);

  public static Task<T> Delay<T>(
    this Task<T> task,
    TimeSpan delayInterval,
    CancellationToken cancellationToken = default
  ) => task.ContinueWith(async continuationTask =>
  {
    await Task.Delay(delayInterval, cancellationToken);

    return continuationTask;
  }).Unwrap().Unwrap();

  /// <summary>
  /// Faults a <see cref="Task{T}"/> with a provided <see cref="Exception"/>.
  /// </summary>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <param name="task">The task.</param>
  /// <param name="exception">The exception that will be used to fault the task.</param>
  /// <returns>The task.</returns>
  public static Task<T> Fault<T>(this Task<T> task, Exception exception) => task.Fault(_ => exception);

  /// <summary>
  /// Faults a <see cref="Task{T}"/> with an <see cref="Exception"/> produced by <paramref name="faultMorphism"/>.
  /// </summary>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <param name="task">The task.</param>
  /// <param name="faultMorphism">The function that will produce the <see cref="Exception"/>.</param>
  /// <returns>The task.</returns>
  public static Task<T> Fault<T>(this Task<T> task, Func<T, Exception> faultMorphism) => task.ResultMap(
    value => Task.FromException<T>(faultMorphism(value))
  ).Unwrap();

  /// <summary>
  /// Faults a <see cref="Task{T}"/> with a provided <see cref="Exception"/>.
  /// </summary>
  /// <typeparam name="T">The input task's underlying type.</typeparam>
  /// <typeparam name="TNext">The output task's underlying type.</typeparam>
  /// <param name="task">The task.</param>
  /// <param name="exception">The exception that will be used to fault the task.</param>
  /// <returns>The task.</returns>
  public static Task<TNext> Fault<T, TNext>(this Task<T> task, Exception exception) =>
    task.Fault<T, TNext>(_ => exception);

  /// <summary>
  /// Faults a <see cref="Task{T}"/> with an <see cref="Exception"/> produced by <paramref name="faultMorphism"/>.
  /// </summary>
  /// <typeparam name="T">The input task's underlying type.</typeparam>
  /// <typeparam name="TNext">The output task's underlying type.</typeparam>
  /// <param name="task">The task.</param>
  /// <param name="faultMorphism">The function that will produce the <see cref="Exception"/>.</param>
  /// <returns>The task.</returns>
  public static Task<TNext> Fault<T, TNext>(this Task<T> task, Func<T, Exception> faultMorphism) => task.ResultMap(
    value => Task.FromException<TNext>(faultMorphism(value))
  ).Unwrap();

  /// <summary>
  /// Performs an action if the <see name="Task{T}"/> is in a fulfilled state.
  /// </summary>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <param name="consumer">The action to perform if the task is fulfilled.</param>
  /// <returns>The task.</returns>
  ///
  public static Task<T> IfFulfilled<T>(this Task<T> task, Action<T> consumer)
    => task.ResultMap(TaskStatics.Tap(consumer));

  /// <summary>
  /// Performs an action if the <see name="Task{T}"/> is in a faulted state.
  /// </summary>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <param name="consumer">The action to perform if the task is faulted.</param>
  /// <returns>The task.</returns>
  public static Task<T> IfFaulted<T>(this Task<T> task, Action<Exception> onFaulted)
    => task.ExceptionMap(TaskStatics.Tap(onFaulted));

  public static Task<TNext> Retry<T, TNext>(
    this Task<T> task,
    Func<T, Task<TNext>> retryFunc,
    RetryParams retryOptions
  ) => task.ResultMap(value => TaskExtras.Retry(() => retryFunc(value), retryOptions)).Unwrap();

  public static Task<TNext> Retry<T, TNext>(this Task<T> task, Func<T, TNext> retryFunc, RetryParams retryOptions)
    => task.Retry(value => Task.FromResult(retryFunc(value)), retryOptions);

  public static Task<TNext> Retry<T, TNext>(this Task<T> task, Func<T, Task<TNext>> retryFunc)
    => task.Retry(retryFunc, RetryParams.Default);

  public static Task<TNext> Retry<T, TNext>(this Task<T> task, Func<T, TNext> retryFunc)
    => task.Retry(retryFunc, RetryParams.Default);

  /// <summary>
  /// Performs an action on a <see name="Task{T}"/> whether it is in a fulfilled or faulted state.
  /// </summary>
  /// <remarks>This method is useful if you need to perform a side effect without altering the <see name"Task{T}"/>'s
  /// value, such as logging.</remarks>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <param name="onFulfilled">The action to perform if the task is fulfilled.</param>
  /// <param name="onFaulted">The action to perform if the task is faulted.</param>
  /// <returns>The task.</returns>
  public static Task<T> Tap<T>(this Task<T> task, Action<T> onFulfilled, Action<Exception> onFaulted)
    => task.IfFulfilled(onFulfilled).IfFaulted(onFaulted);

  /// <summary>
  /// Transforms the value in a fulfilled <see name="Task{T}"/> to another type.
  /// </summary>
  /// <remarks>This method is an alias to <code>Task.ResultMap</code>.</remarks>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <typeparam name="TNext">The transformed type.</typeparam>
  /// <param name="onFulfilled">The transformation function.</param>
  /// <returns>The transformed task.</returns>
  public static Task<TNext> Then<T, TNext>(this Task<T> task, Func<T, TNext> onFulfilled)
    => task.ResultMap(onFulfilled);

  /// <summary>
  /// Transforms the value in a fulfilled <see name="Task{T}"/> to another type.
  /// </summary>
  /// <remarks>This method is an alias to <code>Task.Bind</code>.</remarks>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <typeparam name="TNext">The transformed type.</typeparam>
  /// <param name="onFulfilled">The transformation function.</param>
  /// <returns>The transformed task.</returns>
  public static Task<TNext> Then<T, TNext>(this Task<T> task, Func<T, Task<TNext>> onFulfilled)
    => task.Bind(onFulfilled);

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
}
