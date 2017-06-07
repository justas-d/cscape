using System;
using JetBrains.Annotations;

namespace CScape.Basic.Cache
{
    public sealed class ClientItemDefinition
    {
        
    }

    /// <summary>
    /// Implements a lazy item definition parser that piggybacks off of 317 cache.
    /// </summary>
    public sealed class ItemConfigParser
    {
        public ClientDataReader Data { get; }

        public ItemConfigParser([NotNull] ClientDataReader data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        [CanBeNull]
        public ClientItemDefinition GetById(int id)
        {
            throw new NotImplementedException();
        }
    }
}
