using IoTFileUpload;
using Microsoft.Azure.Devices.Client;

const string connectionString = "";

using var deviceClient = DeviceClient.CreateFromConnectionString(
	connectionString,
	TransportType.Mqtt);
var sample = new FileUploadSample(deviceClient);
await sample.RunSampleAsync();

await deviceClient.CloseAsync();

Console.WriteLine("Done.");
