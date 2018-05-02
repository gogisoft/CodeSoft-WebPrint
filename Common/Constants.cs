using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CodeSoftPrinterApp.Common
{
    /// <summary>
    /// This class is responsible to store common values used by the client and server.
    /// </summary>
    public class Constants
    {
        /// <summary>
        /// string used to delimit a programatically generated set of serial number in sequence from a lower starting value to an end value.
        /// </summary>
        public const string Delimiter_SequencedNumberRange = ":::";
        /// <summary>
        /// Maximum number of units between range values.
        /// </summary>
        public const int MaxRangeSpan = 100;
    }
}