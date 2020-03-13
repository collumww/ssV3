using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using System.Reflection;

namespace ss {
    class ssFormLayout {
        public ssFormLayout() {
            fontCnt = ssDefaults.fontCnt;
            fontNm = new string[fontCnt];
            fontStyle = new FontStyle[fontCnt];
            fontSz = new float[fontCnt];
            }

        public void NextFont() {
            font.Dispose();
            hfont = (IntPtr) 0;
            fontNum = (fontNum + 1) % fontCnt;
            }

        public void SetFont(Font f) {
            font.Dispose();
            hfont = (IntPtr) 0;
            font = f;
            fontNm[fontNum] = font.Name;
            fontStyle[fontNum] = font.Style;
            fontSz[fontNum] = font.Size;
            }

        public void Init(ssEd ed, IntPtr hdc) {
            if (needsInit) {
                fontNum = ed.defs.fontNum;
                for (int i = 0; i < fontCnt; i++) {
                    fontNm[i] = ed.defs.fontNm[i];
                    fontStyle[i] = ed.defs.fontStyle[i];
                    fontSz[i] = ed.defs.fontSz[i];
                    }

                opts = ssGDI.DT_TABSTOP | ssGDI.DT_EXPANDTABS | ssGDI.DT_NOPREFIX;

                autoIndent = ed.defs.autoIndent;
                programming = ed.defs.programming;
                spInTab = ed.defs.spInTab;
                expTabs = ed.defs.expTabs;
                top = ed.defs.top;
                left = ed.defs.left;
                width = ed.defs.width;
                height = ed.defs.height;
                eventset = ed.defs.eventSet;
                }

            font = new Font(fontNm[fontNum], fontSz[fontNum], fontStyle[fontNum]);
            hfont = font.ToHfont();

            tm = new ssGDI.ssTEXTMETRIC();

            IntPtr oldfnt = ssGDI.SelectObject(hdc, hfont);
            if (!ssGDI.GetTextMetrics(hdc, ref tm))
                throw new ssException("error getting text metrics");
            //kps = new ssGDI.ssKERNINGPAIR[1000];
            //n = ssGDI.GetKerningPairsA(hdc, 0, null);
            ssGDI.SelectObject(hdc, oldfnt);

            aveCharWidth = tm.tmAveCharWidth;
            tabPix = aveCharWidth * spInTab;
            lineht = tm.tmHeight;

            dtp = new ssGDI.ssDRAWTEXTPARAMS();
            dtp.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(dtp);
            dtp.iLeftMargin = 0;
            dtp.iRightMargin = 0;
            dtp.iTabLength = spInTab;

            needsInit = false;
            }

        public bool needsInit = true;

        public int fontCnt;
        public int fontNum;

        public string[] fontNm;
        public FontStyle[] fontStyle;
        public float[] fontSz;

        public bool autoIndent;
        public int spInTab;
        public bool expTabs;
        public bool programming;

        public int top;
        public int left;
        public int width;
        public int height;

        public Font font;
        public IntPtr hfont;

        public uint opts;

        public int leftMargin = 25;
        public int rightMargin = 25;

        public int scrollMargin = 15;

        public int aveCharWidth;
        public int tabPix;
        public int xInvInfl = 0;
        public int xDrwInfl = 0;
        public int markYInfl = 2;
        public int cursorYInfl = 0;
        public int cursorWidth = 2;

        public int lineht;

        public ssGDI.ssDRAWTEXTPARAMS dtp;
        public ssGDI.ssTEXTMETRIC tm;
        //public ssGDI.ssKERNINGPAIR[] kps;

        public int maxDispLineChars = 1024;

        public string eventset;


        //---- Private Stuff --------------------------------------------

        }
    }
