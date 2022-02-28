using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace ATNC;

public partial class MainWindow : Window {
	private enum Direction { Left, Right, Up, Down }

	public readonly List<RoadsWrapper> cords = new();
	public readonly List<UserControl> roads = new();
	private const double _advance = 10d;
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
	private bool _done = true;
	private string _prev;
	private uint _scale = 1u, _lastscale = 1u;
	private double _ychange = 0d;

	public MainWindow() {
		InitCords();
		InitializeComponent();

		_cartimer.Interval = new System.TimeSpan(0, 0, 0, 0, 10);
		_cartimer.Start();
		_cartimer.Tick += (sender, e) => _cars.ForEach(x => x.Drive());
	}

	private void B_Add_Car(object sender, RoutedEventArgs e) {
		CarGPX gpx = new() {
			Name = "car",
			HorizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment = VerticalAlignment.Top,
			RenderTransformOrigin = new Point(0.5d, 0.5d)
		};

		gr.Children.Add(gpx);
		Grid.SetZIndex(gpx, 9);
		gpx.RenderTransform = new TransformGroup();
		(gpx.RenderTransform as TransformGroup).Children.Add(new RotateTransform(0));

		Car car = new(gpx) {
			RealSpeed = 50,
			Margin = new Thickness(0, 0, 0, 0),
			Width = _carw / _scale,
			Height = _carh / _scale
		};

		car.MapSpeed = car.RealSpeed / (double)_scale;
		_cars.Add(car);

		car.RoadChanged += (sender, e) => {
			car.RealSpeed = e.RecommendedSpeed;
			car.MapSpeed = car.RealSpeed / (double)_scale;
			state.Text = $"{sender as Car} {Enum.GetName(e.RoadType)} {car.RealSpeed} {car.MapSpeed}";
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
		while (roads.Count != 0) {
			gr.Children.Remove(roads[0]);
			roads.RemoveAt(0);
		}

		foreach (RoadsWrapper item in cords) {
			UserControl c = (UserControl)Activator.CreateInstance(item.type);

			c.HorizontalAlignment = HorizontalAlignment.Left;
			c.VerticalAlignment = VerticalAlignment.Top;
			c.Width = item.w / _scale;
			c.Height = item.h / _scale;
			c.Margin = new(item.x / _scale, (item.y / _scale) + _ychange, 0, 0);
			c.RenderTransform = new RotateTransform(item.angle, 0, c.Height / 2);

			c.Tag = item.name;

			gr.Children.Add(c);

			roads.Add(c);
		}

		foreach (Car car in _cars) {
			if (car.Width != _carw && car.Height != _carh) {
				car.Margin = car.GetRotation() == 0
					? new Thickness(car.Margin.Left * _lastscale, car.Margin.Top, car.Margin.Right, car.Margin.Bottom)
					: new Thickness(car.Margin.Left * _lastscale, car.Margin.Top + (_carh - car.Height), car.Margin.Right, car.Margin.Bottom);
			}

			car.Width = _carw / _scale;
			car.Height = _carh / _scale;

			car.Margin = car.GetRotation() == 0d
				? new Thickness(car.Margin.Left / _scale, car.Margin.Top, car.Margin.Right, car.Margin.Bottom)
				: new Thickness(car.Margin.Left / _scale, car.Margin.Top - (_carh - car.Height), car.Margin.Right, car.Margin.Bottom);
			car.MapSpeed = (car.RealSpeed / (double)_scale);
		}
	}

	private void InitCords() {
		Process psi = new() {
			StartInfo = new ProcessStartInfo {
				FileName = @"C:\Users\pruch\source\repos\ATNC.Headquarters\bin\Debug\net6.0\ATNC.Headquarters.exe",
				Arguments = "ggps",
				UseShellExecute = false,
				RedirectStandardOutput = true,
				CreateNoWindow = true
			}
		};

		psi.Start();

		string s = "";

		while (!psi.StandardOutput.EndOfStream)
			s += psi.StandardOutput.ReadLine();

		Array.ForEach(s.Replace("\n", "").Replace("\r", "").Replace("\t", "").Split(' '), x => cords.Add(new RoadsWrapper(x)));
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