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
        private int                     totalRows;
        private object                  downloadedLock  = new object();
        private int                     downloaded;
        public  object                  isWorkingLock   = new object();
        public  bool                    isWorking       = false;
        private object                  callingClientsLock = new object();
        private bool                    callingClients = false;



        public void AddClient(String name)
        {
            if (name != null)
            {
                lock (clients)
                {
                    if (clients.Exists(c => c == name))
                        name = name + "A";
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

        public bool Start()
        {
            lock(isWorkingLock)
            {
                if (isWorking)
                    return false;
                else
                    isWorking = true;
                return true;
            }
        }

        public bool Finish()
        {
            lock(isWorkingLock)
            {
                isWorking = false;
                return true;
            }
        }

        public bool WaitForAllClientsToFinishDownload()
        {
            if (downloaded == clients.Count)
                return true;
            return false;
        }

        public void downloadedRowGroup()
        {
            lock(downloadedLock)
                downloaded++;
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

        Dictionary<string, RowGroups> assignedRowsGroups;
        private class RowGroups
        {
            public List<int> rowGroups;
            public int totalRows;
            public int downloadedRows;
            public RowGroups()
            {
                rowGroups = new List<int>();
                totalRows = 0;
                downloadedRows = 0;
            }
            public void Add(int row)
            {
                rowGroups.Add(row);
                totalRows++;
            }
        }
        public int[] DispatchRowGroupsToClients(int rowsNumber, string clientSender)
        {
            totalRows = rowsNumber;
            if (totalRows <= 0)
                return null;
            assignedRowsGroups = new Dictionary<string, RowGroups>();
            int clientsNumber = clients.Count;
            int groups = totalRows / clientsNumber;
            int row = 0;
            foreach (string client in clients)
            {
                RowGroups groupForClient = new RowGroups();
                for (int i = 0; i < groups; i++)
                    groupForClient.Add(row++);
                assignedRowsGroups.Add(client, groupForClient);
            }
            while(row < totalRows)
            {
                assignedRowsGroups[clients[0]].Add(row++);
            }

            downloaded = 1; //clientSender has already downloaded its rows
            lock(callingClientsLock)
            {
                callingClients = true;
            }
            foreach( int rowIndex in assignedRowsGroups[clientSender].rowGroups)
            {
                sourceMatrices[rowIndex].sent = true;
                assignedRowsGroups[clientSender].downloadedRows++;
            }
            return assignedRowsGroups[clientSender].rowGroups.ToArray();
        }

        public List<string> Clients()
        {
            return clients;
        }

        public bool CallingClients()
        {
            return callingClients;
        }

        public RowsToMultiply getNextAssignedSourceRow(string clientIP)
        {
            List<int> assignedSourceRows = assignedRowsGroups[clientIP].rowGroups;
            if (assignedSourceRows.Count == 0)
                return null;
            int nextAssignedSourceRow = assignedSourceRows[0];
            assignedSourceRows.RemoveAt(0);
            return getSourceRow(nextAssignedSourceRow);
        }

        public void rowDownloadSuccess(string clientIP)
        {
            //assignedRowsGroups[clients.Where(c => c == clientIP).First()].downloadedRow++;
            assignedRowsGroups[clientIP].downloadedRows++;
        }

        private RowsToMultiply getSourceRow(int rowNumber)
        {
            return sourceMatrices.Where(m => m.rowNumber == rowNumber).First();
        }

        private ArrayList getMultipleSourceRows(int[] rowNumbers)
        {
            ArrayList sourceRows = new ArrayList();
            foreach (int i in rowNumbers)
            {
                int aux_i = i;
                sourceRows.Add(sourceMatrices.Where(m => m.rowNumber == rowNumbers[aux_i]).First());
            }
            return sourceRows;
        }

        public int getClientsNumber()
        {
            return clients.Count;
        }

        public ArrayList getDetails()
        {
            ArrayList details = new ArrayList();
            foreach (string client in clients)
            {
                details.Add(string.Format("{0} {1}/{2}",client, assignedRowsGroups[client].downloadedRows, assignedRowsGroups[client].totalRows));
            }
            return details;
        }
    }

    [Serializable]
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

    [Serializable]
    public class RowResult
    {
        public int      rowNumber   { get; private set; }
        public string   rowResult   { get; private set; }
        public bool     sentToRequesterClient { get; private set; }
        public RowResult(int rowNumber, string rowResult)
        {
            this.rowNumber = rowNumber;
            this.rowResult = rowResult;
            this.sentToRequesterClient = false;
        }
    }
}
