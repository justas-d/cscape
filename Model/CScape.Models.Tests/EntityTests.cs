using System;
using CScape.Models.Extensions;
using CScape.Models.Tests.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CScape.Models.Tests
{
    [TestClass]
    public sealed class EntityTests
    {
        [TestMethod]
        public void SendMessageThrowsNullOnNullMessage()
        {
            var testEntityHandle = ModelImpl.Active.CreateEntity();
            var testEntity = testEntityHandle.Get();

            Assert.ThrowsException<ArgumentNullException>(() =>
            {
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
