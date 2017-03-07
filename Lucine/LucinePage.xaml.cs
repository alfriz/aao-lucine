using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;
using ReactiveUI;
using Xamarin.Forms;

namespace Lucine
{
	public partial class LucinePage : ContentPage
	{
		IAdapter adapter;
		public LucinePage()
		{
			InitializeComponent();
			this.BindingContext = new LucineViewModel();
			this.list.ItemSelected += OnDeviceSelected;
		}

		void OnDeviceSelected(object sender, SelectedItemChangedEventArgs e)
		{
			var device = ((sender as ListView).SelectedItem as IDevice);
			ConnectToHMSoft(device.Id);
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();
			try
			{
				adapter = CrossBluetoothLE.Current.Adapter;
				adapter.DeviceDiscovered += (s, a) => (this.BindingContext as LucineViewModel).AddDeviceToList(a.Device);
				await adapter.StartScanningForDevicesAsync();
				foreach (var d in (this.BindingContext as LucineViewModel).DeviceList)
					System.Diagnostics.Debug.WriteLine($"GUID: {d.Id} e name {d.Name}");
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex);
			}
		}

		async void ConnectToHMSoft(Guid guid)
		{
			try
			{
				var connectedDevice = await adapter.ConnectToKnownDeviceAsync(guid);
				var services = await connectedDevice.GetServicesAsync();
				ICharacteristic car = null;
				foreach (var s in services)
				{
					System.Diagnostics.Debug.WriteLine($"Servizio {s.Name} e {s.Id}");
					var characteristics = await s.GetCharacteristicsAsync();
					foreach (var c in characteristics) {
						System.Diagnostics.Debug.WriteLine($"Caratt. {c.Name} e {c.Id}");
					}
					string command = "AT+PIO20";
					car = characteristics.FirstOrDefault();
				}
				Task.Run(async () => {
					while (true) {
						SimulateOn(car, "3");
						SimulateOff(car, "2");
						await Task.Delay(100);
						SimulateOn(car, "2");
						SimulateOff(car, "3");
						await Task.Delay(100);
					}
				});

				//var characteristic = await service.GetCharacteristicAsync(Guid.Parse("d8de624e-140f-4a22-8594-e2216b84a5f2"));

			}
			catch (DeviceConnectionException ex)
			{
				// ... could not connect to device
			}

		}

		async void SimulateOff (ICharacteristic car, string addr) {
			string command = $"AT+PIO{addr}0";
			await car.WriteAsync(Encoding.UTF8.GetBytes(command));
		}

		async void SimulateOn(ICharacteristic car, string addr)
		{
			string command = $"AT+PIO{addr}1";
			await car.WriteAsync(Encoding.UTF8.GetBytes(command));
		}
	}

	public class LucineViewModel : ReactiveObject
	{
		ObservableCollection<IDevice> deviceList;
		public ObservableCollection<IDevice> DeviceList
		{
			get
			{
				return deviceList;
			}

			set
			{
				this.RaiseAndSetIfChanged(ref deviceList, value);
			}
		}

		public LucineViewModel()
		{
			DeviceList = new ObservableCollection<IDevice>();
		}

		public void AddDeviceToList(IDevice dev)
		{
			DeviceList.Add(dev);
		}
	}
}
