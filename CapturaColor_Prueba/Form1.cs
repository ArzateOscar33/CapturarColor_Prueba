
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge;

namespace CapturaColor_Prueba
{
    public partial class Form1 : Form
    {
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice fuentev=null;
        private Color color; // Color predeterminado
        private BlobCounter blobCounter = new BlobCounter();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Enumerar dispositivos de video disponibles
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo device in videoDevices)
            {
                cboCameras.Items.Add(device.Name);
            }

            if (cboCameras.Items.Count > 0)
                cboCameras.SelectedIndex = 0;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            // Iniciar la captura de video con la cámara seleccionada
            fuentev = new VideoCaptureDevice(videoDevices[cboCameras.SelectedIndex].MonikerString);
            fuentev.NewFrame += VideoSource_NewFrame;
            fuentev.Start();
        }

        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            // Obtener el fotograma actual
            Bitmap video = (Bitmap)eventArgs.Frame.Clone();
            Bitmap temp = video.Clone() as Bitmap;

            // Filtrar el color seleccionado
            // ColorFiltering colorFilter = new ColorFiltering();
            //colorFilter.Red = new IntRange(selectedColor.R - 20, selectedColor.R + 20);
            //colorFilter.Green = new IntRange(selectedColor.G - 20, selectedColor.G + 20);
            //colorFilter.Blue = new IntRange(selectedColor.B - 20, selectedColor.B + 20);
            //colorFilter.FillOutsideRange = false;
            //creo filtro de color en el video temp
            EuclideanColorFiltering filter = new EuclideanColorFiltering();
            // Aplicar el filtro de color
            // Bitmap processedFrame = colorFilter.Apply(video);

            //le doy valores al cuadro de dialogo
            filter.CenterColor = new AForge.Imaging.RGB(color.R, color.G, color.B);
            filter.Radius = 90;

            //Aplico el filtro
            filter.ApplyInPlace(temp);

            // Aplicar detección de blobs
          //  blobCounter.ProcessImage(processedFrame);
           // Blob[] blobs = blobCounter.GetObjectsInformation();
           BlobCounter blobCounter = new BlobCounter();
            blobCounter.MinWidth = 5;
            blobCounter.MinHeight = 5;
            blobCounter.FilterBlobs = true;
            blobCounter.ProcessImage(temp);
            Rectangle[] rects=blobCounter.GetObjectsRectangles();

            // Dibujar recuadro alrededor de los blobs detectados
            foreach (Rectangle recs in rects)
            {
                Rectangle objectRect = rects[0];
                Graphics g = Graphics.FromImage(video);
                using (Pen pen = new Pen(Color.FromArgb(160,255,160), 5))
                {
                    g.DrawRectangle(pen, objectRect);
                }
                g.Dispose();
            }
            pbCamara2.Image=video;
            // Actualizar PictureBox con el fotograma procesado
            if (pictureBox.InvokeRequired)
            {
                pictureBox.Invoke(new MethodInvoker(delegate { pictureBox.Image = video; }));
            }
            else
            {
                pictureBox.Image = video;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Detener la captura de video al cerrar el formulario
            if (fuentev != null && fuentev.IsRunning)
            {
                fuentev.SignalToStop();
                fuentev.WaitForStop();
            }
        }

        private void cboCameras_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Cambiar la cámara en tiempo de ejecución
            if (fuentev != null && fuentev.IsRunning)
            {
                fuentev.SignalToStop();
                fuentev.WaitForStop();
                fuentev = null;
                btnStart_Click(sender, e);
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            // Detener la captura de video
            if (fuentev != null && fuentev.IsRunning)
            {
                fuentev.SignalToStop();
                fuentev.WaitForStop();
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            // Cerrar la aplicación
            this.Close();
        }

        private void BtnSelectColor_Click(object sender, EventArgs e)
        {
            // Mostrar el cuadro de diálogo de selección de color
            using (ColorDialog colorDialog = new ColorDialog())
            {
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    color = colorDialog.Color;
                }
            }
        }
    }
}
