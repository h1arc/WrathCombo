using ECommons.ExcelServices;
using static WrathCombo.Window.Text;

namespace WrathCombo.Extensions
{
    internal static class JobExtensions
    {
        public static string Shorthand(this Job job) =>
        job switch
        {
            Job.ADV => string.Empty,
            Job.MIN or Job.BTN or Job.FSH => "DOL",
            _ => job.GetData().Abbreviation.ToString()
        };


        public static string Name(this Job job)
        {
            string jobName = job switch
            {
                Job.ADV => "Roles and Content",
                Job.MIN or Job.BTN or Job.FSH
                    => job.GetData().ClassJobCategory.Value.Name.ToString(),
                _ => job.GetData().Name.ToString()
            };

            return GetTextInfo().ToTitleCase(jobName);
        }
    }
}
