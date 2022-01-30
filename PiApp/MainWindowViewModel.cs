using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Windows.Input;

namespace PiApp
{

    internal class MainWindowViewModel
    {
        public FlowDocument Document
        {
            get; set;
        }
        //public RichTextBox RichTextBox { get; private set; }
        public ICommand LoadPoemOverlayCommand { get; } = new RelayCommand<MainWindowViewModel>(
            (MainWindowViewModel model) => ProcessTextFile(new OpenFileDialog(), (filename) => model.LoadPoemFromFile(filename))
            );

        public ICommand LoadPoemCommand { get; } = new RelayCommand<MainWindowViewModel>(
            (MainWindowViewModel model) => ProcessTextFile(new OpenFileDialog(), (filename) => model.ClearAndLoadPoemFromFile(filename))
            );

        public ICommand SavePoemCommand { get; } = new RelayCommand<MainWindowViewModel>(
            (MainWindowViewModel model) => ProcessTextFile(new SaveFileDialog(), (filename) => model.SavePoemToFile(filename))
            );

        public ICommand ClearPoemCommand { get; } = new RelayCommand<MainWindowViewModel>(
            (MainWindowViewModel model) => model.Clear()
            );

        public ICommand ToggleZeroSettingCommand { get; } = new RelayCommand<MainWindowViewModel>(
            (MainWindowViewModel model) => model.ZeroMeaningWasToggled()
            );

        public ICommand ToggleTitleSettingCommand { get; } = new RelayCommand<MainWindowViewModel>(
            (MainWindowViewModel model) => model.TitleMeaningWasToggled()
            );

        public bool IsZeroFullStop { get; set; }

        public bool IsTitled { get; set; }

        private RunDigit title;

        public MainWindowViewModel()
        {
            IsZeroFullStop = true;
            IsTitled = true;
            Document = new FlowDocument();
            //RichTextBox = new RichTextBox(Document);
            //RichTextBox.SpellCheck.IsEnabled = true;

            BuildDocument();
        }
        public void BuildDocument()
        {
            LoadDigits();

            LoadPoemFromString(DefaultPoemWithStops());

        }

        private void LoadDigits()
        {
            //            3.
            CreateTitle(3);

            CreateParagraphsForDigits(
            "1415926535" + "8979323846" + "2643383279" + "50" +
            "2884197169" + "39937510" +
            "5820" +
            "9749445923" + "0" +
            "781640" +
            "628620" +
            "89986280" +
            "3482534211" + "70" +
            "679"/*100 digits*/ + "821480" +
            "8651328230" +
            "66470" + // 121 (a logical end)
            "9384460" +
            "9550" +
            "5822317253" + "5940" +
            "8128481117" + "450" +
            "28410" +
            "270" +
            "193852110" +
            "5559644622" + "948954930" +
            "38196"/*200 digits*/ + "4428810" + // 207 (a logical end - next sentence is 37 words long)

            "975" + // 210 digits
            "6659334461284756482337867831652712019091456485669234603486104543266482" + // 280
            "1339360726024914127372458700660631558817488152092096282925409171536436" + // 350
            "7892590360011330530548820466521384146951941511609433057270365759591953" + // 420
            "0921861173819326117931051185480744623799627495673518857527248912279381" + // 490
            "8301194912983367336244065664308602139494639522473719070217986094370277" + // 560
            "0539217176293176752384674818467669405132000568127145263560827785771342" + // 630
            "7577896091736371787214684409012249534301465495853710507922796892589235" + // 700
            "4201995611212902196086403441815981362977477130996051870721134999999837" + // 770
            "2978049951059731732816096318595024459455346908302642522308253344685035" + // 840
            "2619311881710100031378387528865875332083814206171776691473035982534904" + // 910
            "2875546873115956286388235378759375195778185778053217122680661300192787" + // 980
            "661119590921642" + // stop at 996 digits
            // my code splits on 0's and assumes a 0 at end of every string in split, so can't end on a 0 because split creates empty string and code acts like there were two 0's at the end.
            //"01989" + // 1000 digits
            "");
        }

        private void CreateTitle(int length)
        {
            title = NewRunDigitOrStop(length);

            Paragraph titleParagraph = new Paragraph(title);
            Document.Blocks.Add(titleParagraph);

            Paragraph separator = new Paragraph();
            Document.Blocks.Add(separator);

            if (IsTitled)
            {
                titleParagraph.FontSize = 25;
                titleParagraph.TextAlignment = System.Windows.TextAlignment.Center;

                separator.Inlines.Add(new Run("___________"));
                separator.TextAlignment = System.Windows.TextAlignment.Center;
            }
            else
            {
                titleParagraph.Margin = new System.Windows.Thickness();
                separator.Margin = new System.Windows.Thickness();
            }
        }

        private RunDigit NewRunDigitOrStop(int length)
        {
            if (IsZeroFullStop)
                return new RunDigitOrStop(length);
            else
                return new RunDigit(length);
        }

        private void CreateParagraphsForDigits(string v)
        {
            string[] paragraphs = v.Split(RunDigitOrStop.StopDigitLengthChar);
            foreach (string p in paragraphs)
                CreateParagraphForDigits(p);
        }

