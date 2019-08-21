using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using PandocUtil.PandocFilter.Test;

namespace PandocUtil.PandocFilter.Commands.Test {
	public class FormatterCommandTest {
		#region test classes

		public class Filtering {
			#region tests

			[Theory(DisplayName = "Filtering")]
			[ClassData(typeof(FormattingSample.StandardSampleProvider))]
			public void TestFiltering(FormattingSample sample) {
				// Arrange
				List<string> args = new List<string>();
				if (sample.RebaseOtherRelativeLinks) {
					args.Add("-RebaseOtherRelativeLinks");
				}
				foreach (KeyValuePair<string, string> pair in sample.ExtensionMap) {
					args.Add($"-Map:{pair.Key}:{pair.Value}");
				}
				args.Add(sample.SupposedFromBaseDirUri);
				args.Add(sample.SupposedFromFileRelPath);
				args.Add(sample.SupposedToBaseDirUri);
				args.Add(sample.SupposedToFileRelPath);

				void filter(Stream inputStream, Stream outputStream) {
					FormatterCommand command = new FormatterCommand("unit test");
					int exitCode = command.Run(args.ToArray(), inputStream, outputStream);
					if (exitCode != Command.SuccessExitCode) {
						throw new ApplicationException($"The command failed. Exit code: {exitCode}");
					}
				}

				// Act and Assert
				TestUtil.TestFiltering(filter, sample);
			}

			#endregion
		}

		#endregion
	}
}
