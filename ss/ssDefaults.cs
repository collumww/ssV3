using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Diagnostics;

namespace ss {

    public class ssDefaults {
        public string defsLoc;
        public string exePath;
        public string cmd;
        public string cmdArgs;
        public string progPunct;
        public bool senseCase;
        public Encoding encoding;
        //win remove for non-windowed version
        public static int fontCnt = 4;
        public int fontNum;

        public string[] fontNm;
        public FontStyle[] fontStyle;
        public float[] fontSz;

        public bool wrap;
        public bool autoIndent;
        public bool programming;
        public int spInTab;
        public bool expTabs;
        public string eventSet;
        public int top;
        public int left;
        public int width;
        public int height;
        // remove for non-windowed version */

        //win remove for non-windowed version
        public static string defsFnm = "ssDefs.ini";
        // remove for non-windowed version */
        /*/nonwin remove for windowed version
        public static string defsFnm = "ssConDefs.ini";
        // remove for windowed version */

        public static Encoding DecodeEncoding(string es) {
            switch (es) {
                case "US-ASCII": return Encoding.ASCII;
                case "Unicode": return Encoding.Unicode;
                case "Unicode (UTF-32)": return Encoding.UTF32;
                case "Unicode (UTF-8)": return Encoding.UTF8;
                case "Unicode (UTF-7)": return Encoding.UTF7;
                default: return Encoding.UTF8;
                }
            }


        public ssDefaults() {
            //win remove for non-windowed version
            fontNm = new string[fontCnt];
            fontStyle = new FontStyle[fontCnt];
            fontSz = new float[fontCnt];
            // remove for non-windowed version */

            exePath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            //string fnm = exePath + Path.DirectorySeparatorChar + defsFnm;
            defsLoc = defsFnm;
            StreamReader r = null;
            try {
                try {
                    r = new StreamReader(defsLoc); // try local first
                    }
                catch {
                    defsLoc = exePath + Path.DirectorySeparatorChar + defsFnm;
                    r = new StreamReader(defsLoc);  // now, try the one where the exe is
                    }
                cmd = r.ReadLine();
                cmdArgs = r.ReadLine();
                progPunct = r.ReadLine();
                senseCase = System.Convert.ToBoolean(r.ReadLine());
                encoding = DecodeEncoding(r.ReadLine()); 
                //win remove for non-windowed version
                fontNum = System.Convert.ToInt32(r.ReadLine());
                for (int i = 0; i < fontCnt; i++) {
                    fontNm[i] = r.ReadLine();
                    fontStyle[i] = (FontStyle)System.Convert.ToInt32(r.ReadLine());
                    fontSz[i] = (float)System.Convert.ToDouble(r.ReadLine());
                    }
                wrap = System.Convert.ToBoolean(r.ReadLine());
                autoIndent = System.Convert.ToBoolean(r.ReadLine());
                programming = System.Convert.ToBoolean(r.ReadLine());
                spInTab = System.Convert.ToInt32(r.ReadLine());
                expTabs = System.Convert.ToBoolean(r.ReadLine());
                eventSet = r.ReadLine();
                top = System.Convert.ToInt32(r.ReadLine());
                left = System.Convert.ToInt32(r.ReadLine());
                width = System.Convert.ToInt32(r.ReadLine());
                height = System.Convert.ToInt32(r.ReadLine());
                // remove for non-windowed version */
                r.Close();
                }
            catch {
                cmd = "cmd.exe";
                cmd = "powershell.exe";
                cmdArgs = "/C {0}";
                cmdArgs = "-nologo -noninteractive -inputformat text -outputformat text -windowstyle hidden -executionpolicy bypass -command -";
                progPunct = "!@#$%^&*()`~;:/?[{]}'\",<.>\\|+=-_";
                senseCase = true;
                encoding = System.Text.Encoding.UTF8;
                //win remove for non-windowed version
                fontNum = 0;
                fontNm[0] = "Courier New";
                fontStyle[0] = FontStyle.Regular;
                fontSz[0] = 10;
                fontNm[1] = "Georgia";
                fontStyle[1] = FontStyle.Regular;
                fontSz[1] = 10;
                fontNm[2] = "Consolas";
                fontStyle[2] = FontStyle.Regular;
                fontSz[2] = 10;
                fontNm[3] = "MS Sans Serif";
                fontStyle[3] = FontStyle.Regular;
                fontSz[3] = 10;
                wrap = false;
                autoIndent = false;
                programming = false;
                spInTab = 4;
                expTabs = false;
                eventSet = "Qwerty";
                top = 50;
                left = 50;
                width = 600;
                height = 150;
                // remove for non-windowed version */
                }
            }

        public void SaveDefs(bool forcelocal) {
            string fnm = defsLoc;
            if (forcelocal) fnm = defsFnm;
            try {
                StreamWriter w = new StreamWriter(fnm);
                w.WriteLine(cmd);
                w.WriteLine(cmdArgs);
                w.WriteLine(progPunct);
                w.WriteLine(senseCase);
                w.WriteLine(encoding.EncodingName);
                //win remove for non-windowed version
                w.WriteLine(fontNum);
                for (int i = 0; i < fontCnt; i++) {
                    w.WriteLine(fontNm[i]);
                    w.WriteLine((int)fontStyle[i]);
                    w.WriteLine(fontSz[i]);
                    }
                w.WriteLine(wrap);
                w.WriteLine(autoIndent);
                w.WriteLine(programming);
                w.WriteLine(spInTab);
                w.WriteLine(expTabs);
                w.WriteLine(eventSet);
                w.WriteLine(top);
                w.WriteLine(left);
                w.WriteLine(width);
                w.WriteLine(height);
                // remove for non-windowed version */
                w.Close();
                }
            catch (Exception e) {
                throw new ssException("saving defaults:\r\n" + e.Message);
                }

            }

        }
    }