        private void CreateParagraphForDigits(string p)
        {
            Paragraph main = new Paragraph();
            //main.FontFamily = new System.Windows.Media.FontFamily("Fixed");
            foreach (char c in p)
                CreateRunDigitForParagraph(main, int.Parse(c.ToString()));
            if (IsZeroFullStop)
                CreateRunDigitForParagraph(main, RunDigitOrStop.StopDigitLength);
            else
                CreateRunDigitForParagraph(main, 10);
            Document.Blocks.Add(main);
        }

        private void CreateRunDigitForParagraph(Paragraph main, int digit)
        {
            RunDigit run = NewRunDigitOrStop(digit);
            main.Inlines.Add(run);
        }


        private void LoadPoemFromString(string poemString)
        {
            int i = 0;

            string[] lines = StringToPoemLoadFormat(poemString);
            if (lines.Length < 3)
                return;

            title.SetWord(lines[0].Trim());

            foreach (string line in lines.Skip(2))
                if (string.IsNullOrEmpty(line?.Trim()))
                    i++;
                else
                    LoadParagraph(i++, line.Trim());
        }

        private static string DefaultPoemWithStops()
        {
            return new StringBuilder()
                            .AppendLine("Pie")
                            .AppendLine()
                            .AppendLine("I have a treat " +
                            "comprised of tastes salty and sweet, " +
                            "lovingly completed, " +
                            "causing droolings for my lip, " +
                            "diameter near excess so eating adds the hip " +
                            "diameter and so filling reaffirms peace.")
                            .AppendLine("No rational desserts (like a chocolate) provide a demand emulating the sugarplum pleasures now begging “shall I?”")
                            .AppendLine("Baked infinity, no!")
                            .AppendLine("Universes without pie’s immanence must know every existence to end.")
                            .AppendLine("Dessert, treasure a rotten life.")
                            .ToString();
        }

        private static readonly Regex EmptyLineRegex = new Regex(@"^\s*(\d+\s*)+$", RegexOptions.Multiline);
        private static readonly Regex SplitStopRegex = new Regex(@"(\S)([\.\!\?])");
        private static readonly string SplitStopReplacement = "$1 $2";
        private string[] StringToPoemLoadFormat(string poem)
        {
            if (IsZeroFullStop)
                return SplitStopRegex.Replace(
                    EmptyLineRegex.Replace(poem, string.Empty),
                    SplitStopReplacement)
                    .Split('\n');
            else
                return EmptyLineRegex.Replace(poem, string.Empty)
                .Split('\n');
        }

        private static void ProcessTextFile(FileDialog filenameRequest, Action<string> processFilename)
        {
            filenameRequest.Filter = "Text File|*.txt";
            bool? result = filenameRequest.ShowDialog();
            if (result.HasValue && result.Value)
                processFilename.Invoke(filenameRequest.FileName);
        }

        private void ClearAndLoadPoemFromFile(string filename)
        {
            Clear();
            LoadPoemFromFile(filename);
        }

        private void LoadPoemFromFile(string filename)
        {
            string poem = File.ReadAllText(filename);
            LoadPoemFromString(poem);
        }

        private void SavePoemToFile(string filename)
        {
            string poem = PoemToString();
            File.WriteAllText(filename, poem);
        }

        private void Clear()
        {
            foreach (Paragraph p in Document.Blocks.OfType<Paragraph>().ToList())
                foreach (Inline r in p.Inlines.ToList())
                    SetWord(r, string.Empty);
        }

        private string PoemToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (Paragraph p in Document.Blocks.OfType<Paragraph>())
                ParagraphToString(sb, p);

            return sb.ToString();
        }

        private static void ParagraphToString(StringBuilder sb, Paragraph p)
        {
            IEnumerable<RunDigit> words = p.Inlines.OfType<RunDigit>();

            if (words.Any((x) => !string.IsNullOrEmpty(x.Word)))
                foreach (Run r in words)
                    sb.Append(r.Text);

            sb.AppendLine();
        }

        /// <summary>
        /// Space-delimited words to load into the i'th non-title paragraph object
        /// </summary>
        /// <param name="i"></param>
        /// <param name="sentence"></param>
        private void LoadParagraph(int i, string sentence)
        {
            int titleBlocks = 2;// 1;
            //if (IsTitled)
            //    ++titleBlocks;

            Paragraph p = (Paragraph)Document.Blocks.ElementAt(titleBlocks + i);
            string[] words = sentence.Split(' ');
            for (int w = 0; w < words.Length; ++w)
                if (p.Inlines.Count > w)
                    SetWord(p.Inlines.ElementAt(w), words[w]);
        }

        private void SetWord(Inline inline, string word)
        {
            (inline as RunDigit)?.SetWord(word);
        }

        private void TitleMeaningWasToggled()
        {
            string poem = PoemToString();
            ReloadDigits();
            LoadPoemFromString(poem);
        }
        private void ZeroMeaningWasToggled()
        {
            string poem = PoemToString();
            ReloadDigits();
            LoadPoemFromString(poem);
        }

        private void ReloadDigits()
        {
            ClearDigits();
            LoadDigits();
        }

        private void ClearDigits()
        {
            Document.Blocks.Clear();
        }
    }
}
