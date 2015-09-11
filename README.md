# SillyXml

Immutable class XML serialization for the people!

With C#6 we can finally write immutable classes without going insane by using the new getter-only property syntax.

    public class ImmutableClass {
        public ImmutableClass(string fruit) {
            Fruit = fruit;
        }
    
        public string Fruit { get; }
    }
    
However if we try to serialize this class with the `XmlSerializer` class in the standard library it won't work since it doesnt't have a parameterless constructor and since the property has no setters - so we're forced to add those or create a separate class just for the serialization. Both of these options feel pretty bad - and thus this library was born.

## Getting started

SillyXml is available on Nuget, so to add it to your project simply do

    Install-Package SillyXml
    
or if you prefer using Paket add the following to your `paket.dependencies`

    nuget SillyXml

## Serializing an object to XML

SillyXml has a super-simple API, to serialize an object just do

    string xml = XmlSerializer.ToXml(new ImmutableClass("Banana"));
    
and that's it!

## Issues and problems

Please register issues on the github issue tracker if you find any bugs or features that you feel should be added.

## License and contributions

This project is licensed under the Apache License.

Contributions are welcome, just fork and send a pull request.   
If you wish to discuss any potential features etc just open an issue in the issue tracker.
