internal class Subnet{
	public IPAddress SubnetAddress;
	public IPAddress SubnetMask;
	public IPAddress BroadcastAddress;
	
	public IPAddress DefaultGateway;
	public List<IPAddress> Addresses;
	public bool[] OccupiedFlag;

	public IPRanges.Range AddressRange;
	
	public override string ToString() => $"========================================================================================\n" +
	                                     $"Subnet Address:		{this.SubnetAddress}\n" +
	                                     $"Subnet Mask:		{this.SubnetMask}\n" +
	                                     $"Broadcast Address:	{this.BroadcastAddress}\n" +
	                                     $"Default Gateway:	{this.DefaultGateway}\n" +
	                                     $"Address Range:		{this.AddressRange}\n" +
	                                     $"Address Count:		{this.Addresses.Count}\n" +
	                                     $"Used Addresses:		{this.OccupiedFlag.Count(t => t) + 1}\n" +
	                                     $"========================================================================================\n";

	public Subnet(IPAddress subnet, IPAddress mask){
		this.SubnetAddress = IPAddress.GoThroughMask(subnet, mask);
		this.SubnetMask = mask;
		
		this.BroadcastAddress = new IPAddress(this.SubnetAddress.AsUInt | ~this.SubnetMask.AsUInt);
		
		this.Addresses = new List<IPAddress>();
		for(uint address = this.SubnetAddress.AsUInt + 1; address <= this.BroadcastAddress.AsUInt - 1; address++){
			this.Addresses.Add(new IPAddress(address));
		}
		
		this.OccupiedFlag = new bool[this.Addresses.Count];

		this.DefaultGateway = this.Addresses[0];
		this.AddressRange = new IPRanges.Range(this.Addresses[0], this.Addresses[^1]);
	}
	
	public static List<Subnet> SplitSubnet(Subnet original){
		IPAddress newMask = new IPAddress(0x80000000 | original.SubnetMask.AsUInt >> 1);
		
		Subnet firstSubnet = new Subnet(original.SubnetAddress, newMask);
		Subnet secondSubnet = new Subnet(new IPAddress(firstSubnet.BroadcastAddress.AsUInt + 1), newMask);
		
		return new List<Subnet> { firstSubnet, secondSubnet };
	}

	public int NextFree() {
		for (int i = 1; i < this.OccupiedFlag.Length; i++)
			if (!this.OccupiedFlag[i]) return i;
		
		return -1;
	}

}