namespace Resin.Modules.FFT
{
    public interface IFftDataReceiver
    {
        void ReceiveFFTData(double[] fftData);
    }
}
