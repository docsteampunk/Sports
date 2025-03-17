using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using Spectre.Console;

const int TotalRounds = 4;

List<Team> teams = new List<Team>();

teams = JsonFileReader.Read<List<Team>>("./database.json");

MainMenu(teams);

static void MainMenu(List<Team> teams) {
    Console.Clear();

    var rule = new Rule("[darkorange3]A generic sports Generator! [/]");
    rule.Justification = Justify.Center;
    rule.RuleStyle("darkorange3");

    AnsiConsole.Write(new FigletText("Sports!").Centered().Color(Color.Orange1));
    AnsiConsole.Write(rule);

    Console.WriteLine("");

    string selection = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("What would [underline]you[/] like to do?")
            .PageSize(10)
            .MoreChoicesText("[grey](Move up and down to reveal more options)[/]")
            .AddChoices(new[] {
                "Single Match", "Tournament", "Generate Point Scores", "Exit"
            })
            .HighlightStyle(Color.Orange1)
    );

    switch (selection) {
        case "Do Tournament":
            Tournament(teams);
            break;
        case "Single Match":
            SingleMatch(teams);
            break;
        case "Generate Point Scores":
            generatePointScores(teams);
            break;
        case "Exit":
            Environment.Exit(0);
            break;
    }
}

static void Tournament(List<Team> teams) {
    Console.Clear();

    string errorText = "";

    do {
        var rule = new Rule("[darkorange3]Tournament[/]");
        rule.Justification = Justify.Left;
        rule.RuleStyle("darkorange3");
        AnsiConsole.Write(rule);

        Console.WriteLine("");

        if (!String.IsNullOrEmpty(errorText)) {
            AnsiConsole.MarkupLine(errorText);
        }

        List<string> choices = teams.Select(i => i.name).ToList();
        choices.Add("[red]Back[/]");

        var selection = AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
            .Title("Please select which teams are going to play!")
            .PageSize(10)
            .Required()
            .MoreChoicesText("[grey](Move up and down to reveal more options)[/]")
            .InstructionsText(
                "[grey](Press [orange1]<space>[/] to toggle a fruit, " + 
                "[darkorange3]<enter>[/] to accept)[/]")
            .AddChoices(choices.ToArray())
            .HighlightStyle(Color.Orange1)
        );

        if (selection.Contains("[red]Back[/]")) {
            break;
        }

        if (selection.Count < 3) {
            errorText = "[red]Error:[/][grey] Must select at least 3 teams![/]\n";
        } else {
            errorText = "";
            List<Team> teamSelected = teams.Where(i => selection.Any(a => i.name == a)).ToList();
            GenerateBracket(teamSelected);
        }

        Console.Clear();
    } while (!String.IsNullOrEmpty(errorText));

    MainMenu(teams);
}

static void generatePointScores(List<Team> teams) {
    Console.Clear();

    var rule = new Rule("[darkorange3]Generate Point Scores[/]");
    rule.Justification = Justify.Left;
    rule.RuleStyle("darkorange3");
    AnsiConsole.Write(rule);

    Console.WriteLine("");

    
    var pointAllocation = AnsiConsole.Prompt(
        new TextPrompt<string>("Please enter the point allocation: ")
        .Validate((n) => isValidPointString(n).Count switch {
            > 0 => ValidationResult.Success(),
            < 1 => ValidationResult.Error("Invalid String.")
        })
    );

    
    var scoreGeneration = AnsiConsole.Prompt(
        new TextPrompt<string>("Please enter a score value: ")
        .Validate((n) => isValidScoreString(n, teams) switch{ 
            true => ValidationResult.Success(),
            false => ValidationResult.Error("Invalid String.")
        })
    );

    Console.WriteLine("");

    RunningTotalProbability(pointAllocation, scoreGeneration);

    AnsiConsole.MarkupLine("\n[grey]Press any key to continue...[/]");
    Console.ReadKey();
    MainMenu(teams);
}

