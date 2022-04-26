using System;

namespace RLC.TaskChaining;

public record RetryOptions(
  int MaxRetries,
  TimeSpan RetryInterval,
  double RetryBackoffRate,
  Action<int, TimeSpan, Exception>? OnRetry,
  Predicate<Exception> ShouldRetry
)
{
  public static RetryOptions Default
  {
    get
    {
      return new (3, TimeSpan.FromMilliseconds(1000), 2, (_, _, _) => { }, exception => true);
    }
  }
}
