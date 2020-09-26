# JiraConsole
Console app to work with Jira REST API via the Atlassian.SDK for .Net (v12.x.x)

Targets .Net Framework 4.8 (built with Visual Studio for Mac)

This project is somewhat new, and I'll get some tests and refactoring done soon, but it will currently query Jira for Project and Issues. It will also expand the Issue changeLog -- and if you're reading this you'll be happy to know that after many hours of pulling my hair out, I've implemented the ability to get ALL of the issue changeLogs! If you're not aware, the Atlassian.SDK v12 doesn't return more than 100 changeLogs, and doesn't support pagination of changeLogs like it does with Issues. See screenshot below for the magic.

![img](https://github.com/lopperman/JiraConsole/blob/master/JiraConsole_Brower/misc/JiraConsole1.png)

# Get All Issue ChangeLogs!
![img](https://github.com/lopperman/JiraConsole/blob/master/JiraConsole_Brower/misc/GetALLIssueChangeLogs.png)
