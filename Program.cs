﻿using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace HelloNeuralNet
{
    class Program
    {
        const int image_size = 28;
        const double max_neuron_val = 1000.0;

        const string train_data_CSV = "C:\\Users\\Mitch\\source\\repos\\HelloNeuralNet\\mnist\\CSV\\train";
        const string test_data_CSV = "C:\\Users\\Mitch\\source\\repos\\HelloNeuralNet\\mnist\\CSV\\test";
        const string train_data_JSON = "C:\\Users\\Mitch\\source\\repos\\HelloNeuralNet\\mnist\\JSON\\train.json";
        const string test_data_JSON = "C:\\Users\\Mitch\\source\\repos\\HelloNeuralNet\\mnist\\JSON\\test.json";
        const string save_data = "C:\\Users\\Mitch\\source\\repos\\HelloNeuralNet\\mnist\\JSON";

        static List<MNIST_Element> train_set = new List<MNIST_Element>();
        static List<MNIST_Element> test_set = new List<MNIST_Element>();
        static NeuralNet helloNet = null;

        static double succrate = 0.0;

        public static void saveToFile<T>(T to_save, string file_name)
        {
            string json_output = JsonConvert.SerializeObject(to_save, Formatting.None);
            File.WriteAllText(file_name, json_output);
        }
        public static T loadFromFile<T>(string file_name)
        {
            string json_input = File.ReadAllText(file_name);
            T toReturn = JsonConvert.DeserializeObject<T>(json_input);
            return toReturn;
        }

        // Transforms the MNIST dataset from the extracted CSV dataset (see mnistCSV.py)
        // Scans all files in the input folder, converts the CSV file into MNIST_Elements and saves the element array to a JSON file
        public static void mnist_transform(string csv_folderpath, string save_filepath)
        {
            List<MNIST_Element> elements = new List<MNIST_Element>();


            string[] filepaths = Directory.GetFiles(csv_folderpath, "*.csv");
            foreach (string filepath in filepaths)
            {
                if (File.Exists(filepath))
                {
                    StreamReader reader = new StreamReader(File.OpenRead(filepath));

                    try
                    {
                        string number_str = reader.ReadLine();
                        int number_int = int.Parse(number_str);
                        int[,] image = new int[image_size, image_size];

                        // I feel like this has the same problem as feof() but idk
                        int count = 0;
                        while (!reader.EndOfStream && count < image_size)
                        {
                            string line = reader.ReadLine();
                            string[] values = line.Split(',');
                            if (values.Length != image_size)
                            {
                                throw new DivideByZeroException();
                            }

                            for (int x = 0; x < image_size; x++)
                            {
                                image[x, count] = int.Parse(values[x]);
                            }
                            count++;

                        }
                        if (count != image_size)
                        {
                            throw new DivideByZeroException();
                        }

                        MNIST_Element toAdd = new MNIST_Element(number_int, image);

                        elements.Add(toAdd);

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
            saveToFile(elements, save_filepath);
        }
        static void generate_JSON_MNIST()
        {
            mnist_transform(test_data_CSV, test_data_JSON);
            mnist_transform(train_data_CSV, train_data_JSON);
        }

        static void load_mnist()
        {
            Console.WriteLine("Loading MNIST dataset...");
            train_set = loadFromFile<List<MNIST_Element>>(train_data_JSON);
            test_set = loadFromFile<List<MNIST_Element>>(test_data_JSON);
            Console.WriteLine("MNIST dataset loaded\n");

        }
        static void print_mnist(MNIST_Element toPrint)
        {
            Console.WriteLine("\nNumber: " + toPrint.value.ToString());
            Console.WriteLine("--------------------------------------");
            for (int y = 0; y < image_size; y++)
            {
                for (int x = 0; x < image_size; x++)
                {
                    Console.Write((toPrint.image[x, y] > 40) ? "." : "X");
                }
                Console.WriteLine("");
            }
        }
        static int force_int_input()
        {
            string input;
            int int_input = 0;
            while(true)
            {
                try
                {
                    input = Console.ReadLine();
                    if (input == "quit" || input == "exit")
                    {
                        System.Environment.Exit(0);
                    }
                    int_input = int.Parse(input);
                    return int_input;

                }
                catch (Exception)
                {
                    Console.WriteLine("Please enter a valid integer");
                }
            }
        }

        public static void randomize_network(NeuralNet toRandomize, int lower_bound, int upper_bound)
        {
            Random rnd = new Random();
            rnd.Next(0, 100);
            Layer prev_layer = null;
            foreach(Layer l in toRandomize.layers)
            {
                if(prev_layer != null)
                {
                    foreach (Neuron n in l.neurons)
                    {
                        int num_weights = prev_layer.neurons.Count;
                        n.weights = new double[num_weights];
                        n.nudge = 0;

                        for (int i = 0; i < num_weights; i++)
                        {
                            n.weights[i] = rnd.Next(lower_bound * 100 , upper_bound * 100) / 100.00;
                        }
                    }
                }
                prev_layer = l;
            }
        }
        public static double ReLU(double input)
        {
            if(input < 0)
            {
                return 0;
            }
            else if(input < max_neuron_val)
            {
                return input;
            }
            else
            {
                return max_neuron_val;
            }
        }

        public static int get_network_output(NeuralNet finished)
        {
            int end_index = finished.layers.Count - 1;
            int num_end_neurons = finished.layers[end_index].neurons.Count;
            int max_index = 0;
            for (int i = 0; i < num_end_neurons; i++)
            {
                if (finished.layers[end_index].neurons[i].value > finished.layers[end_index].neurons[max_index].value)
                {
                    max_index = i;
                }
            }
            return max_index;
        }
        public static void reset_backwards_prop(NeuralNet toTrain)
        {
            foreach (Layer l in toTrain.layers)
            {
                foreach (Neuron n in l.neurons)
                {
                    n.nudge = 0.0;
                }
            }
        }
        public static void backwards_prop_add(NeuralNet toTrain, MNIST_Element element)
        {
            int end_index = toTrain.layers.Count - 1;
            int num_end_neurons = toTrain.layers[end_index].neurons.Count;
            int what_we_want = element.value;
            int network_output = get_network_output(toTrain);
            for (int i = 0; i < num_end_neurons; i++)
            {
                // IMPORTANT
                if(i == what_we_want)
                {
                    toTrain.layers[end_index].neurons[i].nudge = (max_neuron_val) - toTrain.layers[end_index].neurons[i].value;
                }
                else
                {
                    toTrain.layers[end_index].neurons[i].nudge = 0 - toTrain.layers[end_index].neurons[i].value;
                }
            }
        }

        public static void backwards_prop(NeuralNet toTrain)
        {
            int end_layer = toTrain.layers.Count - 1;
            for (int i = end_layer - 1; i > 0; i--)
            {
                foreach(Neuron n in toTrain.layers[i + 1].neurons)
                {
                    int numweights = n.weights.Length;
                    double avg_bump = n.nudge / max_neuron_val;
                    double thousandth = (1.00 / 1000.00) * max_neuron_val;
                    for (int j = 0; j < numweights; j++)
                    {
                        
                        n.weights[j] += (toTrain.layers[i].neurons[j].value / max_neuron_val) * avg_bump;
                        toTrain.layers[i].neurons[j].nudge += avg_bump;
                        n.bias += 1 / 1000 * avg_bump;
                    }
                }
            }
        }
        public static void forward_prop(NeuralNet toTrain, MNIST_Element element)
        {

            Layer prev_layer = null;
            foreach (Layer l in toTrain.layers)
            {
                // Continue propagation
                if (prev_layer != null)
                {
                    foreach (Neuron n in l.neurons)
                    {
                        double sum = 0;
                        int num_weights = n.weights.Length;
                        for (int i = 0; i < num_weights; i++)
                        {
                            sum += n.weights[i] * prev_layer.neurons[i].value;
                        }
                        n.value = ReLU(sum + n.bias);
                    }
                }
                // Initialize layer 0
                else
                {
                    int xsize = element.image.GetLength(0);
                    int ysize = element.image.GetLength(1);

                    for (int y = 0; y < ysize; y++)
                    {
                        for (int x = 0; x < xsize; x++)
                        {
                            l.neurons[(xsize * y) + x].value = element.image[x, y];
                        }
                    }
                }
                prev_layer = l;
            }
        }
        public static void batch_train(NeuralNet toTrain, int steps)
        {
            int num_tests = train_set.Count;
            if (num_tests <= 0)
            {
                return;
            }

            Random rnd = new Random();

            double succ_rate = 0.0;
            for(int j = 0; j < steps; j++)
            {
                MNIST_Element test_num = train_set[rnd.Next(0, num_tests)];
                forward_prop(toTrain, test_num);
                backwards_prop_add(toTrain, test_num);
                backwards_prop(toTrain);
                reset_backwards_prop(toTrain);
            }

            for (int j = 0; j < steps; j++)
            {
                MNIST_Element test_num = train_set[rnd.Next(0, num_tests)];
                forward_prop(toTrain, test_num);
                //Console.WriteLine("NN RESULT: " + max_index.ToString());
                //Console.WriteLine("ACTUAL NUMBER: " + test_num.value.ToString());
                if(j == steps - 1)
                {
                    //Console.WriteLine(get_network_output(toTrain).ToString());
                }
                
                if (get_network_output(toTrain) == test_num.value)
                {
                    succ_rate += 1.0;
                }
            }
            succ_rate = succ_rate / steps;
            succrate += succ_rate;


            //Console.WriteLine("Success rate over " + steps.ToString() + " steps = "+ succ_rate.ToString());

        }
        static void Main(string[] args)
        {
            load_mnist();
            while (true)
            {
                Console.WriteLine("\nChoose an Option:");
                Console.WriteLine("1. New Neural Network");
                Console.WriteLine("2. Train Neural Network");
                Console.WriteLine("3. Test Neural Network");
                Console.WriteLine("4. Exit");
                string input = Console.ReadLine();
                switch (input)
                {
                    case "1":
                        Console.WriteLine("How many hidden layers?");
                        int num_layers = force_int_input();
                        if(num_layers <= 0)
                        {
                            break;
                        }
                        helloNet = new NeuralNet(2 + num_layers);

                        int[] num_neurons = new int[2 + num_layers];
                        num_neurons[0] = image_size * image_size;
                        num_neurons[1 + num_layers] = 10;
                        for (int i = 0; i < num_layers; i++)
                        {
                            Console.WriteLine("Neurons in hidden layer " + (i + 1).ToString() +"?");
                            num_neurons[i + 1] = force_int_input();
                            
                        }

                        helloNet.create_all_neurons(num_neurons);

                        // Weight Randomization
                        Console.WriteLine("Lower bound of weight randomization?");
                        int rand_lower = force_int_input();
                        Console.WriteLine("Upper bound of weight randomization?");
                        int rand_upper = force_int_input();
                        if(rand_lower > rand_upper)
                        {
                            break;
                        }
                        randomize_network(helloNet, rand_lower, rand_upper);

                        saveToFile(helloNet, "C:\\Users\\Mitch\\Desktop\\helloNet.json");
                        
                        break;
                    case "2":
                        for (int y = 0; y < 20; y++)
                        {
                            succrate = 0;
                            int z = 0;
                            for (z = 0; z < 100; z++)
                            {
                                batch_train(helloNet, 2000);

                            }
                            Console.WriteLine("Success Rate of last " + (z).ToString() + "Trials: " + (succrate / (double)(z)).ToString());
                            saveToFile(helloNet, "C:\\Users\\Mitch\\Desktop\\trained_owo.json");
                        }
                        break;
                    case "3":
                        break;
                    case "4" or "exit" or "quit":
                        System.Environment.Exit(0);
                        break;

                }
            }
        }

        public class MNIST_Element
        {

            public int value;
            public int[,] image;
            public MNIST_Element(int value, int[,] image)
            {
                this.value = value;
                this.image = image;
            }

        }

        public class NeuralNet
        {
            public List<Layer> layers = new List<Layer>();
            public NeuralNet(int num_layers)
            {
                for (int i = 0; i < num_layers; i++)
                {
                    this.layers.Add(new Layer());
                }
            }

            public void create_all_neurons(int[] values)
            {
                if(values.Length == layers.Count)
                {
                    for(int i = 0; i < values.Length; i++)
                    {
                        this.layers[i].define_layer_neurons(values[i]);
                    }
                }
            }
        }

        public class Layer
        {
            public List<Neuron> neurons = new List<Neuron>();
            public Layer()
            {

            }
            public void define_layer_neurons(int num_neurons)
            {
                for(int i = 0; i < num_neurons; i++)
                {
                    this.neurons.Add(new Neuron());
                }
            }
        }

        public class Neuron
        {
            public double value;
            public double[] weights = null;
            public double bias;
            public double nudge;
            public Neuron(int value)
            {
                this.value = value;
            }
            public Neuron()
            {

            }
        }
    }
}

