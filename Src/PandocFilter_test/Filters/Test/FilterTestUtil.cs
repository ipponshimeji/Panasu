using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using PandocUtil.PandocFilter.Test;

namespace PandocUtil.PandocFilter.Filters.Test {
	public static class FilterTestUtil {
		#region utilities

		public static (string dummyFromFilePath, string dummyToFilePath) GetDummyInputOutputFilePaths(string fromExtension, string toExtension, string sampleFileName) {
			// argument checks
			Debug.Assert(fromExtension != null);   // inputExtension can be empty
			Debug.Assert(toExtension != null);  // outputExtension can be empty
			Debug.Assert(!string.IsNullOrEmpty(sampleFileName));

			string directorySeparator = TestUtil.DirectorySeparator;
			string dirPath = Path.GetTempPath();
			string fromFileName = Path.ChangeExtension(sampleFileName, fromExtension);
			string toFileName = Path.ChangeExtension(sampleFileName, toExtension);
			return (
				Path.Combine(dirPath, $"Inputs{directorySeparator}{fromFileName}"),
				Path.Combine(dirPath, $"Outputs{directorySeparator}{toFileName}")
			);
		}

		#endregion
	}
}
