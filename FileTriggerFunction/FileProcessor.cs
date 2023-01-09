using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace FileTriggerFunction
{
    public class FileProcessor
    {
        [FunctionName(nameof(FileProcessor))]
        public async Task Run([BlobTrigger("synccontainer/{deviceId}/usage/{name}", Connection = "BlobStorageConnectionString")]
	        Stream blob,
	        string deviceId,
            string name,
	        ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{deviceId} \n ");
            await Task.FromResult(1);
        }
    }
}
