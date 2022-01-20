using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using System.IO;

namespace ss {
    public partial class ssForm : Form {
        public ssForm(ssEd e, ssText t) {
            InitializeComponent();

            txt = t;

            layout = new ssFormLayout();
            linesUsed = 0;
            rngShown = new ssRange();
            xOrg = 0;
            loaded = false;

            cursor = new ssCursor(0, 0);
            mark = new ssRange(0, 0);
            restCur = new ssRange(0, 0);

            sb = new StringBuilder();

            mouX = 0;           // Set by events and then used by the CmdXxx routines that are hooked to the events.
            mouY = 0;
            mouIndex = 0;
            mouTicks = 0;
            mouEntity = 0;
            useMouEntity = true;
            canMoveMouse = false;
            mouMaster = MouseButtons.None;

            crossX = 0;
            crossY = 0;
            crossOn = false;
            wrap = e.defs.wrap;

            keyChar = '\0';
            firstVMove = true;
            lastX = 0;

            extending = false;
            clicks = 0;

            ed = e;
            active = false;

            nxt = null;

            }


        //--- Private data ------------------------------------------


        ssText txt;
        ssDispLine[] lines;
        int linesUsed;
        ssRange rngShown;
        int xOrg;
        bool loaded;

        ssCursor cursor;
        ssRange mark;
        ssRange restCur;

        bool wrap;
        ssFormLayout layout;
        StringBuilder sb;
        ssEventDecoderV2 evd;

        // Things needed to persist across events

        char keyChar;    // the character just assembled

        int mouX;           // Mouse items
        int mouY;
        long mouTicks;
        int mouEntity;      // Indicates char, word, or paragraph in a move
        bool useMouEntity;
        bool canMoveMouse;

        int crossX;             // Crosshair items
        int crossY;
        bool crossOn;

        static long dTicks = 5000000;   // Timing and spacing for double clicks, etc.
        static int dX = 5;
        static int dY = 5;

        int mouIndex;
        bool firstVMove;
        int lastX;

        //bool dragging;
        MouseButtons mouMaster;

        bool extending;     // This just used by key events
        int clicks;

        ssEd ed;
        bool active;        // Only used by command window

        ssForm nxt;


        public void AdjMarks(int loc, int chg, bool insert) {
            if (chg != 0) {
                cursor.Adjust(loc, chg, insert);
                mark.Adjust(loc, chg, insert);
                restCur.Adjust(loc, chg, insert);
                if (loc <= rngShown.l) {
                    int oldorg = rngShown.l;
                    if (!insert) {
                        chg = Math.Min(chg, rngShown.l - loc);
                        }
                    for (int i = 0; i < linesUsed; i++) {
                        lines[i].rng.Adjust(loc, chg, insert);
                        }
                    rngShown.Adjust(loc, chg, insert);
                    if (rngShown.l != oldorg)
                        Align();
                    }
                }
            }


        public void Align() {
            MoveTextUpDown(TextDown, 0);
            }

        public void InvalidateMarksAndChange(int loc) {
            InvalidateChange(loc);
            InvalidateCursor();
            InvalidateMark();
            }

        public void SaveCursor() {
            InvalidateMarks();
            restCur = cursor.rng;
            }

        public void RestoreCursor() {
            cursor.To(restCur);
            FormMarksToText();
            txt.InvalidateMarksAndChange(cursor.l);
            }

        public ssForm Nxt {
            get { return nxt; }
            set { nxt = value; }
            }


        public ssEd Ed {
            get { return ed; }
            }


        public ssEventDecoderV2 Evd {
            get { return evd; }
            }


        void BeginFormTrans() {
            txt.TLog.InitTrans();
            txt.TLog.BeginTrans();
            }

        public void Delete() {
            if (cursor.Empty) return;
            txt.DoMaint();
            InvalidateMarks();
            string s = txt.ToString();
            cursor.To(txt.Delete());
            txt.InvalidateMarksAndChange(cursor.l);
            if (txt != ed.Log) {
                BeginFormTrans();
                txt.TLog.FormLogTrans(ssTrans.Type.insert, txt.dot, s);  // you log what the undo will do, not what was just done.
                }
            }

        public void Insert(char c) {
            string s = c.ToString();
            Insert(c.ToString());
            }

        public void Insert(string s) {
            txt.DoMaint();
            InvalidateMarks();
            if (!txt.dot.Empty) Delete();
            cursor.To(txt.Insert(s));
            txt.InvalidateMarksAndChange(cursor.l);
            if (txt != ed.Log) {
                BeginFormTrans();
                txt.TLog.FormLogTrans(ssTrans.Type.delete, txt.dot, s);  // you log what the undo will do, not what was just done.
                }
            }


        public void IncIndent() {
            string s;
            if (layout.expTabs) s = ("").PadLeft(layout.spInTab); else s = "\t";
            //s = @"-0,.,+0x/.*\N/i/" + s + "/";
            s = @"-0,.y/\N/i/" + s + "/";
            SaveCursor();
            ed.Do(s);
            RestoreCursor();
            }



        public void DecIndent() {
            SaveCursor();
            ed.Do(@"-0,.y/\N/x/^[ \t]/d");
            RestoreCursor();
            }




        public void TextMarksToForm(bool inv) {
            InvalidateMarks();
            cursor.To(txt.dot);
            mark = txt.mark;
            if (inv) {
                ShowRange(cursor.rng);
                InvalidateMarks();
                }
            }

        public void FormMarksToText() {
            txt.dot = cursor.rng;
            txt.mark = mark;
            }





        //--- Private code ------------------------------------------

        public void SelectWord() {
            extending = false; MoveCursor(cursor.l, IndexToBOWLeft);
            extending = true;
            if (layout.programming) {
                MoveCursor(cursor.r, IndexToNextEOWRight);
                }
            else {
                MoveCursor(cursor.r, IndexToNextBOWRight);
                }
            }


        public void SelectParagraph() {
            extending = false; MoveCursor(cursor.l, IndexCursorLeft);
            extending = false; MoveCursor(cursor.l, IndexToBOParagraphLeft);
            extending = true; MoveCursor(cursor.r, IndexToNextBOParagraphRight);
            }


        public void SelectLines() {
            InvalidateMarks();
            ssRange r = cursor.rng;
            cursor.To(txt.AlignRange(ref r));
            txt.dot = cursor.rng;
            InvalidateMarks();
            }


