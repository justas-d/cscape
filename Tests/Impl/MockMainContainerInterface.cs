using System;
using CScape.Core.Game.Interface;
using JetBrains.Annotations;

namespace CScape.Dev.Tests.Impl
{
    public class MockMainContainerInterface : MockMainInterface, IContainerInterface
    {
        public MockMainContainerInterface(IServiceProvider services,
            int id, [CanBeNull] IButtonHandler buttonHandler = null) : base(id, buttonHandler)
        {
            Items = new BasicItemManager(Id, services, new MockItemProvider(10));
        }

        public IItemContainer Items { get; }
    }
}