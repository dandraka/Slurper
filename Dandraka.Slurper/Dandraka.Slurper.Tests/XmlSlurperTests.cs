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

public class XmlSlurperTests
{
    [Fact]
    public void T01_ObjectNotNullTest()
    {
        var city1 = XmlSlurper.ParseText(getFile("City.xml"));
        var city2 = XmlSlurper.ParseFile(getFileFullPath("City.xml"));

        foreach (var city in new[] { city1, city2 })
        {
            Assert.NotNull(city);
            Assert.NotNull(city.Name);
        }
    }

    [Fact]
    public void T02_SimpleXmlAttributesTest()
    {
        var book1 = XmlSlurper.ParseText(getFile("Book.xml"));
        var book2 = XmlSlurper.ParseFile(getFileFullPath("Book.xml"));

        foreach (var book in new[] { book1, book2 })
        {
            Assert.Equal("bk101", book.id);
            Assert.Equal("123456789", book.isbn);
        }
    }

    [Fact]
    public void T03_SimpleXmlNodesTest()
    {
        var book1 = XmlSlurper.ParseText(getFile("Book.xml"));
        var book2 = XmlSlurper.ParseFile(getFileFullPath("Book.xml"));

        foreach (var book in new[] { book1, book2 })
        {
            Assert.Equal("Gambardella, Matthew", book.author);
            Assert.Equal("XML Developer's Guide", book.title);
            Assert.Equal("Computer", book.genre);
            Assert.Equal("44.95", book.price);
        }
    }

    [Fact]
    public void T04_XmlMultipleLevelsNodesTest()
    {
        var settings1 = XmlSlurper.ParseText(getFile("HardwareSettings.xml"));
        var settings2 = XmlSlurper.ParseFile(getFileFullPath("HardwareSettings.xml"));

        foreach (var settings in new[] { settings1, settings2 })
        {
            Assert.Equal("true", settings.view.displayIcons);
            Assert.Equal("false", settings.performance.additionalChecks.disk.brandOptions.toshiba.useBetaFunc);
        }
    }

