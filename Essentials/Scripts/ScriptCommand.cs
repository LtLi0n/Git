using System;
using MUD_Server.Essentials.Commands;
using MUD_Server.Game.Entities.Players;

namespace MUD_Server.Essentials.Scripts
{
    public abstract class ScriptCommand
    {
        public string Trigger { get; }
        public ScriptCommand(string trigger) => Trigger = trigger;

        public abstract string Translate(SocketUser user, params object[] args);
        public abstract string Translate(params object[] args);

        /// <summary> Create optional/regular variable, whatever the command expects.</summary>
        protected static T MakeVar<T>(object obj)
        {
            if (typeof(T).BaseType == typeof(Optional)) return (T)Optional.Create(obj);
            else return (T)obj;
        }
    }

    public class ScriptCommand<T> : ScriptCommand
    {
        private Func<SciptCommandEventArgs<T>, object> _runFunc;
        public ScriptCommand(string trigger, Func<SciptCommandEventArgs<T>, object> runFunc) : base(trigger) => _runFunc = runFunc;

        public void Execute(SocketUser user, T arg) => _runFunc(new SciptCommandEventArgs<T>(user, arg));
        public void Execute(T arg) => _runFunc(new SciptCommandEventArgs<T>(arg));

        ///<summary>Return string which is recognized for cinematic displays.</summary>
        public override string Translate(SocketUser user, params object[] args) => _runFunc(new SciptCommandEventArgs<T>(user, (T)args[0])).ToString();
        ///<summary>Return string which is recognized for cinematic displays.</summary>
        public override string Translate(params object[] args) => _runFunc(new SciptCommandEventArgs<T>(null, (T)args[0])).ToString();
    }

    public class ScriptCommand<T1, T2> : ScriptCommand
    {
        private Func<SciptCommandEventArgs<T1, T2>, object> _runFunc;
        public ScriptCommand(string trigger, Func<SciptCommandEventArgs<T1, T2>, object> runFunc) : base(trigger) => _runFunc = runFunc;

        public void Execute(SocketUser user, T1 arg1, T2 arg2) => _runFunc(new SciptCommandEventArgs<T1, T2>(user, arg1, arg2));
        public void Execute(T1 arg1, T2 arg2) => _runFunc(new SciptCommandEventArgs<T1, T2>(arg1, arg2));

        ///<summary>Return string which is recognized for cinematic displays.</summary>
        public override string Translate(SocketUser user, params object[] args)
        {
            if(args.Length == 2) return _runFunc(new SciptCommandEventArgs<T1, T2>(user, MakeVar<T1>(args[0]), MakeVar<T2>(args[1]))).ToString();
            else return _runFunc(new SciptCommandEventArgs<T1, T2>(user, MakeVar<T1>(args[0]), Activator.CreateInstance<T2>())).ToString();
        }
        ///<summary>Return string which is recognized for cinematic displays.</summary>
        public override string Translate(params object[] args)
        {
            if (args.Length == 2) return _runFunc(new SciptCommandEventArgs<T1, T2>(null, (T1)args[0], (T2)args[1])).ToString();
            else return _runFunc(new SciptCommandEventArgs<T1, T2>(null, (T1)args[0], Activator.CreateInstance<T2>())).ToString();
        }
    }
}