        public void InvalidateMarks() {
            InvalidateCursor();
            InvalidateMark();
            Invalidate(ChangedRect());
            }

        private void InvalidateCursor() {
            InvalidateRange(cursor.rng, layout.xInvInfl, layout.cursorYInfl);
            }


        private void InvalidateMark() {
            InvalidateRange(mark, layout.xInvInfl, layout.markYInfl);
            }


        private void InvalidateRestCur() {
            InvalidateRange(restCur, layout.xInvInfl, layout.markYInfl);
            }


        public void ReDisplay() {
            Graphics g = CreateGraphics();
            IntPtr hdc = g.GetHdc();
            LayoutLines(hdc, txt.To(txt.AtBOLN, rngShown.l, -1), 0, lines, ref linesUsed, ref rngShown);
            g.ReleaseHdc(hdc);
            g.Dispose();
            ShowRange(cursor.rng);
            Invalidate();
            }

        public void ReDisplayCreateLines() {
            Graphics g = CreateGraphics();
            IntPtr hdc = g.GetHdc();
            layout.Init(ed, hdc);
            int org = lines[0].rng.l;
            lines = MakeLines(true);
            LayoutLines(hdc, org, 0, lines, ref linesUsed, ref rngShown);
            g.ReleaseHdc(hdc);
            Invalidate();
            }

        private int IndexToMatchBrace(int i) {
            if (i == txt.Length) return i;
            char goal = '\0';
            CursorMoveDelegate mov = txt.NxtRight;
            char c = txt[i];
            switch (c) {
                case '{': goal = '}'; break;
                case '(': goal = ')'; break;
                case '[': goal = ']'; break;
                case '<': goal = '>'; break;
                case '}': goal = '{'; mov = txt.NxtLeft; break;
                case ')': goal = '('; mov = txt.NxtLeft; break;
                case ']': goal = '['; mov = txt.NxtLeft; break;
                case '>': goal = '<'; mov = txt.NxtLeft; break;
                default:
                    return i;
                }

            int j = i;
            int lev = 1;
            int x;
            for (; ; ) {
                j = mov(j);
                if (mov == txt.NxtRight && j == txt.Length) return i;
                if ((x = txt[j]) == c) lev++;
                if (x == goal) {
                    lev--;
                    if (lev == 0) return j;
                    }
                if (mov == txt.NxtLeft && j == 0) return i;
                }
            }

        private int IndexToNearestBOWord(int i) {
            if (layout.programming) {
                int r = txt.To(txt.AtProgBOW, i, 1);
                int l = txt.To(txt.AtProgBOW, i, -1);
                if ((r - i) <= (i - l)) return r; else return l;
                }
            else {
                int r = txt.To(txt.AtBOW, i, 1);
                int l = txt.To(txt.AtBOW, i, -1);
                if ((r - i) <= (i - l)) return r; else return l;
                }
            }

        private int IndexToNearestParagraph(int i) {
            int r = txt.To(txt.AtBOLN, i, 1);
            int l = txt.To(txt.AtBOLN, i, -1);
            if ((r - i) <= (i - l)) return r; else return l;
            }

        private int IndexToNextBOWRight(int i) {
            if (layout.programming)
                return txt.To(txt.AtProgBOW, txt.NxtRight(i), 1);
            else
                return txt.To(txt.AtBOW, txt.NxtRight(i), 1);
            }

        private int IndexToNextEOWRight(int i) {
            if (layout.programming)
                return txt.To(txt.AtProgEOW, txt.NxtRight(i), 1);
            else
                return txt.To(txt.AtEOW, txt.NxtRight(i), 1);
            }

        private int IndexToNextBOWLeft(int i) {
            if (layout.programming)
                return txt.To(txt.AtProgBOW, txt.NxtLeft(i), -1);
            else
                return txt.To(txt.AtBOW, txt.NxtLeft(i), -1);
            }

        private int IndexToNextEOWLeft(int i) {
            if (layout.programming)
                return txt.To(txt.AtProgEOW, txt.NxtLeft(i), -1);
            else
                return txt.To(txt.AtEOW, txt.NxtLeft(i), -1);
            }

        private int IndexToBOWRight(int i) {
            if (layout.programming)
                return txt.To(txt.AtProgBOW, i, 1);
            else
                return txt.To(txt.AtBOW, i, 1);
            }

        private int IndexToBOWLeft(int i) {
            if (layout.programming)
                return txt.To(txt.AtProgBOW, i, -1);
            else
                return txt.To(txt.AtBOW, i, -1);
            }


        private int IndexToEOWRight(int i) {
            if (layout.programming)
                return txt.To(txt.AtProgEOW, i, 1);
            else
                return txt.To(txt.AtEOW, i, 1);
            }

        private int IndexToEOWLeft(int i) {
            if (layout.programming)
                return txt.To(txt.AtProgEOW, i, -1);
            else
                return txt.To(txt.AtEOW, i, -1);
            }


        private int IndexToBOLine(int i) {
            ssRange r = new ssRange(i, i);
            if (!RangeVisible(r)) ShowRange(r);
            return lines[IndexToLine(lines, i)].rng.l;
            }


        private int IndexToEOLine(int i) {
            ssRange r = new ssRange(i, i);
            if (!RangeVisible(r)) ShowRange(r);
            return lines[IndexToLine(lines, i)].rng.r;
            }


        private int IndexToBOParagraphLeft(int i) {
            return txt.To(txt.AtBOLN, i, -1);
            }


        private int IndexToBOParagraphRight(int i) {
            return txt.To(txt.AtBOLN, i, 1);
            }


        private int IndexToNextBOParagraphLeft(int i) {
            return txt.To(txt.AtBOLN, txt.NxtLeft(i), -1);
            }


        private int IndexToNextBOParagraphRight(int i) {
            return txt.To(txt.AtBOLN, txt.NxtRight(i), 1);
            }


        private int IndexToEOParagraph(int i) {
            return txt.To(txt.AtEOLN, i, 1);
            }


        private int LineIndexToX(IntPtr hdc, int ln, int i) { // Careful: No range checking here on ln.
            int fit = 0;
            int[] dxs = MeasureLine(hdc, lines[ln].txt, ref fit);
            if (i >= dxs.Length) i = dxs.Length - 1;
            if (i < 0) i = 0;
            return (dxs.Length == 0 ? 0 : dxs[i] - layout.aveCharWidth / 2) + layout.leftMargin;
            }


