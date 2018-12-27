package com.lava.angular.example.page;
import org.openqa.selenium.By;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.WebElement;
import org.openqa.selenium.support.ui.ExpectedConditions;
import org.openqa.selenium.support.ui.WebDriverWait;

public class AppPage {

    //selectors
    String _button1Css="body > app-root > div > app-nav > header > nav > div > div:nth-child(1) > a:nth-child(1)";
    String _button2Css="body > app-root > div > app-nav > header > nav > div > div:nth-child(1) > a:nth-child(2)";


    //elements
    WebElement _homeButton;
    WebElement _heroesButton;

    //other
    WebDriver _driverInstance;
    String _appUrl;

    public AppPage(WebDriver driver,String appUrl)
    {
        _driverInstance=driver;
        _appUrl=appUrl;
        _driverInstance.get(appUrl);

        //find references
        WebDriverWait wait = new WebDriverWait(_driverInstance, 20);

        _homeButton=wait.until(ExpectedConditions.visibilityOfElementLocated(By.cssSelector(_button1Css)));
        _heroesButton=wait.until(ExpectedConditions.visibilityOfElementLocated(By.cssSelector(_button2Css)));



    }

    public HeroesPage NavigateToHeroesPage()
    {
        _heroesButton.click();

        return new HeroesPage(this._driverInstance,_appUrl);
    }


    public boolean AreNavigationButtonsPresent()
    {
        if(_homeButton.isDisplayed() && _heroesButton.isDisplayed())
        {
            return true;
        }


        return false;
    }
}
