using System;
using Shared;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;
using System.Linq;

namespace SmartHome
{
	public class QrPage : ContentPage
	{
		public QrPage()
		{
			NavigationPage.SetHasNavigationBar(this, false);
			Initialize();
		}

		async void Initialize()
		{
			var ip = await SmartHome.App.connection.GetLocalIp() ?? "ERROR";

			Button offlineModeBtn = new Button
				{
					Text = "Offline mode",
					BackgroundColor = new Color(0.8f, 0.8f, 0.8f)
				};
			offlineModeBtn.Clicked += OnOfflineModeClicked;

			var qrSize = 320;
			var barcode = new ZXingBarcodeImageView
			{
				WidthRequest = qrSize,
				HeightRequest = qrSize,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				BarcodeFormat = ZXing.BarcodeFormat.QR_CODE,
				BarcodeOptions =
					{
						Width = qrSize,
						Height = qrSize,
					},
				BarcodeValue = ip,
			};
			BackgroundColor = Color.White;
			var stack = new StackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center,
				Children =
				{
					new Label
					{
						TextColor = Color.Black,
						HorizontalTextAlignment = TextAlignment.Center,
						Text = $"Open SmartHome for HoloLens companion app and\nscan this qr code in order to be connected ({ip}):"
					},
					barcode,
				}
			};

			Content = stack;
            try
            {
                //if (Application.Current.Properties.ContainsKey(nameof(ApartmentsDto)))
                if (SmartHouse.SmartHomeSettings.Exist())
                {
                    stack.Children.Add(offlineModeBtn);
                }
            }
            catch (Exception ex) { string Text = ex.Message; }
            //         var connection = new ScannerConnection();
            if (SmartHome.App.connection != null)  //Master Page loads the first time
            {
                await SmartHome.App.connection.WaitForCompanion();

                if (SmartHome.App.MasterDetailPage.Detail.Navigation.NavigationStack.Any(p => p.GetType().Name == "MainPage"))
                {
                    await SmartHome.App.MasterDetailPage.Detail.Navigation.PopToRootAsync(); //Go Back to Main Page
                }
                else
                {
                    SmartHome.App.MasterDetailPage.Detail = new NavigationPage(new MainPage(SmartHome.App.connection, false));
                }
            }

           
        }

		async void OnOfflineModeClicked(object sender, EventArgs e)
		{
			await Navigation.PushAsync(new MainPage(null, true));
		}
	}
}