        private int IndexInLine(TextUpDownDelegate mov, int txtmovlim, int curmovlim, int dln, int i) {
            Graphics g = CreateGraphics();
            IntPtr hdc = g.GetHdc();
            int ln = IndexToLine(lines, i);
            if (ln == txtmovlim) {
                if (mov(hdc, 1)) {
                    ln -= dln;
                    Invalidate();
                    }
                }
            if (ln != curmovlim) {
                int x = LineIndexToX(hdc, ln, i - lines[ln].rng.l) - xOrg;
                if (firstVMove) {
                    firstVMove = false;
                    lastX = x;
                    }
                else {
                    x = lastX;
                    }

                i = XToIndex(ln + dln, x);
                }
            g.ReleaseHdc(hdc);
            g.Dispose();
            return i;
            }



        private int IndexInLineAbove(int i) {
            return IndexInLine(TextDown, 0, 0, -1, i);
            }


        private int IndexInLineBelow(int i) {
            return IndexInLine(TextUp, lines.Length - 1, linesUsed - 1, 1, i);
            }


        private int IndexFromMouse(int i) {
            return mouIndex;
            }


        private int IndexBeg(int i) { return 0; }


        private int IndexEnd(int i) { return txt.Length; }

        private int IndexCursorLeft(int i) { return cursor.l; }

        private int IndexCursorRight(int i) { return cursor.r; }

        private int IndexMarkLeft(int i) { return mark.l; }

        private int IndexMarkRight(int i) { return mark.r; }

        private int IndexShrinkLeft(int i) { return txt.NxtRight(cursor.l); }


        private delegate int CursorMoveDelegate(int i);


        private void MoveCursor(int iNonExtend, CursorMoveDelegate mov) {
            if (mov != IndexInLineAbove && mov != IndexInLineBelow) firstVMove = true;
            if (cursor.Empty) InvalidateCursor();
            if (extending) {
                cursor.ExtendTo(mov(cursor.boat));
                InvalidateRange(cursor.extRng, layout.xInvInfl, layout.cursorYInfl);
                }
            else {
                InvalidateCursor();
                if (cursor.Empty) {
                    cursor.To(mov(cursor.boat));
                    }
                else {
                    cursor.To(iNonExtend);
                    }
                InvalidateCursor();
                }
            ShowRange(cursor.rng);
            txt.dot = cursor.rng;
            }



        private delegate bool TextUpDownDelegate(IntPtr hdc, int n);


        private void MoveTextUpDown(TextUpDownDelegate mov, int n) {
            Graphics g = CreateGraphics();
            IntPtr hdc = g.GetHdc();
            if (mov(hdc, n)) Invalidate();
            g.ReleaseHdc(hdc);
            g.Dispose();
            }


        private bool TextDown(IntPtr hdc, int n) {  // Assumption is these are used from a window and only move some amount less than the window size.
            if (rngShown.l > 0) {
                n = Math.Min(n, lines.Length - 1);
                int i = FindLineAbove(lines[0].rng.l, n);
                LayoutLines(hdc, i, 0, lines, ref linesUsed, ref rngShown);
                return true;
                }
            return false;
            }


        private bool TextUp(IntPtr hdc, int n) {
            if (n < linesUsed) { //(rngShown.r < txt.Length) {
                n = Math.Min(n, linesUsed - 1);
                LayoutLines(hdc, lines[n].rng.l, 0, lines, ref linesUsed, ref rngShown);
                return true;
                }
            return false;
            }



        private delegate void TextSelectDelegate();

        private void MoveText(bool checkEmpty, TextSelectDelegate sel, CursorMoveDelegate mov) {
            if (!checkEmpty) sel();
            else if (cursor.Empty) sel();
            extending = false;
            string s = txt.ToString();
            Delete();
            MoveCursor(cursor.l, mov);
            Insert(s);
            }



        private bool RangeVisible(ssRange rng) {
            return rng.Overlaps(rngShown)
                || rng.l == lines[0].rng.l
                || rng.l == rngShown.r && rngShown.r == txt.Length;
            }



        private void ShowRange(ssRange rng) {
            Graphics g = CreateGraphics();
            IntPtr hdc = g.GetHdc();
            int n = 0;
            int org;
            int oldorg = rngShown.l;
            if (!RangeVisible(rng)) {
                if (rng.l >= rngShown.r) n = lines.Length * 3 / 4; // off the end below
                else n = lines.Length / 4;                         // above the top
                org = FindLineAbove(rng.l, n);
                if (org != oldorg) {
                    LayoutLines(hdc, org, 0, lines, ref linesUsed, ref rngShown);
                    Invalidate();
                    }
                }
            if (!wrap) {
                int ln = IndexToLine(lines, rng.l);
                int x = LineIndexToX(hdc, ln, rng.l - lines[ln].rng.l);
                int left = xOrg + layout.leftMargin;
                int right = xOrg + ClientRectangle.Right - layout.rightMargin;
                int inc = (right - left) * 3 / 4;
                org = x / inc * inc;
                if (x < left || right < x) {
                    xOrg = org;
                    Invalidate();
                    }
                }
            g.ReleaseHdc(hdc);
            g.Dispose();
            }



        private ssDispLine[] MakeLines(bool force) {
            int cnt = ClientRectangle.Height / layout.lineht;
            if (force || lines == null || lines.Length != cnt) {
                ssDispLine[] lns = new ssDispLine[cnt];
                for (int i = 0; i < cnt; i++) {
                    lns[i] = new ssDispLine();
                    lns[i].txt = "";
                    }
                return lns;
                }
            else {
                return lines;
                }
            }



        private void RangeRegion(IntPtr hdc, Region rgn, ssRange rng, int inflx, int infly) {
            rgn.MakeEmpty();

            ssRange oldr = rng;
            if (!rng.Clip(rngShown)) return;

            Rectangle r;

            int fln = IndexToLine(lines, rng.l);
            Rectangle fr = ToRectangle(PartialLineRect(hdc, fln, lines[fln], rng.l - lines[fln].rng.l, true));
            if (rng.l == rng.r) {
                fr.Width = layout.cursorWidth;
                fr = Rectangle.Inflate(fr, 0, infly);
                rgn.Union(fr);
                return;
                }
            fr = Rectangle.Inflate(fr, inflx, infly);
            rgn.Union(fr);

            int lln = IndexToLine(lines, rng.r);
            Rectangle lr = ToRectangle(PartialLineRect(hdc, lln, lines[lln], rng.r - lines[lln].rng.l, false));
            int inf = inflx;
            if (rng.l != rng.r) inf = 0;
            if (lines[lln].rng.r < oldr.r) lr.Width = 1000000;
            lr = Rectangle.Inflate(lr, inf, infly);

            if (fln == lln) rgn.Intersect(lr);
            else {
                rgn.Union(lr);
                for (int i = fln + 1; i < lln; i++) {
                    r = ToRectangle(LineRect(i));
                    r = Rectangle.Inflate(r, inflx, infly);
                    rgn.Union(r);
                    }
                }
            }




