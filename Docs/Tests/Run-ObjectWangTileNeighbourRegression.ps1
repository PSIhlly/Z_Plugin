param([string]$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path)
$ErrorActionPreference = 'Stop'
$source = Get-Content -LiteralPath (Join-Path $ProjectRoot 'Assets/GameSample/MyScripts/Main/GameMapController.cs') -Raw
function Get-Method([string]$anchor) {
    $start = $source.IndexOf($anchor)
    if ($start -lt 0) { throw "Source anchor missing: $anchor" }
    $open = $source.IndexOf('{', $start)
    $depth = 1
    for ($i = $open + 1; $i -lt $source.Length; $i++) {
        if ($source[$i] -eq '{') { $depth++ }
        elseif ($source[$i] -eq '}') { $depth-- }
        if ($depth -eq 0) { return $source.Substring($start, $i - $start + 1) }
    }
    throw "Unclosed method: $anchor"
}
$fieldsStart = $source.IndexOf('private readonly Dictionary<ObjectUnit, (Bounds bounds, int templateUid, bool notified)> objectWangTileBounds')
$fieldsEnd = $source.IndexOf('private static readonly (int x, int z, int bit)[] wangTileNeighbours', $fieldsStart)
if ($fieldsStart -lt 0 -or $fieldsEnd -lt $fieldsStart) { throw 'WangTile cache source boundaries missing.' }
$fields = $source.Substring($fieldsStart, $fieldsEnd - $fieldsStart)
$methods = @(
    'internal static int GetObjectNeighbourBits(',
    'private static bool TryGetObjectWangTileBounds(',
    'private static void CollectObjectWangTileCandidates(',
    'private int GetObjectWangTileMask(',
    'private static bool HasObjectTemplate(',
    'private void UpdateObjectWangTileNeighbours(',
    'private void RemoveObjectWangTileNeighbours(',
    'private void RefreshChangedObjectWangTileNeighbour('
) | ForEach-Object { Get-Method $_ }
# Execute the actual query/cache/invalidation methods. Small geometry, column
# and appearance spies measure queries and refreshes without a scene or saves.
$fixture = @'
using System;
using System.Collections.Generic;
public struct Vector3 : IEquatable<Vector3> {
    public float x,y,z;
    public Vector3(float x,float y,float z) { this.x=x;this.y=y;this.z=z; }
    public static Vector3 operator +(Vector3 a,Vector3 b) => new Vector3(a.x+b.x,a.y+b.y,a.z+b.z);
    public static Vector3 operator -(Vector3 a,Vector3 b) => new Vector3(a.x-b.x,a.y-b.y,a.z-b.z);
    public static Vector3 operator *(Vector3 a,float b) => new Vector3(a.x*b,a.y*b,a.z*b);
    public bool Equals(Vector3 b) => x==b.x && y==b.y && z==b.z;
}
public struct Vector3Int { public int x,y,z; public Vector3Int(int x,int y,int z) {this.x=x;this.y=y;this.z=z;} }
public struct Bounds : IEquatable<Bounds> {
    public Vector3 center,size;
    public Bounds(Vector3 center,Vector3 size) { this.center=center;this.size=size; }
    public Vector3 min => center-size*.5f;
    public Vector3 max => center+size*.5f;
    public bool Equals(Bounds b) => center.Equals(b.center) && size.Equals(b.size);
}
public static class Mathf {
    public static float Abs(float v) => Math.Abs(v);
    public static float Min(float a,float b) => Math.Min(a,b);
    public static float Max(float a,float b) => Math.Max(a,b);
    public static int CeilToInt(float v) => (int)Math.Ceiling(v);
    public static int FloorToInt(float v) => (int)Math.Floor(v);
}
public class ObjectData { public Bounds bounds; public bool hasBounds=true; public ObjectProductForm.Data product; }
public class MapUnit { public ObjectData data=new ObjectData(); public bool isShowing=true; }
public class ObjectUnit : MapUnit { public ObjectProductForm.Data product => data.product; public int refreshes,shownMask; }
public static class ObjectProductForm {
    public class Data { public int templateUid; public bool isWangTile=true,boundsCollision; }
    public static int GetTemplateUid(Data data) => data.templateUid;
}
public class MapManager {
    public static MapManager instance=new MapManager();
    public readonly Util utilCtrl=new Util(); public readonly Update updateCtrl=new Update();
    public class Util {
        public int reads;
        public Vector3 RealPos2MapPos(Vector3 v) => v;
        public bool TryGetVisionBounds(ObjectData data,out Bounds bounds) { reads++;bounds=data.bounds;return data.hasBounds; }
    }
    public class Update {
        public int queries;
        public readonly List<MapUnit> units=new List<MapUnit>();
        public void CollectHistoryColumn(Vector3Int position,HashSet<MapUnit> result) { queries++;result.UnionWith(units); }
    }
}
public class ControllerFixture {
__FIELDS__
__METHODS__
    public void Notify(ObjectUnit unit) => UpdateObjectWangTileNeighbours(unit,unit.data.product);
    public void Remove(ObjectUnit unit) { MapManager.instance.updateCtrl.units.Remove(unit);RemoveObjectWangTileNeighbours(unit,unit.data.product); }
    public int Mask(ObjectUnit unit) => GetObjectWangTileMask(unit,unit.data.product.templateUid,unit.data.bounds);
    public int MaskCount => objectWangTileMasks.Count;
    private void RefreshObjectAppearance(ObjectUnit unit,ObjectProductForm.Data data) { unit.refreshes++;unit.shownMask=Mask(unit); }
}
public static class ObjectWangTileNeighbourRegression {
    private static int checks;
    private static ControllerFixture ctrl;
    private static ObjectUnit Unit(float x,float z,int template=1,bool showing=true,bool fit=false,float y=0) {
        return new ObjectUnit {isShowing=showing,data=new ObjectData {bounds=new Bounds(new Vector3(x,y,z),new Vector3(1,1,1)),
            product=new ObjectProductForm.Data {templateUid=template,boundsCollision=fit}}};
    }
    private static void Check(bool ok,string message) { if(!ok) throw new Exception(message);checks++; }
    private static void Setup(params ObjectUnit[] units) {
        ctrl=new ControllerFixture();MapManager.instance=new MapManager();
        MapManager.instance.updateCtrl.units.AddRange(units);
        foreach(var unit in units) ctrl.Notify(unit);
        foreach(var unit in units) if(unit.data.product.isWangTile) ctrl.Mask(unit);
        ClearCounters();
    }
    private static void ClearCounters() {
        MapManager.instance.updateCtrl.queries=0;MapManager.instance.utilCtrl.reads=0;
        foreach(var unit in MapManager.instance.updateCtrl.units) if(unit is ObjectUnit obj) obj.refreshes=0;
    }
    private static void Move(ObjectUnit unit,float x,float z,float y=0) {unit.data.bounds=new Bounds(new Vector3(x,y,z),unit.data.bounds.size);ctrl.Notify(unit);}
    public static int Run() {
        var a=Unit(0,0);var b=Unit(1,0);Setup(a,b);
        Check(ctrl.Mask(a)==16 && ctrl.Mask(b)==8,"matching templates connect with the real east/west bit order");
        for(int i=0;i<10000;i++) {ctrl.Notify(a);ctrl.Mask(a);}
        Check(MapManager.instance.updateCtrl.queries==0,"ten thousand unchanged Show/Move/Refresh calls perform no column queries after warmup");
        Check(b.refreshes==0,"unchanged footprint notifications never refresh neighbours");
        Move(b,1.25f,0);
        Check(a.refreshes==1 && a.shownMask==0,"moving away immediately refreshes the old neighbour once");
        Check(ctrl.Mask(b)==0,"the moved unit loses its cached old connectivity");
        ClearCounters();ctrl.Notify(b);ctrl.Mask(b);
        Check(MapManager.instance.updateCtrl.queries==0,"a repeated notification after movement returns to the warm fast path");
        Move(b,1,0);
        Check(a.shownMask==16 && a.refreshes==1,"moving back restores the neighbour immediately");

        var duplicate=Unit(1,0);Setup(a,b,duplicate);
        Move(b,2.25f,0);
        Check(a.refreshes==0 && ctrl.Mask(a)==16,"another object covering the same edge avoids an unnecessary appearance/collision refresh");
        ClearCounters();ctrl.Remove(duplicate);
        Check(a.refreshes==1 && a.shownMask==0,"removing the last object on an edge clears the remaining neighbour immediately");

        a=Unit(0,0,showing:false);b=Unit(1,0);Setup(a,b);
        Move(b,2.25f,0);
        Check(a.refreshes==0,"hidden neighbours without fitted collision update lazily");
        ClearCounters();Check(ctrl.Mask(a)==0 && MapManager.instance.updateCtrl.queries>0,"a hidden neighbour cannot reuse a mask from before an off-screen move");
        a=Unit(0,0,showing:false,fit:true);b=Unit(1,0);Setup(a,b);
        Move(b,2.25f,0);
        Check(a.refreshes==1 && a.shownMask==0,"hidden fitted neighbours refresh even when the moving instance does not enable fitting");

        a=Unit(0,0);b=Unit(1,0);b.data.product.isWangTile=false;Setup(a,b);
        Check(ctrl.Mask(a)==16,"ordinary instances of the same template still contribute to WangTile connectivity");
        Move(b,2.25f,0);
        Check(a.refreshes==1 && a.shownMask==0,"moving an ordinary connected instance invalidates WangTile neighbours too");
        var added=Unit(1,0);added.data.product.isWangTile=false;MapManager.instance.updateCtrl.units.Add(added);ClearCounters();ctrl.Notify(added);
        Check(a.refreshes==1 && a.shownMask==16,"adding an ordinary instance invalidates an already cached template");
        ClearCounters();ctrl.Remove(added);
        Check(a.refreshes==1 && a.shownMask==0,"removing an ordinary connected instance uses its saved old bounds");

        a=Unit(-1,0);var early=Unit(1,0);Setup(a,early);b=Unit(0,0);
        MapManager.instance.updateCtrl.units.Add(b);
        early.data.bounds=new Bounds(new Vector3(1,.02f,0),new Vector3(1,1,1));
        Check(ctrl.Mask(early)==8,"a cold mask can observe a newly indexed unit before its registration notification");
        ctrl.Notify(b);
        Check(a.refreshes==1 && a.shownMask==16,"observing bounds does not suppress the new unit's first neighbour notification");

        a=Unit(0,0);b=Unit(1,0,template:2);var c=Unit(2,0,template:2);Setup(a,b,c);
        b.data.product=new ObjectProductForm.Data {templateUid=1};ctrl.Notify(b);
        Check(a.refreshes==1 && a.shownMask==16,"reassigning a template with unchanged geometry refreshes the new product's neighbours");
        Check(c.refreshes==1 && c.shownMask==0,"reassigning a template also disconnects the old product's neighbours");
        Check(ctrl.Mask(b)==8,"the rebound object cannot retain its old product mask");

        a=Unit(0,0);b=Unit(1,0);Setup(a,b);
        b.data.bounds=new Bounds(new Vector3(1.5f,0,0),new Vector3(2,1,1));ctrl.Notify(b);
        Check(a.refreshes==0 && ctrl.Mask(a)==16,"a resized footprint preserving edge connectivity does not redo neighbour appearance");
        Move(b,1.5f,0,2);
        Check(a.refreshes==1 && a.shownMask==0,"separate Y volumes do not connect after a height change");
        a=Unit(0,0);b=Unit(1,1);Setup(a,b);
        Check(ctrl.Mask(a)==4 && ctrl.Mask(b)==32,"diagonal corner contacts retain the real northeast/southwest bit order");
        Move(b,1.0005f,1);
        Check(ctrl.Mask(a)==4,"contact tolerance is preserved for tiny changes");
        Move(b,1.002f,1);
        Check(ctrl.Mask(a)==0,"exact bounds changes are observed when contact crosses the tolerance");

        a=Unit(0,0);b=Unit(1,0);Setup(a,b);b.data.hasBounds=false;ctrl.Notify(b);
        Check(a.shownMask==0,"losing visual bounds disconnects old neighbours");
        b.data.hasBounds=true;ctrl.Notify(b);
        Check(a.shownMask==16,"recovering bounds reconnects neighbours");
        ClearCounters();var unrelated=Unit(0,1,template:99);unrelated.data.product.isWangTile=false;
        MapManager.instance.updateCtrl.units.Add(unrelated);ctrl.Notify(unrelated);
        Check(MapManager.instance.updateCtrl.queries==0 && a.refreshes==0,"unrelated ordinary products do not enter the expensive WangTile path");
        Check(ctrl.MaskCount>0,"the tests exercise the persistent mask cache");
        a=Unit(0,0);b=Unit(1,0);Setup(a,b);b.data.product=null;ctrl.Remove(b);
        Check(a.refreshes==1 && a.shownMask==0,"removal can invalidate surviving neighbours after the source Product row is gone");
        return checks;
    }
}
'@
Add-Type -TypeDefinition $fixture.Replace('__FIELDS__', $fields).Replace('__METHODS__', ($methods -join "`n"))
try { $checks = [ObjectWangTileNeighbourRegression]::Run() }
catch { throw $_.Exception.GetBaseException() }
Write-Output "OBJECT_WANG_TILE_NEIGHBOUR_REGRESSION_PASS checks=$checks unchangedCalls=10000 columnQueries=0"
