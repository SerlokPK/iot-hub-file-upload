using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Transport;

namespace IoTFileUpload
{
	internal class FileUploadSample
	{
		private readonly DeviceClient _deviceClient;

		public FileUploadSample(DeviceClient deviceClient)
		{
			_deviceClient = deviceClient;
		}

		public async Task RunSampleAsync()
		{
			const string filePath = "TestPayload.txt";

			using var fileStreamSource = new FileStream(filePath, FileMode.Open);
			var fileName = Path.GetFileName(fileStreamSource.Name);

			Console.WriteLine($"Uploading file {fileName}");

			var fileUploadSasUriRequest = new FileUploadSasUriRequest
			{
				BlobName = $"synccontainer/{fileName}"
			};

			// Note: GetFileUploadSasUriAsync and CompleteFileUploadAsync will use HTTPS as protocol
			// regardless of the DeviceClient protocol selection.
			Console.WriteLine("Getting SAS URI from IoT Hub to use when uploading the file...");

			FileUploadSasUriResponse sasUri = await _deviceClient.GetFileUploadSasUriAsync(fileUploadSasUriRequest);
			Uri uploadUri = sasUri.GetBlobUri();


			Console.WriteLine($"Successfully got SAS URI ({uploadUri}) from IoT Hub");

			try
			{
				Console.WriteLine($"Uploading file {fileName} using the Azure Storage SDK and the retrieved SAS URI for authentication");

				var blockBlobClient = new BlockBlobClient(uploadUri);

				//var storageAccountName = "";
				//var containerName = "";
				//var blobName = $"5908764a8e382107d44e8e25_BOX0000311/{fileName}";
				//var blobEndpoint = new Uri($"https://{storageAccountName}.blob.core.windows.net");

				// works
				//var bsConnectionString = "";
				//var blobClient = new BlobClient(bsConnectionString, containerName, blobName);

				// works
				//var key = "";
				//var sharedKeyCred = new StorageSharedKeyCredential(storageAccountName, key);
				//var blobClient = new BlobClient(blobEndpoint, sharedKeyCred);

				// used with both examples
				//var sas = blobClient.GenerateSasUri(BlobSasPermissions.All, DateTimeOffset.Now.AddHours(1));


				//var blobClient = new BlobServiceClient(blobEndpoint, new DefaultAzureCredential());
				//UserDelegationKey key = await blobClient.GetUserDelegationKeyAsync(DateTimeOffset.UtcNow,
				//	DateTimeOffset.UtcNow.AddDays(7));
				//var sasBuilder = new BlobSasBuilder()
				//{
				//	BlobContainerName = containerName,
				//	BlobName = blobName,
				//	Resource = "b",
				//	ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
				//};
				//sasBuilder.SetPermissions(BlobSasPermissions.All);
				//string sasToken = sasBuilder.ToSasQueryParameters(key, storageAccountName).ToString();
				//UriBuilder fullUri = new UriBuilder()
				//{
				//	Scheme = "https",
				//	Host = blobEndpoint.OriginalString,
				//	Path = string.Format("{0}/{1}", containerName, blobName),
				//	Query = sasToken
				//};

				await blockBlobClient.UploadAsync(fileStreamSource, new BlobUploadOptions());
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Failed to upload file to Azure Storage using the Azure Storage SDK due to {ex}");

				var failedFileUploadCompletionNotification = new FileUploadCompletionNotification
				{
					// Mandatory. Must be the same value as the correlation id returned in the sas uri response
					CorrelationId = sasUri.CorrelationId,

					// Mandatory. Will be present when service client receives this file upload notification
					IsSuccess = false,

					// Optional, user-defined status code. Will be present when service client receives this file upload notification
					StatusCode = 500,

					// Optional, user defined status description. Will be present when service client receives this file upload notification
					StatusDescription = ex.Message
				};

				// Note that this is done even when the file upload fails. IoT Hub has a fixed number of SAS URIs allowed active
				// at any given time. Once you are done with the file upload, you should free your SAS URI so that other
				// SAS URIs can be generated. If a SAS URI is not freed through this API, then it will free itself eventually
				// based on how long SAS URIs are configured to live on your IoT Hub.
				await _deviceClient.CompleteFileUploadAsync(failedFileUploadCompletionNotification);
				Console.WriteLine("Notified IoT Hub that the file upload failed and that the SAS URI can be freed");

				return;
			}

			Console.WriteLine("Successfully uploaded the file to Azure Storage");

			var successfulFileUploadCompletionNotification = new FileUploadCompletionNotification
			{
				// Mandatory. Must be the same value as the correlation id returned in the sas uri response
				CorrelationId = sasUri.CorrelationId,

				// Mandatory. Will be present when service client receives this file upload notification
				IsSuccess = true,

				// Optional, user defined status code. Will be present when service client receives this file upload notification
				StatusCode = 200,

				// Optional, user-defined status description. Will be present when service client receives this file upload notification
				StatusDescription = "Success"
			};

			await _deviceClient.CompleteFileUploadAsync(successfulFileUploadCompletionNotification);
			Console.WriteLine("Notified IoT Hub that the file upload succeeded and that the SAS URI can be freed.");
		}
	}
}
