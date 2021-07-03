using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace HelloNeuralNet
{
    class Program
    {
        const string train_data = "";
        const string test_data = "";
        const string save_data = "";

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

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            NeuralNet nutnet = new NeuralNet();
            nutnet.input_layer = new Layer();
            nutnet.input_layer.neurons.Add(new Neuron("Test Neuron Says Hello"));

            saveToFile(nutnet, "C:\\Users\\Mitch\\Desktop\\yote.json");
            NeuralNet nutnet2 = loadFromFile<NeuralNet>("C:\\Users\\Mitch\\Desktop\\yote.json");
            saveToFile(nutnet, "C:\\Users\\Mitch\\Desktop\\yote.json");
        }
    }

    public class NeuralNet
    {
        public Layer input_layer = null;
        private List<Layer> hidden_layers = new List<Layer>();
        public Layer output_layer = null;
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

