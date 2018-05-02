var delimiter_serialsequence;

var hfValues = new Array();
var groups = new Array();
var groupItems = new Array();

var CodeSoftDTO = "";
var varTextBoxID;
var varListID;
var lastSelected;
var lastSelectedVal;
var LabelInfo;



function ShowMessage(msg) {
    $("#savedlg").html(msg);
    $("#savedlg").show();
}
function HideMessage() {
    $("#savedlg").hide();
}
function GetPreview() {
    $("#previewmsg").show();
    var dataOut = JSON.stringify({ jsonDTO: JSON.stringify(CodeSoftDTO) });
    $.ajax({
        type: "POST",
        url: "Handlers/WebService.asmx/GetPreview",
        data: dataOut,
        contentType: "application/json; charset=utf-8",
        dataType: 'json',
        success: function (data) {
            document.getElementById("previewimg").src = "data:image/png;base64," + data.d;
        },
        failure: function (errMsg) {
            alert('error');
        },
        complete: function (e, e2) {
            $("#previewmsg").hide();
        }
    });
}
function SetLabelInfo() {
    $("#lblheight").text(LabelInfo.LabelHeight);
    $("#lblwidth").text(LabelInfo.LabelWidth);
    $("#lblorientation").text(LabelInfo.Orientation);
}
