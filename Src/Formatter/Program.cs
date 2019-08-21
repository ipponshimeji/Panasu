using System;
using PandocUtil.PandocFilter.Commands;

namespace PandocUtil.Formatter {
	class Program {
		static void Main(string[] args) {
			new FormatterCommand(
				commandName: "Formatter",
				invocation: "dotnet Formatter.dll"
			).Run(args);
		}
	}
}
