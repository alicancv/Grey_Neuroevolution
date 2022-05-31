using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Bitirme2;
using System.IO;
using UnityEngine.InputSystem;
using System.Linq;

public class GameController : MonoBehaviour
{
    public float timeScalle;

    public double timeToGoNextGen;
    private double timeBoundary;

    private PlayerInputAction playerInputAction;
    private InputAction saveBest;
    private InputAction loadBest;
    private InputAction restart;

    public GameObject[] monsterObjects;
    public GameObject playerObject;
    public GameObject flagObject;
    public GameObject[] obstacleObjects;
    public int numberOfObstacles;

    GameObject[] obstacles;

    public int numOfAgents;
    public double mutationRate;
    public double crossoverRate;

    public static List<Player> agents = new List<Player>();
    public static List<Player> failedAgents = new List<Player>();

    public Text movementPointText;
    public Text jumpPointText;
    public Text attackPointText;
    public Text teleportPointText;

    public Text genText;
    public Text nextgenTime;

    int gen = 0;

    public static double[] sumScore;

    public Vector2 flagRangeX;
    public Vector2 flagRangeY;

    public Vector2 playerRangeX;
    public Vector2 playerRangeY;

    public Vector2 obstacleRangeX;
    public Vector2 obstacleRangeY;

    public string loadFileName;
    public string saveFileName;

    public int lobeIndex;

    public double monsterCoolDown;
    private double monsterCounter;

    private void OnEnable()
    {
        saveBest = playerInputAction.GameController.SaveBest;
        loadBest = playerInputAction.GameController.LoadBest;
        restart = playerInputAction.GameController.Restart;

        saveBest.started += SaveBestAgent;
        loadBest.started += LoadBestAgent;
        restart.started += RestartNext;

        saveBest.Enable();
        loadBest.Enable();
        restart.Enable();
    }

    private void OnDisable()
    {
        saveBest.started -= SaveBestAgent;
        loadBest.started -= LoadBestAgent;
        restart.started -= RestartNext;

        saveBest.Disable();
        loadBest.Disable();
        restart.Disable();
    }

    private void LoadBestAgent(InputAction.CallbackContext obj)
    {
        Debug.Log("IN LOADBESTAGENT Agents Count: " + agents.Count);
        Debug.Log("IN LOADBESTAGENT Failed Agents Count: " + failedAgents.Count);

        string bestAgentWeights = File.ReadAllText("D:\\Bitirme2_Neuro_Yeni\\Assets\\" + loadFileName + "weights.txt");
        string bestAgentBiases = File.ReadAllText("D:\\Bitirme2_Neuro_Yeni\\Assets\\" + loadFileName + "biases.txt");

        NeuralNetworkv2 a;

        switch (lobeIndex)
        {
            case 0:
                a = new NeuralNetworkv2(new int[] { 6 }, 2, 1, 0.1);
                break;
            case 1:
                a = new NeuralNetworkv2(new int[] { 6 }, 2, 2, 0.1);
                break;
            case 2:
                a = new NeuralNetworkv2(new int[] { 6 }, 2, 2, 0.1);
                break;
            case 3:
                a = new NeuralNetworkv2(new int[] { 4 }, 1, 1, 0.1);
                break;
            default:
                a = new NeuralNetworkv2(new int[] { 1 }, 1, 1, 0.1);
                break;
        }

        NeuralNetworkv2.textToNn(bestAgentWeights, a.weights);
        NeuralNetworkv2.textToNn(bestAgentBiases, a.biases);

        GameObject[] gonnaDeleteAgents = GameObject.FindGameObjectsWithTag("Player");

        for (int i = 0; i < gonnaDeleteAgents.Length; i++)
        {
            Destroy(gonnaDeleteAgents[i]);
        }

        agents.Clear();
        failedAgents.Clear();

        GameObject[] gonnaDeleteFlags = GameObject.FindGameObjectsWithTag("Flag");

        for (int i = 0; i < gonnaDeleteFlags.Length; i++)
        {
            Destroy(gonnaDeleteFlags[i]);
        }
        GameObject bestAgent = Instantiate(playerObject, transform);
        bestAgent.GetComponent<Player>().brain[lobeIndex] = new NeuralNetworkv2(a);
        bestAgent.name = "0";

        agents.Add(bestAgent.GetComponent<Player>());
        ChangeLocation(bestAgent, playerRangeX, playerRangeY, 0.5f);

        GameObject bestFlag = Instantiate(flagObject, transform);
        bestFlag.name = "Flag" + bestAgent.name;
        ChangeLocation(bestFlag, flagRangeX, flagRangeY, 0.5f);

        string t = "";

        for (int j = 0; j < bestAgent.GetComponent<Player>().brain[lobeIndex].weights.Length; j++)
        {
            t += bestAgent.GetComponent<Player>().brain[lobeIndex].weights[j].printMatrix() + "\n";
        }

        Debug.Log("loaded best agent weights: " + t);

        t = "";

        for (int j = 0; j < bestAgent.GetComponent<Player>().brain[lobeIndex].biases.Length; j++)
        {
            t += bestAgent.GetComponent<Player>().brain[lobeIndex].biases[j].printMatrix() + "\n";
        }

        Debug.Log("loaded best agent biases: " + t);
    }


