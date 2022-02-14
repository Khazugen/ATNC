using System;
using System.Windows;
using System.Windows.Media;

namespace ATNC;

internal class Car {
	public enum LightState { Off, Day, Meeting, Fog }
	public enum RoadType { Normal = 50, Bridge = 80, Tunnel = 30 }

	private readonly CarGPX _gpx;
	private Type _lasttype = typeof(Road);

	public string Destination { get; set; }
	public double Height {
		get => _gpx.Height;
		set => _gpx.Height = value;
	}
	public LightState Light { get; set; }
	public Thickness Margin {
		get => _gpx.Margin;
		set => _gpx.Margin = value;
	}
	public ushort RealSpeed { get; set; }
	public double MapSpeed { get; set; }
	public double Width {
		get => _gpx.Width;
		set => _gpx.Width = value;
	}

	public event RoadTypeChanged RoadChanged;
	public delegate void RoadTypeChanged(object sender, RoadTypeEventArgs e);

	public Car(CarGPX gpx) => _gpx = gpx;

	public void Drive() {
		_gpx.Margin = ((_gpx.RenderTransform as TransformGroup).Children[0] as RotateTransform).Angle == 0
			? new Thickness(
				_gpx.Margin.Left + MapSpeed / 5d,
				_gpx.Margin.Top,
				_gpx.Margin.Right,
				_gpx.Margin.Bottom
			)
			: new Thickness(
				_gpx.Margin.Left,
				_gpx.Margin.Top + MapSpeed / 5d,
				_gpx.Margin.Right,
				_gpx.Margin.Bottom
			);

			Touches();
		}

	private void ChangeRotation() {
		RotateTransform rot = (_gpx.RenderTransform as TransformGroup).Children[0] as RotateTransform;
		rot.Angle = rot.Angle == 0 ? 90 : 0;
	}

	private void Touches() {
		Thickness m = _gpx.Margin;
		double h = _gpx.ActualHeight,
			w = _gpx.ActualWidth;

		foreach (RoadsWrapper item in (Window.GetWindow(_gpx) as MainWindow).cords) {
			if (m.Top >= item.y
				&& m.Top <= item.y + item.h
				&& m.Left >= item.x
				&& m.Left <= item.x + item.w)
			{
				if (item.type == typeof(Road) && _lasttype != typeof(Road))
					RoadChanged?.Invoke(this, new(RoadType.Normal));
				else if (item.type == typeof(Tunnel) && _lasttype != typeof(Tunnel))
					RoadChanged?.Invoke(this, new(RoadType.Tunnel));
				else if (item.type == typeof(Bridge) && _lasttype != typeof(Bridge))
					RoadChanged?.Invoke(this, new(RoadType.Bridge));

				_lasttype = item.GetType();

				break;
			}
		}
	}

	public class RoadTypeEventArgs : EventArgs {
		public ushort RecommendedSpeed { get; }
		public RoadType RoadType { get; }

		public RoadTypeEventArgs(RoadType roadType) {
			RoadType = roadType;
			RecommendedSpeed = (ushort)roadType;
		}
	}
}