using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CodeSoftPrinterApp.Admin
{
    public partial class Default : System.Web.UI.Page
    {
        private string DefaultLocation = "Mfg";
        protected void Page_Load(object sender, EventArgs e)
        {
            Message.Text = String.Empty;
            FailMessage.Text = String.Empty;
            if (!IsPostBack)
            {
                LoadPrinters();
                LoadPrinterDropDown();
                NewPrinter.Visible = false;
            }
        }
        private void LoadPrinters()
        {
            DataTable table = DAL.DataAccess.GetPrinters(searchTxt.Text, printerTxt.Text);
            GridView1.DataSource = table;
            GridView1.DataBind();
        }
        private void LoadPrinterDropDown()
        {
            DataTable table = DAL.DataAccess.GetPrinterSelection();
            locationDD.DataSource = table;
            locationDD.DataTextField = "Location";
            locationDD.DataValueField = "Location";
            locationDD.DataBind();
            locationDD.Items.FindByText(DefaultLocation).Selected = true;
        }

        protected void GridView1_RowEditing(object sender, GridViewEditEventArgs e)
        {
            GridView1.EditIndex = e.NewEditIndex;
            if (GridViewSortDirection == SortDirection.Ascending)
            {
                SortGridView(SortExpression, "asc");
            }
            else
            {
                SortGridView(SortExpression, "desc");
            }
        }

        protected void GridView1_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            Label IDLbl = GridView1.Rows[e.RowIndex].FindControl("ID_Lbl") as Label;
            int ID = int.Parse(IDLbl.Text);
            if (DAL.DataAccess.DeleteLabel(ID))
            {
                Message.Text = "Delete success...";
                LoadPrinters();
            }
            else
            {
                FailMessage.Visible = false;
                FailMessage.Text = "Failed to delete record. See logs....";
            }
        }

        protected void btnAddRow_Click(object sender, EventArgs e)
        {
            FailMessage.Text = "";
            Message.Text = "";
            NewPrinter.Visible = true;
            Save_btn.Visible = true;
            btnCancelAddRow.Visible = true;
            btnAddRow.Visible = false;

        }
        protected void btnCancelAddRow_Click(object sender, EventArgs e)
        {
            FailMessage.Text = "";
            Message.Text = "";
            NewPrinter.Visible = false;
            btnCancelAddRow.Visible = false;
            Save_btn.Visible = false;
            btnAddRow.Visible = true;
        }
        protected void Save_btn_Click(object sender, EventArgs e)
        {
            FailMessage.Text = "";
            Message.Text = "";
            string nameplate = NamePlate_Txt.Text;
            string manfg = Manufacturer_txt.Text;
            string location = locationDD.SelectedValue;
            if (string.IsNullOrEmpty(nameplate))
            {
                FailMessage.Visible = true;
                FailMessage.Text = "Nameplate is required";
                return;
            }
            if (string.IsNullOrEmpty(manfg))
            {
                FailMessage.Visible = true;
                FailMessage.Text = "Manufacturer is required";
                return;
            }
            if (string.IsNullOrEmpty(location))
            {
                FailMessage.Visible = true;
                FailMessage.Text = "Location is required";
                return;
            }

            if (DAL.DataAccess.InsertLabel(nameplate, manfg, location))
            {
                Message.Text = "New record added...";
                LoadPrinters();
                btnCancelAddRow.Visible = false;
                Save_btn.Visible = false;
                NewPrinter.Visible = false;
                btnAddRow.Visible = true;
            }
            else
            {
                FailMessage.Visible = true;
                FailMessage.Text = "Failed to add printer. See logs.";
            }
        }

        protected void GridView1_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            //DataTable table = DAL.DataAccess.GetPrinters();
            //GridView1.DataSource = table;
            GridView1.PageIndex = e.NewPageIndex;
            //GridView1.DataBind();
            if (GridViewSortDirection == SortDirection.Ascending)
            {
                SortGridView(SortExpression, "asc");
            }
            else
            {
                SortGridView(SortExpression, "desc");
            }

        }

        protected void GridView1_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            GridViewRow row = (GridViewRow)GridView1.Rows[e.RowIndex];
            //int ID = int.Parse(GridView1.Rows[e.NewEditIndex].FindControl("ID").ToString());
            Label IDLbl = GridView1.Rows[e.RowIndex].FindControl("ID_Lbl") as Label;
            TextBox nameplateTxt = GridView1.Rows[e.RowIndex].FindControl("Nameplate_Txt") as TextBox;
            TextBox printerTxt = GridView1.Rows[e.RowIndex].FindControl("PrinterMfg_Txt") as TextBox;
            DropDownList locationDD = GridView1.Rows[e.RowIndex].FindControl("Location_DD") as DropDownList;
            int id = int.Parse(IDLbl.Text);
            if(!String.IsNullOrEmpty(nameplateTxt.Text) && !String.IsNullOrEmpty(printerTxt.Text)){
                if (DAL.DataAccess.EditLabel(id, nameplateTxt.Text, printerTxt.Text, locationDD.SelectedValue))
                {
                    Message.Text = "Change successful...";
                    if (GridViewSortDirection == SortDirection.Ascending)
                    {
                        SortGridView(SortExpression, "asc");
                    }
                    else
                    {
                        SortGridView(SortExpression, "desc");
                    }
                }
                else
                {
                    FailMessage.Visible = true;
                    FailMessage.Text = "Failed to edit record.";
                }
            }
            else
            {
                FailMessage.Visible = true;
                FailMessage.Text = "No empty data allowed";
            }
        }

        protected void GridView1_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            GridView1.EditIndex = -1;
            //LoadPrinters();
            if (GridViewSortDirection == SortDirection.Ascending)
            {
                SortGridView(SortExpression, "asc");
            }
            else
            {
                SortGridView(SortExpression, "desc");
            }
        }

        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                if ((e.Row.RowState & DataControlRowState.Edit) > 0)
                {
                    DataTable table = DAL.DataAccess.GetPrinterSelection();
                    DropDownList ddlActive = (DropDownList)e.Row.FindControl("Location_DD");
                    ddlActive.Items.Clear();
                    ddlActive.DataSource = table;
                    ddlActive.DataTextField = "Location";
                    ddlActive.DataValueField = "Location";
                    ddlActive.DataBind();

                    DataRowView dr = e.Row.DataItem as DataRowView;
                    string selectedItem = dr["Location"].ToString();
                    if (String.IsNullOrEmpty(selectedItem))
                    {
                        selectedItem = DefaultLocation;
                    }
                    ddlActive.Items.FindByText(selectedItem).Selected = true;
                }
            }
        }

        protected void GridView1_Sorting(object sender, GridViewSortEventArgs e)
        {
            string sortExpression = e.SortExpression;
            ViewState["z_sortexpresion"] = e.SortExpression;
            if (GridViewSortDirection == SortDirection.Ascending)
            {
                GridViewSortDirection = SortDirection.Descending;
                SortGridView(sortExpression, "DESC");
            }
            else
            {
                GridViewSortDirection = SortDirection.Ascending;
                SortGridView(sortExpression, "ASC");
            }
        }
        private void SortGridView(string sortExpression, string direction)
        {
            DataTable dt = DAL.DataAccess.GetPrinters(searchTxt.Text, printerTxt.Text);
            DataView dv = new DataView(dt);
            dv.Sort = sortExpression + " " + direction;
            this.GridView1.DataSource = dv;
            this.GridView1.DataBind();
        }
        public SortDirection GridViewSortDirection
        {
            get
            {
                if (ViewState["sortDirection"] == null)
                    ViewState["sortDirection"] = SortDirection.Ascending;
                return (SortDirection)ViewState["sortDirection"];
            }
            set
            {
                ViewState["sortDirection"] = value;
            }
        }
        public string SortExpression
        {
            get
            {
                if (ViewState["z_sortexpresion"] == null)
                    ViewState["z_sortexpresion"] = "ID";
                return ViewState["z_sortexpresion"].ToString();
            }
            set
            {
                ViewState["z_sortexpresion"] = value;
            }
        }

        protected void searchButton_Click(object sender, EventArgs e)
        {
            string search = searchTxt.Text;
            DataTable table = DAL.DataAccess.GetPrinters(searchTxt.Text,printerTxt.Text);
            GridView1.DataSource = table;
            GridView1.DataBind();
        }

        protected void searchClearBtn_Click(object sender, EventArgs e)
        {
            searchTxt.Text = string.Empty;
            DataTable table = DAL.DataAccess.GetPrinters();
            GridView1.DataSource = table;
            GridView1.DataBind();
        }
    }
}