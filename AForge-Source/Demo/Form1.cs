using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Demo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        CameraActiveX.UCCameraPlayer control = null;
        private void Form1_Load(object sender, EventArgs e)
        {
            control = new CameraActiveX.UCCameraPlayer();
            control.Dock = DockStyle.Fill;
            this.panel1.Controls.Add(control);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            control.DisposeControl();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            string picPath = Application.StartupPath + "\\save\\" + System.DateTime.Now.ToString("yyyyMMddhhmmss") + ".jpg";
            control.myGrabJpg(picPath);
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            string videoPath = Application.StartupPath + "\\save\\" + System.DateTime.Now.ToString("yyyyMMddhhmmss") + ".avi";
            control.myStartCapture(videoPath);
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            control.myStopCapture();
        }

        private void videoCaptureFilterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            control.videoCaptureFilter();
        }

        private void videoCapturePinToolStripMenuItem_Click(object sender, EventArgs e)
        {
            control.Test();
        }

        private void videoPreviewPinToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void videoCrossbarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            control.videoCrossbar();
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            Form1_Load(null, null);
        }
    }
}
