using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using HomeBuyingApp.UI.ViewModels;

namespace HomeBuyingApp.UI.Views
{
    public partial class JournalView : UserControl
    {
        public JournalView()
        {
            InitializeComponent();
        }

        public JournalView(JournalViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }

        #region Rich Text Formatting

        private void FormatBold_Click(object sender, RoutedEventArgs e)
        {
            ApplyPropertyToSelection(TextElement.FontWeightProperty, FontWeights.Bold, FontWeights.Normal);
        }

        private void FormatItalic_Click(object sender, RoutedEventArgs e)
        {
            ApplyPropertyToSelection(TextElement.FontStyleProperty, FontStyles.Italic, FontStyles.Normal);
        }

        private void FormatUnderline_Click(object sender, RoutedEventArgs e)
        {
            var selection = ContentRichTextBox.Selection;
            if (selection != null && !selection.IsEmpty)
            {
                var currentDecorations = selection.GetPropertyValue(Inline.TextDecorationsProperty);
                if (currentDecorations is TextDecorationCollection decorations && decorations.Contains(TextDecorations.Underline[0]))
                {
                    selection.ApplyPropertyValue(Inline.TextDecorationsProperty, null);
                }
                else
                {
                    selection.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Underline);
                }
            }
            ContentRichTextBox.Focus();
        }

        private void HighlightYellow_Click(object sender, RoutedEventArgs e)
        {
            ApplyHighlight(Brushes.Yellow);
        }

        private void HighlightGreen_Click(object sender, RoutedEventArgs e)
        {
            ApplyHighlight(Brushes.LightGreen);
        }

        private void ClearFormatting_Click(object sender, RoutedEventArgs e)
        {
            var selection = ContentRichTextBox.Selection;
            if (selection != null && !selection.IsEmpty)
            {
                selection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
                selection.ApplyPropertyValue(TextElement.FontStyleProperty, FontStyles.Normal);
                selection.ApplyPropertyValue(Inline.TextDecorationsProperty, null);
                selection.ApplyPropertyValue(TextElement.BackgroundProperty, null);
            }
            ContentRichTextBox.Focus();
        }

        private void ApplyPropertyToSelection(DependencyProperty property, object valueOn, object valueOff)
        {
            var selection = ContentRichTextBox.Selection;
            if (selection != null && !selection.IsEmpty)
            {
                var currentValue = selection.GetPropertyValue(property);
                if (currentValue != null && currentValue.Equals(valueOn))
                {
                    selection.ApplyPropertyValue(property, valueOff);
                }
                else
                {
                    selection.ApplyPropertyValue(property, valueOn);
                }
            }
            ContentRichTextBox.Focus();
        }

        private void ApplyHighlight(Brush color)
        {
            var selection = ContentRichTextBox.Selection;
            if (selection != null && !selection.IsEmpty)
            {
                var currentBg = selection.GetPropertyValue(TextElement.BackgroundProperty);
                if (currentBg is SolidColorBrush currentBrush && color is SolidColorBrush newBrush && 
                    currentBrush.Color == newBrush.Color)
                {
                    selection.ApplyPropertyValue(TextElement.BackgroundProperty, null);
                }
                else
                {
                    selection.ApplyPropertyValue(TextElement.BackgroundProperty, color);
                }
            }
            ContentRichTextBox.Focus();
        }

        private void BulletList_Click(object sender, RoutedEventArgs e)
        {
            ToggleList(TextMarkerStyle.Disc);
        }

        private void NumberedList_Click(object sender, RoutedEventArgs e)
        {
            ToggleList(TextMarkerStyle.Decimal);
        }

        private void ToggleList(TextMarkerStyle markerStyle)
        {
            var selection = ContentRichTextBox.Selection;
            if (selection == null) return;

            var startParagraph = selection.Start.Paragraph;
            var endParagraph = selection.End.Paragraph;

            if (startParagraph == null) 
            {
                ContentRichTextBox.Focus();
                return;
            }

            if (startParagraph.Parent is ListItem listItem && 
                listItem.Parent is List existingList && 
                existingList.MarkerStyle == markerStyle)
            {
                RemoveFromList(startParagraph, endParagraph);
            }
            else
            {
                var newList = new List { MarkerStyle = markerStyle };
                
                var paragraphsToMove = new List<Paragraph>();
                var current = startParagraph;
                while (current != null)
                {
                    paragraphsToMove.Add(current);
                    if (current == endParagraph) break;
                    current = current.NextBlock as Paragraph;
                }

                if (paragraphsToMove.Count > 0)
                {
                    var insertPoint = paragraphsToMove[0];
                    var parent = insertPoint.Parent as FlowDocument ?? ContentRichTextBox.Document;
                    
                    parent.Blocks.InsertBefore(insertPoint, newList);

                    foreach (var para in paragraphsToMove)
                    {
                        parent.Blocks.Remove(para);
                        var newListItem = new ListItem(para);
                        newList.ListItems.Add(newListItem);
                    }
                }
            }
            
            ContentRichTextBox.Focus();
        }

