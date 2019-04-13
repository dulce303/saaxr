using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

[RequireComponent(typeof(LineRenderer))]

public class RaycastGrabber : MonoBehaviour
{
    public Transform _grabPoint;

    public Material _hitMaterial;
    public Material _missMaterial;

    public int _grabbableLayer = 8;
    public int _dropzoneLayer = 9;

    private bool _hasItem = false;
    private bool _canGrab = false;
    private bool _canDrop = false;
    private GameObject _currentGrabbable = null;
    private GameObject _currentDropzone = null;



    private LineRenderer _laserLineRenderer;


	private void Awake()
	{
        MLInput.OnTriggerDown += HandleOnTriggerDown;
	}

	// Start is called before the first frame update
	void Start()
    {
        _laserLineRenderer = GetComponent<LineRenderer>();

        _grabbableLayer = 1 << _grabbableLayer;
        _dropzoneLayer = 1 << _dropzoneLayer;
    }

	private void OnDestroy()
	{
        MLInput.OnTriggerDown -= HandleOnTriggerDown;
	}

    private void HandleOnTriggerDown(byte controllerId, float pressure)
    {
        if (_canGrab){
            _currentGrabbable.transform.parent = _grabPoint;
            _currentGrabbable.transform.position = _grabPoint.position;
            _canGrab = false;
        } else if (_canDrop){
            _currentGrabbable.transform.parent = _currentDropzone.transform;
            _currentGrabbable.transform.position = _currentDropzone.transform.position;
            _canDrop = false;
        }
    }

	// Update is called once per frame
	void Update()
    {
        _laserLineRenderer.SetPosition(0, transform.position);
        RaycastHit hit;

        if (_hasItem)
        {
            _canGrab = false;
            
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 50f, _dropzoneLayer))
            {
                _laserLineRenderer.material = _hitMaterial;
                _laserLineRenderer.SetPosition(1, hit.point);
                Debug.Log("Has item and hit dropzone");
                _canDrop = true;

            }
            else if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 50f))
            {
                _laserLineRenderer.material = _missMaterial;
                _laserLineRenderer.SetPosition(1, hit.point);
                Debug.Log("Has item and hit non-dropzone");
                _canDrop = false;
            }
            else {
                _laserLineRenderer.material = _missMaterial;
                _laserLineRenderer.SetPosition(1, transform.TransformDirection(Vector3.forward) * 10f);
                Debug.Log("Has item and missed");
                _canDrop = false;
            }
        } else if (!_hasItem){
            _canDrop = false;

            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 50f, _grabbableLayer)){
                _laserLineRenderer.material = _hitMaterial;
                _laserLineRenderer.SetPosition(1, hit.point);
                Debug.Log("Doesn't have item and hit grabbable");
                _canGrab = true;
            }
            else if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 50f)){
                _laserLineRenderer.material = _missMaterial;
                _laserLineRenderer.SetPosition(1, hit.point);
                Debug.Log("Doesn't have item and hit non-grabbable");
                _canGrab = false;
            }
            else {
                _laserLineRenderer.material = _missMaterial;
                _laserLineRenderer.SetPosition(1, transform.TransformDirection(Vector3.forward) * 10f);
                Debug.Log("Doesn't have item and didn't hit");
                _canGrab = false;
            }
        }

        if (Input.GetKeyDown("i")){
            _hasItem = !_hasItem;
        }
    }


}
