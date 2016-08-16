using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Diagnostics;

namespace Win10AppNetIsolator
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			this.Loaded += MainWindow_Loaded;
		}

		private void MainWindow_Loaded( object sender, RoutedEventArgs e )
		{
			var mappingsKey = Registry.CurrentUser.OpenSubKey( @"Software\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppContainer\Mappings" );

			Guid useless;

			var appIdPrefix = @"HKEY_CURRENT_USER\Software\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppContainer\Mappings\";
			var items = mappingsKey.GetSubKeyNames().Select( _ => mappingsKey.OpenSubKey( _ ) )
						.Select( _ => new MappingListItem { Key = _.Name.Substring( appIdPrefix.Length ), Value = _.GetValue( "DisplayName" )?.ToString() } )
						.Where( _ => !_.Value.StartsWith( "@" ) )
						.Where( _ => !_.Value.Contains( "-" ) || !Guid.TryParse( _.Value, out useless ) )
						.OrderBy( _ => _.Value )
						.ToList();

			listBox.ItemsSource = items;
		}

		private void AppBoxSelectionChanged( object sender, SelectionChangedEventArgs e )
		{
			var selectedItem = listBox.SelectedItem as MappingListItem;
			if ( selectedItem == null ) return;

			commandBox.Text = $"CheckNetIsolation.exe loopbackexempt -a -p={selectedItem.Key}";
		}

		private void ExecuteBtnClick( object sender, RoutedEventArgs e )
		{
			try
			{
				button.IsEnabled = false;

				var fileName = "CheckNetIsolation.exe";
				var arguments = commandBox.Text.Substring( fileName.Length );

				var startInfo = new ProcessStartInfo( fileName, arguments );
				startInfo.RedirectStandardInput = true;
				startInfo.RedirectStandardOutput = true;
				startInfo.UseShellExecute = false;
				startInfo.CreateNoWindow = true;

				var p = Process.Start( startInfo );
				p.OutputDataReceived += MainWindow_OutputDataReceived;

				p.BeginOutputReadLine();
				p.WaitForExit();
			}
			finally
			{
				button.IsEnabled = true;
			}
		}

		private async void MainWindow_OutputDataReceived( object sender, DataReceivedEventArgs e )
		{
			await Dispatcher.InvokeAsync( () => outputBox.Text += e.Data + Environment.NewLine );
		}
	}
}
