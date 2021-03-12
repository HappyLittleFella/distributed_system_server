using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Common;

namespace ServiceLocator
{
	public class MemoryStorage
	{

		static private MemoryStorage? instance;

		public readonly DataStore store;

		private MemoryStorage (string name) {
			store = DataStore.NewClient (name);
		}

		static public MemoryStorage init (string name) {
			return instance ?? (instance = new MemoryStorage (name));
		}

		static public MemoryStorage get () {
			return instance ?? throw new NullReferenceException("Memory store has not been initialized.");
		}

	}
}
