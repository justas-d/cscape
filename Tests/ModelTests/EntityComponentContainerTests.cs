using System;
using CScape.Core;
using CScape.Core.Extensions;
using CScape.Core.Game.Entity;
using CScape.Dev.Tests.Impl;
using CScape.Models;
using CScape.Models.Extensions;
using CScape.Models.Game.Entity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CScape.Dev.Tests.ModelTests
{
    public interface IModelImplementation
    {
        IEntityHandle CreateEntity(string name = "test entity");
    }

    public class CoreModelImpl : IModelImplementation
    {
        public IEntityHandle CreateEntity(string name)
        {
            var services = new ServiceCollection();
            services.AddSingleton<IEntitySystem>(s => new EntitySystem(s.ThrowOrGet<IGameServer>()));
            services.AddSingleton<ILogger>(new TestLogger());
            var server = new GameServer(services);
            return server.Services.ThrowOrGet<IEntitySystem>().Create(name);
        }
    }

    public static class ModelImpl
    {
        public static IModelImplementation Active { get; } = new CoreModelImpl();
    }

    [TestClass]
    public class EntityComponentContainerTests
    {
        private interface IComponentOne : IEntityComponent
        { }

        private interface IComponentTwo : IEntityComponent
        { }

        private interface IComponentThree : IEntityComponent
        { }

        private sealed class MultiInterfaceComponent : IComponentOne, IComponentTwo, IComponentThree
        {
            public IEntity Parent { get; }
            public int Priority => 0;

            public int NumReceivedMessages { get; private set; }

            public MultiInterfaceComponent(IEntity ent)
            {
                Parent = ent;
            }

            public void ReceiveMessage(IGameMessage msg)
            {
                NumReceivedMessages++;
            }
        }

        private sealed class TestMessage : IGameMessage
        {
            public int EventId => 0;
        }

        private static (IEntity Entity, MultiInterfaceComponent Component) GetTestData()
        {
            var entity = ModelImpl.Active.CreateEntity().Get();
            var component = new MultiInterfaceComponent(entity);
            return (entity, component);
        }

        private static void AssertContainsComponent(IEntity entity, Type type)
        {
            Assert.IsTrue(entity.Components.Contains(type));
        }

        private static void AssertContainsComponent<T>(IEntity entity)
            where T : class, IEntityComponent
        {
            Assert.IsTrue(entity.Components.Contains<T>());
        }

        private static void AssertDoesNotContainComponent(IEntity entity, Type type)
        {
            Assert.IsFalse(entity.Components.Contains(type));
        }

        private static void AssertDoesNotContainComponent<T>(IEntity entity)
            where T : class, IEntityComponent
        {
            Assert.IsFalse(entity.Components.Contains<T>());
        }


        [TestMethod]
        public void DoesContainsWorksOnPurelyMappedComponents()
        {
            var data = GetTestData();

            data.Entity.Components.Add(data.Component.GetType(), data.Component);

            AssertContainsComponent(data.Entity, data.Component.GetType());
        }


        [TestMethod]
        public void DoesContainsWorksOnPurelyMappedComponentsGeneric()
        {
            var data = GetTestData();

            data.Entity.Components.Add(data.Component);

            AssertContainsComponent<MultiInterfaceComponent>(data.Entity);
        }

        [TestMethod]
        public void DoesContainsWorksOnMappedComponents()
        {
            var data = GetTestData();

            data.Entity.Components.Add(typeof(IComponentOne), data.Component);

            AssertContainsComponent(data.Entity, typeof(IComponentOne));
        }

        [TestMethod]
        public void DoesContainsWorksOMappedComponentsGeneric()
        {
            var data = GetTestData();

            data.Entity.Components.Add<IComponentOne>(data.Component);

            AssertContainsComponent<IComponentOne>(data.Entity);
        }

        [TestMethod]
        public void MappedTypeMustNotMapToPureType()
        {
            var data = GetTestData();

            data.Entity.Components.Add(typeof(IComponentOne), data.Component);

            AssertDoesNotContainComponent(data.Entity, typeof(MultiInterfaceComponent));
        }

        [TestMethod]
        public void MappedTypeMustNotMapToPureTypeGeneric()
        {
            var data = GetTestData();

            data.Entity.Components.Add<IComponentOne>(data.Component);

            AssertDoesNotContainComponent<MultiInterfaceComponent>(data.Entity);
        }

        // todo : write add/remove tests
        // todo : finish component container tests

        [TestMethod]
        public void CanAddComponentsDuringComponentIterationUsingGetSorted()
        {
            var testEntity = ModelImpl.Active.CreateEntity().Get();
            var component = new MultiInterfaceComponent(testEntity);

            testEntity.Components.Add(component);

            foreach (var element in testEntity.Components.GetSorted())
                testEntity.Components.Add<IComponentOne>(component);

            AssertContainsComponent<IComponentOne>(testEntity);
        }

        [TestMethod]
        public void CanRemoveComponentsDuringComponentIterationUsingGetSorted()
        {

        }

        [TestMethod]
        public void CanAddComponentsDuringComponentIterationUsingLookup()
        {
            
        }

        [TestMethod]
        public void CanRemoveComponentsDuringComponentIterationUsingLookup()
        {

        }

        [TestMethod]
        public void SendMessageThrowsNullOnNullMessage()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                var testEntityHandle = ModelImpl.Active.CreateEntity();
                var testEntity = testEntityHandle.Get();

                // ReSharper disable once AssignNullToNotNullAttribute
                testEntity.SendMessage(null);
            });
        }

        [TestMethod]
        public void SendMessageShouldSendAMessageOnceToEachComponent()
        {
            var testEntityHandle = ModelImpl.Active.CreateEntity();
            var testEntity = testEntityHandle.Get();
            var component = new MultiInterfaceComponent(testEntity);
            var message = new TestMessage();

            testEntity.Components.Add(component);
            testEntity.Components.Add<IComponentOne>(component);
            testEntity.Components.Add<IComponentTwo>(component);
            testEntity.Components.Add<IComponentThree>(component);

            testEntity.SendMessage(message);

            Assert.AreEqual(component.NumReceivedMessages, 1);
        }
    }
}
