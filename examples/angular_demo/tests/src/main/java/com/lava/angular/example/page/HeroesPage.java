package com.lava.angular.example.page;
import org.openqa.selenium.By;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.WebElement;
import org.openqa.selenium.support.ui.ExpectedConditions;
import org.openqa.selenium.support.ui.WebDriverWait;

import java.util.List;

public class HeroesPage {
    //selectors
    String _heroItemSelector="mat-list-item";


    //elements
    List<WebElement> _heroes;


    //other
    WebDriver _driverInstance;
    String _appUrl;

    public HeroesPage(WebDriver driver,String appUrl)
    {
        _driverInstance=driver;
        _appUrl=appUrl;


        //find references
        WebDriverWait wait = new WebDriverWait(_driverInstance, 20);

        _heroes=wait.until(ExpectedConditions.visibilityOfAllElementsLocatedBy(By.cssSelector(_heroItemSelector)));
    }


    public HeroDetailPage GoToHeroDetails(String heroTitle)
    {

        _findHeroElement(heroTitle).click();
        return new HeroDetailPage(this._driverInstance,_appUrl);
    }

    WebElement _findHeroElement(String heroToFind)
    {
        //iterate through each hero on the list
        for (int i = 0; i < _heroes.size(); i++) {

            //find web element holding hero's title
            String heroTitle=_heroes.get(i).findElement(By.cssSelector("div > div.mat-list-text > h3")).getText();
            //if currently processed hero is the one we want, return that web element
            if(heroTitle.toLowerCase().equals(heroToFind.toLowerCase()))
            {
                return _heroes.get(i);
            }

        }

        //if nothing found,null will be returned
       return null;
    }



}
