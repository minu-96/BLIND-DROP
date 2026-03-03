using UnityEngine;

public class ExitManager : MonoBehaviour
{
    public static ExitManager Instance { get; private set; }
    public GameObject exitPanel;
    public bool onUI;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        onUI = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyUp(KeyCode.Escape))
        {
            OnExit();
        }
        
    }

    public void OnExit()
    {
        
        if(onUI ==false)
        {
            Time.timeScale = 0f;
            exitPanel.SetActive(true);
            onUI = true;
        }
        else if(onUI == true)
        {
            OffExit();
        } 
    }
    public void OffExit()
    {
        Time.timeScale = 1f;
        exitPanel.SetActive(false);
        onUI = false;
    }
    
}
