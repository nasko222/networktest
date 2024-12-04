internal record struct IPAddress{
	public byte A;
	public byte B;
	public byte C;
	public byte D;

	internal IPAddress(byte a, byte b, byte c, byte d){
		this.A = a;
		this.B = b;
		this.C = c;
		this.D = d;
	}

	internal IPAddress(byte len) : this(
		len == 0
		? 0u
		: len > 32
			? throw new ArgumentOutOfRangeException(nameof(len), "Length must be between 8 and 30.")
			: uint.MaxValue << 32 - len) {}

	internal IPAddress(uint ipAsInt){
		this.A = (byte)(ipAsInt >> 24 & 0xFF);
		this.B = (byte)(ipAsInt >> 16 & 0xFF);
		this.C = (byte)(ipAsInt >> 8 & 0xFF);
		this.D = (byte)(ipAsInt & 0xFF);
	}

	public static IPAddress CreateSubnetMask(byte len) => new IPAddress(len);
	
	public static IPAddress Create(string ipString){
		string[] parts = ipString.Split('.');
		if (parts.Length != 4) throw new FormatException("Invalid IP address format");

		return new IPAddress(
			byte.Parse(parts[0]),
			byte.Parse(parts[1]),
			byte.Parse(parts[2]),
			byte.Parse(parts[3])
		);
	}

	public override string ToString() => RuntimeSettings.ShowBinary ? this.ToBinary() + " - " + this.ToDecimal() : this.ToDecimal();
	
	private string ToBinary() => $"{Convert.ToString(this.A, 2).PadLeft(8, '0')} " +
	                                               $"{Convert.ToString(this.B, 2).PadLeft(8, '0')} " +
	                                               $"{Convert.ToString(this.C, 2).PadLeft(8, '0')} " +
	                                               $"{Convert.ToString(this.D, 2).PadLeft(8, '0')}";
	
	internal string ToDecimal() => $"{this.A}.{this.B}.{this.C}.{this.D}";
	
	internal uint AsUInt => (uint)(this.A << 24 | this.B << 16 | this.C << 8 | this.D);

	internal static IPAddress GoThroughMask(IPAddress ip, IPAddress subnet) => new IPAddress{
		A = (byte)(ip.A & subnet.A),
		B = (byte)(ip.B & subnet.B),
		C = (byte)(ip.C & subnet.C),
		D = (byte)(ip.D & subnet.D)
	};

	internal byte GetCIDR(){
		uint mask = this.AsUInt;
		byte length = 0;
		
		while (mask != 0){
			if((mask & 0x80000000) == 0) break;
			
			length++;
			mask <<= 1;
		}

		return length;
	}
}