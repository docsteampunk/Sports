# Sports!
_Sports!_ The C# based score generation system.

<sub>NormInv function based upon the code from user kmpm @ https://gist.github.com/kmpm/1211922</sub>

![Screenshot of Sports! title page. Simple, but it gets the job done.](https://github.com/docsteampunk/Sports/blob/main/MainScreen.png)

## How it works!
_Sports!_ is a text based GUI with the only dependancy being Spectre.Console for that wonderful graphical user interface. To launch sports, run it in your console or use the included bash script. The way it works is by using the inverse of the standard normal cumulative distribution function and a random value to generate a score value. The current project right now can:
- Play one on one games to see who can win.
- Run tournaments with as many teams as your hardware can handle.
- Convert your generic game points score to a point by point breakdown (score may change based on pointValueString).

## Single Match
Single match allows you to select two teams. Select two teams in the drop down and press the enter key to run a game. Afterwards you can copy a game point score code into points via a later section.

## Tournament
Single matches not enough, want people to go through a tournament bracket style to find out who is the winner? This option is similar to the Single Match mode but it requires 3 or more selected. It also give placements to make it easy for any players.

## Generate Point Scores
Generate point scores is what is the weirdest and most convoluted part of the program. What it need to work are two strings; a point allocation string and a score value string.
### Point Allocation String
The point allocation string will tell the generator what are the possible points attainable in a single play and what are the chances of getting them. It can be shown similar than this.
```
Points:Probability,Points:Probability
1:15,2:40,3:45
```
When it comes to the string, values must be sorted by points in order of smallest to largest. Everything else should be fine. Points must be integers, probabilities can be decimal as they use the Double data-type.
### Score Value String
The score value string is what the point allocation string uses to generate in points per round. A normal string can look like this:
```
ID - ScoreTotal - Score0,Score1,Score2, Score3
MR7 - 91 - 18,30,14,29
```
- ID: Id is only used during matches to find out which team got what. They will be explained more in the database section.
- ScoreTotal: The total cumulative score created using the ```NormInv()``` function
- Score#: The score gained in a single round. The amount of rounds can be as many as you wish. This can be done with as many rounds as you want to change the amount edit the value ```const int TotalRounds``` with any number greater than 0 for the amount of rounds. Don't wory about changing mean points. Mean points are divided by the amount of rounds which should keep it within the margin of error and not have any statistical anomalies.

## Creating A "Database"
The reason it the database part is in quotations is because it is just a large json file with a list of teams which are loaded at the start of runtime. A standard database file looks like this:
```
[
    {
	"id":"CC0",
        "name": "Calico Catfish",
        "icon": "./Images/Calico.png",
        "players" : [
            { "name" : "Roddy Halfitch", "number" : "05"},
            { "name" : "Roddy Halfitch", "number" : "05"},
            { "name" : "Roddy Halfitch", "number" : "05"},
            { "name" : "Roddy Halfitch", "number" : "05"},
            { "name" : "Roddy Halfitch", "number" : "05"},
            { "name" : "Roddy Halfitch", "number" : "05"},
            { "name" : "Roddy Halfitch", "number" : "05"},
            { "name" : "Roddy Halfitch", "number" : "05"}
        ],
        "mascot" : { "name" : "Caddy The Catfish", "number" : "00"},
        "averagePoints" : 56.5, 
        "stdDeviation" : 13.79,
	"color1" : "gold1",
	"color2" : "chartreuse3"
    }
]
```
- id: A string that is simple as to identify which team had created the points.
- icon: _Not Used._ A file path to the images to load the icon for the team.
- players: _Not Used._ Has a name and a jersey number.
- mascot: _Not Used._ Same as a player. Technically not part of the team so it's put in a special field.
- averagePoints: Mean statistical average that the team has. The higher it is, the higher the points are.
- stdDeviation: The standard deviation. The higher it is, the larger the variance between your mean and your predicted score.
- color1: the color code for the primary highlight color. Can be hex code or spectral.console color code. Will default to red.
- color1: the color code for the secondary highlight color. Can be hex code or spectral.console color code. Will default to white.
