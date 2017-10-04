using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FloodZone.Models
{
  public class LatLong
  {
    public double Latitude { get; set; } = 30.223404;//30.000598;
    public double Longitude { get; set; } = -81.651925; //-81.667869;
    public double OriginalX { get; set; }
    public double OriginalY { get; set; }
    public bool IsValid { get; set; } = false;
    public bool IsTested { get; set; } = false;

    public LatLong(double? X, double? Y)
    {
      IsTested = true;
      if (!X.HasValue | !Y.HasValue)
      { // if we don't have a value for X or Y, we want to just quit here.
        return;
      }
      OriginalX = X.Value;
      OriginalY = Y.Value;
      IsValid = true;
      string source_wkt = @"PROJCS[""NAD_1983_HARN_StatePlane_Florida_East_FIPS_0901_Feet"",GEOGCS[""GCS_North_American_1983_HARN"",DATUM[""NAD83_High_Accuracy_Regional_Network"",SPHEROID[""GRS_1980"",6378137.0,298.257222101]],PRIMEM[""Greenwich"",0.0],UNIT[""Degree"",0.0174532925199433]],PROJECTION[""Transverse_Mercator""],PARAMETER[""False_Easting"",656166.6666666665],PARAMETER[""False_Northing"",0.0],PARAMETER[""Central_Meridian"",-81.0],PARAMETER[""Scale_Factor"",0.9999411764705882],PARAMETER[""Latitude_Of_Origin"",24.33333333333333],UNIT[""Foot_US"",0.3048006096012192]]";
      var x = new ProjNet.CoordinateSystems.CoordinateSystemFactory();
      var csource = x.CreateFromWkt(source_wkt);
      var ctarget = ProjNet.CoordinateSystems.GeographicCoordinateSystem.WGS84;
      var t = new ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory();
      var trans = t.CreateFromCoordinateSystems(csource, ctarget);
      double[] point = { X.Value, Y.Value };
      double[] convpoint = trans.MathTransform.Transform(point);
      Longitude = convpoint[0];
      Latitude = convpoint[1];
    }

    public LatLong()
    {

    }
  }
}