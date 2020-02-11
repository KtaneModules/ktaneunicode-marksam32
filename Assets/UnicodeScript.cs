using UnityEngine;
using Unicode;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using KModkit;
using System;
using System.Text.RegularExpressions;
using rnd = UnityEngine.Random;

public class UnicodeScript : MonoBehaviour
{
    public KMAudio Audio;
    public KMBombModule Module;
    public KMBombInfo Info;

    public KMSelectable[] Buttons;
    public KMSelectable UPlusButton;

    public TextMesh[] SymbolsScreen;
    public TextMesh TextArray;

    private IList<SymbolInfo> Symbols = new List<SymbolInfo>();

    private IList<SymbolInfo> SelectedSymbols = new List<SymbolInfo>();

    private readonly char[] symbols = new char[44] { '§', '¶', 'Ħ', 'Ӕ', 'ſ', 'Ƕ', 'Ƿ', '⁂', 'ͼ', 'ς', 'Ћ', '₪', 'Ю', 'Ѡ', 'Ѭ', '₰', '∯', '∫', '╩', 'Ӭ', '☊', '֍', '☦', 'ﬡ', 'ш', 'Ω', 'փ', '▒', '╋', '⌘', '∴', '∅', '℄', 'Ҩ', '★', 'ƛ', 'Ϫ', 'ت', 'ټ', 'غ', 'ں', 'þ', 'Ɣ', 'ȹ' };
    private readonly string[] codes = new string[44] { "00A7", "00B6", "0126", "04D4", "017F", "01F6", "01F7", "2042", "037C", "03C2", "040B", "20AA", "042E", "0460", "046C", "20B0", "222F", "222B", "2569", "04EC", "260A", "058D", "2626", "FB21", "0428", "03A9", "0583", "2592", "254B", "2318", "2234", "2205", "2104", "04A8", "2605", "019B", "03EA", "062A", "067C", "063A", "06BA", "00FE", "0194", "0239" };
    private List<char> DisplaySymbols = new List<char>();

    private List<char> PressedButtons = new List<char>();
    private int stage = 1;

    private static int _moduleIdCounter = 1; 
    private int _moduleId = 0;
    private bool isSolved = false;
    private bool Interactable = true;

    private string DisplayedText;

    private bool UPlusButtonPressed = false;

    private static readonly List<char> AllowedSerialNumberLetters = new List<char> { 'a', 'b', 'c', 'd', 'e', 'f' };

    private readonly string CorrectText = "That is correct, good job! :D";
    private readonly string WrongText = "That is incorrect, bad job! D:";

    private readonly string[] SolveTexts = { "Nice", "Neat", "Good", "Okay", "Cool" }; 

    private bool TPStrike = false;

