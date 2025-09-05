using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using System.Text;
using System.Collections.Generic;

public class ImprovedParagraphExtractionStrategy : ITextExtractionStrategy
{
    private readonly StringBuilder result = new StringBuilder();
    private readonly List<TextRenderInfo> textRenderInfos = new List<TextRenderInfo>();

    public void EventOccurred(IEventData data, EventType type)
    {
        if (type == EventType.RENDER_TEXT)
        {
            textRenderInfos.Add((TextRenderInfo)data);
        }
    }

    public string GetResultantText()
    {
        // Sort text chunks by their position
        textRenderInfos.Sort((a, b) =>
        {
            // First, sort by vertical position (Y-coordinate)
            int yComparison = b.GetBaseline().GetStartPoint().Get(1).CompareTo(a.GetBaseline().GetStartPoint().Get(1));
            if (yComparison != 0)
            {
                return yComparison;
            }
            // If on the same line, sort by horizontal position (X-coordinate)
            return a.GetBaseline().GetStartPoint().Get(0).CompareTo(b.GetBaseline().GetStartPoint().Get(0));
        });

        if (textRenderInfos.Count == 0)
        {
            return string.Empty;
        }

        // Process the sorted text chunks
        for (int i = 0; i < textRenderInfos.Count; i++)
        {
            var renderInfo = textRenderInfos[i];
            string text = renderInfo.GetText();
            var currentY = renderInfo.GetBaseline().GetStartPoint().Get(1);

            // Check for a new paragraph
            if (i > 0)
            {
                var prevRenderInfo = textRenderInfos[i - 1];
                var prevY = prevRenderInfo.GetBaseline().GetStartPoint().Get(1);

                // A larger-than-average vertical gap between lines often signals a new paragraph.
                // The threshold can be based on the average font height.
                float prevFontHeight = prevRenderInfo.GetAscentLine().GetStartPoint().Get(1) - prevRenderInfo.GetDescentLine().GetEndPoint().Get(1);
                float gap = prevY - currentY;

                // You may need to tune this factor based on your documents.
                // A factor of 1.5-2.0 is a good starting point.
                if (gap > prevFontHeight * 1.5f)
                {
                    result.Append("\n\n");
                }
                // Check for a new line within the same paragraph
                else if (currentY < prevY)
                {
                    result.Append(" "); // Add a space instead of a newline for text on the next line
                }
            }
            result.Append(text);
        }
        return result.ToString();
    }

    public ICollection<EventType> GetSupportedEvents()
    {
        // This is the missing piece. Only text rendering events are needed.
        return new List<EventType> { EventType.RENDER_TEXT };
    }
}