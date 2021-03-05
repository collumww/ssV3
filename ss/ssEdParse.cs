using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace ss {
    public partial class ssEd {
        ssScanner scn;
        int grouping;
        string lastPat;

        CTreeList cts;
        CTree tail;
        CTree root;

        bool swallowing;
        CTree stomach;


        ssAddress edDot {
            get {
                if (txt == null) throw new ssException("no current file");
                return new ssAddress(txt.dot, txt);
                }
            }


        class ATree {
            public ATree(char oo, int nn, string ss, string ff) {
                op = oo;
                n = nn;
                s = ss;
                fnm = ff;
                }
            public char op;
            public int n;
            public string s;
            public string fnm;
            public ATree l;
            public ATree r;
            }

        class CTree {
            public CTree(ATree adr, char c) {
                a = adr;
                cmd = c;
                }
            public char cmd;
            public ATree a;
            public ATree ad;
            public int n;
            public string s;
            public string rep;
            public char opt;
            public CTree sub;
            public CTree nxt;
            public TList txts;
            public SList fs;
            public SubList subs;
            }

        class CTreeList {
            public CTreeList(CTree tt, CTreeList n, bool c) {
                t = tt; nxt = n; compound = c;
                }
            public CTree t;
            public bool compound;
            public CTreeList nxt;
            }


        class SList {
            public string s;
            public SList nxt;
            }


        string SetPat(string pat)
        {
            if (pat == "") return lastPat;
            lastPat = pat;
            return pat;
        }


        string SListJoin(SList l) {
            string s = "";
            while (l != null) { s += l.s + " "; l = l.nxt; }
            return s;
            }



        void Commit() {
            for (ssText t = txts; t != null; t = t.Nxt) {
                if (!t.TLog.getnewtrans) t.TLog.Commit();
                }
            }



        public void Do(string s) {
            try {
                ResetAffected();
                InitAllSeqs();
                //edDot.txt = txt;
                //edDot.rng = txt.dot;
                NewTrans();
                ParseAndExec(s);
                Commit();
                UpdateAffected();
                //win remove for non-windowed version
                if (txt != null && txt.Frm != null) txt.Frm.CmdShowCursor();
                if (!log.Ended()) MsgLn("");
                // remove for non-windowed version */
                }
            catch (Exception e) {
                grouping = 0;
                cts = null;
                swallowing = false;
                root.nxt = null;
                root.sub = null;
                tail = root;
                InitAllSeqs();
                SyncFormToTextAll();
                Err(e.Message);
                //if (!(e is ssException)) throw e;
                }
            }


        void DoTextMaint(ssText t) {
            if (t.DoMaint()) {
                MsgLn("Maintenance done on '" + t.FileName() + "'");
                }
            }


        void ResetAffected() {
            for (ssText t = txts; t != null; t = t.Nxt) {
                t.cmdaffected = false;
                t.InvalidateMarks();
                }
            }

        void UpdateAffected() {
            for (ssText t = txts; t != null; t = t.Nxt) {
                if (t.cmdaffected) {
                    t.InvalidateMarksAndChange(-1);
                    ////win Remove for non-windowed version
                    //for (ssForm f = t.Frms; f != null; f = f.Nxt) {
                    //    f.AdjOrigin();
                    //    }
                    //// Remove for non-windowed version */
                    DoTextMaint(t);
                    }
                }
            }


        void ParseAndExec(string s) {
            if (swallowing) {
                if (s == ".") {
                    swallowing = false;
                    if (grouping == 0)
                        xCmd(root);
                    }
                else stomach.s = stomach.s + Unescape(s) + edDot.txt.Eoln;
                }
            else {
                scn.Init(s, false);
                pChar();
                if (grouping == 0) {
                    root.nxt = null;
                    root.sub = null;
                    tail = root;
                    }
                pCmd();
                if (grouping == 0)
                    xCmd(root);
                }
            }


        void PushCmd(CTree t, bool c) {
            cts = new CTreeList(t, cts, c);
            grouping++;
            }

        CTree PopCmd() {
            if (cts != null) {
                CTree t = cts.t;
                cts = cts.nxt;
                grouping--;
                return t;
                }
            else throw new ssException("unmatched '}'");
            }


        void CheckEOT() {
            scn.GetChar();
            if (!scn.EOT()) throw new ssException("expected newline");
            }


        void CheckTxt() {
            if (txt == null) throw new ssException("no current file");
            }

        //win remove for non-windowed version
        void CheckFrm() {
            if (txt.Frm == null) throw new ssException("no window for current file");
            }
        // remove for non-windowed version */

        void CheckNoAddr(ATree a) {
            if (a != null) throw new ssException("command takes no address");
            }

        void pSkipSp() {
            while (!scn.EOT() && char.IsWhiteSpace(scn.C)) scn.GetChar();
            }


        void pChar() {
            do { scn.GetChar(); } while (!scn.EOT() && char.IsWhiteSpace(scn.C));
            }

        void pSpOrEot() {
            scn.GetChar();
            if (scn.C != ' ' && scn.C != '\0') throw new ssException("blank expected");
            }

        char pDelim() {
            if (char.IsWhiteSpace(scn.C)
                || char.IsDigit(scn.C)
                || char.IsLetter(scn.C)
                || scn.C == '\\') throw new ssException("bad delimiter '" + scn.C + "'");
            else return scn.C;
            }


        void pAppend(CTree t) {
            tail.nxt = t;
            tail = tail.nxt;
            }


        SList pFileList(bool justone) {
            SList h = new SList();
            SList t = h;
            while (!scn.EOT()) {
                t.nxt = new SList();
                t = t.nxt;
                if (scn.C == '"') {
                    char d = pDelim();
                    scn.GetChar();
                    scn.SetDelim(d);
                    t.s = scn.GetStr();
                    }
                else t.s = scn.GetStrSpDelim();
                if (justone) return h.nxt;
                }
            return h.nxt;
            }


        const char listHead = '\u0001';
        const char noCmd = '\u0002';


        void pCmd() {
            if (txt != null) txt.SyncTextToForm();
            ATree a = pARange();
            CTree t = null;
            char c = scn.C;
            switch (c) {
                case '{':
                    t = new CTree(a, c);
                    pAppend(t);
                    PushCmd(tail, false);
                    t.sub = new CTree(null, listHead);
                    tail = t.sub;
                    pChar();
                    if (!scn.EOT()) {
                        pCmd();
                        }
                    CheckEOT();
                    return;
                case '}':
                    CheckEOT();
                    tail = PopCmd();
                    if (cts != null && cts.compound) tail = PopCmd();
                    return;
                case 'a':
                case 'i':
                case 'c':
                    CheckTxt();
                    t = new CTree(a, c);
                    pChar();
                    if (scn.EOT()) {
                        swallowing = true;
                        stomach = t;
                        }
                    else {
                        scn.SetDelim(pDelim());
                        scn.GetChar();
                        t.s = Unescape(scn.GetStr());
                        t.subs = PrepForSub(t.s, false);
                        }
                    CheckEOT();
                    break;
                case 't':
                case 'm':
                    CheckTxt();
                    t = new CTree(a, c);
                    pChar();
                    t.ad = pARange();
                    if (t.ad == null) throw new ssException("address");
                    CheckEOT();
                    break;
                case 'x':
                case 'y':
                case 'g':
                case 'v':
                case 'X':
                case 'Y':
                    t = new CTree(a, c);
                    if (t.cmd != 'X' && t.cmd != 'Y') CheckTxt();
                    pAppend(t);
                    PushCmd(t, true);
                    pChar();
                    scn.SetDelim(pDelim());
                    scn.GetChar();
                    t.s = SetPat(PreEscape(scn.GetStr()));
                    t.sub = new CTree(null, listHead);
                    tail = t.sub;
                    int group = grouping;
                    pCmd();
                    if (group == grouping) tail = PopCmd();
                    return;
                case 's':
                    t = new CTree(a, c);
                    pChar();
                    scn.SetDelim(pDelim());
                    scn.GetChar();
                    t.s = SetPat(PreEscape(scn.GetStr()));
                    t.rep = Unescape(scn.GetStr());
                    t.subs = PrepForSub(t.rep, true);
                    t.opt = scn.C;
                    if (t.opt != 'g' && t.opt != '\0') throw new ssException("expected newline");
                    CheckEOT();
                    break;
                case 'D':
                case 'B':
                case 'b':
                    CheckNoAddr(a);
                    t = new CTree(a, c);
                    pChar();
                    if (scn.C == '<') {
                        pChar();
                        scn.SetDelim('\0');
                        string cmd = scn.GetStr();
                        scn.Init(ShellCmd(cmd, null), false);
                        pChar();
                        t.fs = pFileList(false);
                        }
                    else {
                        if (!scn.EOT()) t.fs = pFileList(false);
                        }
                    break;
                case 'f':
                case 'e':
                case 'r':
                case 'w':
                    if (c != 'w') CheckNoAddr(a);
                    t = new CTree(a, c);
                    pSpOrEot();
                    pChar();
                    t.fs = pFileList(true);
                    CheckEOT();
                    break;
                case '=':
                case 'p':
                case 'd':
                case 'k':
                case 'h':
                    CheckEOT();
                    CheckTxt();
                    t = new CTree(a, c);
                    break;
                case 'n':
                case 'q':
                case 'Q':
                case 'H':
                    CheckEOT();
                    t = new CTree(a, c);
                    break;
                case 'u':
                    CheckTxt();
                    t = new CTree(a, c);
                    pChar();
                    t.n = scn.GetNum();
                    if (scn.Nothing) t.n = 1;
                    CheckEOT();
                    break;
                case '!':
                case '<':
                case '>':
                case '|':
                    if (c == '!') CheckNoAddr(a);
                    else CheckTxt();
                    t = new CTree(a, c);
                    pChar();
                    scn.SetDelim('\0');
                    t.s = scn.GetStr();
                    break;
                case '\0':
                    t = new CTree(a, noCmd);
                    break;
                case 'T':
                    CheckNoAddr(a);
                    CheckTxt();
                    //win remove for non-windowed version
                    CheckFrm();
                    // remove for non-windowed version */
                    t = new CTree(a, c);
                    pChar();
                    t.n = scn.GetNum();
                    if (t.n == 0) t.n = 4; // default spaces in tab
                    CheckEOT();
                    break;
                case 'L':
                    CheckNoAddr(a);
                    CheckTxt();
                    t = new CTree(a, c);
                    pChar();
                    t.s = scn.GetStrSpDelim();
                    if (t.s == "") t.s = "\r\n"; // default line ending for Windows
                    t.s = Unescape(t.s);
                    CheckEOT();
                    break;
                case 'F':
                    CheckNoAddr(a);
                    CheckTxt();
                    t = new CTree(a, c);
                    pChar();
                    t.n = scn.GetNum();
                    CheckEOT();
                    break;
                case 'C':
                    t = new CTree(a, c);
                    pChar();
                    t.opt = scn.C;
                    if ("si".IndexOf(t.opt) < 0) CheckTxt();
                    if ("ultsi\0".IndexOf(t.opt) < 0)
                        throw new ssException("bad option or expected newline");
                    if (t.opt == '\0') t.opt = 'l';
                    CheckEOT();
                    break;
                case 'E':
                    CheckNoAddr(a);
                    t = new CTree(a, c);
                    pChar();
                    t.opt = scn.C;
                    if ("au387\0".IndexOf(t.opt) < 0) // letters for encodings: ascii, unicode, utf-32, etc.
                        throw new ssException("bad option or expected newline");
                    if (t.opt == '\0') t.opt = '8';
                    CheckEOT();
                    break;
                default:
                    throw new ssException("unknown command '" + scn.C + "'");
                }
            pAppend(t);
            }



        ATree pARange() {
            ATree al = pATerm();
            while (scn.C == ',' || scn.C == ';') {
                char c = scn.C;
                if (al == null) al = new ATree('#', 0, "", null);
                pChar();
                ATree ar = pATerm();
                if (ar == null) ar = new ATree('$', 0, "", null);
                ATree x = new ATree(c, 0, "", null);
                x.l = al;
                x.r = ar;
                al = x;
                }
            return al;
            }


        ATree pATerm() {
            char c;
            ATree al = pASimple(false);
            while ("+-0123456789#'$./?".IndexOf(scn.C) != -1) {
                if (al == null) al = new ATree('.', 0, "", null);
                if (scn.C == '+' || scn.C == '-') {
                    c = scn.C;
                    pChar();
                    }
                else c = '+';
                ATree ar = pASimple(true);
                if (ar == null) ar = new ATree('0', 1, "", null);
                ATree x = new ATree(c, 0, "", null);
                x.l = al;
                x.r = ar;
                al = x;
                }
            return al;
            }

        ATree pASimple(bool rel) {
            string fnm = null;
            if (scn.C == '"') {
                scn.GetChar();
                scn.SetDelim('\"');
                fnm = scn.GetStr();
                }
            pSkipSp();
            char c = scn.C;
            if (char.IsDigit(c)) c = '0';
            switch (c) {
                case '#':
                    pChar();
                    int n = scn.GetNum();
                    if (scn.Nothing) n = 1;
                    return new ATree('#', n, "", fnm);
                case '0':
                    n = scn.GetNum();
                    pSkipSp();
                    if (scn.Nothing) n = 1;
                    return new ATree(c, n, "", fnm);
                case '/':
                case '?':
                    pChar();
                    scn.SetDelim(c);
                    string s = SetPat(PreEscape(scn.GetStr()));
                    pSkipSp();
                    return new ATree(c, 0, s, fnm);
                case '.':
                case '$':
                case '\'':
                    if (rel) throw new ssException("address");
                    char x = scn.C;
                    pChar();
                    return new ATree(x, 0, "", fnm);
                default:
                    if (fnm != null) return new ATree('.', 0, "", fnm);
                    break;
                }
            return null;
            }



        }
    }