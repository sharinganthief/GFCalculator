using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
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

        private static string Pal_ARForFormat = "02591550 0000{0}";
        private static string NTSC_ARForFormat = "02587F10 0000{0}";

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
            var hexVal = number.ToString("X");

            var hexPadded = hexVal.PadLeft(4, '0');

            //get binary
            var binaryVal = Convert.ToString(number, 2);
            
            var binRev = new string(binaryVal.Reverse().ToArray());

            var indexes = binRev.FindAllIndexOf("1");
            var forceSeqCode = string.Join(",", indexes.Select(o => o + 1));

            if (ntscRadio.IsChecked != null && (bool) ntscRadio.IsChecked)
            {
                lblArCode.Content = string.Format(NTSC_ARForFormat, hexPadded);
            }
            else
            {
                lblArCode.Content = string.Format(Pal_ARForFormat, hexPadded);
            }

            lblDecimalVal.Content = number;
            lblHexVal.Content = hexPadded;
            lblBinaryVal.Content = binaryVal;
            lblForceSeq.Content = forceSeqCode;
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
            //var fullText = cmb.Text.Insert(WPFHelpers.GetChildOfType<TextBox>(cmb).CaretIndex, pastedText);

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
                //var fullText = cmb.Text.Insert(WPFHelpers.GetChildOfType<TextBox>(cmb).CaretIndex, firstText);
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

            cmbBorg.IsEnabled = false;

            lblBorgName.Content = borg.Name;

            var num = Convert.ToInt32(borg.HexCode, 16);

            PostProcessing(num);

            cmbBorg.IsEnabled = true;
        }
    }
}
