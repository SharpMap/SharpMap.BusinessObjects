# SharpMap.BusinessObjects
With the help of this library you can use [`SharpMap`](https://github.com/SharpMap/SharpMap) with your business objects.
To achieve this you have to 
1. Prepare your business objects by decorating fields and properties with some attributes (`BusinessObjectIdentifierAttribute`, `BusinessObjectGeometryAttribute`, `BusinessObjectAttributeAttribute`) 
2. Create a `IBusinessObjectSource<T>`. There is an implementations for business objects you have in-memory (`InMemoryBusinessObjectSource<T>`) and example implementations for use with either [MongoDB](https://www.mongodb.com) or [Entity Framework 6](https://docs.microsoft.com/en-us/ef/ef6/). In the test project an example for using [NHibernate](http://nhibernate.info) is given, too.
3. Set up a `BusinessObjectProvider<T>` based on the the business object source you created above.
4. You can use that with    
   * the standard `VectorLayer` and `LabelLayer` or 
   * define a special `BusinessObjectLayer<T>` that allows for a special `IBusinessObjectRenderer<T>`.


### Prepare business objects
For your business objects you need to assign the following attributes:
* `BusinessObjectIdentifierAttribute`  
Assign this attribute to the property or field that is the unique identifier for the business object.
It must be of `System.UInt32` type and must not be assigned more than once.
* `BusinessObjectGeometryAttribute`  
Assign this attribute to the property or field that contains the spatial component of your business object. The type of the field must be `GeoAPI.Geometries.IGeometry`. It must not be assigned more than once.
* `BusinessObjectAttributeAttribute`  
Assign this attribute to all other properties or fields relevant for query by `SharpMap.Layers.ICanQueryLayer.ExecuteIntersectionQuery`

As an example have a look at this simple point of interest class:
```C#
/// <summary>
/// A simple point of interest class
/// </summary>
public class PointOfInterest
{
    /// <summary>
    /// Gets or sets a value indicating the unique identifier
    /// </summary>
    [BusinessObjectIdentifier()]
    public uint ID { get; set; }

    /// <summary>
    /// Gets or sets a value indicating the geometry
    /// </summary>
    [BusinessObjectGeometry()]
    public IGeometry Geometry { get; set; }

    /// <summary>
    /// Gets or sets a value indicating the kind of PoI
    /// </summary>
    [BusinessObjectAttribute()]
    public string Kind { get; set; }

    /// <summary>
    /// Gets or sets a value indicating the address of the PoI
    /// </summary>
    [BusinessObjectAttribute()]
    public string Address { get; set; }

    /// <summary>
    /// Gets or sets a value indicating some comments associated with the PoI
    /// </summary>
    [BusinessObjectAttribute(AllowNull = true)]
    public List<string> Comments { get; set; }
}

```


### Business object source and provider

Continuing the previous example, we simply create an in in memory object source and build a provider for it.
```C#
IEnumerable<PointOfInterest> pois = ...;

// Create source and insert items
var poiSource = new InMemoryBusinessObjectSource<PointOfInterest>();
poiSource.Insert(pois);

// Create provider
var poiProvider = new BusinessObjectProvider<PointOfInterest>(poiSource);
```

### Use of the provider in layers
You can simply use your provider along with a standard Vector- and/or LabelLayer.
```C#
var vl = new VectorLayer(poiProvider.ConnectionID, poiProvider);
```
Doing so, you have all styling and theming options you would have using those layers along with the standard data sources.  
Using a `BusinessObjectLayer<T>` you can provide a custom `IBusinessObjectRenderer<T>` that has special knowledge on how to render the business objects.
An example for such a renderer is shown in the [`LinkWithLoad`](https://github.com/SharpMap/SharpMap.BusinessObjects/blob/master/src/SharpMap.BusinessObjects.Tests/Memory/LinkLoad.cs#L35) example.
Not providing a renderer has the same effect as using the `VectorLayer`.