    private void LoadBestAgentCode(string[] fileName)
    {
        NeuralNetworkv2[] a = new NeuralNetworkv2[fileName.Length];

        for (int i = 0; i < a.Length; i++)
        {
            switch (i)
            {
                case 0:
                    a[i] = new NeuralNetworkv2(new int[] { 6 }, 2, 1, 0.1);
                    break;
                case 1:
                    a[i] = new NeuralNetworkv2(new int[] { 6 }, 2, 2, 0.1);
                    break;
                case 2:
                    a[i] = new NeuralNetworkv2(new int[] { 6 }, 2, 2, 0.1);
                    break;
                case 3:
                    a[i] = new NeuralNetworkv2(new int[] { 4 }, 1, 1, 0.1);
                    break;
            }

            if (fileName[i] != "")
            {
                string bestAgentWeights = File.ReadAllText("D:\\Bitirme2_Neuro_Yeni\\Assets\\" + fileName[i] + "weights.txt");
                string bestAgentBiases = File.ReadAllText("D:\\Bitirme2_Neuro_Yeni\\Assets\\" + fileName[i] + "biases.txt");

                NeuralNetworkv2.textToNn(bestAgentWeights, a[i].weights);
                NeuralNetworkv2.textToNn(bestAgentBiases, a[i].biases);
            }
        }

        GameObject[] gonnaDeleteAgents = GameObject.FindGameObjectsWithTag("Player");

        for (int i = 0; i < gonnaDeleteAgents.Length; i++)
        {
            Destroy(gonnaDeleteAgents[i]);
        }

        agents.Clear();
        failedAgents.Clear();

        GameObject[] gonnaDeleteFlags = GameObject.FindGameObjectsWithTag("Flag");

        for (int i = 0; i < gonnaDeleteFlags.Length; i++)
        {
            Destroy(gonnaDeleteFlags[i]);
        }

        GameObject bestAgent = Instantiate(playerObject, transform);
        for (int i = 0; i < a.Length; i++)
        {
            bestAgent.GetComponent<Player>().brain[i] = new NeuralNetworkv2(a[i]);
        }
        bestAgent.name = "0";

        agents.Add(bestAgent.GetComponent<Player>());
        ChangeLocation(bestAgent, playerRangeX, playerRangeY, 0.5f);

        GameObject bestFlag = Instantiate(flagObject, transform);
        bestFlag.name = "Flag" + bestAgent.name;
        ChangeLocation(bestFlag, flagRangeX, flagRangeY, 0.5f);    
        
        //string t = "";

        //for (int j = 0; j < bestAgent.GetComponent<Player>().brain[lobeIndex].weights.Length; j++)
        //{
        //    t += bestAgent.GetComponent<Player>().brain[lobeIndex].weights[j].printMatrix() + "\n";
        //}

        //Debug.Log("loaded best agent weights: " + t);

        //t = "";

        //for (int j = 0; j < bestAgent.GetComponent<Player>().brain[lobeIndex].biases.Length; j++)
        //{
        //    t += bestAgent.GetComponent<Player>().brain[lobeIndex].biases[j].printMatrix() + "\n";
        //}

        //Debug.Log("loaded best agent biases: " + t);
    }


    private void SaveBestAgent(InputAction.CallbackContext obj)
    {
        int highestPoint = 0;
        int index = 0;

        for (int i = 0; i < agents.Count; i++)
        {
            if (highestPoint <= agents[i].lobePoints[lobeIndex])
            {
                highestPoint = agents[i].lobePoints[lobeIndex];
                index = i;
            }
        }

        Debug.Log("highest point agent: " + agents[index].lobePoints[lobeIndex]);
        
        string t = "";

        for (int j = 0; j < agents[index].brain[lobeIndex].weights.Length; j++)
        {
            t += agents[index].brain[lobeIndex].weights[j].printMatrix() + "\n";
        }

        File.WriteAllText("D:\\Bitirme2_Neuro_Yeni\\Assets\\" + saveFileName + "weights.txt", t);

        Debug.Log("highest point agent's weights: " + t);

        t = "";

        for (int j = 0; j < agents[index].brain[lobeIndex].biases.Length; j++)
        {
            t += agents[index].brain[lobeIndex].biases[j].printMatrix() + "\n";
        }

        Debug.Log("highest point agent' s biases: " + t);

        File.WriteAllText("D:\\Bitirme2_Neuro_Yeni\\Assets\\" + saveFileName + "biases.txt", t);
    }