static void RunningTotalProbability(string pointAllocation, string scoreGeneration) {
    List<Tuple<double, double>> pointValues = isValidPointString(pointAllocation);
    List<int> scoreListInt = new List<int> ();
    string[] scoreList = scoreGeneration.Split('-')[2].Trim().Split(',');

    int[] scores;
    double[] weights;

    double totalWeight = 0;

    foreach (Tuple<double, double> tuple in pointValues) {
        totalWeight += tuple.Item1;
    }

    scores = pointValues.Select(x => (int) x.Item1).ToArray();
    weights = pointValues.Select(x => x.Item2).ToArray();

    foreach (string score in scoreList) {
        scoreListInt.Add(int.Parse(score));
    }

    for (int i = 0; i < TotalRounds; i++) {
        string scoreTally = "";

        Console.Write($"Round {i}: ");
        int modifiedScore = 0;
        while (scoreListInt[i] > 0) {
            int value = WeightedProbability(scores, weights, scoreListInt[i]);

            if (value == -1) {
                break;
            }

            scoreListInt[i] -= value;
            scoreTally += $"{value},";
            modifiedScore += value;
        }
        Console.WriteLine($"{scoreTally.Substring(0, scoreTally.Length - 1)} - {modifiedScore}");
    }
}

//3:15,6:45,9:40
//MR7 - 91 - 18,30,14,29

static int WeightedProbability(int[] scores, double[] weights, int oldScore) {
    double totalWeight = weights.Sum();
    List<bool> isAbleToBeUsed = new List<bool>();

    Random rng = new Random();
    double probability = rng.NextDouble() * totalWeight;

    for (int i = 0; i < weights.Length; i++) {
        isAbleToBeUsed.Add(oldScore - scores[i] > -1);
    }

    double runningTotal = totalWeight;

    for (int i = weights.Length - 1; i > -1; i--) {
        runningTotal -= weights[i];

        if (probability >= runningTotal && isAbleToBeUsed[i]) {
            return scores[i];
        }
    }

    return -1;
}

static List<Tuple<double, double>> isValidPointString(string pointString) {
    if (String.IsNullOrEmpty(pointString)) {
        return null;
    }

    List<Tuple<double, double>> change = new List<Tuple<double, double>>();

    string[] pointTotals = pointString.Split(',');

    foreach (string line in pointTotals) {
        if (line.Length < 3 || !line.Contains(':')) {
            return new List<Tuple<double, double>>();
        } else {
            string[] splitValues = line.Split(':');

            if (splitValues.Length > 2) {
                return new List<Tuple<double, double>>();
            }

            Tuple<double, double> tuple = Tuple.Create(Double.Parse(splitValues[0]),Double.Parse(splitValues[1]));
            change.Add(tuple);
        }
    }

    return change;
}

static bool isValidScoreString(string scoreString, List<Team> teams) {
    List<string> teamNames = teams.Select(i => i.name).ToList();

    string[] values= scoreString.Split("-");
    
    if (values.Length != 3) {
        return false;
    }

    int amountOfRounds = values[2].Split(',').Length;

    return amountOfRounds == TotalRounds;
}



static void SingleMatch(List<Team> teams) {
    Console.Clear();

    string errorText = "";

    do {
        var rule = new Rule("[darkorange3]Single Match[/]");
        rule.Justification = Justify.Left;
        rule.RuleStyle("darkorange3");
        AnsiConsole.Write(rule);

        Console.WriteLine("");

        if (!String.IsNullOrEmpty(errorText)) {
            AnsiConsole.MarkupLine(errorText);
        }
        
        List<string> choices = teams.Select(i => i.name).ToList();
        choices.Add("[red]Back[/]");

        var selection = AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
            .Title("Please select which teams are going to play!")
            .PageSize(10)
            .Required()
            .MoreChoicesText("[grey](Move up and down to reveal more options)[/]")
            .InstructionsText(
                "[grey](Press [orange1]<space>[/] to toggle a team, " + 
                "[darkorange3]<enter>[/] to accept)[/]")
            .AddChoices(choices.ToArray())
            .HighlightStyle(Color.Orange1)
        );

        if (selection.Contains("[red]Back[/]")) {
            break;
        }

        if (selection.Count != 2) {
            errorText = "[red]Error:[/][grey] Must select two teams![/]\n";
        } else {
            errorText = "";
            List<Team> teamSelected = teams.Where(i => selection.Any(a => i.name == a)).ToList();
            Match match = new Match(0, teamSelected[0], teamSelected[1]);
            match.winner = CalculateMatch(match, false);
            AnsiConsole.MarkupLine("\n[grey]Press any key to continue...[/]");
            Console.ReadKey();
        }

        Console.Clear();
    } while (!String.IsNullOrEmpty(errorText));

    MainMenu(teams);
}

