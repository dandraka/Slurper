using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit;

namespace Dandraka.Slurper.Tests;

public class JsonSlurperTests
{

    private TestUtility utility = new TestUtility();

    [SkippableFact]
    public void T01_ObjectNotNullTest()
    {
        var city1 = JsonSlurper.ParseText(utility.getFile("City.json"));
        var city2 = JsonSlurper.ParseFile(utility.getFileFullPath("City.json"));

        foreach (var jsonData in new[] { city1, city2 })
        {
            Assert.NotNull(jsonData);
            Assert.NotNull(jsonData.City);
            Assert.Null(jsonData.City.ToString());
            Assert.NotNull(jsonData.City.Name);
        }
    }

    [SkippableFact]
    public void T02_BaseJsonElementsTest()
    {
        var person1 = JsonSlurper.ParseText(utility.getFile("BaseJson.json"));
        
        // assert simple elements
        Assert.Equal("Joe", person1.Name);
        Assert.Equal(22, person1.Age);
        Assert.Equal(true, person1.CanDrive);

        Assert.Null(person1.ContactDetails.ToString());
        
        // assert object
        Assert.Equal("joe@hotmail.com", person1.ContactDetails.Email);
        Assert.Equal("07738277382", person1.ContactDetails.Mobile);
        Assert.Null(person1.ContactDetails.Fax.ToString());
    }

    [SkippableFact]
    public void T03_BaseJsonArrayTest()
    {
        var person2 = JsonSlurper.ParseText(utility.getFile("BaseJsonArray.json"));

        // assert simple elements
        Assert.Null(person2.Addresses.ToString());

        // assert array        
        Assert.Equal("15 Beer Bottle Street", person2.Addresses.AddressesList[0].Line1);
        Assert.Equal("Shell Cottage", person2.Addresses.AddressesList[1].Line1);
    }    

    [SkippableFact]
    public void T04_SimpleJsonElementsTest()
    {
        var bookInfo1 = JsonSlurper.ParseText(utility.getFile("Book.json"));
        var bookInfo2 = JsonSlurper.ParseFile(utility.getFileFullPath("Book.json"));

        foreach (var bookInfo in new[] { bookInfo1, bookInfo2 })
        {
            Assert.Equal("bk101", bookInfo.book.id);
            Assert.Equal("123456789", bookInfo.book.isbn);
            Assert.Equal(44.95, bookInfo.book.price);
            Assert.Equal(true, bookInfo.book.instock);
        }
    }

    [SkippableFact]
    public void T05_SimpleJsonNodesTest()
    {
        var bookInfo1 = JsonSlurper.ParseText(utility.getFile("Book.json"));
        var bookInfo2 = JsonSlurper.ParseFile(utility.getFileFullPath("Book.json"));

        foreach (var bookInfo in new[] { bookInfo1, bookInfo2 })
        {
            Assert.Equal("Gambardella, Matthew", bookInfo.book.author);
            Assert.Equal("XML Developer's Guide", bookInfo.book.title);
            Assert.Equal("Computer", bookInfo.book.genre);
            Assert.Equal("44.95", bookInfo.book.price);
        }
    }

    [SkippableFact]
    public void T06_JsonMultipleLevelsNodesTest()
    {
        var settingsInfo1 = JsonSlurper.ParseText(utility.getFile("HardwareSettings.json"));
        var settingsInfo2 = JsonSlurper.ParseFile(utility.getFileFullPath("HardwareSettings.json"));

        foreach (var settingsInfo in new[] { settingsInfo1, settingsInfo2 })
        {
            Assert.Equal("true", settingsInfo.settings.view.displayIcons);
            Assert.Equal("false", settingsInfo.settings.performance.additionalChecks.disk.brandOptions.toshiba.useBetaFunc);
        }
    }

    [SkippableFact]
    public void T07_ListJsonNodesTest()
    {
        var catalogInfo1 = JsonSlurper.ParseText(utility.getFile("BookCatalog.json"));
        var catalogInfo2 = JsonSlurper.ParseFile(utility.getFileFullPath("BookCatalog.json"));

        foreach (var catalogInfo in new[] { catalogInfo1, catalogInfo2 })
        {
            var bookList = catalogInfo.catalog.book.bookList;

            Assert.Equal(12, bookList.Count);

            var book1 = bookList[0];
            Assert.Equal("bk101", book1.id);
            Assert.Equal("Gambardella, Matthew", book1.author);
            Assert.Equal("XML Developer's Guide", book1.title);
            Assert.Equal("Computer", book1.genre);
            Assert.Equal("44.95", book1.price);

            var book4 = bookList[3];
            Assert.Equal("bk104", book4.id);
            Assert.Equal("Corets, Eva", book4.author);
            Assert.Equal("Oberon's Legacy", book4.title);
            Assert.Equal("Fantasy", book4.genre);
            Assert.Equal("5.95", book4.price);

            var book12 = bookList[11];
            Assert.Equal("bk112", book12.id);
            Assert.Equal("Galos, Mike", book12.author);
            Assert.Equal("Visual Studio 7: A Comprehensive Guide", book12.title);
            Assert.Equal("Computer", book12.genre);
            Assert.Equal("49.95", book12.price);
        }
    }

