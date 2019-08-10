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
	public class ExtensionChangingFilterTest {
		#region test classes

		public class Filtering {
			#region tests

			[Theory(DisplayName="Filtering")]
			[ClassData(typeof(ExtensionChangingSample.StandardSampleProvider))]
			public void TestFiltering(ExtensionChangingSample sample) {
				// argument checks
				Debug.Assert(sample != null);

				// create the test target
				ExtensionChangingFilter target = new ExtensionChangingFilter(sample.SupposedFromFileUri, sample.SupposedToFileUri, sample.RebaseOtherRelativeLinks, sample.ExtensionMap);

				// test each pattern
				TestUtil.TestFiltering(target, false, sample);   // modify, single-thread
			}

			#endregion
		}

		#endregion
	}
}
