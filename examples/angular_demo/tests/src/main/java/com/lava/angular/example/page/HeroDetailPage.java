package com.lava.angular.example.page;

import org.openqa.selenium.By;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.WebElement;
import org.openqa.selenium.support.ui.ExpectedConditions;
import org.openqa.selenium.support.ui.WebDriverWait;

public class HeroDetailPage {

    //selectors
    String _heroTitleLabelSelector="#heroe-detail > mat-card > mat-card-header > div.mat-card-header-text > mat-card-title";
    String _heroNameLabelSelector="#heroe-detail > mat-card > mat-card-header > div.mat-card-header-text > mat-card-subtitle";

    //elements
    WebElement _heroTitleLabel;
    WebElement _heroNameLabel;

    //other
    WebDriver _driverInstance;
    String _appUrl;

    public HeroDetailPage(WebDriver driver,String appUrl)
    {
        _driverInstance=driver;
        _appUrl=appUrl;


        //find references
        WebDriverWait wait = new WebDriverWait(_driverInstance, 20);

        _heroTitleLabel=wait.until(ExpectedConditions.visibilityOfElementLocated(By.cssSelector(_heroTitleLabelSelector)));
        _heroNameLabel=wait.until(ExpectedConditions.visibilityOfElementLocated(By.cssSelector(_heroNameLabelSelector)));



    }


    public String GetHeroTitle()
    {
        return _heroTitleLabel.getText().toLowerCase();
    }

    public String GetHeroName()
    {
        return _heroNameLabel.getText().toLowerCase();
    }

}
