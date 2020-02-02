using UnityEngine;

public class UnicodeScript : MonoBehaviour
{
    public KMAudio Audio;
    public KMBombModule Module;
    public KMBombInfo Info;

    public KMSelectable[] Buttons;
    public KMSelectable UPlusButton;

    public TextMesh[] SymbolsScreen;
    public TextMesh TextArray;

    private static int _moduleIdCounter = 1;
    private int _moduleId = 0;
    private bool isSolved = false;

    // Use this for initialization
    void Start ()
    {
        _moduleId = _moduleIdCounter++;
        Module.OnActivate += Activate;
    }

    void Activate()
    {

    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}
}
