<img src="https://github.com/msporna/LAVA-test-coverage/blob/master/docs/screenshots/readme/lava_logo2.-01.jpeg" data-canonical-src="https://github.com/msporna/LAVA-test-coverage/blob/master/docs/screenshots/readme/lava_logo2.-01.jpeg" width="320" height="200" />

# WHAT IS THIS?

This is a test coverage tool that is intended to work with black box tests. It does not matter what language your tests are written in, the tool is rest api based (Flask) so communication happens independently. It can work with automatic and manual tests.

The tool is aimed for QA engineers that want to see what % of the tested app their tests cover.

# FEATURES

- measure test coverage for Unity3D games, AngularJS apps and simple http/js web apps (without framework)
- suitable for manual and automated tests and in case of the latter programming language does not matter
- web based dashboard for viewing coverage reports, written in flask
- API written in flask so it can be easily integrated with any CI tool
- measures visited routes in case of web applications
- automatically detects source files
- source files can be grouped by custom module names and can select individual modules or all for coverage gathering
- clear reports consisting of chart and tables

![setup](https://github.com/msporna/LAVA-test-coverage/blob/master/docs/screenshots/readme/client1.PNG)
![report](https://github.com/msporna/LAVA-test-coverage/blob/master/docs/screenshots/readme/report1.PNG)
![executed_lines](https://github.com/msporna/LAVA-test-coverage/blob/master/docs/screenshots/readme/preimprovement.gif)

# DEMO / PRESENTATION

## WEB GAME

I prepared a webGL game that allows you to interact with the tool in a somewhat interesting way, showing you how to deploy and use it, perhaps teach you something new.

You can play it here:
https://msporna.github.io/LAVA-test-coverage/lava_game/

Or watch gameplay on youtube:
https://youtu.be/gyeNUG-V4D0

# DOCUMENTATION

[Getting Started](https://github.com/msporna/LAVA-test-coverage/wiki/Initial-setup)

Head over to the [Wiki section](https://github.com/msporna/LAVA-test-coverage/wiki) for more.

Article on how to get it running with a sample Unity3D game:
[Treasure hunting in video game testing: a tale about using a test coverage tool with Unity3D…](https://medium.com/@michalsporna/treasure-hunting-in-video-game-testing-a-tale-about-using-a-test-coverage-tool-with-unity3d-80ca2e434b9a
)

# EXAMPLES

There are 3 examples available:

1.  [getting test coverage for unity game](https://github.com/msporna/LAVA-test-coverage/wiki/Setup-for-Unity-game)
2.  [getting test coverage for angular app (+selenium,java tests)](https://github.com/msporna/LAVA-test-coverage/wiki/Setup-for-Angular-app)
3.  [getting test coverage for a js web app(+selenium,robot framework,python tests)](https://github.com/msporna/LAVA-test-coverage/wiki/Setup-for-web-app)

