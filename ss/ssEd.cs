using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace ss {
    public partial class ssEd {
        public ssEd(string[] ars, int sw) {
            args = ars;
            startWith = sw;
            defs = new ssDefaults();
            txts = null;
            txt = null;
            prvtxt = null;
            //tlog = new ssTransLog();
            curTransId = 0;
            scn = new ssScanner(null, false);
            grouping = 0;
            lastPat = "";
            newfile = 0;
            root = new CTree(null, '\0');
            //win remove for non-windowed version
            log = null;
            // remove for non-windowed version */
            }

        //--- Private ------------------------------------------

        string[] args;
        int startWith;
        ssText txts;
        //ssTransLog tlog;
        long curTransId;
        int newfile;
        //win remove for non-windowed version
        ssText log;
        // remove for non-windowed version */

        //--- Public ------------------------------------------

        public ssText txt;
        public ssText prvtxt;
        public ssDefaults defs;

        public string WinRead(string nm, ref Encoding enc) {
            string s = null;
            try {
                StreamReader sr = new StreamReader(nm, true);
                s = sr.ReadToEnd();
                enc = sr.CurrentEncoding;
                sr.Close();
                }
            catch (Exception e) {
                Err("problem reading \"" + nm + "\"\r\n" + e.Message);
                }
            return s;
            }


        public bool WinWrite(string nm, string s, Encoding enc) {
            try {
                StreamWriter sw = new StreamWriter(nm, false, enc);
                sw.Write(s);
                sw.Close();
                return true;
                }
            catch (Exception e) {
                Err("problem writing \"" + nm + "\"\r\n" + e.Message);
                }
            return false;
            }


        public void NewTrans() {
            curTransId++;
            }

        public long CurTransId {
            get { return curTransId; }
            }

        public string LastPat {
            get { return lastPat; }
            }


        private string NewName() {
            char c = (char)(newfile + (int) 'a');
            newfile = (newfile + 1) % 26;
            return "".PadRight(3, c);
            }


        public void NewText() {
            ssText t = new ssText(this, "", null, NewName(), defs.encoding);
            AddText(t);
            //win Remove for non-windowed version
            t.AddForm(new ssForm(this, t));
            t.Frm.Show();
            // Remove for non-windowed version */
            MsgLn(t.MenuLine());
            }


        public void AddText(ssText t) {
            t.Nxt = txts;
            prvtxt = txt;
            txts = txt = t;
            }

        public void AddTexts(string[] fs, bool withforms) {
            foreach (string f in fs) {
                Encoding enc = defs.encoding;
                string dta = WinRead(f, ref enc);
                if (dta == null) dta = "";
                ssText t = new ssText(this, dta, null, f, enc);
                AddText(t);  // t becomes the current text, that is, txt.
                if (withforms) {
                    txt = t;
                    //win Remove for non-windowed version
                    t.AddForm(new ssForm(this, t));
                    t.Frm.Show();
                    // Remove for non-windowed version */
                    }
                MsgLn(t.MenuLine());
                }
            }



        public bool DeleteText(ssText dtxt, bool formsToo) {
            if (!dtxt.DoubleCheck()) {
                Err("changed file \"" + dtxt.FileName() + "\"");
                return false;
                }
            //win remove for non-Windowed version
            if (formsToo) dtxt.DeleteAllForms();
            // remove for non-Windowed version */
            ssText p = null, t = txts;
            while (t != null && t != dtxt) { p = t; t = t.Nxt; }
            if (t == null) return true;
            if (p == null) txts = t.Nxt;
            else p.Nxt = t.Nxt;
            //ssTrans.VoidTrans(tlog.Ts, dtxt);
            //ssTrans.VoidTrans(seqRoot.nxt, dtxt);
            prvtxt = txt;
            txt = null;
            return true;
            }

        public bool DeleteAllTexts() {
            bool all = true;
            for (ssText t = Txts; t != null; t = t.Nxt) {
                //win remove for non-windowed version
                if (t != Log)
                // remove for non-windowed version */
                    all &= t.DoubleCheck();
                }
            if (!all) {
                Err("changed file(s)");
                }
            return all;
            }


        public void WakeUpText(ssText t) {
            if (t == null) return;
            prvtxt = txt;
            //win remove for non-windowed version
            if (t.Frm != null) {
                t.Activate();
                }
            else
            // remove for non-windowed version */
                {
                txt = t;
                //win remove for non-windowed version
                log.Activate();
                // remove for non-windowed version */
                }
            }

        public void NextText() {
            if (txts == null || txt == null) return;
            ssText t = txt.Nxt;
            if (t == null) t = txts;
            //win remove for non-windowed version
            if (t.Frm == null)
                // remove for non-windowed version */
                MsgLn(t.MenuLine());
            WakeUpText(t);
            }

        public void PrevText() {
            if (prvtxt == null) return;
            //win remove for non-windowed version
            if (prvtxt.Frm == null)
                // remove for non-windowed version */
                MsgLn(prvtxt.MenuLine());
            WakeUpText(prvtxt);
            }

        //win remove for non-windowed version
        public ssText Log {
            get { return log; }
            set { log = value; }
            }
        // remove for non-windowed version */

        public ssText Txts {
            get { return txts; }
            }

        public ssText Txt {
            get { return txt; }
            }

        //public ssTransLog TLog {
        //    get { return tlog; }
        //    }

        public void Err(string s) {
            MsgLn("?" + s);
            }

        public void MsgLn(string s) {
            //win Remove for non-windowed version
            Msg(s + log.Eoln);
            // Remove for non-windowed version */
            /*/nonwin Remove for windowed version 
            Console.WriteLine(s);
            // Remove for Windowed version */
            }

        public void Msg(string s) {
            //win Remove for non-windowed version
            log.Frm.CmdCursorToEOF();
            log.Frm.Insert(s);
            log.Frm.CmdCursorToEOF();
            // Remove for non-windowed version */
            /*/nonwin Remove for windowed version 
            Console.Write(s);
            // Remove for Windowed version */
            }

        public void ProcessArgs() {
            for (int i = startWith; i < args.Length; i++) {
                try {
                    string arg = args[i];
                    string pth = Path.GetDirectoryName(arg);
                    if (pth == "") pth = ".";
                    string fnm = Path.GetFileName(arg);
                    string[] fs = Directory.GetFiles(pth, fnm);
                    if (fs.Length == 0) {
                        if (arg.IndexOf('*') == -1 && arg.IndexOf('?') == -1) {
                            ssText t = new ssText(this, "", null, arg, defs.encoding);
                            AddText(t);
                            MsgLn("arg: could not find '" + arg + "', new file");
                            }
                        else Err("arg: could not find '" + arg + "'");
                        }
                    else {
                        AddTexts(fs, false);
                        }
                    }
                catch (Exception ex) {
                    Err("arg: " + ex.Message);
                    }
                }
            //win Remove for non-windowed version
            if (txt != null) {
                txt.AddForm(new ssForm(this, txt));
                txt.Frm.Show();
                }
            // Remove for non-windowed version */
            }



        }
    }
