using UnityEngine;
using UnityEngine.SceneManagement;

public class Restart : MonoBehaviour
{
    public void restart()
    {
        Time.timeScale = 1f; // undo the freeze from game over
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
