using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// O(ND) Difference Algorithm Implementation based on Longest Common Subsequence by Eugene Mayers University of Arizona, Tucson, Arizona
/// This algorithm compares two arrays of of numbers. Each string comparison is comverted to a hash number.
/// The original algorithm was designed for file diff comparison. This one has been modified to accept out lier part number naming convention.
/// The general idea to follow it this. Two strings (part numbers) are passed as paramters, StringA and StringB. Ask yourself this question, "What does StringA need to have done to become StringB?"
/// Two primary actions are recorded. The first is sequences removed from StringA. The second is sequences from StringB inserted into StringA. 
/// For example: Let us use the "X" is used as part number wildcard (StringB). For an acceptible part number (StringA) replacement the inserts should all be "X" and there should be the same number of deletes from StringA as there were inserts from StringB.
/// This would mean that the only difference in the partnumbers is the "X".
/// 
/// </summary>
/// <remarks></remarks>
public class Diff
{
    // <summary>details of one difference.</summary>
    public class Item
    {
        // <summary>Start Line number in Data A.</summary>
        public int StartA;
        // <summary>Start Line number in Data B.</summary>
        public int StartB;
        // <summary>Number of changes in Data A.</summary>
        public int deletedA;
        // <summary>Number of changes in Data B.</summary>
        public int insertedB;
    }

    // <summary>
    // Shortest Middle Snake Return Data
    // </summary>
    public class SMSRD
    {
        public int x;
        public int y;
        // internal int u, v;  // 2002.09.20: no need for 2 points 
    }

    public static string TestHelper(ref Item[] f)
    {
        StringBuilder ret = new StringBuilder();
        for (int n = 0; n < f.Length; n++)
        {
            ret.Append(f[n].deletedA.ToString() + "." + f[n].insertedB.ToString() + "." + f[n].StartA.ToString() + "." + f[n].StartB.ToString() + "*");
        }
        // Debug.Write(5, "TestHelper", ret.ToString());
        return (ret.ToString());
    }

    public Item[] DiffText(string TextA, string TextB)
    {
        return (DiffText(TextA, TextB, false, false, false));
    }

    public static Item[] DiffText(string TextA, string TextB, bool trimSpace, bool ignoreSpace, bool ignoreCase)
    {
        // prepare the input-text and convert to comparable numbers.
        Hashtable h = new Hashtable(TextA.Length + TextB.Length);

        // The A-Version of the data (original data) to be compared.
        DiffData DataA = new DiffData(DiffCodes(TextA, ref h, trimSpace, ignoreSpace, ignoreCase));

        // The B-Version of the data (modified data) to be compared.
        DiffData DataB = new DiffData(DiffCodes(TextB, ref h, trimSpace, ignoreSpace, ignoreCase));

        h = null;
        // free up hashtable memory (maybe)

        int MAX = DataA.Length + DataB.Length + 1;
        // vector for the (0,0) to (x,y) search
        int[] DownVector = new int[2 * MAX + 3];
        // vector for the (u,v) to (N,M) search
        int[] UpVector = new int[2 * MAX + 3];

        LCS(ref DataA, 0, DataA.Length, ref DataB, 0, DataB.Length, DownVector, UpVector);

        Optimize(ref DataA);
        Optimize(ref DataB);
        return CreateDiffs(ref DataA, ref DataB);
    }

    private static void Optimize(ref DiffData Data)
    {
        int StartPos = 0;
        int EndPos = 0;

        StartPos = 0;
        while ((StartPos < Data.Length))
        {
            while (((StartPos < Data.Length) && (Data.modified[StartPos] == false)))
            {
                StartPos = StartPos + 1;
            }

            EndPos = StartPos;
            while (((EndPos < Data.Length) && (Data.modified[EndPos] == true)))
            {
                EndPos = EndPos + 1;
            }

            if (((EndPos < Data.Length) && (Data.data[StartPos] == Data.data[EndPos])))
            {
                Data.modified[StartPos] = false;
                Data.modified[EndPos] = true;
            }
            else
            {
                StartPos = EndPos;
            }
        }
    }

