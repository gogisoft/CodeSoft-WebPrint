using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using System.Threading;
using System.Text.RegularExpressions;

namespace CodeSoftPrinterApp.Common
{
    public class SessionManager
    {
        private static List<CSDirectory> _Directories = null;
        private static List<CSLabel> _Labels = null;
        private static List<CSPrinter> _Printers = null;
        private static List<CSDocument> _Documents = null;
        private static List<SessionStore> _Sessions = new List<SessionStore>();
        public static void APIEvent(object sender, CSNetAppEventArgs args)
        {
            if (args.AppFail)
            {
                Dispose();
            }
        }
        public static List<CSDirectory> Directories
        {
            get
            {
                if (_Directories == null)
                {
                    _Directories = new List<CSDirectory>();
                }
                return _Directories;
            }
        }
        public static List<CSLabel> Labels
        {
            get
            {
                if (_Labels == null)
                {
                    _Labels = new List<CSLabel>();
                }
                return _Labels;
            }
        }
        public static List<CSPrinter> Printers
        {
            get
            {
                if (_Printers == null)
                {
                    _Printers = new List<CSPrinter>();
                }
                return _Printers;
            }
        }
        public static List<CSDocument> Documents
        {
            get
            {
                if (_Documents == null)
                {
                    _Documents = new List<CSDocument>();
                }
                return _Documents;
            }
        }

        private static string GetCSSessionID()
        {
            if (HttpContext.Current.Session["__CSSessionID"] == null)
            {
                return string.Empty;
            }
            else
            {
                return HttpContext.Current.Session["__CSSessionID"].ToString();
            }
        }
        private static void LoadHandlers()
        {
            // CSNetApp controls the API and is not coupled with other objects. 
            // The APIEvent is designed to give the CSNetApp class states.
            CSNetApp.Instance.APIEvent -= APIEvent;
            CSNetApp.Instance.APIEvent += APIEvent;
        }
        /// <summary>
        /// Method return a new session
        /// </summary>
        /// <returns></returns>
        public static SessionStore GetSession()
        {
            LoadHandlers();
            string id = GetCSSessionID();
            SessionStore sess = null;
            if (!string.IsNullOrEmpty(id))
            {
                sess = _Sessions.Where(s => s.ID == id).FirstOrDefault();
            }
            else
            {
                sess = new SessionStore();
                _Sessions.Add(sess);
                HttpContext.Current.Session["__CSSessionID"] = sess.ID;
            }
            //ValidateSessionsASync();
            if (sess == null)
            {
                throw new UserError("Session has expried!");
            }
            return sess;
        }
        public static void ClearSession()
        {
            string id = GetCSSessionID();
            SessionStore sess = null;
            if (!string.IsNullOrEmpty(id))
            {
                HttpContext.Current.Session["__CSSessionID"] = null;
                sess = _Sessions.Where(s => s.ID == id).FirstOrDefault();
                sess.Dispose();
                _Sessions.Remove(sess);
            }
        }
        /// <summary>
        /// Method to local specific session. Used by web service.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static SessionStore GetSession(string id)
        {
            SessionStore sess = null;
            if (!string.IsNullOrEmpty(id))
            {
                sess = _Sessions.Where(s => s.ID == id).FirstOrDefault();
            }
            return sess;
        }
        public static void ClearVariables(){
            SessionStore store = GetSession();
            store.SetVariables(new List<CodeSoftDTO.Variable>());
        }

