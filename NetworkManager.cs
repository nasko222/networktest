internal class NetworkManager{
    private static List<Subnet> _subnets = new List<Subnet>();
    private static NetworkSession? _currentSession;

    public static void Main(string[] args){
        Console.WriteLine($"Welcome to the Nasko's Network Manager {SharedConstants.VERSION}!");
        Console.WriteLine("Type 'help' to see a list of commands.");

        bool running = true;
        while (running){
            Console.Write("\nroot > ");
            if (_currentSession != null) Console.Write($"@{_currentSession.GetSubnet.SubnetAddress.ToDecimal()} > ");
            string? input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input)) continue;

            string[] parts = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            string command = parts[0].ToLower();
            string argument = parts.Length > 1 ? parts[1] : string.Empty;

            switch (command){
                case "help":
                    ShowHelp();
                    break;
                case "create" when NetworkManager._currentSession == null:
                    CreateSubnet(argument);
                    break;
                case "split" when NetworkManager._currentSession == null:
                    SplitSubnet(argument);
                    break;
                case "list" when NetworkManager._currentSession == null:
                    ListSubnets();
                    break;
                case "show" when NetworkManager._currentSession == null:
                    ShowSubnet(argument);
                    break;
                case "arrange" when NetworkManager._currentSession == null:
                    ArrangeSubnets();
                    break;
                case "delete" when NetworkManager._currentSession == null:
                    DeleteSubnet(argument);
                    break;
                case "enter" when NetworkManager._currentSession == null:
                    EnterSubnet(argument);
                    break;
                case "info" when NetworkManager._currentSession != null:
                    ShowSubnet();
                    break;
                case "map" when NetworkManager._currentSession != null:
                    ShowAddresses(argument);
                    break;
                case "auto" when NetworkManager._currentSession != null:
                    AutoOccupy(argument);
                    break;
                case "occupy" when NetworkManager._currentSession != null:
                    OccupySingleAddress(argument);
                    break;
                case "free" when NetworkManager._currentSession != null:
                    FreeAddress(argument);
                    break;
                case "clear" when NetworkManager._currentSession != null:
                    FreeAllAddress();
                    break;
                case "query" when NetworkManager._currentSession != null:
                    QueryAddress(argument);
                    break;
                case "leave" when NetworkManager._currentSession != null:
                    _currentSession = null;
                    break;
                case "binary":
                    ToggleBinaryDisplay();
                    break;
                case "maplen":
                    SetMapLength(argument);
                    break;
                case "garbage":
                    GC.Collect();
                    Console.WriteLine("Collected garbage.");
                    break;
                case "exit":
                    running = false;
                    break;
                default:
                    Console.WriteLine("Unknown command. Type 'help' to see available commands.");
                    break;
            }
        }
    }

    private static void ShowHelp(){
        Console.WriteLine("\nAvailable commands:");
        Console.WriteLine("  help              -   Show this help message.");
        if(NetworkManager._currentSession != null){
            Console.WriteLine("  info              -   Show information about the current subnet.");
            Console.WriteLine("  map [index]       -   Shows a set of addresses in the current subnet starting from the index.");
            Console.WriteLine("  auto [amount]     -   Automatically occupy a specific number of addresses in the current subnet.");
            Console.WriteLine("  occupy [IP]       -   Occupy a particular IP address in the current subnet.");
            Console.WriteLine("  free [IP]         -   Free an IP address in the current subnet.");
            Console.WriteLine("  query [IP]        -   Returns the current status of the given IP address in the current subnet.");
            Console.WriteLine("  clear             -   Unoccupies all addresses in the current subnet.");
            Console.WriteLine("  leave             -   Disconnect from the current subnet.");
        } else{
            Console.WriteLine("  create [IP/Mask]  -   Create a new subnet (e.g., 'create 192.168.0.0/27').");
            Console.WriteLine("  split [index]     -   Split an existing subnet into two smaller subnets.");
            Console.WriteLine("  list              -   List all created subnets.");
            Console.WriteLine("  show [index]      -   Show detailed information about a specific subnet.");
            Console.WriteLine("  arrange           -   Arrange subnets by network address.");
            Console.WriteLine("  delete [index]    -   Deletes a subnet.");
            Console.WriteLine("  enter [index]     -   Enter a subnet and start managing it.");
        }
        Console.WriteLine("  binary            -   Toggle binary display for IP addresses.");
        Console.WriteLine("  maplen            -   Changes the length of the set for the map command.");
        Console.WriteLine("  garbage           -   Collects garbage.");
        Console.WriteLine("  exit              -   Exit the CLI.");
    }

    private static void CreateSubnet(string argument){
        try{
            string[] parts = argument.Split('/');
            if (parts.Length != 2){
                Console.WriteLine("Invalid format. Use: 'create [IP/Mask]'.");
                return;
            }
            
            IPAddress ip = IPAddress.Create(parts[0]);
            if (IPRanges.GetRangeClass(ip) == IPRanges.PRIVATE_RANGE_CLASS.UNKNOWN){
                Console.WriteLine("The IP address is out of the allowed private IP ranges.");
                return;
            }
            
            if(!byte.TryParse(parts[1], out byte maskLen) || maskLen < IPRanges.GetMinimalMaxLen(ip) || maskLen > 30){
                Console.WriteLine("Invalid mask length.");
                return;
            }
            IPAddress mask = IPAddress.CreateSubnetMask(maskLen);
            
            Subnet subnet = new Subnet(ip, mask);
            _subnets.Add(subnet);
            
            Console.WriteLine($"Successfully created subnet on {subnet.SubnetAddress} with ID {_subnets.Count - 1}");
        }
        catch (Exception ex) {
            Console.WriteLine(ex.Message);
        }
    }

    private static void SplitSubnet(string argument){
        if (!int.TryParse(argument, out int index) || index < 0 || index >= _subnets.Count){
            Console.WriteLine($"Index invalid or out of the available subnet range of {NetworkManager._subnets.Count - 1}");
            return;
        }
        
        try{
            List<Subnet> newSubnets = Subnet.SplitSubnet(_subnets[index]);
            _subnets.RemoveAt(index);
            _subnets.InsertRange(index, newSubnets);

            Console.WriteLine($"Subnet split successfully. (ID's {index} and {index + 1})");
            
            GC.Collect();
        }
        catch (Exception ex) {
            Console.WriteLine(ex.Message);
        }
    }

    private static void ListSubnets(){
        if (_subnets.Count == 0){
            Console.WriteLine("No subnets created yet.");
            return;
        }

        for (int i = 0; i < _subnets.Count; i++){
            Console.WriteLine($"[ID {i}] {NetworkManager._subnets[i].SubnetAddress.ToDecimal()}/{NetworkManager._subnets[i].SubnetMask.GetCIDR()}");
        }
    }

    private static void ShowSubnet(){
        Console.WriteLine(_currentSession?.GetSubnet);
    }
    
    private static void ShowSubnet(string argument){
        if (!int.TryParse(argument, out int index) || index < 0 || index >= _subnets.Count){
            Console.WriteLine($"Index invalid or out of the available subnet range of {NetworkManager._subnets.Count - 1}");
            return;
        }

        Console.WriteLine(_subnets[index]);
    }

    private static void ToggleBinaryDisplay(){
        RuntimeSettings.ShowBinary = !RuntimeSettings.ShowBinary;
        Console.WriteLine($"Binary display toggled to: {(RuntimeSettings.ShowBinary ? "ON" : "OFF")}");
    }
    
    private static void ArrangeSubnets(){
        if(_subnets.Count == 0){
            Console.WriteLine("No subnets created yet.");
            return;
        }
        _subnets.Sort((subnet1, subnet2) => subnet1.SubnetAddress.AsUInt.CompareTo(subnet2.SubnetAddress.AsUInt));
        Console.WriteLine("All subnets arranged successfully.");
    }

    private static void DeleteSubnet(string argument){
        if (!int.TryParse(argument, out int index) || index < 0 || index >= _subnets.Count){
            Console.WriteLine($"Index invalid or out of the available subnet range of {_subnets.Count - 1}");
            return;
        }
        
        _subnets.RemoveAt(index);
        Console.WriteLine($"Subnet {index} deleted successfully.");
        
        GC.Collect();
    }
    
    private static void EnterSubnet(string argument){
        if (!int.TryParse(argument, out int index) || index < 0 || index >= _subnets.Count) {
            Console.WriteLine($"Index invalid or out of the available subnet range of {_subnets.Count - 1}");
            return;
        }

        _currentSession = new NetworkSession(_subnets[index]); 
    }

    private static void AutoOccupy(string argument){
        if (!int.TryParse(argument, out int autos) || autos < 0) {
            _currentSession?.Assign(1);
            return;
        }

        int addresses = Math.Max(1, autos);
        _currentSession?.Assign(addresses);
    }
    
    private static void FreeAllAddress() => _currentSession?.FreeAll();
    
    
    private static void OccupySingleAddress(string ip) => _currentSession?.SetIPFlag(ip, true);
    private static void FreeAddress(string ip) => _currentSession?.SetIPFlag(ip, false);
    private static void ShowAddresses(string arg) => _currentSession?.ShowAddresses(arg);
    private static void QueryAddress(string ip) => _currentSession?.QueryAddress(ip);
    
    private static void SetMapLength(string argument){
        if (!int.TryParse(argument, out int mapLength) || mapLength <= 0) {
            Console.WriteLine("Invalid map length. Please enter a positive integer.");
            return;
        }

        RuntimeSettings.MapLength = Math.Min(Math.Max(8, mapLength), 16777216);
        Console.WriteLine($"Map length set to {RuntimeSettings.MapLength}.");
    }
}
