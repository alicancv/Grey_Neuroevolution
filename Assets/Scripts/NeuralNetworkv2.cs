using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Bitirme2
{
    public class NeuralNetworkv2
    {
        public double learning_rate;

        public int[][] network_structure;

        public Matrix[] weights;
        public Matrix[] biases;

        public NeuralNetworkv2(NeuralNetworkv2 a)
        {
            learning_rate = a.learning_rate;

            network_structure = a.network_structure;

            weights = new Matrix[a.weights.Length];
            biases = new Matrix[a.biases.Length];

            for (int i = 0; i < weights.Length; i++)
            {
                weights[i] = a.weights[i].copy();
            }

            for (int i = 0; i < biases.Length; i++)
            {
                biases[i] = a.biases[i].copy();
            }
        }

        public NeuralNetworkv2(int[] hidden_layers, int input_nodes, int output_nodes, double learning_rate)
        {
            network_structure = new int[hidden_layers.Length + 2][];
            network_structure[0] = new int[input_nodes];
            network_structure[network_structure.Length - 1] = new int[output_nodes];

            for (int i = 1; i < (network_structure.Length - 1); i++)
            {
                network_structure[i] = new int[hidden_layers[i - 1]];
            }

            weights = new Matrix[network_structure.Length - 1];
            biases = new Matrix[network_structure.Length - 1];


            for (int i = 0; i < network_structure.Length - 1; i++)
            {
                weights[i] = new Matrix(network_structure[i + 1].Length, network_structure[i].Length);
                biases[i] = new Matrix(network_structure[i + 1].Length, 1);

                weights[i].randomize();
                biases[i].randomize();
            }

                this.learning_rate = learning_rate;
        }

        //public NeuralNetworkv2(int input_nodes, int hidden_nodes, int output_nodes, double learning_rate)
        //{
        //    this.input_nodes = input_nodes;
        //    this.hidden_nodes = hidden_nodes;
        //    this.output_nodes = output_nodes;

        //    weights_ih = new Matrix(this.hidden_nodes, this.input_nodes);
        //    weights_ho = new Matrix(this.output_nodes, this.hidden_nodes);

        //    weights_ih.randomize();
        //    weights_ho.randomize();

        //    bias_h = new Matrix(this.hidden_nodes, 1);
        //    bias_o = new Matrix(this.output_nodes, 1);

        //    bias_h.randomize();
        //    bias_o.randomize();

        //    this.learning_rate = learning_rate;
        //}

        public static double sigmoid(double x)
        {
            return 1 / (1 + Math.Exp(-1 * x));
        }

        public static double tanh(double x)
        {
            return Math.Tanh(x);
        }

        public Matrix predict(Matrix input)
        {
            Matrix[] hiddens = new Matrix[network_structure.Length - 1];

            hiddens[0] = weights[0].Dot(input);
            hiddens[0] = hiddens[0].Addition(biases[0]);
            hiddens[0] = hiddens[0].map(sigmoid);

            for (int i = 1; i < hiddens.Length; i++)
            {
                hiddens[i] = weights[i].Dot(hiddens[i-1]);
                hiddens[i] = hiddens[i].Addition(biases[i]);
                hiddens[i] = hiddens[i].map(sigmoid);
            }

            return hiddens[hiddens.Length - 1];
        }

        //public double[] feedForward(double[] input_array)
        //{
        //    Matrix input = Matrix.fromArray(input_array);

        //    Matrix hidden = weights_ih.Multiply(input);
        //    hidden = hidden.Addition(bias_h);
        //    hidden.map(sigmoid);

        //    Matrix output = weights_ho.Multiply(hidden);
        //    output = output.Addition(bias_o);
        //    output.map(sigmoid);

        //    return output.toArray();
        //}

        public void train(Matrix input, Matrix target_matrix)
        {
            Matrix[] hiddens = new Matrix[network_structure.Length - 1];

            hiddens[0] = weights[0].Dot(input);
            hiddens[0] = hiddens[0].Addition(biases[0]);
            hiddens[0] = hiddens[0].map(sigmoid);

            for (int i = 1; i < hiddens.Length; i++)
            {
                hiddens[i] = weights[i].Dot(hiddens[i - 1]);
                hiddens[i] = hiddens[i].Addition(biases[i]);
                hiddens[i] = hiddens[i].map(sigmoid);
            }

            Matrix[] errors = new Matrix[hiddens.Length];

            errors[errors.Length - 1] = target_matrix.Addition(hiddens[hiddens.Length - 1].scalarMultiply(-1));
            
            Matrix gradient = calculate_gradient(new Matrix[2] { hiddens[hiddens.Length - 2], hiddens[hiddens.Length - 1] }, errors[errors.Length - 1]); //GRADİENTLER UCUOR DİKKAT

            biases[hiddens.Length - 1] = biases[hiddens.Length - 1].Addition(gradient);

            Matrix delta = gradient.Dot(hiddens[hiddens.Length - 2].transpose());

            Matrix[] new_weights = new Matrix[network_structure.Length - 1];

            new_weights[new_weights.Length - 1] = weights[weights.Length - 1].Addition(delta);


            //bias_o = bias_o.Addition(gradient);

            //Matrix delta = gradient.Multiply(hidden_2_layer[0].transpose());

            //Matrix new_weights_ho = weights_ho.Addition(delta);


            for (int i = (hiddens.Length - 2); i > 0; i--)
            {
                errors[i] = weights[i + 1].transpose().Dot(errors[i + 1]);

                gradient = calculate_gradient(new Matrix[2] { hiddens[i - 1], hiddens[i] }, errors[i]); //ilk hidden olmucak dikkat!!!!!!!!!!!!!!!!!!!

                biases[i] = biases[i].Addition(gradient);

                delta = gradient.Dot(hiddens[i - 1].transpose());

                new_weights[i] = weights[i].Addition(delta);
            }

            errors[0] = weights[1].transpose().Dot(errors[1]);

            gradient = calculate_gradient(new Matrix[2] { input, hiddens[0] }, errors[0]);

            biases[0] = biases[0].Addition(gradient);

            delta = gradient.Dot(input.transpose());

            new_weights[0] = weights[0].Addition(delta);

            for (int i = 0; i < weights.Length; i++)
            {
                weights[i] = new_weights[i].copy();
            }
            
        }

        public Matrix calculate_gradient(Matrix[] hidden_2_layer, Matrix error)
        {
            Matrix gradient = hidden_2_layer[1].map((y) => { return y * (1 - y); }).elementwiseMultiply(error); //Elementwise
            gradient = gradient.scalarMultiply(learning_rate);

            return gradient;
        }

        public NeuralNetworkv2 copy()
        {
            return new NeuralNetworkv2(this);  
        }

        public void mutate(double rate)
        {
            double mutate(double x)
            {
                if (UnityEngine.Random.Range(0f, 1f) < rate)
                {
                    x += UnityEngine.Random.Range(-1f, 1f);
                }

                return x;
            }

            for (int i = 0; i < weights.Length; i++)
            {
                weights[i] = weights[i].map(mutate);
            }

            for (int i = 0; i < biases.Length; i++)
            {
                biases[i] = biases[i].map(mutate);
            }
        }


        public void crossover(NeuralNetworkv2 crossoverWith, double crossoverRate)
        {
            for (int i = 0; i < weights.Length; i++)
            {
                for (int j = 0; j < weights[i].row; j++)
                {
                    for (int k = 0; k < weights[i].column; k++)
                    {
                        if (UnityEngine.Random.Range(0f, 1f) < crossoverRate)
                        {
                            weights[i].matrix[j, k] = crossoverWith.weights[i].matrix[j, k];
                        }
                    }
                }
            }

            for (int i = 0; i < biases.Length; i++)
            {
                for (int j = 0; j < biases[i].row; j++)
                {
                    for (int k = 0; k < biases[i].column; k++)
                    {
                        if (UnityEngine.Random.Range(0f, 1f) < crossoverRate)
                        {
                            biases[i].matrix[j, k] = crossoverWith.biases[i].matrix[j, k];
                        }
                    }
                }
            }
        }

        public static void textToNn(string matrixText, Matrix[] matrixArray)
        {
            int row = 0;
            int column = 0;
            string temp = "";

            int weightIndex = 0;

            for (int k = 0; k < matrixText.Length; k++)
            {
                if (matrixText[k] != ' ' && matrixText[k] != '\n' && matrixText[k] != '*')
                {
                    temp += matrixText[k];
                }
                else if (matrixText[k] == ' ')
                {
                    matrixArray[weightIndex].matrix[row, column] = Convert.ToDouble(temp);
                    temp = "";
                    column++;
                }
                else if (matrixText[k] == '\n')
                {
                    column = 0;
                    if (matrixText[k - 1] == '*')
                    {
                        row = 0;
                    }
                    else
                    {
                        row++;
                    }
                }
                else if (matrixText[k] == '*')
                {
                    weightIndex++;
                }
            }
        }
    }
}
