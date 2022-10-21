using IO = System.IO.Ports;

namespace WTSDataLogger.Core
{
    public sealed class SerialPort : ISerialPort
    {
        private readonly IO.SerialPort _serialPort;

        public SerialPort(ISerialPortSettings settings)
        {
            _serialPort = new IO.SerialPort();
            _serialPort.DataReceived += (sender, e) => DataReceived?.Invoke(this, EventArgs.Empty);

            Initialize(settings);
        }

        #region ISerialPort Members
        public event EventHandler? DataReceived;

        public bool IsOpen => _serialPort.IsOpen;

        public void Initialize(ISerialPortSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings, nameof(settings));
            _serialPort.PortName = settings.PortName;
            _serialPort.BaudRate = settings.BaudRate;
            _serialPort.DataBits = settings.DataBits;
            _serialPort.Parity = Enum.TryParse<IO.Parity>(settings.Parity.ToString(), out var parity) ? parity : IO.Parity.None;
            _serialPort.StopBits = Enum.TryParse<IO.StopBits>(settings.StopBits.ToString(), out var stopBits) ? stopBits : IO.StopBits.One;
            _serialPort.Handshake = Enum.TryParse<IO.Handshake>(settings.Handshake.ToString(), out var handshake) ? handshake : IO.Handshake.None;
            _serialPort.ReadTimeout = settings.ReadTimeout;
            _serialPort.WriteTimeout = settings.WriteTimeout;
        }

        public void Open() => _serialPort.Open();
        public void Close() => _serialPort.Close();
        public void Write(string text) => _serialPort.Write(text);

        public string ReadExisting() => _serialPort.ReadExisting();
        public string[] GetPortNames() => IO.SerialPort.GetPortNames();
        #endregion
    }
}
