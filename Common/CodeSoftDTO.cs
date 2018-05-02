using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CodeSoftPrinterApp.Common
{
    public class CodeSoftDTO
    {
        public string SessionId { get; set; }
        public List<Variable> Variables = new List<Variable>();
        public string Label;
        public string Printer;

        public class Variable
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public string GroupName { get; set; }
            /// <summary>
            /// Property indicates if this variable has an "X" indicator.
            /// </summary>
            public bool IsXBox { get; set; }
            /// <summary>
            /// Assume CS arrays are zero based. Some are not, so this property represents the ordinal shift needed.
            /// </summary>
            public Int32 ArrayBaseShift { get; set; }
            /// <summary>
            /// Poperty indicating CS array index
            /// </summary>
            public Int32 ArrayIndex { get; set; }
            /// <summary>
            /// Property indicates if this variable shoul be displayed in the client.
            /// </summary>
            public bool IsHidden { get; set; }


        }
        public class VariableGroup
        {
            public string Name { get; set; }
            public List<Variable> Variables { get; private set; }
            public VariableGroup()
            {
                Variables = new List<Variable>();
            }
        }
        public class LabelInfo
        {
            public string LabelHeight { get; set; }
            public string LabelWidth { get;set;}
            public string PageHeight { get; set; }
            public string PageWidth { get; set; }
            public string PaperType { get; set; }
            public string Orientation { get; set; }
        }
    }
}