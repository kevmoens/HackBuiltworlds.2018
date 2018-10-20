using SmartHome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace SmartHouse
{
    public class MasterDetail : MasterDetailPage
    {
        MasterPage masterPage;
        public MasterDetail()
        {

            SmartHome.App.connection = new ScannerConnection();
            masterPage = new MasterPage();
            Master = masterPage;
            Detail = new NavigationPage(new QrPage());
            //Title = "Smart Home";
            masterPage.ListView.ItemSelected += OnItemSelected;

            if (Device.OS == TargetPlatform.Windows)
            {
                Master.Icon = "swap.png";
                MasterBehavior = MasterBehavior.Popover; // Added this line of code/
            }
            IsPresented = true;
        }
        void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var item = e.SelectedItem as MasterPageItem;
            if (item != null)
            {
                if (item.Title == "Save")
                {
                    SmartHome.App.Save = true;
                }
                else
                {
                    Page newPage = (Page)Activator.CreateInstance(item.TargetType);


                    if (SmartHome.App.MasterDetailPage.Detail.Navigation.NavigationStack.Any(p => p.GetType().Name == "MainPage"))
                    {
                        SmartHome.App.MasterDetailPage.Detail.Navigation.PushAsync(newPage);
                    }
                    else
                    {
                        SmartHome.App.MasterDetailPage.Detail = new NavigationPage(newPage);
                    }

                }
                masterPage.ListView.SelectedItem = null;
                IsPresented = false;
            }
        }
    }
}
