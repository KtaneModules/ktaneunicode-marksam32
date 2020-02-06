using UnityEngine;
using Unicode;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using KModkit;
using System;

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

    private readonly char[] symbols = new char[44] { '§', '¶', 'Ħ', 'Ӕ', 'ſ', 'Ƕ', 'Ƿ', '⁂', 'ͼ', 'ς', 'Ћ', '₪', 'Ю', 'Ѡ', 'Ѭ', '₰', '∯', '∫', '╩', 'Ӭ', '☊', '֍', '☦', 'ﬡ', '⚙', 'Ω', 'ꀍ', '▒', '╋', '⌘', '∴', '∅', '℄', 'Ҩ', '★', 'ƛ', 'Ϫ', 'ت', 'ټ', 'غ', 'ں', 'þ', 'Ɣ', 'ȹ' };
    private readonly string[] codes = new string[44] { "00A7", "00B6", "0126", "04D4", "017F", "01F6", "01F7", "2042", "037C", "03C2", "040B", "20AA", "042E", "0460", "046C", "20B0", "222F", "222B", "2569", "04EC", "260A", "058D", "2626", "FB21", "2699", "03A9", "A00D", "2592", "254B", "2318", "2234", "2205", "2104", "04A8", "2605", "019B", "03EA", "062A", "067C", "063A", "06BA", "00FE", "0194", "0239" };
    private List<char> DisplaySymbols = new List<char>();

    private static int _moduleIdCounter = 1; 
    private int _moduleId = 0;
    private bool isSolved = false;

    private readonly string CorrectText = "That is correct, good job! :D";
    private readonly string WrongText = "That is incorrect, bad job! D:";

    // Use this for initialization
    void Start()
    {
        //IList<int> temp = new List<int> { 4, 1, 2, 3 };
        //IList<SymbolInfo> si = new List<SymbolInfo>
        //{
        //    new SymbolInfo
        //    {
        //        Code = "1111",
        //        Symbol = '1',
        //    },
        //    new SymbolInfo
        //    {
        //        Code = "2222",
        //        Symbol = '2',
        //    },
        //    new SymbolInfo
        //    {
        //        Code = "3333",
        //        Symbol = '3',
        //    },
        //    new SymbolInfo
        //    {
        //        Code = "4444",
        //        Symbol = '4',
        //    }
        //};
        //si = OrderBy(si, temp);

        //Debug.LogFormat(string.Join(", ", si.Select(x => x.Code).ToArray()));
        //Debug.LogFormat(string.Join(", ", si.Select(x => x.Symbol.ToString()).ToArray()));

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
        for (int i = 0; i < 4; ++i)
        {
            SymbolsScreen[i].text = DisplaySymbols[i].ToString();
        }

         UPlusButton.OnInteract += delegate
         {
             StartCoroutine("SolveAnimation", true);
             return false;
         };

        for(int i = 0; i < Buttons.Length; ++i)
        {
            var index = i;
            Buttons[index].OnInteract += delegate
            {
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
        ApplyRules();
    }

    private void ApplyRules()
    {
        List<int> sortOrder = new List<int>();
        if (Info.GetBatteryCount() == 2 && Info.IsIndicatorOn(Indicator.BOB) && SelectedSymbols.Any(x => x.Code.Contains("0")) && Has2B())
        {
            sortOrder = new List<int> { 1, 2, 3, 4 };
            SelectedSymbols = OrderBy(SelectedSymbols, sortOrder);
        }
        else if (SelectedSymbols[2].Code.Contains("A") || SelectedSymbols[2].Code.Contains("B") || SelectedSymbols[2].Code.Contains("C"))
        {
            sortOrder = new List<int> { 3, 4, 2, 1 };
            SelectedSymbols = OrderBy(SelectedSymbols, sortOrder);
        }
        else if (SelectedSymbols[1].Code.Contains("1") || SelectedSymbols[1].Code.Contains("2") || SelectedSymbols[1].Code.Contains("3"))
        {
            sortOrder = new List<int> { 3, 1, 4, 2 };
            SelectedSymbols = OrderBy(SelectedSymbols, sortOrder);
        }
        else if (NumberOfDEFEven() ^ Info.GetPortCount() % 2 == 0)
        {
            sortOrder = new List<int> { 2, 4, 1, 3 };
            SelectedSymbols = OrderBy(SelectedSymbols, sortOrder);
        }
        else if (OddDigitsGraterThanFive())
        {
            sortOrder = new List<int> { 1, 2, 4, 3 };
            SelectedSymbols = OrderBy(SelectedSymbols, sortOrder);
        }
        else if (FirstSymbolHexValue())
        {
            sortOrder = new List<int> { 4, 3, 2, 1 };
            SelectedSymbols = OrderBy(SelectedSymbols, sortOrder);
        }
        else if (EdgeworkIsGraterThanDigits())
        {
            sortOrder = new List<int> { 1, 4, 3, 2 };
            SelectedSymbols = OrderBy(SelectedSymbols, sortOrder);
        }
        else if (!SelectedSymbols[3].Code.Contains(Info.GetSerialNumberNumbers().Last().ToString()))
        {
            sortOrder = new List<int> { 2, 3, 4, 1 };
            SelectedSymbols = OrderBy(SelectedSymbols, sortOrder);
        }
        else if (HasInCommonWithSerialNumber())
        {

        }
        else if (DigitsGraterThatLetters())
        {
            sortOrder = new List<int> { 1, 3, 4, 2 };
            SelectedSymbols = OrderBy(SelectedSymbols, sortOrder);
        }
        //else if ()
        //{

        //}
    }

    private void HandlePress (int index)
    {
        Debug.LogFormat(index.ToString());
    }

    private bool Has2B()
    {
        return string.Join(string.Empty, SelectedSymbols.Select(x => x.Code).ToArray()).Count(x => "B".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase))  >= 2;
    }

    private bool HasInCommonWithSerialNumber()
    {
            var serialNumber = Info.GetSerialNumber().ToLowerInvariant().ToArray();
            List<char> Letters = new List<char>();
            foreach(char letter in serialNumber)
            {
                if (letter.Equals("a") || letter.Equals("b") || letter.Equals("c") || letter.Equals("d") || letter.Equals("e") || letter.Equals("f"))
                {
                    Letters.Add(letter);
                }
            }
    }

    private bool NumberOfDEFEven()
    {
        int NumberOfD;
        int NumberOfE;
        int NumberOfF;

        NumberOfD = string.Join(string.Empty, SelectedSymbols.Select(x => x.Code).ToArray()).Count(x => "D".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase));
        NumberOfE = string.Join(string.Empty, SelectedSymbols.Select(x => x.Code).ToArray()).Count(x => "E".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase));
        NumberOfF = string.Join(string.Empty, SelectedSymbols.Select(x => x.Code).ToArray()).Count(x => "F".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase));

        return NumberOfD + NumberOfE + NumberOfE % 2 == 0;
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
        TextArray.text = "";

        yield return new WaitForSecondsRealtime(.1f);
        if (correct)
        {
            var CorrectTextChar = CorrectText.ToCharArray();
            string text = "";
            for (int i = 0; i < CorrectTextChar.Length; ++i)
            {
                text = text + CorrectTextChar[i];
                TextArray.text = text;
                yield return new WaitForSecondsRealtime(.1f);
            }
        }
        else
        {
            var WrongTextChar = WrongText.ToCharArray();
            string text = "";
            for (int i = 0; i < WrongTextChar.Length; ++i)
            {
                text = text + WrongTextChar[i];
                TextArray.text = text;
                yield return new WaitForSecondsRealtime(.1f);
            }
        }
    }

    private static IList<SymbolInfo> OrderBy(IList<SymbolInfo> selectedSymbols, IList<int> order)
    {
        return order.Select(x => selectedSymbols[x -1]).ToList();
    }

	// Update is called once per frame
	void Update ()
    {
		
	}
}
