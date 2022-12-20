using IoTFileUpload;
using Microsoft.Azure.Devices.Client;

const string connectionString = "HostName=we-dev-iot-hub.azure-devices.net;DeviceId=5908764a8e382107d44e8e25_BOX0000311;SharedAccessKey=PRgxdJkQzih9aQXyCGVyYLRnxkAOFKL3EdYzN0wioAs=";

using var deviceClient = DeviceClient.CreateFromConnectionString(
	connectionString,
	TransportType.Mqtt);
var sample = new FileUploadSample(deviceClient);
await sample.RunSampleAsync();

await deviceClient.CloseAsync();

Console.WriteLine("Done.");
Console.ReadKey();
