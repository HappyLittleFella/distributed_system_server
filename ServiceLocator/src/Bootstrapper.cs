using System;
using System.Collections.Generic;
using System.Diagnostics;

using Common;

namespace ServiceLocator
{
	public class Bootstrapper
	{

		public Bootstrapper () {

		}

		public Errors? init (string[] args) {
			/*
				- load config
				- check memory storage process
			*/
			string storageName = DataStore.DEFAULT_NAME;
			if (args.Length > 0 && args[0].Length == 0) {
				storageName = args[0];
			}

			Process[] processes = Process.GetProcessesByName ("MemoryStore");
			if (processes.Length == 0) {
				Error e = new Error("MemoryStore process not found on local machine.");
				return new Errors (e);
			}

			try {
				MemoryStorage.init (storageName);
			} catch (Exception e) {
				Error err = new Error("Failed to initialize memory store client.", e);
				return new Errors (err);
			}
			return null;
		}

	}
}
