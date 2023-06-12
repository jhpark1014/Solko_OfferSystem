using System;
using System.Collections.Generic;
using Microsoft.SqlServer.Server;


// Class declaration. We implement IEnumerable and IEnumerator in the
// same class.

namespace TVPDemo
{
    public class CSV_splitter : IEnumerable<SqlDataRecord>,
                               IEnumerator<SqlDataRecord>
    {
        string input;         // The input string.
        char delim;         // The delimiter.
        int start_ix;      // Start position for current list element.
        int end_ix;        // Position for the next list delimiter.
        SqlDataRecord outrec;        // The record we use to return data.


        // Constructor with delimiter parameter.
        public CSV_splitter(string str,
                             char delimiter)
        {
            // Save input string and delimiter.
            this.input = str;
            this.delim = delimiter;

            // Create an SqlDataRecord to return in the Current method.
            this.outrec = new SqlDataRecord(
                   new SqlMetaData("nnnn", System.Data.SqlDbType.BigInt));

            // Perform the Reset() operation for the rest of the initiation.
            this.Reset();
        }

        public CSV_splitter(string str) : this(str, ',') { }

        // GetEnumerator - part of IEnumerable. There are two of them since this
        // is required by the generic class. Since we also implement IEnumerator
        // we just return ourselves.
        System.Collections.IEnumerator
             System.Collections.IEnumerable.GetEnumerator()
        {
            return this;
        }

        public System.Collections.Generic.IEnumerator<SqlDataRecord>
             GetEnumerator()
        {
            return this;
        }


        // Reset - part of IEnumerable. We set current position in the string
        // to be before the string.
        public void Reset()
        {
            this.start_ix = -1;
            this.end_ix = -1;
        }


        // MoveNext - part of IEnumerable. We move start_ix and end_ix to
        // the next element in the list.
        public bool MoveNext()
        {
            this.start_ix = this.end_ix + 1;

            // There may be multiple adjacent delimiters, that is, empty
            // list elements. We skip until we find a character that is
            // not a delimitere.
            while (this.start_ix < this.input.Length &&
                   this.input[this.start_ix] == this.delim)
            {
                this.start_ix++;
            }

            // If we did not find any non-delimiter, we have exhausted the
            // string, and we should return false to tell caller that we're done.
            if (this.start_ix >= this.input.Length)
            {
                return false;
            }

            // Find the next delimiter. If there are no more delimiters, we
            // say that there is one after the end of the list.
            this.end_ix = this.input.IndexOf(this.delim, this.start_ix);
            if (this.end_ix == -1)
            {
                this.end_ix = this.input.Length;
            }

            // Return true since there is at least one more elment
            return true;
        }


        // Current - part if IEnumerable. Extract the current list value and
        // return it in the SqlDataRecord.
        public SqlDataRecord Current
        {
            get
            {
                string str = this.input.Substring(this.start_ix,
                                                  this.end_ix - this.start_ix);
                //this.outrec.SetInt64(0, Convert.ToInt64(str));
                return this.outrec;
            }
        }

        // IEnumerable<T> requires that we also implement a non-generic Current.
        Object System.Collections.IEnumerator.Current
        {
            get
            {
                return this.Current;
            }
        }

        // Dispose is required for IEnumerable.
        public void Dispose()
        {
            this.outrec = null;
        }

        // We override ToString() to make debugging more interesting.
        public override string ToString()
        {
            return "CSV_splitter: " + input;
        }
    }
}