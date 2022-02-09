using System.Windows;
using System.Windows.Threading;

namespace ATNC;

public partial class MainWindow : Window {
	private readonly Car _car;
	private readonly DispatcherTimer _cartimer = new();

	public MainWindow() {
		InitializeComponent();
		_car =  new Car(car);
		_cartimer.Interval = new System.TimeSpan(0, 0, 0, 0, 10);
		_cartimer.Start();
		_car.Speed = 50;
		_cartimer.Tick += (sender, e) => _car.Drive();
	}
}