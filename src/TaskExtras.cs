using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RLC.TaskChaining;

public static class TaskExtras
{
	/// <summary>
  /// Produces a faulted <see cref="Task{T}"/> if the <paramref name="predicate"/> returns <code>false</code>.
  /// </summary>
  /// <example>This is a handy way to perform validation.  You might do something like the following:
  /// <code>
  /// Task.FromResult(someString)
  ///   .Then(RejectIf(
  ///			string.IsNullOrWhitespace,
  ///			s => new Exception($"nameof(someString) must not be blank")
  ///   ));
  /// </code>
  /// </example>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <param name="predicate">A predicate to evaluate with the input argument.</param>
  /// <param name="rejectionMorphism">A function that takes a <typeparamref name="T"/> and returns an
  /// <see cref="Exception"/>.</param>
  /// <returns>A function that performs rejection.</returns>
	public static Func<T, Task<T>> RejectIf<T>(
		Predicate<T> predicate,
		Func<T, Exception> rejectionMorphism
	) => value => predicate(value)
		? Task.FromException<T>(rejectionMorphism(value))
		: Task.FromResult(value);

	/// <summary>
	/// Produces a faulted <see cref="Task{T}"/> if the <paramref name="predicate"/> returns <code>false</code>.
	/// </summary>
	/// <example>This is a handy way to perform validation.  You might do something like the following:
	/// <code>
	/// Task.FromResult(someString)
	///   .Then(RejectIf(
	///			string.IsNullOrWhitespace,
	///			async s => BuildTaskOfException(s)
	///   ));
	/// </code>
	/// </example>
	/// <typeparam name="T">The task's underlying type.</typeparam>
	/// <param name="predicate">A predicate to evaluate with the input argument.</param>
	/// <param name="rejectionMorphism">A function that takes a <typeparamref name="T"/> and returns a
	/// <see cref="Task{Exception}"/>.</param>
	/// <returns>A function that performs rejection.</returns>
	public static Func<T, Task<T>> RejectIf<T, E>(
		Predicate<T> predicate,
		Func<T, Task<E>> rejectionMorphism
	) where E: Exception => value => Task.FromResult(value)
		.Then(async v => predicate(value)
			? await Task.FromException<T>(await rejectionMorphism(v))
			: v
		);
	//) => async value => predicate(value)
	//	? await Task.FromException<T>(await rejectionMorphism(value))
	//	: value;

  /// <summary>
  /// Allows a faulted <see cref="Task{T}"/> to transform its <see cref="Exception"/> into a different type of
  /// <see cref="Exception"/> if the <paramref name="predicate"/> returns <code>true</code>.
  /// </summary>
  /// <remarks>This is intended to be used with <code>Task.Catch</code> to allow for converting one
  /// <see cref="Exception"/> into another.  For example, a <see cref="System.Web.HttpException"/> could be wrapped
  /// with a custom type that adds extra context.</remarks>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <param name="predicate">A predicate to evaluate with the <see cref="Task{T}"/>'s <see cref="Exception"/>.</param>
  /// <param name="rerejectionMorphism">A function that takes an <see cref="Exception"/> and returns an
  /// <see cref="Exception"/>.</param>
  /// <returns>A function that performs re-rejection.</returns>
  public static Func<Exception, Task<T>> ReRejectIf<T>(
		Predicate<Exception> predicate,
		Func<Exception, Exception> rerejectionMorphism
	) => exception => predicate(exception)
		? Task.FromException<T>(rerejectionMorphism(exception))
		: Task.FromException<T>(exception);

	/// <summary>
	/// Produces a fulfilled <see cref="Task{T}"/> using the output of the <paramref name="fulfillmentMorphism"/> if the
	/// <paramref name="predicate"/> returns <code>true</code>.
	/// </summary>
	/// <example>This is a handy way to return to a non-faulted state.  You might do something like the following:
	/// <code>
	/// Task.FromResult(someUrl)
	///   .Then(httpClient.GetAsync)
	///   .Catch(ResolveIf(
	///			exception => exception is HttpRequestException,
	///			exception => someFallbackValue
	///   ));
	/// </code>
	/// </example>
	/// <typeparam name="T">The task's underlying type.</typeparam>
	/// <param name="predicate">A predicate to evaluate with the <see cref="Task{T}"/>'s <see cref="Exception"/>.</param>
	/// <param name="fulfillmentMorphism">A function that takes an <see cref="Exception"/> and returns a
  /// <typeparamref name="T"/>.</param>
	/// <returns>A function that performs fulfillment.</returns>
	public static Func<Exception, Task<T>> ResolveIf<T>(
		Predicate<Exception> predicate,
		Func<Exception, T> fulfillmentMorphism
	) => value => predicate(value)
		? Task.FromResult(fulfillmentMorphism(value))
		: Task.FromException<T>(value);

