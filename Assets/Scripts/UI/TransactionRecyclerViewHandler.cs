using PolyAndCode.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CubeHole.MM
{
    public class TransactionRecyclerViewHandler : MonoBehaviour, IRecyclableScrollRectDataSource
    {
        [SerializeField] RecyclableScrollRect _recyclableScrollRect;
        [SerializeField] private GameObject emptyGraphics;
        private List<Transaction> AllTransactions;
        private bool isInitialized=false;
        public void Init(List<Transaction> transactions)
        {
            if (transactions.Count==-0)
            {
                emptyGraphics.gameObject.SetActive(true);
            }
            else
                emptyGraphics.gameObject.SetActive(false);

            AllTransactions = transactions;
            _recyclableScrollRect.ReloadData();
            _recyclableScrollRect.Rebuild(CanvasUpdate.PostLayout);
            if (!isInitialized)
                _recyclableScrollRect.Initialize(this);
            isInitialized = true;
        }
        public int GetItemCount()
        {
            return AllTransactions.Count;
        }

        public void SetCell(ICell cell, int index)
        {
            var item = cell as TransactionHolder;
            item.InitTransaction(AllTransactions[index]);
        }
    }
}