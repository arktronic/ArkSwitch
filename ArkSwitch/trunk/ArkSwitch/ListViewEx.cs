/*********************************************************************************************
 * ArkSwitch
 * Created by Arktronic - http://www.arktronic.com
 * Licensed under Ms-RL - http://www.opensource.org/licenses/ms-rl.html
*********************************************************************************************/

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ArkSwitch
{
    static class ListViewEx
    {
        public class WndHandler
        {
            private IntPtr _prevWndProc;
            private ListView _listView;
            private GCHandle _gch;

            public delegate void Draw(IntPtr hdc, int item, int subitem, bool selected, RectangleF rect);
            public delegate void Click(Point location, int item, int subitem);

            public event Draw DrawEvent;
            public event Click ClickEvent;

            public WndHandler(IntPtr prevWndProc, ListView listView)
            {
                _prevWndProc = prevWndProc;
                _listView = listView;
                _gch = GCHandle.Alloc(this);
                SetWindowLong(listView.Parent.Handle, GWL_WNDPROC, WndProc);
            }

            /// <summary>
            /// That's right, a destructor!
            /// </summary>
            ~WndHandler()
            {
                _gch.Free();
            }

            /// <summary>
            /// This is the overridden window procedure of the ListView's parent.
            /// </summary>
            /// <param name="hWnd"></param>
            /// <param name="msg"></param>
            /// <param name="wParam"></param>
            /// <param name="lParam"></param>
            /// <returns></returns>
            IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
            {
                if (msg == WM_NOTIFY)
                {
                    var nmhdr = (NMHDR)Marshal.PtrToStructure(lParam, typeof(NMHDR));
                    if (nmhdr.hwndFrom == _listView.Handle)
                    {
                        switch (nmhdr.code)
                        {
                            case NM_CUSTOMDRAW:
                                if (DrawEvent != null) return CustomDraw(lParam);
                                break;
                            case NM_CLICK:
                                if (ClickEvent != null) return ItemClick(lParam);
                                break;
                        }
                    }

                }

                // Very important to continue on with event processing.
                return CallWindowProc(_prevWndProc, hWnd, msg, wParam, lParam);
            }

            /// <summary>
            /// Called when a click event occurs.
            /// </summary>
            /// <param name="lParam"></param>
            /// <returns></returns>
            private IntPtr ItemClick(IntPtr lParam)
            {
                var nmlv = (NMLISTVIEW)Marshal.PtrToStructure(lParam, typeof(NMLISTVIEW));
                if (ClickEvent != null)
                    ClickEvent(new Point(nmlv.ptAction.x, nmlv.ptAction.y), nmlv.iItem, nmlv.iSubItem);
                return IntPtr.Zero;
            }

            /// <summary>
            /// Called when a draw event occurs.
            /// </summary>
            /// <param name="lParam"></param>
            /// <returns></returns>
            private IntPtr CustomDraw(IntPtr lParam)
            {
                int result;
                var nmlvcd = (NMLVCUSTOMDRAW)Marshal.PtrToStructure(lParam, typeof(NMLVCUSTOMDRAW));
                switch (nmlvcd.nmcd.dwDrawStage)
                {
                    case CDDS_PREPAINT:
                        result = CDRF_NOTIFYITEMDRAW;
                        break;

                    case CDDS_ITEMPREPAINT:
                        var index1 = nmlvcd.nmcd.dwItemSpec;
                        var rect1 = new RECT();
                        SendMessage(_listView.Handle, LVM_GETITEMRECT, index1, ref rect1);

                        if (DrawEvent != null)
                            DrawEvent(nmlvcd.nmcd.hdc, index1, -1, ((nmlvcd.nmcd.uItemState & CDIS_SELECTED) != 0), rect1.ToRectangleF());

                        result = CDRF_NOTIFYSUBITEMDRAW;
                        break;

                    case CDDS_SUBITEM | CDDS_ITEMPREPAINT:
                        var index2 = nmlvcd.nmcd.dwItemSpec;
                        var rect2 = new RECT();
                        rect2.top = nmlvcd.iSubItem;
                        rect2.left = 2; // LVIR_LABEL
                        SendMessage(_listView.Handle, LVM_GETSUBITEMRECT, index2, ref rect2);

                        if (DrawEvent != null)
                            DrawEvent(nmlvcd.nmcd.hdc, index2, nmlvcd.iSubItem, ((nmlvcd.nmcd.uItemState & CDIS_SELECTED) != 0), rect2.ToRectangleF());

                        result = CDRF_SKIPDEFAULT | CDRF_NOTIFYSUBITEMDRAW;
                        break;

                    default:
                        result = CDRF_DODEFAULT;
                        break;
                }
                return (IntPtr)result;
            }
        }

        public static Size GetRequiredSize(this ListView listView, int numItems)
        {
            var res = SendMessage(listView.Handle, LVM_APPROXIMATEVIEWRECT, numItems, -1);
            return new Size(LoWord(res), HiWord(res));
        }

        public static int LoWord(int dwValue)
        {
            return (dwValue & 0xFFFF);
        }

        public static int HiWord(int dwValue)
        {
            return (dwValue >> 16) & 0xFFFF;
        }

        #region Extensions
        static RectangleF ToRectangleF(this RECT rectangle)
        {
            return new RectangleF(rectangle.left, rectangle.top, rectangle.right - rectangle.left, rectangle.bottom - rectangle.top);
        }

        public static void SetCustomHandling(this ListView listView, out WndHandler handler)
        {
            var prev = GetWindowLong(listView.Parent.Handle, GWL_WNDPROC);
            handler = new WndHandler(prev, listView);
        }

        /// <summary>
        /// Sets theme style to a listview
        /// *** This code is from a blog post by Alex Yakhnin.
        /// </summary>
        /// <param name="listView">ListView instance</param>
        public static void SetThemeStyle(this ListView listView)
        {
            // Retreive the current extended style
            int currentStyle = SendMessage(listView.Handle, LVM_GETEXTENDEDLISTVIEWSTYLE, 0, 0);
            // Assign the LVS_EX_THEM style 
            SendMessage(listView.Handle, LVM_SETEXTENDEDLISTVIEWSTYLE, 0, currentStyle | LVS_EX_THEME);

        }

        public static void SetBackgroundImage(this ListView listView, Bitmap bmp)
        {
            var imgStruct = new LVBKIMAGE
                                {
                                    ulFlags = LVBKIF_SOURCE_HBITMAP | LVBKIF_STYLE_TILE,
                                    hbm = bmp.GetHbitmap()
                                };
            SendMessage(listView.Handle, LVM_SETBKIMAGE, 0, ref imgStruct);
        }
        #endregion

        #region P/Invoke related stuff

        delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

#pragma warning disable 169
#pragma warning disable 649
        // ReSharper disable InconsistentNaming
        [DllImport("coredll.dll")]
        private static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        [DllImport("coredll")]
        private static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, ref RECT lParam);

        [DllImport("coredll")]
        private static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, ref LVBKIMAGE lParam);

        [DllImport("coredll.dll", SetLastError = true)]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, WndProcDelegate newProc);

        [DllImport("coredll.dll", SetLastError = true)]
        static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("coredll.dll")]
        static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        struct POINT
        {
            public int x;
            public int y;
        }

        struct NMHDR
        {
            public IntPtr hwndFrom;
            public IntPtr idFrom;
            public int code;
        }

        struct NMLISTVIEW
        {
            public NMHDR hdr;
            public int iItem;
            public int iSubItem;
            public uint uNewState;
            public uint uOldState;
            public uint uChanged;
            public POINT ptAction;
            public int lParam;
        }

        struct NMCUSTOMDRAW
        {
            public NMHDR nmcd;
            public int dwDrawStage;
            public IntPtr hdc;
            public RECT rc;
            public int dwItemSpec;
            public int uItemState;
            public IntPtr lItemlParam;
        }

        struct NMLVCUSTOMDRAW
        {
            public NMCUSTOMDRAW nmcd;
            public int clrText;
            public int clrTextBk;
            public int iSubItem;
            public int dwItemType;
            public int clrFace;
            public int iIconEffect;
            public int iIconPhase;
            public int iPartId;
            public int iStateId;
            public RECT rcText;
            public uint uAlign;
        }

        struct LVBKIMAGE
        {
            public uint ulFlags;
            public IntPtr hbm;
            public IntPtr pszImage;
            public uint cchImageMax;
            public int xOffsetPercent;
            public int yOffsetPercent;
        }

        const int LVS_EX_THEME = 0x02000000;

        const int LVM_GETITEMRECT = 0x1000 + 14;
        const int LVM_SETEXTENDEDLISTVIEWSTYLE = 0x1000 + 54;
        const int LVM_GETEXTENDEDLISTVIEWSTYLE = 0x1000 + 55;
        const int LVM_GETSUBITEMRECT = 0x1000 + 56;
        const int LVM_APPROXIMATEVIEWRECT = 0x1000 + 64;
        const int LVM_SETBKIMAGE = 0x1000 + 138;

        const int GWL_WNDPROC = -4;

        const int WM_NOTIFY = 0x4E;

        const int NM_CUSTOMDRAW = (-12);
        const int NM_CLICK = (-2);

        const int CDRF_DODEFAULT = 0x00000000;
        const int CDRF_SKIPDEFAULT = 0x00000004;
        const int CDRF_NOTIFYPOSTPAINT = 0x00000010;
        const int CDRF_NOTIFYITEMDRAW = 0x00000020;
        const int CDRF_NOTIFYSUBITEMDRAW = CDRF_NOTIFYITEMDRAW;
        const int CDRF_SKIPPOSTPAINT = 0x00000100;

        const int CDDS_PREPAINT = 0x00000001;
        const int CDDS_POSTPAINT = 0x00000002;
        const int CDDS_ITEM = 0x00010000;
        const int CDDS_ITEMPREPAINT = (CDDS_ITEM | CDDS_PREPAINT);
        const int CDDS_SUBITEM = 0x00020000;

        const int CDIS_SELECTED = 0x0001;

        const int LVBKIF_FLAG_TILEOFFSET = 0x00000100;
        const int LVBKIF_SOURCE_HBITMAP = 0x00000001;
        const int LVBKIF_SOURCE_NONE = 0x00000000;
        const int LVBKIF_STYLE_NORMAL = LVBKIF_SOURCE_NONE;
        const int LVBKIF_STYLE_TILE = 0x00000010;
        // ReSharper restore InconsistentNaming
#pragma warning restore 649
#pragma warning restore 169
        #endregion

    }
}
