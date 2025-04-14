// See https://aka.ms/new-console-template for more information

using FriendSea.StateMachine;

namespace Hoge.Fuga
{
    partial class TestClass
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
}

namespace FriendSea.StateMachine
{
    public interface IContextContainer { }

    public interface IInjectable
    {
        void OnSetup(FriendSea.StateMachine.IContextContainer ctx);
    }
}
