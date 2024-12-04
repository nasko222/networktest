using System.Diagnostics;

internal class NetworkSession{
	public NetworkSession(Subnet subnet) => this.GetSubnet = subnet;
	public Subnet GetSubnet{ get; }
	
	public void SetIPFlag(string argument, bool occupied){
		try {
			IPAddress ip = IPAddress.Create(argument);
			
			if (!this.GetSubnet.Addresses.Contains(ip)) {
				if (ip.Equals(this.GetSubnet.SubnetAddress) || ip.Equals(this.GetSubnet.BroadcastAddress)) {
					Console.WriteLine($"You cannot {(occupied ? "occupy" : "free")} network, gateway or broadcast.");
					return;
				}
				
				Console.WriteLine($"The IP address {ip} is not part of this subnet.");
				return;
			}
			
			if (ip.Equals(this.GetSubnet.DefaultGateway)) {
				Console.WriteLine($"You cannot {(occupied ? "occupy" : "free")} network, gateway or broadcast.");
				return;
			}

			if(this.GetSubnet.OccupiedFlag[this.GetSubnet.Addresses.IndexOf(ip)] == occupied){
				Console.WriteLine($"{ip} is already {(occupied ? "occupied" : "free")}.");
				return;
			}
			this.GetSubnet.OccupiedFlag[this.GetSubnet.Addresses.IndexOf(ip)] = occupied;
			
			Console.WriteLine($"{(occupied ? "Occupied" : "Freed")} {ip}.");
		} catch (FormatException) {
			Console.WriteLine("Invalid IP address format.");
		} catch (Exception ex) {
			Console.WriteLine(ex.Message);
		}
	}

	public void ShowAddresses(string argument){
		if(!int.TryParse(argument, out int index) || index < 0) index = 0;
		
		for(int i = 0; i < RuntimeSettings.MapLength; i++){
			try{
				switch(i + index){
					case 0:
						Console.WriteLine($"{this.GetSubnet.SubnetAddress} - network");
						continue;

					case 1:
						Console.WriteLine($"{this.GetSubnet.DefaultGateway} - gateway");
						continue;
				}

				if(i + index == this.GetSubnet.Addresses.Count + 1){
					Console.WriteLine($"{this.GetSubnet.BroadcastAddress} - broadcast");
					continue;
				}
				
				if(i + index >= this.GetSubnet.Addresses.Count + 1){
					continue;
				}
				
				string status = this.GetSubnet.OccupiedFlag[i + index - 1] ? "occupied" : "free";
				IPAddress address = this.GetSubnet.Addresses[i + index - 1];
				Console.WriteLine($"{address} - {status}");
			} catch(IndexOutOfRangeException ex){
				Console.WriteLine(ex.Message);
			} catch(Exception ex){
				Console.WriteLine(ex.Message);
			}

		}
	}

	public void QueryAddress(string argument) {
		try {
			IPAddress ip = IPAddress.Create(argument);
			
			if (!this.GetSubnet.Addresses.Contains(ip)) {
				if(ip.Equals(this.GetSubnet.SubnetAddress)){
					Console.WriteLine($"{ip} - network");
					return;
				}
				if(ip.Equals(this.GetSubnet.BroadcastAddress)){
					Console.WriteLine($"{ip} - broadcast");
					return;
				}
				Console.WriteLine($"The IP address {ip} is not part of this subnet.");
				return;
			}
			
			if (ip.Equals(this.GetSubnet.DefaultGateway)) {
				Console.WriteLine($"{ip} - gateway");
				return;
			}
			
			Console.WriteLine(this.GetSubnet.OccupiedFlag[this.GetSubnet.Addresses.IndexOf(ip)] ? $"{ip} - occupied" : $"{ip} - free");
		} catch (FormatException) {
			Console.WriteLine("Invalid IP address format.");
		} catch (Exception ex) {
			Console.WriteLine(ex.Message);
		}
	}
	
	public void Assign(int addresses){
		bool flag = false;
		
		for(int i = 0; i < addresses; i++){
			int nextfree = this.GetSubnet.NextFree();
			if(nextfree < 0) continue;
			this.GetSubnet.OccupiedFlag[nextfree] = true;
			Console.WriteLine($"Auto assigning {this.GetSubnet.Addresses[nextfree]}.");
			
			flag = true;
		}
		
		if (!flag) Console.WriteLine($"No room available.");
	}

	public void FreeAll(){
		for(int i = 0; i < this.GetSubnet.OccupiedFlag.Length; i++){
			this.GetSubnet.OccupiedFlag[i] = false;
		}
		
		Console.WriteLine("Freed all addresses.");
	}
}