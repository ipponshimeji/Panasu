using System;
using PandocUtil.PandocFilter.Commands;

namespace PandocUtil.Formatter {
	class Program {
		static void Main(string[] args) {
			new FormatCommand(
				commandName: "Formatter",
				invoker: "dotnet Formatter.dll"
			).Run(args);
		}
	}
}
