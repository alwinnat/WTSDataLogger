namespace WTSDataLogger.Core
{
    public interface IWeightWriter
    {
        void Write(WeightItem item);
        void Close();
    }
}
