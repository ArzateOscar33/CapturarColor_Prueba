using AForge.Video.DirectShow;
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

using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge;

namespace CapturaColor_Prueba
{
    public partial class Form1 : Form
    {
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;
        private Color selectedColor = Color.Red; // Color predeterminado
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
            videoSource = new VideoCaptureDevice(videoDevices[cboCameras.SelectedIndex].MonikerString);
            videoSource.NewFrame += VideoSource_NewFrame;
            videoSource.Start();
        }

        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            // Obtener el fotograma actual
            Bitmap videoFrame = (Bitmap)eventArgs.Frame.Clone();

            // Filtrar el color seleccionado
            ColorFiltering colorFilter = new ColorFiltering();
            colorFilter.Red = new IntRange(selectedColor.R - 20, selectedColor.R + 20);
            colorFilter.Green = new IntRange(selectedColor.G - 20, selectedColor.G + 20);
            colorFilter.Blue = new IntRange(selectedColor.B - 20, selectedColor.B + 20);
            colorFilter.FillOutsideRange = false;

            // Aplicar el filtro de color
            Bitmap processedFrame = colorFilter.Apply(videoFrame);

            // Aplicar detección de blobs
            blobCounter.ProcessImage(processedFrame);
            Blob[] blobs = blobCounter.GetObjectsInformation();

            // Dibujar recuadro alrededor de los blobs detectados
            foreach (Blob blob in blobs)
            {
                Rectangle rect = blob.Rectangle;
                Graphics g = Graphics.FromImage(videoFrame);
                using (Pen pen = new Pen(Color.Yellow, 2))
                {
                    g.DrawRectangle(pen, rect);
                }
            }
            // Actualizar PictureBox con el fotograma procesado
            if (pictureBox.InvokeRequired)
            {
                pictureBox.Invoke(new MethodInvoker(delegate { pictureBox.Image = videoFrame; }));
            }
            else
            {
                pictureBox.Image = videoFrame;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Detener la captura de video al cerrar el formulario
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.SignalToStop();
                videoSource.WaitForStop();
            }
        }

        private void cboCameras_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Cambiar la cámara en tiempo de ejecución
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.SignalToStop();
                videoSource.WaitForStop();
                videoSource = null;
                btnStart_Click(sender, e);
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            // Detener la captura de video
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.SignalToStop();
                videoSource.WaitForStop();
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
                    selectedColor = colorDialog.Color;
                }
            }
        }
    }
}