	/// <summary>
	/// Produces a fulfilled <see cref="Task{T}"/> using the output of the <paramref name="resolutionSupplier"/> if the
	/// <paramref name="predicate"/> returns <code>true</code>.
	/// </summary>
	/// <example>This is a handy way to return to a non-faulted state.  You might do something like the following:
	/// <code>
	/// Task.FromResult(someUrl)
	///   .Then(httpClient.GetAsync)
	///   .Catch(ResolveIf(
	///			exception => exception is HttpRequestException,
	///			exception => someFallbackValue
	///   ));
	/// </code>
	/// </example>
	/// <typeparam name="T">The task's underlying type.</typeparam>
	/// <param name="predicate">A predicate to evaluate with the <see cref="Task{T}"/>'s <see cref="Exception"/>.</param>
	/// <param name="resolutionSupplier">A function that takes an <see cref="Exception"/> and returns a
	/// <typeparamref name="T"/>.</param>
	/// <returns>A function that performs fulfillment.</returns>
	public static Func<Exception, Task<T>> ResolveIf<T>(
		Predicate<Exception> predicate,
		Func<Exception, Task<T>> resolutionSupplier
	) => value => predicate(value)
		? resolutionSupplier(value)
		: Task.FromException<T>(value);

  /// <summary>
  /// Invokes <paramref name="action"/> if <paramref name="predicate"/> succeeds.
  /// </summary>
  /// <example>This is useful for conditionally executing a side effect.
  /// <code>
  /// Task.FromResult(someUrl)
  ///   .Then(httpClient.GetAsync)
  ///   .IfFulfilled(InvokeIf(
  ///     httpResponse => !httpResponse.IsSuccessStatusCode,
  ///     response => _logger.LogWarning("Got '{StatusCode}' response from server", response.StatusCode)
  ///   );
  /// </code>
  /// </example>
  /// <param name="predicate">A predicate to evaluate with the <see cref="Task{T}"/>'s value.</param>
  /// <param name="action">The function to invoke if <paramref name="predicate"/> returns <code>true</code>.</param>
  /// <typeparam name="T">The type passed into <code>InvokeIf</code>.</typeparam>
  /// <returns>A function that conditionally invokes another function.</returns>
  public static Func<T, Task<T>> InvokeIf<T>(
    Predicate<T> predicate,
    Action<T> action
  )
  {
    return value =>
    {
      if (predicate(value))
      {
        action(value);
      }

      return Task.FromResult(value);
    };
  }

  /// <summary>
  /// Invokes <paramref name="func"/> if <paramref name="predicate"/> succeeds.
  /// </summary>
  /// <example>This is useful for conditionally executing a side effect.
  /// <code>
  /// Task.FromResult(someUrl)
  ///   .Then(httpClient.GetAsync)
  ///   .IfFulfilled(InvokeIf(
  ///     httpResponse => !httpResponse.IsSuccessStatusCode,
  ///     response => _logger.LogWarning("Got '{StatusCode}' response from server", response.StatusCode)
  ///   );
  /// </code>
  /// </example>
  /// <param name="predicate">A predicate to evaluate with the <see cref="Task{T}"/>'s value.</param>
  /// <param name="func">The function to invoke if <paramref name="predicate"/> returns <code>true</code>.</param>
  /// <typeparam name="T">The type passed into <code>InvokeIf</code>.</typeparam>
  /// <returns>A function that conditionally invokes another function.</returns>
  public static Func<T, Task<T>> InvokeIf<T>(
    Predicate<T> predicate,
    Func<T, T> func
  )
  {
    return value =>
    {
      if (predicate(value))
      {
        return Task.FromResult(func(value));
      }

      return Task.FromResult(value);
    };
  }

