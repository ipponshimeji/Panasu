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
				if (parameters.FromBaseDirPath != null) {
					args.Add("-fd");
					args.Add(parameters.FromBaseDirPath);
				}
				if (parameters.FromFileRelPath != null) {
					args.Add("-ff");
					args.Add(parameters.FromFileRelPath);
				}
				if (parameters.ToBaseDirPath != null) {
					args.Add("-td");
					args.Add(parameters.ToBaseDirPath);
				}
				if (parameters.ToFileRelPath != null) {
					args.Add("-tf");
					args.Add(parameters.ToFileRelPath);
				}
				if (parameters.RebaseOtherRelativeLinks.HasValue && parameters.RebaseOtherRelativeLinks.Value) {
					args.Add("-r");
				}
				if (parameters.ExtensionMap != null) {
					foreach (KeyValuePair<string, string> pair in parameters.ExtensionMap) {
						args.Add("-m");
						args.Add($"{pair.Key}:{pair.Value}");
					}
				}

				void filter(Stream inputStream, Stream outputStream) {
					FormatCommand command = new FormatCommand("unit test");
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
