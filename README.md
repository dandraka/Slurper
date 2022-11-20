# Slurper
An XmlSlurper implementation in .Net, both for Xml and Json. The idea came from [Groovy's XmlSlurper](http://groovy-lang.org/processing-xml.html) which is hugely useful.

What this does, is convert a piece of xml, e.g.

```
<card xmlns="http://businesscard.org">
   <name>John Doe</name>
   <title>CEO, Widget Inc.</title>
   <email>john.doe@widget.com</email>
   <phone>(202) 456-1414</phone>
   <logo url="widget.gif"/>
 </card>
```

to a C# object, e.g.

```
card.name
card.title
card.email
card.phone
card.logo.url
```

This is done ***without any need to declare the type*** . Behind the scenes it uses a class similar to System.Dynamic.ExpandoObject, named [ToStringExpandoObject](https://gist.github.com/kcuzner/3670e78ae1707a0e959d).

## Downloading:
Under the Release tab you can find the binaries to download, but the ***recommended*** way is to use it as a nuget dependency. The nuget package is named Dandraka.Slurper, here: https://www.nuget.org/packages/Dandraka.Slurper .

## Usage:

```
using Dandraka.Slurper;

public void PrintXmlContents1()
{
	string xml = "<book id=\"bk101\" isbn=\"123456789\"><author>Gambardella, Matthew</author><title>XML Developer Guide</title></book>";
	var book = XmlSlurper.ParseText(xml);

	// that's it, now we have everything
	Console.WriteLine("id = " + book.id);
	Console.WriteLine("isbn = " + book.isbn);
	Console.WriteLine("author = " + book.author);
	Console.WriteLine("title = " + book.title);
}

public void PrintXmlContents2()
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
	Console.WriteLine("name1 = " + nutrition.foodList[0].name);
	Console.WriteLine("name2 = " + nutrition.foodList[1].name);
}

public void ReadSettings()
{
	var settings = XmlSlurper.ParseText(getFile("HardwareSettings.xml"));
            
	if (!settings.view.displayIcons)
	{
		DoWhatever();
	}
	    
	int? minFreeSpace = settings.performance.additionalChecks.disk.minFreeSpace;

	// Implicit type conversion works for string, bool?, int?, double?, decimal?, 
	// bool, int, double and decimal.
	// Note that if the xml content cannot be parsed (e.g. you try to use 
	// an xml node as bool but it contains "lalala") then for bool? you get null, 
	// and for bool you get a ValueConversionException.
}
```

## Releases: 
Release 2.0 implements JsonSlurper and is fully backwards compatible with all previous versions; the only change needed is to change the using clauses, from:

```using Dandraka.XmlUtilities;``` 

to:

```using Dandraka.Slurper;```

## Note: 
Although not required by the license, the author kindly asks that you share any improvements you make.
