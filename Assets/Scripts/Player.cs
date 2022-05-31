using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using System;
using UnityEngine.UI;
using Bitirme2;

public class Player : MonoBehaviour
{
    private PlayerInputAction playerInputAction;
    private InputAction movement;
    private InputAction punch;
    private InputAction kick;
    private InputAction jump;
    public Action RunAction;

    public Vector3 movementVector;
    public float speed;
    public float jumpSpeed;
    public Rigidbody rb;
    Animator anim;
    public bool attacking;
    public bool inAir;
    public bool jumpPressed;

    [SerializeField]
    private int hitpoint;
    public Text HitPointTxt;

    public Transform punchTransform;
    public Vector3 punchHalfExtents;
    public Transform kickTransform;
    public Vector3 kickHalfExtents;

    public ParticleSystem hurtParticle;

    public NeuralNetworkv2[] brain;

    public int[] lobePoints;
    public double[] lobeScores;
    public double[] lobeFitness;


    public List<Monster> currentEnemies;

    double stuckChecker;
    Vector3 stuckCheckerPos;
    double stuckCounter;
    Collider[] stuckCollider;
    public float stuckCheckerRadius;

    public LayerMask detectableObjectLayers;
    Collider[] detectableObjectsCollider; 
    public float viewRadius;
    GameObject closestGO;

    GameController gameControl;

    private void Awake()
    {
        currentEnemies = new List<Monster>();
        gameControl = GameObject.Find("GameController").GetComponent<GameController>();

        attacking = false;
        playerInputAction = new PlayerInputAction();
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        RunAction += Run;

        brain = new NeuralNetworkv2[4];

        brain[0] = new NeuralNetworkv2(new int[] { 6 }, 2, 1, 0.1);
        brain[1] = new NeuralNetworkv2(new int[] { 6 }, 2, 2, 0.1);
        brain[2] = new NeuralNetworkv2(new int[] { 6 }, 2, 2, 0.1);
        brain[3] = new NeuralNetworkv2(new int[] { 4 }, 1, 1, 0.1);

        lobePoints = new int[4];

        for (int i = 0; i < lobePoints.Length; i++)
        {
            lobePoints[i] = 1;
        }

        lobeFitness = new double[4];
        lobeScores = new double[4];

        lobeScores[1] = 10;

        inAir = true;
    }

    private void Start()
    {
        HitPointTxt.text = "Health: " + hitpoint;
    }

    GameObject findClosestGOx(Collider[] objectColliders)
    {
        if (objectColliders.Length <= 0)
            return null;

        double minDist = Mathf.Infinity;
        int closestIndex = 0;
        double distance = 0;

        for (int i = 0; i < objectColliders.Length; i++)
        {
            distance = Mathf.Abs(transform.position.x - objectColliders[i].gameObject.transform.position.x);
            if (minDist > distance)
            {
                minDist = distance;
                closestIndex = i;
            }
        }
        return objectColliders[closestIndex].gameObject;
    }

