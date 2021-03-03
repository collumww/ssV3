using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

namespace ss {

    public class ssText {
        public const string crlf = "\r\n";

        public bool IsPunctuation(char c) {
            return char.IsPunctuation(c) ||
            ed.defs.progPunct.IndexOf(c) != -1;
            }


        public ssText(ssEd ee, string s, string eoln, string n, Encoding enc) {
            nxt = null;
            txt = new ssRawTextV2(s);
            changed = false;
            firstTry = true;
            cmdaffected = false;
            dot = new ssRange();
            mark = new ssRange();
            EOLN = eoln;
            fixedLn = 0;
            if (EOLN == null) EOLN = crlf;
            nm = n;
            ed = ee;
            encoding = enc;
            seqRoot = new ssTrans(ssTrans.Type.delete, 0, dot, null, null);
            tlog = new ssTransLog(ed, this);
            InitSeq();

            //win Remove for non-windowing version
            frms = null;
            frm = null;
            // Remove for non-windowing version */
            }

        public ssRange dot;
        public ssRange mark;

        public Encoding encoding;

        //--- Private below ------------------------------------------

        string nm;

        ssText nxt;
        ssRawTextV2 txt;
        bool changed;
        bool firstTry;
        public bool cmdaffected;  // the editor uses this to know which windows to update after a command

        int adjEdge;
        public ssTrans seqRoot;
        ssTransLog tlog;

        string EOLN;
        public int fixedLn;


        ssEd ed;

        //win Remove for non-windowed version
        ssForm frm;
        ssForm frms;
        // Remove for non-windowed version */


        public ssTransLog TLog {
            get { return tlog; }
            }

        public void Commit() {
            ssTrans t = seqRoot.nxt;
            while (t != null) {
                if (t.typ != ssTrans.Type.rename) CheckSeq(ref t.rng, t.s != null);
                dot = t.rng;
                switch (t.typ) {
                    case ssTrans.Type.rename:
                        string n = Nm;
                        Rename(t.s);
                        t.s = n;
                        break;
                    case ssTrans.Type.delete:
                        t.s = ToString();
                        t.rng = Delete();
                        t.typ = ssTrans.Type.insert;
                        break;
                    case ssTrans.Type.insert:
                        t.rng = Insert(t.s);
                        t.s = null;
                        t.typ = ssTrans.Type.delete;
                        break;
                    }

                ssTrans tt = t.nxt; // Grab t.nxt before LogTrans changes it.
                if (tt == null) {
                    if (t.typ != ssTrans.Type.rename) dot = t.rng;
                    SyncFormToText();
                    }
                tlog.LogTrans(t);  // Form keeps from logging ed.log transactions. We don't check it here.
                t = tt;
                }
            }

        public void PushTrans(ssTrans t) {
            t.nxt = seqRoot.nxt;
            seqRoot.nxt = t;
            }

        public bool DoMaint() {
            return txt.DoMaint();
            }


        public string FileName() {
            if (nm == "") return nm;
            string fn = nm;
            string wd = Environment.CurrentDirectory.ToUpper();
            string fd = Path.GetDirectoryName(Path.GetFullPath(nm)).ToUpper();
            if (fd == wd) {
                fn = Path.GetFileName(fn);
                }
            return fn;
            }

        public string MenuLine() {
            string ml = changed ? "'" : " ";
            /*/nonwin Remove for windowed version
            ml += "-";
            // Remove for windowed version */
            //win Remove for non-windowed version
            ml += frm == null ? "-" : (frm.Nxt == null? "+" : "*");
            // Remove for non-windowed version */
            ml += ed.encodeEncoding(encoding);
            ml += this == ed.Txt ? ". " : "  ";
            return ml + FileName();
            }


        //win Remove for non-windowed version
        public void AddForm(ssForm f) {
            f.Nxt = frms;
            frms = f;
            frm = f;
            frm.Text = FileName();
            }


        public void DeleteForm(ssForm ff) {
            ssForm p = null, f = frms;
            while (f != null && f != ff) { p = f; f = f.Nxt; }
            if (f == null) return;
            if (p == null) {
                frms = f.Nxt;
                frm = frms;
                }
            else {
                p.Nxt = f.Nxt;
                frm = p;
                }
            }

        public void DeleteAllForms() {
            for (ssForm f = frms; f != null; f = f.Nxt) f.Close();
            }


        public bool LastForm(ssForm f) {
            return f == frms && f.Nxt == null;
            }


        // Remove for non-windowed version */


