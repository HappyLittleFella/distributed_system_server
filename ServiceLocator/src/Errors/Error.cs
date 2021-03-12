using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceLocator
{
	public class Error {

		public Dictionary<string, object> parameters = new Dictionary<string, object>();
		public StackTrace stackTrace;
		public Exception? cause = null;
		public List<string> msgs = new List<string>();


		public Error (string msg) {
			msgs.Add (msg);
		}

		public Error (string msg, Exception e) {
			msgs.Add (msg);
			cause = e;
			addExceptionMsgs (cause);
		}

		private void addExceptionMsgs (Exception? e) {
			while (e != null) {
				msgs.Add (e.Message);
				e = e.InnerException;
			}
		}
	}
}
