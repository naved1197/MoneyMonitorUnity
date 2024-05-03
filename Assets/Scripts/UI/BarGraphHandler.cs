
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CubeHole
{

    public class BarGraphHandler : MonoBehaviour
    {
        [SerializeField] private bool isDynamic = false;
        [SerializeField] private RectTransform barholder;
        [SerializeField] private float maxBarHeight = 200;
        [SerializeField] private BarGraphHolder[] graphHolders;
        [SerializeField] private BarGraphRecyclerView barGraphView;
        [SerializeField] private GameObject emptyGraphics;
        public void Init(List<BarGraphInfo> data)
        {

            float normalizedAmount = 0;
            if (data.Count > 0)
            {
                float highestAmount = data.Max(x => x.data.Max(y => y.value));
                highestAmount = Mathf.Round(highestAmount);
                highestAmount = highestAmount + 1;
                float height = barholder != null ? barholder.rect.height : maxBarHeight;
                normalizedAmount = (float)height / (float)highestAmount;
                emptyGraphics.gameObject.SetActive(false);

            }
            else
                emptyGraphics.gameObject.SetActive(true);
            //Covert the values to the normalized values
            foreach (var item in data)
            {
                foreach (var bar in item.data)
                {
                    bar.value = bar.value * normalizedAmount;
                }
            }
            if (isDynamic)
                barGraphView.Init(data);
            else
            {
                for (int i = 0; i < graphHolders.Length; i++)
                {
                    if (data.Count != 0)
                    {
                        graphHolders[i].gameObject.SetActive(true);
                        graphHolders[i].Init(data[i]);
                    }
                    else
                        graphHolders[i].gameObject.SetActive(false);
                }
            }
        }
    }
}