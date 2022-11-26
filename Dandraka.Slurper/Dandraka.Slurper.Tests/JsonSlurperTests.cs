using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Dandraka.Slurper.Tests;

public class JsonSlurperTests
{
    [Fact]
    public void T01_ObjectNotNullTest()
    {
        var city1 = JsonSlurper.ParseText(getFile("City.json"));
        //var city2 = JsonSlurper.ParseFile(getFileFullPath("City.json"));

        foreach (var jsonData in new[] { city1 /*, city2*/ })
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
        var person = JsonSlurper.ParseText(getFile("BaseJson.json"));

        // assert simple elements
        Assert.Equal("Joe", person.Name);
        Assert.Equal(22, person.Age);
        Assert.Equal(true, person.CanDrive);

        Assert.Null(person.ContactDetails.ToString());
        Assert.Null(person.Addresses.ToString());

        // assert object
        Assert.Equal("joe@hotmail.com", person.ContactDetails.Email);
        Assert.Equal("07738277382", person.ContactDetails.Mobile);
        Assert.Null(person.ContactDetails.Fax.ToString());

        // assert array
        Skip.If(true, "Arrays not yet implemented");
        /* NOT READY
        Assert.Equal("15 Beer Bottle Street", person.Addresses[0].Line1);
        Assert.Equal("Shell Cottage", person.Addresses[1].Line1);
        */
    }

    [Fact]
    public void T02_SimpleJsonElementsTest()
    {
        var bookInfo1 = JsonSlurper.ParseText(getFile("Book.json"));
        var bookInfo2 = JsonSlurper.ParseFile(getFileFullPath("Book.json"));

        foreach (var bookInfo in new[] { bookInfo1, bookInfo2 })
        {
            Assert.Equal("bk101", bookInfo.book.id);
            Assert.Equal("123456789", bookInfo.book.isbn);
            Assert.Equal(44.95, bookInfo.book.price);
            Assert.Equal(true, bookInfo.book.instock);
        }
    }

    [Fact]
    public void T03_SimpleJsonNodesTest()
    {
        var bookInfo1 = JsonSlurper.ParseText(getFile("Book.json"));
        var bookInfo2 = JsonSlurper.ParseFile(getFileFullPath("Book.json"));

        foreach (var bookInfo in new[] { bookInfo1, bookInfo2 })
        {
            Assert.Equal("Gambardella, Matthew", bookInfo.book.author);
            Assert.Equal("XML Developer's Guide", bookInfo.book.title);
            Assert.Equal("Computer", bookInfo.book.genre);
            Assert.Equal("44.95", bookInfo.book.price);
        }
    }

    [Fact]
    public void T04_JsonMultipleLevelsNodesTest()
    {
        var settingsInfo1 = JsonSlurper.ParseText(getFile("HardwareSettings.json"));
        var settingsInfo2 = JsonSlurper.ParseFile(getFileFullPath("HardwareSettings.json"));

        foreach (var settingsInfo in new[] { settingsInfo1, settingsInfo2 })
        {
            Assert.Equal("true", settingsInfo.settings.view.displayIcons);
            Assert.Equal("false", settingsInfo.settings.performance.additionalChecks.disk.brandOptions.toshiba.useBetaFunc);
        }
    }

