using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
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
        var subObjects = AssetDatabaseX.GetSubObjectsOfTypeAsScriptableObjects<CountryGeoData>(worldGeoData);
        foreach(var subObject in subObjects) {
            Object.DestroyImmediate(subObject, true);
        }
        JObject o = (JObject)JToken.Parse(textAsset.text);
        List<CountryGeoData> countryDataList = new List<CountryGeoData>();
        List<RegionGeoData> geoRegionsList = new List<RegionGeoData>();
        var pathOfWorldGeoData = AssetDatabase.GetAssetPath(worldGeoData);
        foreach(var countryObj in o["features"]) {
            geoRegionsList.Clear();
            
            var countryData = ScriptableObject.CreateInstance<CountryGeoData>();
            AssetDatabase.AddObjectToAsset(countryData, pathOfWorldGeoData);
            countryData.name = countryObj["properties"]["ADMIN"].ToObject<string>();

            foreach(var coords in countryObj["geometry"]["coordinates"]) {
                TryBuild(coords);
            }
            
            countryData.geoRegions = geoRegionsList.ToArray();
            countryDataList.Add(countryData);
        }
        worldGeoData.countryGeoData = countryDataList.ToArray();
        AssetDatabase.ImportAsset(pathOfWorldGeoData);
        
        void TryBuild (JToken maybeCoordSet) {
            if(maybeCoordSet[0][0].Type == JTokenType.Float) {
                ParseCoordSet(maybeCoordSet);
            } else {
                foreach(var coordSet in maybeCoordSet) {
                    TryBuild(coordSet);
                }
            }
        }

        void ParseCoordSet (JToken coordSet) {
            List<GeoCoord> geoCoordsList = new List<GeoCoord>();
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