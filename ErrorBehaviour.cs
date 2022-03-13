using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATNC;

internal static class ErrorBehaviour {
	delegate void SetValues(ref bool forcestop, ref bool forceuntilcity, ref int delay);
	private static readonly Dictionary<string, SetValues> _errors = new() {
		{
			"The tire has been punctured, stop and replace the flat tire with a spare.",
			(ref bool forcestop, ref bool forceuntilcity, ref int delay) => {
				forcestop = true;
				delay = 10;
			}
		},
		{
			"You're out of gas, push the car to the side of the road and refuel.",
			(ref bool forcestop, ref bool forceuntilcity, ref int delay) => {
				forcestop = true;
				delay = 5;
			}
		},
		{
			"Car engine error (clutch violation) - start the engine.",
			(ref bool forcestop, ref bool forceuntilcity, ref int delay) => {
				forcestop = true;
				delay = 1;
			}
		},
		{
			"Car engine error - look for the nearest car service.",
			(ref bool forcestop, ref bool forceuntilcity, ref int delay) => forceuntilcity = true
		},
		{
			"Unknown error - look for the nearest car service.",
			(ref bool forcestop, ref bool forceuntilcity, ref int delay) => forceuntilcity = true
		}
	};

	public static void Solution(string err, ref bool forcestop, ref bool forceuntilcity, ref int delay) => _errors[err](ref forcestop, ref forceuntilcity, ref delay);
}