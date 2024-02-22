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

        private CubeScanner scanner;

        public VisualScriptNearestScanner(ConnectType type = ConnectType.Auto)
        {
            scanner = new CubeScanner(type);
        }

        public async void Run()
        {
            var peripheral = await scanner.NearestScan();
            value = peripheral;
            IsComplete = true;
        }

    }

    public class VisualScriptNearScanner : AsyncBase
    {
        public BLEPeripheralInterface[] value { get; set; }

        private CubeScanner scanner;
        private int satisfiedNum;

        public VisualScriptNearScanner(int satisfiedNum, ConnectType type = ConnectType.Auto)
        {
            this.scanner = new CubeScanner(type);
            this.satisfiedNum = satisfiedNum;
        }

        public async void Run()
        {
            var peripheral = await scanner.NearScan(this.satisfiedNum, 20);
            value = peripheral;
            IsComplete = true;
        }

    }
}