using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
namespace CubeHole
{
    public class PieChartHandler : MonoBehaviour
    {
        [SerializeField] private RectTransform chartHolder;
        [SerializeField] private PieLabel pieLabelPrefab;
        [SerializeField] private bool isFill = false;
        [SerializeField] private float thickness = 200;
        [SerializeField] private Color baseColor;
        [SerializeField] private GameObject emptyGraphics;
        private float diameter;
        private List<UICircle> circles = new List<UICircle>();
        private List<PieLabel> pieLabels = new List<PieLabel>();
        private int circleCount = 0;
        private int pieLabelCount = 0;

        public void Init(List<BarGraphData> barGraphs)
        {
            if (barGraphs.Count == 0)
            {
                emptyGraphics.gameObject.SetActive(true);
            }
            else
                emptyGraphics.gameObject.SetActive(false);

            circleCount = 0;
            pieLabelCount = 0;
            foreach (var item in circles)
            {
                item.gameObject.SetActive(false);
            }
            foreach (var item in pieLabels)
            {
                item.gameObject.SetActive(false);
            }
                diameter = chartHolder.rect.width;
                UICircle baseC = GetCircle(chartHolder, "base", baseColor, diameter, thickness, isFill);
                baseC.transform.SetParent(chartHolder);
            circleCount = 1;
            float highestAmount = barGraphs.Sum(x => x.value);
            float normalizedAmount = 1f / (float)highestAmount;

            float labelHeight = (diameter * .5f) + (thickness * .5f);
            float lineHeight = thickness;
            Vector2 labelDimension = new Vector2(10, labelHeight);

            float previousAngle = 0;
            float labelPreviousAngle = 90;
            foreach (var item in barGraphs)
            {
                previousAngle = CreateChart(item, normalizedAmount, previousAngle);
                circleCount=circleCount+1;
                float halfAngle = ((item.value * normalizedAmount) * 0.5f) * 360;
                float angle = labelPreviousAngle + halfAngle;
                CreateLabel(item.label, angle, labelDimension, lineHeight, Color.black);
                pieLabelCount=pieLabelCount+1;
                labelPreviousAngle = angle + halfAngle;
            }


        }
        public void CreateLabel(string label, float angle, Vector2 dimension, float lineHeight, Color labelColor)
        {
            if (pieLabelCount > (pieLabels.Count - 1))
            {
                PieLabel labelT = Instantiate(pieLabelPrefab, chartHolder);
                labelT.Init(angle, dimension, lineHeight, labelColor, label);
                labelT.transform.SetAsLastSibling();
                pieLabels.Add(labelT);
            }
            else
            {
                var labelT = pieLabels[pieLabelCount];
                labelT.Init(angle, dimension, lineHeight, labelColor, label);
                labelT.transform.SetAsLastSibling();
                labelT.gameObject.SetActive(true);
            }
        }
        public float CreateChart(BarGraphData barGraphData, float normalizedAmount, float previousAngle)
        {
            UICircle circle = GetCircle(chartHolder, barGraphData.label, barGraphData.color, diameter, thickness, isFill);
            circle.transform.SetParent(chartHolder);
            circle.Arc = barGraphData.value * normalizedAmount;
            float angle = (circle.Arc * 360f) + previousAngle;
            circle.transform.localEulerAngles = new Vector3(0, 0, angle);
            return angle;
        }

        UICircle GetCircle(Transform parent, string name, Color color, float diameter, float thickness, bool isFill)
        {
            if (circleCount > (circles.Count - 1))
            {
                GameObject go = new GameObject("base");
                go.transform.SetParent(parent);
                go.transform.localScale = Vector3.one;
                go.transform.localPosition = Vector3.zero;
                UICircle circle = go.AddComponent<UICircle>();
                NicerOutline outline = go.AddComponent<NicerOutline>();
                outline.useGraphicAlpha = true;
                outline.effectColor = Color.black;
                outline.effectDistance = new Vector2(1, -1);
                circle.name = name;
                circle.color = color;
                circle.rectTransform.sizeDelta = new Vector2(diameter, diameter);
                circle.Thickness = thickness;
                circle.Fill = isFill;
                circles.Add(circle);
                return circle;
            }
            else
            {
                UICircle circle = circles[circleCount];
                circle.name = name;
                circle.color = color;
                circle.rectTransform.sizeDelta = new Vector2(diameter, diameter);
                circle.Thickness = thickness;
                circle.Fill = isFill;
                circle.gameObject.SetActive(true);
                return circles[circleCount];
            }
        }
    }
}