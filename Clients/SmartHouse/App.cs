using SmartHouse;
using Xamarin.Forms;

namespace SmartHome
{
	public class App : Application
	{
        public static bool Save { get; set; }
        public static ScannerConnection connection { get; set; }
        public static MasterDetail MasterDetailPage { get; set; }

        public App ()
		{
            // The root page of your application
            //MainPage = new NavigationPage(new QrPage());
            Shared.SmartHomeService.SmartHomeService proxy = new Shared.SmartHomeService.SmartHomeService();
            proxy.SetURI("http://localhost/SmartHomeService/SmartHomeService.svc/rest");
            //proxy.Reset().GetAwaiter().GetResult();
            MasterDetailPage = new MasterDetail();
            MainPage = MasterDetailPage;
        }

		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}
