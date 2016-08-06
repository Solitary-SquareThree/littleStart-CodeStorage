using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Video.VFW;
using System.IO;
using System.Collections.Generic;

namespace CameraActiveX
{
    [Guid("EA9837FB-2D31-40c0-BDE5-3E6C0A71CC62")]
    public partial class UCCameraPlayer : UserControl, IObjectSafety
    {
        #region IObjectSafety 成员

        private const string _IID_IDispatch = "{00020400-0000-0000-C000-000000000046}";
        private const string _IID_IDispatchEx = "{a6ef9860-c720-11d0-9337-00a0c90dcaa9}";
        private const string _IID_IPersistStorage = "{0000010A-0000-0000-C000-000000000046}";
        private const string _IID_IPersistStream = "{00000109-0000-0000-C000-000000000046}";
        private const string _IID_IPersistPropertyBag = "{37D84F60-42CB-11CE-8135-00AA004BB851}";

        private const int INTERFACESAFE_FOR_UNTRUSTED_CALLER = 0x00000001;
        private const int INTERFACESAFE_FOR_UNTRUSTED_DATA = 0x00000002;
        private const int S_OK = 0;
        private const int E_FAIL = unchecked((int)0x80004005);
        private const int E_NOINTERFACE = unchecked((int)0x80004002);

        private bool _fSafeForScripting = true;
        private bool _fSafeForInitializing = true;


        public int GetInterfaceSafetyOptions(ref Guid riid, ref int pdwSupportedOptions, ref int pdwEnabledOptions)
        {
            int Rslt = E_FAIL;

            string strGUID = riid.ToString("B");
            pdwSupportedOptions = INTERFACESAFE_FOR_UNTRUSTED_CALLER | INTERFACESAFE_FOR_UNTRUSTED_DATA;
            switch (strGUID)
            {
                case _IID_IDispatch:
                case _IID_IDispatchEx:
                    Rslt = S_OK;
                    pdwEnabledOptions = 0;
                    if (_fSafeForScripting == true)
                        pdwEnabledOptions = INTERFACESAFE_FOR_UNTRUSTED_CALLER;
                    break;
                case _IID_IPersistStorage:
                case _IID_IPersistStream:
                case _IID_IPersistPropertyBag:
                    Rslt = S_OK;
                    pdwEnabledOptions = 0;
                    if (_fSafeForInitializing == true)
                        pdwEnabledOptions = INTERFACESAFE_FOR_UNTRUSTED_DATA;
                    break;
                default:
                    Rslt = E_NOINTERFACE;
                    break;
            }

            return Rslt;
        }

        public int SetInterfaceSafetyOptions(ref Guid riid, int dwOptionSetMask, int dwEnabledOptions)
        {
            int Rslt = E_FAIL;

            string strGUID = riid.ToString("B");
            switch (strGUID)
            {
                case _IID_IDispatch:
                case _IID_IDispatchEx:
                    if (((dwEnabledOptions & dwOptionSetMask) == INTERFACESAFE_FOR_UNTRUSTED_CALLER) &&
                            (_fSafeForScripting == true))
                        Rslt = S_OK;
                    break;
                case _IID_IPersistStorage:
                case _IID_IPersistStream:
                case _IID_IPersistPropertyBag:
                    if (((dwEnabledOptions & dwOptionSetMask) == INTERFACESAFE_FOR_UNTRUSTED_DATA) &&
                            (_fSafeForInitializing == true))
                        Rslt = S_OK;
                    break;
                default:
                    Rslt = E_NOINTERFACE;
                    break;
            }
            return Rslt;
        }

        #endregion

        #region 公共方法属性

