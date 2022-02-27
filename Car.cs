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
	public double MapSpeed { get; set; }

	public Thickness Margin {
		get => _gpx.Margin;
		set => _gpx.Margin = value;
	}
	public ushort RealSpeed { get; set; }
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
		rot.Angle = rot.Angle == 0d ? 90d : 0d;
	}

	public double GetRotation() => ((_gpx.RenderTransform as TransformGroup).Children[0] as RotateTransform).Angle;

	private void Touches() {
		double h = _gpx.ActualHeight,
			w = _gpx.ActualWidth;

		foreach (System.Windows.Controls.UserControl item in (Window.GetWindow(_gpx) as MainWindow).roads) {
			if (_gpx.Margin.Top >= item.Margin.Top
				&& _gpx.Margin.Top <= item.Margin.Top + item.Height
				&& _gpx.Margin.Left >= item.Margin.Left
				&& _gpx.Margin.Left <= item.Margin.Left + item.Width)
			{
				Type t = item.GetType();

				if (t == typeof(Road) && _lasttype != typeof(Road))
					RoadChanged?.Invoke(this, new(RoadType.Normal));
				else if (t == typeof(Tunnel) && _lasttype != typeof(Tunnel))
					RoadChanged?.Invoke(this, new(RoadType.Tunnel));
				else if (t == typeof(Bridge) && _lasttype != typeof(Bridge))
					RoadChanged?.Invoke(this, new(RoadType.Bridge));

				_lasttype = t;

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