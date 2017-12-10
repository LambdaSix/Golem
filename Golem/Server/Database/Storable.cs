using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace Golem.Server.Database
{
    public interface IStorable
    {
        string Key { get; }
    }

    public interface IDatabase : IDisposable
    {
        Dictionary<Type, Dictionary<string, object>> Data { get; }
        string GetStorageLocationFor(Type type, string key);
        string GetTypeRoot(Type type);
        Dictionary<string, object> GetOrCreateItemStore(Type itemType);
        void Put<T>(T item) where T : class, IStorable;
        T Get<T>(string key) where T : class, IStorable;
        void Delete<T>(string key) where T : class, IStorable;
        IEnumerable<T> GetAll<T>() where T : class, IStorable;

        /// <summary>
        /// Write the entire database to disk
        /// </summary>
        void SaveDatabase();

        /// <summary>
        /// Put an item into the database, and write to disk.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        void Save<T>(T item) where T : class, IStorable;

        void Dispose();
        bool Exists<T>(string key) where T : class, IStorable;
    }

    public class Database : IDatabase
    {
        private readonly string storagePath = "database\\";
        private readonly ReaderWriterLockSlim readWriteLock = new ReaderWriterLockSlim();

        public Dictionary<Type, Dictionary<string, object>> Data { get; } =
            new Dictionary<Type, Dictionary<string, object>>();

        public string GetStorageLocationFor(Type type, string key) => Path.Combine(storagePath, type.Name, key + ".db");

        public string GetTypeRoot(Type type) => Path.Combine(storagePath, type.Name);

        public Dictionary<string, object> GetOrCreateItemStore(Type itemType)
        {
            if (!Data.ContainsKey(itemType))
                Data[itemType] = new Dictionary<string, object>();

            return Data[itemType];
        }

        public void Put<T>(T item) where T : class, IStorable
        {
            readWriteLock.EnterReadLock();
            try
            {
                var keyToItemMap = GetOrCreateItemStore(typeof(T));

                keyToItemMap[item.Key] = item;
            }
            finally
            {
                readWriteLock.ExitReadLock();
            }
        }

        private T LoadFile<T>(string fileName)
        {
            string contents = File.ReadAllText(fileName);

            return JsonConvert.DeserializeObject<T>(contents);
        }

        private T LoadFromDisk<T>(string key) where T : class, IStorable
        {
            readWriteLock.EnterWriteLock();
            try
            {
                var keyToItemMap = GetOrCreateItemStore(typeof(T));

                var storagePath = GetStorageLocationFor(typeof(T), key);

                if (!File.Exists(storagePath))
                {
                    return null;
                }

                var loaded = LoadFile<T>(storagePath);

                keyToItemMap[key] = loaded;

                return loaded;
            }
            finally
            {
                readWriteLock.ExitWriteLock();
            }
        }

        public T Get<T>(string key) where T : class, IStorable
        {
            if (key == null)
                key = "(null)";

            readWriteLock.EnterUpgradeableReadLock();
            try
            {
                var keyToItemMap = GetOrCreateItemStore(typeof(T));

                if (keyToItemMap.ContainsKey(key))
                    return (T)keyToItemMap[key];

                return LoadFromDisk<T>(key);
            }
            finally
            {
                readWriteLock.ExitUpgradeableReadLock();
            }
        }

        public void Delete<T>(string key) where T : class, IStorable
        {
            readWriteLock.EnterWriteLock();

            try
            {
                var keyToItemMap = GetOrCreateItemStore(typeof(T));

                if (keyToItemMap.ContainsKey(key))
                    keyToItemMap.Remove(key);

                var storageLocation = GetStorageLocationFor(typeof(T), key);

                if (File.Exists(storageLocation))
                    File.Delete(storageLocation);
            }
            finally
            {
                readWriteLock.ExitWriteLock();
            }
        }

        public IEnumerable<T> GetAll<T>() where T : class, IStorable
        {
            string typeRoot = GetTypeRoot(typeof(T));

            if (!Directory.Exists(typeRoot))
                yield break;

            foreach (var file in Directory.EnumerateFiles(typeRoot, "*.db", SearchOption.AllDirectories))
            {
                var item = LoadFile<T>(file);

                yield return Get<T>(item.Key);
            }
        }

        private void SaveItemToFile(Type type, string key, object item)
        {
            string filePath = GetStorageLocationFor(type, key);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            string json = JsonConvert.SerializeObject(item, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// Write the entire database to disk
        /// </summary>
        public void SaveDatabase()
        {
            readWriteLock.ExitReadLock();
            try
            {
                lock (Data)
                {
                    foreach (var typeToDataMap in Data)
                    {
                        var keyToItemMap = Data[typeToDataMap.Key];

                        foreach (var item in keyToItemMap)
                        {
                            SaveItemToFile(typeToDataMap.Key, item.Key, item.Value);
                        }
                    }
                }
            }
            finally
            {
                readWriteLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Put an item into the database, and write to disk.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        public void Save<T>(T item) where T : class, IStorable
        {
            Put(item);
            SaveItemToFile(typeof(T), item.Key, item);
        }

        public void Dispose()
        {
            readWriteLock.Dispose();
        }

        public bool Exists<T>(string key) where T : class, IStorable
        {
            return Get<T>(key) != null;
        }
    }
}