using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;

namespace HomeBuyingApp.UI.Helpers
{
    public static class RichTextBoxBinding
    {
        public static readonly DependencyProperty XamlTextProperty = DependencyProperty.RegisterAttached(
            "XamlText",
            typeof(string),
            typeof(RichTextBoxBinding),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnXamlTextChanged));

        private static readonly DependencyProperty IsUpdatingProperty = DependencyProperty.RegisterAttached(
            "IsUpdating",
            typeof(bool),
            typeof(RichTextBoxBinding),
            new PropertyMetadata(false));

        private static readonly DependencyProperty IsHookedProperty = DependencyProperty.RegisterAttached(
            "IsHooked",
            typeof(bool),
            typeof(RichTextBoxBinding),
            new PropertyMetadata(false));

        public static string GetXamlText(DependencyObject obj) => (string)obj.GetValue(XamlTextProperty);
        public static void SetXamlText(DependencyObject obj, string value) => obj.SetValue(XamlTextProperty, value);

        public static string ToPlainText(string? xamlOrPlain)
        {
            if (string.IsNullOrWhiteSpace(xamlOrPlain)) return string.Empty;

            try
            {
                var doc = CreateDocumentFromXamlOrPlainText(xamlOrPlain);
                var range = new TextRange(doc.ContentStart, doc.ContentEnd);
                return range.Text?.Trim() ?? string.Empty;
            }
            catch
            {
                return xamlOrPlain.Trim();
            }
        }

        private static bool GetIsUpdating(DependencyObject obj) => (bool)obj.GetValue(IsUpdatingProperty);
        private static void SetIsUpdating(DependencyObject obj, bool value) => obj.SetValue(IsUpdatingProperty, value);

        private static bool GetIsHooked(DependencyObject obj) => (bool)obj.GetValue(IsHookedProperty);
        private static void SetIsHooked(DependencyObject obj, bool value) => obj.SetValue(IsHookedProperty, value);

        private static void OnXamlTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not RichTextBox richTextBox) return;
            if (GetIsUpdating(richTextBox)) return;

            HookEventsIfNeeded(richTextBox);

            var newValue = e.NewValue as string;
            SetIsUpdating(richTextBox, true);
            try
            {
                var newDoc = CreateDocumentFromXamlOrPlainText(newValue);
                richTextBox.Document = newDoc;
                // Force focus to ensure the document is fully loaded
                richTextBox.CaretPosition = newDoc.ContentStart;
            }
            finally
            {
                SetIsUpdating(richTextBox, false);
            }
        }

        private static void HookEventsIfNeeded(RichTextBox richTextBox)
        {
            if (GetIsHooked(richTextBox)) return;

            richTextBox.TextChanged += RichTextBoxOnTextChanged;
            richTextBox.Unloaded += RichTextBoxOnUnloaded;

            SetIsHooked(richTextBox, true);
        }

        private static void RichTextBoxOnUnloaded(object sender, RoutedEventArgs e)
        {
            if (sender is not RichTextBox richTextBox) return;

            richTextBox.TextChanged -= RichTextBoxOnTextChanged;
            richTextBox.Unloaded -= RichTextBoxOnUnloaded;
            SetIsHooked(richTextBox, false);
        }

        private static void RichTextBoxOnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not RichTextBox richTextBox) return;
            if (GetIsUpdating(richTextBox)) return;

            SetIsUpdating(richTextBox, true);
            try
            {
                var xaml = SerializeDocumentToXaml(richTextBox.Document);
                SetXamlText(richTextBox, xaml);
            }
            finally
            {
                SetIsUpdating(richTextBox, false);
            }
        }

        private static FlowDocument CreateDocumentFromXamlOrPlainText(string? xamlOrPlain)
        {
            if (string.IsNullOrWhiteSpace(xamlOrPlain))
            {
                var emptyDoc = new FlowDocument();
                // Remove default paragraph spacing for single-line look
                emptyDoc.Blocks.Add(new Paragraph { Margin = new Thickness(0) });
                return emptyDoc;
            }

            var trimmed = xamlOrPlain.TrimStart();

            if (trimmed.StartsWith("<FlowDocument", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    if (XamlReader.Parse(xamlOrPlain) is FlowDocument parsedDoc)
                    {
                        // Remove paragraph margins for consistent spacing
                        foreach (var block in parsedDoc.Blocks)
                        {
                            if (block is Paragraph p)
                                p.Margin = new Thickness(0);
                        }
                        return parsedDoc;
                    }
                }
                catch
                {
                    // fall through
                }
            }

            if (trimmed.StartsWith("<Section", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var doc = new FlowDocument();
                    var range = new TextRange(doc.ContentStart, doc.ContentEnd);
                    using var ms = new MemoryStream(Encoding.UTF8.GetBytes(xamlOrPlain));
                    range.Load(ms, DataFormats.Xaml);
                    // Remove paragraph margins for consistent spacing
                    foreach (var block in doc.Blocks)
                    {
                        if (block is Paragraph p)
                            p.Margin = new Thickness(0);
                    }
                    return doc;
                }
                catch
                {
                    // fall through
                }
            }

            // Legacy/plain text
            var flow = new FlowDocument();
            flow.Blocks.Add(new Paragraph(new Run(xamlOrPlain)) { Margin = new Thickness(0) });
            return flow;
        }

        private static string SerializeDocumentToXaml(FlowDocument document)
        {
            var range = new TextRange(document.ContentStart, document.ContentEnd);
            using var ms = new MemoryStream();
            range.Save(ms, DataFormats.Xaml);
            return Encoding.UTF8.GetString(ms.ToArray());
        }
    }
}
