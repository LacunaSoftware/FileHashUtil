using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.CommandLine;
using System.Net.Mail;
using System.Net;
using DataSizeUnits;
using System.Runtime.Intrinsics.Arm;
using Pastel;

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

		rootCommand.SetHandler((file, sha256opt, sha1opt, fileInfo) => {
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
					Console.WriteLine($"Size: {info.Length:N0} bytes ({size})");
					Console.WriteLine($"Creation UTC date: {info.CreationTimeUtc:G}");
					Console.WriteLine($"Last Write UTC date: {info.LastWriteTimeUtc:G}");
				}
				sha256opt ??= true;
				sha1opt ??= false;
				using FileStream fileStream = File.OpenRead(file);
				var sha256 = SHA256.Create();
				var sha1 = SHA1.Create();
				var totalLength = fileStream.Length;
				var bufferSize = 1024 * 1024;
				var buffer = new byte[bufferSize];
				int bytesRead;
				long totalBytesRead = 0;
				double percentage = 0;

				while ((bytesRead = fileStream.Read(buffer, 0, bufferSize)) > 0) {
					if (sha256opt.Value) {
						sha256.TransformBlock(buffer, 0, bytesRead, null, 0);
					}
					if (sha1opt.Value) {
						sha1.TransformBlock(buffer, 0, bytesRead, null, 0);
					}
					totalBytesRead += bytesRead;
					percentage = (double)totalBytesRead / totalLength * 100;
					PrintProgressBar(percentage);
				}
				if (sha256opt.Value) {
					sha256.TransformFinalBlock(buffer, 0, 0);
				}
				if (sha1opt.Value) {
					sha1.TransformFinalBlock(buffer, 0, 0);
				}
				if (sha256opt.Value) {
					if (sha256.Hash is null) {
						Console.Write("\r");
						Console.WriteLine($"SHA256: ERROR");
					} else {
						byte[] finalHash = sha256.Hash;
						string hash = BitConverter.ToString(finalHash).Replace("-", "").ToLowerInvariant();
						Console.Write("\r"); // End of progress bar
						Console.WriteLine($"SHA256: {hash}");
					}
				}
				if (sha1opt.Value) {
					if (sha1.Hash is null) {
						Console.Write("\r");
						Console.WriteLine($"SHA1: ERROR");
					} else {
						byte[] finalHash = sha1.Hash;
						string hash = BitConverter.ToString(finalHash).Replace("-", "").ToLowerInvariant();
						Console.Write("\r"); // End of progress bar
						Console.WriteLine($"SHA1: {hash}");
					}
				}
			} catch (Exception e) {
				Console.WriteLine($"Error: {e.Message}".Pastel(ConsoleColor.Red));
			}

		}, fileOption, sha256Option, sha1Option, fileInfoOption);
		return await rootCommand.InvokeAsync(args);
	}

	static void PrintProgressBar(double percentage) {
		Console.Write($"\r{percentage:N1}% Completed".Pastel(ConsoleColor.Green));
	}

}


