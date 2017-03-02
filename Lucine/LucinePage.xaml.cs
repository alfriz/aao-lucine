using System.Collections.Generic;
using MvvmCross.Platform;
using Plugin.BLE.Abstractions.Contracts;
using Xamarin.Forms;

namespace Lucine
{
	public partial class LucinePage : ContentPage
	{
		IAdapter adapter;
		public LucinePage ()
		{
			InitializeComponent ();
		}

		List<IDevice> deviceList = new List<IDevice> ();
		protected override async void OnAppearing ()
		{
			base.OnAppearing ();
			adapter = Mvx.Resolve<IAdapter> ();
			adapter.DeviceDiscovered += (s, a) => deviceList.Add (a.Device);
			await adapter.StartScanningForDevicesAsync ();
			foreach (var d in deviceList)
				System.Diagnostics.Debug.WriteLine ($"GUID: {d.Id} e name {d.Name}");
		}
	}
}
