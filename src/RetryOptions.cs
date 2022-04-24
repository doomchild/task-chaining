using System;

namespace RLC.TaskChaining;

public record RetryOptions<T>(
  int MaxRetries,
  double RetryInterval,
  double RetryBackoffRate,
  Action<int, double, Exception>? OnRetry,
  Predicate<Exception> ErrorEquals
)
{
  public static RetryOptions<T> Default
  {
    get
    {
      return new (3, 1000, 2, (_, _, _) => { }, exception => true);
    }
  }
}