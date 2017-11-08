using System;
using System.Collections.Generic;
using CScape.Dev.Tests.Mock;
using CScape.Models.Extensions;
using CScape.Models.Game.Entity;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CScape.Dev.Tests
{
    [TestClass]
    public class EntityComponentContainerTests
    {
        private static (IEntity Entity, MultiInterfaceComponent Component) GetTestData()
        {
            var entity = ModelImpl.Active.CreateEntity().Get();
            var component = new MultiInterfaceComponent(entity);
            return (entity, component);
        }

        private static void AssertContainsComponent(IEntityComponentContainer components, Type type)
        {
            Assert.IsTrue(components.Contains(type));
        }

        private static void AssertDoesNotContainComponent(IEntityComponentContainer components, Type type)
        {
            Assert.IsFalse(components.Contains(type));
        }

        private static void RunContainerTest(
            Action<(IEntity Entity, MultiInterfaceComponent Component)> test,
            Action<(IEntity Entity, MultiInterfaceComponent Component)> assert)
        {
            var data = GetTestData();
            test(data);
            assert(data);
        }

        private static void RunContainerTestCarryReturn<TResult>(
            Func<(IEntity Entity, MultiInterfaceComponent Component), TResult> test,
            Action<(IEntity Entity, MultiInterfaceComponent Component, TResult ReturnValue)> assert)
        {
            var data = GetTestData();
            var result = test(data);
            assert((data.Entity, data.Component, result));
        }

        [TestMethod]
        public void DoesContainsWorksOnPurelyMappedComponents()
        {
            RunContainerTest(
                data => data.Entity.Components.Add(data.Component.GetType(), data.Component),
                data => AssertContainsComponent(data.Entity.Components, data.Component.GetType()));
        }

        [TestMethod]
        public void DoesContainsWorksOnMappedComponents()
        {
            RunContainerTest(
                data => data.Entity.Components.Add(typeof(IComponentOne), data.Component),
                data => AssertContainsComponent(data.Entity.Components, typeof(IComponentOne)));            
        }

        [TestMethod]
        public void MappedTypeMustNotMapToPureType()
        {
            RunContainerTest(
                data => data.Entity.Components.Add(typeof(IComponentOne), data.Component),
                data => AssertDoesNotContainComponent(data.Entity.Components, typeof(MultiInterfaceComponent)));
        }

        [TestMethod]
        public void RemoveNonexistantComponentReturnsFalse()
        {
            RunContainerTestCarryReturn(
                data => data.Entity.Components.Remove(data.Component.GetType()),
                data => Assert.IsFalse(data.ReturnValue));
        }

        private static bool AddAndRemove(
            IEntityComponentContainer container,
            IEntityComponent component,
            Type addType,
            Type removeType = null)
        {
            if (removeType == null)
                removeType = addType;

            var addResult = container.Add(addType, component);
            Assert.IsTrue(addResult);

            return container.Remove(removeType);
        }

        [TestMethod]
        public void RemoveExistantPurelyMappedComponentReturnsTrue()
        {
            RunContainerTestCarryReturn(
                data => AddAndRemove(data.Entity.Components, data.Component, data.Component.GetType()),
                data => Assert.IsTrue(data.ReturnValue));
        }

        [TestMethod]
        public void RemoveExistantMappedComponentReturnsTrue()
        {
            RunContainerTestCarryReturn(
                data => AddAndRemove(data.Entity.Components, data.Component, typeof(IComponentOne)),
                data => Assert.IsTrue(data.ReturnValue));
        }

        [TestMethod]
        public void RemoveMappedComponentByPureReferenceReturnsFalse()
        {
            RunContainerTestCarryReturn(
                data => AddAndRemove(data.Entity.Components, data.Component, typeof(IComponentOne), data.Component.GetType()),
                data => Assert.IsFalse(data.ReturnValue));
        }

        [TestMethod]
        public void RemovePureMappedComponentByMappedReferenceReturnsFalse()
        {
            RunContainerTestCarryReturn(
                data => AddAndRemove(data.Entity.Components, data.Component, data.Component.GetType(), typeof(IComponentOne)),
                data => Assert.IsFalse(data.ReturnValue));
        }
        
        private static void TestIterationConcurrency(
            Func<IEntityComponentContainer, IEnumerable<IEntityComponent>> enumerableFactory,
            Action<IEntity> setup,
            Func<IEntityComponentContainer, bool> manipulator,
            Action<IEntityComponentContainer> test)
        {

            var entity = ModelImpl.Active.CreateEntity().Get();
            var component = new PureComponent(entity);
            Assert.IsTrue(entity.Components.Add(component));
            
            var enumerable = enumerableFactory(entity.Components);
            setup(entity);
  
            var didAdd = false;
            foreach (var _ in enumerable)
            {
                if (!didAdd)
                {
                    Assert.IsTrue(manipulator(entity.Components));
                    didAdd = true;
                }
            }
                

            test(entity.Components);
        }

        private static void TestIterationConcurrencyAdd(
            Func<IEntityComponentContainer, IEnumerable<IEntityComponent>> enumerableFactory)
        {
            MultiInterfaceComponent component = null;
            TestIterationConcurrency(
                enumerableFactory,
                setup: (ent) =>
                {
                    component = new MultiInterfaceComponent(ent);
                },
                manipulator: (all) => all.Add(component),
                test: (all) => AssertContainsComponent(all, component.GetType()));
        }

        private static void TestIterationConcurrencyRemove(
            Func<IEntityComponentContainer, IEnumerable<IEntityComponent>> enumerableFactory)
        {
            MultiInterfaceComponent component = null;
            TestIterationConcurrency(
                enumerableFactory,
                setup: (ent) =>
                {
                    component = new MultiInterfaceComponent(ent);
                    ent.Components.Add(component);
                },
                manipulator: (all) => all.Remove(component.GetType()),
                test: (all) => AssertDoesNotContainComponent(all, component.GetType()));
        }

        [TestMethod]
        public void FailOnAddingIEntityComponentMappedComponent()
        {
            var ent = ModelImpl.Active.CreateEntity().Get();
            var component = new PureComponent(ent);

            var didFail = false;
            try
            {
                didFail = !ent.Components.Add(typeof(IEntityComponent), component);
                
            }
            catch (Exception e)
            {
                didFail = true;
            }

            Assert.IsTrue(didFail);
        }

        [TestMethod]
        public void CanAddComponentsDuringComponentIterationUsingGetSorted()
        {
            TestIterationConcurrencyAdd(c => c.GetSorted());
        }

        [TestMethod]
        public void CanRemoveComponentsDuringComponentIterationUsingGetSorted()
        {
            TestIterationConcurrencyRemove(c => c.GetSorted());
        }

        [TestMethod]
        public void CanAddComponentsDuringComponentIterationUsingLookup()
        {
            TestIterationConcurrencyAdd(c => c.Lookup.Values);
        }

        [TestMethod]
        public void CanRemoveComponentsDuringComponentIterationUsingLookup()
        {
            TestIterationConcurrencyRemove(c => c.Lookup.Values);
        }
    }
}
