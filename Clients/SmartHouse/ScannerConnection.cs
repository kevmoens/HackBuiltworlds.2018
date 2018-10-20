using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shared;
using Sockets.Plugin;
using Sockets.Plugin.Abstractions;
using Xamarin.Forms;

namespace SmartHome
{
	public class ScannerConnection
	{
		int Port = 5206;
		INetworkSerializer networkSerializer;
		TcpSocketListener listener;
		List<ITcpSocketClient> clients = new List<ITcpSocketClient>();
		Dictionary<Type, Action<object>> callbacks = new Dictionary<Type, Action<object>>();

		public async Task WaitForCompanion()
		{
			networkSerializer = new ProtobufNetworkSerializer();
			var tcs = new TaskCompletionSource<bool>();
			listener = new TcpSocketListener();
			listener.ConnectionReceived += async (s, e) =>
			{
                //Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
                //{
                //    if (SmartHome.App.MasterDetailPage.Detail.Navigation.NavigationStack.Any(p => p.GetType().Name == "MainPage"))
                //    {
                //        SmartHome.App.MasterDetailPage.Detail.Navigation.PopToRootAsync(); //Go Back to Main Page
                //    }
                //    else
                //    {
                //        SmartHome.App.MasterDetailPage.Detail = new NavigationPage(new MainPage(SmartHome.App.connection, false));
                //    }
                //    //SmartHome.App.MasterDetailPage.Detail.Navigation.PushAsync(new MainPage(SmartHome.App.connection, false));
                //});


                networkSerializer.ObjectDeserialized += SimpleNetworkSerializerObjectDeserialized;
				tcs.TrySetResult(true);
                clients.Add(e.SocketClient);
                try
                {
                    networkSerializer.ReadFromStream(e.SocketClient.ReadStream);
                }
                catch (Exception exc)
                {
                    //show error?
                }
			};
            while (true)
            {
                try
                {
                    await listener.StartListeningAsync(Port);
                    break;
                } catch { Port += 1; }
            }
			await tcs.Task;
		}

		public void Send(BaseDto dto)
		{
            foreach (ITcpSocketClient client in clients)
            {
                try
                {
                    networkSerializer.WriteToStream(client.WriteStream, dto);
                }
                catch (Exception exc)
                {
                    //show error?
                }
            }
		}

		public void RegisterFor<T>(Action<T> callback)
		{
			lock (callbacks)
			{
				callbacks[typeof(T)] = obj => callback((T) obj);
			}
		}

		public async Task<string> GetLocalIp()
		{
			var interfaces = await CommsInterface.GetAllInterfacesAsync();
			//TODO: check if any
			return interfaces.Last(i => !i.IsLoopback && i.IsUsable).IpAddress + ":" + Port;
		}

		void SimpleNetworkSerializerObjectDeserialized(object obj)
		{
			if (obj == null)
				return;

			lock (callbacks)
			{
				Action<object> callback;
				if (callbacks.TryGetValue(obj.GetType(), out callback))
				{
					callback(obj);
				}
			}
		}
	}
}
