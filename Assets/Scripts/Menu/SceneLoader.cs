using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public static void LoadGameScene()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public static void LoadMenuScene()
    {
        SceneManager.LoadScene("SceneMenu");
    }
}