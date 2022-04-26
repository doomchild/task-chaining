using System;

namespace RLC.TaskChaining;

public record RetryOptions(
  int MaxRetries,
  double RetryInterval,
  double RetryBackoffRate,
  Action<int, double, Exception>? OnRetry,
  Predicate<Exception> ErrorEquals
)
{
  public static RetryOptions Default
  {
    get
    {
      return new (3, 1000, 2, (_, _, _) => { }, exception => true);
    }
  }
}
