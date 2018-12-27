*** Settings ***
Documentation   tests the example app to showcase LAVA instrumentation tool
Library      Selenium2Library      40      1       Capture Page Screenshot
Library     ${CUSTOM_LIBRARY}
Suite Setup     start_test_session      python_tests_1      *
Suite Teardown      end_test_session
Test Teardown       Finish up

*** Keywords ***
Visit the test app
    [Documentation]     Open browser to the test app to yeld the further testing
    Open browser        http://localhost:8787/      CHROME
    delete all cookies
    
Finish up
	[Documentation]		this is a test suite written for demo app demonstrating test coverage tool and it is based on ajax calls to backend and we need to wait for 3 seconds before closing browser to make sure the call was made to the instrumentation backend; I know the sleep is not a good practice but this is not a 100% real test, it is for test coverage purposes and ajax call to backend is done from app's js so we can't wait for callback here
	Sleep		3
	Close All Browsers

*** Test Cases ***

Verify page presents required UI
    [Documentation]     test that index page has all of the required UI components
    [Tags]  smoke       ui
    Visit the test app 
    Page should contain element         id=testButton1
    Page should contain element       id=testButton2
    Page should contain element         id=output_paragraph


Verify expected output is displayed when button 1 is clicked
    [Documentation]     test that code attached to button1 is triggered when the button is clicked 
    [Tags]  smoke       ui		main
    Visit the test app 
    Click Element         id=testButton1
	element text should be		id=output_paragraph		function 3 output		message=expected output not detected after button1 click


Verify expected output is displayed when button 2 is clicked
    [Documentation]     test that code attached to button2 is triggered when the button is clicked 
    [Tags]  smoke       ui		main
    Visit the test app 
    Click Element         id=testButton2
	element text should be		id=output_paragraph		function 2 output		message=expected output not detected after button2 click


Verify about page is displayed when about link is clicked
    [Documentation]     test that routing is working corectly 
    [Tags]  smoke       ui		main
    Visit the test app 
    Click link        about.html
	wait until page contains element		id=welcomeMessage
	location should be			http://localhost:8787/about.html
