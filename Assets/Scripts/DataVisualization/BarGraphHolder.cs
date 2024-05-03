using PolyAndCode.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CubeHole
{
    public class BarGraphInfo
    {
        public List<BarGraphData> data = new List<BarGraphData>();
        public string label;
    }
    public class BarGraphHolder : MonoBehaviour,ICell
    {
        [SerializeField] private BarGraphBar[] bars;
        [SerializeField] private TextMeshProUGUI label;
        public void Init(BarGraphInfo barGraphInfo)
        {
            label.text = barGraphInfo.label;
            for (int i = 0; i < bars.Length; i++)
            {
                bars[i].SetBar(barGraphInfo.data[i]);
            }
        }
    }
}
