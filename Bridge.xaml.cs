using System.Windows;
using System.Windows.Controls;

namespace ATNC;
/// <summary>
/// Interakční logika pro Bridge.xaml
/// </summary>
public partial class Bridge : UserControl {
	public Bridge() => InitializeComponent();

	private void Resize(object sender, SizeChangedEventArgs e) {
		double h = gr.RowDefinitions[1].ActualHeight / 5d;
		gr.RowDefinitions[0].Height = new(h);
		gr.RowDefinitions[2].Height = new(h);
	}
}