        private Rectangle ToRectangle(ssGDI.ssRECT ssr) {
            return new Rectangle(ssr.left, ssr.top, ssr.right - ssr.left, ssr.bottom - ssr.top);
            }


        private ssGDI.ssRECT LineRect(int i) {
            ssGDI.ssRECT r = new ssGDI.ssRECT();
            r.top = i * layout.lineht;
            r.bottom = r.top + layout.lineht;
            r.left = layout.leftMargin - xOrg;
            if (wrap) {
                r.right = ClientRectangle.Right - layout.rightMargin - xOrg;
                }
            else r.right = 1000000;  // Really big numbers do strange things with the graphics. A million works.
            return r;
            }



        private int IndexToLine(ssDispLine[] lns, int i) {  // Assuming here that i (the offset) is actually visible.
            int prv = 0, cur = 0;
            do {
                prv = cur;
                cur++;
                }
            while (cur < lns.Length && lns[cur].rng.l != 0 && lns[cur].rng.l <= i);
            return prv;
            }



        private void AdjustDxsForTabs(string s, int[] dxs, int fit) {
            for (int i = 1; i < fit; i++) {   // starting at 1 because 0th is always 0.
                if (s[i - 1] == '\t') {
                    int adj = layout.tabPix - (dxs[i] % layout.tabPix);
                    for (int j = i; j < fit; j++) {
                        dxs[j] += adj;
                        }
                    }
                }
            }

        private int[] MeasureLine(IntPtr hdc, string s, ref int fit) {
            ssGDI.ssRECT r = LineRect(0); // Just need the width, don't care about vertical position here.
            int[] dxs = new int[s.Length];
            IntPtr oldfont = ssGDI.SelectObject(hdc, layout.hfont);
            unsafe {
                if (s.Length == 0) return dxs;
                string ms = s + "E";
                ssGDI.ssGCP_RESULTS gcpres = new ssGDI.ssGCP_RESULTS();
                int[] gcpdx = new int[ms.Length];
                int[] gcpCaretPos = new int[ms.Length];
                fixed (int* lpDx = &gcpdx[0], lpCaretPos = &gcpCaretPos[0]) {
                    gcpres.lStructSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(gcpres);
                    gcpres.lpOutString = null;
                    gcpres.lpOrder = null;
                    gcpres.lpDx = lpDx;
                    gcpres.lpCaretPos = lpCaretPos;
                    gcpres.lpClass = "".PadRight(ms.Length, '\0');
                    gcpres.lpGlyphs = null;
                    gcpres.nGlyphs = (uint)ms.Length;
                    gcpres.nMaxFit = layout.maxDispLineChars;

                    uint res = ssGDI.GetCharacterPlacement(
                        hdc,
                        ms,
                        ms.Length,
                        r.right - r.left,
                        ref gcpres,
                        ssGDI.GCP_MAXEXTENT | ssGDI.GCP_USEKERNING
                        );
                    if (res == 0)
                        throw new ssException("problem measuring line: " + s + ", " + hdc.ToString());
                    }
                AdjustDxsForTabs(ms, gcpCaretPos, ms.Length);
                int i = 0, j = 1, k = 2; // Try to account for ligature since the width
                while (k < ms.Length) {  // of the second glyph is given as 0.
                    if (gcpCaretPos[j] == gcpCaretPos[i]) {
                        gcpCaretPos[j] = (gcpCaretPos[i] + gcpCaretPos[k]) / 2;
                        }
                    i++; j++; k++;
                    }
                fit = gcpres.nMaxFit;
                if (fit < ms.Length) { // Accounting for 0 position given for chars that don't fit.
                    gcpCaretPos[fit] = gcpCaretPos[fit - 1] + gcpdx[fit - 1];
                    }
                else {
                    fit--;
                    }
                i = 0; j = 1;
                while (j < ms.Length) {
                    dxs[i++] = gcpCaretPos[j++];
                    }
                }
            ssGDI.SelectObject(hdc, oldfont);
            return dxs;
            }



        private ssGDI.ssRECT PartialLineRect(IntPtr hdc, int ln, ssDispLine dln, int i, bool right) {
            ssGDI.ssRECT r = LineRect(ln);
            if (dln.txt.Length == 0 && !right)
                r.right = r.left;
            else {
                int fit = 0;
                int[] dxs = MeasureLine(hdc, dln.txt, ref fit);
                if (i > dxs.Length) i = dxs.Length;
                int mid = r.left;
                if (i > 0) {
                    int x = dxs[i - 1];
                    mid = x == 0 ? r.right : r.left + x;
                    }
                if (right) r.left = mid;
                else r.right = mid;
                }
            return r;
            }



        private ssGDI.ssRECT ChangeRect(IntPtr hdc, int ln, ssDispLine oldln, ssDispLine newln) {
            int tstlen = Math.Min(oldln.txt.Length, newln.txt.Length);
            ssDispLine measln;
            if (oldln.txt.Length < newln.txt.Length) measln = newln; else measln = oldln;

            int i = 0;
            while (i < tstlen) {
                if (oldln.txt[i] != newln.txt[i]) {
                    break;
                    }
                i++;
                }

            return PartialLineRect(hdc, ln, measln, i, true);
            }


        private void InvalidateRange(ssRange rng, int inflx, int infly) {
            if (!RangeVisible(rng) || WindowState == FormWindowState.Minimized) return;
            Graphics g = CreateGraphics();
            IntPtr hdc = g.GetHdc();
            Region r = new Region();

            RangeRegion(hdc, r, rng, inflx, infly);
            Invalidate(r);

            g.ReleaseHdc(hdc);
            g.Dispose();
            r.Dispose();
            }




