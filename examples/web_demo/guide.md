1.  go to /test directory and run `pip install -r requirements.txt` to
    install requirements
2.  Perform all of the steps described here: https://github.com/msporna/LAVA-test-coverage/wiki/Setup-for-simple-web-app - 1 NOTE:
    add "jquery.js" to 'source_to_exclude' list in config.json.
3.  start a web server to host the example web app by running server.py script that can be found in examples/web_demo/app
4.  go to localhost:[your_port]:8787 (default port of the above server)
5.  example web app should be presented
6.  Last step is to create modules in Lava dashboard and assign detected sources to them. Follow the page: https://github.com/msporna/LAVA-test-coverage/wiki/configure-modules

---

--GET COVERAGE FROM AUTOMATIC TESTS--

7.  start the robot framework tests by either executing run_tests.bat
    or running `pybot --loglevel TRACE --variable CUSTOM_LIBRARY:"CustomKeywords.py" -d test_results\ selenium_test.robot`
    from the examples/web_demo/test folder
8.  the test suite consists of 3 python tests, a coverage session is created prior to execution and is ended in the suite
    teardown
9.  after the tests are done go to [lava_dashboard_url]/dashboard , find 'python_tests_1' session (created automatically in tests setup) and click on it to view report

---

--GET COVERAGE FROM MANUAL TESTS--

Perform all the following steps: https://github.com/msporna/LAVA-test-coverage/wiki/Get-the-coverage

NOTE for step 5 of above guide:
click through the example web app to simulate testing and generate some usage statistics

---

--PLAY THE GAME--

there is also a web based game that takes you through the above setup steps + manual testing scenario. The game can be played at:
https://github.com/msporna/LAVA-test-coverage/blob/master/docs/lava_game/index.html

It is a good idea to give it a try and there is no better way to learn the tool but actually trying it in such interactive way.