        public void Rename(string n) {
            changed = true;
            nm = n;
            ed.Msg(MenuLine());
            //win remove for non-windowed version
            for (ssForm f = frms; f != null; f = f.Nxt) f.Text = FileName();
            // remove for non-windowed version */
            }

        public bool SetFile(string n) {
            nm = n;
            return true;
            }


        public string Nm {
            get { return nm; }
            set { nm = value; }
            }

        public int Length {
            get { return txt.Length; }
            }

        public string Eoln {
            get {
                return EOLN;
                }
            set {
                if (value == "") { throw new ssException("invalid EOLN"); }
                EOLN = value;
                }
            }

        public bool Changed {
            get { return changed; }
            set { changed = value; }
            }

        public ssText Nxt {
            get { return nxt; }
            set { nxt = value; }
            }

        //win Remove for non-windowed version
        public ssForm Frms {
            get { return frms; }
            }

        public ssForm Frm {
            get { return frm; }
            set { frm = value; }
            }
        // Remove for non-windowed version */

        public char this[int i] {
            get { return txt[i]; }
            }

        public string ToString(int first, int length) {
            return txt.ToString(first, length);
            }

        public override string ToString() {
            return txt.ToString(dot.l, dot.len);
            }

        public ssRange Insert(char c) {
            return Insert(c.ToString());
            }

        public ssRange Insert(string s) {
            txt.Insert(dot.l, s);
            changed = true;
            firstTry = true;
            cmdaffected = true;
            mark.Adjust(dot.l, s.Length, true); // needed in case there's no window
            AdjMarks(dot.l, s.Length, true);
            dot.len = s.Length;
            return dot;
            }

        public ssRange Delete() {
            if (dot.l != dot.r) {
                txt.Remove(dot.l, dot.len);
                changed = true;
                firstTry = true;
                cmdaffected = true;
                mark.Adjust(dot.l, dot.len, false); // needed in case there's no window
                AdjMarks(dot.l, dot.len, false);
                dot.len = 0;
                }
            return dot;
            }


        public bool AtEOLN(int i) {
            if (fixedLn != 0) return (i % fixedLn) == fixedLn - 1;
            if (i == txt.Length || (0 <= i && i <= (txt.Length - EOLN.Length) && txt.ToString(i, EOLN.Length) == EOLN))
                return true;
            else return false;
            }


        public bool AtBOLN(int i) {
            if (fixedLn != 0) return (i % fixedLn) == 0;
            if (i == 0 || (i >= EOLN.Length && i <= txt.Length && txt.ToString(i - EOLN.Length, EOLN.Length) == EOLN))
                return true;
            else return false;
            }

       

        public bool AtBOW(int i) {
            if (i == txt.Length) return false;
            if (i == 0) return !char.IsWhiteSpace(txt[i]);
            return (!char.IsWhiteSpace(txt[i]) && char.IsWhiteSpace(txt[i - 1])) 
                || AtBOLN(i)
                || AtEOLN(i);
            }


        public bool AtEOW(int i) {
            if (i == 0) return false;
            if (i == txt.Length) return !char.IsWhiteSpace(txt[i - 1]);
            return (char.IsWhiteSpace(txt[i]) && !char.IsWhiteSpace(txt[i - 1]))
                || AtBOLN(i)
                || AtEOLN(i);
            }

        public bool AtProgBOW(int i) {
            if (i == txt.Length) return false;
            if (i == 0) return char.IsLetterOrDigit(txt[i]);
            return (char.IsLetterOrDigit(txt[i]) && !char.IsLetterOrDigit(txt[i - 1]))
                || AtBOLN(i)
                || AtEOLN(i)
                || i < txt.Length && IsPunctuation(txt[i]);
            }


        public bool AtProgEOW(int i) {
            if (i == 0) return false;
            if (i == txt.Length) return char.IsLetterOrDigit(txt[i - 1]);
            return (!char.IsLetterOrDigit(txt[i]) && char.IsLetterOrDigit(txt[i - 1]))
                || AtBOLN(i)
                || AtEOLN(i)
                || i < txt.Length && IsPunctuation(txt[i]);
            }

        public bool RangeAligned(ssRange r) {
            return AtBOLN(r.l) && AtBOLN(r.r);
            }

        public ssRange AlignRange(ref ssRange r) {
            if (r.l == r.r) r.r = NxtRight(r.r);
            r.l = To(AtBOLN, r.l, -1);
            r.r = To(AtBOLN, r.r, 1);
            return r;
            }


        public delegate bool PosTest(int i);


        public int To(PosTest tst, int start, int dir) {
            int i = start;
            while (0 <= i && i <= txt.Length) {
                if (tst(i)) return i;
                i += dir;
                }
            if (i >= txt.Length) return txt.Length;
            else if (i < 0) return 0;
            return 0;
            }


