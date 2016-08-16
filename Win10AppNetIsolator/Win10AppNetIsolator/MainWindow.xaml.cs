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

			var items = mappingsKey.GetSubKeyNames().Select( _ => mappingsKey.OpenSubKey( _ ) )
						.Select( _ => new MappingListItem { Key = _.Name, Value = _.GetValue( "DisplayName" )?.ToString() } )
						.Where( _ => !_.Value.StartsWith( "@" ) )
						.Where( _ => !_.Value.Contains( "-" ) || !Guid.TryParse( _.Value, out useless ) )
						.OrderBy( _ => _.Value )
						.ToList();

			listBox.ItemsSource = items;
		}
	}
}
