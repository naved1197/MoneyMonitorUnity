using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace CubeHole
{
    public enum BarType { Horizontal, Vertical}
    [System.Serializable]
    public class BarGraphData
    {
        public float value;
        public string label;
        public Color color;
        public BarType type;

        public BarGraphData()
        {

        }
        public BarGraphData(float value, string label, Color color, BarType type)
        {
            this.value = value;
            this.label = label;
            this.color = color;
            this.type = type;
        }
    }
    public class BarGraphBar : MonoBehaviour
    {
        [SerializeField] private Image bar;
        [SerializeField] private TextMeshProUGUI barLabel;

        public void SetBar(BarGraphData barData)
        {
            bar.color = barData.color;
            barLabel.text = barData.label;
            if (barData.type == BarType.Horizontal)
            {
                bar.rectTransform.sizeDelta = new Vector2(barData.value, bar.rectTransform.sizeDelta.y);
            }
            else
            {
                bar.rectTransform.sizeDelta = new Vector2(bar.rectTransform.sizeDelta.x, barData.value);
            }
        }
    }
}
