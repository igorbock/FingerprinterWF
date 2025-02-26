using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
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

                if (StartCapture(this.OnCaptured) == false)
                    Close();
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

        private bool StartCapture(Reader.CaptureCallback onCaptured)
        {
            _reader.On_Captured += new Reader.CaptureCallback(onCaptured);

            if (CaptureFinger() == false)
                return false;

            return true;
        }

        public bool CaptureFinger()
        {
            try
            {
                GetStatus();

                Constants.ResultCode captureResult = _reader.CaptureAsync(Constants.Formats.Fid.ANSI, Constants.CaptureProcessing.DP_IMG_PROC_DEFAULT, _reader.Capabilities.Resolutions[0]);
                if (captureResult != Constants.ResultCode.DP_SUCCESS)
                    throw new Exception("" + captureResult);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error:  " + ex.Message);
                return false;
            }
        }

        public void GetStatus()
        {
            Constants.ResultCode result = _reader.GetStatus();

            if ((result != Constants.ResultCode.DP_SUCCESS))
                throw new Exception("" + result);

            if ((_reader.Status.Status == Constants.ReaderStatuses.DP_STATUS_BUSY))
                Thread.Sleep(50);
            else if ((_reader.Status.Status == Constants.ReaderStatuses.DP_STATUS_NEED_CALIBRATION))
                _reader.Calibrate();
            else if ((_reader.Status.Status != Constants.ReaderStatuses.DP_STATUS_READY))
                throw new Exception("Reader Status - " + _reader.Status.Status);
        }

        public void OnCaptured(CaptureResult captureResult)
        {
            try
            {
                // Check capture quality and throw an error if bad.
                if (CheckCaptureResult(captureResult) == false)
                    return;

                // Create bitmap
                foreach (Fid.Fiv fiv in captureResult.Data.Views)
                    SendMessage(ActionEnum.SendBitmap, CreateBitmap(fiv.RawImage, fiv.Width, fiv.Height));
            }
            catch (Exception ex)
            {
                // Send error message, then close form
                SendMessage(ActionEnum.SendMessage, "Error:  " + ex.Message);
            }
        }

        public bool CheckCaptureResult(CaptureResult captureResult)
        {
            if (captureResult.Data == null)
            {
                if (captureResult.ResultCode != Constants.ResultCode.DP_SUCCESS)
                    throw new Exception(captureResult.ResultCode.ToString());

                if ((captureResult.Quality != Constants.CaptureQuality.DP_QUALITY_CANCELED))
                    throw new Exception("Quality - " + captureResult.Quality);

                return false;
            }

            return true;
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
                    Form1_Load(sender, e);
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

        public Bitmap CreateBitmap(byte[] bytes, int width, int height)
        {
            byte[] rgbBytes = new byte[bytes.Length * 3];

            for (int i = 0; i <= bytes.Length - 1; i++)
            {
                rgbBytes[(i * 3)] = bytes[i];
                rgbBytes[(i * 3) + 1] = bytes[i];
                rgbBytes[(i * 3) + 2] = bytes[i];
            }
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);

            BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            for (int i = 0; i <= bmp.Height - 1; i++)
            {
                IntPtr p = new IntPtr(data.Scan0.ToInt64() + data.Stride * i);
                System.Runtime.InteropServices.Marshal.Copy(rgbBytes, i * bmp.Width * 3, p, bmp.Width * 3);
            }

            bmp.UnlockBits(data);

            return bmp;
        }

        #region SendMessage
        private enum ActionEnum
        {
            SendBitmap,
            SendMessage
        }
        private delegate void SendMessageCallback(ActionEnum action, object payload);
        private void SendMessage(ActionEnum action, object payload)
        {
            try
            {
                if (this.image.InvokeRequired)
                {
                    SendMessageCallback d = new SendMessageCallback(SendMessage);
                    this.Invoke(d, new object[] { action, payload });
                }
                else
                {
                    switch (action)
                    {
                        case ActionEnum.SendMessage:
                            MessageBox.Show((string)payload);
                            break;
                        case ActionEnum.SendBitmap:
                            image.Image = (Bitmap)payload;
                            image.Refresh();
                            break;
                    }
                }
            }
            catch (Exception)
            {
            }
        }
        #endregion
    }
}
