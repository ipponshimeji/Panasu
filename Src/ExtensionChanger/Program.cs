using System;
using PandocUtil.PandocFilter.Commands;

namespace PandocUtil.ExtensionChanger {
	class Program {
		static void Main(string[] args) {
			new ExtensionChangerCommand().Run(args);
		}
	}
}
