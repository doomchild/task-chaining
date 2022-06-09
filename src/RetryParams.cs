using System;

namespace RLC.TaskChaining;

public record RetryParams(
  int? MaxRetries,
  TimeSpan? RetryInterval,
  double? RetryBackoffRate,
  Action<int, TimeSpan, Exception>? OnRetry,
  Predicate<Exception>? ShouldRetry
)
{
  public static RetryParams Default
  {
    get
    {
      return new (3, TimeSpan.FromMilliseconds(1000), 2, (_, _, _) => { }, exception => true);
    }
  }
}

internal record FixedRetryParams(
  int MaxRetries,
  TimeSpan RetryInterval,
  double RetryBackoffRate,
  Action<int, TimeSpan, Exception> OnRetry,
  Predicate<Exception> ShouldRetry
)
{
  public static FixedRetryParams Default
  {
    get
    {
      return new(3, TimeSpan.FromMilliseconds(1000), 2, (_, _, _) => { }, exception => true);
    }
  }
}
