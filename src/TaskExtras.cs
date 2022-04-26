using System;
using System.Threading.Tasks;

namespace RLC.TaskChaining;

public static class TaskExtras
{
	public static Func<T, Task<T>> RejectIf<T>(
		Predicate<T> predicate,
		Func<T, Exception> rejectionSupplier
	) => value => predicate(value)
		? Task.FromException<T>(rejectionSupplier(value))
		: Task.FromResult(value);

	public static Func<Exception, Task<T>> ResolveIf<T>(
		Predicate<Exception> predicate,
		Func<Exception, T> resolutionSupplier
	) => value => predicate(value)
		? Task.FromResult(resolutionSupplier(value))
		: Task.FromException<T>(value);

	public static Func<Exception, Task<T>> ResolveIf<T>(
		Predicate<Exception> predicate,
		Func<Exception, Task<T>> resolutionSupplier
	) => value => predicate(value)
		? resolutionSupplier(value)
		: Task.FromException<T>(value);

	public static Task<T> Defer<T>(Func<T> supplier, TimeSpan deferTime) =>
		Defer(() => Task.FromResult(supplier()), deferTime);

	public static Task<T> Defer<T>(Func<Task<T>> supplier, TimeSpan deferTime) =>
		Task.Run(async () =>
		{
			await Task.Delay(deferTime);

			return await supplier();
		});

	private static Task<T> DoRetry<T>(
		Func<Task<T>> supplier,
		RetryOptions retryOptions,
		Exception? exception,
		int attempts = 0
	)
  {
		double duration = retryOptions.RetryInterval * Math.Pow(retryOptions.RetryBackoffRate, attempts);

		if (retryOptions.OnRetry != null && exception != null && attempts > 0)
    {
			retryOptions.OnRetry(attempts, duration, exception);
    }

		return attempts >= retryOptions.MaxRetries
			? Task.FromException<T>(new RetryException(attempts))
			: Task.Run(supplier)
				.Catch(ResolveIf(
					exception => retryOptions.ErrorEquals(exception),
					exception => Defer(
						() => DoRetry(supplier, retryOptions, exception, attempts + 1),
						TimeSpan.FromMilliseconds(duration)
					)
				));
  }

	public static Task<T> Retry<T>(Func<Task<T>> supplier, RetryOptions retryOptions) =>
		DoRetry(supplier, retryOptions, null, 0);

	public static Task<T> Retry<T>(Func<Task<T>> supplier) =>
		Retry(supplier, RetryOptions.Default);

	public static Task<T> Retry<T>(Func<T> supplier, RetryOptions retryOptions) =>
		Retry(() => Task.Run(supplier), retryOptions);

	public static Task<T> Retry<T>(Func<T> supplier) =>
		Retry(supplier, RetryOptions.Default);
}
