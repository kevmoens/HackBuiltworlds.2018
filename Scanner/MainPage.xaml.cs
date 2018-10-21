using System;
using System.ComponentModel;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;


namespace SmartHome.HoloLens
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page, INotifyPropertyChanged
	{		
	    public MainPage()
	    {
	        InitializeComponent();
	        NavigationCacheMode = NavigationCacheMode.Required;
            DataContext = this;
            Program.XamlPage = this;
	    }
        public bool ShowElectrical { get { return Program.UrhoApp.ShowElectrical; } set { Program.UrhoApp.ShowElectrical = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ShowElectrical")); } }
        public bool ShowPlumbing { get { return Program.UrhoApp.ShowPlumbing; } set { Program.UrhoApp.ShowPlumbing = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ShowPlumbing"));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        private async void OnSwitch2UrhoButton_Clicked(object sender, RoutedEventArgs e)
        {
            await ApplicationViewSwitcher.SwitchAsync(App.View3D.Id);
        }	  
        private void ElectricalToggled(object sender, RoutedEventArgs e)
        {
            Program.UrhoApp.ShowElectrical = tgElectrical.IsOn;
        }
        private void PlumbingToggled(object sender, RoutedEventArgs e)
        {

            Program.UrhoApp.ShowPlumbing = tgPlumbing.IsOn;
        }
    }
}
