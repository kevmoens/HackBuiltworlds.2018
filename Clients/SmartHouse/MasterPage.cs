using SmartHome;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace SmartHouse
{
    class MasterPage : ContentPage
    {
        public ListView ListView { get { return listView; } }

        ListView listView;

        public MasterPage()
        {


            Title = "Smart Home";

            Icon = "hamburger.png";
            var itemDataTemplate = new DataTemplate(() =>
            {
                var grid = new Grid();
   
            var nameLabel = new Label { FontAttributes = FontAttributes.Bold };

                nameLabel.SetBinding(Label.TextProperty, "Title");

                grid.Children.Add(nameLabel);

                return new ViewCell { View = grid };
            });



            var masterPageItems = new List<MasterPageItem>();
            try
            {
                if (SmartHouse.SmartHomeSettings.Exist())
                {
                    masterPageItems.Add(new MasterPageItem
                    {
                        Title = "Load",
                        IconSource = "map.png",
                        TargetType = typeof(MainPage)
                    });
                }
            }
            catch (Exception ex) { string Text = ex.Message; }

            masterPageItems.Add(new MasterPageItem
            {
                Title = "Connect",
                IconSource = "connect.png",
                TargetType = typeof(QrPage)
            });
            masterPageItems.Add(new MasterPageItem
            {
                Title = "Save",
                IconSource = "save.png"
            });


            listView = new ListView { ItemsSource = masterPageItems, ItemTemplate = itemDataTemplate, Margin = new Thickness(0, 20, 0, 0) };
            
            Content = new StackLayout
            {
                Margin = new Thickness(20),
                VerticalOptions = LayoutOptions.FillAndExpand,
                Children = {

               listView
            }
            };
            Appearing += MasterPage_Appearing;
        }

        private async void MasterPage_Appearing(object sender, EventArgs e)
        {

            var ip = await SmartHome.App.connection.GetLocalIp() ?? "ERROR";
            await SmartHome.App.connection.WaitForCompanion();
        }
    }
    public class MasterPageItem
    {
        public string Title { get; set; }
        public string IconSource { get; set; }
        public Type TargetType { get; set; }
    }
}
