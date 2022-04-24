using System;
using System.Threading.Tasks;

namespace RLC.TaskChaining;

public static class TaskExtras
{
	public static Func<T, Task<T>> RejectIf<T>(
		Predicate<T> predicate,
		Func<T, Exception> rejectionSupplier
	) => value => predicate(value)
		? Task.FromResult(value)
		: Task.FromException<T>(rejectionSupplier(value));

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

			return supplier();
		}).Unwrap();
}