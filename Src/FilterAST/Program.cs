using System;
using Panasu.Commands;

namespace Panasu.FilterAST {
	class Program {
		static void Main(string[] args) {
			new FormattingFilterCommand(
				commandName: "FilterAST",
				invoker: "dotnet FilterAST.dll"
			).Run(args);
		}
	}
}
