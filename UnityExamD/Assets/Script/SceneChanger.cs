using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public string nextSceneName;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) { return; }

        if (keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene(nextSceneName);
        }

        if (keyboard.escapeKey.isPressed)
        {
            Application.Quit();
        }
    }
}
