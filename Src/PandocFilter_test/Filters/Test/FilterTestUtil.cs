using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Utf8Json;
using Xunit;

namespace PandocUtil.PandocFilter.Filters.Test {
	public static class FilterTestUtil {
		#region utilities

		public static (string dummyInputFilePath, string dummyOutputFilePath) GetDummyInputOutputFilePaths(string inputExtension, string outputExtension, string sampleFileName) {
			// argument checks
			Debug.Assert(inputExtension != null);   // inputExtension can be empty
			Debug.Assert(outputExtension != null);  // outputExtension can be empty
			Debug.Assert(!string.IsNullOrEmpty(sampleFileName));

			string dirPath = Path.GetTempPath();
			string inputFileName = Path.ChangeExtension(sampleFileName, inputExtension);
			string outputFileName = Path.ChangeExtension(sampleFileName, outputExtension);
			return (
				Path.Combine(dirPath, $"Inputs/{inputFileName}"),
				Path.Combine(dirPath, $"Outputs/{outputFileName}")
			);
		}

		public static void TestFiltering(ChangingExtensionFilter target, string sampleFilePath, string answerFilePath, bool generate, bool concurrent) {
			// Act
			// read input JSON
			Dictionary<string, object> ast;
			using (Stream stream = File.OpenRead(sampleFilePath)) {
				ast = JsonSerializer.Deserialize<Dictionary<string, object>>(stream);
			}

			// filter
			IDictionary<string, object> filtered;
			if (generate) {
				filtered = target.Generate(ast, concurrent);
			} else {
				target.Modify(ast, concurrent);
				filtered = ast;
			}

			// get output JSON
			string actual;
			using (MemoryStream stream = new MemoryStream()) {
				JsonSerializer.Serialize(stream, ast);
				actual = Encoding.UTF8.GetString(stream.ToArray());
			}

			// Assert
			string expected = File.ReadAllText(answerFilePath);
			Assert.Equal(expected, actual);
		}

		public static void TestFiltering(ChangingExtensionFilter target, string sampleFilePath, string answerFilePath, bool generate) {
			TestFiltering(target, sampleFilePath, answerFilePath, generate, false);
			TestFiltering(target, sampleFilePath, answerFilePath, generate, true);
		}

		#endregion
	}
}
