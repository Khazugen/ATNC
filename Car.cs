using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;

namespace ATNC;

internal class Car {
	public enum LightState { Off, Day, Meeting, Fog }
	public enum RoadType { Normal = 50, Bridge = 80, Tunnel = 30 }

	private readonly CarGPX _gpx;
	private Type _lasttype = typeof(Road);
	private bool _active;
	private readonly List<DirectionChange> _changes = new();

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
		if (!_active)
			return;

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

	public void SetDestination(string destination) {
		DirectionProvider dp = new(destination, (Window.GetWindow(_gpx) as Tab).cords);
		_active = true;

		//_changes
	}

	private void DestinationCompleted() {
		Destination = "";
		_active = false;
	}

	private void ChangeRotation() {
		RotateTransform rot = (_gpx.RenderTransform as TransformGroup).Children[0] as RotateTransform;
		rot.Angle = rot.Angle == 0d ? 90d : 0d;
	}

	public double GetRotation() => ((_gpx.RenderTransform as TransformGroup).Children[0] as RotateTransform).Angle;

	private void Touches() {
		double h = _gpx.ActualHeight,
			w = _gpx.ActualWidth;

		foreach (System.Windows.Controls.UserControl item in (Window.GetWindow(_gpx) as Tab).roads) {
			if (_gpx.Margin.Top >= item.Margin.Top
				&& _gpx.Margin.Top <= item.Margin.Top + item.Height
				&& _gpx.Margin.Left >= item.Margin.Left
				&& _gpx.Margin.Left <= item.Margin.Left + item.Width) {
				Type t = item.GetType();

				RoadType? r = null;
				ushort u;

				if (t == typeof(Road) && _lasttype != typeof(Road))
					r = RoadType.Normal;
				else if (t == typeof(Tunnel) && _lasttype != typeof(Tunnel))
					r = RoadType.Tunnel;
				else if (t == typeof(Bridge) && _lasttype != typeof(Bridge))
					r = RoadType.Bridge;

				u = (ushort)r.Value;

				RoadWrapper wrapper = item.Tag as RoadWrapper;

				Process psi = StartProcess($"gwet {wrapper.x};{wrapper.y}");
				psi.Start();

				sbyte weather = sbyte.Parse(psi.StandardOutput.ReadLine());

				if (weather <= 10)
					u /= 2;
				else if (weather <= 0)
					u -= (ushort)(u / 3);

				RoadChanged?.Invoke(this, new(r.Value, u));
				_lasttype = t;

				break;
			}
		}
	}

	private bool MultipleTouches() {
		double h = _gpx.ActualHeight,
			w = _gpx.ActualWidth;

		byte touches = 0;

		foreach (System.Windows.Controls.UserControl item in (Window.GetWindow(_gpx) as Tab).roads)
			if (_gpx.Margin.Top >= item.Margin.Top
				&& _gpx.Margin.Top <= item.Margin.Top + item.Height
				&& _gpx.Margin.Left >= item.Margin.Left
				&& _gpx.Margin.Left <= item.Margin.Left + item.Width)

				if (++touches == 2)
					return true;

		return false;
	}

	public static Process StartProcess(string command) => new() {
		StartInfo = new ProcessStartInfo {
			FileName = @"C:\Users\pruch\source\repos\ATNC.Headquarters\bin\Debug\net6.0\ATNC.Headquarters.exe",
			Arguments = command,
			UseShellExecute = false,
			RedirectStandardOutput = true,
			CreateNoWindow = true
		}
	};

	public class RoadTypeEventArgs : EventArgs {
		public ushort RecommendedSpeed { get; }
		public RoadType RoadType { get; }

		public RoadTypeEventArgs(RoadType roadType, ushort recommendedSpeed) {
			RoadType = roadType;
			RecommendedSpeed = recommendedSpeed;
		}
	}

	 struct DirectionChange {
		public  enum Direction { Up, Down, Left, Right }

		public readonly Direction fromdirection, todirection;
		public readonly double x, y;

		public DirectionChange(Direction fromdirection, Direction todirection, double x, double y) {
			this.fromdirection = fromdirection;
			this.todirection = todirection;
			this.x = x;
			this.y = y;
		}
	}
}