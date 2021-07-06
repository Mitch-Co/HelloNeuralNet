using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace HelloNeuralNet
{
    class Program
    {
        const string train_data_CSV = "C:\\Users\\Mitch\\source\\repos\\HelloNeuralNet\\mnist\\CSV\\train";
        const string test_data_CSV = "C:\\Users\\Mitch\\source\\repos\\HelloNeuralNet\\mnist\\CSV\\test";
        const string train_data_JSON = "C:\\Users\\Mitch\\source\\repos\\HelloNeuralNet\\mnist\\CSV\\train";
        const string test_data_JSON = "C:\\Users\\Mitch\\source\\repos\\HelloNeuralNet\\mnist\\CSV\\test";
        const string save_data = "C:\\Users\\Mitch\\source\\repos\\HelloNeuralNet\\mnist\\JSON";

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
        // Scans all files in the input folders, converts them to 
        public static void mnist_transform(string csv_folderpath, string save_filepath)
        {
            const int image_size = 28;
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
                            if(values.Length != image_size)
                            {
                                throw new DivideByZeroException();
                            }

                            for(int x = 0; x < image_size; x++)
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

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            mnist_transform(test_data, save_data + "\\test.json");
        }
    }

    public class MNIST_Element {

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
        private List<Layer> layers = new List<Layer>();
        public NeuralNet()
        {

        }
    }

    public class Layer
    {
        public List<Neuron> neurons = new List<Neuron>();
        public Layer()
        {

        }
    }

    public class Neuron
    {
        public string value { get; set; }
        public Neuron(string yeet)
        {
            this.value = yeet;
        }
        public Neuron()
        {

        }
    }
}

