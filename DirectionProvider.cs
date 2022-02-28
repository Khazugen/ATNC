using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATNC;

internal class DirectionProvider {
	public DirectionProvider(string destination, IEnumerable<RoadsWrapper> roads) {
		FindBestRoute();

		void FindBestRoute() {
			if (!roads.Any(x => x.name == destination))
				throw new DirectionException("Destination doesn't exist.");


		}
	}

	private class DirectionException : Exception {
		public DirectionException() : base("Direction Exception has been thrown.") { }
		public DirectionException(string message) : base(message) { }
		public DirectionException(string message, Exception innerException) : base(message, innerException) { }
	}

}