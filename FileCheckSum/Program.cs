using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace FileCheckSum
{
	class Program
	{
		static void Main(string[] args)
		{
			//validateChecksum();
			UploadChunks();
		}

		private async static void UploadChunks()
		{
			var filePath = @"C:\Dev\Perpetual\PerpetualFileWatcher\PerpetualFileWatcher\Sample Input files\wbc_res_loan_loan_2018060601stress.csv";
			var reader = new StreamReader(File.OpenRead(filePath));
			List<string> FeedList = new List<string>();
			while (!reader.EndOfStream)
			{
				var line = reader.ReadLine();
				FeedList.Add(line + Environment.NewLine);
				
			}

			CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=perpetualfilewatcher;AccountKey=5UhhTGLz2GRyjvA64zpNkqHXlAjrGce3CDUOHjpLLOeb2VeodWYLttdB3fZLwmsz7mpHRuSK4OM6ehFEzFIsWA==;EndpointSuffix=core.windows.net");
			CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
			CloudBlobContainer container = blobClient.GetContainerReference("portfolio-analytics");
			var outputDirectory = container.GetDirectoryReference("rmbs/processing/");
			var outputBlob = outputDirectory.GetBlockBlobReference("processedFile.csv");
			outputBlob.Properties.ContentType = "text/csv";
			var FeedFileChunksList = SplitFeedFileInChunks(FeedList);
			var uploadStartIndex = 0;

			// This sets the size of the blocks to use when you do a Put Blob and it breaks it into blocks to upload because the file is larger than the value of SingleBlobUploadThresholdInBytes.
			outputBlob.StreamWriteSizeInBytes = 5 * 256 * 1024; //5 mb


			outputBlob.StreamMinimumReadSizeInBytes = 6 * 1024 * 1024;


			//set the blob upload timeout and retry strategy
			BlobRequestOptions options = new BlobRequestOptions();
			options.ServerTimeout = new TimeSpan(0, 180, 0);
			options.RetryPolicy = new ExponentialRetry(TimeSpan.Zero, 20);

			//get the file blocks of 2MB size each and perform upload of each block
			HashSet<string> blocklist = new HashSet<string>();
			var fileContent = FeedList.SelectMany(s => Encoding.ASCII.GetBytes(s)).ToArray();
			List<FileBlock> bloksT = GetFileBlocks(fileContent).ToList();
			foreach (FileBlock block in GetFileBlocks(fileContent))
			{
				outputBlob.PutBlock(
					block.Id,
					new MemoryStream(block.Content, true), null,
					null, options, null
					);

				blocklist.Add(block.Id);

			}
			//commit the blocks that are uploaded in above loop
			outputBlob.PutBlockList(blocklist, null, options, null);

			Console.WriteLine("Done");
			Console.ReadKey();


			//foreach (var chunkList in FeedFileChunksList)
			//{
			//	TimeSpan backOffPeriod = TimeSpan.FromSeconds(5);
			//	int retryCount = 2;
			//	BlobRequestOptions options = new BlobRequestOptions()
			//	{
			//		SingleBlobUploadThresholdInBytes = 3 * 1024 * 1024, //1MB, the minimum
			//		ParallelOperationThreadCount = 1,
			//		RetryPolicy = new ExponentialRetry(backOffPeriod, retryCount),
			//	};

			//	var ChunkBytes = chunkList.SelectMany(s => Encoding.ASCII.GetBytes(s)).ToArray();
			//	blobClient.DefaultRequestOptions = options;
			//	await outputBlob.UploadFromByteArrayAsync(ChunkBytes, uploadStartIndex, ChunkBytes.Length);
			//	uploadStartIndex += ChunkBytes.Length - 1;
			//}
		}

		private static void validateChecksum()
		{
			using (var md5 = MD5.Create())
			{
				using (var stream = File.OpenRead(@"C:\Dev\Perpetual\PerpetualFileWatcher\PerpetualFileWatcher\Sample Input files\wbc_res_loan_loan_201805311953.csv"))
				{
					// wbc_res_loan_loan_201805311953.conf
					// k1I71g0nkLjqX9SVspHOZQ == - base64
					// 93523BD60D2790B8EA5FD495B291CE65 - bit hash

					// wbc_res_loan_loan_201805311953.csv
					// C4CD3C09DFD40396EE9591DC75A79F32
					// xM08Cd/UA5bulZHcdaefMg==
					var hash = md5.ComputeHash(stream);

					var encodedhash = Encoding.ASCII.GetString(hash);
					var base64 = Convert.ToBase64String(hash);
					var bithash = BitConverter.ToString(hash).Replace("-", "");

					CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=perpetualfilewatcher;AccountKey=5UhhTGLz2GRyjvA64zpNkqHXlAjrGce3CDUOHjpLLOeb2VeodWYLttdB3fZLwmsz7mpHRuSK4OM6ehFEzFIsWA==;EndpointSuffix=core.windows.net");
					CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
					CloudBlobContainer container = blobClient.GetContainerReference("portfolio-analytics");
					var InputDirectory = container.GetDirectoryReference("reject/");
					var blob = InputDirectory.GetBlockBlobReference("wbc_res_loan_loan_201805311953.csv");

					using (var memoryStream = new MemoryStream())
					{
						blob.DownloadToStream(memoryStream);
						memoryStream.Position = 1;
						var bhash = md5.ComputeHash(memoryStream);


						var bencodedhash = Encoding.ASCII.GetString(hash);
						var bbase64 = Convert.ToBase64String(bhash);
						var bbithash = BitConverter.ToString(bhash).Replace("-", "");
					}
				}
			}
		}

		#region helpers
		public static List<List<string>> SplitFeedFileInChunks(List<string> FeedList, int Size = 1000)
		{

			var list = new List<List<string>>();

			for (int i = 0; i < FeedList.Count; i += Size)
			{
				list.Add(FeedList.GetRange(i, Math.Min(Size, FeedList.Count - i)));
			}

			return list;
		}

		private static IEnumerable<FileBlock> GetFileBlocks(byte[] fileContent)
		{
			int MaxBlockSize = 6 * 1024 * 1024; // Approx. 6MB chunk size
			HashSet<FileBlock> hashSet = new HashSet<FileBlock>();
			if (fileContent.Length == 0)
				return new HashSet<FileBlock>();

			int blockId = 0;
			int ix = 0;

			int currentBlockSize = MaxBlockSize;

			while (currentBlockSize == MaxBlockSize)
			{
				if ((ix + currentBlockSize) > fileContent.Length)
					currentBlockSize = fileContent.Length - ix;

				byte[] chunk = new byte[currentBlockSize];
				Array.Copy(fileContent, ix, chunk, 0, currentBlockSize);

				hashSet.Add(
					new FileBlock()
					{
						Content = chunk,
						Id = Convert.ToBase64String(System.BitConverter.GetBytes(blockId))
					});

				ix += currentBlockSize;
				blockId++;
			}

			return hashSet;
		}


		internal class FileBlock
		{
			public string Id
			{
				get;
				set;
			}

			public byte[] Content
			{
				get;
				set;
			}
		}
		#endregion

	}
}
