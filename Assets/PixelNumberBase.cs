using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Newtonsoft.Json;

public class PixelNumberBase : MonoBehaviour
{
    public KMBombModule Module;
    public KMAudio Audio;
    
    public GameObject[][] rows = new GameObject[][]{
        new GameObject[4],
        new GameObject[4],
        new GameObject[4],
        new GameObject[4],
        new GameObject[4],
        new GameObject[4],
        new GameObject[4],
        new GameObject[4],
        new GameObject[4],
        new GameObject[4],
        new GameObject[4],
        new GameObject[4],
        new GameObject[4],
        new GameObject[4],
        new GameObject[4],
        new GameObject[4]
    };
    
    public GameObject[] rowsFlattened;
    public GameObject[] borders;
    public KMSelectable[] numpad;
    public KMSelectable reset;
    public TextMesh inputText;
    
    public bool[][] binary = new bool[][]{
         new bool[] {false, false, false, false },
         new bool[] {false, false, false, true },
         new bool[] {false, false, true, false },
         new bool[] {false, false, true, true },
         new bool[] {false, true, false, false },
         new bool[] {false, true, false, true },
         new bool[] {false, true, true, false },
         new bool[] {false, true, true, true },
         new bool[] {true, false, false, false },
         new bool[] {true, false, false, true },
         new bool[] {true, false, true, false },
         new bool[] {true, false, true, true },
         new bool[] {true, true, false, false },
         new bool[] {true, true, false, true },
         new bool[] {true, true, true, false },
         new bool[] {true, true, true, true }
    };
    
    string[] hexidecimal = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F" };
    int[] chosenBinaryIndicies = new int[16];
    string expectedString;
    string inputtedString;
    
    //Logging
    int moduleId;
    static int moduleIdCounter = 1;
    bool solved;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        foreach (KMSelectable i in numpad)
        {
            KMSelectable j = i;
            j.OnInteract += delegate { numpadHandler(j); return false; };
        }
        
        reset.OnInteract += delegate { resetHandler(); return false; };
        
        for (int i = 0; i < 16; i++)
        {
            for(int j = 0; j < 4; j++)
            {
                rows[i][j] = rowsFlattened[4 * i + j];
            }
        }
    }
    
    // Use this for initialization
    void Start()
    {
        for (int i = 0; i < 16; i++)
        {
            chosenBinaryIndicies[i] = UnityEngine.Random.Range(0, 16);
            expectedString += hexidecimal[chosenBinaryIndicies[i]];
        }
        
        for (int i = 0; i < 16; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                rows[i][j].SetActive(binary[chosenBinaryIndicies[i]][j]);
                borders[j].SetActive(true);
            }
        }
        
        Debug.LogFormat("[Pixel Number Base #{0}] The number shown on the module, in hexidecimal, is {1}.", moduleId, expectedString);
    }
    
    void numpadHandler (KMSelectable but)
    {
        but.AddInteractionPunch(.2f);
		Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		
        if (!solved)
        {
            inputtedString += but.GetComponentInChildren<TextMesh>().text;
            inputText.text = inputtedString;
            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    rows[i][j].SetActive(false);
                    borders[j].SetActive(false);
                }
            }
            
            if (inputtedString.Length == 16)
            {
                Debug.LogFormat("[Pixel Number Base #{0}] You entered {1}.", moduleId, inputtedString);
                
                if (Equals(inputtedString, expectedString))
                {
                    Debug.LogFormat("[Pixel Number Base #{0}] That was correct. Module solved.", moduleId);
                    Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
                    Module.HandlePass();
                    solved = true;
                }
                
                else
                {
                    Debug.LogFormat("[Pixel Number Base #{0}] That was incorrect. Strike!", moduleId);
                    Module.HandleStrike();
                    inputText.text = "";
                    inputtedString = "";
                    expectedString = "";
                    Start();
                }
                
            }
        }
    }

    void resetHandler()
    {
        reset.AddInteractionPunch(.2f);
		Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		
        if (!solved)
        {
            Debug.LogFormat("[Pixel Number Base #{0}] You clicked the reset button. Generating another number.", moduleId);
            inputText.text = "";
            inputtedString = "";
            expectedString = "";
            Start();
        }
    }
	
	//twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"To submit your answer, use the command !{0} type <16-digit long string> | To reset the module, use the command !{0} reset";
    #pragma warning restore 414
	
    IEnumerator ProcessTwitchCommand(string command)
    {
		string[] parameters = command.Split(' ');
		if (Regex.IsMatch(command, @"^\s*reset\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
			yield return null;
            reset.OnInteract();
		}
		
        if (Regex.IsMatch(parameters[0], @"^\s*type\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
			if (parameters.Length != 2)
			{
				yield return "sendtochaterror Invalid parameter length. The command was not processed.";
				yield break;
			}
			
			if (parameters[1].Length != 16)
			{
				yield return "sendtochaterror You must submit a 16-digit long string. The command was not processed.";
				yield break;
			}
			
			string Guideline = parameters[1].ToUpper();
		
			for (int i = 0; i < Guideline.Length; i++)
            {
                if (!Guideline[i].ToString().EqualsAny(hexidecimal))
                {
                    yield return "sendtochaterror Invalid character was detected. The command was not processed.";
                    yield break;
                }
            }
			
			for (int j = 0; j < parameters[1].Length; j++)
			{
				numpad[Array.IndexOf(hexidecimal, parameters[1][j].ToString().ToUpper())].OnInteract();
				yield return new WaitForSecondsRealtime(0.1f);
			}
        }
	}
}
