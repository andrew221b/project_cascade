using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        // Visible alpha-bound centres measured from the preserved source art.
        // Audit/source sprite regions: Reports/CardIllustrationCentresOct29/ink-centres.json.
        // No runtime texture readback; keep worn/animated character pivots unchanged.
        static readonly Dictionary<string,Vector2> catalogueArtFocus=new Dictionary<string,Vector2>{
            {"leaf",new Vector2(0.50000000f,0.50000000f)},
            {"sun",new Vector2(0.51454363f,0.51562500f)},
            {"sky",new Vector2(0.50683594f,0.50295858f)},
            {"moon",new Vector2(0.52392578f,0.50789733f)},
            {"wind_chime",new Vector2(0.50000000f,0.50112613f)},
            {"rain_badge",new Vector2(0.51513672f,0.47580645f)},
            {"copper_acorn",new Vector2(0.50000000f,0.50000000f)},
            {"beacon_brooch",new Vector2(0.49862259f,0.50000000f)},
            {"sky_pilot",new Vector2(0.47582873f,0.47152429f)},
            {"moth_mask",new Vector2(0.43024862f,0.46314908f)},
            {"garden_festival",new Vector2(0.43922652f,0.46984925f)},
            {"cloud_gardener",new Vector2(0.44198895f,0.46314908f)},
            {"explorer_hat",new Vector2(0.47859116f,0.48492462f)},
            {"frog_suit",new Vector2(0.48411602f,0.48241206f)},
            {"star_knight",new Vector2(0.49033149f,0.47654941f)},
            {"fox_mask",new Vector2(0.46685083f,0.47571189f)},
            {"mushroom_cap",new Vector2(0.47859116f,0.38860972f)},
            {"bee_suit",new Vector2(0.47099448f,0.41457286f)},
            {"winter_hood",new Vector2(0.50000000f,0.36850921f)},
            {"pirate_hat",new Vector2(0.44958564f,0.44053601f)},
            {"jester_suit",new Vector2(0.52417127f,0.42964824f)},
            {"constellation_mage",new Vector2(0.47237569f,0.44556114f)},
            {"paper_crane",new Vector2(0.55855856f,0.51126126f)},
            {"tiny_comet",new Vector2(0.48081264f,0.49549550f)},
            {"dew_bell",new Vector2(0.53273138f,0.50225225f)},
            {"night_flower",new Vector2(0.50450450f,0.49549550f)},
            {"cloud_kite",new Vector2(0.53941441f,0.53724605f)},
            {"fern_compass",new Vector2(0.49887133f,0.54063205f)},
            {"amber_wing",new Vector2(0.50790068f,0.53498871f)},
            {"moon_boat",new Vector2(0.49211712f,0.53273138f)},
            {"petal_drift",new Vector2(0.50000000f,0.49697885f)},
            {"firefly_dance",new Vector2(0.50271003f,0.50664894f)},
            {"butterfly_garden",new Vector2(0.50000000f,0.50000000f)},
            {"swallow_flock",new Vector2(0.50000000f,0.50000000f)},
            {"dragonfly_stream",new Vector2(0.50000000f,0.50000000f)},
            {"lantern_flight",new Vector2(0.50185874f,0.50506329f)},
            {"snow_wings",new Vector2(0.50000000f,0.50000000f)},
            {"comet_trails",new Vector2(0.49850299f,0.50305810f)},
            {"ribbon_kites",new Vector2(0.50000000f,0.50000000f)},
            {"origami_cranes",new Vector2(0.50000000f,0.50000000f)},
            {"bubble_dream",new Vector2(0.50000000f,0.50000000f)},
            {"aurora_veil",new Vector2(0.50347222f,0.50000000f)},
            {"garden_frame",new Vector2(0.50000000f,0.50000000f)},
            {"sky_frame",new Vector2(0.50000000f,0.50000000f)},
            {"copper_frame",new Vector2(0.50000000f,0.50000000f)},
            {"lunar_frame",new Vector2(0.50000000f,0.50000000f)},
            {"tool/0",new Vector2(0.49912434f,0.50096339f)},
            {"tool/1",new Vector2(0.50837321f,0.49481659f)},
            {"tool/2",new Vector2(0.50000000f,0.50000000f)},
            {"tool/3",new Vector2(0.49838188f,0.50000000f)},
            {"tool/4",new Vector2(0.50195312f,0.49511719f)},
            {"tool/5",new Vector2(0.50000000f,0.50112613f)},
        };
        Image CataloguePicture(Transform parent,Sprite sprite,float x,float y,float width,float height,string artKey)
        {
            var picture=ReferencePicture("Catalogue object",parent,sprite,x,y,width,height);
            var rect=picture.rectTransform;var previous=rect.pivot;rect.pivot=Vector2.one*.5f;
            rect.anchoredPosition+=Vector2.Scale(rect.pivot-previous,rect.rect.size);
            if(sprite!=null&&catalogueArtFocus.TryGetValue(artKey,out var focus)){
                var size=sprite.rect.size;float fit=Mathf.Min(width/size.x,height/size.y);
                rect.anchoredPosition-=Vector2.Scale(focus-Vector2.one*.5f,size)*fit;
            }
            return picture;
        }
    }
}
