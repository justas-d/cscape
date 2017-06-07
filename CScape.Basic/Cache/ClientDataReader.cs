using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CScape.Core.Data;
using ICSharpCode.SharpZipLib.BZip2;
using JetBrains.Annotations;

namespace CScape.Basic.Cache
{
    /*
    *  n Index files -> data file
    *  
    *  Index files
    *      Index files allow lookup of data by its (type; file) pair.
    *      
    *      The type number tells us which index file we need to open.
    *      The file number tells us the offset of the data indices inside the Index file.
    *      
    *  Data indices:
    *      Size  - The size of the data chunks. The size can span multiple data chunks. The size does not include header sizes. If the size does not align with the data chunk size, the unfinished data chunk must be read.
    *      Block - The index of the first data block inside the data file.
    *      
    *      Both of these values are 3 bytes in size, making the whole index 6 bytes in size.
    *      
    *  Data block
    *      Consists of a data header followed by a data chunk.
    *  
    *  Data header
    *      8 bytes in size
    *      
    *      Next File
    *      Current Chunk Index
    *      Next Block Index
    *      Next Type
    *      
    *      
    *  Data chunk
    *      512 bytes in size
    *      
    */

    public class ClientDataReader : IDisposable
    {
        public class Folder
        {
            private readonly Dictionary<int, byte[]> _files;

            public Folder(byte[] data)
            {
                var blob = new Blob(data);

                var folderDecompressedSize = blob.ReadInt24();
                var folderSize = blob.ReadInt24();

                byte[] Decompress(byte[] compressed, int decompressedSize, int offset)
                {

                    var ret = new byte[decompressedSize];

                    using (var input = new MemoryStream(compressed))
                    {
                        input.Position = offset;
                        using (BZip2InputStream bzip = new BZip2InputStream(input))
                            bzip.Read(ret, 0, decompressedSize);
                    }
                    return ret;
                }

                // decompress bzip
                var isEverythingDecompressed = false;
                if (folderSize != folderDecompressedSize)
                {
                    blob = new Blob(Decompress(data, folderDecompressedSize, blob.ReadCaret));
                    isEverythingDecompressed = true;
                }
                    

                var fileCount = blob.ReadInt16();
                _files = new Dictionary<int, byte[]>(fileCount);

                var ids = new int[fileCount];
                var decompressedFileSizes = new int[fileCount];
                var fileSizes = new int[fileCount];
                for (var i = 0; i < fileCount; i++)
                {
                    ids[i] = blob.ReadInt32();
                    decompressedFileSizes[i] = blob.ReadInt24();
                    fileSizes[i] = blob.ReadInt24();
                }

                byte[] ReadNextFile(int size)
                {
                    var fileData = new byte[size];
                    blob.ReadBlock(fileData, 0, size);
                    return fileData;
                }

                for (var i = 0; i < fileCount; i++)
                {
                    if (isEverythingDecompressed)
                    {
                        // whole folder was compressed
                        _files.Add(ids[i], ReadNextFile(decompressedFileSizes[i]));
                    }
                    else
                    {
                        // decompress individual file
                        var compressedFile = ReadNextFile(fileSizes[i]);
                        var file = Decompress(compressedFile, decompressedFileSizes[i], 0);
                        _files.Add(ids[i], file);
                    }
                }
            }

            [CanBeNull]
            public byte[] GetFile(string id)
            {
                // hash id
                id = id.ToUpperInvariant();
                var hash = id.Aggregate(0, (current, c) => current * 61 + c - 32);
                return GetFile(hash);
            }

            [CanBeNull]
            public byte[] GetFile(int id)
            {
                if (!_files.ContainsKey(id))
                    return null;
                return _files[id];
            }
        }

        public const string IndexFileBase = "main_file_cache.idx{0}";
        public const string DataFileName = "main_file_cache.dat";

        public const int IndexSize = 6;
        public const int BlockDataSize = 512;
        public const int BlockHeaderSize = 8;
        public const int BlockSize = BlockDataSize + BlockHeaderSize;

        private readonly string _pathToData;

        // stream caches
        private  Dictionary<(int type, int file), Folder> _folderCache 
            = new Dictionary<(int type, int file), Folder>();

