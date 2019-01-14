<img src="https://github.com/msporna/LAVA-test-coverage/blob/master/docs/screenshots/readme/lava_logo2.-01.jpeg" data-canonical-src="https://github.com/msporna/LAVA-test-coverage/blob/master/docs/screenshots/readme/lava_logo2.-01.jpeg" width="320" height="200" />

# WHAT IS THIS?

This is a test coverage tool that is intended to work with black box functional tests. It does not matter what language your tests are written in or if you perform manual testing instead of automated. Source code of app under test is modified to contain probes (and then it's compiled along with them) that report back to LAVA's backend which processes the statistics. It gathers coverage stats on the fly, as app is running and testing is being done. 

The tool is aimed for QA engineers that want to see what % of the tested app's code their functional tests cover.

More on how it works [here](https://github.com/msporna/LAVA-test-coverage/wiki/How-it-works)

# FEATURES

- measure how much code of application under tests was executed by testing activity (manual or automated)
    - it can be done for: 
        - Unity3D games (c#)
        - Android (Java) apps
        - Javascript web apps
- web based dashboard for viewing coverage reports and managing coverage sessions
- html report is automatically saved to drive after each coverage session and can be easily shared
- exposed rest API allows easy integration with any CI tool
- source files are instrumented automatically
- detected source files are grouped into modules - gather coverage only for source files relevant to test session 
- view timeline of code execution - see sources & executed lines sorted by execution time
- assign tag, owner and build number to the coverage session to relate it to the build being covered and easily integrate with your existing CI process
- view total coverage for specific build within specific release and see if your testing improved from build to build



![setup](https://github.com/msporna/LAVA-test-coverage/blob/master/docs/screenshots/readme/client1.PNG)
![report](https://github.com/msporna/LAVA-test-coverage/blob/master/docs/screenshots/readme/report1.PNG)
![report](https://github.com/msporna/LAVA-test-coverage/blob/master/docs/screenshots/readme/report3.png)
![executed_lines](https://github.com/msporna/LAVA-test-coverage/blob/master/docs/screenshots/readme/preimprovement.gif)


# DOCUMENTATION

--> [Getting Started](https://github.com/msporna/LAVA-test-coverage/wiki/Initial-setup)

Head over to the [Wiki section](https://github.com/msporna/LAVA-test-coverage/wiki) for more.

Article on how to get it running with a sample Unity3D game:
[Treasure hunting in video game testing: a tale about using a test coverage tool with Unity3D…](https://medium.com/@michalsporna/treasure-hunting-in-video-game-testing-a-tale-about-using-a-test-coverage-tool-with-unity3d-80ca2e434b9a
)

# TRY IT IN THE BROWSER

## WEBGL UNITY GAME

I prepared a webGL game that simulates the tool and allows you to see how it works directly in your browser

You can play it here:
https://msporna.github.io/LAVA-test-coverage/lava_game/

It's just a simulation to give you an overview. Note that the game shows version 2 of the tool which varies slightly from the most recent version.

![gameplay_gif](https://github.com/msporna/LAVA-test-coverage/blob/master/docs/screenshots/lava_simulator_video.gif)

# EXAMPLES

There are 3 examples available:

1.  [getting test coverage for unity game](https://github.com/msporna/LAVA-test-coverage/wiki/Setup-for-Unity-game)
2. [getting test coverage for java Android app](https://github.com/msporna/LAVA-test-coverage/wiki/Setup-for-Android-app)
3.  [getting test coverage for a js web app(+selenium,robot framework,python tests)](https://github.com/msporna/LAVA-test-coverage/wiki/Setup-for-web-app)

# CURRENT VERSION
3