  /// <summary>
  /// Invokes <paramref name="func"/> if <paramref name="predicate"/> succeeds.
  /// </summary>
  /// <example>This is useful for conditionally executing a side effect.
  /// <code>
  /// Task.FromResult(someUrl)
  ///   .Then(httpClient.GetAsync)
  ///   .IfFulfilled(InvokeIf(
  ///     httpResponse => !httpResponse.IsSuccessStatusCode,
  ///     response => _logger.LogWarning("Got '{StatusCode}' response from server", response.StatusCode)
  ///   );
  /// </code>
  /// </example>
  /// <param name="predicate">A predicate to evaluate with the <see cref="Task{T}"/>'s value.</param>
  /// <param name="func">The function to invoke if <paramref name="predicate"/> returns <code>true</code>.</param>
  /// <typeparam name="T">The type passed into <code>InvokeIf</code>.</typeparam>
  /// <returns>A function that conditionally invokes another function.</returns>
  public static Func<T, Task<T>> InvokeIf<T>(
    Predicate<T> predicate,
    Func<T, Task<T>> func
  )
  {
    return value =>
    {
      if (predicate(value))
      {
        return func(value);
      }

      return Task.FromResult(value);
    };
  }

  /// <summary>
  /// Invokes <paramref name="func"/> if <paramref name="predicate"/> succeeds.
  /// </summary>
  /// <example>This is useful for conditionally executing a side effect.
  /// <code>
  /// Task.FromResult(someUrl)
  ///   .Then(httpClient.GetAsync)
  ///   .IfFulfilled(InvokeIf(
  ///     httpResponse => !httpResponse.IsSuccessStatusCode,
  ///     response => _logger.LogWarning("Got '{StatusCode}' response from server", response.StatusCode)
  ///   );
  /// </code>
  /// </example>
  /// <param name="predicate">A predicate to evaluate with the <see cref="Task{T}"/>'s value.</param>
  /// <param name="func">The function to invoke if <paramref name="predicate"/> returns <code>true</code>.</param>
  /// <typeparam name="T">The type passed into <code>InvokeIf</code>.</typeparam>
  /// <returns>A function that conditionally invokes another function.</returns>
  public static Func<T, Task<T>> InvokeIf<T>(
    Predicate<T> predicate,
    Func<T, Task> func
  )
  {
    return value =>
    {
      if (predicate(value))
      {
        return Task.FromResult(value).Then(func).Then(_ => value);
      }

      return Task.FromResult(value);
    };
  }

	/// <summary>
  /// A function that executes the <paramref name="supplier"/> after <paramref name="deferTime"/> has elapsed.
  /// </summary>
  /// <typeparam name="T">The task's underlying type.</typeparam>
  /// <param name="supplier">A supplier function.</param>
  /// <param name="deferTime">The length of time to defer execution.</param>
  /// <returns>A function that performs deferred execution.</returns>
	public static Task<T> Defer<T>(Func<T> supplier, TimeSpan deferTime) =>
		Defer(() => Task.FromResult(supplier()), deferTime);

	/// <summary>
	/// A function that executes the <paramref name="supplier"/> after <paramref name="deferTime"/> has elapsed.
	/// </summary>
	/// <typeparam name="T">The task's underlying type.</typeparam>
	/// <param name="supplier">A supplier function.</param>
	/// <param name="deferTime">The length of time to defer execution.</param>
	/// <returns>A function that performs deferred execution.</returns>
	public static Task<T> Defer<T>(Func<Task<T>> supplier, TimeSpan deferTime) =>
		Task.Run(async () =>
		{
			await Task.Delay(deferTime);

			return await supplier();
		});

	private static FixedRetryParams RetryDefaults = FixedRetryParams.Default;

  private static Task<T> DoRetry<T>(
		Func<Task<T>> supplier,
		FixedRetryParams retryParams,
		Exception? exception,
		int attempts = 0
	)
  {
		TimeSpan duration = TimeSpan.FromMilliseconds(
			retryParams.RetryInterval.TotalMilliseconds * Math.Pow(retryParams.RetryBackoffRate, attempts)
		);

		return attempts >= retryParams.MaxRetries
			? Task.FromException<T>(new RetryException(attempts, exception))
			: Task.Run(supplier)
				.Catch(ResolveIf(
					ex => retryParams.ShouldRetry(ex),
          ex =>
          {
            if(retryParams.OnRetry != null)
            {
              retryParams.OnRetry(attempts, duration, ex);
            }

            return Defer(
              () => DoRetry(supplier, retryParams, ex, attempts + 1),
              duration
            );
          }
        ));
  }

