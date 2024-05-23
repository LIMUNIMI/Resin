using NITHdmis.Audio.In;
using NITHdmis.MicroLibrary;
using NITHdmis.Utils;
using System;
using System.Collections.Generic;

namespace Resin.Modules.FFT
{
    public class FFTModule : IPcmDataReceiver, IAudioInParamsListener
    {
        private AudioInParameters audioInParameters;
        private double[] FftDataFiltered;
        private double[] FftDataRaw;
        private MicroTimer FFTtimer;
        private short[] zeroPaddedArray;

        #region Interface

        // INPUTS
        private Int16[] pcmData;

        // OUTPUTS
        public List<IFftDataReceiver> FftDataReceivers { get; set; } = new List<IFftDataReceiver>();

        public List<ISpectrumFilter> SpectrumFilters { get; set; } = new List<ISpectrumFilter>();

        #endregion Interface

        public long Period
        {
            get { return FFTtimer.Interval; }
            set { FFTtimer.Interval = value; }
        }

        /// <summary>
        /// A module which performs all the FFT computations and draws the result in a canvas.
        /// </summary>
        /// <param name="canvas"> </param>
        public FFTModule(AudioInParameters audioInParameters, long period = 10000)
        {
            FFTtimer = new MicroTimer();
            FFTtimer.Interval = period;
            FFTtimer.MicroTimerElapsed += FFTtimer_MicroTimerElapsed;
            this.audioInParameters = audioInParameters;
        }

        void IPcmDataReceiver.ReceivePCMData(short[] pcmData)
        {
            this.pcmData = pcmData;
        }

        public void Start()
        {
            FFTtimer.Start();
        }

        public void Stop()
        {
            FFTtimer.Stop();
        }

        private void FFTtimer_MicroTimerElapsed(object sender, MicroTimerEventArgs timerEventArgs)
        {
            UpdateFFT();
        }

        private void NotifyListeners()
        {
            foreach (IFftDataReceiver l in FftDataReceivers)
            {
                l.ReceiveFFTData(FftDataFiltered);
            }
        }

        private void UpdateFFT()
        {
            if (pcmData != null)
            {
                // zeropadding the FFT data (Nick)
                zeroPaddedArray = audioInParameters.ZeroPad(pcmData);

                // apply a Hamming window function as we load the FFT array then calculate the FFT
                NAudio.Dsp.Complex[] fftFull = new NAudio.Dsp.Complex[zeroPaddedArray.Length];
                for (int i = 0; i < audioInParameters.FftPoints; i++)
                    fftFull[i].X = (float)(zeroPaddedArray[i] * NAudio.Dsp.FastFourierTransform.HammingWindow(i, pcmData.Length));
                NAudio.Dsp.FastFourierTransform.FFT(true, (int)Math.Log(zeroPaddedArray.Length, 2.0), fftFull);

                // copy the complex values into the double array that will be plotted
                if (FftDataRaw == null)
                    FftDataRaw = new double[zeroPaddedArray.Length / 2];
                for (int i = 0; i < zeroPaddedArray.Length / 2; i++)
                {
                    double fftLeft = Math.Abs(fftFull[i].X + fftFull[i].Y);
                    double fftRight = Math.Abs(fftFull[zeroPaddedArray.Length - i - 1].X + fftFull[zeroPaddedArray.Length - i - 1].Y);
                    FftDataRaw[i] = fftLeft + fftRight;
                }

                FftDataFiltered = (double[])FftDataRaw.Clone();

                try
                {
                    foreach (ISpectrumFilter filter in SpectrumFilters)
                    {
                        FftDataFiltered = filter.Compute(FftDataFiltered);
                    }
                }
                catch { }

                NotifyListeners();
            }
        }

        public void ReceiveAudioInParams(AudioInParameters audioInParameters)
        {
            this.audioInParameters = audioInParameters;
        }
    }
}