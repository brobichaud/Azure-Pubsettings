using System;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.IO;

namespace CreatePublishSettingsFile
{
	class Program
	{
		private const string _subscriptionId = "3ae41414-117a-45f9-89b0-669e91061ae3";
		private static string _subscriptionName = "Madras Dev";
		private const string _certificateThumbprint = "3D487F020137ECC4B71FE862F530841E5BD34E58";
		private const StoreLocation _certificateStoreLocation = StoreLocation.CurrentUser;
		private const StoreName _certificateStoreName = StoreName.My;
		private const string _publishFileFormat = @"
<?xml version=""1.0"" encoding=""utf-8""?>
<PublishData>
	<PublishProfile
		SchemaVersion=""2.0""
		PublishMethod=""AzureServiceManagementAPI"">
		<Subscription
		  ServiceManagementUrl=""https://management.core.windows.net""
		  Id=""{1}""
		  Name=""{2}""
		  ManagementCertificate=""{0}"">
		</Subscription>
	</PublishProfile>
</PublishData>";

		static void Main()
		{
			var certificateStore = new X509Store(_certificateStoreName, _certificateStoreLocation);
			certificateStore.Open(OpenFlags.ReadOnly);
			X509Certificate2Collection certificates = certificateStore.Certificates;

			var matchingCertificates = certificates.Find(X509FindType.FindByThumbprint, _certificateThumbprint, false);
			if (matchingCertificates.Count == 0)
				Console.WriteLine("No matching certificate found. Please ensure that proper values are specified for Certificate Store Name, Location and Thumbprint");
			else
			{
				var certificate = matchingCertificates[0];
				var certificateData = Convert.ToBase64String(certificate.Export(X509ContentType.Pkcs12, string.Empty));
				
				if (string.IsNullOrWhiteSpace(_subscriptionName)) _subscriptionName = _subscriptionId;
				
				var publishSettingsFileData = string.Format(_publishFileFormat, certificateData, _subscriptionId, _subscriptionName);
				var fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), _subscriptionName + ".publishsettings");
				File.WriteAllBytes(fileName, Encoding.UTF8.GetBytes(publishSettingsFileData));
				Console.WriteLine("Publish settings file written successfully at: " + fileName);
			}
		}
	}
}
