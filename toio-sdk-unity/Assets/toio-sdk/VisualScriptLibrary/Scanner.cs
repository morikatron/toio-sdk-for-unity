namespace toio.VisualScript
{
    public class AsyncBase
    {
        public bool IsComplete { get; set; }
        public AsyncBase()
        {
            IsComplete = false;
        }
    }

    public class VisualScriptNearestScanner : AsyncBase
    {
        public BLEPeripheralInterface value { get; set; }

        private NearestScanner scanner;

        public VisualScriptNearestScanner(ConnectType type = ConnectType.Auto)
        {
            scanner = new NearestScanner(type);
        }

        public async void Run()
        {
            var peripheral = await scanner.Scan();
            value = peripheral;
            IsComplete = true;
        }

    }

    public class VisualScriptNearScanner : AsyncBase
    {
        public BLEPeripheralInterface[] value { get; set; }

        private NearScanner scanner;

        public VisualScriptNearScanner(int satisfiedNum, ConnectType type = ConnectType.Auto)
        {
            scanner = new NearScanner(satisfiedNum, type);
        }

        public async void Run()
        {
            var peripheral = await scanner.Scan();
            value = peripheral;
            IsComplete = true;
        }

    }
}