namespace Resin.Modules.FFT
{
    public interface ISpectrumFilter
    {
        double[] Compute(double[] spectrum);
    }
}
