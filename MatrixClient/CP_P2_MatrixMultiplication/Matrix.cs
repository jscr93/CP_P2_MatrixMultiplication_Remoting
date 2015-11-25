﻿using System;
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
        private List<string> matrixResults = new List<string>(); // Holds text for matrix
        private static Thread parallelThread;

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

        //private class RowResult
        //{
        //    public string row;
        //    public long rowNumber;
        //}

        private static readonly object syncLock = new object();
        private static List<RowResult> MatrixResult;


        
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

            //Upload matrix to server
            for (int i = 0; i < p.rows_m1; i++)
            {
                Client.server.AddSourceRow(new RowsToMultiply(i, matrix1[i], matrix2[i]));
            }

            //int rowGroups = p.rows_m1 / Client.server.getClientsNumber();

            Client.server.AddClient("Cliente2");
            Client.server.AddClient("Cliente3");
            Client.server.AddClient("Cliente4");

            int a = Client.server.DispatchRowGroupsToClients(p.rows_m1);
            int b = 0;
            //MatrixResult = new List<RowResult>();
            //Task[] mTasks = new Task[p.rows_m1];
            //for( int i = 0; i < p.rows_m1; i++)
            //{
            //    int a = i;
            //    mTasks[a] = new Task(() => executeRowMultiplication(matrix1, matrix2, p.rows_m1, p.columns_m1, p.separator, a));
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

        //private static void executeRowMultiplication(ArrayList matrix1, ArrayList matrix2, int rows_m1, int columns_m1, char separator, int rowIndex_m1)
        //{
        //    RowResult rowResult = new RowResult();
        //    //string matrix1_row = File.ReadLines(path1).Skip(rowIndex_m1).Take(1).First();
        //    string matrix1_row = (string)matrix1[rowIndex_m1];
        //    var m1_rowElements = matrix1_row.Split(separator).Select(Int32.Parse).ToArray();
        //    StringBuilder sbRowResult = new StringBuilder();
        //    //using (StreamReader sr2 = File.OpenText(path2))
        //    {
        //        for (int i_m2 = 0; i_m2 < rows_m1; i_m2++)
        //        {
        //            //string matrix2_row = sr2.ReadLine();
        //            string matrix2_row = (string)matrix2[i_m2];
        //            var m2_rowElements = matrix2_row.Split(separator).Select(Int32.Parse).ToArray();
        //            int mr_element = 0;
        //            for (int j = 0; j < columns_m1; j++)
        //            {
        //                mr_element += m1_rowElements[j] * m2_rowElements[j];
        //            }
        //            sbRowResult.Append(mr_element);
        //            sbRowResult.Append(separator);
        //        }
        //    }
        //    sbRowResult.Length -= 1;
        //    rowResult.row = sbRowResult.ToString();
        //    rowResult.rowNumber = rowIndex_m1;
        //    lock (syncLock) {
        //        MatrixResult.Add(rowResult);
        //    } 
        //}

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