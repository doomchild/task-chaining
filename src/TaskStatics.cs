﻿using System;
using System.Runtime.CompilerServices;

namespace RLC.TaskChaining;

public static class TaskStatics
{
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Func<T> Constant<T>(T value) => () => value;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Func<T, U> Constant<T, U>(U value) => _ => value;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Identity<T>(T value) => value;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Invoke<T>(Func<T> supplier) => supplier();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Func<TA, TC> Pipe2<TA, TB, TC>(Func<TA, TB> f, Func<TB, TC> g) => x => g(f(x));

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