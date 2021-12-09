using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

public class GeoJSONReader : MonoBehaviour {
    [Range(1,10)]
    public int pointsToSkip = 0;
    public TextAsset textAsset;

    public WorldGeoData worldGeoData;

    [ContextMenu("Run")]
    void Run () {
        // var json = JsonConvert.DeserializeObject(textAsset.text);
        // using (JsonTextReader reader = new JsonTextReader(textAsset.text)) {
        // }
        JObject o = (JObject)JToken.Parse(textAsset.text);
        List<CountryGeoData> countryDataList = new List<CountryGeoData>();
        List<RegionGeoData> geoRegionsList = new List<RegionGeoData>();
        List<GeoCoord> geoCoordsList = new List<GeoCoord>();
        foreach(var countryObj in o["features"]) {
            geoCoordsList.Clear();
            
            var countryData = new CountryGeoData();
            countryData.name = countryObj["properties"]["ADMIN"].ToObject<string>();
            Debug.Log(countryData.name);

            foreach(var coords in countryObj["geometry"]["coordinates"]) {
                TryBuild(coords);
            }
            
            countryData.geoRegions = geoRegionsList.ToArray();
            countryDataList.Add(countryData);
        }
        worldGeoData.countryGeoData = countryDataList.ToArray();
        void TryBuild (JToken maybeCoordSet) {
            foreach(var coordSet in maybeCoordSet) {
                if(coordSet[0][0].Type == JTokenType.Float) {
                    ParseCoordSet(coordSet);
                } else {
                    TryBuild(coordSet);
                }
            }
            // Debug.Log(maybeCoordSet);
            // foreach(var coordSet in maybeCoordSet) {
            //     var maybeCoord = coordSet;
            //     Debug.Log(maybeCoord);
            //     if(maybeCoord.Type == JTokenType.Array) {
            //         TryBuild(maybeCoordSet);
            //     } else {
            //         ParseCoordSet(maybeCoord);
            //     }
            // }
        }

        void ParseCoordSet (JToken coordSet) {
            int i = 0;
            foreach(var coord in coordSet) {
                if(i%pointsToSkip == 0) {
                    var geoCoord = new GeoCoord();
                    geoCoord.name = i.ToString();
                    // Debug.Log(coord[0].Type+" "+coord[0]);
                    geoCoord.longitude = coord[0].ToObject<float>();
                    geoCoord.latitude = coord[1].ToObject<float>();
                    geoCoordsList.Add(geoCoord);
                }
                i++;
            }
            var r = new RegionGeoData();
            r.geoCoords = geoCoordsList.ToArray();
            geoRegionsList.Add(r);
        }
    }
    
}
