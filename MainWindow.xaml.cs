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

	public MainWindow() {
		InitializeComponent();
		_car = new Car(car);
		_cartimer.Interval = new System.TimeSpan(0, 0, 0, 0, 10);
		_cartimer.Start();
		_car.Speed = 50;
		_cartimer.Tick += (sender, e) => _car.Drive();
	}

	uint _scale = 1u;

	private void T_Size_TextChanged(object sender, TextChangedEventArgs e) {
		TextBox tb = (sender as TextBox);
		tb.Text = tb.Text.Replace(" ", "");

		for (int i = 0; i < tb.Text.Length; ++i)
			if (!char.IsDigit(tb.Text[i])) {
				tb.Text = "1";
				break;
			}

		if(tb.Text is "0" or "")
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

		while(_roads.Count != 0)
			_roads.RemoveAt(0);

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
	}
}