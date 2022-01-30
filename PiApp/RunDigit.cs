using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Windows.Input;

namespace PiApp
{
    /// <summary>
    /// Focusable Run that represents a digit this run must conform with
    /// </summary>
    internal class RunDigit : Run
    {
        private static readonly Regex LettersRegex = new Regex(@"[a-zA-Z]");

        public int Length { get; }

        public RunDigit(int length)
        {
            this.Length = length;

            Focusable = true;
            ToolTip = string.Format("{1}", 
                Word, 
                Length);

            SetWord(string.Empty);

            MouseDown += (o, e) => {
                this.Focus();  
                e.Handled = true;
            };
            GotFocus += (o, e) => {
                SetBackground();
                e.Handled = true;
            };
            LostFocus += (o, e) =>
            {
                SetBackground();
                e.Handled = true;
            };
            KeyDown += KeyHandler;
            PreviewTextInput += TextHandler;
        }

        private void SetBackground()
        {
            if (IsFocused)
                this.Background = System.Windows.Media.Brushes.CornflowerBlue;
            else
                SetPassiveBackground();
        }

        protected virtual void SetPassiveBackground()
        {
            this.Background = System.Windows.Media.Brushes.Transparent;
        }

        private void TextHandler(object sender, TextCompositionEventArgs e)
        {
            if (IsFocused)
            {
                SetWord(Word + e.Text);
                e.Handled = true;
            }
        }

        private void KeyHandler(object sender, KeyEventArgs e)
        {
            if (IsFocused)
                switch (e.Key)
                {
                    case Key.Delete:
                        SetWord(string.Empty);
                        e.Handled = true;
                        break;

                    case Key.Right:
                        if (FocusNext()) 
                            e.Handled = true;
                        break;
                    case Key.Left:
                        if (FocusPrevious())
                            e.Handled = true;
                        break;

                    case Key.Back:
                        if (string.IsNullOrEmpty(Word))
                            FocusPrevious();
                        else
                            SetWord(Word.Substring(0, Word.Length - 1));
                        e.Handled = true;
                        break;

                    case Key.Space:
                    case Key.Tab:
                        if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Shift))
                            FocusPrevious();
                        else
                            FocusNext();
                        e.Handled = true;
                        break;

                    default:
                        // don't call it handled if we want to produce text for the text handler
                        break;
                }
        }

        private bool FocusNext()
        {
            if (this.NextInline == null)
                return false;
            this.NextInline.Focus();
            return true;
        }

        private bool FocusPrevious()
        {
            if (this.PreviousInline == null)
                return false;
            this.PreviousInline.Focus();
            return true;
        }

        public virtual bool IsValid
        {
            get
            {
                return WordIsSet()
                    && WordHasLengthLetters();
            }
        }

        private bool WordIsSet()
        {
            return !string.IsNullOrEmpty(Word?.Trim());
        }

        private bool WordHasLengthLetters()
        {
            return LettersRegex.Matches(Word).Count == Length;
        }

        public string Word { get; private set; }

        internal void SetWord(string word)
        {
            Word = word;
            SetTextFromWord();

            if (IsValid)
                ValidFormat();
            else
                InvalidFormat();

            SetBackground();
        }

        protected virtual void SetTextFromWord()
        {
            Text = string.Format(
                string.IsNullOrEmpty(Word) 
                ? " {1}" 
                : " {0}",
                 Word, Length);
        }


        private void InvalidFormat()
        {
            this.Foreground = System.Windows.Media.Brushes.Red;
        }

        private void ValidFormat()
        {
            this.Foreground = System.Windows.Media.Brushes.Black;
        }
    }
}
