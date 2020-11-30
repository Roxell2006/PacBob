using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    float x = 1.2f;
    float y = 1.5f;
    float speed = .008f;
    
    void Update()
    {
        if (x > 1.5f || x < 1.2f)
            speed = -speed;
        x += speed;
        y += speed;
        transform.localScale = new Vector3(x, y, 1);
    }

    public void ClickStart()
    {
        SceneManager.LoadScene("Level1");
    }
}
