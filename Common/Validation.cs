using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CodeSoftPrinterApp.Common
{
    public class Validation
    {
        public static void CheckNumberRange(List<CodeSoftDTO.Variable> variables)
        {
            bool hasSerialNumberRange = variables.Exists(p => p.Value.Contains(Constants.Delimiter_SequencedNumberRange));
            if (hasSerialNumberRange)
            {
                int count = variables.Where(p => p.Value.Contains(Constants.Delimiter_SequencedNumberRange)).Count();
                if (count > 1)
                {
                    throw new UserError("More than 1 variables are using a range of values. Ranges can only be used once per label!");
                }
                CodeSoftDTO.Variable rangeVariable = variables.Where(p => p.Value.Contains(Constants.Delimiter_SequencedNumberRange)).First();
                int delimiterIndex = rangeVariable.Value.IndexOf(Constants.Delimiter_SequencedNumberRange);
                int delimiterLength = Constants.Delimiter_SequencedNumberRange.Length;
                string start = rangeVariable.Value.Substring(0, delimiterIndex);
                string end = rangeVariable.Value.Substring(delimiterIndex + delimiterLength);

                decimal decCheck;
                var startNumberCheck = decimal.TryParse(start, out decCheck);
                var endNumberCheck = decimal.TryParse(end, out decCheck);
                if (!startNumberCheck)
                {
                    //throw new UserError("Variable \"" + rangeVariable.Name + "\" has a range where the beginning is not a number!");
                }
                else if (!endNumberCheck)
                {
                    //throw new UserError("Variable \"" + rangeVariable.Name + "\" has a range where the end is not a number!");
                }

                int startValue;
                int endValue;
                bool startIntCheck = int.TryParse(start, out startValue);
                bool endIntCheck = int.TryParse(end, out endValue);
                if (!startIntCheck)
                {
                    //throw new UserError("Variable \"" + rangeVariable.Name + "\"  has a begin number that is too large!");
                }
                else if (!endIntCheck)
                {
                    //throw new UserError("Variable \"" + rangeVariable.Name + "\"  has an end number that is too large!");
                }

                if (startValue > endValue)
                {
                    //throw new UserError("Variable \"" + rangeVariable.Name + "\" has a beginning that is greater than the end!");
                }
                else if ((endValue - startValue) > Constants.MaxRangeSpan)
                {
                    //throw new UserError("Variable \"" + rangeVariable.Name + "\" has a range which spans greater than " + Constants.MaxRangeSpan.ToString() + ".");
                }
            }
        }
    }
}