using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Collections;
using MatrixServerDLL;

namespace MatrixMultiplicationClient
{
    class Matrix
    {
        private List<string> matrix1 = new List<string>(); // Holds text for matrix
        private List<string> matrix2 = new List<string>(); // Holds text for matrix
        private static string[] matrixResults; // Holds text for matrix
        private static Thread parallelThread;
        private static Thread listeningCallingClientsThread;
        private static Thread updatingUI;

        public static void createMatrixFile(string path, long rows, long columns, char separator, int randomSeed)
        {
            if (!File.Exists(path))
            {
                using (StreamWriter sw = File.CreateText(path))
                {
                    Random rnd = new Random(randomSeed);
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < rows; i++)
                    {
                        for (int j = 0; j < columns; j++)
                        {
                            int number = rnd.Next(1001);
                            sb.Append(number);
                            sb.Append(separator);
                        }
                        if (sb.Length > 0)
                            sb.Length -= 1;
                        sw.WriteLine(sb);
                        sb.Clear();
                    }
                }
            }
        }


        //Normal clients
        public static void StartListening()
        {
            listeningCallingClientsThread = new Thread(Listen);
            listeningCallingClientsThread.Start();
        }
        public static void Listen()
        {
            for (;;)
            {
                Thread.Sleep(1000);
                if (Client.server.CallingClients())
                {
                    executeAssignedMultiplications();
                    break;
                }
            }
        }
        public static void executeAssignedMultiplications()
        {
            List<RowsToMultiply> assignedRows_m1 = new List<RowsToMultiply>();
            List<RowsToMultiply> rows_m2 = new List<RowsToMultiply>();
            for(int i = 0;true; i++)
            {
                RowsToMultiply nextAssignedRow = Client.server.getSourceRow_m2(i);
                if (nextAssignedRow == null)
                    break;
                Client.server.downloadedRow_m2(Client.clientName);
                rows_m2.Add(nextAssignedRow);
            }

            for (;;)
            {
                RowsToMultiply nextAssignedRow = Client.server.getNextAssignedSourceRow(Client.clientName);
                if (nextAssignedRow == null)
                    break;
                Client.server.rowDownloadSuccess(Client.clientName);
                assignedRows_m1.Add(nextAssignedRow);
            }
            Client.server.downloadedRowGroup();
            for (;;)
            {
                Thread.Sleep(1000);
                if (Client.server.WaitForAllClientsToFinishDownload())
                    break;
            }

            //object objLockList = new object();
            //Parallel.ForEach<RowsToMultiply>(assignedRows, (rowsToMultiply) => {
            //    var m1_rowElements = rowsToMultiply.row_Matrix.Split(',').Select(Int32.Parse).ToArray(); 
            //    //var m2_rowElements = rowsToMultiply.row_Matrix.Split(',').Select(Int32.Parse).ToArray();
            //    StringBuilder sbRowResult = new StringBuilder();
            //    int mr_element = 0;
            //    for (int j = 0; j < m1_rowElements.Length; j++)
            //    {
            //        //mr_element += m1_rowElements[j] * m2_rowElements[j];
            //    }
            //    sbRowResult.Append(mr_element);
            //    sbRowResult.Append(",");
            //});
        }



