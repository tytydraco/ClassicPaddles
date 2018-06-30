using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameScript : MonoBehaviour
{
    public GameObject paddle1;
    public GameObject paddle2;
    public GameObject ball;
    public Text score1;
    public Text score2;

    public float resizeAnimSpeed = 5;
    public int ballSpeed = 7;
    public int paddleSpeed = 7;
    public int maxPaddleXForce = 15;
    public int maxPaddleXShuffleForce = 12;
    public float minAIPaddleMovementForShuffle = 0.75f;
    public int paddle1Score;
    public int paddle2Score;
    public int activePlayers;
    public bool player1IsBottom;

    private Vector2 screenSize;
    private bool shrinking;
    private float ballScale;
    private float scoreScale;

    // Use this for initialization
    void Start()
    {
        // pre-game world setups
        setupWorldBounds();
        resetBall(ball);

        // set the local variables based on the Unity Editor
        ballScale = ball.transform.localScale.x;
        scoreScale = score1.transform.localScale.x;

        // initially hide the score counter
        score1.enabled = false;
        score2.enabled = false;
    }

    // reposition the ball on the screen randomly and re-apply the standard velocity
    public void resetBall(GameObject ball)
    {
        int xMult = Random.Range(0, 2) == 1 ? 1 : -1;
        int yMult = Random.Range(0, 2) == 1 ? 1 : -1;

        ball.GetComponent<Rigidbody2D>().position = Vector2.zero;
        ball.GetComponent<Rigidbody2D>().velocity = new Vector2(ballSpeed * xMult, ballSpeed * yMult);
    }

    // Update is called once per frame
    void Update()
    {
        // handle animations (shrinking and growing)
        if (shrinking)
        {
            gradResize(ball, 0, resizeAnimSpeed);
            gradResize(score1, scoreScale, resizeAnimSpeed);
            gradResize(score2, scoreScale, resizeAnimSpeed);
        } else
        {
            gradResize(ball, ballScale, resizeAnimSpeed);
            gradResize(score1, 0, resizeAnimSpeed);
            gradResize(score2, 0, resizeAnimSpeed);
        }
        
        handleMovement();
    }

    // handle user input
    private void handleMovement()
    {
        float speed = paddleSpeed * Time.deltaTime;
        activePlayers = Input.touchCount;

        // handle movement for player vs player
        if (Input.touchCount == 2)
        {
            foreach (Touch touch in Input.touches)
            {
                Vector2 touchPos = Camera.main.ScreenToWorldPoint(touch.position);

                // if we are touching in the top half of the screen
                if (touchPos.y > 0)
                {
                    Vector2 newPosition = new Vector2(touchPos.x, paddle2.transform.position.y);
                    paddle2.transform.position = newPosition;
                }
                // if we are touching in the bottom half of the screen
                else
                {
                    Vector2 newPosition = new Vector2(touchPos.x, paddle1.transform.position.y);
                    paddle1.transform.position = newPosition;
                }
            }
        // handle movement for player vs AI
        } else if (Input.touchCount == 1)
        {
            foreach (Touch touch in Input.touches)
            {
                Vector2 touchPos = Camera.main.ScreenToWorldPoint(touch.position);
                // if we are touching in the top half of the screen
                if (touchPos.y > 0)
                {
                    player1IsBottom = false;
                    Vector2 newPosition = new Vector2(touchPos.x, paddle2.transform.position.y);
                    paddle2.transform.position = newPosition;

                    int divisor = 1;

                    // if the ball is not in the AI's territory, slow it down to create a more realistic movement pattern
                    if (ball.transform.position.y > 0)
                    {
                        divisor = 2;
                    }

                    Vector2 newPosition2 = new Vector2(ball.transform.position.x, paddle1.transform.position.y);
                    paddle1.transform.position = Vector2.MoveTowards(paddle1.transform.position, newPosition2, speed / divisor);
                }
                // if we are touching in the bottom half of the screen
                else
                {
                    player1IsBottom = true;
                    Vector2 newPosition = new Vector2(touchPos.x, paddle1.transform.position.y);
                    paddle1.transform.position = newPosition;

                    int divisor = 1;

                    // if the ball is not in the AI's territory, slow it down to create a more realistic movement pattern
                    if (ball.transform.position.y <= 0)
                    {
                        divisor = 2;
                    }

                    Vector2 newPosition2 = new Vector2(ball.transform.position.x, paddle2.transform.position.y);
                    paddle2.transform.position = Vector2.MoveTowards(paddle2.transform.position, newPosition2, speed / divisor);
                }
            }
        // handle movement for AI vs AI
        } else if (Input.touchCount == 0)
        {
            int divisor1 = 1;
            int divisor2 = 1;

            // if the ball is not in the AI's territory, slow it down to create a more realistic movement pattern
            if (ball.transform.position.y > 0)
            {
                divisor2 = 2;
            } else
            {
                divisor1 = 2;
            }

            // apply a speed boost to the AI vs AI to ensure neither can score on each other
            Vector2 newPosition = new Vector2(ball.transform.position.x, paddle1.transform.position.y);
            paddle1.transform.position = Vector2.MoveTowards(paddle1.transform.position, newPosition, 1.75f * speed / divisor2);

            Vector2 newPosition2 = new Vector2(ball.transform.position.x, paddle2.transform.position.y);
            paddle2.transform.position = Vector2.MoveTowards(paddle2.transform.position, newPosition2, 1.75f * speed / divisor1);
        }
    }

    // gradual resize for a GameObject
    private void gradResize(GameObject obj, float targetScale, float speed)
    {
        obj.transform.localScale = Vector3.Lerp(obj.transform.localScale, new Vector3(targetScale, targetScale, targetScale), Time.deltaTime * speed);
    }

    // gradual resize for a Text
    private void gradResize(Text obj, float targetScale, float speed)
    {
        obj.transform.localScale = Vector3.Lerp(obj.transform.localScale, new Vector3(targetScale, targetScale, targetScale), Time.deltaTime * speed);
    }

    // display the score and reset the scene for another round
    public IEnumerator showScoreAndReset()
    {
        // begin shrinking the UI elements
        shrinking = true;

        // display the score counters
        score1.enabled = true;
        score2.enabled = true;

        // stop the ball from moving
        ball.GetComponent<Rigidbody2D>().velocity = Vector2.zero;

        // set the score counters to the current player score
        score1.text = paddle1Score.ToString();
        score2.text = paddle2Score.ToString();

        // apply score rotation to optimize viewing for each viewer
        if (activePlayers == 2)
        {
            score1.transform.rotation = Quaternion.Euler(0, 0, 0);
            score2.transform.rotation = Quaternion.Euler(180, 180, 0);
        } else if (activePlayers == 1 && !player1IsBottom)
        {
            score1.transform.rotation = Quaternion.Euler(180, 180, 0);
            score2.transform.rotation = Quaternion.Euler(180, 180, 0);
        } else {
            score1.transform.rotation = Quaternion.Euler(0, 0, 0);
            score2.transform.rotation = Quaternion.Euler(0, 0, 0);
        }

        yield return new WaitForSeconds(1);

        // center the ball and begin growing
        ball.transform.position = Vector2.zero;
        shrinking = false;

        yield return new WaitForSeconds(1);

        // hide the score counters
        score1.enabled = false;
        score2.enabled = false;

        // randomize the ball's velocity
        resetBall(ball);
    }

    // setup world borders and global variables for future collision checks
    public void setupWorldBounds()
    {
        Transform topCollider;
        Transform bottomCollider;
        Transform leftCollider;
        Transform rightCollider;
        float colDepth = 4f;
        Vector3 cameraPos;
        float zPosition = 0f;

        //Generate our empty objects
        topCollider = new GameObject().transform;
        bottomCollider = new GameObject().transform;
        rightCollider = new GameObject().transform;
        leftCollider = new GameObject().transform;

        //Name our objects 
        topCollider.name = "TopCollider";
        bottomCollider.name = "BottomCollider";
        rightCollider.name = "RightCollider";
        leftCollider.name = "LeftCollider";

        //Add the colliders
        topCollider.gameObject.AddComponent<BoxCollider2D>();
        bottomCollider.gameObject.AddComponent<BoxCollider2D>();
        rightCollider.gameObject.AddComponent<BoxCollider2D>();
        leftCollider.gameObject.AddComponent<BoxCollider2D>();

        //Make them the child of whatever object this script is on, preferably on the Camera so the objects move with the camera without extra scripting
        topCollider.parent = transform;
        bottomCollider.parent = transform;
        rightCollider.parent = transform;
        leftCollider.parent = transform;

        //Generate world space point information for position and scale calculations
        cameraPos = Camera.main.transform.position;
        screenSize.x = Vector2.Distance(Camera.main.ScreenToWorldPoint(new Vector2(0, 0)), Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, 0))) * 0.5f;
        screenSize.y = Vector2.Distance(Camera.main.ScreenToWorldPoint(new Vector2(0, 0)), Camera.main.ScreenToWorldPoint(new Vector2(0, Screen.height))) * 0.5f;

        //Change our scale and positions to match the edges of the screen...   
        rightCollider.localScale = new Vector3(colDepth, screenSize.y * 2, colDepth);
        rightCollider.position = new Vector3(cameraPos.x + screenSize.x + (rightCollider.localScale.x * 0.5f), cameraPos.y, zPosition);
        leftCollider.localScale = new Vector3(colDepth, screenSize.y * 2, colDepth);
        leftCollider.position = new Vector3(cameraPos.x - screenSize.x - (leftCollider.localScale.x * 0.5f), cameraPos.y, zPosition);
        topCollider.localScale = new Vector3(screenSize.x * 2, colDepth, colDepth);
        topCollider.position = new Vector3(cameraPos.x, cameraPos.y + screenSize.y + (topCollider.localScale.y * 0.5f), zPosition);
        bottomCollider.localScale = new Vector3(screenSize.x * 2, colDepth, colDepth);
        bottomCollider.position = new Vector3(cameraPos.x, cameraPos.y - screenSize.y - (bottomCollider.localScale.y * 0.5f), zPosition);
    }
}
