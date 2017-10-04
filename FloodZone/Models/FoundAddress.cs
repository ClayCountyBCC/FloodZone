using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using Dapper;

namespace FloodZone.Models
{
  public class FoundAddress
  {
    private LatLong _ll = new LatLong();
    public long ObjectId { get; set; }
    public string FloodZone { get; set; } = "";
    public string WholeAddress { get; set; }
    public string City { get; set; }
    public string Zip { get; set; }
    public double XCoord { get; set; }
    public double YCoord { get; set; }
    public LatLong ToLatLong
    {
      get
      {
        if (!_ll.IsTested)
        {
          _ll = new LatLong(XCoord, YCoord);
          return _ll;
        }
        else
        {
          return _ll;
        }
      }
    }

    public FoundAddress()
    {

    }

    public static List<FoundAddress> Find(SearchAddress sa)
    {
      sa.street = sa.street.ToUpper().Trim();
      var replacements = new Dictionary<string, string>
      { { " RD", "" },
        { " ROAD", "" },
        { " DR", "" },
        { " DRIVE", "" },
        { " PKWY", "" },
        { " CT", "" },
        { " LN", "" },
        { " LANE", "" },
        { " BLVD", "" },
        { " ST", "" },
        { " STREET", "" },
        { " CIR", "" },
        { " CV", "" },
        { " PT", "" },
        { " RUN", "" },
        { " TR", "" },
        { " CONC", "" },
        { " TER", "" },
        { " TRC", "" },
        { " LOOP", "" },
        { " WAY", "" }
      };

      var filtered = replacements.Aggregate(sa.street, (current, replacement) => current.Replace(replacement.Key, replacement.Value));
      var dpa = new DynamicParameters();
      dpa.Add("@house", sa.house);
      dpa.Add("@street", sa.street);
      dpa.Add("@filtered", filtered);
      string query = $@"
        WITH AddressPoints AS (

        SELECT 
          OBJECTID,
          UPPER(WholeAddress) AS WholeAddress, 
          Community AS City,
          Zip, 
          XCoord,
          YCoord,
          Shape FROM ADDRESS_SITE 
	        WHERE (UPPER(WholeAddress) LIKE '%' + @street + '%' OR 
            UPPER(WholeAddress) LIKE '%' + @filtered + '%') 
		        AND House = @house
        ), FloodZoneCodes AS (
          SELECT 
            '1001' FloodZone, 
            'AE' Code,
            '' Floodway
          UNION ALL 
          SELECT 
            '1001' FloodZone, 
            'AE-FLOODWAY' Code,
            '1000' Floodway
          UNION ALL 
          SELECT 
            '2000' FloodZone, 
            '0.2 PCT ANNUAL CHANCE FLOOD HAZARD' Code, 
            '' Floodway
          UNION ALL 
          SELECT 
            '1000' FloodZone, 
            'A' Code, 
            '' Floodway
          UNION ALL 
          SELECT 
            '4002' FloodZone, 
            'X' Code, 
            '' Floodway
          UNION ALL 
          SELECT 
            'A' FloodZone, 
            'A' Code, 
            '' Floodway
          UNION ALL 
          SELECT 
            'AE' FloodZone, 
            'AE' Code, 
            '' Floodway
          UNION ALL 
          SELECT 
            'AH' FloodZone, 
            'AH' Code, 
            '' Floodway  
        )

        SELECT DISTINCT  
        FZC.Code FloodZone,
        A.OBJECTID ObjectId,
        A.WholeAddress,
        A.City,
        A.Zip,
        A.XCoord,
        A.YCoord
        FROM AddressPoints A 
        INNER JOIN PARCEL_INFO P ON P.Shape.STIntersects(A.Shape) = 1
        LEFT OUTER JOIN DFIRM_FLOODZONES F ON P.Shape.STIntersects(F.Shape) = 1
        LEFT OUTER JOIN FloodZoneCodes FZC ON F.FLD_ZONE = FZC.FloodZone AND F.FLOODWAY = FZC.Floodway";
      try
      {
        using (IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["GIS"].ConnectionString))
        {
          List<FoundAddress> fal =  (List<FoundAddress>)db.Query<FoundAddress>(query, dpa);
          if (fal.Count() == 0) return fal;
          var rl = new List<FoundAddress>();
          var objects = (from o in fal
                         select o.ObjectId).Distinct().ToList();
          foreach( long obj in objects)
          {
            var fa = new FoundAddress();
            var t = (from f in fal
                     where f.ObjectId == obj
                     select f).ToList();
           foreach(FoundAddress f in t)
            {
              fa.ObjectId = f.ObjectId;
              fa.WholeAddress = f.WholeAddress;
              fa.City = f.City;
              fa.Zip = f.Zip;
              fa.XCoord = f.XCoord;
              fa.YCoord = f.YCoord;
              if (fa.FloodZone.Length > 0) fa.FloodZone += ", ";
              fa.FloodZone += f.FloodZone;
            }
            rl.Add(fa);
          }
          return rl;
            
        }
      }
      catch (Exception ex)
      {
        new ErrorLog(ex, query);
        return null;
      }
    }
  }
}