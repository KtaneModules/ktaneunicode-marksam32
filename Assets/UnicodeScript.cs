using UnityEngine;
using Unicode;
using System.Linq;
using System.Collections.Generic;
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
        for (int i = 0; i < 4; ++i)
        {
            SymbolsScreen[i].text = DisplaySymbols[i].ToString();
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
        if (Info.GetBatteryCount() == 2 && Info.IsIndicatorOn(Indicator.BOB) && SelectedSymbols.Any(x => x.Code.Contains("0")) && Has2B())
        {
            return;
        }
        else if (SelectedSymbols[2].Code.Contains("A") || SelectedSymbols[2].Code.Contains("B") || SelectedSymbols[2].Code.Contains("C"))
        {

        }
        else if (SelectedSymbols[1].Code.Contains("1") || SelectedSymbols[1].Code.Contains("2") || SelectedSymbols[1].Code.Contains("3"))
        {

        }
        else if (NumberOfDEFEven() ^ Info.GetPortCount() % 2 == 0)
        {

        }
        else if (OddDigitsGraterThanFive())
        {

        }
        else if (FirstSymbolHexValue())
        {

        }
        else if (EdgeworkIsGraterThanDigits())
        {

        }
        else if (!SelectedSymbols[3].Code.Contains(Info.GetSerialNumberNumbers().Last().ToString()))
        {

        }
        else if()
    }

    private bool Has2B()
    {
        return string.Join(string.Empty, SelectedSymbols.Select(x => x.Code).ToArray()).Count(x => "B".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase))  >= 2;
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
        int symbolDigits = string.Join(string.Empty, SelectedSymbols.Select(x => x.Code).ToArray()).Count(x => "1".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase) || "2".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase) || "3".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase) || "4".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase) || "5".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase) || "6".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase) || "7".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase) || "8".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase) || "9".Equals(x.ToString(), StringComparison.InvariantCultureIgnoreCase));

        return edgework > symbolDigits;
    }

	// Update is called once per frame
	void Update ()
    {
		
	}
}
