using System;
using System.Collections.Generic;
using System.Linq;

namespace ATNC;

internal class DirectionProvider {
	public DirectionProvider(string destination, IEnumerable<RoadWrapper> roads) {
		FindBestRoute();

		void FindBestRoute() {
			if (!roads.Any(x => x.name == destination))
				throw new DirectionException("Destination doesn't exist.");

			string current;

			do {
				current = "";//roads[0].name;
			} while (current != destination);
		}
	}

	private class DirectionException : ApplicationException {
		public DirectionException() : base("Direction exception has been thrown.") { }
		public DirectionException(string message) : base(message) { }
		public DirectionException(string message, Exception innerException) : base(message, innerException) { }
	}
}