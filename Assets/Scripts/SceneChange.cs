using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{
    public float transitionTime;
    public string currentLevel;

    private void OnCollisionEnter(Collision other) 
    {
        if(other.GameObject.compareTag("Player")){

            if(currentLevel == "Level 1"){
                StartCoroutine(TransitionToScene());
            }
           
        }
    }

    private IEnumerator TransitionToScene()
    {
        // Wait for the animation to complete
        yield return new WaitForSeconds(transitionTime);

        // Load the new scene
        SceneManager.LoadScene(currentLevel);
    }
}