        private string _VideoFilePath = "";
        /// <summary>
        /// 视频文件完整路径
        /// </summary>
        public string VideoFilePath
        {
            get { return _VideoFilePath; }
            set { _VideoFilePath = value; }
        }
        private string _PictrueFilePath = "";
        /// <summary>
        /// 截图文件完整路径
        /// </summary>
        public string PictrueFilePath
        {
            get { return _PictrueFilePath; }
            set { _PictrueFilePath = value; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public UCCameraPlayer()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 释放控件
        /// </summary>
        public void DisposeControl()
        {
            myOnClose();
            this.Dispose();
        }
        /// <summary>
        /// 关闭摄像头连接
        /// </summary>
        public void myOnClose()
        {
            stopRec();
            if (videoSourcePlayer.VideoSource != null)
            {
                // stop video device
                videoSourcePlayer.SignalToStop();
                videoSourcePlayer.WaitForStop();
                videoSourcePlayer.VideoSource = null;

                if (videoDevice.ProvideSnapshots)
                {
                    videoDevice.SnapshotFrame -= new NewFrameEventHandler(videoDevice_SnapshotFrame);
                }
            }
        }
        /// <summary>
        /// 将控件填充到指定句柄的窗体中。
        /// 填充状态为Fill
        /// </summary>
        /// <param name="containerHandle">指定的窗体句柄对象</param>
        /// <returns>操作是否成功</returns>
        public bool AppInit(int containerHandle)
        {
            man = new CrossPlatformControlHostManager();
            man.ContainerHandle = new IntPtr(containerHandle);
            man.ControlHandle = this.Handle;
            man.Dock = DockStyle.Fill;
            return man.UpdateLayout();
        }
        /// <summary>
        /// 重置视频窗口大小
        /// </summary>
        public void myRightWindow() 
        {
            man.Dock = DockStyle.Fill;
            man.UpdateLayout();
        }
        /// <summary>
        /// 拍照
        /// </summary>
        /// <param name="path"></param>
        public void myGrabJpg(string path)
        {
            if (string.IsNullOrEmpty(path.Trim()))
            {
                MessageBox.Show("没有完整的图片保存路径，无法拍照！", "信息提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            CheckPathAndCreate(path);
            PictrueFilePath = path;
            if (videoDevice != null && videoDevice.ProvideSnapshots)
            {
                videoDevice.SimulateTrigger();
            }
        }
        /// <summary>
        /// 开始录像
        /// </summary>
        /// <param name="filePath">录像生成avi文件的完整路径</param>
        public void myStartCapture(string filePath)
        {
            if (string.IsNullOrEmpty(filePath.Trim()))
            {
                MessageBox.Show("没有指定录像保存路径，无法录像！", "信息提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            CheckPathAndCreate(filePath);
            VideoFilePath = filePath;
            if (!running)
            {
                try
                {
                    if (writer == null)
                        writer = new AVIWriter("wmv3");
                    writer.Open(VideoFilePath, videoDevice.DesiredFrameSize.Width, videoDevice.DesiredFrameSize.Height);

                    running = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("录像操作失败！\r\n" + ex.Message, "错误提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        /// <summary>
        /// 停止录像
        /// </summary>
        public void myStopCapture()
        {
            stopRec();
        }

        public void videoCaptureFilter() 
        {
            if ((videoDevice != null) && (videoDevice is VideoCaptureDevice))
             {
                 try
                 {
                     ((VideoCaptureDevice)videoDevice).DisplayPropertyPage(this.Handle);
                     //videoCapturePin
                     //videoPreviewPin
                     //((VideoCaptureDevice)videoDevice).
                 }
                 catch (NotSupportedException ex)
                 {
                     MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                 }
             }
        }

        public void videoCrossbar() 
        {
            if ((videoDevice != null) && (videoDevice is VideoCaptureDevice))
             {
                 try
                 {
                     //videoCrossbar
                     ((VideoCaptureDevice)videoDevice).DisplayCrossbarPropertyPage(this.Handle);
                 }
                 catch (NotSupportedException ex)
                 {
                     MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                 }
             }
        }

        public void Test() 
        {
            VideoCaptureDeviceForm form = new VideoCaptureDeviceForm();
            form.ShowDialog();
        }

        #endregion

        #region 私有方法属性
        private CrossPlatformControlHostManager man = null;
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoDevice;
        private VideoCapabilities[] videoCapabilities;
        private VideoCapabilities[] snapshotCapabilities;
        private AVIWriter writer;
        /// <summary>
        /// 是否正在录像标识
        /// </summary>
        private bool running = false;
        /// <summary>
        /// 控件初始化加载事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UCCameraPlayer_Load(object sender, EventArgs e)
        {
            // enumerate video devices
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            if (videoDevices.Count > 1)
            {
                MessageBox.Show("发现多个摄像设备！请去除多余设备再运行！", "信息提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (videoDevices.Count <= 0)
            {
                MessageBox.Show("没有发现摄像设备！", "信息提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (videoDevices.Count == 1)
            {
                string videoName = videoDevices[0].Name;
                videoDevice = new VideoCaptureDevice(videoDevices[0].MonikerString);
                videoDevice.DesiredFrameSize = this.Size;
                videoCapabilities = videoDevice.VideoCapabilities;
                snapshotCapabilities = videoDevice.SnapshotCapabilities;

                List<Size> sizeList = videoDevice.GetCaptureSupportSize();
            }

            ConnectCamera();
        }
        /// <summary>
        /// 打开摄像头连接
        /// </summary>
        private void ConnectCamera()
        {
            if (videoDevice != null)
            {
                if (videoCapabilities != null && videoCapabilities.Length > 0)
                {
                    videoDevice.DesiredFrameSize = videoCapabilities[0].FrameSize;
                    videoSourcePlayer.NewFrame += new AForge.Controls.VideoSourcePlayer.NewFrameHandler(videoSourcePlayer_NewFrame);
                }

                if (snapshotCapabilities != null && snapshotCapabilities.Length > 0)
                {
                    videoDevice.ProvideSnapshots = true;
                    videoDevice.DesiredSnapshotSize = snapshotCapabilities[0].FrameSize;
                    videoDevice.SnapshotFrame += new NewFrameEventHandler(videoDevice_SnapshotFrame);
                }

                videoSourcePlayer.VideoSource = videoDevice;
                videoSourcePlayer.Start();
            }
        }
        void videoSourcePlayer_NewFrame(object sender, ref Bitmap image)
        {
            if (running)
                writer.AddFrame(image);
        }
        // New snapshot frame is available
        void videoDevice_SnapshotFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if (!string.IsNullOrEmpty(PictrueFilePath.Trim()))
            {
                Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();
                try
                {
                    lock (this)
                    {
                        bitmap.Save(PictrueFilePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                        MessageBox.Show("图片已保存！保存路径为：\r\n" + PictrueFilePath, "信息提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("图片保存失败！\r\n" + ex.Message, "错误提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        /// <summary>
        /// 停止录像
        /// </summary>
        private void stopRec()
        {
            if (running)
            {
                running = false;
                writer.Close();
            }
        }
        /// <summary>
        /// 检查文件目录是否存在，不存在就创建文件目录
        /// </summary>
        /// <param name="path"></param>
        private void CheckPathAndCreate(string path)
        {
            string fileDir = path.Substring(0, path.LastIndexOf("\\"));
            if (!Directory.Exists(fileDir))
            {
                Directory.CreateDirectory(fileDir);
            }
        }

        #endregion
    }
}
