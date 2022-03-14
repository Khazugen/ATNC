using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ATNC;

public partial class Tab : Window {
	private enum Direction { Left, Right, Up, Down }

	public readonly List<RoadWrapper> cords = new();
	public readonly List<UserControl> roads = new();
	private const double _advance = 50d;
	private const double _carh = 100;
	private const double _carw = 200;
	private static readonly Dictionary<Direction, Func<Thickness, Thickness>> _dirs = new() {
		{ Direction.Left, (item) => new Thickness(item.Left - _advance, item.Top, item.Right, item.Bottom) },
		{ Direction.Right, (item) => new Thickness(item.Left + _advance, item.Top, item.Right, item.Bottom) },
		{ Direction.Up, (item) => new Thickness(item.Left, item.Top - _advance, item.Right, item.Bottom) },
		{ Direction.Down, (item) => new Thickness(item.Left, item.Top + _advance, item.Right, item.Bottom) }
	};

	private readonly List<Car> _cars = new();
	private readonly DispatcherTimer _cartimer = new();
	private const uint _scale = 20u;
	private Car _selected;

	public Tab() {
		InitCords();
		InitializeComponent();
		DrawGPS();

		_cartimer.Interval = new System.TimeSpan(0, 0, 0, 0, 10);
		_cartimer.Start();
		_cartimer.Tick += (sender, e) => _cars.ForEach(x => x.Drive());
		Headquaters.MeteoStation.WeatherChanged += (e) => l_temp.Content = $"{e.Weather}°C";
	}

	private void B_Add_Car_Click(object sender, RoutedEventArgs e) {
		CarGPX gpx = new() {
			Name = "car",
			HorizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment = VerticalAlignment.Center,
			RenderTransformOrigin = new Point(0.5d, 0.5d)
		};

		gr.Children.Add(gpx);
		Panel.SetZIndex(gpx, 9);

		Car car = new(gpx) {
			RealSpeed = 50,
			Margin = new Thickness(0, 0, 0, 0),
			Width = _carw / _scale,
			Height = _carh / _scale
		};

		gpx.Tag = car.id;

		_cars.Add(car);

		car.RoadChanged += (sender, e) => state.Text = $"Auto: {sender as Car} Silnice: {Enum.GetName(e.RoadType)} " +
			$"Rychlost: {car.RealSpeed} Město: {e.Name}";

		gpx.PreviewMouseDown += (sender, e) => {
			_selected = _cars.First(x => x.id == (ulong)(sender as CarGPX).Tag);
			state.Text = _selected.ToString();

			foreach(Car item in _cars)
				item.RoadChanged -= WriteCarRoadTypeChanged;

			_selected.RoadChanged += WriteCarRoadTypeChanged;
		};
	}

	private void WriteCarRoadTypeChanged(object sender, Car.RoadTypeEventArgs e) =>
		state.Text = $"Auto - {sender as Car}{Environment.NewLine}" +
		$"Poloha - Město: {e.Name} Povolená rychlost: {e.AllowedSpeed}";

	private void B_Left_Click(object sender, RoutedEventArgs e) {
		foreach (UserControl item in roads)
			item.Margin = _dirs[Direction.Left](item.Margin);

		foreach (Car item in _cars)
			item.Margin = _dirs[Direction.Left](item.Margin);
	}

	private void B_Right_Click(object sender, RoutedEventArgs e) {
		foreach (UserControl item in roads)
			item.Margin = _dirs[Direction.Right](item.Margin);

		foreach (Car item in _cars)
			item.Margin = _dirs[Direction.Right](item.Margin);
	}

	private void DrawGPS() {
		foreach (RoadWrapper item in cords) {
			UserControl c = (UserControl)Activator.CreateInstance(item.type);

			c.HorizontalAlignment = HorizontalAlignment.Left;
			c.VerticalAlignment = VerticalAlignment.Center;
			c.Width = item.w / _scale;
			c.Height = item.h / _scale;
			c.Margin = new(item.x / _scale, 0, 0, 0);

			c.Tag = item;

			gr.Children.Add(c);

			roads.Add(c);
		}

		foreach (Car car in _cars) {
			if (car.Width != _carw && car.Height != _carh) {
				car.Margin = new Thickness(car.Margin.Left * _scale, car.Margin.Top, car.Margin.Right, car.Margin.Bottom);
			}

			car.Width = _carw / _scale;
			car.Height = _carh / _scale;

			car.Margin = new Thickness(car.Margin.Left / _scale, car.Margin.Top, car.Margin.Right, car.Margin.Bottom);
		}
	}

	private void InitCords() {
		string s = Headquaters.commands["ggps"]("");

		Array.ForEach(s.Replace("\n", "").Replace("\r", "").Replace("\t", "").Split(' '), x => cords.Add(new RoadWrapper(x)));
	}

	private void B_Direction_Click(object sender, RoutedEventArgs e) => _selected.Destination = tb_destination.Text;
}