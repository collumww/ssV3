using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

using System.Runtime.InteropServices;

namespace ss {
    class ssGDI {
        public struct ssRECT {
            public int left;
            public int top;
            public int right;
            public int bottom;
            }

        public struct ssSIZE {
            public short cx;
            public short cy;
            }

        public struct ssDRAWTEXTPARAMS {
            public uint cbSize;
            public int iTabLength;
            public int iLeftMargin;
            public int iRightMargin;
            public int uiLengthDrawn;
            }

        public struct ssGCP_RESULTS {
            public uint lStructSize;
            public string lpOutString;
            public uint[] lpOrder;
            public unsafe int* lpDx;
            public unsafe int* lpCaretPos;
            public string lpClass;
            public short[] lpGlyphs;
            public uint nGlyphs;
            public int nMaxFit;
            }

        public const uint GCP_USEKERNING = 0x0008;
        public const uint GCP_MAXEXTENT = 0x00100000;

        public struct ssKERNINGPAIR {
            public short wFirst;
            public short wSecond;
            public int iKernAmount;
            }

        //These are in winuser.h

        public const uint DT_WORDBREAK = 0x00000010;
        public const uint DT_EXPANDTABS = 0x00000040;
        public const uint DT_TABSTOP = 0x00000080;
        public const uint DT_HIDEPREFIX = 0x00100000;
        public const uint DT_NOPREFIX = 0x00000800;

        public const uint TRANSPARENT = 1;
        public const uint OPAQUE = 2;


        public struct ssTEXTMETRIC {
            public int tmHeight;
            public int tmAscent;
            public int tmDescent;
            public int tmInternalLeading;
            public int tmExternalLeading;
            public int tmAveCharWidth;
            public int tmMaxCharWidth;
            public int tmWeight;
            public int tmOverhang;
            public int tmDigitizedAspectX;
            public int tmDigitizedAspectY;
            public char tmFirstChar;
            public char tmLastChar;
            public char tmDefaultChar;
            public char tmBreakChar;
            public byte tmItalic;
            public byte twUnderLined;
            public byte tmStruckOut;
            public byte tmPitchAndFamily;
            public byte tmCharSet;
            }
        
        public struct ssWNDCLASSEX {
            public uint cbSize;
            public uint style;
            public IntPtr lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            public string lpszMenuName;
            public string lpszClassName;
            public IntPtr hIconSm;
            }

        [DllImport("gdi32.dll")]
        public static extern bool GetTextExtentExPoint(
            IntPtr hdc,
            string lpszStr,
            int cchString,
            int nMaxExtent,
            ref int lpnFit,
            int[] alpDx,
            ref System.Drawing.Size lpSize
            );

        [DllImport("gdi32.dll")]
        public static extern uint GetCharacterPlacement(
            IntPtr hdc,
            string spString,
            int nCount,
            int nMaxExtent,
            ref ssGCP_RESULTS lpResults,
            uint dwFlags
            );

        [DllImport("gdi32.dll")]
        public static extern uint GetKerningPairsA(
            IntPtr hdc,
            uint nPairs,
            ssKERNINGPAIR[] lpKernPair
            );

        [DllImport("user32.dll")]
        public static extern int DrawTextEx(
            IntPtr hdc,
            string lpchText,
            int cchText,
            ref ssRECT lprc,
            uint dwDTFormat,
            ref ssDRAWTEXTPARAMS lpDTParams
            );


        [DllImport("user32.dll")]
        public static extern bool InvalidateRect(
            IntPtr hWnd,
            ref ssRECT lpRect,
            bool bErase);


        [DllImport("user32.dll")]
        public static extern int GetTabbedTextExtent(
            IntPtr hDC,
            string lpString,
            int nCount,
            int nTabPositions,
            int[] lpnTabStopPositions
            );


        [DllImport("user32.dll")]
        public static extern int TabbedTextOut(
            IntPtr HDC,
            int X,
            int Y,
            string lpString,
            int nCount,
            int nTabPositions,
            int[] lpnTabStopPositions,
            int nTabOrigin
            );


        [DllImport("gdi32.dll")]
        public static extern bool GetTextExtentPoint32(
            IntPtr HDC,
            string lpString,
            int c,
            ref System.Drawing.Size lpSize
            );


        [DllImport("gdi32.dll")]
        public static extern bool GetTextMetrics(
            IntPtr HDC,
            ref ssTEXTMETRIC lptm
            );


        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(
            IntPtr hdc,
            IntPtr hgdiobj);


        [DllImport("gdi32.dll")]
        public static extern bool FillRgn(
            IntPtr hdc,
            IntPtr hrgn,
            IntPtr hbrush);


        [DllImport("user32.dll")]
        public static extern int FillRect(
            IntPtr HDC,
            ref ssRECT lprc,
            IntPtr hbr
            );

        [DllImport("gdi32.dll")]
        public static extern int SetBkMode(
            IntPtr HDC,
            uint iBkMode);

		[DllImport("gdi32.dll")]
		public static extern uint SetTextColor(
			IntPtr hdc,
			uint color
			);
        }
    }
