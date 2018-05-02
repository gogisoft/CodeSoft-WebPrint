using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using Tkx.Lppa;
using System.Web.UI.WebControls;
using System.Data;
using System.Collections;

namespace CodeSoftPrinterApp.Common
{
    public class SessionStore
    {
        private string _ID;
        private System.DateTime _CreatedTime;
        public System.DateTime CreatedTime { get { return this._CreatedTime; } }
        public string ID { get { return this._ID; } }
        private List<CodeSoftDTO.Variable> _Variables { get; set; }
        private CSDocument _CurrentDocument;
        private CSDirectory _CurrentDirectory { get; set; }
        private CSLabel _CurrentLabel { get; set; }
        //private CSNetApp NetApp { get; set; }
        public SessionStore()
        {
            _Variables = new List<CodeSoftDTO.Variable>();
            this._ID = (Guid.NewGuid()).ToString();
            _CreatedTime = System.DateTime.Now;
            //NetApp = CSNetApp.Instance;
        }
        public void Dispose()
        {
            if (_CurrentDocument != null)
            {
                try
                {
                    //_CurrentDocument.Close(false);
                    _CurrentDocument = null;
                    //log4net.LogManager.GetLogger("DEBUG").Error(new Exception("Document Dispose. Session ID:" + ID));
                }
                catch (Exception err)
                {
                    log4net.LogManager.GetLogger("DEBUG").Error(err);
                }
            }
            if (_CurrentLabel != null)
            {
                _CurrentLabel.CSVariables.Clear();
                _CurrentLabel = null;
            }
        }
        /// <summary>
        /// Sets an immutable reference to the current CSDirectory
        /// </summary>
        /// <param name="directory"></param>
        public void SetCurrentDirectory(CSDirectory directory)
        {
            //prevent mutation
            CSDirectory obj = new CSDirectory();
            obj.Name = directory.Name;
            obj.Path = directory.Path;
            obj.Labels.AddRange(directory.Labels);
            _CurrentDirectory = directory;
        }
        /// <summary>
        /// Returns a mutable reference to the current CSDirectory
        /// </summary>
        /// <returns></returns>
        public CSDirectory GetCurrentDirectory()
        {
            return _CurrentDirectory;
        } 
        public CSDocument GetCurrentDocument()
        {
            /*
            if (this._CurrentDocument != null)
                this._CurrentDocument.Close();
            
            if (this.CurrentLabel != null)
                this._CurrentDocument = CSNetApp.Instance.OpenDocument(this.CurrentLabel.Path);
            */

            return this._CurrentDocument;
        }
        public void CloseDocument()
        {
            //_CurrentDocument.Close(false);
            this._CurrentDocument = null;
        }
        /// <summary>
        /// Sets an immutable reference to the current CSLabel
        /// </summary>
        /// <param name="label"></param>
        public void SetLabel(CSLabel label)
        {
                if (label == null)
                {
                    Dispose();
                }
                else
                {
                    //prevent mutation
                    CSLabel obj = CSLabelManager.GetLabel(label.Name, label.Path);
                    obj.AddVariables(label.CSVariables);
                    this._CurrentLabel = obj;
                    //if (this._CurrentDocument != null)
                        //this._CurrentDocument.Close(false);
                    CSDocument Doc = SessionManager.Documents.Where(p => p.LabelPath == label.Path).FirstOrDefault();
                    this._CurrentDocument = Doc;
                    if (Doc == null)
                    {
                        Doc = CSNetApp.Instance.OpenDocument(obj.Path);
                        this._CurrentDocument = Doc;
                        SessionManager.Documents.Add(Doc);
                    }
                }
        }
        /// <summary>
        /// Returns a mutable reference to the current CSLabel
        /// </summary>
        /// <returns></returns>
        public CSLabel GetCurrentLabel()
        {
            return _CurrentLabel;
        }
        /// <summary>
        /// Sets an immutable reference to the current CodeSoftDTO.Variable collection
        /// </summary>
        /// <param name="variables"></param>
        public void SetVariables(List<CodeSoftDTO.Variable> variables)
        {
            //prevent mutation
            List<CodeSoftDTO.Variable> newList = new List<CodeSoftDTO.Variable>(variables);
            _CurrentLabel.AddVariables(variables);
            _Variables.Clear();
            _Variables.AddRange(newList);
        }
        /// <summary>
        /// Returns a mutable reference to the current CodeSoftDTO.Variable collection
        /// </summary>
        /// <returns></returns>
        public List<CodeSoftDTO.Variable> GetVariables()
        {
            return _Variables;
        }
    }
    public class CSPrinter
    {
        public bool IsLocal { get; set; }
        public bool IsDebug { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public CSPrinter()
        {
        }
        public CSPrinter(DataRow row)
        {
            this.Name = "";
            this.Label = "";
            if (!string.IsNullOrEmpty(row["Printer_mfg"].ToString()))
            {
                this.Name = row["Printer_mfg"].ToString();
            }
            if (!string.IsNullOrEmpty(row["Nameplate"].ToString()))
            {
                this.Label = row["Nameplate"].ToString();
            }
        }
    }
    /// <summary>
    ///  Class returns all new CSLabel instances.
    ///  If custom label logic is needed, add condition for label name and return new instance of specific derived CSLabel class.
    /// </summary>
    public class CSLabelManager
    {
        public static CSLabel GetLabel(string labelName, string labelPath)
        {
            CSLabel standardLabel = new CSLabel();
            standardLabel.Name = labelName;
            standardLabel.Path = labelPath;
            return standardLabel;
        }
    }
    /// <summary>
    /// Base class representing a label.
    /// If you need custom logic per label extend this class and override necessary methods.
    /// </summary>
    public class CSLabel
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public List<CodeSoftDTO.Variable> CSVariables { get; private set; }
        public CSLabel()
        {
            if (CSVariables == null)
                CSVariables = new List<CodeSoftDTO.Variable>();
        }
        /// <summary>
        /// Method add variable 
        /// </summary>
        /// <param name="csVar"></param>
        public virtual void AddVariable(CodeSoftDTO.Variable csVar)
        {
            if (!this.CSVariables.Exists(v => v.Name == csVar.Name) &&
                !csVar.Name.ToLower().Contains("d_"))
                this.CSVariables.Add(csVar);
        }
        /// <summary>
        /// Method adds collection copy
        /// </summary>
        /// <param name="csVars"></param>
        public virtual void AddVariables(List<CodeSoftDTO.Variable> csVars)
        {
            //prevent mutation
            List<CodeSoftDTO.Variable> newList = new List<CodeSoftDTO.Variable>(csVars.Where(p=> p.Name.ToLower().Contains("d_") == false));
            this.CSVariables.Clear();
            this.CSVariables.AddRange(newList);
        }
    }
    /// <summary>
    /// Class represents the CS lavel directory structure.
    /// </summary>
    public class CSDirectory : TreeNode
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public List<CSLabel> Labels { get; set; }
        public CSDirectory ParentDirectory { get; set; }
        public List<CSDirectory> SubDirectory { get; set; }
        /// <summary>
        /// Property indicating one or more sub directories has label files
        /// </summary>
        public bool HasLabels { get; set; }
        /// <summary>
        /// Property indicating directory holds physical label files
        /// </summary>
        public bool HasFilesInChild { get; set; }
    
