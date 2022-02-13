using System.Windows;
using System.Windows.Controls;

namespace ATNC;

public partial class Road : UserControl {
	public Road() => InitializeComponent();

	private void Resize(object sender, SizeChangedEventArgs e) => 
		gr.RowDefinitions[1].Height = new((gr.RowDefinitions[0].ActualHeight + gr.RowDefinitions[2].ActualHeight) / 10d);
}