using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    public static int score;

    public void AfficheScore()
    {
        GetComponent<Text>().text = "Score: " + score;
    }
}
