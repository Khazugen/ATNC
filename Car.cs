using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ATNC;

internal class Car {
	public enum LightState { Normal, Tunnel }
	public enum RoadType { Normal = 50, Bridge = 80, Tunnel = 30 }

	private readonly Random _rnd = new();
	public readonly ulong id;
	private static ulong _id;
	private readonly CarGPX _gpx;
	private bool _active;
	private string _destination;
	private Type _lasttype = null;
	private bool _back, _forcestop, _forceuntilcity, _skip;
	private int _delay;
	private readonly DispatcherTimer _timer = new();

	public string Destination {
		get => _destination;
		set {
			_active = value is not (null or "");

			List<UserControl> roads = (Window.GetWindow(_gpx) as Tab).roads;

			List<RoadWrapper> wrappers = roads.Select(x => (x.Tag as RoadWrapper)).ToList();

			if (!wrappers.Any(x => x.name == value))
				throw new FormatException("City does not exist.");

			if (_active)
				_back = (roads.First(x => Touches(x)).Tag as RoadWrapper).x > wrappers.First(x => x.name == value).x;

			_destination = value;
		}
	}
	public double Height {
		get => _gpx.Height;
		set => _gpx.Height = value;
	}
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
	public LightState Lights { private set; get; }

	public event RoadTypeChanged RoadChanged;
	public delegate void RoadTypeChanged(object sender, RoadTypeEventArgs e);

	public Car(CarGPX gpx) {
		_gpx = gpx;
		id = _id++;
	}

	public static Process StartProcess(string command) => new() {
		StartInfo = new ProcessStartInfo {
			FileName = @$"{SaveSystem.FilePath.headquaters}ATNC.Headquarters.exe",
			Arguments = command,
			UseShellExecute = false,
			RedirectStandardOutput = true,
			CreateNoWindow = true
		}
	};

	public void Drive() {
		if (_forcestop && !_skip) {
			_skip = true;
			_timer.Interval = new TimeSpan(0, 0, _delay);
			_timer.Tick += (sender, e) => {
				_forcestop = false;
				_timer.Stop();
				_skip = false;
			};

			_timer.Start();
		}

		if (!_active || _skip)
			return;

		_gpx.Margin = new Thickness(
				_forceuntilcity ? _gpx.Margin.Left + (1d * (_back ? -1d : 1d)) : _gpx.Margin.Left + ((MapSpeed / 5d) * (_back ? -1d : 1d)),
				_gpx.Margin.Top,
				_gpx.Margin.Right,
				_gpx.Margin.Bottom
			);

		ProceedChanges();

		if (!_forcestop && !_forceuntilcity)
			Error();
	}

	private void Error() {
		int val = _rnd.Next(0, 50000);

		if (val > 4)
			return;

		Process p = StartProcess($"err {val}");

		p.Start();
		string s = p.StandardOutput.ReadLine();

		if (s == "-")
			return;

		ErrorBehaviour.Solution(s, ref _forcestop, ref _forceuntilcity, ref _delay);
		MessageBox.Show(s);
	}

	public override int GetHashCode() => HashCode.Combine(id);

	private bool Touches(UserControl item) => _gpx.Margin.Top >= item.Margin.Top
		&& _gpx.Margin.Top <= item.Margin.Top + item.Height
		&& _gpx.Margin.Left >= item.Margin.Left
		&& _gpx.Margin.Left <= item.Margin.Left + item.Width;

	private void ProceedChanges() {
		double h = _gpx.ActualHeight,
			w = _gpx.ActualWidth;

		foreach (System.Windows.Controls.UserControl item in (Window.GetWindow(_gpx) as Tab).roads) {
			if (Touches(item)) {
				RoadWrapper wrapper = item.Tag as RoadWrapper;

				if (_forceuntilcity && wrapper.name != null) {
					_forceuntilcity = false;
					_forcestop = true;
					_delay = 5;
				}

				Type t = item.GetType();

				RoadType? r;

				if (t == typeof(Road) && _lasttype != typeof(Road)) {
					Lights = LightState.Normal;
					r = RoadType.Normal;
				} else if (t == typeof(Tunnel) && _lasttype != typeof(Tunnel)) {
					r = RoadType.Tunnel;
					Lights = LightState.Tunnel;
				} else if (t == typeof(Bridge) && _lasttype != typeof(Bridge)) {
					Lights = LightState.Normal;
					r = RoadType.Bridge;
				} else
					continue;

				_lasttype = t;

				ushort u = (ushort)r.Value;

				if (wrapper.name == Destination)
					Destination = null;

				Process psi = StartProcess($"gwet {wrapper.x}");
				psi.Start();

				sbyte weather = sbyte.Parse(psi.StandardOutput.ReadLine());

				if (weather is <= 10 and > 0)
					u /= 2;
				else if (weather <= 0)
					u /= 3;

				RoadChanged?.Invoke(this, new(r.Value, u, weather, wrapper.name));

				break;
			}
		}
	}

	public override string ToString() =>
		$"ID: {id}; Světla: {Enum.GetName(Lights)}; Destinace: {Destination}; Možná reálná rychlost: {RealSpeed}; Možná rychlost na mapě: {MapSpeed};{(_forceuntilcity ? " Auto jede do opravny; " : "")}" +
		$"{(_forcestop ? $"Auto stojí po dobu {_delay} sekund " : "")}";

	public class RoadTypeEventArgs : EventArgs {
		public ushort RecommendedSpeed { get; }
		public RoadType RoadType { get; }
		public sbyte Weather { get; }
		public string Name { get; }

		public RoadTypeEventArgs(RoadType roadType, ushort recommendedSpeed, sbyte weather, string name) {
			RoadType = roadType;
			RecommendedSpeed = recommendedSpeed;
			Weather = weather;
			Name = name;
		}
	}
}