static void GenerateBracket(List<Team> teams) {
    int teamCount = teams.Count;
    int rounds = (int) Math.Ceiling(Math.Log2(teams.Count));
    
    List<String> placement = new List<String>();
    List<List<Match>> bracket = new List<List<Match>>();
    List<Match> currentRound = new List<Match>();

    int currMatch = 0;

    for (int i = 0; i < teamCount; i += 2) {
        Match match;

        if (i + 1 >= teamCount) {
            match = new Match(currMatch, teams[i], null);
        } else {
            match = new Match(currMatch, teams[i], teams[i+1]);
        }

        match.winner = CalculateMatch(match);

        if (match.winner == match.team1) {
            match.loser = match.team0;
        } else if (match.winner == match.team0 && match.team1 != null) {
            match.loser = match.team1;
        }

        if (match.loser != null) {
            placement.Add(match.loser.name);
        }

        currentRound.Add(match);
        currMatch += 1;
    }
    bracket.Add(currentRound);

    for (int i = 0; i < rounds; i++) {
        List<Match> nextRound = new List<Match>();

        for (int j = 0; j < currentRound.Count; j += 2) {
            Match match;
            

            if (j + 1 >= currentRound.Count) {
                match = new Match(currMatch, currentRound[j].winner, null);
            } else {
                match = new Match(currMatch, currentRound[j].winner, currentRound[j+1].winner);
            }

            match.winner = CalculateMatch(match);
            
            if (match.winner == match.team1) {
                match.loser = match.team0;
            } else if (match.winner == match.team0 && match.team1 != null) {
                match.loser = match.team1;
            }

            if (match.loser != null) {
                placement.Add(match.loser.name);
            }

            if (i + 1 == rounds && j + 1 == currentRound.Count) {
                placement.Add(match.winner.name);
            }

            nextRound.Add(match);
            currMatch += 1;
        }

        bracket.Add(nextRound);
        currentRound = nextRound;
    }

    Console.WriteLine("");

    placement.Reverse();

    for (int i = 0; i < placement.Count; i++) {
        string color = "grey";

        switch (i) {
            case 0:
                color = "gold1";
                break;
            case 1:
                color = "silver";
                break;
            case 2:
                color = "orange3";
                break;
        }

        AnsiConsole.MarkupLine($"[{color}]{i} - {placement[i]}[/]");
    }

    AnsiConsole.MarkupLine("\n[grey]Press any key to continue...[/]");
    Console.ReadKey();
    return;
}

static Team? CalculateMatch(Match m, bool showMatchNumber = true) {
    if (m.team1 == null) {
        return m.team0;
    }

    if (showMatchNumber) {
        Console.WriteLine($"Match {m.id}:");
    }

    long score0 = GeneratePerQuarter(m.team0.id, m.team0.averagePoints, m.team0.stdDeviation, showMatchNumber);
    long score1 = GeneratePerQuarter(m.team1.id, m.team1.averagePoints, m.team1.stdDeviation, showMatchNumber);

    Team winningTeam = (score0 > score1) ? m.team0 : m.team1;
    long winningScore = (score0 > score1) ? score0 - score1 : score1 - score0;

    string winnerString = $"[{winningTeam.color1}]{winningTeam.name}[/][grey] won by[/] [{winningTeam.color2}]{winningScore}[/] [grey]points![/]";
    winnerString = (showMatchNumber) ? "\t" + winnerString : winnerString;

    AnsiConsole.MarkupLine(winnerString);

    return winningTeam;
}

static long GeneratePerQuarter(string id, double m, double sd, bool showMatchNumber = true) {
    Random rng = new Random();
    List<long> scoresPerRound = new List<long>();

    long totalPoints = 0;

    for (int i = 0; i < TotalRounds; i++) {
        var roundScore = NormInv(rng.NextDouble(), m / TotalRounds, sd / TotalRounds);
        long iRoundScore = Math.Max(Convert.ToInt64(roundScore), 0);
        scoresPerRound.Add(iRoundScore);

        totalPoints += iRoundScore;
    }

    string winnerString = $"[grey]{id} - {totalPoints} - {String.Join(",", scoresPerRound.ToArray())}[/]";
    winnerString = (showMatchNumber) ? "\t" + winnerString : winnerString;

    AnsiConsole.MarkupLine(winnerString);

    return totalPoints;
}