    [Fact(Skip = "Arrays not yet implemented")]
    public void T05_ListJsonNodesTest()
    {
        var catalogInfo1 = JsonSlurper.ParseText(getFile("BookCatalog.json"));
        var catalogInfo2 = JsonSlurper.ParseFile(getFileFullPath("BookCatalog.json"));

        foreach (var catalogInfo in new[] { catalogInfo1, catalogInfo2 })
        {
            var bookList = catalogInfo.catalog.bookList;

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

    [Fact(Skip = "Arrays not yet implemented")]
    public void T06_BothPropertiesAndListRootXmlTest()
    {
        var nutritionInfo1 = JsonSlurper.ParseText(getFile("Nutrition.json"));
        var nutritionInfo2 = JsonSlurper.ParseFile(getFileFullPath("Nutrition.json"));

        foreach (var nutritionInfo in new[] { nutritionInfo1, nutritionInfo2 })
        {
            var foodList = nutritionInfo.nutrition.foodList;

            Assert.Equal(10, foodList.Count);

            var food1 = foodList[0];
            Assert.Equal("Avocado Dip", food1.name);
            Assert.Equal("Sunnydale", food1.mfr);
            Assert.Equal("11", food1.totalfat);

            Assert.Equal("1", food1.vitamins.a);
            Assert.Equal("0", food1.vitamins.c);
        }
    }

    [Fact(Skip = "Arrays not yet implemented")]
    public void T07_BothPropertiesAndListRecursiveXmlTest()
    {
        var cityInfo1 = JsonSlurper.ParseText(getFile("Cityinfo.json"));
        var cityInfo2 = JsonSlurper.ParseFile(getFileFullPath("Cityinfo.json"));

        foreach (var cityInfo in new[] { cityInfo1, cityInfo2 })
        {
            Assert.Equal("Roni Müller", cityInfo.City.Mayor);
            Assert.Equal("Schulstrasse 12", cityInfo.City.CityHall);
            Assert.Equal("Wilen bei Wollerau", cityInfo.City.Name);
            Assert.Equal("Freienbach", cityInfo.City.Gemeinde);

            Assert.Equal(3, cityInfo.City.StreetList.Count);

            Assert.Equal("8832", cityInfo.City.StreetList[2].PostCode);
            Assert.Equal(3, cityInfo.City.StreetList[2].HouseNumberList.Count);
        }
    }

    /// <summary>
    /// Usage showcase
    /// </summary>
    //[Fact(Skip = "Arrays not yet implemented")]
    [Fact]
    public void T08_PrintXmlContents1()
    {
        string json = "{  \"id\": \"bk101\",  \"isbn\": \"123456789\",  \"author\": \"Gambardella, Matthew\",  \"title\": \"XML Developer Guide\"}";
        var book = JsonSlurper.ParseText(json);

        // that's it, now we have everything            
        Console.WriteLine("T08 id = " + book.id);
        Console.WriteLine("T08 isbn = " + book.isbn);
        Console.WriteLine("T08 author = " + book.author);
        Console.WriteLine("T08 title = " + book.title);
    }

    /// <summary>
    /// Usage showcase
    /// </summary>
    [Fact(Skip = "Arrays not yet implemented")]
    //[Fact]
    public void T09_PrintXmlContents2()
    {
        string json = "[  {    \"name\": \"Avocado Dip\",    \"mfr\": \"Sunnydale\",    \"carb\": \"2\",    \"fiber\": \"0\",    \"protein\": \"1\"  },  {    \"name\": \"Bagels, New York Style\",    \"mfr\": \"Thompson\",    \"carb\": \"54\",    \"fiber\": \"3\",    \"protein\": \"11\"  },  {    \"name\": \"Beef Frankfurter, Quarter Pound\",    \"mfr\": \"Armitage\",    \"carb\": \"8\",    \"fiber\": \"0\",    \"protein\": \"13\"  }]";
        var nutrition = JsonSlurper.ParseText(json);

        // since many food nodes were found, a list was generated and named foodList (common name + "List")
        Console.WriteLine("T09 name1 = " + nutrition.foodList[0].name);
        Console.WriteLine("T09 name2 = " + nutrition.foodList[1].name);
    }

    [Fact]
    public void T10_BoolIntDecimalDoubleTest()
    {
        var settingsInfo1 = JsonSlurper.ParseText(getFile("HardwareSettings.json"));
        var settingsInfo2 = JsonSlurper.ParseFile(getFileFullPath("HardwareSettings.json"));

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

    [Fact]
    public void T11_ConversionExceptionTest()
    {
        var settingsInfo1 = JsonSlurper.ParseText(getFile("HardwareSettings.json"));
        var settingsInfo2 = JsonSlurper.ParseFile(getFileFullPath("HardwareSettings.json"));

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

    [Fact]
    public void T12_BigJsonTest()
    {
        var urlList = new List<string>()
        {
            // 25MB
            "https://github.com/json-iterator/test-data/blob/master/large-file.json?raw=true"
        };

        var getter = getHttpFiles(urlList);
        getter.Wait(5 * 60 * 1000); // 5min max        
        var stopWatch = new Stopwatch();
        foreach (string json in getter.Result)
        {
            stopWatch.Reset();
            stopWatch.Start();
            var cdata = JsonSlurper.ParseText(json);
            stopWatch.Stop();

            Decimal fileSizeMb = Math.Round(json.Length / (1024m * 1024m), 2);
            Int64 timeMs = stopWatch.ElapsedMilliseconds;
            Decimal speed = Math.Round(timeMs / fileSizeMb, 0);
            Console.WriteLine($"T13 Parsed {fileSizeMb} MB in {timeMs} ms (approx. {speed} ms/MB)");
        }
    }

    private string getFile(string fileName)
    {
        string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "testdata", fileName);
        return File.ReadAllText(path);
    }

    private string getFileFullPath(string fileName)
    {
        string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "testdata", fileName);
        return path;
    }

    private async Task<List<string>> getHttpFiles(List<string> urlList)
    {
        var list = new List<string>();
        using (var client = new HttpClient())
        {
            foreach (var url in urlList)
            {
                try
                {
                    var result = await client.GetAsync(url);
                    if (result.IsSuccessStatusCode)
                    {
                        var content = await result.Content.ReadAsByteArrayAsync();
                        list.Add(Encoding.UTF8.GetString(content));
                        //Console.WriteLine($"GET HTTP: Read {Math.Round(list[list.Count - 1].Length / (1024m * 1024m), 2)} MB from {url}");
                    }
                    else
                    {
                        Console.WriteLine($"*** WARNING *** GET HTTP: Could not download from {url}, skipping.\r\nResult {result.StatusCode}: {result.ReasonPhrase}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"*** WARNING *** GET HTTP: Could not download from {url}, skipping.\r\nException {ex.GetType().FullName}: {ex.Message}");
                }
            }
        }
        return list;
    }
}