        private void InvalidateChange(int loc) {
            if (WindowState == FormWindowState.Minimized) return;
            if (loc == -1)
                loc = rngShown.l;
            Graphics g = CreateGraphics();
            IntPtr hdc = g.GetHdc();
            ssDispLine[] lns = MakeLines(true);
            Rectangle r;
            int ln = IndexToLine(lines, loc);
            if (wrap && ln > 0) ln--;
            LayoutLines(hdc, lines[ln].rng.l, ln, lns, ref linesUsed, ref rngShown);
            rngShown.l = lines[0].rng.l; // since lns is empty layoutlines creams rngShown. Need to put back.
            for (int i = ln; i < lines.Length; i++) {
                r = ToRectangle(ChangeRect(hdc, i, lines[i], lns[i]));
                if (r.Width > 0) {
                    r.Inflate(6, 0);
                    Invalidate(r);
                    }
                lines[i] = lns[i];
                }
            g.ReleaseHdc(hdc);
            g.Dispose();
            }


        private string DisplayString(string s) {
            sb.Clear();
            int len = s.Length;
            if (txt.fixedLn == 0 && ssString.AtBOLN(s, len, txt.Eoln))
                len -= txt.Eoln.Length;
            for (int i = 0; i < len; i++) {
                char c = s[i];
                if (!Char.IsControl(c))
                    sb.Append(c);
                else
                    if (txt.fixedLn != 0)
                    sb.Append('.');
                else
                        if (c == '\t')
                    sb.Append(c);
                else
                    sb.Append('.');
                }
            return sb.ToString();
            }


        private int FindLineBreak(IntPtr hdc, string s) {
            int mlen = Math.Min(s.Length, layout.maxDispLineChars);
            if (!wrap || s.Length == 0) return mlen;
            int fit = 0, i;

            int[] dxs = MeasureLine(hdc, s, ref fit);

            int lim = ClientRectangle.Width - layout.leftMargin - layout.rightMargin;
            int spbrk = 0;
            int lnbrk = 0;
            i = fit;
            while (i > 0) {
                i--;
                if (dxs[i] < lim) {
                    if (char.IsWhiteSpace(s[i])) {
                        if (spbrk == 0) spbrk = i;
                        }
                    else {
                        if (lnbrk == 0) lnbrk = i;
                        }
                    }
                }
            lnbrk++;
            if (lnbrk != mlen && spbrk != 0) return spbrk + 1;
            else return lnbrk;
            }


        private void AdjustLengthDrawn(IntPtr hdc, string s, ref int len) {
            len = Math.Min(len, layout.maxDispLineChars);
            }


        private int FindLineAbove(int strt, int n) {
            if (strt <= 0) return 0;
            Graphics g = CreateGraphics();
            IntPtr hdc = g.GetHdc();
            IntPtr oldfnt = ssGDI.SelectObject(hdc, layout.hfont);

            ssDispLine dls = null;
            ssDispLine dl = null;

            int left = txt.To(txt.AtBOLN, strt, -1);
            int right = txt.To(txt.AtBOLN, txt.NxtRight(left), 1);
            int drlen;

            for (; ; ) {
                string logln = txt.ToString(left, right - left);
                int st = 0;
                dls = null;
                do {
                    string s = logln.Substring(st);
                    drlen = FindLineBreak(hdc, s);
                    dl = new ssDispLine();
                    dl.nxt = dls;
                    dls = dl;

                    dls.rng.l = left + st;
                    dls.rng.len = drlen;
                    dls.txt = txt.ToString(dls.rng.l, dls.rng.len);
                    st += drlen;
                    }
                while (drlen != 0 && !dls.rng.Contains(strt));
                if (n == 0) break;
                dl = dls;
                while (dl.nxt != null && n > 0) {
                    dl = dl.nxt;
                    n--;
                    }
                if ((left > 0 && n == 0) || dl.rng.l == 0) break;
                strt = txt.NxtLeft(left);
                right = left;
                left = txt.To(txt.AtBOLN, txt.NxtLeft(left), -1);
                n--;
                }
            ssGDI.SelectObject(hdc, oldfnt);
            g.ReleaseHdc(hdc);
            g.Dispose();
            return dl.rng.l;
            }




        private void LayoutLines(IntPtr hdc, int org, int lnorg, ssDispLine[] lns, ref int used, ref ssRange shown) {
            if (layout.lineht > ClientRectangle.Height) {
                SetClientSizeCore(ClientRectangle.Width, layout.lineht * 3 / 2);
                lns = MakeLines(true);
                }
            int ln = lnorg;
            if (org == -1) //org = shown.l; // org = lines[ln].rng.l;
                org = txt.To(txt.AtBOLN, shown.l, -1);
            if (org >= txt.Length)
                org = txt.To(txt.AtBOLN, txt.Length, -1);

            int lnstrt = org;
            ssRange rng = new ssRange();

            IntPtr oldfnt = ssGDI.SelectObject(hdc, layout.hfont);
            ssGDI.ssRECT rtry = LineRect(0);
            rtry.bottom = rtry.top;

            while (ln < lns.Length && lnstrt < txt.Length) {

                int lnlen = txt.To(txt.AtBOLN, txt.NxtRight(lnstrt), 1) - lnstrt;

                string logln = DisplayString(txt.ToString(lnstrt, lnlen));

                int st = 0;
                do {
                    string s = logln.Substring(st);
                    int drlen = FindLineBreak(hdc, s);
                    rng.l = lnstrt + st;
                    rng.len = drlen;
                    lns[ln].rng = rng;
                    lns[ln].txt = logln.Substring(st, drlen);

                    st += drlen;
                    ln++;
                    }
                while (ln < lns.Length && st < logln.Length);

                lnstrt += lnlen;
                }

            if (ln < lns.Length && lnstrt >= txt.Length && txt.AtBOLN(lnstrt)) { // This empty line at the end allows text to be appended to the end of the display.
                rng.l = rng.r = txt.Length;
                lns[ln].rng = rng;
                lns[ln].txt = "";
                ln++;
                }

            for (int i = ln; i < lns.Length; i++) { // Blank the rest of the display.
                lns[i].rng.l = 0;
                lns[i].rng.r = 0;
                lns[i].txt = "";
                }

            ssGDI.SelectObject(hdc, oldfnt);

            used = ln;
            shown.l = lns[0].rng.l;
            shown.r = rng.r;
            }


        private uint ColorToRGB(Color c) {
            uint x = (uint)c.ToArgb();
            uint r = (x & 0x00ff0000) >> 16;
            uint g = (x & 0x0000ff00);
            uint b = (x & 0x000000ff) << 16;
            return r | g | b;
            }


