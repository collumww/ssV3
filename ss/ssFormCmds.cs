using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ss {
    public partial class ssForm : Form {

        //--- Form commands ------------------------------------------

        // Parameterless routines to be called by the ssEventDecoder


        public void CmdExtendSelectionOn() {
            extending = true;
            }


        public void CmdExtendSelectionOff() {
            extending = false;
            }

        public void CmdToggleWrap() {
            xOrg = 0;
            wrap = !wrap;
            if (txt == ed.Log) UpdateDefs();
            ReDisplay();
            }

        public void CmdMakeMarkFromCursor() {
            InvalidateMark();
            mark = cursor.rng;
            txt.mark = mark;
            InvalidateMark();
            }

        public void CmdCursorFromMouseWithEntitySelect() {
            useMouEntity = true;
            canMoveMouse = true;
            switch (mouEntity % 3) {
                case 0:
                    MoveCursor(mouIndex, IndexFromMouse);
                    break;
                case 1:
                    SelectWord();
                    break;
                case 2:
                    SelectParagraph();
                    break;
                }
            }

        public void CmdCursorFromMouse() {
            useMouEntity = false;
            canMoveMouse = true;
            MoveCursor(mouIndex, IndexFromMouse);
            }

        public void CmdCursorToNearestBOWord() {
            mouIndex = IndexToNearestBOWord(mouIndex);
            CmdCursorFromMouse();
            }

        public void CmdCursorToNearestBOWordWithEntitySelect() {
            mouIndex = IndexToNearestBOWord(mouIndex);
            CmdCursorFromMouseWithEntitySelect();
            }

        public void CmdCursorToBOLine() {
            MoveCursor(cursor.l, IndexToBOLine);
            }

        public void CmdCursorToEOLine() {
            MoveCursor(cursor.r, IndexToEOLine);
            }

        public void CmdCursorToBOParagraph() {
            MoveCursor(cursor.l, IndexToBOParagraphLeft);
            }

        public void CmdCursorToEOParagraph() {
            MoveCursor(cursor.r, IndexToEOParagraph);
            }


        public void CmdCursorLeft() {
            MoveCursor(cursor.l, txt.NxtLeft);
            }


        public void CmdCursorRight() {
            MoveCursor(cursor.r, txt.NxtRight);
            }


        public void CmdScrollTextDownOneLine() {
            MoveTextUpDown(TextDown, 1);
            }


        public void CmdScrollTextUpOneLine() {
            MoveTextUpDown(TextUp, 1);
            }

        public void CmdScrollTextDownOnePage() {
            MoveTextUpDown(TextDown, lines.Length - 1);
            }

        public void CmdScrollTextUpOnePage() {
            MoveTextUpDown(TextUp, lines.Length - 1);
            }

        private void AdjCursorForUpAndDown() {
            if (lastX <= layout.leftMargin) return;
            int ln = IndexToLine(lines, cursor.l);
            if (ln > 0) {
                if (lines[ln - 1].rng.r == lines[ln].rng.l 
                    && lines[ln].rng.l == cursor.l) {
                    InvalidateCursor();
                    cursor.To(txt.NxtLeft(cursor.l));
                    }
                }
            }

        public void CmdCursorUp() {
            AdjCursorForUpAndDown();
            MoveCursor(cursor.l, IndexInLineAbove);
            }

        public void CmdCursorDown() {
            AdjCursorForUpAndDown();
            MoveCursor(cursor.r, IndexInLineBelow);
            }

        public void CmdCursorUpToNearestBOWord() {
            AdjCursorForUpAndDown();
            MoveCursor(cursor.l, IndexInLineAbove);
            MoveCursor(cursor.l, IndexToNearestBOWord);
            firstVMove = false;
            }

        public void CmdCursorDownToNearestBOWord() {
            AdjCursorForUpAndDown();
            MoveCursor(cursor.l, IndexInLineBelow);
            MoveCursor(cursor.l, IndexToNearestBOWord);
            firstVMove = false;
            }

        public void CmdCursorToNextWordRight() {
            MoveCursor(cursor.r, IndexToNextBOWRight);
            }

        public void CmdCursorToNextWordLeft() {
            MoveCursor(cursor.l, IndexToNextBOWLeft);
            }

        public void CmdCursorToNextEOWordRight() {
            MoveCursor(cursor.r, IndexToNextEOWRight);
            }

        public void CmdCursorToNextEOWordLeft() {
            MoveCursor(cursor.l, IndexToNextEOWLeft);
            }

        public void CmdCursorToWordRight() {
            MoveCursor(cursor.r, IndexToBOWRight);
            }

        public void CmdCursorToWordLeft() {
            MoveCursor(cursor.l, IndexToBOWLeft);
            }

        public void CmdCursorToEOWordRight() {
            MoveCursor(cursor.r, IndexToEOWRight);
            }

        public void CmdCursorToEOWordLeft() {
            MoveCursor(cursor.l, IndexToEOWLeft);
            }



        public void CmdTabOrIncreaseIndent() {
            if (txt.dot.Empty) {
                if (layout.expTabs) {
                    keyChar = ' ';
                    for (int i = 0; i < layout.spInTab; i++) CmdInsertChar();
                    }
                else {
                    keyChar = '\t';
                    CmdInsertChar();
                    }
                TypingOn();
                }
            else {
                IncIndent();
                }
            }

        public void CmdDecreaseIndent() {
            if (!txt.dot.Empty) DecIndent();
            }


        public void CmdInsertChar() {
            Insert(keyChar);
            cursor.To(cursor.l);
            extending = false;
            MoveCursor(cursor.boat, txt.NxtRight);
            TypingOn();
            }


        public void CmdBack() {
            if (!cursor.Empty) {
                Delete();
                }
            else {
                extending = true;
                MoveCursor(cursor.boat, txt.NxtLeft);
                Delete();
                extending = false;
                }
            }


        public void CmdEnter() {
            Delete();
            int b = cursor.boat;
            Insert(txt.Eoln);
            int oldl = cursor.l;
            extending = false;
            MoveCursor(cursor.boat, IndexCursorRight);
            if (layout.autoIndent) {
                int l = txt.To(txt.AtBOLN, b, -1);
                if (l != oldl && l+1 < txt.Length) {
                    int r = l;
                    while (r < txt.Length
                        && !txt.AtEOLN(r)
                        && char.IsWhiteSpace(txt[r]))
                        r = txt.NxtRight(r);
                    Insert(txt.ToString(l, r - l));
                    MoveCursor(cursor.r, IndexCursorRight);
                    }
                }
            if ((cursor.boat == txt.Length && txt == ed.Log)) {
                int a = txt.To(txt.AtBOLN, b, -1);
                Cursor.Current = Cursors.WaitCursor;
                ed.Do(txt.ToString(a, b - a)); // Exclude the line ending for cases where line ending is set weird.
                Cursor.Current = Cursors.Default;
                //ed.WakeUpText(ed.txt);
                //ed.Log.Activate();
                }
            TypingOn();
            }


        public void CmdDelete() {
            if (!cursor.Empty) Delete();
            else {
                extending = true;
                MoveCursor(cursor.boat, txt.NxtRight);
                Delete();
                extending = false;
                }
            }

        public void CmdCut() {
            Clipboard.SetDataObject(txt.ToString());
            Delete();
            extending = false;
            }

        public void CmdSnarf() {
            Clipboard.SetDataObject(txt.ToString());
            }

        public void CmdPaste() {
            IDataObject d = Clipboard.GetDataObject();
            if (d.GetDataPresent(DataFormats.Text)) {
                Insert((string)d.GetData(DataFormats.Text));
                MoveCursor(cursor.r, IndexCursorRight);
                }
            }

        public void CmdPastePreserveSelection() {
            IDataObject d = Clipboard.GetDataObject();
            if (d.GetDataPresent(DataFormats.Text)) {
                Insert((string)d.GetData(DataFormats.Text));
                }
            }

        public void CmdMoveLinesUp() {
            MoveText(false, SelectLines, IndexToNextBOParagraphLeft);
            }

        public void CmdMoveLinesDown() {
            MoveText(false, SelectLines, IndexToNextBOParagraphRight);
            }

        public void CmdMoveTextWordLeft() {
            MoveText(true, SelectWord, IndexToNextBOWLeft);
            }

        public void CmdMoveTextWordRight() {
            MoveText(true, SelectWord, IndexToNextBOWRight);
            }

        public void CmdMoveTextCharLeft() {
            MoveText(true, SelectWord, txt.NxtLeft);
            }

        public void CmdMoveTextCharRight() {
            MoveText(true, SelectWord, txt.NxtRight);
            }

        public void CmdCopyMarkToCursor() {
            Insert(txt.ToString(mark.l, mark.len));
            }

        public void CmdMoveMarkToCursor() {
            InvalidateMarks();
            ssRange r = cursor.rng;
            cursor.To(mark);
            mark = r;
            txt.dot = cursor.rng;
            string s = txt.ToString();
            Delete();
            r = cursor.rng;
            cursor.To(mark);
            mark = r;
            txt.dot = cursor.rng;
            Insert(s);
            }

        public void CmdCopyCursorToMarkBeginning() {
            InvalidateMarks();
            string s = txt.ToString();
            cursor.To(mark.l);
            txt.dot = cursor.rng;
            Insert(s);
            if (mark.r == cursor.l) {
                mark.r = cursor.r;
                InvalidateMark();
                }
            }

        public void CmdCopyCursorToMarkEnd() {
            InvalidateMarks();
            string s = txt.ToString();
            cursor.To(mark.r);
            txt.dot = cursor.rng;
            Insert(s);
            mark.r = cursor.r;
            InvalidateMark();
            }

        public void CmdCursorToMarkBeginning() {
            MoveCursor(mark.l, IndexMarkLeft);
            }


        public void CmdCursorToMarkEnd() {
            MoveCursor(mark.r, IndexMarkRight);
            }


        public void CmdShowCursor() {
            ShowRange(cursor.rng);
            }

        public void CmdShowMark() {
            ShowRange(mark);
            }

        public void CmdToggleCrossHairs() {
            Graphics g = CreateGraphics();
            IntPtr hdc = g.GetHdc();
            Region r = new Region();

            RangeRegion(hdc, r, cursor.rng, layout.xInvInfl, layout.cursorYInfl);
            g.ReleaseHdc(hdc);
            RectangleF rx = r.GetBounds(g);
            g.Dispose();

            Invalidate(CrossRectVert());
            Invalidate(CrossRectHorz());
            crossOn = !crossOn;
            if (crossOn) {
                crossX = (int)Math.Round(rx.Left);
                crossY = (int)Math.Round(rx.Bottom);
                Invalidate(CrossRectVert());
                Invalidate(CrossRectHorz());
                }
            }


        public void CmdPause() {
            }



        public void CmdNew() {
            ed.NewText();
            }


        public void CmdSave() {
            if (ed.Txt == null) {
                ed.Err("no current file");
                return;
                }
            try {
                if (ed.Txt.Nm == "") {
                    SaveFileDialog d = new SaveFileDialog();
                    d.InitialDirectory = ".";
                    d.Filter = filter;
                    d.RestoreDirectory = true;
                    if (d.ShowDialog() != DialogResult.OK) throw new ssException("no file chosen");
                    else {
                        ed.Txt.Nm = d.FileName;
                        Text = ed.Txt.FileName(); // real name here because or long names from dialog
                        }
                    }
                if (ed.WinWrite(ed.Txt.Nm, ed.Txt.ToString(0, ed.Txt.Length), ed.Txt.encoding)) {
                    ed.MsgLn(ed.Txt.FileName() + ": #" + ed.Txt.Length.ToString());
                    ed.txt.TLog.RecordSave();
                    }
                }
            catch (Exception e) {
                ed.MsgLn(e.Message);
                }
            }


        public void CmdSaveAs() {
            string n = txt.Nm;
            txt.Nm = "";
            CmdSave();
            if (txt.Nm == "") txt.Nm = n;
            }

        private static string filter = "All files (*.*)|*.*|txt files (*.txt)|*.txt|PowerShell files (*.ps1)|*.ps1|SQL files (*.sql)|*.sql|perl files (*.pl)|*.pl";


        public void CmdOpen() {
            OpenFileDialog d = new OpenFileDialog();
            d.Multiselect = true;
            d.InitialDirectory = System.IO.Directory.GetCurrentDirectory();// ".";
            d.Filter = filter;
            d.RestoreDirectory = true;
            if (d.ShowDialog() == DialogResult.OK) ed.AddTexts(d.FileNames, true);
            else ed.Err("no file chosen");
            }


        public void CmdCursorToBOF() {
            MoveCursor(0, IndexBeg);
            }

        public void CmdCursorToEOF() {
            MoveCursor(txt.Length, IndexEnd);
            }

        public void CmdSetFont() {
            FontDialog d = new FontDialog();
            d.Font = layout.font;
            if (d.ShowDialog() == DialogResult.OK) {
                layout.SetFont(d.Font);
                if (txt == ed.Log) UpdateDefs();
                ReDisplayCreateLines();
                }
            }

        public void CmdNextFont() {
            layout.fontNum = (layout.fontNum + 1) % layout.fontCnt;
            if (txt == ed.Log) UpdateDefs();
            ReDisplayCreateLines();
            }

        public void CmdSelectAll() {
            cursor = new ssCursor(0, txt.Length);
            txt.dot = cursor.rng;
            Invalidate();
            }

        public void CmdUndo() {
            ed.Do("u");
            }

        public void CmdRedo() {
            ed.Do("U");
            }

        public void CmdToggleToCommand() {
            if (txt == ed.Log && ed.Txt != null && ed.Txt.Frm != null) ed.Txt.Activate();
            else {
                ed.Log.Activate();
                }
            }

        public void CmdXerox() {
            if (txt == ed.Log) return;
            txt.AddForm(new ssForm(ed, txt));
            txt.Frm.Activate();
            txt.Frm.Show();
            }

        public void CmdToggleAutoIndent() {
            layout.autoIndent = !layout.autoIndent;
            if (txt == ed.Log) UpdateDefs();
            }

        public void CmdToggleProgramming() {
            layout.programming = !layout.programming;
            if (txt == ed.Log) UpdateDefs();
            }

        public void CmdToggleExpTabs() {
            layout.expTabs = !layout.expTabs;
            if (txt == ed.Log) UpdateDefs();
            }

        public void CmdToggleIgnoreCase() {
            ed.defs.senseCase = !ed.defs.senseCase;
            if (txt == ed.Log) UpdateDefs();
            }

        public void CmdShowInternals() {
            if (txt != ed.Log) txt.ShowInternals();
            }

        public void CmdMatchBraces() {
            ssRange r = new ssRange();
            r.l = cursor.boat;
            r.r = IndexToMatchBrace(r.l);
            r.Normalize();
            if (!r.Empty) r.l = txt.NxtRight(r.l);
            InvalidateCursor();
            cursor.To(r);
            txt.dot = cursor.rng;
            InvalidateCursor();
            }

        public void CmdDeleteWord() {
            if (typing) {
                CmdDelete();
                TypingOn();
                }
            else { 
                if (cursor.Empty) {
                    extending = true;
                    CmdCursorToNextWordRight();
                    }
                Delete();
                extending = false;
                }
            }

        public void CmdBackDeleteWord() {
            if (typing) {
                CmdBack();
                TypingOn();
                }
            else {
                if (cursor.Empty) {
                    extending = true;
                    CmdCursorToNextWordLeft();
                    }
                Delete();
                extending = false;
                }
            }

        public void CmdBackDeleteEOWord() {
            if (typing) {
                CmdBack();
                TypingOn();
                }
            else {
                if (cursor.Empty) {
                    extending = true;
                    CmdCursorToNextEOWordLeft();
                    }
                Delete();
                extending = false;
                }
            }

        public void CmdPrevText() {
            ed.PrevText();
            }

        public void CmdNextText() {
            ed.NextText();
            }

        public void CmdLookForward() {
            MenuLookForward(null, null);
            }

        public void CmdLookBackward() {
            MenuLookBackward(null, null);
            }
        }
    }