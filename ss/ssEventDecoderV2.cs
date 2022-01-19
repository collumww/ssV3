using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using System.IO;

namespace ss {
    public class eStack {
        public eStack(ssEvent ee, eStack st) { e = ee; nxt = st; }
        public ssEvent e;
        public eStack nxt;
        }

    public class eSet {
        public eSet(eSet set) {nxt = set;}
        public string nm;
        public ssEvent e;
        public eSet nxt;
        }

    public class ssEventDecoderV2 {
        public ssEventDecoderV2(ssForm f) {
            frm = f;
            ssEventCmdOption x = ssEventCmdOption.exec;

            evFile = "ssEvents.ini";
            keyconv = new KeysConverter();
            methods = Type.GetType("ss.ssForm").GetMethods(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            defInsert = pAction("InsertChar", ref x);

            subsets = null;
            if (f == f.Ed.Log.Frm) {
                esets = LoadEvents(f, evFile);
                }
            else {
                esets = f.Ed.Log.Frm.Evd.esets;
                }
            curset = esets;
            SetEventSet(f.Ed.defs.eventSet);
            }


        public bool IsModifier(Keys k) {
            return k == Keys.ShiftKey
                || k == Keys.ControlKey
                || k == Keys.Menu
                || k == Keys.LButton
                || k == Keys.MButton
                || k == Keys.RButton
                || k == Keys.XButton1
                || k == Keys.XButton2;
            }


        public void SetKeyModifier(Keys k, bool val) {
            switch (k) {
                case Keys.ShiftKey: mShift = val; break;
                case Keys.ControlKey: mCtl = val; break;
                case Keys.Menu: mAlt = val; break;
                }
            if (!MouseModifierDown() && !KeyModifierDown()) Reset();
            }

        public void SetMouModifier(MouseButtons k, bool val) {
            switch (k) {
                case MouseButtons.Left: mMouLeft = val; break;
                case MouseButtons.Middle: mMouMiddle = val; break;
                case MouseButtons.Right: mMouRight = val; break;
                case MouseButtons.XButton1: mMouX1 = val; break;
                case MouseButtons.XButton2: mMouX2 = val; break;
                }
            if (!MouseModifierDown() && !KeyModifierDown()) Reset();
            }

        public bool ShouldIgnoreKeyPress() {
            return mCtl ||
                   mMouLeft ||
                   mMouMiddle ||
                   mMouRight ||
                   mMouX1 ||
                   mMouX2;
            }

        public bool MouseModifierDown() {
            return mMouLeft ||
                   mMouMiddle ||
                   mMouRight ||
                   mMouX1 ||
                   mMouX2;
            }

        public bool KeyModifierDown() {
            return mShift ||
                   mCtl ||
                   mAlt;
            }

        public bool Enabled() {
            return enabled;
            }

        

        public void Eat(Keys k, char c, ssEventType t) {
            if (curset == null) return;
            if (!enabled) {
                if (MouseModifierDown()) return;
                else enabled = true;
                }

            if (IsModifier(k) && SingleMatch(ematch.e, k, c, t)) {
                if (ematch.e.a == null)
                    return;
                }

            if (ematch.e.nxt == null) {
                Reset();
                return;
                }

            ssEvent e = ematch.e.nxt;
            int lcnt = 0;
            while (e != null) {
                if (SingleMatch(e, k, c, t)) {
                    break;
                    }
                e = e.alt; lcnt++;
                }

            if (e == null) {
                if (t == ssEventType.press) defInsert.Invoke(frm, null);
                Reset();
                return;
                }
            else {
                ematch = new eStack(e, ematch);
                if (e.a != null || e.cmd != "") {
                    if (e.a != null)
                        e.a.Invoke(frm, null);
                    else {
                        if (e.cmdopt == ssEventCmdOption.begin) {
                            frm.Ed.Msg(e.cmd);
                            frm.Ed.Log.Activate();
                            }
                        else {
                            string[] ss = e.cmd.Split(new string[] { "`" }, StringSplitOptions.None);
                            if (e.cmdopt == ssEventCmdOption.execPreserveDot) frm.SaveCursor();
                            foreach (string s in ss) frm.Ed.Do(s);
                            if (e.cmdopt == ssEventCmdOption.execPreserveDot) frm.RestoreCursor();
                            }
                        }

                    if (!e.cont) {
                        ematch = ematch.nxt;
                        enabled = false;
                        }
                    }
                }
            }


        public void Reset() {
            ClearModifiers();
            enabled = true;
            if (curset == null) return;
            ematch = new eStack(curset.e, null);
            }


        public void Pause() {
            }


        public string SetEventSet(string nm) {
            curset = FindEventSet(esets, nm);
            if (curset == null) curset = esets;
            Reset();
            if (curset != null) return curset.nm;
            else return "";
            }


        public void InitEventSetEnum() {
            esetenum = esets;
            }


        public string NextEventSet() {
            if (esetenum == null) return null;
            else {
                string s = esetenum.nm;
                esetenum = esetenum.nxt;
                return s;
                }
            }


        //---- Private things below ---------------------------------------------------------

        ssForm frm;
        MethodInfo defInsert;

        eSet esets;           // List of event sets
        eSet subsets;         // Reusable sections, used in parsing only.
        eSet curset;
        eSet esetenum;

        //ssEvent events;       // Current event set
        eStack ematch;        // The last event matched in the current chain.
        bool enabled;

        bool mShift, mCtl, mAlt, mMouLeft, mMouMiddle, mMouRight, mMouX1, mMouX2;

        string evFile;
        KeysConverter keyconv;
        MethodInfo[] methods;


        private eSet FindEventSet(eSet src, string nm) {
            while (src != null && src.nm != nm) src = src.nxt;
            return src;
            }


        private bool SingleMatch(ssEvent e, Keys k, char c, ssEventType t) {
            return t == e.t && (
                t == ssEventType.press && e.c == c ||
                t != ssEventType.press && e.k == k
                );
            }


        private void ClearModifiers() {
            mShift = mCtl = mAlt = mMouLeft = mMouMiddle = mMouRight = mMouX1 = mMouX2 = false;
            }




        // Parsing the events file...


        char pChar(string s) {
            if (s.Length == 0) return '\0';
            if (char.IsDigit(s[0])) {
                int x = Int32.Parse(s);
                return (char)x;
                }
            if (s.Length == 1) return char.Parse(s);
            throw new ssException("error parsing event field 'char' in events file");
            }



        ssEventType pType(string s) {
            switch (s) {
                case "down":
                    return ssEventType.down;
                case "up":
                    return ssEventType.up;
                case "press":
                    return ssEventType.press;
                case "move":
                    return ssEventType.move;
                case "turn":
                    return ssEventType.turn;
                default:
                    throw new ssException("error parsing event field 'type' in events file");
                }
            }


        MethodInfo pAction(string s, ref ssEventCmdOption cmdopt) {
            switch (s) {
                case "null":
                    cmdopt = ssEventCmdOption.none;
                    return null;
                case "ssCmd":
                    cmdopt = ssEventCmdOption.exec;
                    return null;
                case "ssCmdPreserveDot":
                    cmdopt = ssEventCmdOption.execPreserveDot;
                    return null;
                case "ssCmdBegin":
                    cmdopt = ssEventCmdOption.begin;
                    return null;
                }
            s = "Cmd" + s;
            foreach (MethodInfo m in methods) {
                if (m.Name == s) return m;  //(ssAction)m.CreateDelegate(actiontyp, f);
                }
            throw new ssException("error parsing event field 'action' in events file");
            }


        bool AtEventBoundry(ssScanner scnr) {
            return scnr.C == '|' || scnr.C == '+' || scnr.C == ';';
            }


        ssEvent pEvent(ssScanner scnr, ssForm f) {
            ssEvent e;
            try {
                scnr.GetStrSpDelim();
                eSet es = FindEventSet(subsets, scnr.S);
                if (es != null) {
                    e = es.e.nxt.copy();
                    if (!AtEventBoundry(scnr)) scnr.GetChar();
                    }
                else {
                    e = new ssEvent();
                    e.k = (Keys)keyconv.ConvertFromString(scnr.S);
                    scnr.GetStrSpDelim(); e.t = pType(scnr.S);
                    if (e.t == ssEventType.press) {
                        scnr.GetStrSpDelim(); e.c = pChar(scnr.S);
                        }
                    if (!AtEventBoundry(scnr)) {
                        scnr.GetStrSpDelim(); e.a = pAction(scnr.S, ref e.cmdopt);
                        }
                    if (!AtEventBoundry(scnr)) {
                        scnr.GetStrSpDelim(); e.cont = bool.Parse(scnr.S);
                        }
                    if (e.cmdopt != ssEventCmdOption.none) {
                        scnr.SetDelim("\n");
                        scnr.AllowComment = false;
                        scnr.GetStr(); e.cmd = scnr.S;
                        scnr.AllowComment = true;
                        scnr.SetDelim("");
                        if (!AtEventBoundry(scnr)) scnr.GetChar();
                        }
                    }
                }
            catch (Exception ee) {
                scnr.SetDelim("");
                scnr.AllowComment = true;
                throw new ssException("error parsing event in events file" + "\r\n?" + ee.Message);
                }
            return e;
            }


        ssEvent LastAlt(ssEvent e) {
            while (e.alt != null) e = e.alt;
            return e;
            }


        ssEvent pEventTree(ssScanner scnr, ssForm f) {
            ssEvent rt = pEvent(scnr, f);
            ssEvent e = LastAlt(rt);

            for (;;) {
                switch (scnr.C) {
                    case '|':
                        scnr.GetChar();
                        e.alt = pEvent(scnr, f);
                        e = LastAlt(e);
                        break;
                    case '+':
                        scnr.GetChar();
                        e.nxt = pEventTree(scnr, f);
                        break;
                    case ';':
                        scnr.GetChar();
                        return rt;
                    case '\0':
                        return rt;
                    default:
                        if (char.IsWhiteSpace(scnr.C)) {
                            scnr.GetChar();
                            }
                        else throw new ssException("error parsing event tree in events file");
                        break;
                    }
                }
            }


        eSet pSet(ssScanner scnr, ssForm f) {
            eSet es = null;
            eSet ss = null;
            for (;;) {
                switch (scnr.C) {
                    case '=':
                    case '<':
                        char c = scnr.C;
                        scnr.GetChar();
                        scnr.GetStrSpDelim();
                        eSet s = new eSet(null);
                        s.nm = scnr.S;
                        s.e = new ssEvent();
                        s.e.nxt = pEventTree(scnr, f);
                        if (c == '=') {
                            s.nxt = es;
                            es = s;
                            }
                        else {
                            s.nxt = subsets;
                            subsets = s;
                            }
                        break;
                    case '\0':
                        return es;
                    default:
                        if (char.IsWhiteSpace(scnr.C)) {
                            scnr.GetChar();
                            }
                        else throw new ssException("error parsing event set in events file");
                        break;
                    }
                }
            }



        private eSet LoadEvents(ssForm f, string fnm) {
            eSet es = null;
            //ssEvent es = new ssEvent();

            int cnt = 0;
            ssScanner scnr = null;
            while (cnt < 2 && scnr == null) {
                try {
                    cnt++;
                    try {
                        scnr = new ssScanner(File.ReadAllText(fnm), true); // try local
                        }
                    catch {
                        fnm = System.IO.Path.GetDirectoryName(Application.ExecutablePath) +
                            System.IO.Path.DirectorySeparatorChar + fnm;
                        scnr = new ssScanner(File.ReadAllText(fnm), true); // try exe one
                        }
                    scnr.GetChar();
                    }
                catch {
                    try {
                        if (cnt < 2) {
                            f.Ed.MsgLn("creating default events file");
                            StreamWriter sw = new StreamWriter(fnm);
                            sw.WriteLine(defEvents);
                            sw.WriteLine("# Available commands:");
                            foreach (MethodInfo m in methods) {
                                if (m.Name.Length > 2 && m.Name.Substring(0,3) == "Cmd")
                                    sw.WriteLine(string.Format("#   {0}", m.Name));
                                }
                            sw.Close();
                            }
                        else {
                            throw new ssException("no events file");
                            }
                        }
                    catch (Exception ee) {
                        f.Ed.Err("error writing default events file" + "\r\n" + ee.Message);
                        }
                    }
                }

            try {
                es = pSet(scnr, f);
                }
            catch (Exception e) {
                if (f == f.Ed.Log.Frm) f.Ed.Err(e.Message + ": " + scnr.S); // Guard this so you don't get messages for every form created.
                }
            return es;
            }

        // Defaults for event handling

        private const string defEvents =
@"# Key Character type (up down press move turn) action continue to match

# --- Reusable stuff -------------------------------------

< Basics
  Tab down TabOrIncreaseIndent
| Enter down Enter
| PageUp down ScrollTextDownOnePage
| PageDown down ScrollTextUpOnePage
| Escape down ToggleToCommand
| ShiftKey up ExtendSelectionOff
;

< BasicHomeEnd
  Home down CursorToBOParagraph
| End down CursorToEOParagraph
;

< ControlHomeEnd
  Home down CursorToBOF
| End down CursorToEOF
;

< CharArrows
  Left down CursorLeft 
| Right down CursorRight 
| Up down CursorUp
| Down down CursorDown
;

< WordArrows
  Left down CursorToNextEOWordLeft 
| Right down CursorToNextWordRight 
| Up down CursorUpToNearestBOWord
| Down down CursorDownToNearestBOWord
;

< CharMovement
  CharArrows
| Back down Back
| Delete down Delete
;

< WordMovement
  WordArrows		
| Back down BackDeleteEOWord
| Delete down DeleteWord
;

< CharLButton
  LButton down CursorFromMouse true
;

< WordLButton
  LButton down CursorToNearestBOWordWithEntitySelect true
;

< Mnemonics
  S down Save
| O down Open
| N down New
| D down NextFont
| H down PrevText
| T down NextText
| A down SelectAll
| F down ssCmdBegin false /
| X down Cut
| C down Snarf
| V down Paste
| Z down Undo
| G down Xerox
| W down ToggleWrap
| U down ToggleToCommand
;

< AltStuff
  Home down CursorToBOLine
| End down CursorToEOLine
| ShiftKey down
	+ Left down MoveTextCharLeft
	| Right down MoveTextCharRight
	;
| Home down CursorToBOLine
| End down CursorToEOLine
| Left down MoveTextWordLeft
| Right down MoveTextWordRight
| Up down MoveLinesUp
| Down down MoveLinesDown
;


< LButtonPositionalQwerty
  D down MakeMarkFromCursor   
| F down CopyMarkToCursor 
| S down MoveMarkToCursor 
| A down MatchBraces true
| E down CopyCursorToMarkBeginning 
| R down CopyCursorToMarkEnd
| C down CursorToMarkBeginning 
| V down CursorToMarkEnd
| G down ShowMark 
| T down ToggleCrossHairs
| RButton down Cut
| MButton down Snarf
;

< LButtonPositionalDvorak
  E down MakeMarkFromCursor   
| U down CopyMarkToCursor 
| O down MoveMarkToCursor 
| A down MatchBraces true
| OemPeriod down CopyCursorToMarkBeginning 
| P down CopyCursorToMarkEnd
| J down CursorToMarkBeginning 
| K down CursorToMarkEnd
| I down ShowMark 
| Y down ToggleCrossHairs
| RButton down Cut
| MButton down Snarf
;

< RButtonActions
  RButton up ShowMenu
| LButton down PastePreserveSelection
| P down ssCmdPreserveDot false y/\r\n/i/#/
| OemPeriod down ssCmdPreserveDot false x/^#/d
| I down ssCmdPreserveDot false {`i'/*'`a'*/'`}
| U down ssCmdPreserveDot false y/\r\n/i/\/\//
| E down ssCmdPreserveDot false x/^\/\//d
;



= DvorakWord
  Basics
| WordMovement
| BasicHomeEnd
| ControlKey down  
    + Mnemonics
    | CharMovement
    | ControlHomeEnd
    | CharLButton
    ;
| ShiftKey down ExtendSelectionOn true
    + ShiftKey up ExtendSelectionOff
    | WordLButton
    | WordMovement
    | BasicHomeEnd
    | Tab down DecreaseIndent
    | ControlKey down
       + CharLButton
       | CharMovement
       | ControlHomeEnd
       | S down SaveAs
       ;
    ;
| Menu down
	+ AltStuff
	;
| LButton down CursorToNearestBOWordWithEntitySelect true
	+ LButtonPositionalDvorak
	;
| RButton down
	+ RButtonActions
	;
;

= Dvorak
  Basics
| CharMovement
| BasicHomeEnd
| ControlKey down  
    + Mnemonics
    | WordMovement
    | ControlHomeEnd
    | WordLButton
    ;
| ShiftKey down ExtendSelectionOn true
    + ShiftKey up ExtendSelectionOff
    | CharLButton
    | CharMovement
    | BasicHomeEnd
    | Tab down DecreaseIndent
    | ControlKey down
       + WordLButton
       | WordMovement
       | ControlHomeEnd
       | S down SaveAs
       ;
    ;
| Menu down
	+ AltStuff
	;
| LButton down CursorFromMouseWithEntitySelect true
	+ LButtonPositionalDvorak
	;
| RButton down
	+ RButtonActions
	;
;

= QwertyWord
  Basics
| WordMovement
| BasicHomeEnd
| ControlKey down  
    + Mnemonics
    | CharMovement
    | ControlHomeEnd
    | CharLButton
    ;
| ShiftKey down ExtendSelectionOn true
    + ShiftKey up ExtendSelectionOff
    | WordLButton
    | WordMovement
    | BasicHomeEnd
    | Tab down DecreaseIndent
    | ControlKey down
       + CharLButton
       | CharMovement
       | ControlHomeEnd
       | S down SaveAs
       ;
    ;
| Menu down
	+ AltStuff
	;
| LButton down CursorToNearestBOWordWithEntitySelect true
	+ LButtonPositionalQwerty
	;
| RButton down
	+ RButtonActions
	;
;



= Qwerty
  Basics
| CharMovement
| BasicHomeEnd
| ControlKey down  
    + Mnemonics
    | WordMovement
    | ControlHomeEnd
    | WordLButton
    ;
| ShiftKey down ExtendSelectionOn true
    + ShiftKey up ExtendSelectionOff
    | CharLButton
    | CharMovement
    | BasicHomeEnd
    | Tab down DecreaseIndent
    | ControlKey down
       + WordLButton
       | WordMovement
       | ControlHomeEnd
       | S down SaveAs
       ;
    ;
| Menu down
	+ AltStuff
	;
| LButton down CursorFromMouseWithEntitySelect true
	+ LButtonPositionalQwerty
	;
| RButton down
	+ RButtonActions
	;
;

";
        }
    }