        private void DrawLines(IntPtr hdc, Rectangle clipR) {
            uint[] cs = new uint[3];
            cs[0] = ColorToRGB(Color.Red);
            cs[1] = ColorToRGB(Color.Green);
            cs[2] = ColorToRGB(Color.Blue);

            ssGDI.SetBkMode(hdc, ssGDI.TRANSPARENT);
            for (int ln = 0; ln < lines.Length && lines[ln].txt != null; ln++) {
                ssGDI.ssRECT r = LineRect(ln);
                string s = lines[ln].txt;
                if (clipR.IntersectsWith(new Rectangle(
                    r.left, r.top, (r.right - r.left), (r.bottom - r.top)))) {
                    if (true) {
                        ssGDI.DrawTextEx(hdc,
                            s,
                            s.Length,
                            ref r,
                            layout.opts,
                            ref layout.dtp);
                        }
                    else {
                        int fit = 0;
                        int[] dxs = MeasureLine(hdc, s, ref fit);
                        int i = 0, j = 0, x = 0, lx = 0;
                        int cnt = 0;
                        while (i < s.Length) {
                            i = j;
                            lx = x;
                            if (j < s.Length
                                && !char.IsLetterOrDigit(s[j])
                                && !char.IsWhiteSpace(s[j])) {
                                x = dxs[j];
                                j++;
                                }
                            else {
                                while (j < s.Length && char.IsLetterOrDigit(s[j])) {
                                    x = dxs[j];
                                    j++;
                                    }
                                }
                            while (j < s.Length && char.IsWhiteSpace(s[j])) {
                                x = dxs[j];
                                j++;
                                }
                            r.left = layout.leftMargin + lx;

                            uint col = cs[cnt % cs.Length] & 0x00ffffff;
                            uint oldcol = ssGDI.SetTextColor(hdc, col);
                            ssGDI.DrawTextEx(hdc,
                                s.Substring(i, j - i),
                                j - i,
                                ref r,
                                layout.opts,
                                ref layout.dtp
                                );
                            ssGDI.SetTextColor(hdc, oldcol);
                            cnt++;
                            }

                        }
                    }


                //string txt = "the quick brown fox jumps over the lazy dog";

                //if (ln % 2 == 0) {
                //	ssGDI.DrawTextEx(hdc,
                //		txt,
                //		txt.Length,
                //		ref r,
                //		layout.opts,
                //		ref layout.dtp);
                //}
                //else {
                //	int a = 0, aa = layout.leftMargin;
                //	for (int i = 0; i < dxs.Length; i++) {
                //		if (char.IsWhiteSpace(txt[i])) {
                //			int b = i;
                //			r.left = aa;
                //			r.right = layout.leftMargin + dxs[b];
                //			ssGDI.DrawTextEx(hdc,
                //				txt.Substring(a, b - a),
                //				b - a,
                //				ref r,
                //				layout.opts,
                //				ref layout.dtp
                //				);
                //			aa = r.right;
                //			a = b + 1;
                //		}
                //	}
                //}


                }
            }



        private int YToLine(int y) {
            if (linesUsed == 0) return 0;
            if (y < 0) y = 0;
            if (y > ClientRectangle.Height) y = ClientRectangle.Height;
            int ln = y / layout.lineht;
            return ln >= linesUsed ? linesUsed - 1 : ln;
            }


        private int XToIndex(int ln, int x) {
            Graphics g = CreateGraphics();
            IntPtr hdc = g.GetHdc();
            int fit = 0;
            int[] dxs = MeasureLine(hdc, lines[ln].txt, ref fit);
            ssGDI.ssRECT r = LineRect(ln);

            if (x < 0) x = 0;
            if (x > ClientRectangle.Right) x = ClientRectangle.Right;
            x -= r.left;
            int i = 0;
            while (i < dxs.Length && x > dxs[i]) {
                i++;
                }

            int j = lines[ln].rng.l + i;

            g.ReleaseHdc(hdc);
            g.Dispose();
            return j;
            }


        private int PtToIndex(int x, int y) {
            return XToIndex(YToLine(y), x);  //todo guard this with a try catch. saw a crazy bug i've never seen in months
            }


        private void SetFormLocSiz() {
            Rectangle scr = Screen.GetWorkingArea(ed.Log.Frm.Location);
            Size lsz = ed.Log.Frm.DesktopBounds.Size;
            Point lloc = ed.Log.Frm.Location;
            int above = lloc.Y - scr.Y;
            int below = (scr.Y + scr.Height) - (lloc.Y + lsz.Height);
            int left = lloc.X - scr.X;
            int right = (scr.X + scr.Width) - (lloc.X + lsz.Width);
            if (lsz.Width >= lsz.Height) {
                Width = lsz.Width;
                if (above > below) {
                    Height = above - below;
                    lloc.Y = below;
                    }
                else {
                    Height = below - above;
                    lloc.Y += lsz.Height;
                    }
                }
            else {
                Height = scr.Height - 2 * above;
                if (right > left) {
                    Width = lsz.Width * 4 / 2;
                    lloc.X += lsz.Width;
                    }
                else {
                    Width = lsz.Width * 4 / 2;
                    lloc.X -= Width;
                    }
                }
            Location = lloc;
            Height = Math.Max(Height, lsz.Height);
            Width = Math.Max(Width, lsz.Width);
            }


        private Keys MouseButtonToKey(MouseButtons b) {
            Keys k = Keys.None;
            switch (b) {
                case MouseButtons.Left:
                    k = Keys.LButton;
                    break;
                case MouseButtons.Right:
                    k = Keys.RButton;
                    break;
                case MouseButtons.Middle:
                    k = Keys.MButton;
                    break;
                case MouseButtons.XButton1:
                    k = Keys.XButton1;
                    break;
                case MouseButtons.XButton2:
                    k = Keys.XButton2;
                    break;
                }
            return k;
            }


        int PropOfHeight(long whole, long y) {
            int marg = MinimumSize.Height / 10;
            y = Math.Max(0, y - marg);
            int h = ClientRectangle.Height - 2 * marg;
            y = Math.Min(y, h);
            return (int)(whole * y / h);
            }


