using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace proj2;

public class AzureStorageService
{
    private readonly ILogger<AzureStorageService> _logger;
    private readonly IConfiguration _configuration;
    private readonly ShareServiceClient _serviceClient;
    private readonly QueueServiceClient _queueServiceClient;

    public AzureStorageService(ILogger<AzureStorageService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _serviceClient = new ShareServiceClient(_configuration["Prometric-StorageAccountConnString"]);
        _queueServiceClient = new QueueServiceClient(_configuration["Prometric-StorageAccountConnString"]);
    }

    public async Task<List<string>> GetFileSharesAndFilesAsync()
    {
        var results = new List<string>();

        await foreach (var shareItem in _serviceClient.GetSharesAsync())
        {
            _logger.LogInformation("Share: {ShareName}", shareItem.Name);
            results.Add($"[Share] {shareItem.Name}");

            var shareClient = _serviceClient.GetShareClient(shareItem.Name);
            var directoryClient = shareClient.GetRootDirectoryClient();

            await foreach (var fileItem in directoryClient.GetFilesAndDirectoriesAsync())
            {
                var type = fileItem.IsDirectory ? "Directory" : "File";
                _logger.LogInformation("  {Type}: {Name}", type, fileItem.Name);
                results.Add($"  [{type}] {fileItem.Name}");
            }
        }

        return results;
    }

    public async Task<List<string>> GetQueueMessagesAsync()
    {
        var results = new List<string>();

        await foreach (var queueItem in _queueServiceClient.GetQueuesAsync())
        {
            _logger.LogInformation("Queue: {QueueName}", queueItem.Name);
            results.Add($"[Queue] {queueItem.Name}");

            var queueClient = _queueServiceClient.GetQueueClient(queueItem.Name);
            PeekedMessage[] messages = await queueClient.PeekMessagesAsync(maxMessages: 10);

            foreach (var message in messages)
            {
                _logger.LogInformation("  Message: {MessageId} - {Body}", message.MessageId, message.Body);
                results.Add($"  [Message] {message.MessageId}: {message.Body}");
            }
        }

        return results;
    }
}
