using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace CubeHole
{


    public class PieLabel : MonoBehaviour
    {
        [SerializeField] private RectTransform labelTransform;
        [SerializeField] private Image labelLine;
        [SerializeField] private TextMeshProUGUI labelTxt;

        public void Init(float angle, Vector2 labelDimension, float lineHieght, Color lineColor, string label)
        {
            labelTransform.pivot = new Vector2(0.5f, 0);
            labelTransform.localEulerAngles = new Vector3(0, 0, angle);
            labelTransform.sizeDelta = labelDimension;
            labelLine.rectTransform.sizeDelta = new Vector2(labelLine.rectTransform.sizeDelta.x, lineHieght);
            labelLine.color = lineColor;
            labelTxt.text = label;
            labelTxt.color = lineColor;

            labelTxt.rectTransform.localEulerAngles = new Vector3(0, 0, -angle);
        }
    }
}
