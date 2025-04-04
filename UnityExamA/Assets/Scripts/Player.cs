using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    // y‘ÌŒ±‚PzfloatŒ^‚ÌjumpPower‚ğ€”õ‚µ‚æ‚¤I


    // Update is called once per frame
    void Update()
    {
        // y‘ÌŒ±‚QzJumpƒ{ƒ^ƒ“(SpaceƒL[)‚ª‰Ÿ‚³‚ê‚½‚©‚Ç‚¤‚©”»’è‚µ‚æ‚¤I
        if (Input.GetButtonDown(""))
        {
            // y‘ÌŒ±‚RzVector3‚ÌY²•ûŒü‚ÉAjumpPower‚¾‚¯ˆÚ“®‚·‚é‚æ‚¤‚Éİ’è‚µ‚æ‚¤I
            GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
        }
    }

    // •Ç‚É‚Ô‚Â‚©‚Á‚½‚Ìˆ—
    void OnCollisionEnter(Collision collision)
    {
        // y‘ÌŒ±‚SzƒV[ƒ“uMainv‚ğŒÄ‚Ño‚»‚¤I
        SceneManager.LoadScene("");
    }
}
