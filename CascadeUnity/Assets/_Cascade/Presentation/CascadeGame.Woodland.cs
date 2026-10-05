using System.Collections.Generic;
using TMPro;
using UnityEngine;
namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        sealed class ContactFace {public int floor,slot;public TextMeshProUGUI label;}
        readonly List<ContactFace> contactFaces=new List<ContactFace>();
        void LateUpdate()
        {
            if(machine==null||Interaction==null)return;
            machine.TickWoodDynamics(Time.unscaledDeltaTime);
            for(int i=0;i<tokenLabels.Count;i++) {
                var rect=tokenLabels[i].rectTransform;var position=machine.TokenVisualPosition(i);position.z=rect.position.z;rect.position=position;
                rect.rotation=Quaternion.Euler(0,0,machine.TokenVisualAngle(i));
            }
            foreach(var face in contactFaces)if(face.label!=null&&face.label.gameObject.activeInHierarchy){var rect=face.label.rectTransform;var position=machine.ContactPosition(face.floor,face.slot);position.z=rect.position.z;rect.position=position;}
        }
    }
}
