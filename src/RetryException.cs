using System;

namespace RLC.TaskChaining;

public class RetryException : Exception
{
  public RetryException(int maxRetries, Exception? innerException = null)
    : base($"Retries exhausted after {maxRetries} attempts", innerException)
  {
  }
}
