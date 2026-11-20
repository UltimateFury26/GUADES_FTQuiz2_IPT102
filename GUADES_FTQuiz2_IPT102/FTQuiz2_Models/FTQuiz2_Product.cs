using System;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Models
{
    /// <summary>dbo.Products: the product catalogue.</summary>
    public class FTQuiz2_Product
    {
        public int ProductId { get; set; }               // INT IDENTITY PK
        public string ProductName { get; set; }          // NVARCHAR(100) NOT NULL
        public string Flavor { get; set; }               // NVARCHAR(50) NULL
        public string ShortDescription { get; set; }     // NVARCHAR(250) NULL
        public string FullDescription { get; set; }      // NVARCHAR(MAX) NULL
        public string ImagePath { get; set; }            // NVARCHAR(260) NULL
        public int SortOrder { get; set; }               // INT NOT NULL, default 0
        public bool IsPublished { get; set; }            // BIT NOT NULL, default 1
        public DateTime CreatedAt { get; set; }          // DATETIME2(0) NOT NULL, default SYSDATETIME()
        public DateTime? UpdatedAt { get; set; }         // DATETIME2(0) NULL
        public string BackgroundColor { get; set; }      // NVARCHAR(7) NULL, "#RRGGBB"; null = automatic
    }
}
