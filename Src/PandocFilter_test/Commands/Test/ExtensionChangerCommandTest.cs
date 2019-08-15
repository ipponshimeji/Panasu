using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using PandocUtil.PandocFilter.Test;
using PandocUtil.PandocFilter.Filters.Test;

namespace PandocUtil.PandocFilter.Commands.Test {
	public class ExtensionChangerCommandTest {
		#region test classes

		public class Filtering {
			#region tests

			[Theory(DisplayName = "Filtering")]
			[ClassData(typeof(ExtensionChangingSample.StandardSampleProvider))]
			public void TestFiltering(ExtensionChangingSample sample) {
				// Arrange
				List<string> args = new List<string>();
				if (sample.RebaseOtherRelativeLinks) {
					args.Add("-RebaseOtherRelativeLinks");
				}
				foreach (KeyValuePair<string, string> pair in sample.ExtensionMap) {
					args.Add($"-Map:{pair.Key}:{pair.Value}");
				}
				args.Add(sample.SupposedFromFileUri);
				args.Add(sample.SupposedToFileUri);

				void filter(Stream inputStream, Stream outputStream) {
					ExtensionChangerCommand command = new ExtensionChangerCommand();
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