        /// <summary>
        /// The Print app is a web based label printing program that allows a user to print from a large list of labels.
        /// The map used is a large poly zone so it should accept most variable inputs.
        /// Input Folder: \\netapp\labels\Sentinel\Print_app\INPUT 
        /// Output Folder:  \\netapp\labels\Sentinel\Print_app\OUTPUT 
	    /// Label files will be retained for 7 days.
        /// File format:  The data files MUST start with “PA”.  (PA*.*).  It is strongly recommended that you make each file name unique.  i.e. add date and time to the filename.  This will prevent the files from being overwritten in the output folder and will help with troubleshooting.
        /// </summary>
        /// <param name="printer"></param>
        /// <param name="label"></param>
        public static void PrintLabel(string printer, CSLabel label, Int32 quantity)
        {
            bool hasSerialNumberRange = label.CSVariables.Exists(p => p.Value.Contains(Constants.Delimiter_SequencedNumberRange));
            string sentinelPath = @"\\netapp\labels\Sentinel\Print_app\INPUT";
            string sentinelPrefix = "PA";
            string prnstring = "";
            if (quantity > 1 || hasSerialNumberRange)
            {
                //for (int i = 0; i < quantity; i++)
                //{
                    if (hasSerialNumberRange)
                    {
                        CodeSoftDTO.Variable rangeVariable = label.CSVariables.Where(p => p.Value.Contains(Constants.Delimiter_SequencedNumberRange)).First();
                        string originalValue = rangeVariable.Value;
                        int delimiterIndex = rangeVariable.Value.IndexOf(Constants.Delimiter_SequencedNumberRange);
                        int delimiterLength = Constants.Delimiter_SequencedNumberRange.Length;
                        string startString = rangeVariable.Value.Substring(0, delimiterIndex);
                        string endString = rangeVariable.Value.Substring(delimiterIndex + delimiterLength);

                        //The sequence will be some specific place within the variable string.
                        //Let's look for a commong prefix and suffix if it exists.
                        //The remaining charcters will be the start and end range integers.
                        string commonPrefix = commonString(startString, endString);
                        string commonSuffix = commonReverseString(startString, endString);
                        if (!String.IsNullOrEmpty(commonPrefix))
                        {
                            startString = startString.Replace(commonPrefix, "");
                            endString = endString.Replace(commonPrefix, "");
                        }
                        if (!String.IsNullOrEmpty(commonSuffix))
                        {
                            startString = startString.Replace(commonSuffix, "");
                            endString = endString.Replace(commonSuffix, "");
                        }

                        //try to get the start and end integers.
                        int startValue = 0;
                        int endValue = 0;
                        if (!String.IsNullOrEmpty(startString))
                        {
                            int value;
                            if (Int32.TryParse(startString, out value))
                                startValue = value;
                            else
                                throw new UserError("The start range is not a number");
                        }
                        if (!String.IsNullOrEmpty(endString))
                        {
                            int value;
                            if (Int32.TryParse(endString, out value))
                                endValue = value;
                            else
                                throw new UserError("The end range is not a number");
                        }
                        if (startValue > endValue)
                        {
                            throw new UserError("The begin range is greater than the end");
                        }
                        if (startValue == endValue)
                        {
                            throw new UserError("The begin and end range are the same");
                        }

                        for (int j = startValue; j <= endValue; j++)
                        {
                            rangeVariable.Value = commonPrefix + j.ToString() + commonSuffix;
                            prnstring += GetPrintVariables(printer, label.Path, label.CSVariables, (quantity < 1));
                            if (quantity > 0)
                            {
                                prnstring += "@LABEL_QUANTITY=" + quantity.ToString() + System.Environment.NewLine;
                                prnstring += "END" + System.Environment.NewLine;
                            }
                        }
                        rangeVariable.Value = originalValue;
                    }
                    else
                    {
                        prnstring += GetPrintVariables(printer, label.Path, label.CSVariables, (quantity < 1) );
                        if (quantity > 0)
                        {
                            prnstring += "@LABEL_QUANTITY=" + quantity.ToString() + System.Environment.NewLine;
                            prnstring += "END" + System.Environment.NewLine;
                        }
                    }
                //}
            }
            else
            {
                prnstring = GetPrintVariables(printer, label.Path, label.CSVariables, false);
            }

            string file = sentinelPath + @"\" + sentinelPrefix + "_" + label.Name.Replace(".lab", "").Replace(".LAB", "") + "_" + DateTime.Now.ToString("MMddyyfff") + ".txt";
            FileStream fs = new FileStream(file, FileMode.CreateNew, FileAccess.Write);
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(prnstring);
            fs.Write(byteArray, 0, byteArray.Length);
            fs.Close();

            //Test method. Remove when Milton is done with it. 20180502:GD
            if (printer.ToLower() == "uid4")
                ForkPrintToMTSB(label, quantity);
        }
        /// <summary>
        /// Debug method for Milton to test where printing to UID4 is failing.20180502:GD
        /// Remove at some point.
        /// </summary>
        /// <param name="label"></param>
        /// <param name="quantity"></param>
        private static void ForkPrintToMTSB(CSLabel label, Int32 quantity)
        {
            (new Thread(() =>
            {
                PrintLabel("MTSB", label, quantity);
            })).Start();

        }
        private static string commonString(string s1, string s2)
        {

            List<string> result = new List<string>();
            int len = s1.Length;
            char[] match_chars = new char[len];
            for (var i = 0; i < len; ++i)
            {
                if (Char.ToLower(s1[i]) != Char.ToLower(s2[i]))
                    i = len;
                else
                    result.Add(s1[i].ToString());
            }
            return String.Join(String.Empty, result.ToArray());
        }
        private static string commonReverseString(string s1, string s2)
        {
            List<string> result = new List<string>();
            int len = s1.Length;
            if (s2.Length < s1.Length)
                len = s2.Length;
            char[] match_chars = new char[len];
            for (var i = 0; i < len; ++i)
            {
                if (Char.ToLower(s1[s1.Length - 1 - i]) != Char.ToLower(s2[s2.Length - 1 - i]))
                    i = len;
                else
                    result.Add(s1[s1.Length - 1 - i].ToString());
            }

            return String.Join(String.Empty, result.ToArray().Reverse());
        }
        /// <summary>
        /// Method to print specified variable collection
        /// </summary>
        /// <param name="printer"></param>
        /// <param name="label"></param>
        /// <param name="variables"></param>
        private static void PrintVariables(string printer,string label, List<CodeSoftDTO.Variable> variables)
        {
            string sentinelPath = @"\\netapp\labels\Sentinel\Print_app\INPUT";
            string sentinelPrefix = "PA";
            string prnstring = @"LABELNAME = " + label + System.Environment.NewLine;
            prnstring = prnstring + "PRINTER = " + printer + System.Environment.NewLine;
            foreach (CodeSoftDTO.Variable variable in variables)
            {
                prnstring = prnstring + variable.Name + " = " + variable.Value + System.Environment.NewLine;
            }
            string file = sentinelPath + @"\" + sentinelPrefix + "_" + label.Replace(".lab", "").Replace(".LAB", "") + "_" + DateTime.Now.ToString("MMddyyfff") + ".txt";
            FileStream fs = new FileStream(file, FileMode.CreateNew, FileAccess.Write);
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(prnstring);
            fs.Write(byteArray, 0, byteArray.Length);
            fs.Close();
        }
        /// <summary>
        /// Method to permutate collections of variables.
        /// </summary>
        /// <param name="printer"></param>
        /// <param name="label"></param>
        /// <param name="variables"></param>
        /// <param name="permutationVar"></param>
        private static void PrintVariables(string printer, string label, List<CodeSoftDTO.Variable> variables, CodeSoftDTO.Variable permutationVar)
        {
            string sentinelPath = @"\\netapp\labels\Sentinel\Print_app\INPUT";
            string sentinelPrefix = "PA";
            string prnstring = @"LABELNAME = " + label + System.Environment.NewLine;
            prnstring = prnstring + "PRINTER = " + printer + System.Environment.NewLine;
            foreach (CodeSoftDTO.Variable variable in variables)
            {
                prnstring = prnstring + variable.Name + " = " + variable.Value + System.Environment.NewLine;
                if (variable.Name.ToLower() == permutationVar.Value.ToLower())
                {
                    prnstring = prnstring + variable.Name + " = " + permutationVar.Value + System.Environment.NewLine;
                }
            }
            string file = sentinelPath + @"\" + sentinelPrefix + "_" + label.Replace(".lab", "").Replace(".LAB", "") + "_" + DateTime.Now.ToString("MMddyyfff") + ".txt";
            FileStream fs = new FileStream(file, FileMode.CreateNew, FileAccess.Write);
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(prnstring);
            fs.Write(byteArray, 0, byteArray.Length);
            fs.Close();
        }
        private static string GetPrintVariables(string printer, string label, List<CodeSoftDTO.Variable> variables, bool appendEnd)
        {
            string prnstring = @"LABELNAME = " + label + System.Environment.NewLine;
            prnstring = prnstring + "PRINTER = " + printer + System.Environment.NewLine;
            foreach (CodeSoftDTO.Variable variable in variables)
            {
                prnstring = prnstring + variable.Name + " = " + variable.Value + System.Environment.NewLine;
            }
            if (appendEnd)
            {
                prnstring = prnstring + "END" + System.Environment.NewLine + System.Environment.NewLine;
            }
            return prnstring;
        }
        private static string GetPrintVariables(string printer, string label, List<CodeSoftDTO.Variable> variables, CodeSoftDTO.Variable permutationVar, bool appendEnd)
        {
            string prnstring = @"LABELNAME = " + label + System.Environment.NewLine;
            prnstring = prnstring + "PRINTER = " + printer + System.Environment.NewLine;
            foreach (CodeSoftDTO.Variable variable in variables)
            {
                prnstring = prnstring + variable.Name + " = " + variable.Value + System.Environment.NewLine;
                if (variable.Name.ToLower() == permutationVar.Value.ToLower())
                {
                    prnstring = prnstring + variable.Name + " = " + permutationVar.Value + System.Environment.NewLine;
                }
            }
            if (appendEnd)
            {
                prnstring = prnstring + "END" + System.Environment.NewLine + System.Environment.NewLine;
            }
            return prnstring;
        }
        private static void ValidateSessionsASync()
        {
            var thread = new Thread((arg) =>
            {
                for (int i = 0; i < _Sessions.Count; i++) //{ 
                //foreach (SessionStore sess in _Sessions.Where(s => (DateTime.Now - s.CreatedTime).TotalMinutes > 20))
                {
                    SessionStore sess = _Sessions[i];
                    if ((DateTime.Now - sess.CreatedTime).Seconds >  30)
                    {
                        //if (sess.GetCurrentDocument() != null)
                         //   sess.GetCurrentDocument().Close();

                        _Sessions.Remove(sess);
                    }
                }
            });
            thread.Start();
        }
        private static void Dispose()
        {
            HttpContext.Current.Session["__CSSessionID"] = null;
            if (_Directories != null)
                _Directories.Clear();
            if (_Labels != null)
                _Labels.Clear();
            //if (_Sessions != null)
            //_Sessions.Clear();
        }
    }
}