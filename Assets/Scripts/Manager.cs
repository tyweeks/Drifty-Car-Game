using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using UnityEngine;

public class Manager : MonoBehaviour {

    public GameObject carPrefab;
    public GameObject checkPointPrefab;
    public float roundTime  = 60;
    public string netId = "dc001";
    public string netDesc = "Drifty Car Neural Net";

    private string loadWeights = "";
    private bool isTraining = false;
    private int populationSize = 50;
    public int generationNumber = 0;
    private int[] layers = new int[] { 10, 10, 10, 4 }; //10 input and 4 output
    private List<NeuralNetwork> nets;
    private List<CarPhysics> CarList = null;
    private float bestFitness = 0.0f;

    void Start()
    {
        Instantiate(checkPointPrefab, new Vector3(-40.0f, 0.3f, 40.0f), checkPointPrefab.transform.rotation);
        GetNetFromWeb();
    }

    void Timer()
    {
        isTraining = false;
    }


	void Update ()
    {
        if (isTraining == false)
        {
            if (generationNumber == 0)
            {
                InitCarNeuralNetworks();
            }
            else
            {
                nets.Sort();
                DisplayNetInfo();

                for (int i = 0; i < populationSize / 2; i++)
                {
                    nets[i] = new NeuralNetwork(nets[i+(populationSize / 2)]);
                    nets[i].Mutate();

                    nets[i + (populationSize / 2)] = new NeuralNetwork(nets[i + (populationSize / 2)]); //too lazy to write a reset neuron matrix values method....so just going to make a deepcopy lol
                }

                for (int i = 0; i < populationSize; i++)
                {
                    nets[i].SetFitness(0f);
                }
            }

           
            generationNumber++;
            
            isTraining = true;
            Invoke("Timer",roundTime);
            CreateCars();
        }
    }


    private void CreateCars()
    {
        if (CarList != null)
        {
            for (int i = 0; i < CarList.Count; i++)
            {
                if (CarList[i] != null)
                {
                    GameObject.Destroy(CarList[i].gameObject);
                }
            }

        }

        CarList = new List<CarPhysics>();

        for (int i = 0; i < populationSize; i++)
        {
            Vector3 spawnLocation = new Vector3(-30.0f + UnityEngine.Random.Range(-3f, 3f), 0.3f, 42.0f + UnityEngine.Random.Range(-5f, 5f));
            CarPhysics car = ((GameObject)Instantiate(carPrefab, spawnLocation, carPrefab.transform.rotation)).GetComponent<CarPhysics>();

            car.Init(nets[i]);
            CarList.Add(car);
        }

    }

    void InitCarNeuralNetworks()
    {
        //population must be even, just setting it to 20 incase it's not
        if (populationSize % 2 != 0)
        {
            populationSize = 20;
        }

        nets = new List<NeuralNetwork>();
        

        for (int i = 0; i < populationSize; i++)
        {
            NeuralNetwork net = new NeuralNetwork(layers);
            if (i == 0 && loadWeights != "")
            {
                net.LoadWeights(loadWeights);
            }
            else
            {
                net.Mutate();
            }
            nets.Add(net);
        }
    }

    void DisplayNetInfo()
    {
        float bestGenFitness = nets[populationSize - 1].GetFitness();
        float avgFitness = 0.0f;

        if (bestGenFitness > bestFitness)
        {
            bestFitness = bestGenFitness;
            PostNetToWeb();
        }

        for (int i = 0; i < populationSize; i++)
        {
            avgFitness += nets[i].GetFitness();
        }

        avgFitness /= populationSize;

        Debug.Log(nets[populationSize - 1].GetWeights());
        Debug.Log("Best fitness (gen): " + bestGenFitness.ToString("F2") + " Avg fitness: " + avgFitness.ToString("F2") + " Best fitness (overall): " + bestFitness.ToString("F2"));
    }

    void WriteBestNet(NeuralNetwork net)
    {
        string path = "\\\\studioserver\\Plex\\fitness.txt";
        StreamWriter writer = new StreamWriter(path, false);
        writer.WriteLine("Fitness: " + net.GetFitness());
        writer.WriteLine(net.GetWeights());
        writer.Close();
    }

    private void GetNetFromWeb()
    {
        HttpClient client = new HttpClient();
        client.BaseAddress = new System.Uri("http://codetweeks.com/api/neuralnetwork/");
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        HttpResponseMessage response = client.GetAsync(netId).Result;
        if (response.IsSuccessStatusCode)
        {
            NeuralNetworkObject neuralNetworkObject = JsonUtility.FromJson<NeuralNetworkObject>(response.Content.ReadAsStringAsync().Result);
            loadWeights = neuralNetworkObject.weights;
            bestFitness = neuralNetworkObject.fitness;
            netDesc = neuralNetworkObject.description;
        }
    }

    private void PostNetToWeb()
    {
        NeuralNetworkObject neuralNetworkObject = new NeuralNetworkObject { id = netId, description = netDesc, fitness = bestFitness, weights = nets[populationSize - 1].GetWeights() };

        HttpClient client = new HttpClient();
        client.BaseAddress = new System.Uri("http://codetweeks.com/api/neuralnetwork/");
        Debug.Log(JsonUtility.ToJson(neuralNetworkObject));
        HttpResponseMessage response = client.PostAsync(netId, new StringContent(JsonUtility.ToJson(neuralNetworkObject))).Result;
        Debug.Log(response.StatusCode);
        Debug.Log(response.Content.ReadAsStringAsync().Result);
    }

    private class NeuralNetworkObject
    {
        public string id;
        public string description;
        public float fitness;
        public string weights;
    }
}
