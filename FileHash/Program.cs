using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.CommandLine;
using System.Net.Mail;
using System.Net;
using DataSizeUnits;

namespace FileHash;
internal class Program {
	static async Task<int> Main(string[] args) {
		var fileOption = new Option<string>(aliases: new string[] { "--file", "-f" }, description: "Input file") { IsRequired = true };
		var sha256Option = new Option<bool?>("--sha256",description: "Compute SHA256 hash") { IsRequired = false };
		var sha1Option = new Option<bool?>(name: "--sha1", description: "Compute SHA256 hash") { IsRequired = false };
		var fileInfoOption = new Option<bool?>(aliases: new string[] { "--fileInfo", "-i" }, description: "Add file info to output") { IsRequired = false };

		var rootCommand = new RootCommand("Lacuna Software File Hash");
		rootCommand.AddOption(fileOption);
		rootCommand.AddOption(sha256Option);
		rootCommand.AddOption(sha1Option);
		rootCommand.AddOption(fileInfoOption);

		rootCommand.SetHandler((file, sha256, sha1, fileInfo) => {
			Console.WriteLine("File Hash v1.1");
			Console.WriteLine("");
			if (!File.Exists(file)) {
				Console.WriteLine($"{file} not found");
				return;
			}
			try {
				var info = new FileInfo(file);
				var size = string.Format(new DataSizeFormatter(), "{0:A1}", info.Length);
				Console.WriteLine($"Name: {info.Name}");
				if (fileInfo is null || fileInfo.Value) {
					Console.WriteLine($"Size: {info.Length} bytes ({size})");
					Console.WriteLine($"Creation UTC date: {info.CreationTimeUtc:G}");
					Console.WriteLine($"Last Write UTC date: {info.LastWriteTimeUtc:G}");
				}

				using FileStream fileStream = File.OpenRead(file);
				using BufferedStream bufferedStream = new BufferedStream(fileStream, 1024 * 32);

				if (sha256 is null || sha256.Value) {
					var hashBytes = SHA256.Create().ComputeHash(bufferedStream);
					var hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
					Console.WriteLine($"SHA256: {hash}");
				}
				if (sha1.HasValue && sha1.Value) {
					var hashBytes = SHA1.Create().ComputeHash(bufferedStream);
					var hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
					Console.WriteLine($"SHA1: {hash}");
				}
			} catch (Exception e) {
				Console.WriteLine($"Error: {e.Message}");
			}

		}, fileOption, sha256Option, sha1Option, fileInfoOption);
		return await rootCommand.InvokeAsync(args);
	}
}


