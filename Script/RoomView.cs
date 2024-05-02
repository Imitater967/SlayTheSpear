using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI.Extensions;

namespace Script {
    public class RoomView : MonoBehaviour {
        public UILineRenderer LineRenderer;
        public TMP_Text Text;
        public void SetNextPosition(Vector2 position) {
            var lineRendererPoints = LineRenderer.Points;
            var a = lineRendererPoints.ToList();
            a.AddRange(new Vector2[] { Vector2.zero, position });
            LineRenderer.Points = a.ToArray();
        }

        public void SetRoomType(Room.Type type) {
            Text.text = type.ToString().ToCharArray()[0]+"";
        }
    }
}