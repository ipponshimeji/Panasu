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
		#region constants

		public const string FilteringSampleGroup = "ChangingExtension";

		#endregion


		#region test classes

		public class Filtering {
			#region utilities

			public static void TestFiltering(ChangingExtensionFilter target, FilteringSample sample) {
				// argument checks
				Debug.Assert(target != null);

				// test each pattern
				TestUtil.TestFiltering(target, false, false, sample);	// modify, single-thread
				TestUtil.TestFiltering(target, false, true, sample);	// modify, concurrent
			}

			#endregion


			#region tests

			[Fact]
			public void Simple() {
				// Arrange
				string sampleName = "simple";
				FilteringSample sample = FilteringSample.GetSample(FilteringSampleGroup, sampleName);
				string inputExtension = ".md";
				string outputExtension = ".html";
				(string fromFilePath, string toFilePath) = FilterTestUtil.GetDummyInputOutputFilePaths(inputExtension, outputExtension, sampleName);
				bool rebaseOtherRelativeLinks = false;
				ChangingExtensionFilter target = new ChangingExtensionFilter(fromFilePath, toFilePath, rebaseOtherRelativeLinks, inputExtension, outputExtension);

				// Act and Assert
				TestFiltering(target, sample);
			}

			#endregion
		}

		#endregion
	}
}
