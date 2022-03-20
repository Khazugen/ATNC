using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ATNC;

public partial class Tab : Window {
	public readonly List<RoadWrapper> cords = new();
	public readonly List<UserControl> roads = new();
	private const double _advance = 50d;
	private const double _carh = 100;
	private const double _carw = 200;

	private double _xadvance;
	private readonly List<Car> _cars = new();
	private readonly List<CarGPX> _cargpxs = new();
	private readonly DispatcherTimer _cartimer = new();
	public const uint scale = 20u;
	private Car _selected;

	public Tab() {
		InitCords();
		InitializeComponent();
		DrawGPS();

		_cartimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
		_cartimer.Start();
		_cartimer.Tick += (sender, e) => {
			for (int i = 0; i < _cars.Count; ++i) {
				_cars[i].Drive();
				_cargpxs[i].Margin = new Thickness(_xadvance + (_cars[i].X / scale), _cargpxs[i].Margin.Top, _cargpxs[i].Margin.Right, _cargpxs[i].Margin.Bottom);
			}
		};

		Headquaters.MeteoStation.WeatherChanged += (e) => l_temp.Content = $"{e.Weather}°C";
	}

	private void B_Add_Car_Click(object sender, RoutedEventArgs e) {
		Car car = new(cords) {
			RealSpeed = 50,
			Width = _carw / scale,
			Height = _carh / scale
		};

		_cars.Add(car);

		car.RoadChanged += (sender, e) => state.Text = $"Auto: {sender as Car} Silnice: {Enum.GetName(e.RoadType)} " +
			$"Rychlost: {car.RealSpeed} Město: {e.Name}";

		CarGPX gpx = new() {
			Name = "car",
			HorizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment = VerticalAlignment.Center,
			RenderTransformOrigin = new Point(0.5d, 0.5d),
			Width = car.Width,
			Height = car.Height
		};

		gr.Children.Add(gpx);
		Panel.SetZIndex(gpx, 9);

		gpx.Tag = car.id;

		gpx.PreviewMouseDown += (sender, e) => {
			_selected = _cars.First(x => x.id == (ulong)(sender as CarGPX).Tag);
			state.Text = _selected.ToString();

			foreach (Car item in _cars)
				item.RoadChanged -= WriteCarRoadTypeChanged;

			_selected.RoadChanged += WriteCarRoadTypeChanged;
		};
		_cargpxs.Add(gpx);
	}

	private void WriteCarRoadTypeChanged(object sender, Car.RoadTypeEventArgs e) =>
		state.Text = $"Auto - {sender as Car}{Environment.NewLine}" +
		$"Poloha - Město: {e.Name} Povolená rychlost: {e.AllowedSpeed}";

	private void B_Left_Click(object sender, RoutedEventArgs e) {
		foreach (UserControl item in roads)
			item.Margin = new Thickness(item.Margin.Left - _advance, item.Margin.Top, item.Margin.Right, item.Margin.Bottom);

		foreach (Car item in _cars)
			_xadvance -= _advance;
	}

	private void B_Right_Click(object sender, RoutedEventArgs e) {
		foreach (UserControl item in roads)
			item.Margin = new Thickness(item.Margin.Left + _advance, item.Margin.Top, item.Margin.Right, item.Margin.Bottom);

		foreach (Car item in _cars)
			_xadvance += _advance;
	}

	private void DrawGPS() {
		foreach (RoadWrapper item in cords) {
			UserControl c = (UserControl)Activator.CreateInstance(item.type);

			c.HorizontalAlignment = HorizontalAlignment.Left;
			c.VerticalAlignment = VerticalAlignment.Center;
			c.Width = item.w / scale;
			c.Height = item.h / scale;
			c.Margin = new(item.x / scale, 0, 0, 0);

			c.Tag = item;

			gr.Children.Add(c);

			roads.Add(c);
		}

		foreach (Car car in _cars) {
			car.Width = _carw / scale;
			car.Height = _carh / scale;
		}
	}

	private void InitCords() {
		string s = Headquaters.GetGPS();

		Array.ForEach(s.Replace("\n", "").Replace("\r", "").Replace("\t", "").Split(' '), x => cords.Add(new RoadWrapper(x)));
	}

	private void B_Direction_Click(object sender, RoutedEventArgs e) => _selected.Destination = tb_destination.Text;
}