    private void Awake()
    {
        obstacles = new GameObject[numberOfObstacles];

        for (int i = 0; i < obstacles.Length; i++)
        {
            obstacles[i] = Instantiate(obstacleObjects[Random.Range(0, obstacleObjects.Length)], transform);
        }

        Player playerPlayer = playerObject.GetComponent<Player>();

        playerPlayer.brain = new NeuralNetworkv2[4];

        playerPlayer.brain[0] = new NeuralNetworkv2(new int[] { 6 }, 2, 1, 0.1);
        playerPlayer.brain[1] = new NeuralNetworkv2(new int[] { 6 }, 2, 2, 0.1);
        playerPlayer.brain[2] = new NeuralNetworkv2(new int[] { 6 }, 2, 2, 0.1);
        playerPlayer.brain[3] = new NeuralNetworkv2(new int[] { 4 }, 1, 1, 0.1);

        monsterCounter = monsterCoolDown;
        playerInputAction = new PlayerInputAction();
        timeBoundary = timeToGoNextGen;
    }

    private void Start()
    {
        sumScore = new double[4];

        for (int i = 0; i < sumScore.Length; i++)
        {
            sumScore[i] = 0;
        }

        Time.timeScale = timeScalle;

        startGame();
        LoadBestAgentCode(new string[] { "newwalk2", "bestAgentWithJump", "bestAgentWithAttack", "bestAgentWithAttackLow_Up" });
    }

    private void RestartNext(InputAction.CallbackContext obj)
    {
        timeBoundary = timeToGoNextGen;

        Debug.Log("IN RESTART Agents Count: " + agents.Count);
        Debug.Log("IN RESTART Failed Agents Count: " + failedAgents.Count);

        for (int i = 0; i < agents.Count; i++)
        {
            sumScore[lobeIndex] += agents[i].lobeScores[lobeIndex];
            agents[i].lobePoints[lobeIndex] = 0;
            failedAgents.Add(agents[i]);
        }

        for (int i = 0; i < agents.Count; i++)
        {
            Destroy(GameObject.Find("Flag" + agents[i].gameObject.name));
            Destroy(agents[i].gameObject);
        }

        agents.Clear();

        for (int i = 0; i < failedAgents.Count; i++)
        {
            failedAgents[i].lobeFitness[lobeIndex] = failedAgents[i].lobeScores[lobeIndex] / sumScore[lobeIndex];
        }

        for (int i = 0; i < sumScore.Length; i++)
        {
            sumScore[i] = 0;
        }

        startGame();
    }

    public void RestartNext()
    {
        Debug.Log("IN RESTART Agents Count: " + agents.Count);
        Debug.Log("IN RESTART Failed Agents Count: " + failedAgents.Count);

        for (int i = 0; i < agents.Count; i++)
        {
            sumScore[lobeIndex] += agents[i].lobeScores[lobeIndex];
            agents[i].lobePoints[lobeIndex] = 0;
            failedAgents.Add(agents[i]);
        }

        for (int i = 0; i < agents.Count; i++)
        {
            Destroy(GameObject.Find("Flag" + agents[i].gameObject.name));
            Destroy(agents[i].gameObject);
        }

        agents.Clear();

        for (int i = 0; i < failedAgents.Count; i++)
        {
            failedAgents[i].lobeFitness[lobeIndex] = failedAgents[i].lobeScores[lobeIndex] / sumScore[lobeIndex];
        }

        for (int i = 0; i < sumScore.Length; i++)
        {
            sumScore[i] = 0;
        }

        startGame();
    }

    private void Update()
    {
        if(timeBoundary <= 0)
        {
            RestartNext();
            timeBoundary = timeToGoNextGen;
        }
        else
        {
            timeBoundary -= Time.deltaTime;
            nextgenTime.text = "Time To Next Gen: " + ((int)timeBoundary).ToString();
        }

        if (monsterCounter <= 0)
        {
            for (int i = 0; i < agents.Count; i++)
            {
                GameObject a = Instantiate(monsterObjects[Random.Range(0, monsterObjects.Length)], transform);
                a.GetComponent<Monster>().target = agents[i];
                agents[i].currentEnemies.Add(a.GetComponent<Monster>());
                monsterCounter = monsterCoolDown;
                ChangeMonsterLocation(a.GetComponent<Monster>(), 5, 95, new Vector2(1.5f, 0.85f), 0.5f);
            }
        }
        else
        {
            monsterCounter -= Time.deltaTime;
        }

    }

