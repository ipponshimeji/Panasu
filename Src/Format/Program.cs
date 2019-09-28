using System;
using Panasu.Commands;

namespace Panasu.Format {
	class Program {
		static void Main(string[] args) {
			new FormatCommand(
				commandName: "Format",
				invoker: "dotnet Format.dll"
			).Run(args);
		}
	}
}
