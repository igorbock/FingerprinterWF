using System;
using System.Drawing;
using System.Windows.Forms;
using DPUruNet;

namespace FingerprintWF
{
    public partial class Form1 : Form
    {
        private Reader _reader;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                _reader = ReaderCollection.GetReaders()[0];
                image.Image = null;

                var result = _reader.Open(Constants.CapturePriority.DP_PRIORITY_COOPERATIVE);
                if (result != Constants.ResultCode.DP_SUCCESS)
                    MakeReport($"Ainda não está pronto: {result}");
                else
                    MakeReport("Pronto para iniciar");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        protected void MakeReport(string message)
        {
            Invoke(new Action(delegate ()
            {
                StatusText.Text = message;
            }));
        }

        //protected virtual void Proccess(DPFP.Sample sample)
        //{
        //    DrawImage(ConvertSampleToBitmap(sample));
        //}

        //protected Bitmap ConvertSampleToBitmap(DPFP.Sample sample)
        //{
        //    DPFP.Capture.SampleConversion conversor = new SampleConversion();
        //    Bitmap bitmap = null;
        //    conversor.ConvertToPicture(sample, ref bitmap);

        //    return bitmap;
        //}

        //private void DrawImage(Bitmap bitmap) => image.Image = new Bitmap(bitmap, image.Size);

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (_reader != null)
            {
                try
                {
                    //_reader.CaptureAsync();
                    MakeReport("Faça a leitura do dedo");
                }
                catch (Exception ex)
                {
                    MakeReport($"um erro aconteceu: {ex.Message}");
                }
            }
            else
                Form1_Load(sender, e);
        }
    }
}
