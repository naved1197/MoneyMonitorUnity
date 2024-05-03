using CubeHole.MM;
using PolyAndCode.UI;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;
using UnityEngine.UI;

namespace CubeHole
{
    public class BarGraphRecyclerView : MonoBehaviour, IRecyclableScrollRectDataSource
    {
        [SerializeField] RecyclableScrollRect _recyclableScrollRect;
        private List<BarGraphInfo> data;
        private bool isInitialized=false;

        public void Init(List<BarGraphInfo> data)
        {
            this.data = data;
            _recyclableScrollRect.ReloadData();
            _recyclableScrollRect.Rebuild(CanvasUpdate.PostLayout);
            if (!isInitialized)
                _recyclableScrollRect.Initialize(this);
            isInitialized = true;
        }
        public int GetItemCount()
        {
            return data.Count;
        }

        public void SetCell(ICell cell, int index)
        {
            var item = cell as BarGraphHolder;
            item.Init(data[index]);
        }
    }
}