using System;
using System.Data;
using System.Drawing;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Drawing.Printing;
using Tkx.Lppa;
using CodeSoftPrinterApp.Common;
using Newtonsoft.Json;
using log4net;
using log4net.Config;
using System.Text.RegularExpressions;

namespace CodeSoftPrinterApp
{
    public partial class _Default : Page
    {
        private string QParamDirectory = string.Empty;
        private string QParamLabel = string.Empty;
        private string QParamPrinter = string.Empty;

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            try
            {
                SessionStore csSession = SessionManager.GetSession();
                if (csSession != null &&
                    csSession.GetCurrentDocument() != null)
                {
                    //csSession.CurrentDocument.Close();
                }
            }
            catch (UserError uerr)
            {

            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {

            //string values = "[{\"name\":\"X_MODS~1\",\"value\":\"X\"},{\"name\":\"X_MODS~2\",\"value\":\"X\"},{\"name\":\"X_MODS~3\",\"value\":\"X\"},{\"name\":\"X_MODS~4\",\"value\":\"X\"}]";
            //List<HFValue> valuesJSON = JsonConvert.DeserializeObject<List<HFValue>>(values);
            try
            {
                if (IsPostBack == false)
                {
                    UpdatePrinterList(false);
                    PopulateTreeView();
                    DirectoriesTV.Nodes[0].Expand(); //expand the top node
                    if (Request.QueryString["dir"] != null)
                    {
                        this.QParamDirectory = Request.QueryString["dir"];
                    }
                    if (Request.QueryString["lbl"] != null)
                    {
                        this.QParamLabel = Request.QueryString["lbl"];
                    }
                    if (Request.QueryString["pr"] != null)
                    {
                        this.QParamPrinter = Request.QueryString["pr"];
                    }

                    SelectTreeNodeFromPath(this.QParamDirectory);
                    foreach (ListItem item in TemplatesLB.Items)
                    {
                        if (item.Value == this.QParamLabel)
                        {
                            item.Selected = true;
                        }
                    }
                    foreach (ListItem item in PrintersLB.Items)
                    {
                        if (item.Value == this.QParamPrinter)
                        {
                            item.Selected = true;
                        }
                    }
                }
                AddAutoScroll();
                LoadConstants();
            }
            catch (UserError uerr)
            {
                MessageLbl.Text = uerr.Message;
            }
        }
        private void AddAutoScroll()
        {
            string treeviewid = DirectoriesTV.ClientID;
            string selectedNodeId = treeviewid + "n" + DirectoriesTV.Nodes.IndexOf(DirectoriesTV.SelectedNode) + "Nodes";
            string s = "var parentDiv = $('#" + treeviewid + "');";
            s += "var innerListItem = $('#" + selectedNodeId + "');";
            s += "parentDiv.scrollTop(parentDiv.scrollTop() + (innerListItem.position.top - parentDiv.position.top) - (parentDiv.height()/2) + (innerListItem.height()/2) );";

            Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "AddAutoScroll", s, true);

        }

        public void SelectTreeNodeFromPath(string path)
        {
            // set up some delimters to split our path on.
            char[] delimiters = new char[] { '/' };
            // split the array and store the values inside a string array.
            string[] pathArray = path.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            // clean up this array.
            //ensurePathArrayAccuracy(ref pathArray);
            // a simple celing variable.
            int numberOfLvlsToProbe = pathArray.Length;
            // a variable for to keep an eye on the level of the TreeNode.
            int currentLevel = 0;
            // this collection always get re-populated each iteration.
            TreeNodeCollection globalTreeNCollection = DirectoriesTV.Nodes;

            do
            {
                // start iterating through the global tree node collection.
                foreach (TreeNode rootNode in globalTreeNCollection)
                {
                    // only set the level if the node level is less!
                    if (rootNode.Depth < pathArray.Length)
                    {
                        currentLevel = rootNode.Depth;

                        // the 'currentLevel' variable can also be used to help index the 'pathArray' to make comparisons straightforward with current node.
                        if (rootNode.Text == pathArray[currentLevel])
                        {
                            // update our control variables and start again, the next level down.
                            globalTreeNCollection = rootNode.ChildNodes;
                            // once we have found the node then ...
                            break;
                        }
                    }
                    else // this portion of code means we are at the end of the 'pathArray.'
                    {
                        rootNode.Select();
                        //treeDrives.SelectedNode.EnsureVisible();

                        // to make sure the loop ends ... we need to update the currentLevel counter
                        // to allow the loop to end.
                        currentLevel = numberOfLvlsToProbe;
                        break;
                    }
                }
            }
            // if the 'currentLevel' is less than the 'numberOfLvlsToProbe' then we need
            // to keep on looping till we get to the end.
            while (currentLevel < numberOfLvlsToProbe);
        }
    
