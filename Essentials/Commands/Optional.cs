using System;

namespace MUD_Server.Essentials.Commands
{
    public class Optional<T> : Optional
    {
        public T Value { get; }
        public Optional(T value) => Value = value;
        public Optional() => Value = default;

        public override bool Equals(object obj) => (object)Value == obj;

        public static bool operator ==(Optional<T> left, T right) => (object)left.Value == (object)right;
        public static bool operator ==(T left, Optional<T> right) => (object)left == (object)right.Value;

        public static bool operator !=(Optional<T> left, T right) => (object)left.Value != (object)right;
        public static bool operator !=(T left, Optional<T> right) => (object)left != (object)right.Value;

        public static T operator +(Optional<T> left, object right) => (dynamic)left.Value + (dynamic)right;
        public static T operator +(object left, Optional<T> right) => (dynamic)left + (dynamic)right.Value;

        public static T operator -(Optional<T> left, object right) => (dynamic)left.Value - (dynamic)right;
        public static T operator -(object left, Optional<T> right) => (dynamic)left - (dynamic)right.Value;
    }

    public class Optional
    {
        public static object Create(object value) => Activator.CreateInstance(typeof(Optional<>).MakeGenericType(value.GetType()), value);
        public static object Create(Type type) => Activator.CreateInstance(typeof(Optional<>).MakeGenericType(type));

        public static object CreateCopy(Type type) => Activator.CreateInstance(typeof(Optional<>).MakeGenericType(type));
    }
}