    [Fact]
    public void T05_ListXmlNodesTest()
    {
        var catalog1 = XmlSlurper.ParseText(getFile("BookCatalog.xml"));
        var catalog2 = XmlSlurper.ParseFile(getFileFullPath("BookCatalog.xml"));

        foreach (var catalog in new[] { catalog1, catalog2 })
        {
            var bookList = catalog.bookList;

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

    [Fact]
    public void T06_BothPropertiesAndListRootXmlTest()
    {
        var nutrition1 = XmlSlurper.ParseText(getFile("Nutrition.xml"));
        var nutrition2 = XmlSlurper.ParseFile(getFileFullPath("Nutrition.xml"));

        foreach (var nutrition in new[] { nutrition1, nutrition2 })
        {
            var foodList = nutrition.foodList;

            Assert.Equal(10, foodList.Count);

            var food1 = foodList[0];
            Assert.Equal("Avocado Dip", food1.name);
            Assert.Equal("Sunnydale", food1.mfr);
            Assert.Equal("11", food1.totalfat);

            Assert.Equal("1", food1.vitamins.a);
            Assert.Equal("0", food1.vitamins.c);
        }
    }

    [Fact]
    public void T07_BothPropertiesAndListRecursiveXmlTest()
    {
        var city1 = XmlSlurper.ParseText(getFile("CityInfo.xml"));
        var city2 = XmlSlurper.ParseFile(getFileFullPath("CityInfo.xml"));

        foreach (var city in new[] { city1, city2 })
        {
            Assert.True(city.Mayor == "Roni Mueller");
            Assert.True(city.CityHall == "Schulstrasse 12");
            Assert.True(city.Name == "Wilen bei Wollerau");
            Assert.True(city.Gemeinde == "Freienbach");

            Assert.Equal(3, city.StreetList.Count);

            Assert.Equal("8832", city.StreetList[2].PostCode);
            Assert.Equal(3, city.StreetList[2].HouseNumberList.Count);
        }
    }

    /// <summary>
    /// Usage showcase
    /// </summary>
    [Fact]
    public void T08_PrintXmlContents1()
    {
        string xml = "<book id=\"bk101\" isbn=\"123456789\"><author>Gambardella, Matthew</author><title>XML Developer Guide</title></book>";
        var book = XmlSlurper.ParseText(xml);

        // that's it, now we have everything            
        Console.WriteLine("T08 id = " + book.id);
        Console.WriteLine("T08 isbn = " + book.isbn);
        Console.WriteLine("T08 author = " + book.author);
        Console.WriteLine("T08 title = " + book.title);
    }

    /// <summary>
    /// Usage showcase
    /// </summary>
    [Fact]
    public void T09_PrintXmlContents2()
    {
        string xml = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" +
                        "<nutrition>" +
                        "	<food>" +
                        "		<name>Avocado Dip</name>" +
                        "		<mfr>Sunnydale</mfr>" +
                        "		<carb>2</carb>" +
                        "		<fiber>0</fiber>" +
                        "		<protein>1</protein>" +
                        "	</food>" +
                        "	<food>" +
                        "		<name>Bagels, New York Style </name>" +
                        "		<mfr>Thompson</mfr>" +
                        "		<carb>54</carb>" +
                        "		<fiber>3</fiber>" +
                        "		<protein>11</protein>" +
                        "	</food>" +
                        "	<food>" +
                        "		<name>Beef Frankfurter, Quarter Pound </name>" +
                        "		<mfr>Armitage</mfr>" +
                        "		<carb>8</carb>" +
                        "		<fiber>0</fiber>" +
                        "		<protein>13</protein>" +
                        "	</food>" +
                        "</nutrition>";
        var nutrition = XmlSlurper.ParseText(xml);

        // since many food nodes were found, a list was generated and named foodList (common name + "List")
        Console.WriteLine("T09 name1 = " + nutrition.foodList[0].name);
        Console.WriteLine("T09 name2 = " + nutrition.foodList[1].name);
    }

    [Fact]
    public void T10_BoolIntDecimalDoubleTest()
    {
        var settings1 = XmlSlurper.ParseText(getFile("HardwareSettings.xml"));
        var settings2 = XmlSlurper.ParseFile(getFileFullPath("HardwareSettings.xml"));

        foreach (var settings in new[] { settings1, settings2 })
        {
            Assert.Equal<bool?>(true, settings.view.displayIcons);
            Assert.Equal<bool?>(false, settings.view.showFiles);
            Assert.Equal<int?>(2, settings.performance.additionalChecks.disk.minFreeSpace);
            Assert.Equal<double?>(5.5, settings.performance.additionalChecks.disk.warnFreeSpace);
            Assert.Equal<decimal?>(5.5m, settings.performance.additionalChecks.disk.warnFreeSpace);

            Assert.True(settings.view.displayIcons);
            Assert.False(settings.view.showFiles);
            Assert.Equal<int>(2, settings.performance.additionalChecks.disk.minFreeSpace);
            Assert.Equal<double>(5.5, settings.performance.additionalChecks.disk.warnFreeSpace);
            Assert.Equal<decimal>(5.5m, settings.performance.additionalChecks.disk.warnFreeSpace);

            // usage showcase
            if (!settings.view.displayIcons)
            {
                Assert.True(false);
            }
            int? minFreeSpace = settings.performance.additionalChecks.disk.minFreeSpace;
            if (minFreeSpace != 2)
            {
                Assert.True(false);
            }
        }
    }

    [Fact]
    public void T11_ConversionExceptionTest()
    {
        var settings1 = XmlSlurper.ParseText(getFile("HardwareSettings.xml"));
        var settings2 = XmlSlurper.ParseFile(getFileFullPath("HardwareSettings.xml"));

        foreach (var settings in new[] { settings1, settings2 })
        {
            Assert.Throws<ValueConversionException>(() =>
            {
                int t = settings.view.displayIcons;
            });
            Assert.Throws<ValueConversionException>(() =>
            {
                decimal t = settings.view.displayIcons;
            });
            Assert.Throws<ValueConversionException>(() =>
            {
                double t = settings.view.displayIcons;
            });
            Assert.Throws<ValueConversionException>(() =>
            {
                bool t = settings.performance.additionalChecks.disk.minFreeSpace;
            });
        }
    }

    [Fact]
    public void T12_CDataTest()
    {
        var cdata1 = XmlSlurper.ParseText(getFile("CData.xml"));
        var cdata2 = XmlSlurper.ParseFile(getFileFullPath("CData.xml"));

        foreach (var cdata in new[] { cdata1, cdata2 })
        {
            // test cdata for single nodes
            Assert.Equal("DOCUMENTO N. 1234-9876", cdata.Title);

            // test cdata for list nodes
            dynamic attr = cdata.AttributeList[0];
            Assert.Equal("document.id", attr.Name);
            Assert.Equal("<string>DOCUMENTO N. 1234-9876</string>", attr);

            attr = cdata.AttributeList[4];
            Assert.Equal("receipt.date", attr.Name);
            Assert.Equal("<string>2020-12-28</string>", attr);

            attr = cdata.AttributeList[5];
            Assert.Equal("fcurrency", attr.Name);
            Assert.Equal("EUR", attr);
        }
    }

    //[Fact(Skip = "on dev laptop")]
    [Fact]
    public void T13_BigXmlTest()
    {
        var urlList = new List<string>()
        {
            // 1MB
            "http://aiweb.cs.washington.edu/research/projects/xmltk/xmldata/data/mondial/mondial-3.0.xml" /*,
            // 30 MB                
            "http://aiweb.cs.washington.edu/research/projects/xmltk/xmldata/data/tpc-h/lineitem.xml",
            // 109 MB
            "http://aiweb.cs.washington.edu/research/projects/xmltk/xmldata/data/SwissProt/SwissProt.xml",
            // 683 MB
            "http://aiweb.cs.washington.edu/research/projects/xmltk/xmldata/data/pir/psd7003.xml"*/
        };

        var getter = getHttpFiles(urlList);
        getter.Wait(5 * 60 * 1000); // 5min max        
        var stopWatch = new Stopwatch();                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             
        foreach (string xml in getter.Result)
        {   
            stopWatch.Reset();             
            stopWatch.Start();
            var cdata = XmlSlurper.ParseText(xml);
            stopWatch.Stop();

            Decimal fileSizeMb = Math.Round(xml.Length / (1024m * 1024m), 2);
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