        private Dictionary<int, FileStream> _indexCache 
            = new Dictionary<int, FileStream>();

        private FileStream _data;

        public bool IsDisposed { get; private set; }

        public ClientDataReader(string pathToData)
        {
            _pathToData = pathToData;

            var dataFilePath = Path.Combine(_pathToData, DataFileName);
            if (!File.Exists(dataFilePath))
                throw new FileNotFoundException($"Could not find client data file in the expected path: {dataFilePath}");

            _data = File.OpenRead(dataFilePath);
        }

        private void ThrowIfDisposed()
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().Name);
        }

        private FileStream GetIndex(int type)
        {
            ThrowIfDisposed();

            var idxFileName = string.Format(IndexFileBase, type);
            var idxFilePath = Path.Combine(_pathToData, idxFileName);

            if (!File.Exists(idxFilePath))
                throw new FileNotFoundException($"Client data index file {idxFileName} for type {type} was  not found in the expected path: {idxFilePath}", idxFileName );

            if (!_indexCache.ContainsKey(type))
                _indexCache.Add(type, File.OpenRead(idxFilePath));

            // return from cache
            var ret = _indexCache[type];
            ret.Position = 0;
            return ret;
        }

        public Folder GetFolder(int type, int 
            file)
        {
            ThrowIfDisposed();
            var key = (type, file);

            if (_folderCache.ContainsKey(key))
                return _folderCache[key];
                
            // load file
            var index = GetIndex(type);
            index.Position = IndexSize * file;

            var header = new byte[BlockHeaderSize];
            index.Read(header, 0, IndexSize);

            int Read16(int off)
            {
                return (header[0 + off] << 8) + header[1 + off];
            }

            int Read24(int off)
            {
                return (header[0 + off] << 16) + (header[1 + off] << 8) + header[2 + off];
            }

            var size = (header[0] << 16) + (header[1] << 8) + header[2];
            var blockIndex = (header[3] << 16) + (header[4] << 8) + header[5];

            Debug.Assert(_data.Length > size);
            Debug.Assert(blockIndex > 0);
            Debug.Assert(_data.Length / BlockSize > blockIndex);

            var blockCount = size / BlockDataSize;
            var incompleteBlockByteCount = size % BlockDataSize;

            if (incompleteBlockByteCount != 0)
                blockCount++;

            var folderData = new byte[size];
            var read = 0;

            for (var i = 0; i < blockCount; i++)
            {
                _data.Seek(blockIndex * BlockSize, SeekOrigin.Begin);

                // read header
                _data.Read(header, 0, BlockHeaderSize);

                var nextFile = Read16(0);
                var currentDataIndex = Read16(2);
                blockIndex = Read24(4);
                var nextType = header[7];

                // match header data to current state
                if (i != currentDataIndex)
                    throw new CacheDataBlockReadFailureException(
                        $"Unexpected data index read from data block header.", currentDataIndex, i, i);


                // flush data
                if (read + BlockDataSize >= size)
                {
                    // special case for writing the last incomplete 512 byte block.
                    _data.Read(folderData, read, incompleteBlockByteCount);
                    read += incompleteBlockByteCount;
                }
                else
                {
                    _data.Read(folderData, read, BlockDataSize);
                    read += BlockDataSize;
                }

                // we still have to read, make sure the header of the upcoming block matches our current state.
                if (size > read)
                {
                    if (nextFile != file)
                        throw new CacheDataBlockReadFailureException(
                            $"Upcoming file type specified by the current data block header mismatches expected file type. File size: {size} read so far: {read}",
                            nextFile, file, i);

                    if (nextType != type + 1)
                        throw new CacheDataBlockReadFailureException(
                            $"Upcoming block type specified by the current data block header mismatches expected data type. File size: {size} read so far: {read}",
                            nextType, type + 1, i);
                }
            }

            var folder = new Folder(folderData);
            _folderCache.Add(key, folder);
            return folder;
        }

        public void Dispose()
        {
            if (IsDisposed) return;

            IsDisposed = true;

            _data?.Dispose();
            foreach (var file in _indexCache.Values)
                file?.Dispose();

            _indexCache = null;
            _folderCache = null;
            _data = null;
            
        }
    }
}
