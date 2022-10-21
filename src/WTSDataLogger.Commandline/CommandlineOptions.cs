using CommandLine;
using WTSDataLogger.Core;

namespace WTSDataLogger.Commandline
{
    internal sealed class CommandlineOptions : ISerialPortSettings
    {
        [Option('o', "output-file", Required = true, HelpText = "Path + filename to CSV file.")]
        public string OutputFileName { get; set; } = string.Empty;

        #region ISerialPortSettings Members
        [Option('p', "port", Required = false, Default = "COM1", HelpText = "Port for communications.")]
        public string PortName { get; set; } = "COM1";

        [Option('b', "baudrate", Required = false, Default = 9600, HelpText = "Data speed in baud.")]
        public int BaudRate { get; set; } = 9600;

        [Option('d', "databits", Required = false, Default = 8, HelpText = "Number of data bits.")]
        public int DataBits { get; set; } = 8;

        [Option('y', "parity", Required = false, Default = Parity.None, HelpText = "Method of detecting errors in transmission.")]
        public Parity Parity { get; set; } = Parity.None;

        [Option('s', "stopbits", Required = false, Default = StopBits.One, HelpText = "Number of stop bits.")]
        public StopBits StopBits { get; set; } = StopBits.One;

        [Option('h', "handshake", Required = false, Default = Handshake.None, HelpText = "Specifies the control protocol used in establishing a serial port communication.")]
        public Handshake Handshake { get; set; } = Handshake.None;

        [Option('r', "readtimeout", Required = false, Default = -1, HelpText = "Number of milliseconds before a time-out occurs when a read operation does not finish.")]
        public int ReadTimeout { get; set; } = -1;

        [Option('w', "writetimeout", Required = false, Default = -1, HelpText = "Number of milliseconds before a time-out occurs when a write operation does not finish.")]
        public int WriteTimeout { get; set; }
        #endregion
    }
}
