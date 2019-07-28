using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Utf8Json;
using Xunit;
using PandocUtil.PandocFilter.Filters;
using PandocUtil.PandocFilter.Test;

namespace PandocUtil.PandocFilter.Filters.Test {
	public class ChangingExtensionFilterTest {
		#region properties

		public static string ResourceDirPath {
			get {
				return Path.Combine(TestUtil.ResourceDirPath, "ChangingExtensionFilterTest");
			}
		}

		#endregion


		#region utilities
		#endregion


		#region test classes

		public class Filtering {
			#region properties

			public static string ResourceDirPath {
				get {
					return Path.Combine(TestUtil.ResourceDirPath, "ChangingExtensionFilterTest/Filtering");
				}
			}

			#endregion


			#region utilities

			public static (string inputFilePath, string answerFilePath) GetSampleFilePaths(string sampleFileName) {
				// argument checks
				Debug.Assert(!string.IsNullOrEmpty(sampleFileName));

				string resourceDirPath = ResourceDirPath;
				return (
					Path.Combine(resourceDirPath, $"Inputs/{sampleFileName}"),
					Path.Combine(resourceDirPath, $"Answers/{sampleFileName}")
				);
			}

			public static void TestFiltering(ChangingExtensionFilter target, string sampleFileName, bool generate) {
				// argument checks
				Debug.Assert(target != null);
				Debug.Assert(!string.IsNullOrEmpty(sampleFileName));

				(string inputFilePath, string answerFilePath) = GetSampleFilePaths(sampleFileName);
				FilterTestUtil.TestFiltering(target, inputFilePath, answerFilePath, generate);
			}


			#endregion


			#region tests

			[Fact]
			public void Simple() {
				// Arrange
				string sampleFileName = "simple.json";
				string inputExtension = ".md";
				string outputExtension = ".html";
				(string inputFilePath, string outputFilePath) = FilterTestUtil.GetDummyInputOutputFilePaths(inputExtension, outputExtension, sampleFileName);
				bool rebaseOtherRelativeLink = false;
				ChangingExtensionFilter target = new ChangingExtensionFilter(inputFilePath, outputFilePath, rebaseOtherRelativeLink, inputExtension, outputExtension);

				// Act and Assert
				TestFiltering(target, sampleFileName, false);
			}

			#endregion
		}

		#endregion
	}
}
