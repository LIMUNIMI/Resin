using NeeqDMIs.Utils;
using Resin.DataTypes;
using Resin.DMIBox;
using Resin.Modules.FFT;
using Resin.Modules.SpectrumAnalyzer;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Resin.Modules.FFTplot
{
    public class FFTplotModule : IPeakIntensityReceiver, IPeakBucketRealReceiver, IPeakBucketSmoothReceiver, IFftDataReceiver
    {
        private const int BINVISUALIZATIONMARGIN = 20;
        private const int NOTELINEHEIGHT = 20;
        private const int NOTELINETHICKNESS = 3;
        private readonly SolidColorBrush calibrationSetpointBrush = new SolidColorBrush(Colors.Yellow);
        private readonly SolidColorBrush noteEnergyBrush = new SolidColorBrush(Colors.GreenYellow);
        private readonly SolidColorBrush noteLineBinBrush = new SolidColorBrush(Colors.Orange);
        private readonly SolidColorBrush peakRealBrush = new SolidColorBrush(Colors.Blue);
        private readonly SolidColorBrush peakSmoothBrush = new SolidColorBrush(Colors.Red);
        private readonly SolidColorBrush spectrumLineBrush = new SolidColorBrush(Colors.White);
        private Line calibrationSetpointLine;
        private Canvas canvas;
        private SegmentMapper cnvMapX;
        private SegmentMapper cnvMapY;
        private double maxDb = 1000f;

        private NoteEnergiesDrawingModes noteEnergiesDrawingMode = NoteEnergiesDrawingModes.MoreEnergetic;
        private List<Line> noteEnergiesLines = new List<Line>();
        private Line noteLine;
        private Line peakRealLine;
        private Line peakSmoothLine;
        private List<Line> playableNotesBinsLines = new List<Line>();
        private Polyline spectrumLine;
        public bool Enabled { get; set; } = false;

        #region Interface

        public double CalibrationSetpoint { get; set; }
        public double[] FftData { get; private set; }

        public NoteEnergiesDrawingModes NoteEnergiesDrawingMode
        {
            get => noteEnergiesDrawingMode;
            set
            {
                noteEnergiesDrawingMode = value;
                foreach (Line l in noteEnergiesLines)
                {
                    canvas.Children.Remove(l);
                }
                if (noteLine != null && canvas.Children.Contains(noteLine))
                {
                    canvas.Children.Remove(noteLine);
                }
            }
        }

        public int PeakRealX { get; private set; } = 0;
        public double PeakRealY { get; private set; } = 0;
        public int PeakSmoothX { get; private set; } = 0;

        #endregion Interface

        public FFTplotModule(Canvas canvas)
        {
            this.canvas = canvas;
            RemapCanvas();
        }

        public void DrawPlayableNotesBins()
        {
            foreach (Line ln in playableNotesBinsLines)
            {
                canvas.Children.Remove(ln);
            }

            playableNotesBinsLines.Clear();

            foreach (NoteData nd in G.TB.GetPlayableNoteDatas())
            {
                noteLine = new Line();
                noteLine.Stroke = noteLineBinBrush;
                noteLine.StrokeThickness = NOTELINETHICKNESS;

                double X = cnvMapX.Map(nd.In_CenterBin);
                noteLine.X1 = X;
                noteLine.Y1 = canvas.ActualHeight;
                noteLine.X2 = X;
                noteLine.Y2 = canvas.ActualHeight - cnvMapY.Map(NOTELINEHEIGHT);

                canvas.Children.Add(noteLine);

                playableNotesBinsLines.Add(noteLine);
            }
        }

        void IFftDataReceiver.ReceiveFFTData(double[] fftData)
        {
            FftData = fftData;
        }

        void IPeakBucketRealReceiver.ReceivePeakBucketReal(int peakBucketReal)
        {
            PeakRealX = peakBucketReal;
        }

        void IPeakBucketSmoothReceiver.ReceivePeakBucketSmooth(int peakBucketSmooth)
        {
            PeakSmoothX = peakBucketSmooth;
        }

        void IPeakIntensityReceiver.ReceivePeakIntensity(double peakIntensity)
        {
            PeakRealY = peakIntensity;
        }

        public void RemapCanvas()
        {
            EraseAll();

            int maxBin = 0;
            int minBin = 1000000000;

            List<NoteData> playableNotes = G.TB.GetPlayableNoteDatas();

            if (playableNotes.Count > 0)
            {
                minBin = playableNotes[0].In_LowBin;
                maxBin = playableNotes[playableNotes.Count - 1].In_HighBin;

                cnvMapX = new SegmentMapper(minBin, maxBin, 0, canvas.ActualWidth);
                cnvMapY = new SegmentMapper(0, maxDb, 0, canvas.ActualHeight);
            }
            else
            {
                minBin = 1;
                maxBin = 2;
                cnvMapX = new SegmentMapper(minBin, maxBin, 0, canvas.ActualWidth);
                cnvMapY = new SegmentMapper(0, maxDb, 0, canvas.ActualHeight);
            }
        }

        private void EraseAll()
        {
            EraseAllNoteEnergies();
            EraseCalibrationSetpoint();
            EraseNoteMoreEnergetic();
            ErasePeakReal();
            ErasePeakSmooth();
            EraseSpectrumPolyLine();
        }

        public void UpdateFrame()
        {
            if (Enabled)
            {
                if (FftData != null)
                    DrawFrameCycle();
            }
        }

        private void DrawAllNoteEnergies()
        {
            EraseAllNoteEnergies();

            foreach (Line l in noteEnergiesLines)
            {
                canvas.Children.Remove(l);
            }

            noteEnergiesLines.Clear();

            foreach (NoteData nd in G.TB.NoteDatas)
            {
                if (nd.IsPlayable)
                {
                    Line line = new Line { Stroke = noteEnergyBrush, StrokeThickness = 3 };

                    double X = cnvMapX.Map(nd.In_CenterBin);
                    line.X1 = X;
                    line.Y1 = canvas.ActualHeight;
                    line.X2 = X;
                    line.Y2 = canvas.ActualHeight - cnvMapY.Map(nd.In_Energy);

                    canvas.Children.Add(line);
                    noteEnergiesLines.Add(line);
                }
            }
        }

        private void EraseAllNoteEnergies()
        {
            if (noteEnergiesLines == null)
            {
                noteEnergiesLines = new List<Line>();
            }
        }

        private void DrawCalibrationSetpoint()
        {
            EraseCalibrationSetpoint();

            calibrationSetpointLine = new Line();
            calibrationSetpointLine.Stroke = calibrationSetpointBrush;
            calibrationSetpointLine.StrokeThickness = 1;

            double Y = cnvMapY.Map(CalibrationSetpoint);
            calibrationSetpointLine.X1 = 0;
            calibrationSetpointLine.Y1 = canvas.ActualHeight - Y;
            calibrationSetpointLine.X2 = canvas.ActualWidth;
            calibrationSetpointLine.Y2 = canvas.ActualHeight - Y;

            canvas.Children.Add(calibrationSetpointLine);
        }

        private void EraseCalibrationSetpoint()
        {
            if (calibrationSetpointLine != null)
            {
                canvas.Children.Remove(calibrationSetpointLine);
            }
        }

        private void DrawFrameCycle()
        {
            DrawSpectrumPolyLine();
            DrawPeakReal();
            DrawPeakSmooth();
            DrawCalibrationSetpoint();

            switch (noteEnergiesDrawingMode)
            {
                case NoteEnergiesDrawingModes.None:
                    break;

                case NoteEnergiesDrawingModes.MoreEnergetic:
                    DrawNoteMoreEnergetic();
                    break;

                case NoteEnergiesDrawingModes.All:
                    DrawAllNoteEnergies();
                    break;
            }
        }

        private void DrawNoteMoreEnergetic()
        {
            EraseNoteMoreEnergetic();

            var note = G.TB.GetNoteMoreEnergeticData();
            noteLine = new Line();
            noteLine.Stroke = noteEnergyBrush;
            noteLine.StrokeThickness = 3;

            double X = cnvMapX.Map(note.In_CenterBin);
            noteLine.X1 = X;
            noteLine.Y1 = canvas.ActualHeight;
            noteLine.X2 = X;
            noteLine.Y2 = canvas.ActualHeight - cnvMapY.Map(note.In_Energy);

            canvas.Children.Add(noteLine);
        }

        private void DrawPeakReal()
        {
            ErasePeakReal();

            peakRealLine = new Line();
            peakRealLine.Stroke = peakRealBrush;
            peakRealLine.StrokeThickness = 1;

            double X = cnvMapX.Map(PeakRealX);
            peakRealLine.X1 = X;
            peakRealLine.Y1 = canvas.ActualHeight;
            peakRealLine.X2 = X;
            peakRealLine.Y2 = 0;

            canvas.Children.Add(peakRealLine);
        }

        private void DrawPeakSmooth()
        {
            ErasePeakSmooth();

            peakSmoothLine = new Line();
            peakSmoothLine.Stroke = peakSmoothBrush;
            peakSmoothLine.StrokeThickness = 1;

            double X = cnvMapX.Map(PeakSmoothX);
            peakSmoothLine.X1 = X;
            peakSmoothLine.Y1 = canvas.ActualHeight;
            peakSmoothLine.X2 = X;
            peakSmoothLine.Y2 = 0;

            canvas.Children.Add(peakSmoothLine);
        }

        private void DrawSpectrumPolyLine()
        {
            EraseSpectrumPolyLine();

            spectrumLine = new Polyline();
            spectrumLine.Stroke = spectrumLineBrush;
            spectrumLine.StrokeThickness = 1;

            for (int i = (int)cnvMapX.BaseMin; i <= (int)cnvMapX.BaseMax; i++)
            {
                spectrumLine.Points.Add(new Point((int)cnvMapX.Map(i), canvas.ActualHeight - cnvMapY.Map(FftData[i])));
                if (FftData[i] > cnvMapY.BaseMax)
                {
                    //cnvMapY.BaseMax = dataFft[i];
                }
            }

            canvas.Children.Add(spectrumLine);
        }

        private void EraseNoteMoreEnergetic()
        {
            if (noteLine != null)
            {
                canvas.Children.Remove(noteLine);
            }
        }

        private void ErasePeakReal()
        {
            if (peakRealLine != null)
            {
                canvas.Children.Remove(peakRealLine);
            }
        }

        private void ErasePeakSmooth()
        {
            if (peakSmoothLine != null)
            {
                canvas.Children.Remove(peakSmoothLine);
            }
        }

        private void EraseSpectrumPolyLine()
        {
            if (spectrumLine != null)
            {
                canvas.Children.Remove(spectrumLine);
            }
        }

        public enum NoteEnergiesDrawingModes
        {
            None,
            MoreEnergetic,
            All
        }
    }
}