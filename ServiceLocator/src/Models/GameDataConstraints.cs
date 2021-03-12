using System;
using System.Collections.Generic;
using System.Net;

namespace ServiceLocator
{
	public class GameDataConstraints
	{

		public readonly int						nameMaxLength,
												nameMinLength;
		public readonly HashSet<IPAddress>		ipBlacklist;

		public GameDataConstraints (int nameMaxLength, int nameMinLength, HashSet<IPAddress> ipBlacklist) {
			this.nameMaxLength = nameMaxLength;
			this.nameMinLength = nameMinLength;
			this.ipBlacklist = ipBlacklist;
		}

	}
}
