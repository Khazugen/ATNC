using System;
using System.Windows;
using System.Windows.Media;

namespace ATNC;

internal class Car {
	public enum LightState { Off, Day, Meeting, Fog }
	public enum RoadType { Normal = 50, Bridge = 80, Tunnel = 30 }

	private readonly CarGPX _gpx;

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

	public double Speed { get; set; }

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
				_gpx.Margin.Left + Speed / 5d,
				_gpx.Margin.Top,
				_gpx.Margin.Right,
				_gpx.Margin.Bottom
			)
			: new Thickness(
				_gpx.Margin.Left,
				_gpx.Margin.Top + Speed / 5d,
				_gpx.Margin.Right,
				_gpx.Margin.Bottom
			);

			Touches();
		}

	private Type _lasttype = typeof(Road);

	private void Touches() {
		Thickness m = _gpx.Margin;
		double h = _gpx.ActualHeight,
			w = _gpx.ActualWidth;

		foreach (System.Windows.Controls.UserControl item in (Window.GetWindow(_gpx) as MainWindow).roads) {
			if (m.Top >= item.Margin.Top
				&& m.Top <= item.Margin.Top + item.ActualHeight
				&& m.Left >= item.Margin.Left
				&& m.Left <= item.Margin.Left + item.ActualWidth)
			{
				if (item is Road && _lasttype != typeof(Road))
					RoadChanged?.Invoke(this, new(RoadType.Normal));
				else if (item is Tunnel && _lasttype != typeof(Tunnel))
					RoadChanged?.Invoke(this, new(RoadType.Tunnel));
				else if (item is Bridge && _lasttype != typeof(Bridge))
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