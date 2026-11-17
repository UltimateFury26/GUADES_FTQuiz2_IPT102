namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Models
{
    /// <summary>dbo.GalleryItems.</summary>
    public class FTQuiz2_GalleryItem
    {
        public int GalleryId { get; set; }           // INT IDENTITY PK
        public string ImagePath { get; set; }        // NVARCHAR(260) NOT NULL
        public string Caption { get; set; }          // NVARCHAR(200) NULL
        public int SortOrder { get; set; }           // INT NOT NULL, default 0
    }
}
