# JiraConsole
Hacking together console app to expand IssueChangeLogs from Jira REST API

Built on Mac with Mono, so can compile and run on Mac or PC. 

** Note, the 'Project Key' has nothing to do with security. It's the 'key' in Jira for you project ... e.g. a "Point of Sale" project might have a key called "POS" -- or however the project was set up.

Mac Syntax: Mono JiraConsole_Brower.exe "[Jira Email]" "[Jira API Token]" "https://[Base Url]" "[Project Key]" "[Full Path including file name where the data file will be written"

PC Syntax:  JiraConsole_Brower.exe "[Jira Email]" "[Jira API Token]" "https://[Base Url]" "[Project Key]" "[Full Path including file name where the data file will be written"

There are a couple of minor magic strings that you might want to change. I'll make those all configurable soon, as I think this app will be helpful for other project at work, but it pretty straightforward to read through, and I'll answer questions if there are any.

Oh yeah, one last thing, no tests. I haven't written a line of code for several years, and on top of that this was my first stab writing code on a MAC. A few things might not be up to par, so I apologize for that.
