using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Bitirme2
{
    public class Matrix
    {
        public int row;
        public int column;
        public double[,] matrix;

        public Matrix(int row, int column)
        {
            this.row = row;
            this.column = column;
            matrix = new double[row, column];
        }

        public static Matrix fromArray(double[] input_array)
        {
            Matrix a = new Matrix(input_array.Length, 1);

            for (int i = 0; i < input_array.Length; i++)
            {
                a.matrix[i, 0] = input_array[i];
            }

            return a;
        }

        public Matrix scalarMultiply(double x)
        {
            Matrix a = new Matrix(row, column);
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    a.matrix[i, j] = matrix[i, j] * x;
                }
            }
            return a;
        }
        public Matrix elementwiseMultiply(Matrix x)
        {
            try
            {
                if(row == x.matrix.GetLength(0) && column == x.matrix.GetLength(1))
                {
                    Matrix a = new Matrix(row, column);
                    for (int i = 0; i < matrix.GetLength(0); i++)
                    {
                        for (int j = 0; j < matrix.GetLength(1); j++)
                        {
                            a.matrix[i, j] = matrix[i, j] * x.matrix[i, j];
                        }
                    }
                    return a;
                }
                else
                {
                    throw new Exception("For elementwise multiplication matrices must have same dimesions.");
                }

            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public Matrix Dot(Matrix x)
        {
            try
            {
                if (column == x.matrix.GetLength(0))
                {
                    Matrix a = new Matrix(row, x.matrix.GetLength(1));
                    double sum = 0;
                    for (int i = 0; i < row; i++)
                    {
                        for (int j = 0; j < x.matrix.GetLength(1); j++)
                        {
                            for (int k = 0; k < column; k++)
                            {
                                sum += matrix[i, k] * x.matrix[k, j];
                            }
                            a.matrix[i, j] = sum;
                            sum = 0;
                        }
                    }
                    return a;
                }
                else
                {
                    throw new Exception("For dot product column of first matrix must be same as row of second matrix.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }

        }

        public static Matrix Pow(Matrix x, int power)
        {
            if (power == 2)
            {
                return x.Dot(x);
            }

            return x.Dot(Pow(x, power - 1));
        }

        public void Addition(double x)
        {
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    matrix[i, j] += x;
                }
            }
        }

        public Matrix Addition(Matrix x)
        {
            try
            {
                if (row != x.matrix.GetLength(0) || column != x.matrix.GetLength(1))
                {
                    throw new Exception("Matrices must have same dimesions");
                }

                Matrix a = new Matrix(row, column);
                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    for (int j = 0; j < matrix.GetLength(1); j++)
                    {
                        a.matrix[i, j] = matrix[i, j] + x.matrix[i, j];
                    }
                }
                return a;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public string printMatrix()
        {
            string t = "";
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    t += matrix[i, j] + " ";
                }
                t += "\n";
            }
            t += "*";
            return t;
        }

        public void randomize()
        {
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    matrix[i, j] = UnityEngine.Random.Range(-1f, 1f);
                }
            }
        }

        public Matrix copy()
        {
            Matrix a = new Matrix(row, column);
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    a.matrix[i, j] = matrix[i, j];
                }
            }
            return a;
        }

        public Matrix transpose()
        {
            Matrix a = new Matrix(column, row);

            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    a.matrix[j, i] = matrix[i, j];
                }
            }
            return a;
        }

        public Matrix map(Func<double/*, int, int*/, double> mapFunc)
        {
            Matrix a = new Matrix(row, column);
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    a.matrix[i, j] = mapFunc(matrix[i, j]);
                }
            }
            return a;
        }
    }
}
