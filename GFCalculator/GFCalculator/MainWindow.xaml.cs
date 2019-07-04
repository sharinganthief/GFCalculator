using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GFCalculator.Data;
using GFCalculator.Helpers;

namespace GFCalculator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<string> BorgNames
        {
            get { return new ObservableCollection<string>(GFData.BorgNames); }
        }

        private static int gridColumns = 5;
        private static int gridRows = 8;


        //0458fed0 - PAL
        private static int oneOnePal = 72941264;

        //72902192 - NTSC
        private static int oneOneNTSC = 72902192;
        

        public MainWindow()
        {
            InitializeComponent();
        }


        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    int num;
        //    var codeTxtValue = txtCode.Text;
        //    //0 dec, 1 hex, 2 binary
        //    var selectedFormat = comboCodeType.SelectedIndex;

        //    var baseInt = 0;

        //    switch (selectedFormat)
        //    {
        //        case 0:
        //        default:
        //            baseInt = 10;
        //            break;
        //        case 1:
        //            baseInt = 16;
        //            break;
        //        case 2:
        //            baseInt = 2;
        //            break;
        //    }

        //    //get int from provided value
        //    num = Convert.ToInt32(codeTxtValue, baseInt);
        //    PostProcessing(num);
        //}

        //private void Button2_Click(object sender, RoutedEventArgs e)
        //{
        //    PreProcessing();

        //    var forceSeq = txtForceSeq.Text;

        //    var entries = forceSeq.Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse);
        //    var num = entries.Sum(entry => Math.Pow(2.0, entry - 1));

        //    PostProcessing((int)num);
            
        //}

        private void PreProcessing()
        {
            lblDecimalVal.Content = lblHexVal.Content = lblBinaryVal.Content = lblForceSeq.Content = string.Empty;
        }
        private void PostProcessing(int number)
        {

            //get hex padded for ar before color/level mod
            var baseHexPadded = number.ToString("X").PadLeft(4, '0');

            var selectedColor = cmbColor.SelectedIndex;
            var selectedLevel = cmbLvl.SelectedIndex;

            var hasColor = selectedColor > -1;
            var hasLevel = selectedLevel > -1;

            var color = hasColor ? selectedColor : 0;
            var level = hasLevel ? selectedLevel : 0;

            SetArCode(baseHexPadded, color, level);

            string colorText = "Normal";

            if (hasColor || hasLevel)
            {
                //default selections
                cmbColor.SelectedIndex = cmbLvl.SelectedIndex = -1;

                switch (color)
                {
                    case 0: //Normal
                        colorText = "Normal";
                        if (level > 0)
                            number = level - 1;
                        else
                            number = level + 255;
                        break;
                    case 1: //Alternate
                        colorText = "Alternate";
                        if (level > 0)
                            number = level + 255;
                        else
                            number = level + 511;
                        break;
                    case 2: //Clear
                        colorText = "Clear";
                        if (level > 0)
                            number = level + 1023;
                        else
                            number = level + 1279;
                        break;
                    case 3: //Silver
                        colorText = "Silver";
                        if (level > 0)
                            number = level + 767;
                        else
                            number = level + 1023;
                        break;
                    case 4: //Gold
                        colorText = "Gold";
                        if (level > 0)
                            number = level + 511;
                        else
                            number = level + 767;
                        break;
                    case 5: //Black
                        colorText = "Black";
                        if (level > 0)
                            number = level + 1279;
                        else
                            number = level + 1535;
                        break;
                    default:
                        number = 0;
                        break;
                }
            }

            var modHexVal = number.ToString("X");

            var modHexPadded = modHexVal.PadLeft(4, '0');

            //get binary
            var binaryVal = Convert.ToString(number, 2);

            var binRev = new string(binaryVal.Reverse().ToArray());

            var indexes = binRev.FindAllIndexOf("1");
            var forceSeqCode = string.Join(",", indexes.Select(o => o + 1));

            lblDecimalVal.Content = number;
            lblHexVal.Content = modHexPadded;
            lblBinaryVal.Content = binaryVal;
            lblColor.Content = colorText;
            lblLevel.Content = level + 1;
            lblForceSeq.Content = forceSeqCode;
        }

        private void SetArCode(string hexPadded, int color, int level)
        {
            //AR code is two parts, location and borg info
            //location starts with one of two hex values, on PAL one NTSC

            //Get start position
            var startPosition = ntscRadio.IsChecked != null && (bool) ntscRadio.IsChecked ? oneOneNTSC : oneOnePal;
            
            
            //Get desired row/col, default to 8 and/or 25 - default last cell on neither selection
            var selectedRow = cmbRow.SelectedIndex;
            var selectedCol = cmbCol.SelectedIndex;

            cmbCol.SelectedIndex = cmbRow.SelectedIndex = -1;


            var haveRow = selectedRow > -1;
            var haveCol = selectedCol > -1;

            var desiredRow = haveRow ? selectedRow : 7;
            var desiredCol = haveCol ? selectedCol : 25;

            //lblGrid.Content = $"{(haveRow ? desiredRow + 1: desiredRow)}x{(haveCol ? selectedCol + 1 : selectedCol)}";
            lblGrid.Content = $"{desiredRow + 1}x{desiredCol + 1}";

            //Math to get us the selected row/col value in dec
            //var index = ((desiredRow) * 25) + (desiredCol);
            //"T" - indexing uses the index of(g, r, c) as 40g + 5r + c and by inverting that formula, c = mod(i, 5), r = mod((i - c) / 5, 8) and g = (i - 5r - c)/ 40.

            //need to find grid as we are outside the first
            var grid = desiredCol > 0 ? (desiredCol / 5) : 0;

            //remainder gives us non-zero index of col
            var realColumnValue = desiredCol % gridColumns;
            
            //if (desiredCol > 5 && realColumnValue == 0)
            //{
                
            //}

            var index = (40 * grid) + (5 * desiredRow) + realColumnValue;

            var increaseToStart = index * 32;

            var finalDec = startPosition + increaseToStart;

            //need 8 characters for final position
            var hexLocation = finalDec.ToString("X").PadLeft(8, '0');

            //pad color and level to get remaining 4 digits on borg info
            var paddedColor = color.PadLeftZeroes(2);
            var paddedLevel = level.PadLeftZeroes(2);

            //final code, location borg code|color code|level code 
            lblArCode.Content = $"{hexLocation} {hexPadded}{paddedColor}{paddedLevel}";
        }

        private void PreviewTextInput_EnhanceComboSearch(object sender, TextCompositionEventArgs e)
        {
            var cmb = (ComboBox)sender;

            cmb.IsDropDownOpen = true;

            HandleCombo(cmb, cmb.Text, e.Text);
        }

        private void PreviewKeyUp_EnhanceComboSearch(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            var cmb = (ComboBox)sender;

            if (string.IsNullOrWhiteSpace(cmb.Text))
            {
                cmb.ItemsSource = GFData.BorgNames;
                return;
            }

            PopulateFields();

        }

        private void Pasting_EnhanceComboSearch(object sender, DataObjectPastingEventArgs e)
        {
            var cmb = (ComboBox)sender;

            cmb.IsDropDownOpen = true;

            var pastedText = (string)e.DataObject.GetData(typeof(string)) ?? string.Empty;

            HandleCombo(cmb, pastedText);
        }

        private void HandleCombo(ComboBox cmb, string firstText, string secondText = null)
        {
            if (string.IsNullOrWhiteSpace(cmb.Text))
            {
                return;
            }

            if (!string.IsNullOrEmpty(firstText))
            {
                cmb.ItemsSource = GetNames(firstText);
            }
            else if (!string.IsNullOrEmpty(secondText))
            {
                cmb.ItemsSource = GetNames(secondText);
            }
            else
            {
                cmb.ItemsSource = GFData.BorgNames;
            }
        }

        private IList GetNames(string firstText)
        {
            return GFData.BorgNames.Where(s => s.IndexOf(firstText, StringComparison.InvariantCultureIgnoreCase) != -1).ToList();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PopulateFields();
        }

        private void PopulateFields()
        {
            cmbBorg.IsEnabled = btnCalc.IsEnabled = btnClear.IsEnabled = false;

            try
            {
                var borgName = cmbBorg.Text;

                if (string.IsNullOrWhiteSpace(cmbBorg.Text))
                {
                    return;
                }

                var borg = GFData.Current[borgName];

                if (borg == null)
                {
                    return;
                }

                cmbBorg.Text = string.Empty;
                HandleCombo(cmbBorg, null);
                
                lblBorgName.Content = borg.Name;

                var num = Convert.ToInt32(borg.HexCode, 16);

                PostProcessing(num);
            }
            finally
            {
                cmbBorg.IsEnabled = btnCalc.IsEnabled = btnClear.IsEnabled = true;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ResetFormFields();
        }

        private void ResetFormFields()
        {
            cmbLvl.SelectedIndex = cmbBorg.SelectedIndex = cmbColor.SelectedIndex = cmbCol.SelectedIndex = cmbRow.SelectedIndex = -1;
            lblBorgName.Content = lblDecimalVal.Content = lblBinaryVal.Content = lblForceSeq.Content = lblHexVal.Content = lblArCode.Content = string.Empty;
            HandleCombo(cmbBorg, null);
        }

        private void OutputLabel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var lbl = (Label) sender;
            Clipboard.SetText(lbl.Content.ToString());
        }
    }
}
