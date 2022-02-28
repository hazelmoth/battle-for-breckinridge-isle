using UnityEngine;

public class SelectionDisplayer : MonoBehaviour
{
	private static SelectionDisplayer _instance;
	[SerializeField] private GameObject selectionRingPrefab;
	[SerializeField] private GameObject hoverSelectorPrefab;
	private GameObject _selectionRing;
	private GameObject _hoverSelector;
	// Start is called before the first frame update
	private void Start()
	{
		_instance = this;
	}
	private void OnDestroy()
	{
		_instance = null;
	}
	// Update is called once per frame
	private void Update()
	{
        
	}

	public static void PlaceSelectionRing (Vector2Int location)
	{
		if (_instance._selectionRing == null)
		{
			_instance._selectionRing = GameObject.Instantiate(_instance.selectionRingPrefab);
		}
		_instance._selectionRing.SetActive(true);
		_instance._selectionRing.transform.position = new Vector3(location.x + 0.5f, location.y + 0.5f, -0.5f);
	}
	public static void ClearSelectionRing ()
	{
		if (_instance._selectionRing != null)
		{
			_instance._selectionRing.SetActive(false);
		}
	}

}