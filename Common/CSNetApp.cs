using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tkx.Lppa;

namespace CodeSoftPrinterApp.Common
{
    /// <summary>
    /// Class to manage CodeSoft label applications.
    /// The best performance seems to be a static instance with the sessions managing documents
    /// </summary>
    public class CSNetApp
    {
        public delegate void AppEvent(object sender, CSNetAppEventArgs args);
        private Tkx.Lppa.Application _NetApp = null;
        private Tkx.Lppa.Application _NetAppStatic = null;
        private static  CSNetApp _Instance = null;
        public event AppEvent APIEvent;
        public static CSNetApp Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new CSNetApp();
                }
                return _Instance;
            }
        }
        private Tkx.Lppa.Application CodeSoftPreViewApi
        {
            get
            {
                if (_NetAppStatic == null)
                {
                    try
                    {
                        log4net.LogManager.GetLogger("DEBUG").Error(new Exception("Loading CodeSoft API (Preview Instance)..... "));
                        _NetAppStatic = new Tkx.Lppa.Application();
                        log4net.LogManager.GetLogger("DEBUG").Error(new Exception("CodeSoft API (Preview Instance) ready.... "));
                    }
                    catch (Exception err)
                    {
                        log4net.LogManager.GetLogger("ERROR").Error(new Exception("CodeSoft API (Preview Instance) failed: " + err.Message));
                       APIEvent(this, new CSNetAppEventArgs() { AppFail = true, SessionId = "0" });
                       log4net.LogManager.GetLogger("ERROR").Error(new Exception("CodeSoft API (Preview Instance) shutdown...."));
                    }
                }
                return _NetAppStatic;
            }
        }
        private Tkx.Lppa.Application CodeSoftApi
        {
            get
            {
                if (_NetApp == null)
                {
                    try
                    {
                        log4net.LogManager.GetLogger("DEBUG").Error(new Exception("Loading CodeSoft API (Main Instance)..... "));
                        _NetApp = new Tkx.Lppa.Application();
                        log4net.LogManager.GetLogger("DEBUG").Error(new Exception("CodeSoft API (Main Instance) ready.... "));
                    }
                    catch (Exception err)
                    {
                        log4net.LogManager.GetLogger("ERROR").Error(new Exception("CodeSoft API (Main Instance) Failed: " + err.Message));
                        //The COM has crashed. Kill all processes and try to restart.
                        ClearInstances();
                        string sessionId = string.Empty;
                        if(SessionManager.GetSession() != null)
                        sessionId = SessionManager.GetSession().ID;
                        APIEvent(this, new CSNetAppEventArgs() { AppFail = true,SessionId = sessionId });
                        log4net.LogManager.GetLogger("ERROR").Error(new Exception("CodeSoft API (Main Instance) shutdown...."));
                        //Try one more time.
                        return _NetApp;
                    }
                }
                return _NetApp;
            }

        }
        private void ClearInstances()
        {
            //This needs special permissions to work with IIS_USR
            try
            {
                foreach (System.Diagnostics.Process myProc in System.Diagnostics.Process.GetProcesses())
                {
                    if (myProc.ProcessName.ToLower() == "lppa")
                    {
                        myProc.Kill();
                    }
                }
                _NetApp = null;
            }
            catch(Exception err) { 
                /*This is a system process and may have permission issues at times.*/
                log4net.LogManager.GetLogger("ERROR").Error(new Exception("Failed to kill process: " + err.Message));
            }
        }
        public void OpenAPIDocument(string label, out Document result, out Tkx.Lppa.Application NetApp)
        {
            result = null;
            NetApp = CodeSoftPreViewApi;
            try
            {
                result = NetApp.Documents.Open(label, true);
            }
            catch (Exception err)
            {

               log4net.LogManager.GetLogger("ERROR").Error(new Exception("Failed to open Document for label " + label + " : " + err.Message));
                /*Can one really error handle a  COM++ ?*/
            }
            //return result;
        }
        public CSDocument OpenDocument(string label)
        {
            CSDocument result = new CSDocument();
            //result.Load(label);
            try
            {
                Document apiDoc = CodeSoftApi.Documents.Open(label, true);
                result.Load(apiDoc, label);
                apiDoc.Close(false);
                apiDoc.Dispose();
            }
            catch(Exception err)
            {
                string sessionId = string.Empty;
                if (SessionManager.GetSession() != null)
                    sessionId = SessionManager.GetSession().ID;
                APIEvent(this, new CSNetAppEventArgs() { DocFail = true, SessionId = sessionId });
                _NetApp = null;
                log4net.LogManager.GetLogger("ERROR").Error(new Exception("Failed to open Document for label " + label + " : " + err.Message));
                /*Can one really error handle a  COM++ ?*/
            }

            return result;
        }
        public void Dispose()
        {
            // This API is a share resources. A number of users may be getting labels from the same instance.
            // All Document requests are closed by the caller, so we let the caller with the last Document kill the API process.
            try
            {
                if (CodeSoftApi != null)
                {
                    if (CodeSoftApi.Documents != null &&
                        CodeSoftApi.Documents.Count() == 0)
                    {
                        //CodeSoftApi.Documents.CloseAll(false);
                        _NetApp.Quit();
                        _NetApp = null;
                    }
                }
            }
            catch(Exception err) { 
                /* External process. Not much control over this */
                log4net.LogManager.GetLogger("ERROR").Error(new Exception("CodeSoft API dispose faile :" + err.Message));
            }
        }
    }
    public class CSNetAppEventArgs : EventArgs
    {
        public bool AppFail { get; set; }
        public bool DocFail { get; set; }
        public bool ClearCache { get; set; }
        public string SessionId { get; set; }
    }
}