    private void Update()
    {
        detectableObjectsCollider = Physics.OverlapSphere(transform.position, viewRadius, detectableObjectLayers);
        closestGO = findClosestGOx(detectableObjectsCollider);

        thinkMove();
        thinkAttack();
        thinkJump();

        if (stuckChecker <= 0)
        {
            stuckCheckerPos = transform.position;
            stuckChecker = 6;
            stuckCounter = 4;
        }
        else
        {
            stuckCollider = Physics.OverlapSphere(stuckCheckerPos, stuckCheckerRadius);
            foreach (Collider item in stuckCollider)
            {
                if (item.name == name)
                {
                    stuckCounter -= Time.deltaTime;
                    if (stuckCounter <= 0)
                    {
                        Damage(1);
                    }
                }
            }
            stuckChecker -= Time.deltaTime;
        }

        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector3.up * Physics.gravity.y * 7f * Time.deltaTime;
        }
        else
        {
            rb.velocity += Vector3.up * Physics.gravity.y * 3f * Time.deltaTime;
        }
    }

    private void thinkMove()
    {
        double[] input = { (transform.position.x), (GameObject.Find("Flag" + gameObject.name).transform.position.x)};

        Matrix output = brain[0].predict(Matrix.fromArray(input)); 

        double outputDouble = output.matrix[0, 0];

        Vector3 agentDirection;

        if (outputDouble > 0.5f)
        {
            agentDirection = Vector3.right;
        }
        else
        {
            agentDirection = Vector3.left;
        }

        rb.velocity = new Vector3(agentDirection.x * speed, rb.velocity.y, 0);
        anim.SetFloat("run", Math.Abs(agentDirection.x));
        if (!attacking)
            transform.localScale = new Vector3(((agentDirection.x != 0) ? agentDirection.x : transform.localScale.x), 1, 1);
    }

    private void thinkJump()
    {
        double[] input;

        if(closestGO != null)
        {
            input = new double[] { Mathf.Abs(closestGO.transform.position.x - transform.position.x), closestGO.CompareTag("Obstacle") ? 1 : 0 };
        }
        else
        {
            input = new double[] { 0, 0 };
        }

        Matrix output = brain[1].predict(Matrix.fromArray(input)); 

        double outputDouble1 = output.matrix[0, 0];
        double outputDouble2 = output.matrix[1, 0];

        if (outputDouble1 > outputDouble2 && !inAir)
        {
            rb.velocity = new Vector3((Mathf.Abs(rb.velocity.x) / rb.velocity.x) * speed, jumpSpeed, 0) / Mathf.Sqrt(2);
            jumpPressed = true;

            if(lobeScores[1] > 0)
            {
                lobeScores[1] -= lobePoints[1] * 2; 
            }

            gameControl.jumpPointText.text = "Jump Point: " + lobePoints[1];

            if (lobeScores[1] <= 0 && gameControl.mutationRate != 0)
            {
                Damage(1);
            }

            if (closestGO != null && closestGO.CompareTag("Monster") && gameControl.mutationRate != 0 /* && currentEnemies.Contains(closestGO.GetComponent<Monster>())*/)
            {
                Damage(1);
            }
        }
    }

    private void thinkAttack()
    {
        double[] input;
        double[] input1;

        if (closestGO != null)
        {
            input = new double[] { Mathf.Abs(closestGO.transform.position.x - transform.position.x), closestGO.CompareTag("Monster") ? 1 : 0 };
            input1 = new double[] { transform.position.y - closestGO.transform.position.y };
        }
        else
        {
            input = new double[] { 0, 0 };
            input1 = new double[] { 0 };
        }

        Matrix output = brain[2].predict(Matrix.fromArray(input));

        double outputDouble1 = output.matrix[0, 0];
        double outputDouble2 = output.matrix[1, 0];

        Matrix output1 = brain[3].predict(Matrix.fromArray(input1));

        double output1Double = output1.matrix[0, 0];


        if (closestGO != null)
        {
            if (outputDouble1 > outputDouble2 && !attacking && currentEnemies.Contains(closestGO.GetComponent<Monster>()))
            {
                if (output1Double > 0.5)
                {
                    attacking = true;
                    anim.Play("punchh");
                    Attack(punchTransform.position, punchHalfExtents);
                }
                else
                {
                    attacking = true;
                    anim.Play("kickk");
                    Attack(kickTransform.position, kickHalfExtents);
                }
            } 
        }
    }

    public void generateNN(NeuralNetworkv2[] newBrain, double mutationRate, int lobeIndex)
    {
        for (int i = 0; i < brain.Length; i++)
        {
            if (i == lobeIndex)
                continue;

            brain[i] = new NeuralNetworkv2(newBrain[i]);
        }

        brain[lobeIndex] = new NeuralNetworkv2(newBrain[lobeIndex]);
        brain[lobeIndex].mutate(mutationRate);
    }

    public void Damage(int damage)
    {
        hitpoint -= damage;
        HitPointTxt.text = "Health: " + hitpoint;
        hurtParticle.Play();

        if(hitpoint <= 0)
        {
            List<Player> agentList = GameController.agents;
            agentList.Remove(GetComponent<Player>());

            List<Player> failedAgentsList = GameController.failedAgents;
            failedAgentsList.Add(GetComponent<Player>());

            for (int i = 0; i < lobePoints.Length; i++)
            {
                lobePoints[i] = 0;
            }

            GameController.sumScore[gameControl.lobeIndex] += lobeScores[gameControl.lobeIndex];

            if (agentList.Count == 0)
            {
                for (int i = 0; i < failedAgentsList.Count; i++)
                {
                    failedAgentsList[i].lobeFitness[gameControl.lobeIndex] = failedAgentsList[i].lobeScores[gameControl.lobeIndex] / GameController.sumScore[gameControl.lobeIndex];
                }

                GameController.sumScore[gameControl.lobeIndex] = 0;
                
                GameObject.Find("GameController").GetComponent<GameController>().startGame();
            }
            Destroy(GameObject.Find("Flag" + name));
            if(currentEnemies.Count > 0)
            {
                for (int i = 0; i < currentEnemies.Count; i++)
                {
                    Destroy(currentEnemies[i].gameObject);
                }
                currentEnemies.Clear(); 
            }
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        movement = playerInputAction.Player.Movement;
        punch = playerInputAction.Player.Punch;
        kick = playerInputAction.Player.Kick;
        jump = playerInputAction.Player.Jump;

        jump.started += Jump;
        punch.started += Punch;
        kick.started += Kick;

        movement.performed += Run;
        movement.canceled += Run;

        jump.Enable();
        kick.Enable();
        punch.Enable();
        movement.Enable();
    }

    private void Attack(Vector3 pos, Vector3 halfExtents)
    {
        Collider[] colliders = Physics.OverlapBox(pos, halfExtents, Quaternion.identity, detectableObjectLayers);

        foreach (Collider item in colliders)
        {
            if (item.CompareTag("Monster") && !item.GetComponent<Monster>().invincible && currentEnemies.Contains(item.GetComponent<Monster>()))
            {
                currentEnemies.Remove(item.GetComponent<Monster>());
                Destroy(item.gameObject);

                if (gameControl.lobeIndex == 2 || gameControl.lobeIndex == 3)
                {
                    lobePoints[gameControl.lobeIndex]++;
                    lobeScores[gameControl.lobeIndex] += lobePoints[gameControl.lobeIndex] * 5;
                    gameControl.attackPointText.text = "Attack Point: " + lobePoints[gameControl.lobeIndex];
                }
                return;
            }
        }

        //if (gameControl.lobeIndex == 2)
        //{
            if (lobeScores[gameControl.lobeIndex] > 0)
            {
                lobeScores[gameControl.lobeIndex] -= lobePoints[gameControl.lobeIndex] * 2;
            }

            gameControl.attackPointText.text = "Attack Point: " + lobePoints[gameControl.lobeIndex];

            if (lobeScores[gameControl.lobeIndex] <= 0 && gameControl.mutationRate != 0)
            {
                Damage(1);
                Debug.Log("sakin ol þampiyon");
            }
        //}
    }
    
    private void Jump(InputAction.CallbackContext obj)
    {
        if (!inAir)
        {
            rb.velocity = (movementVector * speed + Vector3.up * jumpSpeed) / Mathf.Sqrt(2);
            jumpPressed = true;
        }
    }

    private void Kick(InputAction.CallbackContext obj)
    {
        if (!attacking)
        {
            attacking = true;
            anim.Play("kickk");
            Attack(kickTransform.position, kickHalfExtents);
        }
    }

    private void Punch(InputAction.CallbackContext obj)
    {
        if (!attacking)
        {
            attacking = true;
            anim.Play("punchh");
            Attack(punchTransform.position, punchHalfExtents);
        }
    }

    private void Run(InputAction.CallbackContext obj)
    {
        movementVector = new Vector2(obj.ReadValue<float>(), 0);

        rb.velocity = new Vector3(movementVector.x * speed, rb.velocity.y, 0);
        anim.SetFloat("run", Math.Abs(movementVector.x));
        if (!attacking) 
            transform.localScale = new Vector3(((movementVector.x != 0) ? movementVector.x : transform.localScale.x), 1, 1);
    }

    private void Run()
    {
        rb.velocity = new Vector3(movementVector.x * speed, rb.velocity.y, 0);
        anim.SetFloat("run", Math.Abs(movementVector.x));
        if (!attacking)
            transform.localScale = new Vector3(((movementVector.x != 0) ? movementVector.x : transform.localScale.x), 1, 1);
    }

    private void OnDisable()
    {
        jump.started -= Jump;
        punch.started -= Punch;
        kick.started -= Kick;
        movement.performed -= Run;
        movement.canceled -= Run;

        kick.Disable();
        punch.Disable();
        movement.Disable();
        jump.Disable();
        //block.Disable();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(punchTransform.position, punchHalfExtents * 2);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(kickTransform.position, kickHalfExtents * 2);
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(stuckCheckerPos, stuckCheckerRadius);
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, viewRadius);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Flag") && other.name == "Flag" + gameObject.name)
        {
            lobePoints[0]++;
            lobeScores[0] += lobePoints[0] * 5; 
            gameControl.movementPointText.text = "Movement Point: " + lobePoints[0];
            GameController.ChangeLocation(other.gameObject, gameControl.flagRangeX, gameControl.flagRangeY, 0.5f);
        }
    }
}