    private NeuralNetworkv2[] pickOne(int lobeIndex)
    {
        if (failedAgents.Count == 0)
        {
            return playerObject.GetComponent<Player>().brain;
        }

        double bestFitness = 0;
        int index = 0;

        List<Player> orderedFailedAgents = failedAgents.OrderBy(a => a.lobeFitness[lobeIndex]).ToList<Player>();

        for (int i = 0; i < failedAgents.Count; i++)
        {
            if(bestFitness <= failedAgents[i].lobeFitness[lobeIndex])
            {
                bestFitness = failedAgents[i].lobeFitness[lobeIndex];
                index = i;
            }
        }

        int temp = orderedFailedAgents.Count - 1;

        if(orderedFailedAgents.Count >= 3)
        {
            temp = Random.Range(orderedFailedAgents.Count - 3, orderedFailedAgents.Count - 1);
        }

        failedAgents[index].brain[lobeIndex].crossover(orderedFailedAgents[temp].brain[lobeIndex], crossoverRate);
         
        return failedAgents[index].brain;
    }

    private List<Player> generateAgents()
    {
        List<Player> agentList = new List<Player>();

        NeuralNetworkv2[] agentBrain = pickOne(lobeIndex);

        for (int i = 0; i < numOfAgents; i++)
        {
            GameObject agent = Instantiate(playerObject, transform);

            agent.GetComponent<Player>().generateNN(agentBrain, mutationRate, lobeIndex);
            agentList.Add(agent.GetComponent<Player>());
            agent.name = i.ToString();
            ChangeLocation(agent, playerRangeX, playerRangeY, 0.5f);
        }

        string t = "";
        for (int i = 0; i < agentList.Count; i++)
        {
            for (int j = 0; j < agentList[i].brain[lobeIndex].weights.Length; j++)
            {
                t += agentList[i].brain[lobeIndex].weights[j].printMatrix() + "\n";
            }
            Debug.Log(t);
            t = "";
        }

        return agentList;
    }

    public void startGame()
    {
        for (int i = 0; i < obstacles.Length; i++)
        {
            ChangeLocation(obstacles[i], obstacleRangeX, obstacleRangeY, 0.5f);
        }

        GameObject[] a = new GameObject[numOfAgents];

        for (int i = 0; i < numOfAgents; i++)
        {
            a[i] = Instantiate(flagObject, transform);
            a[i].name = "Flag" + i;
            ChangeLocation(a[i], flagRangeX, flagRangeY, 0.5f);
        }

        genText.text = "GEN: " + (++gen);
        agents = generateAgents();
        failedAgents.Clear();
    }

    public static void ChangeLocation(GameObject anObject, Vector2 xPosRange, Vector2 yPosRange, float rate)
    {
        if(Random.Range(0f, 1f) < rate)
        {
            anObject.transform.position = new Vector3(Random.Range(xPosRange[0], xPosRange[1]), yPosRange[0], 0f);
        }
        else
        {
            anObject.transform.position = new Vector3(Random.Range(xPosRange[1], xPosRange[0]), yPosRange[1], 0f);
        }

    }

    public static void ChangeMonsterLocation(Monster monster, int x1, int x2, Vector2 yPosRange, float rate)
    {
        if (Random.Range(0f, 1f) < rate)
        {
            if(Random.Range(0f, 1f) < 0.5f)
            {
                monster.gameObject.transform.position = new Vector3(x1, yPosRange[0], 0f);
                monster.moveDirection = Monster.DirectionEnum.Rigth;
            }
            else
            {
                monster.gameObject.transform.position = new Vector3(x2, yPosRange[0], 0f);
                monster.moveDirection = Monster.DirectionEnum.Left;
            }
        }
        else
        {
            if(Random.Range(0f, 1f) < 0.5f)
            {
                monster.gameObject.transform.position = new Vector3(x1, yPosRange[1], 0f);
                monster.moveDirection = Monster.DirectionEnum.Rigth;
            }
            else
            {
                monster.gameObject.transform.position = new Vector3(x2, yPosRange[1], 0f);
                monster.moveDirection = Monster.DirectionEnum.Left;
            }
        }
    }
}
