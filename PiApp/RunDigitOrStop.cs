using System.Text.RegularExpressions;

namespace PiApp
{
    /// <summary>
    /// Focusable Run that represents a digit this run must conform with where 0 is a stop
    /// </summary>
    internal class RunDigitOrStop : RunDigit
    {
        private static readonly Regex StopsRegex = new Regex(@"[\.\?\!]");
        internal static readonly int StopDigitLength = 0;
        internal static readonly char StopDigitLengthChar = '0';

        public RunDigitOrStop(int length) : base(length)
        {
        }

        protected override void SetPassiveBackground()
        {
            if (Length == StopDigitLength)
                this.Background = System.Windows.Media.Brushes.AliceBlue;
            else
                base.SetPassiveBackground();
        }

        public override bool IsValid
        {
            get
            {
                return base.IsValid
                    && WordOrHasExactlyOneStop()
                    && StopWordOrHasNoStops();
            }
        }

        private bool StopWordOrHasNoStops()
        {
            return Length == StopDigitLength
                || !StopsRegex.IsMatch(Word);
        }

        private bool WordOrHasExactlyOneStop()
        {
            return Length != StopDigitLength
                || StopsRegex.Matches(Word).Count == 1;
        }

        protected override void SetTextFromWord()
        {
            if (string.IsNullOrEmpty(Word))
                NoWordText();
            else
                WordText();
        }

        private void NoWordText()
        {
            Text = string.Format(
                (Length != StopDigitLength
                ? " {1}"
                : ".{1}"),
                Word, Length);
        }

        private void WordText()
        {
            Text = string.Format(
                (Length != StopDigitLength
                ? " {0}"
                : "{0}"),
                Word, Length);
        }
    }
}