        void ScrollV(MouseButtons b, int y) {
            int dy = y / layout.lineht;
            switch (b) {
                case MouseButtons.Left:
                    MoveTextUpDown(TextUp, dy);
                    break;
                case MouseButtons.Right:
                    MoveTextUpDown(TextDown, dy);
                    break;
                case MouseButtons.Middle:
                    int i = txt.To(txt.AtBOLN, PropOfHeight(txt.Length, y), -1);
                    ShowRange(new ssRange(i, i));
                    Invalidate();
                    break;
                }
            }



        void ScrollH(MouseButtons b, int y) {
            int prop = PropOfHeight(ClientRectangle.Width, y);
            switch (b) {
                case MouseButtons.Left:
                    xOrg = Math.Max(0, xOrg - prop);
                    Invalidate();
                    break;
                case MouseButtons.Right:
                    xOrg += prop;
                    Invalidate();
                    break;
                case MouseButtons.Middle:
                    break;
                }
            }



        //---- Event handlers ---------------------------------------------------


        private void ssForm_Load(object sender, EventArgs e) {
            if (txt != ed.Log)
                SetFormLocSiz(); // Under Windows 10, this generates a resize event. That is handled using the loaded flag, along with logic to detect resize events on window minimization.


            Graphics g = CreateGraphics();
            IntPtr hdc = g.GetHdc();
            layout.Init(ed, hdc);
            if (txt == ed.Log) {
                Top = layout.top;
                Left = layout.left;
                Width = layout.width;
                Height = layout.height;
                bool canSee = false;
                foreach (Screen s in Screen.AllScreens) {
                    canSee |= DesktopBounds.IntersectsWith(s.Bounds);
                    }
                if (!canSee) {
                    DesktopBounds = new Rectangle(ssDefaults.defleft, ssDefaults.deftop, ssDefaults.defwidth, ssDefaults.defheight);
                    }
                }

            lines = MakeLines(true);
            LayoutLines(hdc, 0, 0, lines, ref linesUsed, ref rngShown);
            g.ReleaseHdc(hdc);
            g.Dispose();
            BackColor = Color.BlanchedAlmond;

            evd = new ssEventDecoderV2(this); // This can generate exceptions and so has to happen after form is up and running.

            if (txt == ed.Log) {
                BackColor = Color.PaleGoldenrod;
                Text = "ssCmd — " + Directory.GetCurrentDirectory();
                ed.Msg("");
                ed.ProcessArgs(); // Just like above, this needs to happen after form is up and running.
                }
            loaded = true;
            }


        private Rectangle HScrollRect() {
            Rectangle rct = ClientRectangle;
            rct.X = ClientRectangle.Width - layout.scrollMargin;  // Right scroll bar
            rct.Width = layout.scrollMargin;
            return rct;
            }


        private Rectangle CrossRectVert() {
            Rectangle r = ClientRectangle;
            r.X = crossX;
            r.Width = 2;
            return r;
            }


        private Rectangle CrossRectHorz() {
            Rectangle r = ClientRectangle;
            r.Y = crossY;
            r.Height = 2;
            return r;
            }


        private Rectangle ChangedRect() {
            const int width = 2;
            Rectangle r = ClientRectangle;
            r.X = r.Width - width;
            r.Width = width;
            return r;
            }

        private void ssForm_Paint(object sender, PaintEventArgs e) {
            if (WindowState == FormWindowState.Minimized) return;
            Region r = new Region();
            r.MakeEmpty();

            Rectangle rct = ClientRectangle;

            rct.Width = layout.leftMargin;
            e.Graphics.FillRectangle(Brushes.Ivory, rct);

            rct.Width = layout.scrollMargin;                            // Left scroll bar
            e.Graphics.FillRectangle(Brushes.MistyRose, rct);

            Brush sb = Brushes.White;
            if (txt == ed.Txt && txt.Frm == this) sb = Brushes.LightSkyBlue;
            if (txt == ed.Log && active) sb = Brushes.LightPink;
            e.Graphics.FillRectangle(sb, HScrollRect());                // Right scroll bar

            if (wrap) {
                rct = ClientRectangle;
                rct.X = ClientRectangle.Right - layout.scrollMargin;
                rct.Width = 2;
                e.Graphics.FillRectangle(Brushes.Chocolate, rct);
                }

            if (txt.TLog.Changed) {
                e.Graphics.FillRectangle(Brushes.Red, ChangedRect());
                }

            rct = ClientRectangle;
            rct.Width = layout.scrollMargin;
            if (txt.Length > 0) {
                long ht = rct.Height;
                long sl = rngShown.l;
                rct.Y = (int)(ht * sl / txt.Length);                   // Left thumb
                rct.Height = rct.Height * rngShown.len / txt.Length;
                if (rct.Height < 4) { rct.Height = 4; }
                }
            e.Graphics.FillRectangle(Brushes.LightSalmon, rct);

            IntPtr hdc = e.Graphics.GetHdc();                           // The mark
            RangeRegion(hdc, r, mark, layout.xDrwInfl, layout.markYInfl);
            e.Graphics.ReleaseHdc(hdc);
            e.Graphics.FillRegion(Brushes.MediumAquamarine, r);

            if (crossOn) {                                              // Crosshairs
                Brush crb = Brushes.BurlyWood;
                e.Graphics.FillRectangle(crb, CrossRectVert());
                e.Graphics.FillRectangle(crb, CrossRectHorz());
                }

            hdc = e.Graphics.GetHdc();                                  // The cursor
            if (layout.needsInit) layout.Init(ed, hdc);
            RangeRegion(hdc, r, cursor.rng, layout.xDrwInfl, layout.cursorYInfl);  // inflating x creates cursor when length of selection is 0. Same for mark.
            e.Graphics.ReleaseHdc(hdc);
            Brush cb = Brushes.Black;
            if (cursor.l != cursor.r) cb = Brushes.Orange;
            else if (cursor.l == txt.Length) cb = Brushes.Red;
            e.Graphics.FillRegion(cb, r);

            hdc = e.Graphics.GetHdc();
            IntPtr oldfnt = ssGDI.SelectObject(hdc, layout.hfont);
            DrawLines(hdc, e.ClipRectangle);
            ssGDI.SelectObject(hdc, oldfnt);
            e.Graphics.ReleaseHdc(hdc);
            r.Dispose();
            }


        // Keyboard and mouse event handlers


        private void ssForm_KeyPress(object sender, KeyPressEventArgs e) {
            if (!evd.ShouldIgnoreKeyPress() && e.KeyChar >= 32) {  // Don't process control char presses, 
                keyChar = e.KeyChar;                               // Use keydown and keyup for that
                evd.Eat(Keys.None, e.KeyChar, ssEventType.press);
                }
            }


