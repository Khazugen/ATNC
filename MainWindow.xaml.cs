using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace ATNC;

public partial class MainWindow : Window {
	private enum Direction { Left, Right, Up, Down }

	private const double _advance = 10d;
	private const double _carh = 100;
	private const double _carw = 200;
	private const string _str = "n500;0;1000;200;0 b500+1000;-25;3000;250;0 n3000+500+1000;0;10000;200;0 t3000+500+1000+10000;0;3000;200;0";
	private readonly Car _car;
	private readonly List<Car> _cars = new();
	private readonly DispatcherTimer _cartimer = new();
	private static readonly Dictionary<Direction, Func<Thickness, Thickness>> _dirs = new() {
		{ Direction.Left, (item) => new Thickness(item.Left - _advance, item.Top, item.Right, item.Bottom) },
		{ Direction.Right, (item) => new Thickness(item.Left + _advance, item.Top, item.Right, item.Bottom) },
		{ Direction.Up, (item) => new Thickness(item.Left, item.Top - _advance, item.Right, item.Bottom) },
		{ Direction.Down, (item) => new Thickness(item.Left, item.Top + _advance, item.Right, item.Bottom) }
	};

	public readonly List<RoadsWrapper> cords = new();
	public readonly List<UserControl> roads = new();

	private bool _done = true;
	private string _prev;
	private uint _scale = 1u, _lastscale = 1u;
	private double _ychange = 0d;

	public MainWindow() {
		InitializeComponent();

		_car = new Car(car);
		_cartimer.Interval = new System.TimeSpan(0, 0, 0, 0, 10);
		_cartimer.Start();
		_car.RealSpeed = 50;
		_car.MapSpeed = _car.RealSpeed / _scale;
		_cartimer.Tick += (sender, e) => _car.Drive();
		_cars.Add(_car);
		_car.RoadChanged += (sender, e) => {
			_car.RealSpeed = e.RecommendedSpeed;
			_car.MapSpeed = _car.RealSpeed / (double)_scale;
			state.Text = $"{sender as Car} {Enum.GetName(e.RoadType)} {_car.RealSpeed} {_car.MapSpeed}";
		};
	}

	private void B_Down(object sender, RoutedEventArgs e) {
		foreach (UserControl item in roads)
			item.Margin = _dirs[Direction.Down](item.Margin);

		foreach (Car item in _cars)
			item.Margin = _dirs[Direction.Down](item.Margin);

		_ychange += _advance;
	}

	private void B_Left(object sender, RoutedEventArgs e) {
		foreach (UserControl item in roads)
			item.Margin = _dirs[Direction.Left](item.Margin);

		foreach (Car item in _cars)
			item.Margin = _dirs[Direction.Left](item.Margin);
	}

	private void B_Right(object sender, RoutedEventArgs e) {
		foreach (UserControl item in roads)
			item.Margin = _dirs[Direction.Right](item.Margin);

		foreach (Car item in _cars)
			item.Margin = _dirs[Direction.Right](item.Margin);
	}

	private void B_Up(object sender, RoutedEventArgs e) {
		foreach (UserControl item in roads)
			item.Margin = _dirs[Direction.Up](item.Margin);

		foreach (Car item in _cars)
			item.Margin = _dirs[Direction.Up](item.Margin);

		_ychange -= _advance;
	}

	private void DrawGPS() {
		//
		// Získá z centrály silnice a postaví jej na základě měřítka
		//

		while (roads.Count != 0) {
			gr.Children.Remove(roads[0]);
			roads.RemoveAt(0);
		}

		cords.Clear();

		foreach (string item in _str.Split(' ')) {
			cords.Add(new RoadsWrapper(item));

			UserControl c = (UserControl)Activator.CreateInstance(cords[^1].type);

			c.HorizontalAlignment = HorizontalAlignment.Left;
			c.VerticalAlignment = VerticalAlignment.Top;
			c.Width = cords[^1].w / _scale;
			c.Height = cords[^1].h / _scale;
			c.Margin = new(cords[^1].x / _scale, (cords[^1].y / _scale) + _ychange, 0, 0);
			c.RenderTransform = new RotateTransform(cords[^1].angle, c.Width / 2, c.Height / 2);

			gr.Children.Add(c);

			roads.Add(c);
		}

		foreach (Car car in _cars) {
			Thickness margin = car.Margin;

			if (car.Width != _carw && car.Height != _carh)
				margin = new Thickness(margin.Left * _lastscale, margin.Top + (_carh - car.Height), margin.Right, margin.Bottom);

			car.Width = _carw / _scale;
			car.Height = _carh / _scale;
			car.Margin = new Thickness(margin.Left / _scale, margin.Top - (_carh - car.Height), margin.Right, margin.Bottom);
			car.MapSpeed = (car.RealSpeed / (double)_scale);
		}
	}

	private void T_Size_TextChanged(object sender, TextChangedEventArgs e) {
		TextBox tb = (sender as TextBox);

		if (!_done) {
			tb.Text = _prev;
			return;
		}

		_prev = tb.Text;
		_done = false;

		tb.Text = tb.Text.Replace(" ", "");

		for (int i = 0; i < tb.Text.Length; ++i)
			if (!char.IsDigit(tb.Text[i])) {
				tb.Text = _prev = "1";
				break;
			}

		if (tb.Text is "0" or "")
			tb.Text = _prev = "1";

		try {
			_lastscale = _scale;
			_scale = uint.Parse(tb.Text);
		} catch (OverflowException) {
			tb.Text = _prev = "1";
		}

		DrawGPS();
		_done = true;
	}
}