    [SkippableFact]
    public void T08_BothPropertiesAndListRootJsonTest()
    {
        var nutritionInfo1 = JsonSlurper.ParseText(utility.getFile("Nutrition.json"));
        var nutritionInfo2 = JsonSlurper.ParseFile(utility.getFileFullPath("Nutrition.json"));

        foreach (var nutritionInfo in new[] { nutritionInfo1, nutritionInfo2 })
        {
            var dailyvalues = nutritionInfo.nutrition.dailyvalues;
            Assert.Equal("g", dailyvalues.totalfat.units);
            Assert.Equal(65, dailyvalues.totalfat.text);

            var foodList = nutritionInfo.nutrition.food.foodList;

            Assert.Equal(10, foodList.Count);

            var food1 = foodList[0];
            Assert.Equal("Avocado Dip", food1.name);
            Assert.Equal("Sunnydale", food1.mfr);
            Assert.Equal("11", food1.totalfat);

            Assert.Equal("1", food1.vitamins.a);
            Assert.Equal("0", food1.vitamins.c);
        }
    }

    [SkippableFact]
    public void T09_BothPropertiesAndListRecursiveJsonTest()
    {
        var cityInfo1 = JsonSlurper.ParseText(utility.getFile("Cityinfo.json"));
        var cityInfo2 = JsonSlurper.ParseFile(utility.getFileFullPath("Cityinfo.json"));

        foreach (var cityInfo in new[] { cityInfo1, cityInfo2 })
        {
            Assert.Equal("Roni Müller", cityInfo.City.Mayor);
            Assert.Equal("Schulstrasse 12", cityInfo.City.CityHall);
            Assert.Equal("Wilen bei Wollerau", cityInfo.City.Name);
            Assert.Equal("Freienbach", cityInfo.City.Gemeinde);

            Assert.Equal(3, cityInfo.City.Street.StreetList.Count);

            // note that the underscore ("_name" in the file) gets removed
            Assert.Equal("Wolleraustrasse", cityInfo.City.Street.StreetList[0].name);
            Assert.Equal("8832", cityInfo.City.Street.StreetList[2].PostCode);            
            Assert.Equal(3, cityInfo.City.Street.StreetList[2].HouseNumber.HouseNumberList.Count);
        }
    }

    /// <summary>
    /// Usage showcase 1
    /// </summary>
    [SkippableFact]
    public void T10_Usage_PrintJsonContents1_Simple()
    {
        string json = 
@"{
  'id': 'bk101',
  'isbn': '123456789',
  'author': 'Gambardella, Matthew',
  'title': 'XML Developer Guide'
}".Replace("'", "\"");
        var book = JsonSlurper.ParseText(json);

