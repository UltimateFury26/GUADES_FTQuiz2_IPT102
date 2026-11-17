using System;
using System.Collections.Generic;
using System.Globalization;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Data;
using GUADES_FTQuiz2_IPT102.FTQuiz2_Security;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_ViewModels
{
    /// <summary>One column of a per-month CSS bar chart (source SignupBar / VisitBar).</summary>
    public class FTQuiz2_ReportBar
    {
        public string Month { get; set; }
        public string Year { get; set; }
        public int Value { get; set; }
        public int HeightPercent { get; set; }
        public string Tooltip { get; set; }
    }

    /// <summary>
    /// Everything Reports.aspx.cs bound to its markup: the user summary, the signups chart, the
    /// active/inactive split bar, the latest signups, the content summary, the visit summary and
    /// the visits chart. All figures come from the usp_Report_* procedures; percentages are computed here.
    /// </summary>
    public class FTQuiz2_ReportsViewModel
    {
        private const string DateFormat = "MMM d, yyyy";

        /// <summary>A bar can never be shorter than this (percent) unless it is zero, so one signup stays visible.</summary>
        private const int MinimumBarPercent = 4;

        /// <summary>False when the database could not be reached (pnlReport hidden, error alert shown).</summary>
        public bool Loaded { get; set; }
        public string Message { get; set; }

        public FTQuiz2_UserSummary Summary { get; set; }
        public int ActivePercent { get; set; }
        public int InactivePercent { get; set; }

        public List<FTQuiz2_ReportBar> Signups { get; set; }
        public string SignupTotalText { get; set; }

        public FTQuiz2_VisitSummary VisitSummary { get; set; }
        public string VisitNote { get; set; }
        public List<FTQuiz2_ReportBar> Visits { get; set; }
        public string VisitsTotalText { get; set; }

        public List<FTQuiz2_RecentUser> Recent { get; set; }
        public List<FTQuiz2_ContentCount> Content { get; set; }

        public FTQuiz2_ReportsViewModel()
        {
            Summary = new FTQuiz2_UserSummary();
            VisitSummary = new FTQuiz2_VisitSummary();
            Signups = new List<FTQuiz2_ReportBar>();
            Visits = new List<FTQuiz2_ReportBar>();
            Recent = new List<FTQuiz2_RecentUser>();
            Content = new List<FTQuiz2_ContentCount>();
        }

        // ---- summary + split bar (source BindSummary) ----

        public void SetSummary(FTQuiz2_UserSummary summary)
        {
            Summary = summary;
            ActivePercent = Percent(summary.ActiveUsers, summary.TotalUsers);
            InactivePercent = summary.TotalUsers == 0 ? 0 : 100 - ActivePercent;
        }

        public string ActiveTooltip { get { return "Active: " + Number(Summary.ActiveUsers) + " (" + Number(ActivePercent) + "%)"; } }
        public string InactiveTooltip { get { return "Inactive: " + Number(Summary.InactiveUsers) + " (" + Number(InactivePercent) + "%)"; } }
        public string ActiveLegend { get { return Number(Summary.ActiveUsers) + " (" + Number(ActivePercent) + "%)"; } }
        public string InactiveLegend { get { return Number(Summary.InactiveUsers) + " (" + Number(InactivePercent) + "%)"; } }

        // ---- charts (source BindSignups / BindVisits) ----

        public void SetSignups(List<FTQuiz2_MonthlySignup> months)
        {
            var pairs = new List<KeyValuePair<string, int>>();
            foreach (FTQuiz2_MonthlySignup m in months) { pairs.Add(new KeyValuePair<string, int>(m.Label, m.Signups)); }

            int total;
            Signups = BuildBars(pairs, "signup", "signups", out total);
            SignupTotalText = total == 0
                ? "No signups in the last 12 months."
                : Number(total) + (total == 1 ? " signup" : " signups") + " in the last 12 months.";
        }

        public void SetVisitSummary(FTQuiz2_VisitSummary summary)
        {
            VisitSummary = summary;
            VisitNote = "Counts how many times a registered User has opened the portfolio. The Admin's own visits are not included.";
        }

        public void SetVisits(List<FTQuiz2_MonthlyVisits> months)
        {
            var pairs = new List<KeyValuePair<string, int>>();
            foreach (FTQuiz2_MonthlyVisits m in months) { pairs.Add(new KeyValuePair<string, int>(m.Label, m.Visits)); }

            int total;
            Visits = BuildBars(pairs, "visit", "visits", out total);
            VisitsTotalText = total == 0
                ? "No visits in the last 12 months."
                : Number(total) + (total == 1 ? " visit" : " visits") + " in the last 12 months.";
        }

        // Columns scaled against the busiest month (max starts at 1, so an all-zero chart has no division by zero).
        private static List<FTQuiz2_ReportBar> BuildBars(List<KeyValuePair<string, int>> months, string one, string many, out int total)
        {
            int max = 1;
            total = 0;

            foreach (var month in months)
            {
                total += month.Value;
                if (month.Value > max) { max = month.Value; }
            }

            var bars = new List<FTQuiz2_ReportBar>();

            foreach (var month in months)
            {
                string[] parts = (month.Key ?? string.Empty).Split(' ');

                bars.Add(new FTQuiz2_ReportBar
                {
                    Month = parts[0],
                    Year = parts.Length > 1 ? parts[1] : string.Empty,
                    Value = month.Value,
                    HeightPercent = BarHeight(month.Value, max),
                    Tooltip = month.Key + ": " + Number(month.Value) + " " + (month.Value == 1 ? one : many)
                });
            }

            return bars;
        }

        // ---- helpers ----

        /// <summary>Bar height in percent: 0 for none, otherwise proportional to the busiest month with a small minimum.</summary>
        private static int BarHeight(int value, int max)
        {
            return value <= 0 ? 0 : Math.Max(MinimumBarPercent, value * 100 / max);
        }

        /// <summary>Whole-number percentage of part in total, or 0 when there is no total.</summary>
        private static int Percent(int part, int total)
        {
            return total <= 0 ? 0 : (int)Math.Round(part * 100.0 / total, MidpointRounding.AwayFromZero);
        }

        public static string Number(int value) { return value.ToString(CultureInfo.InvariantCulture); }

        public string RoleBadgeClass(string role)
        {
            return role == FTQuiz2_Constants.Roles.Admin ? "badge badge-admin" : "badge badge-user";
        }

        public string StatusBadgeClass(string status)
        {
            return status == FTQuiz2_Constants.Status.Active ? "badge badge-active" : "badge badge-inactive";
        }

        public string FormatDate(DateTime value)
        {
            return value.ToString(DateFormat, CultureInfo.InvariantCulture);
        }
    }
}
