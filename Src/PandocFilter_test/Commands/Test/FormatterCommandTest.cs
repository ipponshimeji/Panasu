using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using PandocUtil.PandocFilter.Filters;
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
				FormattingFilter.Parameters parameters = sample.Config.Parameters;
				if (parameters.RebaseOtherRelativeLinks.HasValue && parameters.RebaseOtherRelativeLinks.Value) {
					args.Add("-RebaseOtherRelativeLinks");
				}
				if (parameters.ExtensionMap != null) {
					foreach (KeyValuePair<string, string> pair in parameters.ExtensionMap) {
						args.Add($"-Map:{pair.Key}:{pair.Value}");
					}
				}
				args.Add(parameters.FromBaseDirPath);
				args.Add(parameters.FromFileRelPath);
				args.Add(parameters.ToBaseDirPath);
				args.Add(parameters.ToFileRelPath);

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
