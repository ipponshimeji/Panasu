using System;
using System.Diagnostics;
using Xunit;
using PandocUtil.PandocFilter.Test;

namespace PandocUtil.PandocFilter.Filters.Test {
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
				FormattingFilter target = new FormattingFilter();
				target.FromBaseDirPath = sample.SupposedFromBaseDirUri;
				target.FromFileRelPath = sample.SupposedFromFileRelPath;
				target.ToBaseDirPath = sample.SupposedToBaseDirUri;
				target.ToFileRelPath = sample.SupposedToFileRelPath;
				target.RebaseOtherRelativeLinks = sample.RebaseOtherRelativeLinks;
				target.ExtensionMap = sample.ExtensionMap;

				// test each pattern
				TestUtil.TestFiltering(target, false, sample);   // modify
			}

			#endregion
		}

		#endregion
	}
}
