using System;

namespace MUD_Server.DevTools
{
    public static class Library
    {
        public static class Tuples
        {
            public static Tuple<T1, T2> ToTuple<T1, T2>(object[] input) => new Tuple<T1, T2>((T1)input[0], (T2)input[1]);
            public static Tuple<T1, T2, T3> ToTuple<T1, T2, T3>(object[] input) => new Tuple<T1, T2, T3>((T1)input[0], (T2)input[1], (T3)input[2]);
            public static Tuple<T1, T2, T3, T4> ToTuple<T1, T2, T3, T4>(object[] input) => new Tuple<T1, T2, T3, T4>((T1)input[0], (T2)input[1], (T3)input[2], (T4)input[3]);

            public static bool IsTupleType(Type type, bool checkBaseTypes = false)
            {
                if (type == null) throw new ArgumentNullException(nameof(type));
                if (type == typeof(Tuple)) return true;
                if (type == typeof(ValueTuple)) return true;

                while (type != null)
                {
                    if (type.IsGenericType)
                    {
                        var genType = type.GetGenericTypeDefinition();
                        if (genType == typeof(Tuple<>)
                            || genType == typeof(Tuple<,>)
                            || genType == typeof(Tuple<,,>)
                            || genType == typeof(Tuple<,,,>)
                            || genType == typeof(Tuple<,,,,>)
                            || genType == typeof(Tuple<,,,,,>)
                            || genType == typeof(Tuple<,,,,,,>)
                            || genType == typeof(Tuple<,,,,,,,>)
                            || genType == typeof(Tuple<,,,,,,,>))
                            return true;

                        if (genType == typeof(ValueTuple<>)
                            || genType == typeof(ValueTuple<,>)
                            || genType == typeof(ValueTuple<,,>)
                            || genType == typeof(ValueTuple<,,,>)
                            || genType == typeof(ValueTuple<,,,,>)
                            || genType == typeof(ValueTuple<,,,,,>)
                            || genType == typeof(ValueTuple<,,,,,,>)
                            || genType == typeof(ValueTuple<,,,,,,,>)
                            || genType == typeof(ValueTuple<,,,,,,,>))
                            return true;
                    }

                    if (!checkBaseTypes) break;

                    type = type.BaseType;
                }

                return false;
            }
        }
    }
}
