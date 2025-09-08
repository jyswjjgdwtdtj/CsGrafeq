The geometric-shapes(`GeometryShape`) are arranged in the form of trees. \
Other types which extend to `Shape` can not be attached to the tree;\
The roots of the tree are points with PointGetter_Location.\
Shapes has `Parameter as GeometryShape[]` property, which refers to the shapes it is based on.\
When these points are moved or modified, the event will be delivered through the tree.\
A geometric shape:
- Shape extends to `GeometryShape`
   - CanInteract as `Boolean`, refers to if user can interact with the shape.
   - Visible as `Boolean`, refers to the visibility of shape.
   - Name as `String`
   - TypeName as `String`
   - Description as `String`
   - PointerOver as `Boolean`
   - Selected as `Boolean`
   - HitTest(location as `Vec`) as `Vec`, location refers to the point to test, returned value refers to the vector from test point to the nearest point on the shape
   - RefreshValues() as `Void`
   - SubShapes as `List<GeometryShape>`
   - Getter extends to `GeometryGetter`
     - ActionName as `String`
     - Parameters as `GeometryShape[]`, refers to the shapes it based on.
     - Attach(handler as `ShapeChangedHandler`, subShape as `GeometryShape`) as `Void`, handler should be the `RefreshValues` method of the shape, subShape should be the shape. It attachs the shape a sub-shape to the shape, or shape tree. The method will be invoked when the sub-shape is created.

When a `GeometryShape` is changed:\
1.Modify values of itself directly. (This may be caused by user's action, or from its `Parameter`)
2.Call `RefreshValues` method, indicating something is done to the shape.
3.Modify relative values.
4.`InvokeEvent` method is called in `RefreshValues`.
5.`ShapeChanged` event is Invoked.