    private readonly static Regex TwitchPlaysRegex = new Regex("^submit ([0-9 a-f]{4}) ([0-9 a-f]{4}) ([0-9 a-f]{4}) ([0-9 a-f]{4})$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    // Use this for initialization
    void Start()
    {        
        for (int i = 0; i < symbols.Length; ++i)
        {
            Symbols.Add(new SymbolInfo
            {
                Symbol = symbols[i],
                Code = codes[i]
            });
        }
        _moduleId = _moduleIdCounter++;
        DetermineCorrectOrder();
        Module.OnActivate += Activate;
    }

    void Activate()
    {
        TextArray.text = "";
        for (int i = 0; i < 4; ++i)
        {
            SymbolsScreen[i].text = DisplaySymbols[i].ToString();
        }
    
        Debug.LogFormat("[Unicode #{0}] The symbols are: {1}. and the codes are: {2}", _moduleId, string.Join(", ", SelectedSymbols.Select(x => x.Symbol.ToString()).ToArray()), string.Join(", ", SelectedSymbols.Select(x => x.Code).ToArray()));

        ApplyRules();

        Debug.LogFormat("[Unicode #{0}] The symbols in the correct order are: {1}. and the codes are: {2}", _moduleId, string.Join(", ", SelectedSymbols.Select(x => x.Symbol.ToString()).ToArray()), string.Join(", ", SelectedSymbols.Select(x => x.Code).ToArray()));
        UPlusButton.OnInteract += delegate
         {
             Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
             UPlusButton.AddInteractionPunch(.3f);

             if (isSolved || !Interactable)
             {
                 return false;
             }
             if (!UPlusButtonPressed)
             {
                 AddToTextArray("U+", false);
             }
             UPlusButtonPressed = true;
             return false;
         };

        for(int i = 0; i < Buttons.Length; ++i)
        {
            var index = i;          
            Buttons[index].OnInteract += delegate
            {
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
                UPlusButton.AddInteractionPunch(.3f);
                if (isSolved || !Interactable)
                {
                    return false;
                }
                HandlePress(index);
                return false;
            };
        }
    }

    private void DetermineCorrectOrder()
    {
        for (int i = 0; i < 4; ++i)
        {
            var pickedSymbol = Symbols.PickRandom();
            while (SelectedSymbols.FirstOrDefault(x => x.Symbol.Equals(pickedSymbol.Symbol)) != null)
            {
                pickedSymbol = Symbols.PickRandom();
            }
            SelectedSymbols.Add(pickedSymbol);
        }

        for (int i = 0; i < 4; ++i)
        {
            DisplaySymbols.Add(SelectedSymbols[i].Symbol);
        }
    }

    private void ApplyRules()
    {
        List<int> sortOrder = new List<int>();
        if (Info.GetBatteryCount() == 2 && Info.IsIndicatorOn(Indicator.BOB) && SelectedSymbols.Any(x => x.Code.Contains("0")) && Has2B())
        {
            sortOrder = new List<int> { 1, 2, 3, 4 };
            Debug.LogFormat("[Unicode #{0}] Rule 1 applies. Sort order is 1 2 3 4.", _moduleId);
            SelectedSymbols = OrderBy(SelectedSymbols, sortOrder);
        }
        else if (SelectedSymbols[2].Code.Contains("A"))
        {
            sortOrder = new List<int> { 3, 4, 2, 1 };
            Debug.LogFormat("[Unicode #{0}] Rule 2 applies. Sort order is 3 4 2 1.", _moduleId);
            SelectedSymbols = OrderBy(SelectedSymbols, sortOrder);
        }
        else if (SelectedSymbols[1].Code.Contains("1"))
        {
            sortOrder = new List<int> { 3, 1, 4, 2 };
            Debug.LogFormat("[Unicode #{0}] Rule 3 applies. Sort order is 3 1 2 4.", _moduleId);
            SelectedSymbols = OrderBy(SelectedSymbols, sortOrder);
        }
        else if (NumberOfDEFEven() ^ Info.GetPortCount() % 2 != 0)
        {
            sortOrder = new List<int> { 2, 4, 1, 3 };
            Debug.LogFormat("[Unicode #{0}] Rule 4 applies. Sort order is 2 4 1 3.", _moduleId);
            SelectedSymbols = OrderBy(SelectedSymbols, sortOrder);
        }
        else if (OddDigitsGraterThanFive())
        {
            sortOrder = new List<int> { 1, 2, 4, 3 };
            Debug.LogFormat("[Unicode #{0}] Rule 5 applies. Sort order is 1 2 4 3.", _moduleId);
            SelectedSymbols = OrderBy(SelectedSymbols, sortOrder);
        }
        else if (FirstSymbolHexValue())
        {
            sortOrder = new List<int> { 4, 3, 2, 1 };
            Debug.LogFormat("[Unicode #{0}] Rule 6 applies. Sort order is 4 3 2 1.", _moduleId);
            SelectedSymbols = OrderBy(SelectedSymbols, sortOrder);
        }
        else if (EdgeworkIsGraterThanDigits())
        {
            sortOrder = new List<int> { 1, 4, 3, 2 };
            Debug.LogFormat("[Unicode #{0}] Rule 7 applies. Sort order is 1 4 3 2.", _moduleId);
            SelectedSymbols = OrderBy(SelectedSymbols, sortOrder);
        }
        else if (!SelectedSymbols[3].Code.Contains(Info.GetSerialNumberNumbers().Last().ToString()))
        {
            sortOrder = new List<int> { 2, 3, 4, 1 };
            Debug.LogFormat("[Unicode #{0}] Rule 8 applies. Sort order is 2 3 4 1.", _moduleId);
            SelectedSymbols = OrderBy(SelectedSymbols, sortOrder);
        }
        else if (HasInCommonWithSerialNumber())
        {
            sortOrder = new List<int> { 4, 1, 2, 3 };
            Debug.LogFormat("[Unicode #{0}] Rule 10 applies. Sort order is 4 1 2 3.", _moduleId);
            SelectedSymbols = OrderBy(SelectedSymbols, sortOrder);
        }
        else if (DigitsGraterThatLetters())
        {
            sortOrder = new List<int> { 1, 3, 4, 2 };
            Debug.LogFormat("[Unicode #{0}] Rule 11 applies. Sort order is 1 3 4 2.", _moduleId);
            SelectedSymbols = OrderBy(SelectedSymbols, sortOrder);
        }
        else if (WhenAllConcatinated())
        {
            sortOrder = new List<int> { 2, 3, 1, 4 };
            Debug.LogFormat("[Unicode #{0}] Rule 12 applies. Sort order is 2 3 1 4.", _moduleId);
            SelectedSymbols = OrderBy(SelectedSymbols, sortOrder);
        }
        else if(IsDigitsOrLettersOnly())
        {
            sortOrder = new List<int> { 3, 2, 1, 4 };
            Debug.LogFormat("[Unicode #{0}] Rule 13 applies. Sort order is 3 2 1 4.", _moduleId);
            SelectedSymbols = OrderBy(SelectedSymbols, sortOrder);
        }
        else
        {
            sortOrder = new List<int> { 4, 2, 1, 3 };
            Debug.LogFormat("[Unicode #{0}] Rule 14 applies. Sort order is 4 2 1 3.", _moduleId);
            SelectedSymbols = OrderBy(SelectedSymbols, sortOrder);
        }
    }

    private void HandlePress (int index)
    {
        if (UPlusButtonPressed)
        {          
            switch (index)
            {
                case 0:
                    PressedButtons.Add('0');
                    AddToTextArray("0", false);
                    break;
                case 1:
                    PressedButtons.Add('1');
                    AddToTextArray("1", false);
                    break;
                case 2:
                    PressedButtons.Add('2');
                    AddToTextArray("2", false);
                    break;
                case 3:
                    PressedButtons.Add('3');
                    AddToTextArray("3", false);
                    break;
                case 4:
                    PressedButtons.Add('4');
                    AddToTextArray("4", false);
                    break;
                case 5:
                    PressedButtons.Add('5');
                    AddToTextArray("5", false);
                    break;
                case 6:
                    PressedButtons.Add('6');
                    AddToTextArray("6", false);
                    break;
                case 7:
                    PressedButtons.Add('7');
                    AddToTextArray("7", false);
                    break;
                case 8:
                    PressedButtons.Add('8');
                    AddToTextArray("8", false);
                    break;
                case 9:
                    PressedButtons.Add('9');
                    AddToTextArray("9", false);
                    break;
                case 10:
                    PressedButtons.Add('A');
                    AddToTextArray("A", false);
                    break;
                case 11:
                    PressedButtons.Add('B');
                    AddToTextArray("B", false);
                    break;
                case 12:
                    PressedButtons.Add('C');
                    AddToTextArray("C", false);
                    break;
                case 13:
                    PressedButtons.Add('D');
                    AddToTextArray("D", false);
                    break;
                case 14:
                    PressedButtons.Add('E');
                    AddToTextArray("E", false);
                    break;
                default:
                    PressedButtons.Add('F');
                    AddToTextArray("F", false);
                    break;
            }
            if (PressedButtons.ToArray().Length == 4)
            {
                UPlusButtonPressed = false;
                if (SelectedSymbols[stage - 1].Code == string.Join("", PressedButtons.Select(x => x.ToString()).ToArray()))
                {
                    if(stage == 4)
                    {
                        Debug.LogFormat("[Unicode #{0}] Entered: {1}. That is correct.", _moduleId, string.Join("", PressedButtons.Select(x => x.ToString()).ToArray()));
                        Debug.LogFormat("[Unicode #{0}] Module solved.", _moduleId);
                        StartCoroutine("SolveAnimation", true);
                    }
                    else
                    {
                        Debug.LogFormat("[Unicode #{0}] Entered: {1}. That is correct.", _moduleId, string.Join("", PressedButtons.Select(x => x.ToString()).ToArray()));
                        stage++;
                        AddToTextArray(" ", false);
                        PressedButtons.Clear();
                    }
                }
                else
                {
                    Debug.LogFormat("[Unicode #{0}] Entered: {1}. That is incorrect. Expected: {2}.", _moduleId, string.Join("", PressedButtons.Select(x => x.ToString()).ToArray()), SelectedSymbols[stage - 1].Code);
                    PressedButtons.Clear();
                    stage = 1;
                    StartCoroutine("SolveAnimation", false);
                }
            }
        }
    }

    private bool IsDigitsOrLettersOnly()
    {
        var codes = SelectedSymbols.Select(x => x.Code).ToArray();

        for(int i = 0; i < 4; ++i)
        {
            if (codes[i].All(char.IsLetter))
            {
                return true;
            }
            if (codes[i].All(char.IsDigit))
            {
                return true;
            }
        }
        return false;
    }

    private bool WhenAllConcatinated()
    {
        var concatinatedString = string.Join("", SelectedSymbols.Select(x => x.Code).ToArray());
        if (concatinatedString.Contains("1A") || concatinatedString.Contains("2B") || concatinatedString.Contains("3C") || concatinatedString.Contains("4D") || concatinatedString.Contains("5E") || concatinatedString.Contains("6F"))
        {
            return true;
        }
        return false;
    }

    private bool Has2B()
    {
        return string.Join(string.Empty, SelectedSymbols.Select(x => x.Code).ToArray()).Count(x => "B".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase))  >= 2;
    }

