using System;
using System.Collections.Generic;
using CScape.Core.Data;
using CScape.Core.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Basic.Cache
{
    public class MapIndexParseFailureException : Exception
    {
        public int EntryIndex { get; }
        public short PackedPosition { get; }
        public override string Message { get; }

        public MapIndexParseFailureException(int entryIndex, short packedPosition, string message)
        {
            EntryIndex = entryIndex;
            PackedPosition = packedPosition;
            Message = message;
        }
    }

    // map: 64*64 tiles
    public class MapIndex
    {
        public int PackedPosition { get; }
        public int ObjectMapIdx { get; }
        public int TileMapIdx { get; }
        public bool IsMembers { get; }

        public MapIndex(int packedPosition, int objectMapIdx, int tileMapIdx, bool isMembers)
        {
            PackedPosition = packedPosition;
            ObjectMapIdx = objectMapIdx;
            TileMapIdx = tileMapIdx;
            IsMembers = isMembers;
        }
    }

    public sealed class MapIndexParser
    {
        public ClientDataReader Data { get; }

        private readonly Dictionary<int, MapIndex> _cache;

        public MapIndexParser([NotNull] ClientDataReader data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            
            // read into cache
            var file = new Blob(data.GetFolder(0, 5).GetFile("map_index"));

            var len = file.Buffer.Length / 7;
            _cache = new Dictionary<int, MapIndex>(len);

            for (int i = 0; i < len; i++)
            {
                var packedPos = file.ReadInt16();
                var tileMap = file.ReadInt16();
                var objMap = file.ReadInt16();
                var isMemb = file.ReadByte();


                if (_cache.ContainsKey(packedPos))
                    throw new MapIndexParseFailureException(i, packedPos, $"Cache already contains position packed: {packedPos} entry: {i}");

                _cache[packedPos] = new MapIndex(packedPos, objMap, tileMap, isMemb != 0);
            }
        }

        [CanBeNull]
        public MapIndex GetByPosition(IPosition pos)
        {
            // translate world coords to 64x64 region coords
            //  (regionX << 8) + regionY;
            var key = ((pos.X >> 3) << 8) | (pos.X >> 3);

            if (!_cache.ContainsKey(key))
                return null;

            return _cache[key];
        }
    }
}
