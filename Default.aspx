<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="CodeSoftPrinterApp._Default" EnableEventValidation="False" %>

<%@ Register Assembly="CodeSoftPrinterApp" Namespace="CodeSoftPrinterApp.Controls" TagPrefix="ctrl" %>

<asp:Content runat="server" ID="FeaturedContent" ContentPlaceHolderID="FeaturedContent">
</asp:Content>
<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <asp:HiddenField ID="VariableValuesHf" runat="server" />
                        <asp:Label runat="server" ID="PrintMessageLbl"></asp:Label>
    <div style="width:100%;height:100%;border:0px solid gray;">
        <div style="float:left;width:270px;height:800px;border:0px solid gray;">
            <div id="directoryelector" style="float:left;height:250px;">
                <h3 style="text-align:center;background-color:#0094ff;color:white;border:1px solid gray;">Directory</h3>
                    <asp:TreeView ID="DirectoriesTV" runat="server"  style="overflow:auto" Height="750px" Width="270px" OnSelectedNodeChanged="DirectoriesTV_SelectedNodeChanged" BackColor="White" BorderColor="#333333" BorderStyle="Solid" BorderWidth="1px" SkipLinkText="" NodeIndent="5" CssClass="directorylist">
                        <NodeStyle Font-Size="Small" />
                        <SelectedNodeStyle BackColor="#66CCFF" BorderColor="Gray" BorderStyle="Solid" BorderWidth="1px" />
                    </asp:TreeView>
            </div>
        </div>
        <div style="float:left;width:650px;height:250px;border:0px solid gray;">
            <div id="templatelist" style="float:left;margin-left:10px;width:200px;height:250px;">
            <h3 style="width:199px;text-align:center;background-color:#0094ff;color:white;border:1px solid gray;">Template</h3>
                <asp:ListBox ID="TemplatesLB" runat="server" AutoPostBack="True" OnSelectedIndexChanged="TemplatesLB_SelectedIndexChanged" Height="200px" Width="200px"></asp:ListBox>
            </div>
            <div id="printerList" style="float:left;margin-left:10px;width:100px;height:250px;">
             <h3 style="width:99px;text-align:center;background-color:#0094ff;color:white;border:1px solid gray;">Printer</h3>
                <asp:ListBox ID="PrintersLB" runat="server" Height="200px" Width="100px" OnSelectedIndexChanged="PrintersLB_SelectedIndexChanged"></asp:ListBox>
            </div>
            <div id="localPrinterList" style="float:left;margin-left:10px;display:none;">
             <h3>Local Printer</h3>
                <asp:ListBox ID="LocalPrintersLb" runat="server" Height="200px" Width="200px" OnSelectedIndexChanged="LocalPrintersLb_SelectedIndexChanged"></asp:ListBox>
            </div>
        </div>
        <div style="margin-left:10px;float:left;padding:10px;border:1px solid gray;width:500px;height:520px;background:url(data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAcAAAAHCAYAAADEUlfTAAAAGUlEQVQImWNIqL7tiwszJFTf9mXABQabJAAKGxooqye0mwAAAABJRU5ErkJggg==) repeat;">
            <div id="variablelist" style="float:left;position:relative;">
                <div id="x_dialogIcon">X_</div>
                <div id="range_dialogIcon">R</div>
                <div style="float:left;">
                    <h3 style="width:199px;text-align:center;background-color:green;color:white;border:1px solid gray;">Select Variable</h3>
                    <ctrl:Variables_ListBox ID="VariablesLB" runat="server" height="300px" width="200px" ></ctrl:Variables_ListBox>
                </div>
                <div style="float:left; margin-left:5px;">
                    <h3 style="width:100%;text-align:center;background-color:#ff0000;color:white;border:1px solid gray;">Set Variable</h3>
                    <asp:TextBox ID="VariableValueTxt" runat="server" Width="200px" AutoCompleteType="Disabled" BorderStyle="Solid" BorderWidth="1" BorderColor="Gray"></asp:TextBox><br />
                    <asp:Button runat="server" ID="SaveBtn" Text="Save" OnClick="SaveBtn_Click"   BorderColor="#3333FF" BorderStyle="Solid" BorderWidth="1" />
                    <input id="VariableSaveBtn" type="button" value="Save" style="margin-left:2px;display:none;"/>
                    <input id="UpdatePreviewBtn" type="button" value="Update Preview" style="margin-left:2px;display:none;"/>
                </div>
                <div style="float:left; margin-left:5px;width:90%;">
                    <div style="float:left; margin-left:5px;">
                        <asp:Button runat="server" ID="PrintBtn" Text="Print" OnClick="PrintBtn_Click" BorderColor="Blue" BorderStyle="Solid" BorderWidth="1" />
                    </div>
                    <div style="float:left; margin-left:5px;margin-top:5px;">
                        <asp:Label runat="server" ID="PrintQuantityLbl" Text="Qty: " Font-Bold="True" Font-Size="Small"></asp:Label>
                        <asp:TextBox ID="PrintQuantity" runat="server" Text="1" Width="20px" Height="10px"></asp:TextBox>
                    </div>
                    <div style="float:left; margin-left:5px;width:100%;">
                        <asp:CheckBox ID="PersistVariableValuesChk" runat="server" /><span><b> Remember</b></span>
                    </div>
                    <div style="float:left; margin-left:5px;width:100%;">
                    </div>
                </div>
            </div>
            <div id="preview"  style="position:absolute;min-width:250px;top:0px;right:0px;margin-right:10px;background-color:black;border:1px solid grey">
                <h3 style="margin-left:5px;color:#ffffff;">Preview:</h3>
                <div style="position:absolute;top:0px;right:5px;height:25px;">
                    <div  id="previewshow" class="previewcontrol">show</div>
                    <div  id="previewclose" class="previewcontrol">close</div>
                </div>
                <div id="previewmsg" style="width:100%;color:#ff0000;display:none;font-weight:bold;margin-left:3px;">Loading Preview...</div>
                <div style="width:100%;">
                    <img src="Images/preview.png" id="previewimg"  /><br />
                </div>
                <div style="width:100%;">
                    <div class="lableinfo" style="color:#ffffff;font-weight: bold;">Label Height: <span id="lblheight" style="color:#ffffff;font-weight: normal;"></span></div>
                    <div class="lableinfo" style="color:#ffffff;font-weight: bold;">Label Width: <span id="lblwidth" style="color:#ffffff;font-weight: normal;"></span></div>
                    <div class="lableinfo" style="color:#ffffff;font-weight: bold;">Orientation: <span id="lblorientation" style="color:#ffffff;font-weight: normal;"></span></div>
                </div>
            </div>
        </div>
    </div>
    <div style="float:left;">
        <asp:Label ID="MessageLbl" runat="server" Text="" ForeColor="Red"></asp:Label>
    </div>
    <div id="savedlg" style="border:2px solid black;position:absolute;top:50%;left:50%;display:none;width:200px;height:200px;background-color:white;text-align:center;line-height:200px;">...</div>
    <div id="x_dialogcontainer">
        <div id="x_dialogclose">X</div>
            <div id="x_dialogitems">
                <!-- testing with N12937 -->
            </div>
        <div id="x_dialogsave">Save</div>
    </div>
    <div id="range_dialogcontainer">
        <div id="range_dialogclose">X</div>
        <div style="font-weight:bold;margin-top:10px;width:100%;">Enter Begin and End Ranges </div><br /><div>examples: (ABC<b>1</b>-ABC<b>11</b>, <b>1</b>ABC-<b>11</b>ABC, A<b>1</b>BC-A<b>11</b>BC)</div>
        <div class="item"><div style="padding:5px;width:100%;">Begin Sequence</div><input type="text" id="range_lrange_tb" /></div>
        <div class="item"><div style="padding:5px;width:100%;">End Sequence</div><input type="text" id="range_urange_tb" /></div>
        <div id="range_dialogsave">Save</div>
        <div id="range_dialogmessage" class="message"></div>
    </div>
    <style>
        .message {
            width: 100%;
            color: red;
            font-weight: bold;
        }
        .previewcontrol{
            float:left;
            height:25px;
            width:50px;
            text-align:center;
            line-height:25px;
            background-color:#0094ff;   
            border:1px solid black;
            margin-right:5px;
            margin-bottom:5px;
            color:#ffffff;
        }
        .previewcontrol:hover {
            cursor:pointer;
            opacity:0.75;
        }
        .lableinfo{
            float:left;
            margin-right:5px;
        }
        .directorylist td{
            padding:0px;
        }
        #x_dialogcontainer {
            border:2px solid black;
            position:absolute;
            top:200px;
            right:25%;
            display:none;
            width:800px;
            height:600px;
            background-color:white;
        }
        #x_dialogitems {
            margin-left:15px;
        }
        #x_dialogclose {
            position:absolute;
            right:10px;
            top:10px;
            border: 2px solid black;
            width: 25px;
            height: 25px;
            background-color: white;
            line-height:25px;
            text-align:center;
            font-weight:bold;
            color:red;
        }
        #x_dialogclose:hover{
            cursor:pointer;
        }
        #x_dialogsave {
            border: 2px solid black;
            width: 50px;
            height: 25px;
            background-color: white;
            line-height:25px;
            text-align:center;
            font-weight:bold; 
        }
        #x_dialogsave:hover{
            cursor:pointer;
        }
        #x_dialogIcon{
            display:none;
            position: absolute;
            top: 0px;
            right: 0px;
            border: 2px solid black;
            width: 25px;
            height: 25px;
            background-color: white;
            line-height:25px;
            text-align:center;
            font-weight:bold;
            color:green;
        }
        #x_dialogIcon:hover{
            cursor:pointer;
        }
        .x_item_group{
            width:100%;
            min-height:75px;
        }
        .x_item_group_header {
            font-weight:bold;
        }
        .x_item{
            float:left;
            max-width:27px;
            max-height:50px;
            border:1px solid grey;
        }
        .x_item_label{
            text-align:center;
            width:25px;
        }
        .x_item_chk{
            width:25px !important;
        }
        #range_dialogcontainer {
            border:2px solid black;
            position:absolute;
            top:200px;
            right:25%;
            padding:5px;
            display:none;
            width:400px;
            height:300px;
            background-color:white;
        }
        #range_dialogclose {
            position:absolute;
            right:10px;
            top:10px;
            margin-bottom:5px;
            border: 2px solid black;
            width: 25px;
            height: 25px;
            background-color: white;
            line-height:25px;
            text-align:center;
            font-weight:bold;
            color:red;
        }
        #range_dialogclose:hover{
            cursor:pointer;
        }
        #range_dialogsave {
            border: 2px solid black;
            width: 50px;
            height: 25px;
            background-color: white;
            line-height:25px;
            text-align:center;
            font-weight:bold;
        }
        #range_dialogsave:hover{
            cursor:pointer;
        }
        #range_dialogIcon{
            position: absolute;
            top: 30px;
            right: 0px;
            border: 2px solid black;
            width: 25px;
            height: 25px;
            background-color: white;
            line-height:25px;
            text-align:center;
            font-weight:bold;
            color:green;
        }
        #range_dialogIcon:hover{
            cursor:pointer;
        }
    </style>

    <script type="text/javascript">

        $(document).ready(function () {

            for(i = 0; i < groups.length; i++){
                openXElementGroup(groups[i]);
                for (j = 0; j < groupItems.length; j++) {
                    var groupItem = groupItems[j];
                    if (groupItem.group == groups[i]) {
                        addXElement(groupItem.group, $(groupItem.item));
                    }
                }
                closeXElementGroup(groups[i]);
            }
            $("#x_dialogsave").on('click', function () {
                var dialog = $("#x_dialogcontainer");
                dialog.hide();
                GetPreview();
            });
            $("#x_dialogclose").on('click', function () {

                var dialog = $("#x_dialogcontainer");
                var variablesContainer = $("#variablelist");
                dialog.hide();
            });
            $("#x_dialogIcon").on('click', function () {
                var dialog = $("#x_dialogcontainer");
                dialog.show();
                hfValues = getHFValues();
                for (i = 0; i < hfValues.length; i++) {
                    if (hfValues[i].Value.toLowerCase() == 'x')
                        $('#x_dialogitems :input[id$="' + hfValues[i].Name + '"]').attr("checked"," ");
                    else
                        $('#x_dialogitems :input[id$="' + hfValues[i].Name + '"]').removeAttr("checked");
                }
            });
            $("#range_dialogIcon").on('click', function () {
                var selected = $('#' + varListID + ' :selected');
                if (!selected.length) {
                    return;
                }
                var dialog = $("#range_dialogcontainer");
                dialog.show();
                $("#range_dialogmessage").text("");
                var valueTextBox = $("#" + varTextBoxID);
                if (valueTextBox.val().toLowerCase().indexOf(delimiter_serialsequence) > -1) {
                    var parts = valueTextBox.val().split(delimiter_serialsequence);
                    $("#range_lrange_tb").val(parts[0]);
                    $("#range_urange_tb").val(parts[1]);
                }
            });
            $("#range_dialogclose").on('click', function () {
                var dialog = $("#range_dialogcontainer");
                dialog.hide();
            });
            $("#range_dialogsave").on('click', function () {
                var dialog = $("#range_dialogcontainer");
                var dialogIcon = $("#range_dialogIcon");
                var lowerRange = $("#range_lrange_tb").val();
                var upperRange = $("#range_urange_tb").val();
                //Handle the user input.
                $("#" + varTextBoxID).val(lowerRange + delimiter_serialsequence + upperRange);
                dialog.hide();
                saveValue();
            });
            $(this).keydown(function (e) {
                switch (e.which) {
                    case 13: // enter
                        saveValue();
                        break;
                    case 37: // left
                        break;
                    case 38: // up
                        var selected = $('#' + varListID + ' :selected');
                        $('#' + varListID + ' option').removeAttr('selected');
                        var first = $('#' + varListID + ' option:first');
                        var last = $('#' + varListID + ' option:last');
                        var prev = selected.prev('option');
                        if (selected.val() == first.val())
                            prev = last;
                        prev.attr('selected', 'selected');
                        selectOption(prev);
                        break;
                    case 39: // right
                        break;
                    case 40: // down
                        var selected = $('#' + varListID + ' :selected');
                        $('#' + varListID + ' option').removeAttr('selected');
                        var first = $('#' + varListID + ' option:first');
                        var last = $('#' + varListID + ' option:last');
                        var next = selected.next('option');
                        if (selected.val().toLowerCase() == last.val().toLowerCase())
                            next = first;
                        next.attr('selected', 'selected');
                        selectOption(next);
                        break;
                    default: return; // exit this handler for other keys
                }
                e.preventDefault(); // prevent the default action (scroll / move caret)
            });
            $("#previewclose").on('click', function () {
                $("#previewimg").hide();
            })
            $("#previewshow").on('click', function () {
                $("#previewimg").show();
            })
            $("#" + varListID).on('change', function () {
                var selected = $('#' + varListID + ' :selected');
                selectOption(selected);
            });
            $("#VariableSaveBtn").on('click', function () {
                saveValue();
            });
            $("#UpdatePreviewBtn").on('click', function () {
                GetPreview();
                $("#previewimg").show();
            });

            function openXElementGroup(groupname) {
                var element = $("<div class='x_item_group' id='" + groupname + "' >");
                $("#x_dialogitems").append(element);
                element.append("<div class='x_item_group_header'>" + groupname + "</div>");

            }
            function closeXElementGroup(groupname) {
                $("#x_dialogitems #" + groupname).append("<br />");
            }
            function addXElement(group, element) {
                element.find(":input").on('change', function () {
                    var variableName = $(this).attr('id');
                    var value = "";
                    if ($(this).attr('checked')) {
                        value = "x";
                    }
                    SetVariableValue(variableName, value);
                });
                $("#x_dialogitems #" + group).append(element);
            }

            function saveValue() {
                var selected = $('#' + varListID + ' :selected');
                $("#" + varListID + ' option').removeAttr('selected');
                var value = $("#" + varTextBoxID).val();
                SetVariableValue(selected.val(), value);
                $("#previewimg").show();
                GetPreview();
                selectOption(selected.next('option').attr('selected', 'selected'));    
                $("#" + varTextBoxID).focus();
            }
            function selectOption(option) {
                $("#" + varTextBoxID).val("");
                $("#" + varTextBoxID).val(option.attr('tag'));
            }
            function SetVariableValue(name, value) {
                for (var i = 0; i < CodeSoftDTO.Variables.length; i++) {
                    if (CodeSoftDTO.Variables[i].Name.toLowerCase() == name.toLowerCase()) {
                        CodeSoftDTO.Variables[i].Value = value;
                        var option = $('#' + varListID + ' option').filter(function () {
                            return $(this).text().toLowerCase() == name.toLowerCase();
                        })
                        option.attr('tag', value);
                    }
                }
                saveXDialog(name, value);
            }
            function getHFValues() {
                var hfstring = $('#<%=this.VariableValuesHf.ClientID%>').val();
                if (hfstring)
                    hfValues = JSON.parse(hfstring);
                return hfValues;
            }
            function saveXDialog(name, value) {
                hfValues = getHFValues();
                var doesExist = false; //corny, medieval versions of IE which do not support array splicing.
                for (i = 0; i < hfValues.length; i++) {
                    if (hfValues[i].Name.toLowerCase() == name.toLowerCase()) {
                        doesExist = true;
                        hfValues[i].Value = value;
                    }
                }
                //This push should never happen, but strange things do occur.
                if (doesExist == false) {
                    hfValues.push({ Name: name, Value: value });
                }
                $('#<%=this.VariableValuesHf.ClientID%>').val(JSON.stringify(hfValues));
            }

        });
    </script>
</asp:Content>
