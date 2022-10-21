namespace WTSDataLogger.Core
{
    public enum Parity
    {
        None,
        Odd,
        Even,
        Mark,
        Space
    }

    public enum StopBits
    {
        None,
        One,
        Two,
        OnePointFive
    }

    public enum Handshake
    {
        None,
        XOnXOff,
        RequestToSend,
        RequestToSendXOnXOff
    }

    public interface ISerialPortSettings
    {
        string PortName { get; set; }
        int BaudRate { get; set; }
        int DataBits { get; set; }
        Parity Parity { get; set; }
        StopBits StopBits { get; set; }
        Handshake Handshake { get; set; }
        int ReadTimeout { get; set; }
        int WriteTimeout { get; set; }
    }
}