    public static Item[] DiffInt(int[] ArrayA, int[] ArrayB)
    {
        // The A-Version of the data (original data) to be compared.
        DiffData DataA = new DiffData(ArrayA);

        // The B-Version of the data (modified data) to be compared.
        DiffData DataB = new DiffData(ArrayB);

        int MAX = DataA.Length + DataB.Length + 1;
        // vector for the (0,0) to (x,y) search
        int[] DownVector = new int[2 * MAX + 3];
        // vector for the (u,v) to (N,M) search
        int[] UpVector = new int[2 * MAX + 3];

        LCS(ref DataA, 0, DataA.Length, ref DataB, 0, DataB.Length, DownVector, UpVector);
        return CreateDiffs(ref DataA, ref DataB);
    }


    private static int[] DiffCodes(string aText, ref Hashtable h, bool trimSpace, bool ignoreSpace, bool ignoreCase)
    {
        // get all codes of the text
        string[] Lines = null;
        int[] Codes = null;
        int lastUsedCode = h.Count;
        int aCode = 0;
        string s = null;

        // strip off all cr, only use lf as textline separator.
        aText = aText.Replace("\r", "");
        Lines = aText.Split(Environment.NewLine.ToCharArray());

        Codes = new int[aText.Length];

        for (int i = 0; i < aText.Length; i++)
        {
            s = aText.Substring(i, 1) + i;

            if ((trimSpace == true))
            {
                s = s.Trim();
            }
            if ((ignoreSpace))
            {
                s = Regex.Replace(s, "\\s+", " ");
                // TODO: optimization: faster blank removal.
            }

            if ((ignoreCase))
            {
                s = s.ToLower();
            }

            aCode = 0;
            if (h.ContainsKey(s))
                int.TryParse(h[s].ToString(), out aCode);

            if ((aCode == 0))
            {
                lastUsedCode += 1;
                h[s] = lastUsedCode;
                Codes[i] = lastUsedCode;
            }
            else
            {
                Codes[i] = aCode;
            }

        }
        return (Codes);

    }


