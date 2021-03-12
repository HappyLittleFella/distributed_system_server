using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceLocator
{

	public class Program
	{
		static public int Main(string[] args) {
			Bootstrapper boot = new Bootstrapper ();
			Errors? e = boot.init (args);
			if (e != null) {
				printErrors (e);
				return -1;
			}

			CreateHostBuilder(args).Build().Run();
			return 0;
		}

		static public IHostBuilder CreateHostBuilder(string[] args) {
			return Host.CreateDefaultBuilder (args)
				.ConfigureWebHostDefaults (webBuilder => {
					webBuilder
					.UseKestrel ()
					.UseStartup<Startup> ();
				});
		}

		static private void printErrors (Errors e) {
			foreach (Error err in e.errors) {
				foreach (string msg in err.msgs) {
					Console.WriteLine ("ERROR: {0}", msg);
				}
			}
		}
	}
}
