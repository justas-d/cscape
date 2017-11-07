using System;
using CScape.Dev.Tests.ModelTests.Mock;
using CScape.Models.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CScape.Dev.Tests.ModelTests
{
    [TestClass]
    public sealed class ConfigurationServiceTests
    {
        private static void AssertKeyExists(IConfigurationService service, string key)
        {
            var doesntExist = service.Get(key) == null;
            Assert.IsFalse(doesntExist);
        }

        private void AssertKeyDoesNotExist(IConfigurationService service, string key)
        {
            var doesntExist = service.Get(key) == null;
            Assert.IsTrue(doesntExist);
        }

        [TestMethod]
        public void AddReturnsTrueIfKeyDidntExistBeforehand()
        {
            var cfg = ModelImpl.Active.GetConfig();
            var key = "test-key";
            var value = "test-value";

            AssertKeyDoesNotExist(cfg, key);

            var didAddSuccessfully = cfg.Add(key, value);
            Assert.IsTrue(didAddSuccessfully);
        }

        [TestMethod]
        public void AddReturnsFalseIfKeyExists()
        {
            var cfg = ModelImpl.Active.GetConfig();
            var key = "test-key";
            var value = "test-value";

            AssertKeyDoesNotExist(cfg, key);

            cfg.Add(key, value);

            var didAddSuccessfully = cfg.Add(key, value);
            Assert.IsFalse(didAddSuccessfully);
        }

        [TestMethod]
        public void AddFailsIfKeyIsNull()
        {
            var didFail = false;

            var cfg = ModelImpl.Active.GetConfig();
            string key = null;
            var value = "test value";

            try
            {
                if (!cfg.Add(key, value))
                {
                    // if we did fail to add, assert that the key doesn't exist.
                    AssertKeyDoesNotExist(cfg, key);
                    didFail = true;
                }
            }
            catch (Exception e)
            {
                didFail = true;
            }

            Assert.IsTrue(didFail);
        }

        [TestMethod]
        public void GetFailsIfKeyIsNull()
        {
            var didFail = false;

            var cfg = ModelImpl.Active.GetConfig();
            string key = null;

            try
            {
                var value = cfg.Get(key);
                if (value == null)
                    didFail = true;
            }
            catch (Exception e)
            {
                didFail = true;
            }

            Assert.IsTrue(didFail);
        }


        [TestMethod]
        public void CanGetAfterAdding()
        {
            var cfg = ModelImpl.Active.GetConfig();
            var key = "test key";
            var value = "test value";

            cfg.Add(key, value);

            Assert.IsTrue(cfg.Get(key).Equals(value, StringComparison.Ordinal));
        }

        [TestMethod]
        public void GetReturnsNullIfKeyDoesntExist()
        {
            var cfg = ModelImpl.Active.GetConfig();
            var key = "test key";

            Assert.IsNull(cfg.Get(key));
        }


    }
}