        public bool Contains(int i) {
            return i >= 0 && i <= txt.Length;
            }

        public bool Contains(ssRange r) {
            return r.l >= 0 && r.r <= txt.Length;
            }

        public int NxtLeft(int i) {
            if (i == 0) return i;
            if (fixedLn != 0) return i - 1;
            int inc = 0;
            if (AtBOLN(i)) inc = Eoln.Length;
            else inc = 1;
            return i - inc;
            }


        public int NxtRight(int i) {
            if (i == txt.Length) return i;
            if (fixedLn != 0) return i + 1;
            int inc = 0;
            if (AtEOLN(i)) inc = Eoln.Length;
            else inc = 1;
            return i + inc;
            }


        public void SeekEnd() {
            dot.To(txt.Length);
            }

        public void SeekHome() {
            dot.To(0);
            }

        public bool Ended() {
            return AtEOLN(Length - Eoln.Length);
            }


        public bool DoubleCheck() {
            if (changed) {
                if (firstTry) {
                    firstTry = false;
                    return false;
                    }
                else return true;
                }
            else return true;
            }



        delegate int FindLineDelegate(int i);

        public ssRange FindLine(int start, int n, int dir) {
            FindLineDelegate mov = NxtRight;
            if (dir < 0) mov = NxtLeft;
            n = Math.Abs(n);
            int tail = start;
            int head = To(AtBOLN, start, dir);
            int m = 0;
            while (m < n) {
                tail = head;
                head = To(AtBOLN, mov(head), dir);
                if (head == tail && n - m > 1) throw new ssException("address range");
                m++;
                }
            return new ssRange(tail, head).Normalize();
            }


        public void InitSeq() {
            adjEdge = Length;
            seqRoot.nxt = null;
            }

        public void CheckSeq(ref ssRange r, bool insert) {
            int newEdge = insert ? r.l : r.r;
            if (newEdge > adjEdge) { 
                ed.Undo(1);
                throw new ssException("changes not in sequence");
                }
            adjEdge = newEdge;
            }

        public void ShowInternals() {
            txt.Show(ed);
            }



        public void AdjMarks(int loc, int chg, bool insert) {
            //win Remove contents of this routine for non-windowing version
            for (ssForm f = frms; f != null; f = f.Nxt) {
                f.AdjMarks(loc, chg, insert);
                }
            // Remove contents of this routine for non-windowing version */
            }

        public void InvalidateMarks() {
            //win Remove contents of this routine for non-windowing version
            for (ssForm f = frms; f != null; f = f.Nxt) {
                f.InvalidateMarks();
                }
            // Remove contents of this routine for non-windowing version */
            }

        public void InvalidateMarksAndChange(int loc) {
            //win Remove contents of this routine for non-windowing version
            for (ssForm f = frms; f != null; f = f.Nxt) {
                f.InvalidateMarksAndChange(loc);
                }
            // Remove contents of this routine for non-windowing version */
            }

        public void SyncTextToForm() {
            //win Remove contents of this routine for non-windowing version
            if (frm != null) frm.FormMarksToText();
            // Remove contents of this routine for non-windowing version */
            }

        public void SyncFormToText() {
            //win Remove contents of this routine for non-windowing version
            if (frm != null) frm.TextMarksToForm(true);
            // Remove contents of this routine for non-windowing version */
            }

        public void Activate() {
            //win Remove contents of this routine for non-windowing version
            if (frm != null) {
                if (frm.WindowState == System.Windows.Forms.FormWindowState.Minimized)
                    frm.WindowState = System.Windows.Forms.FormWindowState.Normal;
                frm.Activate();
                }
            // Remove contents of this routine for non-windowing version */
            }


        //win Remove contents of this routine for non-windowing version
        public void MenuClick(Object sender, EventArgs e) {
            ed.WakeUpText(this);
            if (frms == null) {
                ssForm f = new ssForm(ed, this);
                AddForm(f);
                f.Show();
                }
            }
        // Remove contents of this routine for non-windowing version */


        public void ChangeEoln(string s) {
            EOLN = s;
            //win Remove contents of this routine for non-windowing version
            for (ssForm f = frms; f != null; f = f.Nxt) f.ReDisplay();
            // Remove contents of this routine for non-windowing version */
            }

        public void FixLineLen(int n) {
            fixedLn = n;
            //win Remove contents of this routine for non-windowing version
            for (ssForm f = frms; f != null; f = f.Nxt) f.ReDisplay();
            // Remove contents of this routine for non-windowing version */
            }

        }
    }
