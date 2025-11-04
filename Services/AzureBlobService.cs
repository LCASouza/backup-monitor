using Azure.Storage.Blobs;
using System;
using System.IO;

namespace BackupMonitor.Services;

public class AzureBlobService
{
    private const string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=tcc2lucassouza;AccountKey=jcGWa32uq3wJVNsZ+cZI5fnu9t4ha+Xs45whjmsIyN0ozb5iNbzeLwDmIYAh536jlWz0al5k15UB+AStbvcfig==;EndpointSuffix=core.windows.net";
    private const string ContainerName = "financas";

    public void Upload(string filePath, string blobName = null)
    {
        if (string.IsNullOrEmpty(blobName))
            blobName = Path.GetFileName(filePath);

        var service = new BlobServiceClient(ConnectionString);
        var container = service.GetBlobContainerClient(ContainerName);

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
            var service = new BlobServiceClient(ConnectionString);
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

}