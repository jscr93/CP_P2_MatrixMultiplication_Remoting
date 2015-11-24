using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace MatrixServerDLL
{
    public class MatrixServer : MarshalByRefObject
    {
        private List<string>            clients         = new List<string>();           // Ip of clients
        private List<RowsToMultiply>    sourceMatrices  = new List<RowsToMultiply>();   //
        private List<RowResult>         resultMatrix    = new List<RowResult>();        //
        public  bool                    executing       = false;

        // Commands
        public void AddClient(String name)
        {
            if (name != null)
            {
                lock (clients)
                {
                    clients.Add(name);
                }
            }
        }

        public void RemoveClient(String name)
        {
            lock (clients)
            {
                clients.Remove(name);
            }
        }

        public bool AddSourceRow(RowsToMultiply newSourceRow)
        {
            if (newSourceRow == null)
                return false;
            lock (sourceMatrices)
            {
                sourceMatrices.Add(newSourceRow);
            }
            return true;
        }

        public void AddResultRow(RowResult rowResultToAdd)
        {
            if (rowResultToAdd != null)
            {
                lock(resultMatrix)
                {
                    lock(sourceMatrices[rowResultToAdd.rowNumber])
                    {
                        RowsToMultiply sourceRow = sourceMatrices[rowResultToAdd.rowNumber];
                        if (!sourceRow.received)
                        {
                            sourceRow.received = true;
                            resultMatrix.Add(rowResultToAdd);
                        }
                    }
                }
            }
        }

        // Queries
        public List<string> Clients()
        {
            return clients;
        }

        public RowsToMultiply getSourceRows(int rowNumber)
        {
            return sourceMatrices.Where(m => m.rowNumber == rowNumber).First();
        }

        public ArrayList getMultipleSourceRows(int[] rowNumbers)
        {
            ArrayList sourceRows = new ArrayList();
            foreach (int i in rowNumbers)
            {
                int aux_i = i;
                sourceRows.Add(sourceMatrices.Where(m => m.rowNumber == rowNumbers[aux_i]).First());
            }
            return sourceRows;
        }
    }

    public class RowsToMultiply
    {
        public int      rowNumber       { get; private set; }
        public string   row_Matrix1     { get; private set; }
        public string   row_Matrix2     { get; private set; }
        public bool     sent            { get; set; }
        public bool     received        { get; set; }

        public RowsToMultiply(int rowNumber, string row_Matrix1, string row_Matrix2)
        {
            this.rowNumber = rowNumber;
            this.row_Matrix1 = row_Matrix1;
            this.row_Matrix2 = row_Matrix2;
            sent = false;
            received = false;
        }
    }

    public class RowResult
    {
        public int      rowNumber   { get; private set; }
        public string   rowResult   { get; private set; }

        public RowResult(int rowNumber, string rowResult)
        {
            this.rowNumber = rowNumber;
            this.rowResult = rowResult;
        }
    }
}
