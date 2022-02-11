using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ATNC;

internal class Car {
	public enum LightState { Off, Day, Meeting, Fog }
	public enum RoadType { Normal = 50, Bridge = 80, Tunnel = 30 }

	public Car(CarGPX gpx) => _gpx = gpx;

	private readonly CarGPX _gpx;
	public ushort Speed { get; set; }
	public LightState Light { get; set; }
	public string Destination { get; set; }
	public Thickness Margin {
		get => _gpx.Margin;
		set => _gpx.Margin = value;
	}
	public double Width {
		get => _gpx.Width;
		set => _gpx.Width = value;
	}
	public double Height {
		get => _gpx.Height;
		set => _gpx.Height = value;
	}

	public delegate void RoadTypeChanged(object sender, RoadTypeEventArgs e);
	public event RoadTypeChanged RoadChanged;

	public void Drive() => _gpx.Margin = new System.Windows.Thickness(
			_gpx.Margin.Left + Speed / 10,
			_gpx.Margin.Top,
			_gpx.Margin.Right,
			_gpx.Margin.Bottom
		);

	public class RoadTypeEventArgs : EventArgs {
		public RoadTypeEventArgs(RoadType roadType, ushort recommendedSpeed) {
			RoadType = roadType;
			RecommendedSpeed = (ushort)roadType;
		}

		public RoadType RoadType { get; }
		public ushort RecommendedSpeed { get; }
	}
}