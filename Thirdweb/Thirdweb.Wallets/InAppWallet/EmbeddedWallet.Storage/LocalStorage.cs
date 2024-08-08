using System.Runtime.Serialization.Json;

namespace Thirdweb.EWS
{
    internal abstract class LocalStorageBase
    {
        internal abstract LocalStorage.DataStorage Data { get; }
        internal abstract LocalStorage.SessionStorage Session { get; }

        internal abstract Task RemoveAuthTokenAsync();
        internal abstract Task RemoveSessionAsync();
        internal abstract Task SaveDataAsync(LocalStorage.DataStorage data);
        internal abstract Task SaveSessionAsync(string sessionId);
    }

    internal partial class LocalStorage : LocalStorageBase
    {
        internal override DataStorage Data => storage.Data;
        internal override SessionStorage Session => storage.Session;
        private readonly Storage storage;
        private readonly string filePath;

        internal LocalStorage(string clientId, string storageDirectoryPath = null)
        {
            string directory;
            directory = storageDirectoryPath ?? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            directory = Path.Combine(directory, "EWS");
            Directory.CreateDirectory(directory);
            filePath = Path.Combine(directory, $"{clientId}.txt");
            try
            {
                byte[] json = File.ReadAllBytes(filePath);
                DataContractJsonSerializer serializer = new(typeof(Storage));
                MemoryStream fin = new(json);
                storage = (Storage)serializer.ReadObject(fin);
            }
            catch (Exception)
            {
                storage = new Storage();
            }
        }

        internal override Task RemoveAuthTokenAsync()
        {
            return UpdateDataAsync(() =>
            {
                if (storage.Data?.AuthToken != null)
                {
                    storage.Data.ClearAuthToken();
                    return true;
                }
                return false;
            });
        }

        private async Task<bool> UpdateDataAsync(Func<bool> fn)
        {
            if (fn())
            {
                DataContractJsonSerializer serializer = new(typeof(Storage));
                MemoryStream fout = new();
                serializer.WriteObject(fout, storage);
                await File.WriteAllBytesAsync(filePath, fout.ToArray()).ConfigureAwait(false);
                return true;
            }
            return false;
        }

        internal override Task SaveDataAsync(DataStorage data)
        {
            return UpdateDataAsync(() =>
            {
                storage.Data = data;
                return true;
            });
        }

        internal override Task SaveSessionAsync(string sessionId)
        {
            return UpdateDataAsync(() =>
            {
                storage.Session = new SessionStorage(sessionId);
                return true;
            });
        }

        internal override Task RemoveSessionAsync()
        {
            return UpdateDataAsync(() =>
            {
                storage.Session = null;
                return true;
            });
        }
    }
}
