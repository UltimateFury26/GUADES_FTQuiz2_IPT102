using System.Collections.Generic;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_ViewModels
{
    /// <summary>Close button + back button + titles at the top of a board overlay.</summary>
    public class FTQuiz2_BoardHeaderViewModel
    {
        public string Title { get; set; }
        public string CountText { get; set; }
        public string Description { get; set; }
    }

    /// <summary>One line inside a pin detail modal: optional bold label, text, optional CSS class.</summary>
    public class FTQuiz2_PinLine
    {
        public string Label { get; set; }
        public string Text { get; set; }
        public string CssClass { get; set; }
    }

    /// <summary>A pin detail modal (announcement, project, school or skill).</summary>
    public class FTQuiz2_PinModalViewModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public List<FTQuiz2_PinLine> Lines { get; set; }
        public string LinkUrl { get; set; }          // empty = no link
        public string LinkText { get; set; }

        public FTQuiz2_PinModalViewModel()
        {
            Lines = new List<FTQuiz2_PinLine>();
            LinkUrl = string.Empty;
        }
    }
}
