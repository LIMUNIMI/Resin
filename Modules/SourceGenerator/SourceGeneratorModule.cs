using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tong_Sharp.Modules.SourceGenerator
{
    public class SourceGeneratorModule
    {

        private readonly IWavePlayer driverOut;

        private SignalGenerator signalGenerator;

        // Frequency Max
        private const double FMax = 20000;

        // Frequency Min
        private const double FMin = 20;

        // constante  Math.Log10(FMax / FMin)
        private readonly double log10FmaxFMin;


        public SourceGeneratorModule()
        {
            // Const
            log10FmaxFMin = Math.Log10(FMax / FMin);

            // Init Audio
            driverOut = new WaveOutEvent();
            signalGenerator = new SignalGenerator();

            // Init Driver Audio
            driverOut.Init(signalGenerator);

        }

        private void Start()
        {
            if (driverOut != null)
            {
                driverOut.Play();
            }
        }

        private void Bstop()
        {
            if (driverOut != null)
            {
                driverOut.Stop();
            }
        }

        private void OnComboPrecisionFrequencySelectedIndexChanged(object sender, EventArgs e)
        {
            // change Type
            int octave = cmbPrecisionToOctave(cmbPrecisionFrq.SelectedIndex);

            // change tbFrq
            tbFrq.Maximum = octave;

            // 
            CalculateTrackBarFrequency();
        }

        private void CalculateTrackBarFrequency()
        {
            double x = Math.Pow(10, (tbFrq.Value / (tbFrq.Maximum / log10FmaxFMin))) * FMin;
            x = Math.Round(x, 1);


            // Change Frequency in Generator
            if (cmbFrq.SelectedIndex <= 0)
                signalGenerator.Frequency = x;
            else
                signalGenerator.Frequency = Convert.ToDouble(cmbFrq.SelectedItem);

            // View Frq
            lblFrq.Text = x.ToString(CultureInfo.InvariantCulture);
        }

        // Frq Enabled
        private void FrqEnabled(bool state)
        {
            grbFrq.Enabled = state;
            bool bFrqVariable = (cmbFrq.SelectedIndex <= 0);
            tbFrq.Enabled = bFrqVariable;
            cmbPrecisionFrq.Enabled = bFrqVariable;
            lblFrqPrecision.Enabled = bFrqVariable;
            lblFrq.Enabled = bFrqVariable;
            lblFrqUnit.Enabled = bFrqVariable;
            lblFrqTitle.Enabled = bFrqVariable;
        }

        // --------------
        // Frequency End
        // --------------

        // cmb Frq End
        private void OnComboFrequencyEndSelectedIndexChanged(object sender, EventArgs e)
        {
            CalculateTrackBarEndFrequency();
            FrqEndEnabled(true);
        }

        // trackbar FrqEnd
        private void OnTrackBarFrequencyEndScroll(object sender, EventArgs e)
        {
            CalculateTrackBarEndFrequency();
        }

        // combobox FrqEnd Precision
        private void OnComboPrecisionFrequencyEndSelectedIndexChanged(object sender, EventArgs e)
        {
            // change Type
            int octave = cmbPrecisionToOctave(cmbPrecisionFrqEnd.SelectedIndex);

            // change tbFrq
            tbFrqEnd.Maximum = octave;

            // 
            CalculateTrackBarEndFrequency();
        }

        private void CalculateTrackBarEndFrequency()
        {
            double x = Math.Pow(10, (tbFrqEnd.Value / (tbFrqEnd.Maximum / log10FmaxFMin))) * FMin;
            x = Math.Round(x, 1);

            // Change Frequency in Generator
            if (cmbFrqEnd.SelectedIndex <= 0)
                signalGenerator.FrequencyEnd = x;
            else
                signalGenerator.FrequencyEnd = Convert.ToDouble(cmbFrqEnd.SelectedItem);


            // View Frq
            lblFrqEnd.Text = x.ToString(CultureInfo.InvariantCulture);
        }

        // FrqEnd Enabled
        private void FrqEndEnabled(bool state)
        {
            grpFrqEnd.Enabled = state;
            bool bFrqEndVariable = (cmbFrqEnd.SelectedIndex <= 0);
            tbFrqEnd.Enabled = bFrqEndVariable;
            cmbPrecisionFrqEnd.Enabled = bFrqEndVariable;
            lblFrqEndPrecision.Enabled = bFrqEndVariable;
            lblFrqEnd.Enabled = bFrqEndVariable;
            lblFrqEndUnit.Enabled = bFrqEndVariable;
            lblFrqEndTitle.Enabled = bFrqEndVariable;
        }

        // --------------
        // Gain 
        // --------------

        // trackbar Gain
        private void OnTrackBarGainScroll(object sender, EventArgs e)
        {
            CalculateTrackBarToGain();
        }

        private void CalculateTrackBarToGain()
        {
            lblGain.Text = tbGain.Value.ToString();
            signalGenerator.Gain = Decibels.DecibelsToLinear(tbGain.Value);
        }

        private void OnTrackBarSweepLengthScroll(object sender, EventArgs e)
        {
            CalculateTrackBarToSweepLength();
        }

        private void CalculateTrackBarToSweepLength()
        {
            lblSweepLength.Text = tbSweepLength.Value.ToString();
            signalGenerator.SweepLengthSecs = tbSweepLength.Value;

        }

        // Sweep Length Enabled
        private void SweepLengthEnabled(bool state)
        {
            grpSweepLength.Enabled = state;
        }

        // --------------
        // Phase Reverse
        // --------------

        // Reverse Left
        private void OnReverseLeftCheckedChanged(object sender, EventArgs e)
        {
            PhaseReverse();
        }

        // Reverse Right
        private void OnReverseRightCheckedChanged(object sender, EventArgs e)
        {
            PhaseReverse();
        }

        // Apply PhaseReverse
        private void PhaseReverse()
        {
            signalGenerator.PhaseReverse[0] = chkReverseLeft.Checked;
            signalGenerator.PhaseReverse[1] = chkReverseRight.Checked;
        }

        // --------------
        // Other
        // --------------

        // Nb of Frequency
        private int cmbPrecisionToOctave(int idx)
        {
            return (int)(10.35f * idx);
        }

        // Clean DriverOut
        private void Cleanup()
        {
            if (driverOut != null)
                driverOut.Stop();

            signalGenerator = null;

            if (driverOut != null)
            {
                driverOut.Dispose();
            }
        }

        private void OnButtonSaveClick(object sender, EventArgs e)
        {
            OnButtonStopClick(this, e);
            var sfd = new SaveFileDialog();
            sfd.Filter = "WAV File|*.wav";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                var osp = new OffsetSampleProvider(signalGenerator);
                osp.TakeSamples = signalGenerator.WaveFormat.SampleRate * 20 * signalGenerator.WaveFormat.Channels;
                WaveFileWriter.CreateWaveFile16(sfd.FileName, osp);
            }
        }
    }
