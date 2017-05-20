using System;
using System.Collections.Generic;
using System.Linq;
using CScape.Basic.Model;
using CScape.Core.Game.Item;
using CScape.Core.Injection;

namespace CScape.Dev.Tests.Internal.Impl
{
    public class MockItemDb : IItemDefinitionDatabase
    {
        public const int UndefinedId = 10000; // todo : test UndefinedId 
        public const int EquipmentId = 10001; // todo : test EquipmentId  
        public const int IdMismatch = 10002; // todo : test IdMismatch
        public const int OutOfRangeAmount = 10003; // todo : test OutOfRangeAmount 

        private Queue<MockItem> _itemQueue = new Queue<MockItem>();

        public void PushToQueue(MockItem item) => _itemQueue.Enqueue(item);

        public IItemDefinition Get(int id)
        {
            switch (id)
            {
                case UndefinedId: return null;
                case EquipmentId:
                    throw
                        new NotImplementedException(); /*return new MockEquippable(id, "asdf", int.MaxValue, true, 1, false, -1, EquipSlotType.Head, null, null,0,0,0,0,null);*/
                case IdMismatch: throw new NotImplementedException();
                case OutOfRangeAmount: throw new NotImplementedException();
                default:
                    if (_itemQueue.Any())
                        return _itemQueue.Dequeue();

                    return new MockItem(id, "Mock item", int.MaxValue, true, 1, false, -1);
            }
        }
    }
}