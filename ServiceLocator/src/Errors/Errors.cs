using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceLocator
{
	public class Errors
	{

		public List<Error> errors = new List<Error>();


		public Errors () {
		}

		public Errors (Error error) {
			errors.Add (error);
		}

		public Errors (List<Error> errors) {
			errors.AddRange (errors);
		}

		public void addError (Error error) {
			errors.Add (error);
		}
	}
}
