using System;
using System.Diagnostics;
using Xunit;
using Panasu.Test;

namespace Panasu.Filters.Test {
	public class FormattingFilterTest {
		#region test classes

		public class Filtering {
			#region tests

			[Theory(DisplayName="Filtering")]
			[ClassData(typeof(FormattingSample.StandardSampleProvider))]
			public void TestFiltering(FormattingSample sample) {
				// argument checks
				Debug.Assert(sample != null);

				// create the test target
				FormattingFilter target = new FormattingFilter(sample.Config);

				// test each pattern
				TestUtil.TestFiltering(target, false, sample);   // modify
			}

			#endregion
		}

		#endregion
	}
}