    private static SMSRD SMS(ref DiffData DataA, int LowerA, int UpperA, ref DiffData DataB, int LowerB, int UpperB, int[] DownVector, int[] UpVector)
    {

        SMSRD ret = new SMSRD();
        int MAX = DataA.Length + DataB.Length + 1;

        int DownK = LowerA - LowerB;
        // the k-line to start the forward search
        int UpK = UpperA - UpperB;
        // the k-line to start the reverse search

        int Delta = (UpperA - LowerA) - (UpperB - LowerB);
        bool oddDelta = Delta == 1;

        // The vectors in the publication accepts negative indexes. the vectors implemented here are 0-based
        // and are access using a specific offset: UpOffset UpVector and DownOffset for DownVektor
        int DownOffset = MAX - DownK;
        int UpOffset = MAX - UpK;

        int MaxD = ((UpperA - LowerA + UpperB - LowerB) / 2) + 1;

        //  Debug.Write(2, "SMS", String.Format("Search the box: A[{0}-{1}] to B[{2}-{3}]", LowerA, UpperA, LowerB, UpperB));

        // init vectors
        DownVector[DownOffset + DownK + 1] = LowerA;
        UpVector[UpOffset + UpK - 1] = UpperA;


        for (int D = 0; D <= MaxD; D++)
        {
            // Extend the forward path.
            for (int k = DownK - D; k <= DownK + D; k += 2)
            {
                // Debug.Write(0, "SMS", "extend forward path " + k.ToString())

                // find the only or better starting point
                int x = 0;
                int y = 0;
                if ((k == DownK - D))
                {
                    x = DownVector[DownOffset + k + 1];
                    // down
                }
                else
                {
                    x = DownVector[DownOffset + k - 1] + 1;
                    // a step to the right
                    if (((k < DownK + D) && (DownVector[DownOffset + k + 1] >= x)))
                    {
                        x = DownVector[DownOffset + k + 1];
                        // down
                    }
                }
                y = x - k;

                // find the end of the furthest reaching forward D-path in diagonal k.
                while (((x < UpperA) && (y < UpperB) && (DataA.data[x] == DataB.data[y])))
                {
                    x = x + 1;
                    y = y + 1;
                }
                DownVector[DownOffset + k] = x;

                // overlap ?
                if ((oddDelta == true && (UpK - D < k) && (k < UpK + D)))
                {
                    if ((UpVector[UpOffset + k] <= DownVector[DownOffset + k]))
                    {
                        ret.x = DownVector[DownOffset + k];
                        ret.y = DownVector[DownOffset + k] - k;
                        // ret.u = UpVector(UpOffset + k);      // 2002.09.20: no need for 2 points 
                        // ret.v = UpVector(UpOffset + k) - k;
                        return (ret);
                    }
                }

            }

            // Extend the reverse path.
            for (int k = UpK - D; k <= UpK + D; k += 2)
            {
                // Debug.Write(0, "SMS", "extend reverse path " + k.ToString());

                // find the only or better starting point
                int x = 0;
                int y = 0;
                if ((k == UpK + D))
                {
                    x = UpVector[UpOffset + k - 1];
                    // up
                }
                else
                {
                    x = UpVector[UpOffset + k + 1] - 1;
                    // left
                    if (((k > UpK - D) && (UpVector[UpOffset + k - 1] < x)))
                    {
                        x = UpVector[UpOffset + k - 1];
                        // up
                    }
                }
                y = x - k;

                while (((x > LowerA) && (y > LowerB) && (DataA.data[x - 1] == DataB.data[y - 1])))
                {
                    x = x - 1;
                    y = y - 1;
                    // diagonal
                }
                UpVector[UpOffset + k] = x;

                // overlap ?
                if ((oddDelta == false && (DownK - D <= k) && (k <= DownK + D)))
                {
                    if ((UpVector[UpOffset + k] <= DownVector[DownOffset + k]))
                    {
                        ret.x = DownVector[DownOffset + k];
                        ret.y = DownVector[DownOffset + k] - k;
                        // ret.u = UpVector(UpOffset + k)     ' 2002.09.20: no need for 2 points 
                        // ret.v = UpVector(UpOffset + k) - k
                        return (ret);
                    }
                }

            }

        }

        throw new ApplicationException("the algorithm should never come here.");
    }

    private static void LCS(ref DiffData DataA, int LowerA, int UpperA, ref DiffData DataB, int LowerB, int UpperB, int[] DownVector, int[] UpVector)
    {
        // Fast walkthrough equal lines at the start
        while ((LowerA < UpperA && LowerB < UpperB && DataA.data[LowerA] == DataB.data[LowerB]))
        {
            LowerA += 1;
            LowerB += 1;
        }

        // Fast walkthrough equal lines at the end
        while ((LowerA < UpperA && LowerB < UpperB && DataA.data[UpperA - 1] == DataB.data[UpperB - 1]))
        {
            UpperA -= 1;
            UpperB -= 1;
        }

        if ((LowerA == UpperA))
        {
            // mark as inserted lines.
            while ((LowerB < UpperB))
            {
                //LowerB += 1
                DataB.modified[LowerB] = true;
                LowerB += 1;
            }

        }
        else if ((LowerB == UpperB))
        {
            // mark as deleted lines.
            while ((LowerA < UpperA))
            {
                //LowerA += 1
                DataA.modified[LowerA] = true;
                LowerA += 1;
            }

        }
        else
        {
            // Find the middle snakea and length of an optimal path for A and B
            SMSRD smsrd = SMS(ref DataA, LowerA, UpperA, ref DataB, LowerB, UpperB, DownVector, UpVector);
            // Debug.Write(2, "MiddleSnakeData", String.Format("{0},{1}", smsrd.x, smsrd.y));

            // The path is from LowerX to (x,y) and (x,y) to UpperX
            LCS(ref DataA, LowerA, smsrd.x, ref DataB, LowerB, smsrd.y, DownVector, UpVector);
            LCS(ref DataA, smsrd.x, UpperA, ref DataB, smsrd.y, UpperB, DownVector, UpVector);
            // 2002.09.20: no need for 2 points 
        }
    }


