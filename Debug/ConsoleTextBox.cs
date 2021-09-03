using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MyReusableCodes
{
    /// <summary>
    /// Show Console output in textbox for WPF
    /// <summary>
    public class ConsoleTextBox : TextBox
    {
        static ConsoleTextBox()
        {
            var oldTextMetadata = TextProperty.GetMetadata(typeof(ConsoleTextBox)) as FrameworkPropertyMetadata;
            var newTextMetadata = new FrameworkPropertyMetadata(string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                oldTextMetadata.PropertyChangedCallback, CoerceConsoleText, isAnimationProhibited: true, System.Windows.Data.UpdateSourceTrigger.LostFocus);
            TextProperty.OverrideMetadata(typeof(ConsoleTextBox), newTextMetadata);
        }

        /// <summary>
        /// Logical of limit count of total line
        /// </summary>
        private static object CoerceConsoleText(DependencyObject d, object baseValue)
        {
            var ctb = d as ConsoleTextBox;
            int lineLimit = ctb.LineLimit;
            if (lineLimit <= 0) return baseValue;
            if (ctb.LineCount > lineLimit)
                return baseValue.ToString().Substring(ctb.GetCharacterIndexFromLineIndex(ctb.LineCount - lineLimit - 1));
            return baseValue;
        }
        public ConsoleTextBox()
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            IsReadOnly = true;
        }

        private class TextBoxWriter : System.IO.TextWriter
        {
            private readonly TextBox textBox;

            public TextBoxWriter(TextBox textBox) => this.textBox = textBox;

            public override System.Text.Encoding Encoding => System.Text.Encoding.UTF8;
            public override void Write(string value) => AppendTextSafe(value);
            public override void WriteLine(string value) => AppendTextSafe(value + Environment.NewLine);
            private void AppendTextSafe(string value)
                => textBox.Dispatcher.BeginInvoke(new Action<string>(AppendText), value);
            private void AppendText(string value)
            {
                var scollToEnd = textBox.SelectionStart == textBox.Text.Length;
                textBox.AppendText(value);
                if (scollToEnd)
                {
                    textBox.SelectionStart = textBox.Text.Length;
                    textBox.ScrollToEnd();
                }
            }
        }
        private TextBoxWriter textBoxWriter;

        /// <summary>
        /// Limit the count of total line in textbox
        /// </summary>
        public int LineLimit
        {
            get { return (int)GetValue(LineLimitProperty); }
            set { SetValue(LineLimitProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LineLimit.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LineLimitProperty =
            DependencyProperty.Register("LineLimit", typeof(int), typeof(ConsoleTextBox), new PropertyMetadata(-1));
        
        /// <summary>
        /// Intercept console output to textbox
        /// </summary>
        public void RegisterConsole()
        {
            if (textBoxWriter == null)
                textBoxWriter = new TextBoxWriter(this);
            Console.SetOut(textBoxWriter);
        }
        /// <summary>
        /// Restore the default output of the console
        /// </summary>
        public void UnregisterConsole()
        {
            Console.SetOut(new System.IO.StreamWriter(Console.OpenStandardOutput()));
        }

        /// <summary>
        /// Display a window to show the console output
        /// </summary>
        public static void ShowWindow()
        {
            var ctb = new ConsoleTextBox();
            ctb.RegisterConsole();
            var window = new Window { Content = ctb, Width = 400, Height = 300, Topmost = true };
            window.Closed += delegate { ctb.UnregisterConsole(); ctb.textBoxWriter.Dispose(); };
            window.Show();
        }
    }
}
