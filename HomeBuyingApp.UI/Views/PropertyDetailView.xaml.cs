using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using HomeBuyingApp.UI.ViewModels;

namespace HomeBuyingApp.UI.Views
{
    public partial class PropertyDetailView : UserControl
    {
        public PropertyDetailView()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                string url = e.Uri.ToString();
                // Handle URLs missing the protocol (e.g. "www.example.com")
                if (!url.StartsWith("http://", System.StringComparison.OrdinalIgnoreCase) && 
                    !url.StartsWith("https://", System.StringComparison.OrdinalIgnoreCase))
                {
                    url = "https://" + url;
                }

                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch
            {
                // Handle invalid URLs or other errors silently
            }
            e.Handled = true;
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Controls.Image clickedImage && clickedImage.Source is BitmapImage bitmapImage)
            {
                // Create a new window to display the enlarged image
                var imageWindow = new Window
                {
                    Title = "Property Image",
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    Width = 900,
                    Height = 700,
                    Background = System.Windows.Media.Brushes.Black,
                    Content = new System.Windows.Controls.Image
                    {
                        Source = bitmapImage,
                        Stretch = System.Windows.Media.Stretch.Uniform,
                        Margin = new Thickness(10)
                    }
                };

                // Close on click or Escape
                imageWindow.KeyDown += (s, args) =>
                {
                    if (args.Key == Key.Escape)
                        imageWindow.Close();
                };
                imageWindow.MouseLeftButtonDown += (s, args) => imageWindow.Close();

                imageWindow.ShowDialog();
            }
        }

        #region Comments Formatting Toolbar

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
            var selection = CommentsRichTextBox.Selection;
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
            CommentsRichTextBox.Focus();
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
            var selection = CommentsRichTextBox.Selection;
            if (selection != null && !selection.IsEmpty)
            {
                selection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
                selection.ApplyPropertyValue(TextElement.FontStyleProperty, FontStyles.Normal);
                selection.ApplyPropertyValue(Inline.TextDecorationsProperty, null);
                selection.ApplyPropertyValue(TextElement.BackgroundProperty, null);
            }
            CommentsRichTextBox.Focus();
        }

        private void ApplyPropertyToSelection(DependencyProperty property, object valueOn, object valueOff)
        {
            var selection = CommentsRichTextBox.Selection;
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
            CommentsRichTextBox.Focus();
        }

        private void ApplyHighlight(Brush color)
        {
            var selection = CommentsRichTextBox.Selection;
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
            CommentsRichTextBox.Focus();
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
            var selection = CommentsRichTextBox.Selection;
            if (selection == null) return;

            // Get the paragraph at the current position
            var startParagraph = selection.Start.Paragraph;
            var endParagraph = selection.End.Paragraph;

            if (startParagraph == null) 
            {
                CommentsRichTextBox.Focus();
                return;
            }

            // Check if already in a list with the same marker style
            if (startParagraph.Parent is ListItem listItem && 
                listItem.Parent is List existingList && 
                existingList.MarkerStyle == markerStyle)
            {
                // Remove from list - convert back to regular paragraphs
                RemoveFromList(startParagraph, endParagraph);
            }
            else
            {
                // Create new list
                var newList = new List { MarkerStyle = markerStyle };
                
                // Collect paragraphs to move
                var paragraphsToMove = new System.Collections.Generic.List<Paragraph>();
                var current = startParagraph;
                while (current != null)
                {
                    paragraphsToMove.Add(current);
                    if (current == endParagraph) break;
                    current = current.NextBlock as Paragraph;
                }

                if (paragraphsToMove.Count > 0)
                {
                    // Insert the list before the first paragraph
                    var insertPoint = paragraphsToMove[0];
                    var parent = insertPoint.Parent as FlowDocument ?? CommentsRichTextBox.Document;
                    
                    parent.Blocks.InsertBefore(insertPoint, newList);

                    // Move each paragraph into a list item
                    foreach (var para in paragraphsToMove)
                    {
                        parent.Blocks.Remove(para);
                        var newListItem = new ListItem(para);
                        newList.ListItems.Add(newListItem);
                    }
                }
            }
            
            CommentsRichTextBox.Focus();
        }

        private void RemoveFromList(Paragraph startParagraph, Paragraph? endParagraph)
        {
            if (startParagraph.Parent is not ListItem startListItem ||
                startListItem.Parent is not List list)
                return;

            var document = CommentsRichTextBox.Document;
            var paragraphsToExtract = new System.Collections.Generic.List<Paragraph>();
            
            // Collect paragraphs from list items
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

            // Insert paragraphs after the list, then remove the list
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
    }
}
