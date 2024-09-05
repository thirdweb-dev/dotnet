using System.Runtime.Serialization.Json;

namespace Thirdweb.EWS;

internal abstract class LocalStorageBase
{
    internal abstract LocalStorage.DataStorage Data { get; }

    internal abstract Task RemoveAuthTokenAsync();
    internal abstract Task SaveDataAsync(LocalStorage.DataStorage data);
}

internal partial class LocalStorage : LocalStorageBase
{
    internal override DataStorage Data => this._storage.Data;
    private readonly Storage _storage;
    private readonly string _filePath;

    internal LocalStorage(string clientId, string storageDirectoryPath)
    {
        if (string.IsNullOrEmpty(storageDirectoryPath))
        {
            throw new ArgumentException("Storage directory path is required", nameof(storageDirectoryPath));
        }
        _ = Directory.CreateDirectory(storageDirectoryPath);
        this._filePath = Path.Combine(storageDirectoryPath, $"{clientId}.txt");
        try
        {
            var json = File.ReadAllBytes(this._filePath);
            DataContractJsonSerializer serializer = new(typeof(Storage));
            MemoryStream fin = new(json);
            this._storage = (Storage)serializer.ReadObject(fin);
        }
        catch (Exception)
        {
            this._storage = new Storage();
        }
    }

    internal override Task RemoveAuthTokenAsync()
    {
        return this.UpdateDataAsync(() =>
        {
            if (this._storage.Data?.AuthToken != null)
            {
                this._storage.Data.ClearAuthToken();
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
            serializer.WriteObject(fout, this._storage);
            await File.WriteAllBytesAsync(this._filePath, fout.ToArray()).ConfigureAwait(false);
            return true;
        }
        return false;
    }

    internal override Task SaveDataAsync(DataStorage data)
    {
        return this.UpdateDataAsync(() =>
        {
            this._storage.Data = data;
            return true;
        });
    }
}