        private void SetDirectory()
        {
            try
            {
                SessionStore csSession = SessionManager.GetSession();

                CSDirectory csobj = csSession.GetCurrentDirectory();    // Directories.Where(d => d.Name == name).FirstOrDefault();
                if (csobj != null)
                {
                    //log4net.LogManager.GetLogger("DEBUG").Error(new Exception("Existing Current Directory"));
                    if (Directory.Exists(csobj.Path))
                    {
                        //log4net.LogManager.GetLogger("DEBUG").Error(new Exception("Found Directory Path"));
                        if (csobj.HasFilesInChild == true && csobj.Labels.Count == 0)
                        {
                            //log4net.LogManager.GetLogger("DEBUG").Error(new Exception("Found Labels"));
                            TemplatesLB.Items.Clear();
                            string[] fileEntries = Directory.GetFiles(csobj.Path, "*.lab", SearchOption.AllDirectories);
                            foreach (string fileName in fileEntries)
                            {
                                //log4net.LogManager.GetLogger("DEBUG").Error(new Exception("Found File: " + fileName));
                                CSLabel csobjl = new CSLabel();
                                csobjl.Name = fileName.Substring(fileName.LastIndexOf("\\") + 1, fileName.Length - fileName.LastIndexOf("\\") - 1);
                                csobjl.Path = fileName;

                                if (!csobj.Labels.Exists(l => l.Name.ToLower() == csobjl.Name.ToLower()))
                                {
                                    csobj.Labels.Add(csobjl);
                                }

                                if (!SessionManager.Labels.Exists(l => l.Name.ToLower() == csobjl.Name.ToLower()))
                                {
                                    SessionManager.Labels.Add(csobjl);
                                }
                                //SessionManager.Labels.Add(csobjl);
                            }
                        }
                        if (csobj.HasFilesInChild == true)
                        {
                            TemplatesLB.DataSource = csobj.Labels.OrderBy(o => o.Name);
                            TemplatesLB.DataTextField = "Name";
                            TemplatesLB.DataValueField = "Path";
                            TemplatesLB.DataBind();
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                SessionManager.ClearSession();
                log4net.LogManager.GetLogger("ERROR").Error(ex);
                //MessageLbl.Text = "An internal server error has occurred. Check the server logs for details.";
                //Response.Redirect("./");
            }
        }
        private void PopulateTreeView()
        {
            try
            {

                SessionManager.Directories.Clear();

                //DirectoriesTV.Nodes.Add(DirectoryToTreeView(null, @"\\netapp\labels"));
                //return;
                CSDirectory rootNode;
                DirectoryInfo info = new DirectoryInfo(@"\\netapp\labels");
                if (info.Exists &&
                    !info.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    rootNode = new CSDirectory(info.Name);
                    rootNode.Value = info.FullName;
                    rootNode.Path = info.FullName;
                    GetDirectories(info.GetDirectories().OrderBy(p=> p.Name).ToArray(), rootNode);

                    if (!SessionManager.Directories.Exists(d => d.Path == rootNode.Path))
                    {
                        SessionManager.Directories.Add(rootNode);
                        DirectoriesTV.Nodes.Add(rootNode);
                    }
                    DirectoriesTV.CollapseAll();
                }
            }
            catch (Exception ex)
            {
                SessionManager.ClearSession();
                log4net.LogManager.GetLogger("ERROR").Error(ex);
                //MessageLbl.Text = "An internal server error has occurred. Check the server logs for details.";
                Response.Redirect("./");
            }
        }
        private CSDirectory DirectoryToTreeView(CSDirectory parentNode, string path,
                                     string extension = ".lab")
        {
            var result = new CSDirectory(parentNode == null ? path : Path.GetFileName(path));
            foreach (var dir in Directory.GetDirectories(path))
            {
                CSDirectory node = DirectoryToTreeView(result, dir);
                if (node.ChildNodes.Count > 0)
                {
                    result.ChildNodes.Add(node);
                }
            }
            return result;
        }
        private void GetDirectories(DirectoryInfo[] subDirs, CSDirectory nodeToAddTo)
        {
            try
            {
                CSDirectory subNode;
                DirectoryInfo[] subSubDirs;
                foreach (DirectoryInfo subDir in subDirs)
                {
                    //The name Sentinel will not change ever per mike Harvey. It is the root directory of the Senitnel application, so filter it out. 20170412:GD
                    if (subDir.GetFiles().Where(f => f.Name.ToLower() == "csprintfilter.config").FirstOrDefault() == null)
                    {
                        if (!subDir.Attributes.HasFlag(FileAttributes.Hidden))
                        {
                            subNode = new CSDirectory(subDir.Name);
                            subNode.Value = subDir.FullName;
                            subNode.Path = subDir.FullName;
                            subSubDirs = subDir.GetDirectories();
                            if (subSubDirs.Length != 0)
                            {
                                GetDirectories(subSubDirs, subNode);
                            }
                            if (!SessionManager.Directories.Exists(d => d.Path == subNode.Path))
                            {
                                subNode.ParentDirectory = nodeToAddTo;
                                nodeToAddTo.SubDirectory.Add(subNode);
                                if (subDir.GetFiles().Where(f => f.Name.ToLower().Contains(".lab")).Count() > 0)
                                {
                                    subNode.HasFilesInChild = true;
                                    if (subNode.Parent != null)
                                    {
                                        CSDirectory parent = (CSDirectory)subNode.Parent;
                                        parent.HasLabels = true;
                                    }
                                }
                                SessionManager.Directories.Add(subNode);
                                nodeToAddTo.ChildNodes.Add(subNode);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SessionManager.ClearSession();
                log4net.LogManager.GetLogger("ERROR").Error(ex);
                //MessageLbl.Text = "An internal server error has occurred. Check the server logs for details.";
                Response.Redirect("./");
            }
        }
        private void LoadVariables()
        {
            VariablesLB.Items.Clear();
            SessionStore csSession = SessionManager.GetSession();
            CSLabel csobj = csSession.GetCurrentLabel();
            if (csobj != null)
            {
                if (csobj.CSVariables.Count == 0)
                {
                    using (var doc = csSession.GetCurrentDocument())
                    {
                        foreach (CSVariable aVariable in doc.CSVariables.OrderBy(p=> p.Name))
                        {
                            //CSVariable aVariable = doc.CSVariables[i];
                            CodeSoftDTO.Variable csVar = new CodeSoftDTO.Variable();

                            csVar.Name = aVariable.Name;
                            csVar.Value = ""; //aVariable.Value;
                            csVar.ArrayBaseShift = 0;
                            csVar.IsHidden = !aVariable.IsUserInput;

                            if (aVariable.Name.ToLower().Contains("x_"))
                            {
                                var regex = new Regex("[^a-zA-Z0-9]");
                                var regex2 = new Regex(@"\d+");
                                string root = aVariable.Name.Replace("X_", "").Replace("x_", "");
                                string index = regex2.Match(root).Value;
                                root = root.Remove(regex.Match(root).Index);
                                if (!root.ToLower().Contains("mod"))
                                {
                                    csVar.ArrayBaseShift = -1;
                                }
                                csVar.ArrayIndex = Int32.Parse(index);
                                csVar.GroupName = root;
                                csVar.IsXBox = true;
                                csVar.IsHidden = true;
                            }

                            csobj.AddVariable(csVar);
                        }
                        /*CodeSoftDTO.Variable csVarQty = new CodeSoftDTO.Variable();
                        csVarQty.Name = "LABEL_QUANTITY";
                        csVarQty.Value = "1";
                        csVarQty.ArrayBaseShift = 0;
                        csobj.AddVariable(csVarQty);*/
                    }
                }


                List<HFValue> HFValues = new List<HFValue>();
                foreach (CodeSoftDTO.Variable v in csobj.CSVariables.Where(v => string.IsNullOrEmpty(v.Name) == false))
                {

                    ListItem item = new ListItem(v.Name);
                    item.Attributes.Add("tag", v.Value);

                    if (v.IsXBox)
                    {
                        HFValues.Add(new HFValue() { Name = v.Name, Value = v.Value });
                    }
                    /*if (v.IsHidden)
                    {
                        item.Attributes.Add("style", "display:none;");
                        item.Attributes.Add("disabled", "true");
                    }*/
                    if (!v.IsHidden)
                    {
                        VariablesLB.Items.Add(item);
                    }
                }

                VariableValuesHf.Value = JsonConvert.SerializeObject(HFValues);
                //load the groups in the browser.
                string jscript = "";
                var groups = csobj.CSVariables.Where(l => l.GroupName != null).GroupBy(g => g.GroupName);
                foreach (var group in groups)
                {
                    CodeSoftDTO.VariableGroup vgroup = new CodeSoftDTO.VariableGroup();
                    vgroup.Name = group.Key;
                    jscript += "groups.push('" + vgroup.Name + "');";
                    vgroup.Variables.AddRange(csobj.CSVariables.Where(v => v.GroupName == vgroup.Name).Cast<CodeSoftDTO.Variable>());
                    foreach (CodeSoftDTO.Variable v in vgroup.Variables.OrderBy(o => o.ArrayIndex))
                    {
                        jscript += "groupItems.push({ group: \"" + vgroup.Name + "\", item: \"<div class='x_item' ><label for='" + v.Name + "' class='x_item_label' >" + (v.ArrayIndex + v.ArrayBaseShift) + "</label><input type='checkbox' id='" + v.Name + "' class='x_item_chk' /></div>\" });";
                    }

                }
                ClientScriptManager cm = ClientScript;
                cm.RegisterClientScriptBlock(GetType(), "LoadXGroupData", jscript, true);

                if (HFValues.Count > 0)
                {
                    cm = ClientScript;
                    jscript = "$(document).ready(function () {";
                    jscript += "$('#x_dialogIcon').show();";
                    jscript += "});";
                    cm.RegisterClientScriptBlock(GetType(), "HideXDialog", jscript, true);
                }
            }
        }
        private void UpdatePrinterList(bool forceUpdate)
        {
            try
            {
                CSPrinter csobj = null;
                if (forceUpdate)
                {
                    LocalPrintersLb.Items.Clear();
                    SessionManager.Printers.Clear();
                    DataTable table = DAL.DataAccess.GetPrinters();
                    if (table != null)
                    {
                        foreach (DataRow row in table.Rows)
                        {
                            csobj = new CSPrinter(row);
                            SessionManager.Printers.Add(csobj);
                            LocalPrintersLb.Items.Add(csobj.Name);
                        }
                    }
                    csobj = new CSPrinter();
                    csobj.IsDebug = true;
                    csobj.Label = "PDF";
                    csobj.Name = "PDF";
                    SessionManager.Printers.Add(csobj);
                }
                else
                {
                    if (SessionManager.Printers.Count == 0)
                    {
                        DataTable table = DAL.DataAccess.GetPrinters();
                        if (table != null)
                        {
                            foreach (DataRow row in table.Rows)
                            {
                                csobj = new CSPrinter(row);
                                SessionManager.Printers.Add(csobj);
                                LocalPrintersLb.Items.Add(csobj.Name);
                            }
                        }
                        csobj = new CSPrinter();
                        csobj.IsDebug = true;
                        csobj.Label = "PDF";
                        csobj.Name = "PDF";
                        SessionManager.Printers.Add(csobj);
                    }
                }
            }
            catch (Exception ex)
            {
                SessionManager.ClearSession();
                log4net.LogManager.GetLogger("ERROR").Error(ex);
                //MessageLbl.Text = "An internal server error has occurred. Check the server logs for details.";
                Response.Redirect("./");
            }
        }
        private void LoadConstants()
        {
            ClientScriptManager cm = ClientScript;
            String JS = "$(document).ready(function(){" +
                "delimiter_serialsequence = '" + Constants.Delimiter_SequencedNumberRange + "';" +
             "});";
            cm.RegisterClientScriptBlock(GetType(), "LoadConstants", JS, true);
        }
        private void LoadPreView()
        {
            SessionStore csSession = SessionManager.GetSession();
            CSLabel csobj = csSession.GetCurrentLabel();
            if (csobj != null)
            {
                CodeSoftDTO dto = new CodeSoftDTO();
                dto.SessionId = csSession.ID;
                dto.Label = TemplatesLB.SelectedValue;
                dto.Variables.AddRange(csobj.CSVariables);
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;
                settings.PreserveReferencesHandling = PreserveReferencesHandling.None;
                string jsonDTO = JsonConvert.SerializeObject(dto);
                ClientScriptManager cm = ClientScript;
                String JS = "$(document).ready(function(){" +
                "CodeSoftDTO=" + jsonDTO + ";" +
                "varTextBoxID='" + VariableValueTxt.ClientID + "';" +
                "varListID='" + VariablesLB.ClientID + "';" +
                 "});";
                cm.RegisterClientScriptBlock(GetType(), "LoadPreviewData", JS, true);

                JS = "$(document).ready(function(){" +
                "GetPreview();" +
                 "});";
                cm.RegisterClientScriptBlock(GetType(), "LoadPreView", JS, true);
            }
        }
        private void LoadLabelInfo()
        {
            SessionStore csSession = SessionManager.GetSession();
            CSLabel csobj = csSession.GetCurrentLabel();
            using (var doc = csSession.GetCurrentDocument())
            {
                CodeSoftDTO.LabelInfo dto = new CodeSoftDTO.LabelInfo();
                dto.LabelHeight = doc.CSFormat.LabelHeight.ToString();
                dto.LabelWidth = doc.CSFormat.LabelWidth.ToString();
                dto.PageHeight = doc.CSFormat.PageHeight.ToString();
                dto.PageWidth = doc.CSFormat.PageWidth.ToString();

                dto.Orientation = "Landscape";
                if (doc.CSFormat.Portrait)
                    dto.Orientation = "Portrait";

                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;
                settings.PreserveReferencesHandling = PreserveReferencesHandling.None;
                string jsonDTO = JsonConvert.SerializeObject(dto);
                ClientScriptManager cm = ClientScript;
                String JS = "$(document).ready(function(){" +
                    "LabelInfo =" + jsonDTO + ";" +
                    "SetLabelInfo();" +
                    "});";
                cm.RegisterClientScriptBlock(GetType(), "LoadLabelInfo", JS, true);
            }
        }

        protected void TemplatesLB_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                PrintMessageLbl.Text = string.Empty;
                VariableValueTxt.Text = string.Empty;
                SessionStore csSession = SessionManager.GetSession();

                csSession.SetLabel(SessionManager.Labels.Where(l => l.Name.ToLower() == TemplatesLB.SelectedItem.Text.ToLower()).First());

                PrintersLB.Items.Clear();
                if (SessionManager.Printers.Exists(p => TemplatesLB.SelectedItem.Text.ToLower().Trim().Replace(".lab", "") == p.Label.ToLower().Trim() || p.IsDebug))
                {
                    foreach (CSPrinter printer in SessionManager.Printers.Where(p => TemplatesLB.SelectedItem.Text.ToLower().Trim().Replace(".lab","") == p.Label.ToLower().Trim() || p.IsDebug).ToList())
                    {
                        PrintersLB.Items.Add(printer.Name);
                    }
                }
                LoadVariables(); 
                //LoadPreviewData();
                LoadPreView();
                LoadLabelInfo();
            }
            catch (UserError ex)
            {
                MessageLbl.Text = "Label selection error: " + ex.Message;
            }
            catch (Exception ex)
            {
                SessionManager.ClearSession();
                log4net.LogManager.GetLogger("ERROR").Error(ex);
                Response.Redirect("./");
            }

        }

        protected void PrintBtn_Click(object sender, EventArgs e)
        {
            try
            {
                SaveNoLoading();
                string printer = string.Empty;
                Int32 quantity = int.Parse(PrintQuantity.Text);
                if (PrintersLB.SelectedItem == null)
                {
                    PrintMessageLbl.Text = "Select a printer!";
                    LoadVariables();
                    LoadPreView();
                    return;
                }
                if (TemplatesLB.SelectedItem == null)
                {
                    PrintMessageLbl.Text = "Select a label!";
                    LoadPreView();
                    LoadLabelInfo();
                    return;
                }
                if (PrintersLB.SelectedItem != null)
                {
                    printer = PrintersLB.SelectedValue;
                }
                SessionStore csSession = SessionManager.GetSession();

                //Do validations.

                Validation.CheckNumberRange(csSession.GetCurrentLabel().CSVariables);

                //End validation.
                if (!PersistVariableValuesChk.Checked)
                {
                    SessionManager.PrintLabel(printer, csSession.GetCurrentLabel(), quantity);
                    csSession.SetVariables(new List<CodeSoftDTO.Variable>());
                    //csSession.SetLabel(null);
                    PrintMessageLbl.Text = "Sent to printer!";
                    //TemplatesLB.SelectedIndex = -1;
                    //PrintersLB.SelectedIndex = -1;
                    PrintQuantity.Text = "1";
                    VariableValueTxt.Text = "";
                    VariableValueTxt.Focus();
                    LoadVariables();
                }
                else
                {
                    SessionManager.PrintLabel(printer, csSession.GetCurrentLabel(), quantity);
                    PrintMessageLbl.Text = "Sent to printer!";
                    PrintQuantity.Text = "1";
                    VariableValueTxt.Text = "";
                    VariableValueTxt.Focus();
                    SaveAndLoad();
                }
            }
            catch (UserError ex)
            {
                PrintMessageLbl.Text = "<b>Input Error!</b><br /> " + ex.Message;
                SaveAndLoad();
            }
            catch (Exception ex)
            {
                SessionManager.ClearSession();
                log4net.LogManager.GetLogger("ERROR").Error(ex);
                PrintMessageLbl.Text = "<span style=\"color:red;\" ><b>Print Failed!</b><br /> " + ex.Message + "</span>";
            }

        }

        protected void PrintersLB_SelectedIndexChanged(object sender, EventArgs e)
        {
            PrintMessageLbl.Text = string.Empty;
            LocalPrintersLb.SelectedIndex = -1;
        }

        protected void LocalPrintersLb_SelectedIndexChanged(object sender, EventArgs e)
        {
            PrintMessageLbl.Text = string.Empty;
            PrintersLB.SelectedIndex = -1;
        }

        protected void SaveBtn_Click(object sender, EventArgs e)
        {
            try
            {
                //[{"name":"X_MODS~1","value":"X"},{"name":"X_MODS~2","value":"X"},{"name":"X_MODS~3","value":"X"},{"name":"X_MODS~4","value":"X"}]
                if (VariablesLB.SelectedItem != null)
                {
                    int selectedIndex = VariablesLB.SelectedIndex;
                    string variableName = VariablesLB.SelectedItem.Text;
                    string variableValue = VariableValueTxt.Text;
                    SessionStore csSession = SessionManager.GetSession();
                    CSLabel csobj = csSession.GetCurrentLabel();
                    CodeSoftDTO.Variable variable = csobj.CSVariables.Where(v => v.Name == variableName).First();
                    variable.Value = variableValue;

                    if (selectedIndex >= VariablesLB.Items.Count - 1)
                        selectedIndex = -1;

                    VariablesLB.SelectedIndex = selectedIndex + 1;
                    VariableValueTxt.Text = "";
                    VariableValueTxt.Focus();
                }
                SaveAndLoad();
            }
            catch (UserError ex)
            {
                MessageLbl.Text = "Save error: " + ex.Message;
            }
            catch (Exception ex)
            {
                SessionManager.ClearSession();
                log4net.LogManager.GetLogger("ERROR").Error(ex);
                Response.Redirect("./");
            }
        }
        private void SaveAndLoad()
        {
            SessionStore csSession = SessionManager.GetSession();
            CSLabel csobj = csSession.GetCurrentLabel();
            string values = VariableValuesHf.Value;
            List<HFValue> valuesJSON = JsonConvert.DeserializeObject<List<HFValue>>(values);
            foreach (HFValue hf in valuesJSON)
            {
                CodeSoftDTO.Variable variable = csobj.CSVariables.Where(v => v.Name == hf.Name).FirstOrDefault();
                if (variable != null)
                {
                    variable.Value = hf.Value;
                }
            }
            LoadVariables();
            LoadPreView();
            LoadLabelInfo();
        }
        private void SaveNoLoading()
        {
            SessionStore csSession = SessionManager.GetSession();
            CSLabel csobj = csSession.GetCurrentLabel();
            string values = VariableValuesHf.Value;
            List<HFValue> valuesJSON = JsonConvert.DeserializeObject<List<HFValue>>(values);
            foreach (HFValue hf in valuesJSON)
            {
                CodeSoftDTO.Variable variable = csobj.CSVariables.Where(v => v.Name == hf.Name).FirstOrDefault();
                if (variable != null)
                {
                    variable.Value = hf.Value;
                }
            }
        }
        protected void DirectoriesTV_SelectedNodeChanged(object sender, EventArgs e)
        {
            try
            {
                VariablesLB.Items.Clear();
                PrintersLB.Items.Clear();
                TemplatesLB.Items.Clear();
                SessionStore csSession = SessionManager.GetSession();
                csSession.SetCurrentDirectory(SessionManager.Directories.Where(d => d.Path.ToLower() == DirectoriesTV.SelectedValue.ToLower()).First());
                SetDirectory();
            }
            catch (UserError ex)
            {
                MessageLbl.Text = "Directory selection error: " + ex.Message;
            }
            catch (Exception ex)
            {
                SessionManager.ClearSession();
                log4net.LogManager.GetLogger("ERROR").Error(ex);
                Response.Redirect("./");
            }
        }

    }

    public class HFValue
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}