        private void RemoveFromList(Paragraph startParagraph, Paragraph? endParagraph)
        {
            if (startParagraph.Parent is not ListItem startListItem ||
                startListItem.Parent is not List list)
                return;

            var document = ContentRichTextBox.Document;
            var paragraphsToExtract = new List<Paragraph>();
            
            foreach (ListItem item in list.ListItems)
            {
                foreach (var block in item.Blocks)
                {
                    if (block is Paragraph para)
                    {
                        paragraphsToExtract.Add(para);
                    }
                }
            }

            var insertAfter = list as Block;
            foreach (var para in paragraphsToExtract)
            {
                var newPara = new Paragraph();
                while (para.Inlines.FirstInline != null)
                {
                    var inline = para.Inlines.FirstInline;
                    para.Inlines.Remove(inline);
                    newPara.Inlines.Add(inline);
                }
                document.Blocks.InsertAfter(insertAfter, newPara);
                insertAfter = newPara;
            }

            document.Blocks.Remove(list);
        }

        #endregion

        #region Markdown Support

        private void ContentRichTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Process Markdown on Space or Enter
            if (e.Key == Key.Space || e.Key == Key.Enter)
            {
                // Defer processing to allow the key to be processed first
                Dispatcher.BeginInvoke(new Action(() => ProcessMarkdownInDocument()), 
                    System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        private void ProcessMarkdownInDocument()
        {
            try
            {
                var document = ContentRichTextBox.Document;
                
                // Process each paragraph
                foreach (var block in document.Blocks.ToList())
                {
                    if (block is Paragraph paragraph)
                    {
                        ProcessMarkdownInParagraph(paragraph);
                    }
                }
            }
            catch
            {
                // Silently ignore any parsing errors
            }
        }

        private void ProcessMarkdownInParagraph(Paragraph paragraph)
        {
            // Get all inlines and find markdown patterns
            var inlines = paragraph.Inlines.ToList();
            
            foreach (var inline in inlines)
            {
                if (inline is Run run)
                {
                    var text = run.Text;
                    if (string.IsNullOrEmpty(text)) continue;

                    // Check for bold **text**
                    var boldMatch = Regex.Match(text, @"\*\*(.+?)\*\*");
                    if (boldMatch.Success)
                    {
                        ApplyMarkdownFormat(paragraph, run, boldMatch, FontWeights.Bold, null, null, null);
                        return; // Process one at a time to avoid collection modification issues
                    }

                    // Check for italic *text* (not preceded/followed by *)
                    var italicMatch = Regex.Match(text, @"(?<!\*)\*([^*]+)\*(?!\*)");
                    if (italicMatch.Success)
                    {
                        ApplyMarkdownFormat(paragraph, run, italicMatch, null, FontStyles.Italic, null, null);
                        return;
                    }

                    // Check for strikethrough ~~text~~
                    var strikeMatch = Regex.Match(text, @"~~(.+?)~~");
                    if (strikeMatch.Success)
                    {
                        ApplyMarkdownFormat(paragraph, run, strikeMatch, null, null, TextDecorations.Strikethrough, null);
                        return;
                    }

                    // Check for code `text`
                    var codeMatch = Regex.Match(text, @"`([^`]+)`");
                    if (codeMatch.Success)
                    {
                        ApplyMarkdownFormat(paragraph, run, codeMatch, null, null, null, new SolidColorBrush(Color.FromRgb(240, 240, 240)));
                        return;
                    }
                }
            }
        }

        private void ApplyMarkdownFormat(Paragraph paragraph, Run originalRun, Match match, FontWeight? fontWeight, FontStyle? fontStyle, TextDecorationCollection? textDecorations, Brush? background)
        {
            var text = originalRun.Text;
            var beforeText = text.Substring(0, match.Index);
            var innerText = match.Groups[1].Value;
            var afterText = text.Substring(match.Index + match.Length);

            // Create the formatted run
            var formattedRun = new Run(innerText);
            if (fontWeight.HasValue)
                formattedRun.FontWeight = fontWeight.Value;
            if (fontStyle.HasValue)
                formattedRun.FontStyle = fontStyle.Value;
            if (textDecorations != null)
                formattedRun.TextDecorations = textDecorations;
            if (background != null)
            {
                formattedRun.Background = background;
                formattedRun.FontFamily = new FontFamily("Consolas");
            }

            // Replace the original run with before + formatted + after
            var insertPoint = originalRun;
            
            if (!string.IsNullOrEmpty(afterText))
            {
                var afterRun = new Run(afterText);
                paragraph.Inlines.InsertAfter(insertPoint, afterRun);
            }

            paragraph.Inlines.InsertAfter(insertPoint, formattedRun);

            if (!string.IsNullOrEmpty(beforeText))
            {
                var beforeRun = new Run(beforeText);
                paragraph.Inlines.InsertAfter(insertPoint, beforeRun);
            }

            paragraph.Inlines.Remove(originalRun);
        }

        #endregion
    }
}
