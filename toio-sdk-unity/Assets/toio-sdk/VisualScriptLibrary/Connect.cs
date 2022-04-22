namespace toio.VisualScript
{
    public class VisualScriptCubeConnecter : AsyncBase
    {
        public Cube value { get; set; }
        public Cube[] values { get; set; }
        private CubeConnecter connecter;
        public VisualScriptCubeConnecter(ConnectType type = ConnectType.Auto)
        {
            connecter = new CubeConnecter(type);
        }

        public async void Run(BLEPeripheralInterface peripheral)
        {
            var cube = await connecter.Connect(peripheral);
            value = cube;
            IsComplete = true;
        }
        public async void Run(BLEPeripheralInterface[] peripheral)
        {
            var cube = await connecter.Connect(peripheral);
            values = cube;
            IsComplete = true;
        }
    }

}