namespace WTSDataLogger.Core
{
    public interface ISerialPort
    {
        event EventHandler? DataReceived;

        bool IsOpen { get; }

        void Initialize(ISerialPortSettings settings);
        void Open();
        void Close();
        void Write(string text);

        string ReadExisting();
        string[] GetPortNames();
    }
}
