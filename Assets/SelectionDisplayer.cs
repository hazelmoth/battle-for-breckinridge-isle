using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionDisplayer : MonoBehaviour
{
	static SelectionDisplayer instance;
	[SerializeField] GameObject selectionRingPrefab;
	[SerializeField] GameObject hoverSelectorPrefab;
	GameObject selectionRing;
	GameObject hoverSelector;
    // Start is called before the first frame update
    void Start()
    {
		instance = this;
    }
	private void OnDestroy()
	{
		instance = null;
	}
	// Update is called once per frame
	void Update()
    {
        
    }

	public static void PlaceSelectionRing (Vector2Int location)
	{
		if (instance.selectionRing == null)
		{
			instance.selectionRing = GameObject.Instantiate(instance.selectionRingPrefab);
		}
		instance.selectionRing.SetActive(true);
		instance.selectionRing.transform.position = new Vector3(location.x + 0.5f, location.y + 0.5f, -0.5f);
	}
	public static void ClearSelectionRing ()
	{
		if (instance.selectionRing != null)
		{
			instance.selectionRing.SetActive(false);
		}
	}

}
