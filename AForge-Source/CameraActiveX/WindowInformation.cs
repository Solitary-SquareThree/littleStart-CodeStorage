using System;
using System.Runtime.InteropServices;

namespace CameraActiveX
{
    /// <summary>
    /// Windows����ϵͳ������Ϣ����
    /// </summary>
    /// <remarks>
    /// ��������Windows������ص�API�������йܰ�װ.
    /// 
    /// </remarks>
    internal class WindowInformation : System.Windows.Forms.IWin32Window
    {
        /// <summary>
        /// �������洰����Ϣ����
        /// </summary>
        public static WindowInformation DeskTop
        {
            get
            {
                IntPtr hwnd = GetDesktopWindow();
                return new WindowInformation(hwnd);
            }
        }

        /// <summary>
        /// ��õ�ǰ�������뽹��Ĵ������
        /// </summary>
        /// <returns>�������</returns>
        public static WindowInformation GetFocusWindow()
        {
            IntPtr hwnd = GetFocus();
            if (hwnd != IntPtr.Zero)
            {
                return new WindowInformation(hwnd);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// ��ȫ�ĸ��ݴ�������������
        /// </summary>
        /// <param name="handle">����������</param>
        /// <returns>��õĴ�����Ϣ����,�������Ч�򷵻ؿ�����</returns>
        public static WindowInformation FromHandle(System.IntPtr handle)
        {
            if (CheckHandle(handle))
            {
                WindowInformation info = new WindowInformation(handle);
                return info;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// ��ʼ������
        /// </summary>
        /// <param name="handle">������</param>
        public WindowInformation(IntPtr handle)
        {
            myControl = new Win32Handle(handle);
        }
        /// <summary>
        /// ��ʼ������
        /// </summary>
        /// <param name="win32">�������</param>
        public WindowInformation(System.Windows.Forms.IWin32Window win32)
        {
            myControl = win32;
        }

        private bool bolThrowWin32Exception = false;
        /// <summary>
        /// ���ڲ�����ʧ���Ƿ��׳�Win32�쳣
        /// </summary>
        public bool ThrowWin32Exception
        {
            get
            {
                return bolThrowWin32Exception;
            }
            set
            {
                bolThrowWin32Exception = value;
            }
        }

        /// <summary>
        /// ������
        /// </summary>
        public System.IntPtr Handle
        {
            get
            {
                if (this.CheckHandle())
                {
                    return myControl.Handle;
                }
                else
                {
                    return IntPtr.Zero;
                }
            }
        }

        /// <summary>
        /// ����ClassName
        /// </summary>
        public string ClassName
        {
            get
            {
                if (CheckHandle())
                {
                    int len = 1000;
                    byte[] bs = new byte[1000];
                    len = GetClassName(myControl.Handle, bs, bs.Length);
                    if (len == 0)
                    {
                        InnerThrowWin32Exception();
                    }
                    else
                    {
                        string txt = System.Text.Encoding.Unicode.GetString(bs, 0, len * 2);
                        return txt;
                    }
                }
                return null ;
            }
        }

        /// <summary>
        /// �����ı�
        /// </summary>
        public string Text
        {
            get
            {
                if (CheckHandle())
                {
                    int len = GetWindowTextLength(myControl.Handle);
                    if (len > 0)
                    {
                        System.Text.StringBuilder str = new System.Text.StringBuilder(len + 1);
                        if (GetWindowText(myControl.Handle, str, str.Capacity) == 0)
                        {
                            InnerThrowWin32Exception();
                        }
                        return str.ToString();
                    }
                    else if (len == 0)
                    {
                        InnerThrowWin32Exception();
                    }
                }
                return "";
            }
            set
            {
                if (this.CheckHandle())
                {
                    if (value == null)
                        value = "";
                    if (SetWindowText(myControl.Handle, value) == false)
                    {
                        InnerThrowWin32Exception();
                    }
                }
            }
        }

        /// <summary>
        /// ����ͻ����߽�
        /// </summary>
        public System.Drawing.Rectangle ClientBounds
        {
            get
            {
                if (this.CheckHandle())
                {
                    RECT rect = new RECT();
                    if (GetClientRect(myControl.Handle, ref rect) == false)
                    {
                        InnerThrowWin32Exception();
                    }
                    return new System.Drawing.Rectangle(
                        rect.left,
                        rect.top,
                        rect.right - rect.left,
                        rect.bottom - rect.top);
                }
                else
                {
                    return System.Drawing.Rectangle.Empty;
                }
            }
        }

        /// <summary>
        /// ����߽�
        /// </summary>
        public System.Drawing.Rectangle Bounds
        {
            get
            {
                if (this.CheckHandle())
                {
                    RECT rect = new RECT();
                    if (GetWindowRect(myControl.Handle, ref rect) == false)
                    {
                        InnerThrowWin32Exception();
                    }
                    return new System.Drawing.Rectangle(
                        rect.left,
                        rect.top,
                        rect.right - rect.left,
                        rect.bottom - rect.top);
                }
                else
                {
                    return System.Drawing.Rectangle.Empty;
                }
            }
            set
            {
                if (this.CheckHandle())
                {
                    if (SetWindowPos(
                        myControl.Handle,
                        IntPtr.Zero,
                        value.Left,
                        value.Top,
                        value.Width,
                        value.Height,
                        20) == false)
                    {
                        InnerThrowWin32Exception();
                    }
                }
            }
        }
        /// <summary>
        /// �����Ƿ����
        /// </summary>
        public bool Enabled
        {
            get
            {
                if (this.CheckHandle())
                    return IsWindowEnabled(myControl.Handle);
                else
                    return false;
            }
            set
            {
                if (this.CheckHandle())
                {
                    EnableWindow(myControl.Handle, value);
                }
            }
        }
        /// <summary>
        /// �����Ƿ�ɼ�
        /// </summary>
        public bool Visible
        {
            get
            {
                if (this.CheckHandle())
                {
                    return IsWindowVisible(myControl.Handle);
                }
                else
                    return false;
            }
            set
            {
                if (this.CheckHandle())
                {
                    SetWindowPos(
                        myControl.Handle,
                        IntPtr.Zero,
                        0,
                        0,
                        0,
                        0,
                        0x17 | (value ? 0x40 : 0x80));
                }
            }
        }
        /// <summary>
        /// ������ʽ
        /// </summary>
        public int WindowStyle
        {
            get
            {
                if (this.CheckHandle())
                {
                    int result = (int)GetWindowLong(myControl.Handle, GWL_STYLE);
                    if (result == 0)
                    {
                        this.InnerThrowWin32Exception();
                    }
                    return result;
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                if (this.CheckHandle())
                {
                    if (SetWindowLong(myControl.Handle, GWL_STYLE, new IntPtr(value)) == IntPtr.Zero)
                    {
                        this.InnerThrowWin32Exception();
                    }
                }
            }
        }
        /// <summary>
        /// ������չ��ʽ
        /// </summary>
        public int WindowExStyle
        {
            get
            {
                if (this.CheckHandle())
                {
                    int result = (int)GetWindowLong(myControl.Handle, GWL_EXSTYLE);
                    if (result == 0)
                    {
                        this.InnerThrowWin32Exception();
                    }
                    return result;
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                if (this.CheckHandle())
                {
                    if (SetWindowLong(this.Handle, GWL_EXSTYLE, new IntPtr(value)) == IntPtr.Zero)
                    {
                        this.InnerThrowWin32Exception();
                    }
                }
            }
        }

        /// <summary>
        /// ��������,��û�и������򷵻���
        /// </summary>
        public IntPtr ParentHandle
        {
            get
            {
                if (this.CheckHandle())
                {
                    IntPtr ptr = GetParent(this.Handle);
                    if (ptr == IntPtr.Zero)
                    {
                        this.InnerThrowWin32Exception();
                    }
                    return ptr;
                }
                return IntPtr.Zero;
            }
        }

        /// <summary>
        /// ����״̬
        /// </summary>
        public System.Windows.Forms.FormWindowState WindowState
        {
            get
            {
                if (this.CheckHandle())
                {
                    WINDOWPLACEMENT place = new WINDOWPLACEMENT();
                    int result = GetWindowPlacement(this.Handle, ref place);
                    if (result == 0)
                    {
                        InnerThrowWin32Exception();
                    }
                    int cmd = place.showCmd;
                    if (cmd == SW_SHOWNORMAL || cmd == SW_SHOWNOACTIVATE || cmd == SW_SHOW || cmd == SW_RESTORE)
                        return System.Windows.Forms.FormWindowState.Normal;
                    if (cmd == SW_SHOWMINIMIZED || cmd == SW_MINIMIZE || cmd == SW_SHOWMINNOACTIVE)
                        return System.Windows.Forms.FormWindowState.Minimized;
                    if (cmd == SW_SHOWMAXIMIZED)
                        return System.Windows.Forms.FormWindowState.Maximized;
                    if (IsIconic(this.Handle))
                        return System.Windows.Forms.FormWindowState.Minimized;
                }
                return System.Windows.Forms.FormWindowState.Minimized;
            }
            set
            {
                if (this.CheckHandle())
                {
                    switch (value)
                    {
                        case System.Windows.Forms.FormWindowState.Maximized:
                            ShowWindow(this.Handle, SW_MAXIMIZE);
                            break;
                        case System.Windows.Forms.FormWindowState.Minimized:
                            ShowWindow(this.Handle, SW_MINIMIZE);
                            break;
                        case System.Windows.Forms.FormWindowState.Normal:
                            ShowWindow(this.Handle, SW_NORMAL);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// ��������Ϣ����
        /// </summary>
        public WindowInformation Parent
        {
            get
            {
                if (this.CheckHandle())
                {
                    IntPtr h = GetParent(this.Handle);
                    if (h == IntPtr.Zero)
                    {
                        this.InnerThrowWin32Exception();
                        return null;
                    }
                    else
                    {
                        return new WindowInformation(h);
                    }
                }
                return null;
            }
        }

        private System.Drawing.Icon myIcon = null;
        /// <summary>
        /// ����Сͼ�����
        /// </summary>
        public System.Drawing.Icon Icon
        {
            get
            {
                if (this.CheckHandle())
                {
                    if (myIcon == null)
                    {
                        IntPtr h = IntPtr.Zero;
                        if (System.Environment.OSVersion.Version >= new Version("6.1"))
                        {
                            // ����WindowsXP�������ϰ汾����ʹ�� ICON_SMALL2 ����
                            h = SendMessage(this.Handle, WM_GETICON, ICON_SMALL2, 0);
                        }
                        if (h == IntPtr.Zero)
                        {
                            h = SendMessage(this.Handle, WM_GETICON, ICON_SMALL, 0);
                        }
                        if (h == IntPtr.Zero)
                        {
                            h = SendMessage(this.Handle, WM_GETICON, ICON_BIG, 0);
                        }
                        if (h == IntPtr.Zero)
                        {
                            h = GetClassLong(this.Handle, GCLP_HICON);
                        }
                        if (h != IntPtr.Zero)
                        {
                            System.Drawing.Icon icon = System.Drawing.Icon.FromHandle(h);
                            if (icon.Width == 0 || icon.Height == 0)
                            {
                                icon.Dispose();
                                return null;
                            }
                            myIcon = icon;
                        }
                    }//if
                    return myIcon;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (this.CheckHandle())
                {
                    SendMessage(this.Handle, 0x80, ICON_SMALL, value == null ? 0 : value.Handle.ToInt32());
                    myIcon = value;
                }
            }
        }


        /// <summary>
        /// �ж�ָ������Ĵ����Ǹ�����
        /// </summary>
        /// <param name="parentHandle">������</param>
        /// <returns>�Ƿ��Ǹ�����</returns>
        public bool IsParent(IntPtr parentHandle)
        {
            if (parentHandle == IntPtr.Zero
                || IsWindow(parentHandle) == false)
            {
                return false;
            }
            if (this.CheckHandle())
            {

                IntPtr p = this.Handle;
                while (p != IntPtr.Zero)
                {
                    p = GetParent(p);
                    if (p == parentHandle)
                        return true;
                    if (p == IntPtr.Zero)
                        break;
                }
            }
            return false;
        }

        public bool SetParent(IntPtr newParentHandle)
        {
            if (newParentHandle == IntPtr.Zero
                || IsWindow(newParentHandle) == false)
            {
                return false;
            }
            if (this.CheckHandle())
            {
                IntPtr result = SetParent(this.Handle, newParentHandle);
                if (result == IntPtr.Zero)
                {
                    //if (ThrowWin32Exception)
                    {
                        InnerThrowWin32Exception();
                    }
                    return false;
                }
                return true;
            }
            return false;
        }
         
        /// <summary>
        /// ��ô����ô��ڵĽ��̶���
        /// </summary>
        /// <returns>��õĽ��̶���</returns>
        public System.Diagnostics.Process GetOwnerProcess()
        {
            if (this.CheckHandle())
            {
                int id = 0;
                int thread = GetWindowThreadProcessId(this.Handle, ref id);
                if (id != 0)
                {
                    return System.Diagnostics.Process.GetProcessById(id);
                }
            }
            return null;
        }

        /// <summary>
        /// ��ö������������
        /// </summary>
        /// <returns>��õĶ������������</returns>
        public WindowInformation GetTopLevelParentWindow()
        {
            WindowInformation[] ws = DesktopWindows;
            WindowInformation info = this;
            while (info != null)
            {
                foreach (WindowInformation w in ws)
                {
                    if (w.Handle == info.Handle)
                        return w;
                }
                info = info.Parent;
            }
            return null;
        }

        /// <summary>
        /// ���͹رմ�����Ϣ���رմ���
        /// </summary>
        public void Close()
        {
            if (this.CheckHandle())
            {
                SendMessage(this.Handle, 0x10, 0, 0);
            }
        }

        /// <summary>
        /// ���ؼ����ڴ����ƶ���������
        /// </summary>
        public void BringToTop()
        {
            if (this.CheckHandle())
            {
                if (BringWindowToTop(this.Handle) == false)
                {
                    this.InnerThrowWin32Exception();
                }
            }
        }
        /// <summary>
        /// ���ô���Ϊ�����
        /// </summary>
        public void Activate()
        {
            if (this.CheckHandle())
            {
                SetForegroundWindow(this.Handle);
            }
        }
        /// <summary>
        /// ��˸����
        /// </summary>
        public void Flash()
        {
            if (this.CheckHandle())
            {
                FlashWindow(this.Handle, true);
            }
        }
        /// <summary>
        /// �жϵ�ǰ����Ƿ��Ǵ�����
        /// </summary>
        /// <returns>�Ƿ��Ǵ�����</returns>
        public bool CheckHandle()
        {
            if (myControl.Handle == IntPtr.Zero)
            {
                return false;
            }
            if (IsWindow(myControl.Handle) == false)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// �ж�ָ���ľ���Ƿ��Ǵ�����
        /// </summary>
        /// <param name="handle">���ֵ</param>
        /// <returns>�Ƿ��Ǵ�����</returns>
        public static bool CheckHandle(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
            {
                return false;
            }
            if (IsWindow(handle) == false)
            {
                return false;
            }
            return true;
        }

        [NonSerialized()]
        private System.Windows.Forms.IWin32Window myControl = null;

        private class Win32Handle : System.Windows.Forms.IWin32Window
        {
            public Win32Handle()
            {
            }
            public Win32Handle(IntPtr h)
            {
                handle = h;
            }
            public IntPtr handle = IntPtr.Zero;

            public System.IntPtr Handle
            {
                get
                {
                    return handle;
                }
            }
        }

        private static System.Collections.ArrayList myDesktopWindows = null;
        /// <summary>
        /// ��ǰ���������еĶ���������Ϣ����
        /// </summary>
        public static WindowInformation[] DesktopWindows
        {
            get
            {
                IntPtr handle = GetThreadDesktop(GetCurrentThreadId());
                if (handle != IntPtr.Zero)
                {
                    lock (typeof(WindowInformation))
                    {
                        myDesktopWindows = new System.Collections.ArrayList();
                        EnumDesktopWindows(handle, new EnumDesktopWindowsProc(MyEnumDesktopWindows), handle);
                        CloseDesktop(handle);
                        WindowInformation[] infos = (WindowInformation[])myDesktopWindows.ToArray(typeof(WindowInformation));
                        myDesktopWindows = null;
                        return infos;
                    }
                }
                else
                {
                    throw new System.ComponentModel.Win32Exception();
                    //return null;
                }
            }
        }

        private static bool MyEnumDesktopWindows(IntPtr hwnd, IntPtr lParam)
        {
            IntPtr threadId = GetWindowThreadProcessId(hwnd, 0);
            IntPtr desk = GetThreadDesktop(threadId);
            CloseDesktop(desk);
            if (desk == lParam)
            {
                WindowInformation info = new WindowInformation(hwnd);
                if (info.Visible == false)
                {
                    return true;
                }
                string title = info.Text;
                if (title == null || title.Length == 0)
                {
                    return true;
                }
                myDesktopWindows.Add(info);
                //return true;
            }
            else
            {
                //return false;
            }
            return true;
        }

        private void InnerThrowWin32Exception()
        {
            int err = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
            if (err != 0)
            {
                System.ComponentModel.Win32Exception ext = new System.ComponentModel.Win32Exception(err);
                if (this.ThrowWin32Exception)
                {
                    throw ext;
                }
            }
        }

        #region ���� Win32API���� *********************************************

        [DllImport("user32.dll")]
        private static extern IntPtr GetFocus();

        [DllImport("user32.dll", EntryPoint = "GetWindowPlacement", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern int GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT placement);

        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public int ptMinPosition_x;
            public int ptMinPosition_y;
            public int ptMaxPosition_x;
            public int ptMaxPosition_y;
            public int rcNormalPosition_left;
            public int rcNormalPosition_top;
            public int rcNormalPosition_right;
            public int rcNormalPosition_bottom;
        }

        [DllImport("user32.dll", EntryPoint = "GetWindowThreadProcessId")]
        private static extern int GetWindowThreadProcessId(IntPtr hwnd, ref int PorcessId);

        [DllImport("user32.dll", EntryPoint = "IsIconic")]
        private static extern bool IsIconic(IntPtr hwnd);

        [DllImport("user32.dll", EntryPoint = "GetDesktopWindow")]
        private static extern IntPtr GetDesktopWindow();

        private const int GCLP_HICON = -14;

        [DllImport("user32.dll", EntryPoint = "GetWindowThreadProcessId")]
        private static extern IntPtr GetWindowThreadProcessId(IntPtr hwnd, int p);

        [DllImport("user32.dll", EntryPoint = "GetThreadDesktop")]
        private static extern IntPtr GetThreadDesktop(IntPtr dwThreadId);

        [DllImport("user32.dll", EntryPoint = "CloseDesktop")]
        private static extern bool CloseDesktop(IntPtr hDesktop);

        [DllImport("Kernel32.dll", EntryPoint = "GetCurrentThreadId")]
        private static extern IntPtr GetCurrentThreadId();

        private delegate bool EnumDesktopWindowsProc(IntPtr desktopHandle, IntPtr lParam);


        [DllImport("user32.dll", EntryPoint = "EnumDesktopWindows")]
        private static extern bool EnumDesktopWindows(
            IntPtr hwnd,
            EnumDesktopWindowsProc lpfn,
            IntPtr lParam);

        private const int ICON_SMALL = 0;
        private const int ICON_BIG = 1;
        private const int ICON_SMALL2 = 2;

        private const int WM_GETICON = 0x007F;
        private const int WM_SETICON = 0x0080;

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
            //public RECT(int left, int top, int right, int bottom);
        }

        [DllImport("user32.dll", EntryPoint = "BringWindowToTop", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "GetParent", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "SetParent", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr SetParent(IntPtr hWndChild , IntPtr hWndNewParent);

        [DllImport("user32.dll", EntryPoint = "FlashWindow", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern bool FlashWindow(IntPtr hWnd, bool invert);

        [DllImport("user32.dll", EntryPoint = "SetForegroundWindow", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        [DllImport("user32.dll", EntryPoint = "ShowWindow", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_HIDE = 0;
        private const int SW_SHOWNORMAL = 1;
        private const int SW_NORMAL = 1;
        private const int SW_SHOWMINIMIZED = 2;
        private const int SW_SHOWMAXIMIZED = 3;
        private const int SW_MAXIMIZE = 3;
        private const int SW_SHOWNOACTIVATE = 4;
        private const int SW_SHOW = 5;
        private const int SW_MINIMIZE = 6;
        private const int SW_SHOWMINNOACTIVE = 7;
        private const int SW_SHOWNA = 8;
        private const int SW_RESTORE = 9;
        private const int SW_SHOWDEFAULT = 10;
        private const int SW_FORCEMINIMIZE = 11;
        private const int SW_MAX = 11;

        [DllImport("user32.dll", EntryPoint = "EnableWindow", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern bool EnableWindow(IntPtr hWnd, bool enable);

        [DllImport("user32.dll", EntryPoint = "IsWindowEnabled", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern bool IsWindowEnabled(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "IsWindowVisible", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "SetWindowPos", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int flags);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", CharSet = CharSet.Auto)]
        private static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);



        [DllImport("user32.dll", EntryPoint = "GetClassLong")]
        private static extern IntPtr GetClassLong(IntPtr hwnd, int param);


        [DllImport("user32.dll", EntryPoint = "GetWindowLong", CharSet = CharSet.Auto)]
        private static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        /*
         * Window field offsets for GetWindowLong()
         */
        private const int GWL_WNDPROC = -4;
        private const int GWL_HINSTANCE = -6;
        private const int GWL_HWNDPARENT = -8;
        private const int GWL_STYLE = -16;
        private const int GWL_EXSTYLE = -20;
        private const int GWL_USERDATA = -21;
        private const int GWL_ID = -12;

        /*
         * Class field offsets for GetClassLong()
         */
        private const int GCL_MENUNAME = -8;
        private const int GCL_HBRBACKGROUND = -10;
        private const int GCL_HCURSOR = -12;
        private const int GCL_HICON = -14;
        private const int GCL_HMODULE = -16;
        private const int GCL_CBWNDEXTRA = -18;
        private const int GCL_CBCLSEXTRA = -20;
        private const int GCL_WNDPROC = -24;
        private const int GCL_STYLE = -26;
        private const int GCW_ATOM = -32;


        [DllImport("user32.dll", EntryPoint = "GetWindowRect", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern bool GetWindowRect(IntPtr hWnd, ref RECT rect);

        [DllImport("user32.dll", EntryPoint = "GetClientRect", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern bool GetClientRect(IntPtr hWnd, ref RECT rect);


        [DllImport("user32.dll", EntryPoint = "IsWindow")]
        private extern static bool IsWindow(IntPtr hwnd);

        [DllImport("user32.dll", EntryPoint = "GetWindowTextLength", CharSet = CharSet.Auto)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "GetWindowText", CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", EntryPoint = "GetClassName", CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, byte[] lpString, int nMaxCount);


        [DllImport("user32.dll", EntryPoint = "SetWindowText", CharSet = CharSet.Auto)]
        private static extern bool SetWindowText(IntPtr hWnd, string text);


        #endregion

    }//public class WindowInformation
}