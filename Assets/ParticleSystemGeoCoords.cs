using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ParticleSystemGeoCoords : MonoBehaviour
{
    public WorldGeoData worldGeoData;
    public new ParticleSystem particleSystem;

    void OnEnable () {
        Set();
    }
    [ContextMenu("Set")]
    void Set () {
        particleSystem.Stop();
        particleSystem.Clear();
        var particles = new List<ParticleSystem.Particle>();
        foreach(var country in worldGeoData.countryGeoData) {
            foreach(var geoRegion in country.geoRegions) {
                foreach(var geoCoord in geoRegion.geoCoords) {
                    var emitParams = new ParticleSystem.EmitParams();
                    emitParams.position = geoCoord.PolarToCartesian() * Globe.Instance.radius;
                    particleSystem.Emit(emitParams, 1);
                }
            }
        }
    }
}
