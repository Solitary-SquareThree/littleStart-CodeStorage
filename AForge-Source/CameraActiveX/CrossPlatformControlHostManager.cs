using System;
using System.Drawing;
using System.Windows.Forms;

namespace CameraActiveX
{
    /// <summary>
    /// 跨进程的承载控件管理器
    /// </summary>
    /// <remarks></remarks>
    internal class CrossPlatformControlHostManager
    {
        private IntPtr _ControlHandle = IntPtr.Zero;
        /// <summary>
        /// 操作的控件句柄对象
        /// </summary>
        public IntPtr ControlHandle
        {
            get { return _ControlHandle; }
            set { _ControlHandle = value; }
        }

        private IntPtr _ContainerHandle = IntPtr.Zero;
        /// <summary>
        /// 容器元素对象
        /// </summary>
        public IntPtr ContainerHandle
        {
            get { return _ContainerHandle; }
            set { _ContainerHandle = value; }
        }

        private DockStyle _Dock = DockStyle.Fill;
        /// <summary>
        /// 停靠样式
        /// </summary>
        public DockStyle Dock
        {
            get { return _Dock; }
            set { _Dock = value; }
        }

        /// <summary>
        /// 更新排版
        /// </summary>
        /// <returns>操作是否成功</returns>
        public bool UpdateLayout()
        {
            WindowInformation info = new WindowInformation(this.ControlHandle);
            WindowInformation container = new WindowInformation(this.ContainerHandle);
            if (info.CheckHandle() == false
                || container.CheckHandle() == false)
            {
                return false;
            }
            if (info.ParentHandle != container.Handle)
            {
                if (info.SetParent(container.Handle) == false)
                {
                    return false;
                }
            }
            Rectangle clientRect = container.ClientBounds;
            Rectangle bounds = info.Bounds;
            Rectangle descBounds = bounds;
            switch (this.Dock)
            {
                case DockStyle.Fill:
                    descBounds = clientRect;
                    break;
                case DockStyle.Bottom:
                    descBounds = new Rectangle(
                        0,
                        clientRect.Height - bounds.Height,
                        clientRect.Width,
                        bounds.Height);
                    break;
                case DockStyle.Left:
                    descBounds = new Rectangle(
                        0,
                        0,
                        bounds.Width,
                        clientRect.Height);
                    break;
                case DockStyle.Right:
                    descBounds = new Rectangle(
                        clientRect.Width - bounds.Width,
                        0,
                        bounds.Width,
                        clientRect.Height);
                    break;
                case DockStyle.Top:
                    descBounds = new Rectangle(
                        0,
                        0,
                        clientRect.Width,
                        bounds.Height);
                    break;
            }
            if (descBounds != bounds)
            {
                info.Bounds = descBounds;
            }

            return true;
        }
    }
}