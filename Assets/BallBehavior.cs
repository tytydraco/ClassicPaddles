using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BallBehavior : MonoBehaviour {

    private Vector2 screenSize;
    private GameScript gameScript;

    // Use this for initialization
    void Start()
    {
        GameObject g = GameObject.Find("ScriptObject");
        gameScript = g.GetComponent<GameScript>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // hits top of the screen
        if (collision.collider.name.Equals("TopCollider"))
        {
            gameScript.paddle1Score++;
            StartCoroutine(gameScript.showScoreAndReset());
        // hits bottom of the screen
        } else if (collision.collider.name.Equals("BottomCollider"))
        {
            gameScript.paddle2Score++;
            StartCoroutine(gameScript.showScoreAndReset());
            // hits another paddle
        } else if (collision.collider.tag.Equals("Paddle"))
        {
            Vector2 ballPos = transform.position;
            Vector2 paddlePos = collision.rigidbody.transform.position;

            float newX;
            float newY;

            // calculate the newX force based on which half of the screen && the position of the ball relative to the paddle
            if (transform.position.x < 0)
            {
                newX = (Mathf.Abs(ballPos.x) - Mathf.Abs(paddlePos.x)) * -gameScript.maxPaddleXForce;
            } else
            {
                newX = (ballPos.x - paddlePos.x) * gameScript.maxPaddleXForce;
            }

            // shuffle the ball randomly if it is not moving enough with 2 AI players
            if (gameScript.activePlayers == 0 && Mathf.Abs(newX) <= gameScript.minAIPaddleMovementForShuffle)
            {
                int dirMult;
                dirMult = (newX > 0) ? 1 : -1;
                newX = gameScript.maxPaddleXShuffleForce * dirMult;
            }

            // flip ball directions based on the paddle hit (except when it has already passed the paddle (to ensure a win))
            if (ballPos.y > 0 && ballPos.y < paddlePos.y)
            {
                newY = -gameScript.ballSpeed;
            } else if (ballPos.y < 0 && ballPos.y > paddlePos.y)
            {
                newY = gameScript.ballSpeed;
            } else
            {
                newY = GetComponent<Rigidbody2D>().velocity.y;
            }

            // apply the new velocity
            GetComponent<Rigidbody2D>().velocity = new Vector2(newX, newY);
        }
    }
}
