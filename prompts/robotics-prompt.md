Robotics Scrimmage Manager
The following is a description of a c# .Net Solution. Check the current solution to ensure it meets the requirements.

Robotics Scrimmage Manager Description
Build a C# dot net MVC app project using the latest LTS version of dot net. The app must use entity framework identity to allow user authentication and authorization. Once logged in users should be able access administative functions.  The name of the project is "Robotics Manager". The project will be backed by an SQL Server database hosted on azure.

The theme colour of the project should be a black and gold.

Use bootstrap, jquery and other relevant frameworks and libraries to improve the user interface and user experience.

My Environment uses powershell.

STRUCTURE
Create 2 projects. Do not use top level statements.
A lib project that handles business logic including the entities, models, database contexts, services and migrations.
A web app project that handles the ui and admin describe inthe section labelled WEB APP

PUBLIC UI
On the main page without a logged in user  there should be a partial view section for announcements. The page should automatically reload every 5 minutes.
On this same page there should be a partial view showing a leader board showing the list of teams and total points.
Use signalR to push realtime update to the app.
thERE shoud be a public page that lists all the challenges.

GENERAL OVERVIEW
There are a list of team Each team has a NAME, TEAMNO, SCHOOL, COLOUR, URL to a image logo and should track points.
There is a list of challenges that a team can complete.
Each challenge has a NAME, DESCRIPTION, POINTS, IsUnique. Points from unique challenge can only be awarded to the first team to complete the activity.
When a team complete the challenge the team is awarded the points.
When a team is recorded as completing a challenge automatically generate an announcement to say so.
For each challenge store a record of the update including what was changed and by which user.
There should be a list of announcements with just text body and a priority (info, warning, danger, primary, secondary) and show/hide flag.
A list of updates (match score updates) and the time it was created.
Use signalR to push update to the app.

Create a VS Code workspace file and a Visual Studio solution file.
Create the relevant readme and gitignore files.
All primary key ids must use GUID.
use appsettings.json and not web.config. Generally prefer json settings files over XML
Set the gitignore file to ingnore the appsetting.json files

1. Generate a db context for all the entities models
2. include a markdown package which must be used to parse the updates and annoucements
3. add entity framework for SQL server database
4. add a reference to the lib project
5. add entity framework identity for use with google single sign on. Be sure to include the settings in the appsettings.json file.
6. Use SQL script to handle database creation and setup. USe the schema name "dbo"

Create a readme file that contains instructions on what i need to set up on Azure to make all this work.
Create a teraform file which can be used to provision the resources.
You may include a .NET ASpire

LIB PROJECT
1. Generate a db context for all the entities models

WEB APP
The web app manages a one day robotics competetion titled "ST JAGO ROBOTICS SCRIMMAGE 2024"
On the home page before logged in user  implement all the logic descried in the PUBLIC UI section.
There should be link that allows the user to view a list of the challenges
The rest of the app is accessed by logging in.
A logged in user should be able to add, edit, delete, view and list teams. 
A logged in user should be able to add, edit, delete, view and list challenges.
A logged in user should be able to add, edit, delete, view and list announcements. 
A logged in user should be able to add, edit, delete, view and list updates.

In the index lists place the action buttons at the begining of the row not at the end.
Ensure the create, edit, details, delete and indiex view are well styled and contained in boostrap cards.

