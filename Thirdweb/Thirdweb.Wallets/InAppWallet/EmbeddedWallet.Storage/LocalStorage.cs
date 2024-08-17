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
    internal override DataStorage Data => this.storage.Data;
    private readonly Storage storage;
    private readonly string filePath;

    internal LocalStorage(string clientId, string storageDirectoryPath = null)
    {
        string directory;
        directory = storageDirectoryPath ?? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        directory = Path.Combine(directory, "Thirdweb", "InAppWallet");
        _ = Directory.CreateDirectory(directory);
        this.filePath = Path.Combine(directory, $"{clientId}.txt");
        try
        {
            var json = File.ReadAllBytes(this.filePath);
            DataContractJsonSerializer serializer = new(typeof(Storage));
            MemoryStream fin = new(json);
            this.storage = (Storage)serializer.ReadObject(fin);
        }
        catch (Exception)
        {
            this.storage = new Storage();
        }
    }

    internal override Task RemoveAuthTokenAsync()
    {
        return this.UpdateDataAsync(() =>
        {
            if (this.storage.Data?.AuthToken != null)
            {
                this.storage.Data.ClearAuthToken();
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
            serializer.WriteObject(fout, this.storage);
            await File.WriteAllBytesAsync(this.filePath, fout.ToArray()).ConfigureAwait(false);
            return true;
        }
        return false;
    }

    internal override Task SaveDataAsync(DataStorage data)
    {
        return this.UpdateDataAsync(() =>
        {
            this.storage.Data = data;
            return true;
        });
    }
}
