0.  go to /app directory and clone https://github.com/Ismaestro/angular5-example-app (author: Ismael Ramos Silvan)
1.  go to /examples/angular_demo/app/angular5-example-app and from terminal run: 'npm update' and then 'npm i' (to install required modules)
1.  do the following steps carefully: https://github.com/msporna/TECO-code-coverage/wiki/Setup-for-Angular-app
    and also:

    - a. add "module-import-guard.ts" file to sources_to_exclude list in config.json - there are some problems with injecting probes into that file. Let's skip it for this demo.
    - b. if you have problems with some angular related module, do:
      -npm i -g @angular/cli@latest
      -npm i --save-dev @angular/cli@latest
    - c. if you are having problems with building the example app, you must debug it by yourself as it's environmental

1.  copy server.py from lava_root/examples/angular_demo to /dist folder after angular app has been built
1.  make sure the dist folder contains instrument.js and teco_test_coverage directory containing instrumenter.ts module

1.  open index.html and change 'base href='/' to base href='.'
1.  There is a tag 'meta http-equiv="Content-Security-Policy' that sits in index.html. For test automation&code coverage measurements you can remove this tag from index.html or update it to something less restrictive.

1.  Last step is to create modules in Lava dashboard and assign detected sources to them. Follow the page: https://github.com/msporna/LAVA-test-coverage/wiki/configure-modules

---

<h3>starting related automated tests:</h3>

1.  start python server by running python server.py from /dist folder of the app
1.  go to localhost:[8787]/angular
1.  example angular app should be presented

1.  start the java tests located in /tests folder. Run BasicTests.java file as junit.

- I recommend you briefly
  browse through the code (it's short) and see if all variables, especially urls match your setup. For example default url of app to test is: http://localhost:8787

1.  the test suite consists of 2 java tests, a coverage session is created prior to execution and is ended in the suite
    teardown
1.  after the tests are done go to [lava_url]/dashboard , find 'java_tests_1' session (created automatically by the automated tests) and click on it to view report

---

<h3>Manual testing</h3>

You can also create test session manually in LAVA dashboard and then click through example app manually and see how the coverage appears in LAVA's test report. The only difference in the flow is that you have to manually start the session in dashboard as in automated tests scenario, it's handled by the setup code. Test session is requried for coverage to be gathered by the Lava server.
