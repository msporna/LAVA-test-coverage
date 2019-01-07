Most recent Unity version that I tested against: unity 2018.2.1f1. Build target: web.

1.  Open project with the latest version of Unity
2.  Load the 'LAVA' scene from /Scenes and set build target to webGL
3.  Perform all of the steps described in: https://github.com/msporna/LAVA-test-coverage/wiki/Setup-for-Unity-game

- SKIP STEP 3: LavaTestCoverage plugin is already in the Plugins folder of the example game's assets

4.  Build the game
5.  Create modules in Lava dashboard and assign detected sources to them. Follow the page: https://github.com/msporna/LAVA-test-coverage/wiki/configure-modules
6.  Create a new test session in Lava dashboard
7.  Start a web server to host the example web app by running server.py script that can be found in examples/unity_demo (default url: localhost:8787) and navigate to the build folder where index.html resides
8.  Play the game and view the coverage as described in: https://github.com/msporna/LAVA-test-coverage/wiki/Get-the-coverage