        // that's it, now we have everything            
        Console.WriteLine("J-T10 id = " + book.id);
        Console.WriteLine("J-T10 isbn = " + book.isbn);
        Console.WriteLine("J-T10 author = " + book.author);
        Console.WriteLine("J-T10 title = " + book.title);
    }

    /// <summary>
    /// Usage showcase 2
    /// </summary>
    [SkippableFact]
    public void T11_Usage_PrintJsonContents2_Array()
    {
        string json = 
@"{
'Groceries': 
    [
        {
            'name': 'Avocado Dip',
            'mfr': 'Sunnydale',
            'carb': '2',
            'fiber': '0',
            'protein': '1'
        },
        {
            'name': 'Bagels, New York Style',
            'mfr': 'Thompson',
            'carb': '54',
            'fiber': '3',
            'protein': '11'
        },
        {
            'name': 'Beef Frankfurter, Quarter Pound',
            'mfr': 'Armitage',
            'carb': '8',
            'fiber': '0',
            'protein': '13'
        }
    ]
}".Replace("'", "\"");
        JsonSlurper.ListSuffix = "Inventory";
        var nutrition = JsonSlurper.ParseText(json);

        // Since many nodes were found, a list was generated. 
        // It's named common name + "List", so in this case GroceriesList.
        // But note that we've changed the value of ListSuffix to Inventory,
        // so the list name will become GroceriesInventory.
        Console.WriteLine("J-T11 name1 = " + nutrition.Groceries.GroceriesInventory[0].name);
        Console.WriteLine("J-T11 name2 = " + nutrition.Groceries.GroceriesInventory[1].name);
    }    

    /// <summary>
    /// Usage showcase 3
    /// </summary>
    [SkippableFact]
    public void T12_Usage_PrintJsonContents3_TopLevelArray()
    {
        string json = 
@"[
  {
    'name': 'Avocado Dip',
    'mfr': 'Sunnydale',
    'carb': '2',
    'fiber': '0',
    'protein': '1'
  },
  {
    'name': 'Bagels, New York Style',
    'mfr': 'Thompson',
    'carb': '54',
    'fiber': '3',
    'protein': '11'
  },
  {
    'name': 'Beef Frankfurter, Quarter Pound',
    'mfr': 'Armitage',
    'carb': '8',
    'fiber': '0',
    'protein': '13'
  }
]".Replace("'", "\"");        
        var nutrition = JsonSlurper.ParseText(json);

        // Since many nodes were found, a list was generated and named List. 
        // Normally it's named common name + "List" (e.g. GroceriesList) 
        // but in this case the parent of the array is nameless 
        // (it's the root) ergo just "List".
        Console.WriteLine("J-T12 name1 = " + nutrition.List[0].name);
        Console.WriteLine("J-T12 name2 = " + nutrition.List[1].name);
    }

    [SkippableFact]
    public void T13_BoolIntDecimalDoubleTest()
    {
        var settingsInfo1 = JsonSlurper.ParseText(utility.getFile("HardwareSettings.json"));
        var settingsInfo2 = JsonSlurper.ParseFile(utility.getFileFullPath("HardwareSettings.json"));

        foreach (var settingsInfo in new[] { settingsInfo1, settingsInfo2 })
        {
            Assert.Equal<bool?>(true, settingsInfo.settings.view.displayIcons);
            Assert.Equal<bool?>(false, settingsInfo.settings.view.showFiles);
            Assert.Equal<int?>(2, settingsInfo.settings.performance.additionalChecks.disk.minFreeSpace);
            Assert.Equal<double?>(5.5, settingsInfo.settings.performance.additionalChecks.disk.warnFreeSpace);
            Assert.Equal<decimal?>(5.5m, settingsInfo.settings.performance.additionalChecks.disk.warnFreeSpace);

            Assert.True(settingsInfo.settings.view.displayIcons);
            Assert.False(settingsInfo.settings.view.showFiles);
            Assert.Equal<int>(2, settingsInfo.settings.performance.additionalChecks.disk.minFreeSpace);
            Assert.Equal<double>(5.5, settingsInfo.settings.performance.additionalChecks.disk.warnFreeSpace);
            Assert.Equal<decimal>(5.5m, settingsInfo.settings.performance.additionalChecks.disk.warnFreeSpace);

            // usage showcase
            if (!settingsInfo.settings.view.displayIcons)
            {
                Assert.True(false);
            }
            int? minFreeSpace = settingsInfo.settings.performance.additionalChecks.disk.minFreeSpace;
            if (minFreeSpace != 2)
            {
                Assert.True(false);
            }
        }
    }

    [SkippableFact]
    public void T14_ConversionExceptionTest()
    {
        var settingsInfo1 = JsonSlurper.ParseText(utility.getFile("HardwareSettings.json"));
        var settingsInfo2 = JsonSlurper.ParseFile(utility.getFileFullPath("HardwareSettings.json"));

        foreach (var settingsInfo in new[] { settingsInfo1, settingsInfo2 })
        {
            Assert.Throws<ValueConversionException>(() =>
            {
                int t = settingsInfo.settings.view.displayIcons;
            });
            Assert.Throws<ValueConversionException>(() =>
            {
                decimal t = settingsInfo.settings.view.displayIcons;
            });
            Assert.Throws<ValueConversionException>(() =>
            {
                double t = settingsInfo.settings.view.displayIcons;
            });
            Assert.Throws<ValueConversionException>(() =>
            {
                bool t = settingsInfo.settings.performance.additionalChecks.disk.minFreeSpace;
            });
        }
    }

    [SkippableFact]
    public void T15_BigJsonTest()
    {
        var jsonList = new List<string>();
        jsonList.Add(utility.getFile("socialsample.json"));

        // not when building online
        // TODO find a better condition to detect running local vs github
        bool isLocal = Debugger.IsAttached;
        if (isLocal)
        {
            var urlList = new List<string>()
            {
                // 2.15MB
                "https://github.com/miloyip/nativejson-benchmark/blob/master/data/canada.json?raw=true", 
                // 25MB
                "https://github.com/json-iterator/test-data/blob/master/large-file.json?raw=true"
            };

            var getter = utility.getHttpFiles(urlList);
            getter.Wait(5 * 60 * 1000); // 5min max
            jsonList.AddRange(getter.Result);
        }

        var stopWatch = new Stopwatch();
        foreach (string json in jsonList)
        {
            stopWatch.Reset();
            stopWatch.Start();
            var cdata = JsonSlurper.ParseText(json);
            stopWatch.Stop();

            Decimal fileSizeMb = Math.Round(json.Length / (1024m * 1024m), 2);
            Int64 timeMs = stopWatch.ElapsedMilliseconds;
            Decimal speed = Math.Round(timeMs / fileSizeMb, 0);
            Console.WriteLine($"J-T15 Parsed {fileSizeMb} MB in {timeMs} ms (approx. {speed} ms/MB)");
        }
    }
}