        private void ssForm_KeyDown(object sender, KeyEventArgs e) {
            evd.SetKeyModifier(e.KeyCode, true);
            evd.Eat(e.KeyCode, '\0', ssEventType.down);
            }


        private void ssForm_KeyUp(object sender, KeyEventArgs e) {
            if (evd.IsModifier(e.KeyCode)) {
                evd.Eat(e.KeyCode, '\0', ssEventType.up);
                }
            evd.SetKeyModifier(e.KeyCode, false);
            }



        private void ssForm_MouseDown(object sender, MouseEventArgs e) {
            if (useMouEntity) {
                long t = DateTime.Now.Ticks;
                if (Math.Abs(mouX - e.X) < dX
                    && Math.Abs(mouY - e.Y) < dY
                    && t - mouTicks < dTicks) {
                    mouEntity++;
                    }
                else mouEntity = 0;
                mouTicks = t;
                }
            mouX = e.X;
            mouY = e.Y;

            if (mouX < layout.scrollMargin)
                ScrollV(e.Button, mouY);
            else if (mouX > ClientRectangle.Width - layout.scrollMargin) {
                ScrollH(e.Button, mouY);
                }
            else {
                evd.SetMouModifier(e.Button, true);
                mouIndex = PtToIndex(mouX, mouY);
                mouMaster = e.Button;
                evd.Eat(MouseButtonToKey(e.Button), '\0', ssEventType.down);
                }
            }

        private void ssForm_MouseMove(object sender, MouseEventArgs e) {
            //if (!evd.Enabled() || clicks == 0) return;
            mouX = e.X;
            mouY = e.Y;
            if (mouMaster != MouseButtons.None && e.Button == mouMaster) {
                mouIndex = PtToIndex(mouX, mouY);
                extending = true;
                if (useMouEntity) {
                    switch (mouEntity % 3) {
                        case 0:
                            break;
                        case 1:
                            mouIndex = IndexToNearestBOWord(mouIndex);
                            break;
                        case 2:
                            mouIndex = IndexToNearestParagraph(mouIndex);
                            break;
                        }
                    }
                if (canMoveMouse && mouIndex != cursor.boat) {
                    MoveCursor(cursor.boat, IndexFromMouse);
                    }
                }
            }

        private void ssForm_MouseUp(object sender, MouseEventArgs e) {
            if (evd.Enabled()) {
                mouX = e.X;
                mouY = e.Y;
                if (e.Button == mouMaster) {
                    mouIndex = PtToIndex(mouX, mouY);
                    mouMaster = MouseButtons.None;
                    }
                }
            evd.Eat(MouseButtonToKey(e.Button), '\0', ssEventType.up);
            evd.SetMouModifier(e.Button, false);
            extending = false;
            canMoveMouse = false;
            clicks++;
            }


        private void ssForm_MouseWheel(object sender, MouseEventArgs e) {
            if (e.Delta == 0) return;
            int dy = Math.Max(1, (e.Y - ClientRectangle.Y) / layout.lineht / 2);
            if (e.Delta < 0) MoveTextUpDown(TextUp, dy);
            else MoveTextUpDown(TextDown, dy);
            }


        public void UpdateDefs() {
            ed.defs.fontNum = layout.fontNum;
            for (int i = 0; i < layout.fontCnt; i++) {
                ed.defs.fontNm[i] = layout.fontNm[i];
                ed.defs.fontStyle[i] = layout.fontStyle[i];
                ed.defs.fontSz[i] = layout.fontSz[i];
                }
            ed.defs.wrap = wrap;
            ed.defs.autoIndent = layout.autoIndent;
            ed.defs.programming = layout.programming;
            ed.defs.spInTab = layout.spInTab;
            ed.defs.expTabs = layout.expTabs;
            ed.defs.eventSet = layout.eventset;
            if (WindowState == FormWindowState.Normal) {
                ed.defs.top = Top;
                ed.defs.left = Left;
                ed.defs.width = Width;
                ed.defs.height = Height;
                }
            }



        private void ssForm_FormClosing(object sender, FormClosingEventArgs e) {
            if (txt == ed.Log) {
                e.Cancel = !ed.DeleteAllTexts();
                UpdateDefs();
                ed.defs.SaveDefs(false);
                }
            else {
                if (txt.LastForm(this)) {
                    e.Cancel = !ed.DeleteText(txt, false);
                    }
                else txt.DeleteForm(this);
                }
            }

        private void ssForm_Resize(object sender, EventArgs e) {
            if (!loaded || ClientRectangle.Width == 0 || ClientRectangle.Height == 0) return; // Needed for Windows 10
            Graphics g = CreateGraphics();
            IntPtr hdc = g.GetHdc();
            lines = MakeLines(true);
            LayoutLines(hdc, rngShown.l, 0, lines, ref linesUsed, ref rngShown);
            Invalidate();
            g.ReleaseHdc(hdc);
            g.Dispose();
            }

        private void ssForm_Activated(object sender, EventArgs e) {
            txt.Frm = this;
            if (txt != ed.Log) {
                ed.txt = txt;
                FormMarksToText();
                for (ssText t = ed.Txts; t != null; t = t.Nxt)
                    for (ssForm f = t.Frms; f != null; f = f.Nxt)
                        f.Invalidate(f.HScrollRect());
                }
            else {
                active = true;
                Invalidate(HScrollRect());
                }
            evd.Reset();
            }

        private void ssForm_Deactivate(object sender, EventArgs e) {
            Invalidate(HScrollRect());
            if (txt == ed.Log) active = false;
            evd.Reset();
            clicks = 0;
            }

        private void ssForm_DragEnter(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
            }

        private void ssForm_DragDrop(object sender, DragEventArgs e) {
            try {
                ed.AddTexts((string[])e.Data.GetData(DataFormats.FileDrop), true);
                }
            catch (Exception ex) {
                ed.Err("arg: " + ex.Message);
                }
            }

        public void ChangeTab(int i) {
            layout.spInTab = i;
            layout.tabPix = layout.spInTab * layout.aveCharWidth;
            layout.dtp.iTabLength = i;
            ReDisplay();
            }

        public void ChooseFont(int i) {
            layout.fontNum = i;
            if (txt == ed.Log) UpdateDefs();
            ReDisplayCreateLines();
            }

        }
    }
