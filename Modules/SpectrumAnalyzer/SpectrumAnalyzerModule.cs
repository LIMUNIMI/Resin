using NeeqDMIs.Utils.ValueFilters;
using System;
using System.Collections.Generic;
using Resin.DMIBox;
using Resin.Modules.FFT;

namespace Resin.Modules.SpectrumAnalyzer
{
    public class SpectrumAnalyzerModule : IFftDataReceiver
    {
        #region Interface

        // INPUT
        private double[] fftData;

        // OUTPUT
        public List<IPeakBucketRealReceiver> PeakBucketRealReceivers { get; set; } = new List<IPeakBucketRealReceiver>();

        public List<IPeakBucketSmoothReceiver> PeakBucketSmoothReceivers { get; set; } = new List<IPeakBucketSmoothReceiver>();
        public List<IPeakIntensityReceiver> PeakIntensityReceivers { get; set; } = new List<IPeakIntensityReceiver>();

        #endregion Interface

        private IDoubleFilter smoothFilter;
        public int PeakBucketReal { get; private set; }
        public int PeakBucketSmooth { get; private set; }
        public double PeakIntensity { get; private set; }

        private EnergyModes EnergyMode { get; set; }

        public SpectrumAnalyzerModule(IDoubleFilter smoothFilter, EnergyModes energyMode = EnergyModes.Quadratic)
        {
            this.smoothFilter = smoothFilter;
            this.EnergyMode = energyMode;
        }

        void IFftDataReceiver.ReceiveFFTData(double[] fftData)
        {
            this.fftData = fftData;
            Compute();
        }

        private void Compute()
        {
            double maxY = 0f;
            int maxX = 0;

            for (int i = 0; i < fftData.Length; i++)
            {
                if (fftData[i] > maxY)
                {
                    maxY = fftData[i];
                    maxX = i;
                }
            }

            PeakBucketReal = maxX;
            smoothFilter.Push(PeakBucketReal);
            PeakBucketSmooth = (int)smoothFilter.Pull();
            PeakIntensity = maxY;

            // NOTEMOREENERGETIC extraction
            ComputeNoteEnergies();

            NotifyReceivers();
        }

        private void ComputeNoteEnergies()
        {
            double maxEnergy = 0f;
            int maxEnergyIndex = 0;

            for (int i = 0; i < G.TB.NoteDatas.Count; i++)
            {
                if (G.TB.NoteDatas[i].IsPlayable)
                {
                    G.TB.NoteDatas[i].In_Energy = 0f;
                    for (int j = G.TB.NoteDatas[i].In_LowBin; j <= G.TB.NoteDatas[i].In_HighBin; j++)
                    {
                        switch (EnergyMode)
                        {
                            case EnergyModes.Normal:
                                G.TB.NoteDatas[i].In_Energy += fftData[j] / G.TB.NoteDatas[i].In_Dimension;
                                break;

                            case EnergyModes.Quadratic:
                                G.TB.NoteDatas[i].In_Energy += fftData[j] * fftData[j] / G.TB.NoteDatas[i].In_Dimension;
                                break;
                        }
                    }

                    G.TB.NoteDatas[i].In_Energy = Math.Sqrt(G.TB.NoteDatas[i].In_Energy);

                    if (G.TB.NoteDatas[i].In_Energy >= maxEnergy)
                    {
                        maxEnergy = G.TB.NoteDatas[i].In_Energy;
                        maxEnergyIndex = i;
                    }
                }
            }

            G.TB.NoteMoreEnergeticDataIndex = maxEnergyIndex;
        }

        private void NotifyReceivers()
        {
            foreach (IPeakBucketRealReceiver r in PeakBucketRealReceivers)
            {
                r.ReceivePeakBucketReal(PeakBucketReal);
            }

            foreach (IPeakBucketSmoothReceiver r in PeakBucketSmoothReceivers)
            {
                r.ReceivePeakBucketSmooth(PeakBucketSmooth);
            }

            foreach (IPeakIntensityReceiver r in PeakIntensityReceivers)
            {
                r.ReceivePeakIntensity(PeakIntensity);
            }
        }

        public enum EnergyModes
        {
            Normal,
            Quadratic
        }
    }
}