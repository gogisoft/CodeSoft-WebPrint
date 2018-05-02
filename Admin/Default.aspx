<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="CodeSoftPrinterApp.Admin.Default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">

</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Printer Management</h2>
    <div style="float:left;min-width:400px;min-height:600px;">
        <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" AutoGenerateEditButton="True" OnRowDeleting="GridView1_RowDeleting" OnRowEditing="GridView1_RowEditing" AllowPaging="True" OnPageIndexChanging="GridView1_PageIndexChanging" PageSize="25" OnRowCancelingEdit="GridView1_RowCancelingEdit" OnRowUpdating="GridView1_RowUpdating" OnRowDataBound="GridView1_RowDataBound" AllowSorting="True" OnSorting="GridView1_Sorting">
            <Columns>
            <asp:TemplateField ShowHeader="False">
                <ItemTemplate>
                    <asp:LinkButton ID="Delete_btn" runat="server" CommandName="Delete" OnClientClick="return confirm('Are you sure you want to delete this entry?');">Delete </asp:LinkButton>             
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="ID" SortExpression="ID">
                <ItemTemplate>
                    <asp:Label ID="ID_Lbl" runat="server"
                        Text='<%# Bind("ID") %>'></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Label Name" SortExpression="Nameplate">
                <EditItemTemplate>
                    <asp:TextBox ID="Nameplate_Txt" runat="server"
                        Text='<%# Bind("Nameplate") %>' Width="200"></asp:TextBox>
                </EditItemTemplate>
                <ItemTemplate>
                    <asp:Label ID="Nameplate_Lbl" runat="server"
                        Text='<%# Bind("Nameplate") %>'></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Printer" SortExpression="Printer_mfg">
                <EditItemTemplate>
                    <asp:TextBox ID="PrinterMfg_Txt" runat="server"
                        Text='<%# Bind("Printer_mfg") %>' Width="50"></asp:TextBox>
                </EditItemTemplate>
                <ItemTemplate>
                    <asp:Label ID="PrinterMfg_Lbl" runat="server"
                        Text='<%# Bind("Printer_mfg") %>'></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Location" SortExpression="Location">
                <EditItemTemplate>
                    <asp:DropDownList ID="Location_DD"  AppendDataBoundItems="True" runat="server" ></asp:DropDownList>
                </EditItemTemplate>
                <ItemTemplate>
                    <asp:Label ID="Location_Lbl" runat="server"
                        Text='<%# Bind("Location") %>'></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
         <EmptyDataTemplate>No records Found</EmptyDataTemplate>
        </asp:GridView>
     </div>
        <div style="padding:5px;margin-bottom:5px; margin-left:55px;border:1px solid grey;float:left;width:350px;">
            <div style="font-weight:bold">Search </div><span>*complete or partial name</span>
            <hr />
            <div>
               <div >Label</div><div style="margin-left:5px;"><asp:TextBox ID="searchTxt" runat="server" Width="150px"></asp:TextBox></div>
            </div>
            <div>
                <div style="margin-right:10px;">Printer</div><div  style="margin-left:5px;"><asp:TextBox ID="printerTxt" runat="server" Width="150"></asp:TextBox></div>
            </div>
            <div>
                <asp:Button ID="searchButton" runat="server" Text="Search" OnClick="searchButton_Click" />
                <asp:LinkButton ID="searchClearBtn" runat="server" OnClick="searchClearBtn_Click">Clear</asp:LinkButton>
            </div>
        </div>
    <div style="margin-left:55px;border:1px solid grey;float:left;background-color:white;width:350px;">
        <div style="background-color:lightblue;">
            <asp:LinkButton ID="btnAddRow" runat="server" OnClick="btnAddRow_Click" Text="Add New Label" Font-Bold="True" />
            <asp:LinkButton ID="btnCancelAddRow" runat="server" OnClick="btnCancelAddRow_Click" Text="Cancel" Visible="False" Font-Bold="True" />
            <asp:LinkButton ID="Save_btn" runat="server" OnClick="Save_btn_Click" Visible="False" Font-Bold="True">Save</asp:LinkButton>
        </div>
        <div runat="server" id="NewPrinter" style="margin-top:10px;margin-bottom:25px;">
            <div id="nameplateContainer" style="margin-left:5px;">
                <asp:Label ID="Label1" runat="server" Text="Label Name" ></asp:Label>
                 <asp:TextBox ID="NamePlate_Txt" runat="server"></asp:TextBox>
            </div>
            <div id="manufacturerContainer" style="margin-left:5px;">
                <asp:Label ID="Label2" runat="server" Text="Printer"></asp:Label>
                <asp:TextBox ID="Manufacturer_txt" runat="server"></asp:TextBox>
            </div>
            <div id="locationContainer" style="margin-left:5px;">
                <asp:Label ID="Label3" runat="server" Text="Location"></asp:Label>
                <asp:DropDownList ID="locationDD" runat="server" Width="150" style="margin-top:25px;"></asp:DropDownList>
            </div>
        </div>
        <div style="margin-left:5px;color:red">*Changes will apply the following day.</div>
        <div style="margin-left:5px;color:red">*If you require a new location, contact IT.</div>
    </div>
    <div style="float:left;margin-left:50px;">
        <asp:Label ID="Message" runat="server" Font-Bold="True" Font-Size="Larger"></asp:Label>
        <asp:Label ID="FailMessage" runat="server" ForeColor="Red" Visible="False" Font-Bold="True" Font-Size="Larger"></asp:Label>
    </div>
</asp:Content>
 