/*
    p  = probability
    m  = mean
    sd = standard deviation

    Based upon the code from https://gist.github.com/kmpm/1211922
*/
static double NormInv(double p, double m, double sd) {
    if (p < 0 || p > 1) {
        throw new Exception("Probability must be between zero and one.");
    }
    if (sd < 0) {
        throw new Exception("The standard deviation must be positive.");
    }

    if (p == 0) {
        return -double.NegativeInfinity;
    }
    if (p == 1) {
        return double.PositiveInfinity;
    }
    if (sd == 0) {
        return m;
    }

    double q = 0;
    double r = 0;
    double val = 0;
    q = p - 0.5;

    if (Math.Abs(q) < .425) {
        r = .180625 - (q * q);
        val = q * (((((((r * 2509.0809287301226727 +
                          33430.575583588128105) * r + 67265.770927008700853) * r +
                        45921.953931549871457) * r + 13731.693765509461125) * r +
                      1971.5909503065514427) * r + 133.14166789178437745) * r +
                    3.387132872796366608)
               / (((((((r * 5226.495278852854561 +
                        28729.085735721942674) * r + 39307.89580009271061) * r +
                      21213.794301586595867) * r + 5394.1960214247511077) * r +
                    687.1870074920579083) * r + 42.313330701600911252) * r + 1);
    } else {
        if (q > 0)
            r = 1 - p;
        else
            r = p;

        r = Math.Sqrt(-Math.Log(r));
        /* r = sqrt(-log(r))  <==>  min(p, 1-p) = exp( - r^2 ) */

        if (r <= 5)
        { /* <==> min(p,1-p) >= exp(-25) ~= 1.3888e-11 */
            r += -1.6;
            val = (((((((r * 7.7454501427834140764e-4 +
                       .0227238449892691845833) * r + .24178072517745061177) *
                     r + 1.27045825245236838258) * r +
                    3.64784832476320460504) * r + 5.7694972214606914055) *
                  r + 4.6303378461565452959) * r +
                 1.42343711074968357734)
                / (((((((r *
                         1.05075007164441684324e-9 + 5.475938084995344946e-4) *
                        r + .0151986665636164571966) * r +
                       .14810397642748007459) * r + .68976733498510000455) *
                     r + 1.6763848301838038494) * r +
                    2.05319162663775882187) * r + 1);
        }
        else
        { /* very close to  0 or 1 */
            r += -5;
            val = (((((((r * 2.01033439929228813265e-7 +
                       2.71155556874348757815e-5) * r +
                      .0012426609473880784386) * r + .026532189526576123093) *
                    r + .29656057182850489123) * r +
                   1.7848265399172913358) * r + 5.4637849111641143699) *
                 r + 6.6579046435011037772)
                / (((((((r *
                         2.04426310338993978564e-15 + 1.4215117583164458887e-7) *
                        r + 1.8463183175100546818e-5) * r +
                       7.868691311456132591e-4) * r + .0148753612908506148525)
                     * r + .13692988092273580531) * r +
                    .59983220655588793769) * r + 1);
        }

        if (q < 0.0)
        {
            val = -val;
        }
    }

    return m + sd * val;
}

public class Match {
    public int id {get; set;}
    public Team? team0;
    public Team? team1;
    public Team? winner;
    public Team? loser;

    public Match(int id, Team? team0, Team? team1) {
        this.id = id;
        this.team0 = team0;
        this.team1 = team1;
    }
}

public class Team {
    public string? id {get; set;}
    public string name {get; set;} = "Test";
    public string? icon {get; set;}
    public Player[] players {get; set;} = {new Player()};

    public Player mascot {get; set;} = new Player();

    public double averagePoints {get; set;}
    public double stdDeviation {get; set;}

    public string color1 {get; set;} = "red";
    public string color2 {get; set;} = "white";
}

public class Player {
    public string? name {get; set;}
    public string? number {get; set;}

    override public string ToString() {
        return $"{name} - #{number}";
    }
}

static class JsonFileReader {
    public static T Read<T>(string filePath) {
        string json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<T>(json);
    }
}