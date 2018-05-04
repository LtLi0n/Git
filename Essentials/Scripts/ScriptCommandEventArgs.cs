using MUD_Server.Game.Entities.Players;

namespace MUD_Server.Essentials.Scripts
{
    public abstract class ScriptComamndEventArgs
    {
        public SocketUser User { get; }
        public SocketCharacter Character => User.OnlineCharacter;

        public ScriptComamndEventArgs(SocketUser user) => User = user;
        public ScriptComamndEventArgs() { }
    }

    public class SciptCommandEventArgs<T> : ScriptComamndEventArgs
    {
        public T Arg { get; }

        public SciptCommandEventArgs(SocketUser user, T arg) : base(user) => Arg = arg;
        public SciptCommandEventArgs(T arg) => Arg = arg;
    }

    public class SciptCommandEventArgs<T1, T2> : ScriptComamndEventArgs
    {
        public T1 Arg1 { get; }
        public T2 Arg2 { get; }

        public SciptCommandEventArgs(SocketUser user, T1 arg1, T2 arg2) : base(user)
        {
            Arg1 = arg1;
            Arg2 = arg2;
        }
        public SciptCommandEventArgs(T1 arg1, T2 arg2)
        {
            Arg1 = arg1;
            Arg2 = arg2;
        }
    }
}
