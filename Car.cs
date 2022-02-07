using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATNC;

internal class Car {
	public Car() { }

	public enum LightState { Off, Day, Meeting }

	public ushort Speed { get; set; }
	public LightState Light { get; set; }
	public string Destination { get; set; }

}