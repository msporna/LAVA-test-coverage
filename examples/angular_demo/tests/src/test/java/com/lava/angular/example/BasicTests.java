package com.lava.angular.example;

import com.lava.angular.example.page.AppPage;
import com.lava.angular.example.page.HeroDetailPage;
import com.lava.angular.example.page.HeroesPage;
import org.junit.*;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.chrome.ChromeDriver;
import static org.junit.Assert.assertEquals;

public class BasicTests {


    private static String _AppUrl="http://localhost:8005/angular/"; //set app url here to speed things up
    private static WebDriver _Webdriver;

    @Before
    public void TestSetup() {
        _Webdriver = new ChromeDriver();

    }

    @After
    public void TestTeardown() {
        try
        {
            //ugly but we are instrumenting here, need to make sure the rest api call to instrumenting server
            //happens before test finishes forever, we need its stats; so wait a bit before finishing the test
            Thread.sleep(3000);
            _Webdriver.close();
        }
        catch (InterruptedException e)
        {
            System.out.println(e.toString());
        }

    }

    @BeforeClass
    public static void Setup()
    {
        //start coverage session
        //this will appear in lavas's dashboard
        LavaServiceHelper.StartCoverageSession("java_tests_1");
    }

    @AfterClass
    public static void Teardown() {
        //finish coverage session
        LavaServiceHelper.StopCoverageSession();
    }




    @Test
    //test if index has 2 buttons: home and heroes list
    public void test1() {

        LavaServiceHelper.SetTest("T1","Verify navigation buttons are present","index");
        AppPage app = new AppPage(_Webdriver,_AppUrl);
        assertEquals("Navigation buttons not present!", true, app.AreNavigationButtonsPresent());

    }

    @Test
    //test if clicking on hero on heroes list page goes to selected hero details
    public void test2() {
        LavaServiceHelper.SetTest("T2","Verify hero details page opens up","heroes");
        AppPage app = new AppPage(_Webdriver,_AppUrl);
        HeroesPage heroesPage=app.NavigateToHeroesPage();
        HeroDetailPage heroDetailsPage=heroesPage.GoToHeroDetails("deadpool");

        assertEquals("Hero details page is not showing expected title!", "deadpool", heroDetailsPage.GetHeroTitle());
        assertEquals("Hero details page is not showing expected name!", "wade winston wilson", heroDetailsPage.GetHeroName());

    }



}
