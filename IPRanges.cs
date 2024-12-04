internal static class IPRanges{
	internal struct Range{
		public IPAddress Start;
		public IPAddress End;

		public Range(IPAddress start, IPAddress end){
			this.Start = start;
			this.End = end;
		}
		
		public override string ToString() => $"{this.Start.ToDecimal()} - {this.End.ToDecimal()}";
	}
	
	internal enum PRIVATE_RANGE_CLASS{
		UNKNOWN,
		CLASS_A,
		CLASS_B,
		CLASS_C
	}
	
	public static readonly Dictionary<PRIVATE_RANGE_CLASS, Range> PrivateRanges = new Dictionary<PRIVATE_RANGE_CLASS, Range>{
		{ PRIVATE_RANGE_CLASS.CLASS_A, new Range(IPAddress.Create("10.0.0.0"), IPAddress.Create("10.255.255.255")) },
		{ PRIVATE_RANGE_CLASS.CLASS_B, new Range(IPAddress.Create("172.16.0.0"), IPAddress.Create("172.31.255.255")) },
		{ PRIVATE_RANGE_CLASS.CLASS_C, new Range(IPAddress.Create("192.168.0.0"), IPAddress.Create("192.168.255.255")) }
	};
	
	public static readonly Dictionary<PRIVATE_RANGE_CLASS, byte> PrivateRangesMaskLen = new Dictionary<PRIVATE_RANGE_CLASS, byte>{
		{ PRIVATE_RANGE_CLASS.UNKNOWN, 0 },
		{ PRIVATE_RANGE_CLASS.CLASS_A, 8 },
		{ PRIVATE_RANGE_CLASS.CLASS_B, 12 },
		{ PRIVATE_RANGE_CLASS.CLASS_C, 16 }
	};
	
	internal static PRIVATE_RANGE_CLASS GetRangeClass(IPAddress ip) => (from range
			in IPRanges.PrivateRanges
		where IsInRange(ip.AsUInt, range.Value.Start.AsUInt, range.Value.End.AsUInt)
		select range.Key).FirstOrDefault();

	private static bool IsInRange(uint ip, uint start, uint end) => ip >= start && ip <= end;
	public static int GetMinimalMaxLen(IPAddress ip) => PrivateRangesMaskLen[GetRangeClass(ip)];
}