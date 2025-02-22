using DPFP;
using DPFP.Capture;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace FingerprintWF
{
    public partial class Form1 : Form, DPFP.Capture.EventHandler
    {
        private DPFP.Capture.Capture _capturer;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                _capturer = new Capture();
                if (_capturer != null)
                {
                    _capturer.EventHandler = this;
                    MakeReport("Pronto para iniciar");
                }
                else
                    MakeReport("ainda não está pronto");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        protected void MakeReport(string message)
        {
            Invoke(new Action(delegate ()
            {
                StatusText.Text = message;
            }));
        }

        public void OnComplete(object Capture, string ReaderSerialNumber, Sample Sample)
        {
            MakeReport("A captura foi realizada");
            Proccess(Sample);
        }

        public void OnFingerGone(object Capture, string ReaderSerialNumber) => MakeReport("O dedo foi removido do leitor");

        public void OnFingerTouch(object Capture, string ReaderSerialNumber) => MakeReport("O leitor foi tocado");

        public void OnReaderConnect(object Capture, string ReaderSerialNumber) => MakeReport("Conectada");

        public void OnReaderDisconnect(object Capture, string ReaderSerialNumber) => MakeReport("Desconectada");

        public void OnSampleQuality(object Capture, string ReaderSerialNumber, CaptureFeedback CaptureFeedback)
        {
            if (CaptureFeedback == CaptureFeedback.Good)
                MakeReport("A captura está boa");
            else
                MakeReport("A captura está ruim");
        }

        protected virtual void Proccess(DPFP.Sample sample)
        {
            DrawImage(ConvertSampleToBitmap(sample));
        }

        protected Bitmap ConvertSampleToBitmap(DPFP.Sample sample)
        {
            DPFP.Capture.SampleConversion conversor = new SampleConversion();
            Bitmap bitmap = null;
            conversor.ConvertToPicture(sample, ref bitmap);

            return bitmap;
        }

        private void DrawImage(Bitmap bitmap) => image.Image = new Bitmap(bitmap, image.Size);

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (_capturer != null)
            {
                try
                {
                    _capturer.StartCapture();
                    MakeReport("Faça a leitura do dedo");
                }
                catch (Exception ex)
                {
                    MakeReport($"um erro aconteceu: {ex.Message}");
                }
            }
        }
    }
}
