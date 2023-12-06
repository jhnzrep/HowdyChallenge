using Newtonsoft.Json;


var jsonFilePath = "answers.json";
try
{
    var json = File.ReadAllText(jsonFilePath);
    var sessions = JsonConvert.DeserializeObject<List<AnswerSession>>(json);

    if (!sessions?.Any() ?? true)
    {
        Console.WriteLine("No data available or the data is invalid.");
        return;
    }

    var groupScores = Evaluator.EvaluateGroups(sessions).GroupBy(score => score.GroupId)
        .OrderBy(group => group.Key); ;


    foreach (var group in groupScores)
    {
        Console.WriteLine($"Group ID: {group.Key}");
        foreach (var score in group.OrderBy(s => s.Month))
        {
            Console.WriteLine($"\tMonth: {score.Month:yyyy MMMM}, Average Score: {score.AverageScore:f3}");
        }
    }
}
catch (FileNotFoundException)
{
    Console.WriteLine($"File not found: {jsonFilePath}");
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
}


public class AnswerSession
{
    public int EmployeeId { get; set; }
    public int GroupId { get; set; }
    public DateTime AnsweredOn { get; set; }
    public decimal Answer1 { get; set; }
    public decimal Answer2 { get; set; }
    public decimal Answer3 { get; set; }
    public decimal Answer4 { get; set; }
    public decimal Answer5 { get; set; }
}

public class GroupScore
{
    public int GroupId { get; set; }
    public DateTime Month { get; set; }
    public decimal AverageScore { get; set; }
}

public class Evaluator
{
    public static List<GroupScore> EvaluateGroups(List<AnswerSession> sessions)
    {
        var individualMonthlyAverages = sessions
            .Select(s => new
            {
                s.EmployeeId,
                s.GroupId,
                Month = new DateTime(s.AnsweredOn.Year, s.AnsweredOn.Month, 1),
                Score = (s.Answer1 + s.Answer2 + s.Answer3 + s.Answer4 + s.Answer5) / 5
            })
            .GroupBy(s => new { s.EmployeeId, s.GroupId, s.Month })
            .Select(g => new
            {
                g.Key.GroupId,
                g.Key.Month,
                EmployeeAverageScore = g.Average(x => x.Score)
            });

        var groupScores = individualMonthlyAverages
            .GroupBy(s => new { s.GroupId, s.Month })
            .Select(g => new GroupScore
            {
                GroupId = g.Key.GroupId,
                Month = g.Key.Month,
                AverageScore = g.Average(x => x.EmployeeAverageScore)
            })
            .ToList();

        return groupScores;
    }
}


