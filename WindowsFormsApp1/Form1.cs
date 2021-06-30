using AForge.Video;
using AForge.Video.DirectShow;
using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private FilterInfoCollection filterInfoCollection;
        private VideoCaptureDevice VideoCaptureDevice;

        private static readonly CascadeClassifier Face_Cascade1 = new CascadeClassifier("haarcascade_profileface.xml");
        private static readonly CascadeClassifier Face_Cascade2 = new CascadeClassifier("haarcascade_upperbody.xml");
        private static readonly CascadeClassifier Face_Cascade3 = new CascadeClassifier("haarcascade_frontalface_alt2.xml");
        private static readonly CascadeClassifier Face_Cascade4 = new CascadeClassifier("haarcascade_frontalface_default.xml");
        private static readonly CascadeClassifier Face_Cascade5 = new CascadeClassifier("haarcascade_frontalface_alt.xml");
        private static readonly CascadeClassifier Mouth_Cascade = new CascadeClassifier("Mouth.xml");
        private static readonly CascadeClassifier Nose_Cascade = new CascadeClassifier("Nose.xml");

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach(FilterInfo filterInfo in filterInfoCollection)            
                cbCamera.Items.Add(filterInfo.Name);

            cbCamera.SelectedIndex = 0;
            VideoCaptureDevice = new VideoCaptureDevice();
        }

        private void BDetectar_Click(object sender, EventArgs e)
        {
            if (!VideoCaptureDevice.IsRunning)
            {
                VideoCaptureDevice = new VideoCaptureDevice(filterInfoCollection[cbCamera.SelectedIndex].MonikerString);
                VideoCaptureDevice.NewFrame += VideoCaptureDevice_NewFrame;
                VideoCaptureDevice.Start();
            }
        }

        private void VideoCaptureDevice_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            TratarImagem((Bitmap)eventArgs.Frame.Clone());
        }

        private void TratarImagem(Bitmap bitmap)
        {
            try
            {
                bitmap.Save("img.bmp");
                var gray = new Image<Gray, byte>("img.bmp");
                gray.Save("rosto.bmp");

                var correcao = new Image<Gray, byte>("rosto.bmp");
                //var black_and_white = new Image<Bgr, byte>("img.bmp");

                //var faces1 = Face_Cascade1.DetectMultiScale(gray, 3);
                //var faces2 = Face_Cascade2.DetectMultiScale(gray, 3);
                //var faces3 = Face_Cascade3.DetectMultiScale(gray, 3);
                var faces4 = Face_Cascade4.DetectMultiScale(gray, 8);
                //var faces5 = Face_Cascade5.DetectMultiScale(gray, 6, 3);

                //if (faces1.Length > 0)
                //    TratarFace(bitmap, faces1);

                //if (faces2.Length > 0)
                //    TratarFace(bitmap, faces2);

                //if (faces3.Length > 0)
                //    TratarFace(bitmap, faces3);

                if (faces4.Length > 0)
                    TratarFace(bitmap, faces4);

                //if (faces5.Length > 0)
                //    TratarFace(bitmap, faces5);
            }

            finally { pbFoto.Image = bitmap; }
        }

        private void TratarFace(Bitmap bitmap, Rectangle[] faces)
        {
            var weared_mask = "Está usando mascara";
            var not_weared_mask = "Não esta usando mascara";

            foreach (var retangulo in faces)
            {
                var rosto = CropImage(bitmap, retangulo);
                rosto.Save("Rosto.bmp");
                var NovoGray = new Image<Gray, byte>("Rosto.bmp");
                var mouth_rects = Mouth_Cascade.DetectMultiScale(NovoGray, 5);
                //var nose_rects = Nose_Cascade.DetectMultiScale(NovoGray, 2, 1);
                if (mouth_rects.Length == 0)
                {
                    var greenBrush = new SolidBrush(Color.Green);
                    var graphics = Graphics.FromImage(bitmap);
                    graphics.DrawString(weared_mask, new Font("Arial", 10), greenBrush, retangulo.Location);
                    graphics.DrawRectangle(new Pen(Color.Green, 2), retangulo.X, retangulo.Y, retangulo.Width, retangulo.Height);
                }

                else
                {
                    foreach (var mouth in mouth_rects)
                    {
                        var redBrush = new SolidBrush(Color.Red);
                        var graphics = Graphics.FromImage(bitmap);
                        graphics.DrawString(not_weared_mask, new Font("Arial", 10), redBrush, retangulo.Location);
                        graphics.DrawRectangle(new Pen(Color.Red, 2), retangulo.X, retangulo.Y, retangulo.Width, retangulo.Height);
                    }
                }

                File.Delete("Rosto.bmp");
            }
        }

        private Bitmap CropImage(Image img, Rectangle cropArea)
        {
            var bmpImage = new Bitmap(img);
            return bmpImage.Clone(cropArea, bmpImage.PixelFormat);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (VideoCaptureDevice.IsRunning)
                VideoCaptureDevice.Stop();
        }
    }
}
