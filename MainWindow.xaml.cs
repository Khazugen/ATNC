using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ATNC;

public partial class MainWindow : Window {
	private readonly Car _car;
	private readonly DispatcherTimer _cartimer = new();
	private readonly List<UserControl> _roads = new();
	private readonly List<Car> _cars = new();
	private const double _carw = 200;
	private const double _carh = 100;

	public MainWindow() {
		InitializeComponent();
		_car = new Car(car);
		_cartimer.Interval = new System.TimeSpan(0, 0, 0, 0, 10);
		_cartimer.Start();
		_car.Speed = 10;
		_cartimer.Tick += (sender, e) => _car.Drive();
		_cars.Add(_car);
	}

	private uint _scale = 1u;

	private void T_Size_TextChanged(object sender, TextChangedEventArgs e) {
		TextBox tb = (sender as TextBox);
		tb.Text = tb.Text.Replace(" ", "");

		for (int i = 0; i < tb.Text.Length; ++i)
			if (!char.IsDigit(tb.Text[i])) {
				tb.Text = "1";
				break;
			}

		if (tb.Text is "0" or "")
			tb.Text = "1";

		try {
			_scale = uint.Parse(tb.Text);
		} catch (OverflowException) {
			tb.Text = "1";
		}

		DrawGPS();
	}

	private const string _str = "n0;0;1000;200 b1000;0;300;200";
	private readonly Dictionary<char, Type> _types = new() {
		{ 'n', typeof(Road) },
		{ 'b', typeof(Bridge) },
		{ 't', typeof(Tunnel) }
	};

	private void DrawGPS() {
		//
		// Získá z centrály silnice a postaví jej na základě měřítka
		//

		while (_roads.Count != 0) {
			gr.Children.Remove(_roads[0]);
			_roads.RemoveAt(0);
		}

		foreach (string item in _str.Split(' ')) {
			UserControl c = (UserControl)Activator.CreateInstance(_types[item[0]]);

			string[] spl = item.Split(';');
			spl[0] = spl[0].Remove(0, 1);

			double[] spld = spl.Select(x => double.Parse(x)).ToArray();

			c.HorizontalAlignment = HorizontalAlignment.Left;
			c.VerticalAlignment = VerticalAlignment.Top;
			c.Width = spld[2] / _scale;
			c.Height = spld[3] / _scale;
			c.Margin = new(spld[0] / _scale, spld[1] / _scale, 0, 0);

			gr.Children.Add(c);

			_roads.Add(c);
		}

		foreach (Car car in _cars) {
			car.Width = _carw / _scale;
			car.Height = _carh / _scale;
		}
	}

	private enum Direction { Left, Right, Up, Down }

	private readonly Dictionary<Direction, Func<Thickness, Thickness>> _dirs = new() {
		{ Direction.Left, (item) => new Thickness(item.Left - 10d, item.Top, item.Right, item.Bottom) },
		{ Direction.Right, (item) => new Thickness(item.Left + 10d, item.Top, item.Right, item.Bottom) },
		{ Direction.Up, (item) => new Thickness(item.Left, item.Top - 10d, item.Right, item.Bottom) },
		{ Direction.Down, (item) => new Thickness(item.Left, item.Top + 10d, item.Right, item.Bottom) }
	};

	private void B_Left(object sender, RoutedEventArgs e) {
		foreach (UserControl item in _roads)
			item.Margin = _dirs[Direction.Left](item.Margin);

		foreach (Car item in _cars)
			item.Margin = _dirs[Direction.Left](item.Margin);
	}

	private void B_Right(object sender, RoutedEventArgs e) {
		foreach (UserControl item in _roads)
			item.Margin = _dirs[Direction.Right](item.Margin);

		foreach (Car item in _cars)
			item.Margin = _dirs[Direction.Right](item.Margin);
	}

	private void B_Down(object sender, RoutedEventArgs e) {
		foreach (UserControl item in _roads)
			item.Margin = _dirs[Direction.Down](item.Margin);

		foreach (Car item in _cars)
			item.Margin = _dirs[Direction.Down](item.Margin);
	}

	private void B_Up(object sender, RoutedEventArgs e) {
		foreach (UserControl item in _roads)
			item.Margin = _dirs[Direction.Up](item.Margin);

		foreach (Car item in _cars)
			item.Margin = _dirs[Direction.Up](item.Margin);
	}
}