using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuButtonns : MonoBehaviour
{
  public void PlayGame()
    {
        SceneManager.LoadScene("0");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