    private bool HasInCommonWithSerialNumber()
    {
        var codes = string.Join(string.Empty, SelectedSymbols.Take(2).Select(x => x.Code).ToArray()).ToLowerInvariant().ToArray();
        var remainingCodes = AllowedSerialNumberLetters.Where(x => codes.Contains(x)).ToList();

        if (remainingCodes.Count == 0)
        {
            return false;
        }

        var serialNumber = Info.GetSerialNumber().ToLowerInvariant().ToArray();
        var remainingSerialLetters = AllowedSerialNumberLetters.Where(x => serialNumber.Contains(x)).ToList();

        if (remainingSerialLetters.Count == 0)
        {
            return false;
        }

        return remainingSerialLetters.Except(remainingCodes).Count() == 0;
    }

    private bool NumberOfDEFEven()
    {
        int NumberOfD;
        int NumberOfE;
        int NumberOfF;

        NumberOfD = string.Join(string.Empty, SelectedSymbols.Select(x => x.Code).ToArray()).Count(x => "D".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase));
        NumberOfE = string.Join(string.Empty, SelectedSymbols.Select(x => x.Code).ToArray()).Count(x => "E".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase));
        NumberOfF = string.Join(string.Empty, SelectedSymbols.Select(x => x.Code).ToArray()).Count(x => "F".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase));

        return NumberOfD + NumberOfE + NumberOfF % 2 == 0;
    }

    private bool OddDigitsGraterThanFive()
    {
        int OddDigits = string.Join(string.Empty, SelectedSymbols.Select(x => x.Code).ToArray()).Count(x => "1".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase) || "3".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase) || "5".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase) || "7".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase) || "9".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase) || "B".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase) || "D".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase) || "F".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase));

        return OddDigits > 5;
    }

    private bool FirstSymbolHexValue()
    {
        int symbolValue = Convert.ToInt32(SelectedSymbols[0].Code, 16);

        return symbolValue > Convert.ToInt32("1FFF", 16);
    }

    private bool EdgeworkIsGraterThanDigits()
    {
        int edgework = Info.GetPortCount() + Info.GetIndicators().Count() + Info.GetBatteryCount();
        int symbolDigits = string.Join(string.Empty, SelectedSymbols.Select(x => x.Code).ToArray()).Count(x => "1".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase) || "2".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase) || "3".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase) || "4".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase) || "5".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase) || "6".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase) || "7".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase) || "8".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase) || "9".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase) || "0".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase));

        return edgework > symbolDigits;
    }

    private bool DigitsGraterThatLetters()
    {
        int letters = string.Join(string.Empty, SelectedSymbols.Select(x => x.Code).ToArray()).Count(x => "A".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase) || "B".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase) || "C".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase) || "D".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase) || "E".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase) || "F".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase));
        int digits = string.Join(string.Empty, SelectedSymbols.Select(x => x.Code).ToArray()).Count(x => "1".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase) || "2".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase) || "3".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase) || "4".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase) || "5".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase) || "6".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase) || "7".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase) || "8".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase) || "9".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase) || "0".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase));
        return digits > letters;
    }

    private IEnumerator SolveAnimation(bool correct)
    {
        Interactable = false;
        if (correct)
        {
            isSolved = true;
            yield return new WaitForSecondsRealtime(.1f);
            var CorrectTextChar = CorrectText.ToCharArray();
            string text = "";
            for (int i = 0; i < CorrectTextChar.Length; ++i)
            {
                text = text + CorrectTextChar[i];
                TextArray.text = text;
                yield return new WaitForSecondsRealtime(.1f);
            }
            Module.HandlePass();
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
            var selectedMessage = SolveTexts[rnd.Range(0, SolveTexts.Length)].ToCharArray();
            for (int i = 0; i < 4; ++i)
            {
                SymbolsScreen[i].text = selectedMessage[i].ToString();
                yield return new WaitForSecondsRealtime(.2f);
            }
        }
        else
        {
            TPStrike = true;
            yield return new WaitForSecondsRealtime(.1f);
            var WrongTextChar = WrongText.ToCharArray();
            string text = "";
            for (int i = 0; i < WrongTextChar.Length; ++i)
            {
                text = text + WrongTextChar[i];
                TextArray.text = text;
                yield return new WaitForSecondsRealtime(.1f);
            }
            Module.HandleStrike();
            yield return new WaitForSecondsRealtime(.5f);
            TextArray.text = "";
            AddToTextArray("", true);
            TPStrike = false;
        }
        Interactable = true;
    }

    private static IList<SymbolInfo> OrderBy(IList<SymbolInfo> selectedSymbols, IList<int> order)
    {
        return order.Select(x => selectedSymbols[x -1]).ToList();
    }

    private void AddToTextArray(string add, bool clear)
    {
        if (clear)
        {
            DisplayedText = "";
            TextArray.text = DisplayedText;          
        }
        else
        {
            DisplayedText = DisplayedText + add;
            TextArray.text = DisplayedText;
        }      
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = "Submit your answer by entering !{0} 1234 abcd 1234 abcd.";
#pragma warning restore 414

    public IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant().Trim();
        Match m = TwitchPlaysRegex.Match(command);

        if (m.Success)
        {
            yield return null;
            if (!Interactable)
            {
                yield return string.Format("sendtochaterror Please wait until the solve/strike animation is over!");
                yield break;
            }
            for(int i = 0; i < 4; ++i)
            {
                UPlusButton.OnInteract();
                int[] group = GroupToInt(m.Groups[i + 1].ToString().ToCharArray());
                for(int x = 0; x < 4; ++x)
                {
                    Buttons[group[x]].OnInteract();
                    yield return new WaitForSecondsRealtime(.1f);
                }
                if (TPStrike)
                {
                    yield return "strike";
                    yield break;
                }
            }
                
        }
        if (isSolved)
        {
            yield return "solve";
        }
        yield break;
    }

    private int[] GroupToInt(char[] group)
    {
        List<int> indexes = new List<int>();
        for(int i = 0; i < 4; ++i)
        {
            switch (group[i])
            {
                case 'a':
                    indexes.Add(10);
                    break;
                case 'b':
                    indexes.Add(11);
                    break;
                case 'c':
                    indexes.Add(12);
                    break;
                case 'd':
                    indexes.Add(13);
                    break;
                case 'e':
                    indexes.Add(14);
                    break;
                case 'f':
                    indexes.Add(15);
                    break;
                default:
                    indexes.Add(int.Parse(group[i].ToString()));
                    break;
            }
        }
        return indexes.ToArray();  
    }

    // Update is called once per frame
    void Update ()
    {

	}
}