  /// <summary>
  /// Partitions a collection of <see cref="Task{T}"/> into faulted and fulfilled lists.
  /// </summary>
  /// <typeparam name="T">The underlying type of the tasks in <paramref name="tasks"/>.</typeparam>
  /// <param name="tasks">The collection of tasks to partition.</param>
  /// <returns>A tuple of the partitioned tasks.</returns>
  public static Task<(IReadOnlyList<Exception> Faulted, IReadOnlyList<T> Fulfilled)> Partition<T>(
    IEnumerable<Task<T>> tasks
  )
  {
    return tasks.Aggregate<Task<T>, Task<(IReadOnlyList<Exception> Faulted, IReadOnlyList<T> Fulfilled)>>(
      Task.FromResult(((IReadOnlyList<Exception>)new List<Exception>(), (IReadOnlyList<T>)new List<T>())),
      ZipTasksWith<T, (IReadOnlyList<Exception> Faulted, IReadOnlyList<T> Fulfilled), (IReadOnlyList<Exception> Faulted, IReadOnlyList<T> Fulfilled), Exception>
      (
        (values, v) => (values.Faulted, values.Fulfilled.Append(v).ToList()),
        (values, v) => (values.Faulted.Append<Exception>(v).ToList(), values.Fulfilled)
      )
    );
  }

  private static Func<Task<TAccum>, Task<TValue>, Task<TResult>> ZipTasksWith<TValue, TAccum, TResult, TException>(
    Func<TAccum, TValue, TResult> f,
    Func<TAccum, TException, TResult> g
  ) where TException : Exception
  {
    return (b, a) => a.Then(
      valueA => b.Then(valueB => f(valueB, valueA)),
      error => b.Then(valueB => g(valueB, (TException)error))
    );
  }

	/// <summary>
	/// A function that performs retries of the <paramref name="supplier"/> if it fails.
	/// </summary>
	/// <typeparam name="T">The task's underlying type.</typeparam>
	/// <param name="supplier">A supplier function.</param>
	/// <param name="retryParams">The retry parameters.</param>
	/// <returns>A function that retries execution of the <paramref name="supplier"/>.</returns>
	public static Task<T> Retry<T>(Func<Task<T>> supplier, RetryParams retryParams)
	{
		FixedRetryParams fixedRetryParams = new FixedRetryParams(
			retryParams.MaxRetries ?? RetryDefaults.MaxRetries,
			retryParams.RetryInterval ?? RetryDefaults.RetryInterval,
			retryParams.RetryBackoffRate ?? RetryDefaults.RetryBackoffRate,
			retryParams.OnRetry ?? RetryDefaults.OnRetry,
			retryParams.ShouldRetry ?? RetryDefaults.ShouldRetry
		);

		return DoRetry(supplier, fixedRetryParams, null, 0);
	}

  /// <summary>
	/// A function that performs retries of the <paramref name="supplier"/> if it fails using the default retry parameters.
	/// </summary>
	/// <typeparam name="T">The task's underlying type.</typeparam>
	/// <param name="supplier">A supplier function.</param>
	/// <returns>A function that retries execution of the <paramref name="supplier"/>.</returns>
	public static Task<T> Retry<T>(Func<Task<T>> supplier) =>
		Retry(supplier, RetryParams.Default);

  /// <summary>
	/// A function that performs retries of the <paramref name="supplier"/> if it fails.
	/// </summary>
	/// <typeparam name="T">The task's underlying type.</typeparam>
	/// <param name="supplier">A supplier function.</param>
	/// <param name="retryParams">The retry parameters.</param>
	/// <returns>A function that retries execution of the <paramref name="supplier"/>.</returns>
	public static Task<T> Retry<T>(Func<T> supplier, RetryParams retryParams) =>
		Retry(() => Task.Run(supplier), retryParams);

  /// <summary>
	/// A function that performs retries of the <paramref name="supplier"/> if it fails using the default retry parameters.
	/// </summary>
	/// <typeparam name="T">The task's underlying type.</typeparam>
	/// <param name="supplier">A supplier function.</param>
	/// <returns>A function that retries execution of the <paramref name="supplier"/>.</returns>
	public static Task<T> Retry<T>(Func<T> supplier) =>
		Retry(supplier, RetryParams.Default);
}
