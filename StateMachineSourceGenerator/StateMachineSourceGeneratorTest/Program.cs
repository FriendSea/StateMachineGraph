using FriendSea.StateMachine;

namespace Hoge.Fuga
{
    partial class TestClass : IBehaviour
    {
        [InjectContext]
        int hoge;

        [InjectContext]
        int fuga;
    }

    partial class TestClass2
    {
        [InjectContext]
        IContextContainer fuga;
    }

    partial class OuterClass
    {
        partial class InnerClass : IBehaviour
        {
            [InjectContext]
            IContextContainer piyo;
        }
    }
}

namespace FriendSea.StateMachine
{
    public interface IContextContainer { }

    public interface IInjectable
    {
        void OnSetup(FriendSea.StateMachine.IContextContainer ctx);
    }

    public interface IBehaviour
    {
    }
}