        public CSDirectory()
        {
            Labels = new List<CSLabel>();
            SubDirectory = new List<CSDirectory>();
        }
        public CSDirectory(string name) : base(name) 
        { 
            Labels = new List<CSLabel>();
            SubDirectory = new List<CSDirectory>();
            this.Name = name; 
        }
        public CSDirectory(string text, string value, string imageURL) : base(text, value, imageURL) 
        { 
            Labels = new List<CSLabel>();
            SubDirectory = new List<CSDirectory>();
        }
    }
    public class CSDocument : IDisposable
    {

        private CSFormat _CSFormat = null;
        private CSVariables _CSVariables = null;
        public CSFormat CSFormat { get {return _CSFormat;} }
        public CSVariables CSVariables { get {return _CSVariables;} }
        public String FullName { get; private set; }
        public String LabelPath { get; private set; }
        public void Load(Document doc, string labelPath)
        {
            _CSVariables = new CSVariables();
            _CSFormat = new CSFormat();
            //Document doc = CSNetApp.Instance.OpenDocument(label);
            List<CSVariable> _Variables = new List<CSVariable>();
            foreach (Variable var in doc.Variables)
            {
                CSVariable csVar = new CSVariable();
                csVar.Name = var.Name;
                csVar.Value = var.Value;
                if (var.DataSource == DataSourceType.Form ||
                    var.DataSource == DataSourceType.Free)
                {
                    csVar.IsUserInput = true;
                }
                _CSVariables.Add(csVar);
            }
            _CSFormat.AutomaticMargins = doc.Format.AutomaticMargins;
            _CSFormat.AutoSize = doc.Format.AutoSize;
            _CSFormat.Background = doc.Format.Background;
            _CSFormat.ColumnCount = doc.Format.ColumnCount;
            _CSFormat.Corner = doc.Format.Corner;
            _CSFormat.HorizontalGap = doc.Format.HorizontalGap;
            _CSFormat.LabelHeight = doc.Format.LabelHeight;
            _CSFormat.LabelWidth = doc.Format.LabelWidth;
            _CSFormat.MarginLeft = doc.Format.MarginLeft;
            _CSFormat.MarginTop = doc.Format.MarginTop;
            _CSFormat.PageHeight = doc.Format.PageHeight;
            _CSFormat.PageWidth = doc.Format.PageWidth;
            _CSFormat.PaperName = doc.Format.PaperName;
            _CSFormat.Portrait = doc.Format.Portrait;
            _CSFormat.RowCount = doc.Format.RowCount;
            _CSFormat.StockName = doc.Format.StockName;
            _CSFormat.StockType = doc.Format.StockType;
            _CSFormat.SupportName = doc.Format.SupportName;
            _CSFormat.VerticalGap = doc.Format.VerticalGap;

            this.FullName = doc.FullName;
            this.LabelPath = labelPath;
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
    public class CSVariables : IEnumerable<CSVariable>, IEnumerable
    {
        private int _Count;
        private List<CSVariable> _elements;
        public int Count { get { return _Count; } }
        public CSVariable this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                {
                    throw new IndexOutOfRangeException("index");
                }
                return _elements[index];
            }
        }
        public CSVariable this[string name]
        {
            get
            {
                return _elements.Where(p=> p.Name == name).FirstOrDefault();
            }
        }

        public CSVariables()
        {
            _Count = 0;
            _elements = new List<CSVariable>();
        }
        public void Remove(string name)
        {
            var e = _elements.Where(p=> p.Name == name).FirstOrDefault();
            if (e != null)
                _elements.Remove(e);
            _Count = _elements.Count;
        }
        public void Add(CSVariable variable)
        {
            _elements.Add(variable);
            _Count = _elements.Count;
        }
        public IEnumerator<CSVariable> GetEnumerator()
        {
            return this._elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
    public class CSVariable
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public bool IsUserInput { get; set; }
    }
    public class CSFormat
    {
        public bool AutomaticMargins { get; set; }
        public bool AutoSize { get; set; }
        public string Background { get; set; }
        public int ColumnCount { get; set; }
        public int Corner { get; set; }
        public int HorizontalGap { get; set; }
        public int LabelHeight { get; set; }
        public int LabelWidth { get; set; }
        public int MarginLeft { get; set; }
        public int MarginTop { get; set; }
        public int PageHeight { get; set; }
        public int PageWidth { get; set; }
        public string PaperName { get; set; }
        public bool Portrait { get; set; }
        public int RowCount { get; set; }
        public string StockName { get; set; }
        public string StockType { get; set; }
        public string SupportName { get; set; }
        public int VerticalGap { get; set; }
    }
}