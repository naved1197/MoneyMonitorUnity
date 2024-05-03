using System;
using System.Collections.Generic;
using UnityEngine;

public class FiltersHolder : MonoBehaviour
{
    [SerializeField] private GameObject toggleSwitchPrefab;
    [SerializeField] private GameObject rowHolderPrefab;
    [SerializeField] private int numberOfColumns;
    [SerializeField] private List<ToggleSwitch> toggleSwitches = new List<ToggleSwitch>();
    private int counter;

    private void Awake()
    {
        FilterLabel.OnFilterRemoved += OnFilterRemoved;
    }
    private void OnDestroy()
    {
        FilterLabel.OnFilterRemoved -= OnFilterRemoved;
    }
    private void OnFilterRemoved(string obj)
    {
        ToggleSwitch toggle= toggleSwitches.Find(x => x.label == obj);
        if (toggle != null)
        {
            toggle.ChangeState();
        }
    }
    public void RemoveAllFilters()
    {
        foreach (var item in toggleSwitches)
        {
            item.ResetSwitch();
        }
    }

    public void Init(List<string> data)
    {
        counter = 0;
        int numberOfRows = Mathf.CeilToInt((float)data.Count / numberOfColumns);
        for (int i = 0; i < numberOfRows; i++)
        {
            GameObject rowHolder = Instantiate(rowHolderPrefab, transform);
            for (int j = 0; j < numberOfColumns; j++)
            {
                if (counter < data.Count)
                {
                    if (counter >= (toggleSwitches.Count - 1))
                    {
                        GameObject toggleSwitch = Instantiate(toggleSwitchPrefab, rowHolder.transform);
                        toggleSwitch.GetComponent<ToggleSwitch>().Init(counter, data[counter], false);
                        toggleSwitches.Add(toggleSwitch.GetComponent<ToggleSwitch>());
                    }
                    else
                    {
                        toggleSwitches[counter].Init(counter, data[counter], false);
                    }
                    counter++;
                }
            }
        }
    }
}
