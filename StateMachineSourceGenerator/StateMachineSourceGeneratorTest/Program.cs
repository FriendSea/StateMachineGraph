// See https://aka.ms/new-console-template for more information
namespace Hoge.Fuga
{
    partial class TestClass : FriendSea.StateMachine.IInjectable
    {
        [FriendSea.StateMachine.InjectContext]
        int hoge;

        [FriendSea.StateMachine.InjectContext]
        int fuga;
    }
}

namespace FriendSea.StateMachine
{
    interface IContextContainer { }

    interface IInjectable
    {
        void OnSetup(FriendSea.StateMachine.IContextContainer ctx);
    }
}
