# LAVA Changelog

## Version 2.2.3

- squashed all commits to tidy up 

## Version 2.2.2

- fixed issue #16

## Version 2.2.1

- fixed instrument_server.py formatting issue (invalid indent)

## Version 2.2

- highlighting missed statements on file details popup (shown when source is clicked on report page)
- Angular demo uses python server instead of tomcat now

## Version 2.1

- added new Unity example - a webGL game called "LAVA SIMULATOR"
- added gevent wsgi server to run the backend
- added Unity plugin to handle sending stats from Unity games
- refreshed wiki pages

## Version 2.0

- renamed project to 'lava test coverage' from 'teco test coverage'
- added modules - sources can be now grouped by modules and individual modules are selected for each coverage session
- creating database on server start if wasn't detected
- support for unity/c#
- added code formatting before injecting probes (instrument_client)
- renamed instrument2.py to instrument_client.py
- code refactoring
- various bugfixes
- removed live session for now
- removed unnecessary entries from config.json
- showing progress bar in terminal when instrument_client is running (previously instrument2.py)

## Version 1.2.2

- added example of getting coverage from selenium java tests executed against angular 5 app
- removed previous basic selenium java example
- added wiki

## Version 1.2.1

- fixed issue #3
- updated db schema, added [sessions_files] intersection table so history reports can be viewed without problems
- refactored code responsible for displaying executed line count for instrumented files and viewing reports

## Version 1.2.0

- Refactored dashboard page to present sessions in table instead of divs
- Showing last session result (percentage) on dashboard
- Showing project name in the dashboard's top bar
- Added tasks_list file with upcoming tasks for the project

## Version 1.1.0

- New feature: when user clicks on some file in report the content of the file is shown and lines that were executed are highlighted

## Version 1.0.0

- First public release,stable
