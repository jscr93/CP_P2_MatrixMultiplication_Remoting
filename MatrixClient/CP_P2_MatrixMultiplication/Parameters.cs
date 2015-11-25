using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatrixMultiplicationClient
{
    class Parameters
    {
        public string   path1            { get; set; }
        public string   path2            { get; set; }
        public string   path_result      { get; set; }
        public int      rows_m1          { get; set; }
        public int      columns_m1       { get; set; }
        public char     separator        { get; set; }
    }
}
