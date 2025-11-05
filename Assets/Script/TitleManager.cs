using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    public void OnstartButton()
    {
        SceneManager.LoadScene("SurveryScene");
    }
    
}
