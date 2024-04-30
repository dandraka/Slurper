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

or json, e.g.

```
{
  "card": {
    "name": "John Doe",
    "title": "CEO, Widget Inc.",
    "email": "john.doe@widget.com",
    "phone": "(202) 456-1414",
    "logo": {
      "url": "widget.gif",
    }
  }
}
```

to a C# object, e.g.

```
card.name
card.title
card.email
card.phone
card.logo.url
```

This is done ***without any need to declare the type***. Behind the scenes it uses a class similar to System.Dynamic.ExpandoObject, named [ToStringExpandoObject](https://gist.github.com/kcuzner/3670e78ae1707a0e959d).

## Downloading:
Under the Release tab you can find the binaries to download, but the ***recommended*** way is to use it as a nuget dependency. The nuget package is named Dandraka.Slurper, here: https://www.nuget.org/packages/Dandraka.Slurper .

## Usage:

The library contains two classes, XmlSlurper and JsonSlurper. Both of them are static and contain two methods: ```ParseFile(string path)``` which accepts a filename and ```ParseText(string text)``` which accepts xml and json text respectively.

Both of them have a settable string property ```ListSuffix``` which has the default value of List. This is used when encountering arrays; a property is generated that is named as ```<commonName><ListSuffix>```. For example, parsing the following xml:

```
<?xml version="1.0" encoding="UTF-8"?>
  <Groceries>
    <name>Avocado Dip</name>
    <mfr>Sunnydale</mfr>
    <carb>2</carb>
    <fiber>0</fiber>
    <protein>1</protein>
  </Groceries>
  <Groceries>
    <name>Bagels, New York Style</name>
    <mfr>Thompson</mfr>
    <carb>54</carb>
    <fiber>3</fiber>
    <protein>11</protein>
  </Groceries>
  <Groceries>
    <name>Beef Frankfurter, Quarter Pound</name>
    <mfr>Armitage</mfr>
    <carb>8</carb>
    <fiber>0</fiber>
    <protein>13</protein>
  </Groceries>
```

will return _Bagels, New York Style_ under ```xmlObj.GroceriesList[1].name```.

In a similar way, parsing the following json:

```
{
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
}
```

will return _Bagels, New York Style_ under ```jsonObj.Groceries.GroceriesList[1].name```.

If the value of ```ListSuffix``` is changed to, say, 'Catalogue', the above object with be ```jsonObj.Groceries.GroceriesCatalogue[1].name```.

## Examples:

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

```
using Dandraka.Slurper;

public void PrintJsonContents1_Simple()
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
	Console.WriteLine("id = " + book.id);
	Console.WriteLine("isbn = " + book.isbn);
	Console.WriteLine("author = " + book.author);
	Console.WriteLine("title = " + book.title);
}

public void PrintJsonContents2_Array()
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
	Console.WriteLine("name1 = " + nutrition.Groceries.GroceriesInventory[0].name);
	Console.WriteLine("name2 = " + nutrition.Groceries.GroceriesInventory[1].name);
}    

public void PrintJsonContents3_TopLevelArray()
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
	Console.WriteLine("name1 = " + nutrition.List[0].name);
	Console.WriteLine("name2 = " + nutrition.List[1].name);
}
```

A VB.Net example:

```
Imports Dandraka.Slurper
Imports System
Imports System.Linq
Imports System.Collections.Generic
Imports System.IO

				
Public Module Module1
	Public Sub Main()
		
		Dim myXml As String = "<Vegetable><name>Avocado Dip</name><mfr>Sunnydale</mfr><nutrient><name>carb</name><value>2</value></nutrient><nutrient><name>fiber</name><value>1</value></nutrient><nutrient><name>protein</name><value>11</value></nutrient></Vegetable>"
		
		Dim myVegetable
		myVegetable = XmlSlurper.ParseText(myXml)
		Console.WriteLine("Vegetable name: " & myVegetable.name.ToString)
		Console.WriteLine("Vegetable manufacturer: " & myVegetable.mfr.ToString)
		Console.WriteLine()
		Dim myNutrientsList As List(Of Dandraka.Slurper.ToStringExpandoObject) = myVegetable.nutrientList
		Dim proteinNutrient As Object = myNutrientsList.FirstOrDefault(Function(i) CType(i, Object).name = "protein")
		Console.WriteLine(proteinNutrient.name.ToString() + " - " + proteinNutrient.value.ToString())
		Console.ReadLine()
	End Sub
End Module
```

## Releases: 
Release 2.0 is renamed, from XmlUtilities to Slurper (since it's more than Xml now, duh 😊). It implements JsonSlurper alongside XmlSlurper and is fully backwards compatible with all previous versions; the only change needed is to change the using clauses, from:

```using Dandraka.XmlUtilities;``` 

to:

```using Dandraka.Slurper;```

## Note: 
Although not required by the license, the author kindly asks that you share any improvements you make.
