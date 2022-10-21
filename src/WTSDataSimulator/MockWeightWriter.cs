using WTSDataLogger.Core;

namespace WTSDataSimulator
{
    internal sealed class MockWeightWriter : IWeightWriter
    {
        #region IWeightWriter Members
        public void Write(WeightItem item) { }

        public void Close() { }
        #endregion
    }
}
