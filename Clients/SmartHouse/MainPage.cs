using System;
using System.Threading.Tasks;
using Shared;
using Urho;
using Urho.Forms;
using Xamarin.Forms;
using Application = Xamarin.Forms.Application;
using Color = Xamarin.Forms.Color;

namespace SmartHome
{
	public class MainPage : ContentPage
	{
        private static Plugin.Settings.Abstractions.ISettings AppSettings => Plugin.Settings.CrossSettings.Current;
        readonly StackLayout bulbsStack;
		readonly UrhoSurface urhoSurface;
		readonly ApartmentsDto apartments;
		readonly INetworkSerializer serializer;
		UrhoApp app;
        public string PrimarySessionID { get; set; }

        public MainPage()
        {
            Init(out bulbsStack, out urhoSurface, out serializer);
            apartments = serializer.Deserialize<ApartmentsDto>(
            SmartHouse.SmartHomeSettings.Load());
        }
        public MainPage(ScannerConnection connection, bool offlineMode)
		{
            Init(out bulbsStack, out urhoSurface,out serializer);

			if (!offlineMode)
			{
				apartments = new ApartmentsDto();
				connection.RegisterFor<SurfaceDto>(OnSurfaceReceived);
				connection.RegisterFor<BulbAddedDto>(OnBulbAdded);
				connection.RegisterFor<CurrentPositionDto>(OnCurrentPositionUpdated);
                connection.RegisterFor<NewSessionDto>(OnNewSessionAdded);

                Start();
			}
			else
			{
				//apartments = serializer.Deserialize<ApartmentsDto>(
				//	(byte[])Application.Current.Properties[nameof(ApartmentsDto)]);
				apartments = serializer.Deserialize<ApartmentsDto>(
					SmartHouse.SmartHomeSettings.Load());
			}
		}
        private void Init(
                out StackLayout bulbsStack
                ,out UrhoSurface urhoSurface
                ,out INetworkSerializer serializer
            )
        {

            serializer = new ProtobufNetworkSerializer();
            NavigationPage.SetHasNavigationBar(this, false);

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(4, GridUnitType.Star) });

            bulbsStack = new StackLayout();
            grid.Children.Add(bulbsStack);

            urhoSurface = new UrhoSurface
            {
                BackgroundColor = Color.Black,
                VerticalOptions = LayoutOptions.FillAndExpand
            };

            var stack = new StackLayout
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                Children = { urhoSurface }
            };

            grid.Children.Add(stack);
            Grid.SetColumn(stack, 1);
            Content = grid;
        }

		async void Start()
		{
            try
            {
                while (true)
                {
                    if (SmartHome.App.Save) {
                        lock (apartments)
                            SmartHouse.SmartHomeSettings.Save(serializer.Serialize(apartments));
                        SmartHome.App.Save = false;
                        //Application.Current.Properties[nameof(ApartmentsDto)] = serializer.Serialize(apartments);
                        //await Application.Current.SavePropertiesAsync(); 
                    }
                    await Task.Delay(3000);
                }
            } catch (Exception ex) { string Text = ex.Message; }
		}

		void OnBulbAdded(BulbAddedDto dto)
		{
			lock (apartments)
				apartments.Bulbs.Add(new BulbAddedDto() { scale_factor   = dto.scale_factor, Position = dto.Position, Text = dto.Text, obj_name = dto.obj_name });
            AddBulb(dto.Position, dto.Text, dto.obj_name, dto.scale_factor);
		}

		void AddBulb(Vector3Dto position, string text, string objName, int scaleFactor)
		{
			Urho.Application.InvokeOnMain(() => app?.AddBulb(new Vector3(position.X, position.Y, position.Z),text, objName, scaleFactor));
			Device.BeginInvokeOnMainThread(() =>
			{
				int index = bulbsStack.Children.Count;
				Button button = new Button();
				button.FontSize = 24;
				button.TextColor = Color.Black;
				button.BackgroundColor = new Color(0.8, 0.8, 0.8);
                button.Text = text; // "Bulb " + index;
				button.Clicked += (s, e) =>
				{
					ToggleRealDevice(index);
					Urho.Application.InvokeOnMain(() => app?.ToggleLight(index));
				};
				bulbsStack.Children.Add(button);
			});
		}

		async void ToggleRealDevice(int index)
		{
			// This code is just an example of how to work with some IoT devices, 
			// for example - LIFX bulbs using LifxHttp library https://github.com/mensly/LifxHttpNet

#if !WINDOWS_UWP
			try
			{
				// generate a new token at https://cloud.lifx.com/settings
				var client = new LifxHttp.LifxClient("your token here");
				var lights = await client.ListLights();
				if (index < lights.Count)
					await lights[index].TogglePower();
			}
			catch (Exception exc) { }
#endif
		}

		void OnCurrentPositionUpdated(CurrentPositionDto dto)
		{
			Urho.Application.InvokeOnMain(() => app?.UpdateCurrentPosition(
                dto.SessionID,
				new Vector3(dto.Position.X, dto.Position.Y, dto.Position.Z),
				new Vector3(dto.Direction.X, dto.Direction.Y, dto.Direction.Z)));
		}

		void OnNewSessionAdded(NewSessionDto dto)
		{
            if (PrimarySessionID == string.Empty )
            {
                PrimarySessionID = dto.SessionID;
            }
            Urho.Application.InvokeOnMain(() => app?.AddHumanNode(dto.SessionID));
        }

		protected override void OnAppearing()
		{
			base.OnAppearing();
			StartUrhoApp();
		}

		void OnSurfaceReceived(SurfaceDto surface)
		{
			lock (apartments)
				apartments.Surfaces[surface.Id] = surface;
			Urho.Application.InvokeOnMain(() => app?.AddOrUpdateSurface(surface));
		}

		async void StartUrhoApp()
		{
            if (app == null)
            {
                app = await urhoSurface.Show<UrhoApp>(new Urho.ApplicationOptions(assetsFolder: "Data"));
                app.PrimarySessionID = PrimarySessionID;

                Urho.Application.InvokeOnMain(() =>
                    {
                        foreach (var surface in apartments.Surfaces)
                            app.AddOrUpdateSurface(surface.Value);
                        foreach (var bulb in apartments.Bulbs)
                            AddBulb(bulb.Position, bulb.Text, bulb.obj_name, bulb.scale_factor);
                    });
            }
		}
	}
}
