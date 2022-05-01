using System;
using System.Runtime.CompilerServices;

namespace RLC.TaskChaining;

public static class TaskStatics
{
  /// <summary>
  /// Wraps a value in a supplier function.
  /// </summary>
  /// <typeparam name="T">The type of the supplied value.</typeparam>
  /// <param name="value">The value to supply.</param>
  /// <returns>A supplier function.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Func<T> Constant<T>(T value) => () => value;

  /// <summary>
  /// Wraps a value in a <typeparamref name="T"/> -> <typeparamref name="U"/> function that ignores the input value.
  /// </summary>
  /// <typeparam name="T">The input type of the resulting function.</typeparam>
  /// <typeparam name="U">The output type of the resulting function.</typeparam>
  /// <param name="value">A function that ignores its input argument and returns <paramref name="value"/></param>
  /// <returns></returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Func<T, U> Constant<T, U>(U value) => _ => value;

  /// <summary>
  /// Returns its input value.
  /// </summary>
  /// <typeparam name="T">Type of the input value.</typeparam>
  /// <param name="value">The value to return.</param>
  /// <returns>The input value.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Identity<T>(T value) => value;

  /// <summary>
  /// Executes the input supplier function.
  /// </summary>
  /// <typeparam name="T">The output type of the supplier function.</typeparam>
  /// <param name="supplier">The supplier function to execute.</param>
  /// <returns></returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Invoke<T>(Func<T> supplier) => supplier();

  /// <summary>
  /// Composes two functions together.
  /// </summary>
  /// <typeparam name="TA">The input type of the first function.</typeparam>
  /// <typeparam name="TB">The output type of the first function and the input type of the second function.</typeparam>
  /// <typeparam name="TC">The output type of the second function.</typeparam>
  /// <param name="f">The first function to compose.</param>
  /// <param name="g">The second function to compose.</param>
  /// <returns>The composition of both functions.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Func<TA, TC> Pipe2<TA, TB, TC>(Func<TA, TB> f, Func<TB, TC> g) => x => g(f(x));

  /// <summary>
  /// Wraps an <see cref="Action{TTappedValue}"/> in a <see cref="Func{TTappedValue, TTappedValue}"/> that executes the
  /// Action and then returns the input value.
  /// </summary>
  /// <typeparam name="TTappedValue">The type of the value passed into the Action and returned.</typeparam>
  /// <param name="consumer">The Action to perform on the input value.</param>
  /// <returns>A function that takes a value, executes the <param name="consumer"/>, and returns the value.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Func<TTappedValue, TTappedValue> Tap<TTappedValue>(Action<TTappedValue> consumer)
  {
    return value =>
    {
      consumer(value);

      return value;
    };
  }
}