        //ClientSender
        public static long multiplicationParallel(string Path1, string Path2, string Path_result, int Rows_m1, int Columns_m1, char Separator)
        {
            ParameterizedThreadStart pts = new ParameterizedThreadStart(Matrix.executeParallel);
            parallelThread = new Thread(pts);
            parallelThread.Start(new Parameters { path1 = Path1,
                path2 = Path2,
                path_result = Path_result,
                rows_m1 = Rows_m1,
                columns_m1 = Columns_m1,
                separator = Separator
            });
            return 0;
        }        
        public static void executeParallel(object parameters)
        {
            Parameters p = (Parameters)parameters;
            //var stopwatch = new Stopwatch();
            //stopwatch.Start();
            File.Delete(p.path_result);

            List<string> matrix1 = loadMatrix(p.path1);
            List<string> matrix2 = transpose(p.path2, p.columns_m1, p.rows_m1, p.separator);

            if (!Client.server.Start())
                return;
            Client.server.setTotalRows(p.rows_m1);
            startUpdateGUI();
            listeningCallingClientsThread.Abort();
            //Upload matrix to server
            for (int i = 0; i < p.rows_m1; i++)
            {
                Client.server.AddSourceRow_m1(new RowsToMultiply(i, matrix1[i]));
            }

            for (int i = 0; i < p.rows_m1; i++)
            {
                Client.server.AddSourceRow_m2(new RowsToMultiply(i, matrix2[i]));
            }

            int[] clientRows = Client.server.DispatchRowGroupsToClients(Client.clientName);
            //Esto ya esta, solo lo comento para probarlo
            //if(clientRows != null)
            //{
            //    matrixResults = new string[p.rows_m1];
            //    Task[] mTasks = new Task[clientRows.Length];
            //    for (int i = 0; i < clientRows.Length; i++)
            //    {
            //        int aux_i = i;
            //        mTasks[aux_i] = new Task(() => executeRowMultiplication(matrix1, matrix2, p.rows_m1, p.columns_m1, p.separator, clientRows[aux_i]));
            //    }

            //    foreach (Task t in mTasks)
            //        t.Start();
            //}

            /*foreach (Task t in mTasks)
                t.Start();

            Task.WaitAll(mTasks);


            RowResult[] sortedMatrix = MatrixResult.OrderBy(s => s.rowNumber).ToArray();
            using (StreamWriter sw = File.CreateText(p.path_result))
            {
                for (int i = 0; i < sortedMatrix.Length; i++)
                {
                    sw.WriteLine(sortedMatrix[i].row);
                }
            }*/
            //stopwatch.Stop();
            //return stopwatch.ElapsedMilliseconds;
        }
        private static void executeRowMultiplication(List<string> matrix1, List<string> matrix2, int rows_m1, int columns_m1, char separator, int rowIndex_m1)
        {
            //string matrix1_row = File.ReadLines(path1).Skip(rowIndex_m1).Take(1).First();
            string matrix1_row = (string)matrix1[rowIndex_m1];
            var m1_rowElements = matrix1_row.Split(separator).Select(Int32.Parse).ToArray();
            StringBuilder sbRowResult = new StringBuilder();
            //using (StreamReader sr2 = File.OpenText(path2))
            {
                for (int i_m2 = 0; i_m2 < rows_m1; i_m2++)
                {
                    //string matrix2_row = sr2.ReadLine();
                    string matrix2_row = matrix2[i_m2];
                    var m2_rowElements = matrix2_row.Split(separator).Select(Int32.Parse).ToArray();
                    int mr_element = 0;
                    for (int j = 0; j < columns_m1; j++)
                    {
                        mr_element += m1_rowElements[j] * m2_rowElements[j];
                    }
                    sbRowResult.Append(mr_element);
                    sbRowResult.Append(separator);
                }
            }
            sbRowResult.Length -= 1;
            matrixResults[rowIndex_m1] = sbRowResult.ToString();
            //RowResult rowResult = new RowResult(rowIndex_m1,sbRowResult.ToString());
            //lock (syncLock)
            //{
            //    MatrixResult.Add(rowResult);
            //}
        }

        private static void startUpdateGUI()
        {
            updatingUI = new Thread(updateGUI);
            updatingUI.Start();
        }
        private static void updateGUI()
        {
            for(;;)
            {
                Thread.Sleep(1000);
                ArrayList details = Client.server.getDetails();
                if(details != null)
                {
                    MainWindow.lstDetails.ClearOnUI();
                    foreach (string detail in details)
                        MainWindow.lstDetails.AddOnUI(detail);
                }
            }
        }

        private static List<string> loadMatrix(string path_source)
        {
            List<string> loadedMatrix = null;
            using (StreamReader sr = File.OpenText(path_source))
            {
                string source_line = "";
                loadedMatrix = new List<string>();
                while ((source_line = sr.ReadLine()) != null)
                {
                    loadedMatrix.Add(source_line);
                }
            }
            return loadedMatrix;
        }
        private static List<string> transpose(string path_source, long rows, long columns, char separator)
        {
            List<string> transposedMatrix = null;
            using (StreamReader sr = File.OpenText(path_source))
            {
                //Inizialite StringBuilder array
                StringBuilder[] sbTempFile = new StringBuilder[columns];
                for (int i = 0; i < columns; i++)
                    sbTempFile[i] = new StringBuilder();

                //Retrieving lines of source file. Each line will append a new number to each StringBuilder array element
                string source_line = "";
                while ((source_line = sr.ReadLine()) != null)
                {
                    string[] split_Line = source_line.Split(separator);
                    for (int i = 0; i < columns; i++)
                    {
                        sbTempFile[i].Append(split_Line[i]);
                        sbTempFile[i].Append(separator);
                    }
                }

                transposedMatrix = new List<string>();
                for (int i = 0; i < columns; i++)
                {
                    sbTempFile[i].Length -= 1;
                    transposedMatrix.Add(sbTempFile[i].ToString());
                    sbTempFile[i] = null;
                }
            }
            return transposedMatrix;
        }
    }
}
