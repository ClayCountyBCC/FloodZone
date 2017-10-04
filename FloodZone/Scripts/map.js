"use strict";
var EM = null;
var LocationLayer = null;

function MapStart()
{
  require(["esri/map",
    "esri/layers/ArcGISDynamicMapServiceLayer",
    "esri/dijit/BasemapToggle",
    "esri/dijit/Legend",
    "dojo/_base/array",
    "dojo/parser",
    "esri/layers/ImageParameters",
    "dijit/layout/BorderContainer",
    "dojo/domReady!"],
    function (
      Map,
      ArcGISDynamicMapServiceLayer,
      BasemapToggle,
      Legend,
      arrayUtils,
      Parser,
      ImageParameters
    )
    {
      Parser.parse();

      EM = new Map("FloodMap", {
        center: [-81.80, 29.950],
        zoom: 10,
        basemap: "osm",
        smartNavigation: false,
        logo: false,
        maxZoom: 18
      });
      var toggle = new BasemapToggle({
        map: EM,
        basemap: "satellite" //hybrid
      }, "BasemapToggle");
      toggle.startup();
      //EM.disablePan();
      //EM.disableScrollWheelZoom();
      //EM.disableMapNavigation();
      var imageParameters = new ImageParameters();
      imageParameters.layerIds = [0, 1];
      imageParameters.layerOption = ImageParameters.LAYER_OPTION_SHOW;
      var floodZone = new ArcGISDynamicMapServiceLayer('https://maps.claycountygov.com:6443/arcgis/rest/services/FEMA/MapServer', { "imageParameters": imageParameters });
      var parcels = new ArcGISDynamicMapServiceLayer('https://maps.claycountygov.com:6443/arcgis/rest/services/Parcel/MapServer');
      //add the legend
      EM.on("layers-add-result", function (evt)
      {
        console.log('layers', evt.layers);
        var layerInfo = arrayUtils.map(evt.layers, function (layer, index)
        {
          return { layer: layer.layer, title: layer.layer.name };
        });
        if (layerInfo.length > 0)
        {
          var legendDijit = new Legend({
            map: EM,
            layerInfos: layerInfo
          }, "legendDiv");
          legendDijit.startup();
        }
      });

      LocationLayer = new esri.layers.GraphicsLayer();
      EM.addLayers([floodZone, parcels, LocationLayer]);
    });
}
function Zoom(latlong, FloodZone)
{
  var results = document.getElementById("results");
  window.scrollTo(0, results.offsetTop);
  console.log('results.offsetTop', results.offsetTop);
  require(["esri/geometry/Point",
    "esri/symbols/PictureMarkerSymbol",
    "esri/graphic",
    "esri/SpatialReference",
    "esri/symbols/TextSymbol"],
    function (Point, PictureMarkerSymbol, Graphic, SpatialReference, TextSymbol)
    {
      var symbol = new PictureMarkerSymbol({
        "angle": 0,
        "xoffset": 0,
        "yoffset": 0,
        "type": "esriPMS",
        "contentType": "image/png",
        "width": 20,
        "height": 20,
        "url": "http://static.arcgis.com/images/Symbols/Basic/GreenSphere.png"
      });
      var textSymbol = new TextSymbol('Flood Zone: ' + FloodZone); //esri.symbol.TextSymbol(data.Records[i].UnitName);
      textSymbol.setColor(new dojo.Color([0, 0, 0]));
      textSymbol.setAlign(TextSymbol.ALIGN_MIDDLE);
      textSymbol.setOffset(0, -25);
      textSymbol.setHaloColor(new dojo.Color([255, 255, 255]));
      textSymbol.setHaloSize(3);
      LocationLayer.clear();
      var p = new Point([latlong.Longitude, latlong.Latitude]);
      //var p = new Point([latlong.OriginalX, latlong.OriginalY], new SpatialReference({ wkid: 4326 }));
      //var wmIncident = esri.geometry.geographicToWebMercator(p);
      //var graphic = new Graphic(wmIncident);
      var font = new esri.symbol.Font();
      font.setSize("14pt");
      font.setWeight(esri.symbol.Font.WEIGHT_BOLD);
      textSymbol.setFont(font);
      var graphic = new Graphic(p);
      graphic.setSymbol(symbol);
      var s = new Graphic(p);
      s.setSymbol(textSymbol);
      LocationLayer.add(graphic);
      LocationLayer.add(s);
      EM.centerAndZoom(p, 18);
    });

}