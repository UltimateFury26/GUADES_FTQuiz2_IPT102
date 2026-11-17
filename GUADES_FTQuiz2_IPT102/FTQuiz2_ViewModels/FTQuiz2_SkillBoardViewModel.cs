using System.Collections.Generic;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Models;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_ViewModels
{
    /// <summary>
    /// One skill board as Home draws it (port of Home.aspx.cs SkillBoardView). The look (title, blurb,
    /// colour, icon) is fixed presentation; which skills appear comes from the database, by Category.
    /// </summary>
    public class FTQuiz2_SkillBoardViewModel
    {
        public string Category { get; set; }        // matches Skills.Category
        public string Key { get; set; }             // "frontend" -> data-board="frontend", overlay id "frontendBoard"
        public string Title { get; set; }
        public string Description { get; set; }
        public string Color { get; set; }
        public string IconMarkup { get; set; }      // inner SVG shapes; fixed strings written in code, never user input
        public List<FTQuiz2_Skill> Skills { get; set; }

        public FTQuiz2_SkillBoardViewModel() { Skills = new List<FTQuiz2_Skill>(); }

        public string OverlayId { get { return Key + "Board"; } }
    }
}
