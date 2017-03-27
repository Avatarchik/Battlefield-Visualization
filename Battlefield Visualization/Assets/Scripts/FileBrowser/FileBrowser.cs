using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class FileBrowser : MonoBehaviour
{

    // loads script of the ResourceManager
    private ResourceManager resourceScript;

    private string messageBlue = "";
    private string messageNeutral = "";
    private string messageRed = "";
    private char pathChar = '/';

    private Texture2D texture_SV_logo;
    private Texture2D texture_FileUploadBackground;
    private float SV_logo_width = 400f;

    void Awake()
    {
        this.resourceScript = GameObject.Find("ResourceManager").GetComponent<ResourceManager>() as ResourceManager;
    }

    void Start()
    {
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            pathChar = '\\';
        }

        this.texture_SV_logo = (Texture2D)Resources.Load("Images/SV_logo", typeof(Texture2D));
        this.texture_FileUploadBackground = (Texture2D)Resources.Load("Images/FileUpload_Background", typeof(Texture2D));
    }

    void OnGUI()
    {
        float left = (Screen.width - SV_logo_width) / 2;
        float top = (Screen.height - SV_logo_width * 855 / 923) / 6;

        GUI.DrawTexture(new Rect(left, top, SV_logo_width, SV_logo_width * 855 / 923), this.texture_SV_logo);
       
        top = Screen.height / 2 + 120;
        var centeredStyle = GUI.skin.GetStyle("Label");
        centeredStyle.alignment = TextAnchor.UpperCenter;

        GUI.Label(new Rect(left, top, 113, 1000), messageBlue);
        GUI.Label(new Rect(left + 142, top, 113, 1000), messageNeutral);
        GUI.Label(new Rect(left + 284, top, 113, 1000), messageRed);
    }

    public void OpenFileWindow_blue()
    {
        CustomFileBrowser.use.OpenFileWindow(SetBlueFile);
    }

    public void OpenFileWindow_neutral()
    {
        CustomFileBrowser.use.OpenFileWindow(SetNeutralFile);
    }

    public void OpenFileWindow_red()
    {
        CustomFileBrowser.use.OpenFileWindow(SetRedFile);
    }

    void SetBlueFile(string pathToFile)
    {
        var fileIndex = pathToFile.LastIndexOf(pathChar);
        messageBlue = pathToFile.Substring(fileIndex + 1, pathToFile.Length - fileIndex - 1);
        this.resourceScript.blueFilePath = pathToFile;
    }

    void SetNeutralFile(string pathToFile)
    {
        var fileIndex = pathToFile.LastIndexOf(pathChar);
        messageNeutral = pathToFile.Substring(fileIndex + 1, pathToFile.Length - fileIndex - 1);
        this.resourceScript.neutralFilePath = pathToFile;
    }

    void SetRedFile(string pathToFile)
    {
        var fileIndex = pathToFile.LastIndexOf(pathChar);
        messageRed = pathToFile.Substring(fileIndex + 1, pathToFile.Length - fileIndex - 1);
        this.resourceScript.redFilePath = pathToFile;
    }

    public void ValidateFiles()
    {
        if (this.resourceScript.blueFilePath == "" || this.resourceScript.neutralFilePath == "" || this.resourceScript.redFilePath == "")
            return;

        LoadScene();
    }

    public void LoadScene()
    {
        // loads the next scene
        SceneManager.LoadScene("scene_Visualisation");
        DestroyImmediate(this.gameObject);
    }
}
