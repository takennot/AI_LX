using System.Text;
using UnityEngine;

namespace L5
{
    public class GoapDebugHUD : MonoBehaviour
    {
        public GoapAgent agent;
        [Header("HUD")]
        public bool show = true;
        public Vector2 screenOffset = new Vector2(10, 10);
        GUIStyle _style;
        void Awake()
        {

        }
        void OnGUI()
        {
            _style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 36,
                richText = true
            };
            if (!show || agent == null)
            {
                return;
            }
            var sb = new StringBuilder();
            sb.AppendLine("<b>GOAP Debug</b>");
            sb.AppendLine(agent.GetDebugString());
            GUI.Label(new Rect(screenOffset.x, screenOffset.y, 1920, 1080), sb.ToString(), _style);
        }

    }
}
