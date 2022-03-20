using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;

namespace ATNC;

internal class Car {
	public enum LightState { Normal, Tunnel }
	public enum RoadType { Normal = 50, Bridge = 80, Tunnel = 30 }

	private readonly Random _rnd = new();
	public readonly ulong id;
	private static ulong _id;
	private bool _active;
	private string _destination;
	private Type _lasttype = null;
	private bool _back, _forcestop, _forceuntilcity, _skip;
	private int _delay;
	private sbyte _weather;
	private readonly DispatcherTimer _timer = new();
	private readonly IEnumerable<RoadWrapper> _roads;
	private const uint _speedscale = 3u;

	public string Destination {
		get => _destination;
		set {
			_active = value is not (null or "");

			if (!_roads.Any(x => x.name == value))
				throw new FormatException("City does not exist.");

			if (_active)
				_back = _roads.First(x => Touches(x)).x > _roads.First(x => x.name == value).x;

			_destination = value;
		}
	}
	public double Height { get; set; }
	public double X { get; set; }
	public ushort RealSpeed { get; set; }
	public double Width { get; set; }
	public LightState Lights { private set; get; }

	public event RoadTypeChanged RoadChanged;
	public delegate void RoadTypeChanged(object sender, RoadTypeEventArgs e);

	public Car(IEnumerable<RoadWrapper> roads) {
		_roads = roads;
		id = _id++;
		Headquaters.MeteoStation.WeatherChanged += (e) => _weather = e.Weather;
		RoadChanged += (sender, e) => RealSpeed = e.AllowedSpeed;
	}

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

		double adv = _forceuntilcity ? RealSpeed / _speedscale * (_back ? -1d : 1d) / 2 : (RealSpeed / _speedscale * (_back ? -1d : 1d));

		if (_weather is <= 10 and > 0)
			adv /= 1.5d;
		else if (_weather < 0)
			adv /= 2d;

		X += adv;

		ProceedChanges();

		if (!_forcestop && !_forceuntilcity)
			Error();
	}

	private void Error() {
		int val = _rnd.Next(0, 50000);

		if (val > 4)
			return;

		string s = Headquaters.GetError(val.ToString());

		if (s == "-")
			return;

		ErrorBehaviour.Solution(s, ref _forcestop, ref _forceuntilcity, ref _delay);
		System.Windows.MessageBox.Show(s);
	}

	public override int GetHashCode() => HashCode.Combine(id);

	private bool Touches(RoadWrapper item) => X >= item.x && X <= item.x + item.w;

	private void ProceedChanges() {
		foreach (RoadWrapper item in _roads) {
			if (Touches(item)) {
				if (_forceuntilcity && item.name != null) {
					_forceuntilcity = false;
					_forcestop = true;
					_delay = 5;
				}

				RoadType? r;

				if (item.type == typeof(Road) && _lasttype != typeof(Road)) {
					Lights = LightState.Normal;
					r = RoadType.Normal;
				} else if (item.type == typeof(Tunnel) && _lasttype != typeof(Tunnel)) {
					r = RoadType.Tunnel;
					Lights = LightState.Tunnel;
				} else if (item.type == typeof(Bridge) && _lasttype != typeof(Bridge)) {
					Lights = LightState.Normal;
					r = RoadType.Bridge;
				} else
					continue;

				_lasttype = item.type;

				if (item.name == Destination)
					Destination = null;

				RoadChanged?.Invoke(this, new(r.Value, (ushort)r.Value, item.name));

				break;
			}
		}
	}

	public override string ToString() =>
		$"ID: {id}; Světla: {Enum.GetName(Lights)}; Destinace: {Destination}; " +
		$"Povolená reálná rychlost: {RealSpeed}; {(_forceuntilcity ? " Auto jede do opravny; " : "")}" +
		$"{(_forcestop ? $"Auto stojí po dobu {_delay} sekund " : "")}";

	public class RoadTypeEventArgs : EventArgs {
		public ushort AllowedSpeed { get; }
		public RoadType RoadType { get; }
		public string Name { get; }

		public RoadTypeEventArgs(RoadType roadType, ushort allowedspeed, string name) {
			RoadType = roadType;
			AllowedSpeed = allowedspeed;
			Name = name;
		}
	}
}