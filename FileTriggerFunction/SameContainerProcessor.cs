using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace FileTriggerFunction
{
    public class SameContainerProcessor
    {
        [FunctionName("SameContainerProcessor")]
        public async Task Run([BlobTrigger("synccontainer/usage/{name}", Connection = "BlobStorageConnectionString")]
	        Stream myBlob,
	        string name,
	        ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
            await Task.FromResult(1);
		}
    }
}