    private static Item[] CreateDiffs(ref DiffData DataA, ref DiffData DataB)
    {
        ArrayList a = new ArrayList();
        Item aItem = null;
        Item[] result = null;

        int StartA = 0;
        int StartB = 0;
        int LineA = 0;
        int LineB = 0;

        LineA = 0;
        LineB = 0;

        while ((LineA < DataA.Length) || (LineB < DataB.Length))
        {
            if (((LineA < DataA.Length) && (DataA.modified[LineA] == false) && (LineB < DataB.Length) && (DataB.modified[LineB] == false)))
            {
                // equal lines
                LineA = LineA + 1;
                LineB = LineB + 1;
            }
            else
            {
                // maybe deleted and/or inserted lines
                StartA = LineA;
                StartB = LineB;

                while ((LineA < DataA.Length) && (LineB >= DataB.Length || DataA.modified[LineA] == true))
                {
                    // while (LineA < DataA.Length && DataA.modified[LineA])
                    LineA = LineA + 1;
                }

                while ((LineB < DataB.Length) && (LineA >= DataA.Length || DataB.modified[LineB] == true))
                {
                    // while (LineB < DataB.Length && DataB.modified[LineB])
                    LineB = LineB + 1;
                }

                if (((StartA < LineA) || (StartB < LineB)))
                {
                    // store a new difference-item
                    aItem = new Item();
                    aItem.StartA = StartA;
                    aItem.StartB = StartB;
                    aItem.deletedA = LineA - StartA;
                    aItem.insertedB = LineB - StartB;
                    a.Add(aItem);
                }
            }

        }
        result = new Item[a.Count];

        a.CopyTo(result);

        return (result);
    }
    public static int[] DiffCharCodes(string aText, bool ignoreCase)
    {
        int[] Codes = null;
        if ((ignoreCase == true))
        {
            aText = aText.ToUpperInvariant();
        }

        Codes = new int[aText.Length];
        char[] textChar = aText.ToCharArray();
        for (int n = 0; n < textChar.Length; n++)
        {
            Codes[n] = (int)  textChar[n];
        }
        return (Codes);
    }
    public static Diff.DiffResults GetResults(string partA, string partB, ref Diff.Item[] items)
    {
        Diff.DiffResults result = new Diff.DiffResults();
        foreach (Diff.Item i in items)
        {
            int pos = 0;
            Diff.Item it = i;
            //  deleted chars
            if ((it.deletedA > 0))
            {
                string seq = "";
                Diff.Delete del = new Diff.Delete(partA, it.StartA, it.deletedA);
                for (int m = 0; m < it.deletedA; m++)
                {
                    seq = seq + partA.Substring(it.StartA + m, 1);
                }
                del.Sequence = seq;
                result.Deletes.Add(del);
            }
            //  inserted chars
            if ((it.insertedB > 0))
            {
                string seq = "";
                int pos2 = it.StartB;
                Diff.Insert insert = new Diff.Insert(partB, it.StartB, it.insertedB);
                while ((pos2 < (it.StartB + it.insertedB)))
                {
                    seq = seq + partB.Substring(pos2, 1);
                    pos2 += 1;
                }
                insert.Sequence = seq;
                result.Inserts.Add(insert);
            }
        }
        return result;
    }
    public class DiffData
    {
        public int Length;
        public int[] data;

        public bool[] modified;
        public DiffData(int[] initData)
        {
            data = initData;
            Length = initData.Length;
            modified = new bool[Length + 2];
        }

    }

    public class DiffResults
    {
        public List<Delete> Deletes = new List<Delete>();
        public List<Insert> Inserts = new List<Insert>();

        public DiffResults()
        {
        }
    }
    public class Delete
    {
        public readonly string Text;
        public readonly int Start;
        public readonly int Length;
        public string Sequence;
        public Delete(string text, int start, int length)
        {
            this.Text = text;
            this.Start = start;
            this.Length = length;
        }
    }
    public class Insert
    {
        public readonly string Text;
        public readonly int Start;
        public readonly int Length;
        public string Sequence;
        public Insert(string text, int start, int length)
        {
            this.Text = text;
            this.Start = start;
            this.Length = length;
        }
    }
}