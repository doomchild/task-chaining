using System;

namespace RLC.TaskChaining;

public class RetryException : Exception
{
  public RetryException(int maxRetries)
    : base($"Retries exhausted after {maxRetries} attempts")
  {
  }
}
