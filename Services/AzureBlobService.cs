using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BackupMonitor.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace BackupMonitor.Services;

public class AzureBlobService
{
    private readonly BlobContainerClient _container;
    private AppConfig _config = new();

    public AzureBlobService(AppConfig cfg)
    {
        _config = cfg;
        _container = new BlobContainerClient(cfg.AzureConnectionString, cfg.AzureContainer);
        _container.CreateIfNotExists();
    }

    public AzureBlobService()
    {
        
    }

    public void Upload(string filePath, string blobName = "")
    {
        if (string.IsNullOrEmpty(blobName))
            blobName = Path.GetFileName(filePath);

        var service = new BlobServiceClient(_config.AzureConnectionString);
        var container = service.GetBlobContainerClient(_config.AzureContainer);

        // Cria container se não existir
        if (!container.Exists())
            container.Create();

        var blob = container.GetBlobClient(blobName);

        using (var fileStream = File.OpenRead(filePath))
        {
            blob.Upload(fileStream, overwrite: true);
        }
    }

    public bool TestarConexao()
    {
        try
        {
            var service = new BlobServiceClient(_config.AzureConnectionString);
            // Tenta listar os containers
            var containers = service.GetBlobContainers();
            foreach (var c in containers)
            {
                break;
            }
            return true; //Conexão bem-sucedida
        }
        catch (Exception ex)
        {
            return false; //Falha na conexão
        }
    }
    
    public async Task<List<string>> ListarBlobsAsync()
    {
        var lista = new List<string>();
        await foreach (BlobItem blob in _container.GetBlobsAsync())
        {
            lista.Add(blob.Name);
        }
        return lista;
    }

    public async Task DownloadAsync(string blobName, string destinoLocal)
    {
        var blob = _container.GetBlobClient(blobName);
        using var fs = File.OpenWrite(destinoLocal);
        await blob.DownloadToAsync(fs);
    }

}