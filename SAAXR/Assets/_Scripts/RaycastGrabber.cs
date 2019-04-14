using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using TMPro;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(ControllerConnectionHandler))]

public class RaycastGrabber : MonoBehaviour
{
    public Transform _grabPoint;

    public Material _hitMaterial;
    public Material _missMaterial;

    public GameObject _touchpad;
    public TextMeshPro _volumeNumber;
    public GameObject _volumeGUI;
    private float _touchpadRadius;

    //public int _grabbableLayer = 8;
    //public int _dropzoneLayer = 9;

    private bool _hasItem = false;
    private bool _canGrab = false;
    private bool _canDrop = false;
    private GameObject _currentGrabbable = null;
    private GameObject _currentDropzone = null;
    private StemSoundBehavior _ssBehavior = null;
    private DropzoneBehavior _dzBehavior = null;

    private LineRenderer _laserLineRenderer = null;
    private ControllerConnectionHandler _controllerConnectionHandler = null;

	private void Awake()
	{
        MLInput.OnTriggerDown += HandleOnTriggerDown;

	}


	void Start()
    {
        _controllerConnectionHandler = GetComponent<ControllerConnectionHandler>();
        _laserLineRenderer = GetComponent<LineRenderer>();

        Mesh mesh = _touchpad.GetComponent<MeshFilter>().mesh;
        _touchpadRadius = Vector3.Scale(mesh.bounds.extents, _touchpad.transform.lossyScale).x;

        _volumeGUI.SetActive(false);

        //_grabbableLayer = 1 << _grabbableLayer;
        //_dropzoneLayer = 1 << _dropzoneLayer;
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
            _hasItem = true;

            if (_currentGrabbable.CompareTag("Stem")){
                _ssBehavior = _currentGrabbable.GetComponentInChildren<StemSoundBehavior>();
                _dzBehavior = _ssBehavior.GetDropZone();

                _ssBehavior.SetPickupMix();
                _dzBehavior.RemoveStem();

                _volumeGUI.SetActive(true);
            }
        } else if (_canDrop){
            _currentGrabbable.transform.parent = _currentDropzone.transform;
            _currentGrabbable.transform.position = _currentDropzone.transform.position;
            _currentGrabbable.transform.rotation = _currentDropzone.transform.rotation;
            _canDrop = false;
            _hasItem = false;

            _dzBehavior = _currentDropzone.GetComponent<DropzoneBehavior>();
            _ssBehavior = _currentGrabbable.GetComponentInChildren<StemSoundBehavior>();
            if (_dzBehavior._isMuter)
            {
                _ssBehavior.SetMute();
            }
            else
            {
                _ssBehavior.SetMix(_dzBehavior);
                _dzBehavior.SetStem(_ssBehavior);
            }
            _dzBehavior = null;
            _ssBehavior = null;

            _volumeGUI.SetActive(false);
        }
    }

	// Update is called once per frame
	void Update()
    {
        LaserGrabber();

        if (_hasItem)
        {
            TouchpadBehavior();
        }
    }

    private void TouchpadBehavior()
    {
        if (!_controllerConnectionHandler.IsControllerValid())
        {
            return;
        }
        MLInputController controller = _controllerConnectionHandler.ConnectedController;
        //controller.Touch1PosAndForce.x

        if (controller.Touch1Active)
        {
            //_ssBehavior.SetPitch(Mathf.Clamp(controller.Touch1PosAndForce.y, -3.0f, 3.0f));
            var _volume = Mathf.Clamp(controller.Touch1PosAndForce.x, 0.0f, 1.0f);
            _ssBehavior.SetVolume(_volume);
            _volume = Mathf.RoundToInt((_volume * 10));
            _volumeNumber.SetText(_volume.ToString());
        }



    }

    private void LaserGrabber(){
        _laserLineRenderer.SetPosition(0, transform.position);
        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 50f))
        {
            if (hit.transform.CompareTag("Character"))
            {
                var _hitAnimator = hit.transform.GetComponent<CharacterAnimationBehavior>();
                _hitAnimator.TriggerAnim();
                _canDrop = false;
                _canGrab = false;
                _currentDropzone = null;
                _laserLineRenderer.material = _missMaterial;
                _laserLineRenderer.SetPosition(1, hit.point);
            }

            else if (_hasItem)
            {
                _canGrab = false;

                if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 50f))
                {
                    if (hit.transform.CompareTag("Dropzone"))
                    {
                        _laserLineRenderer.material = _hitMaterial;
                        _laserLineRenderer.SetPosition(1, hit.point);
                        Debug.Log("Has item and hit dropzone");
                        _canDrop = true;
                        _currentDropzone = hit.transform.gameObject;
                    }
                    else if (hit.transform.tag != null)
                    {
                        _laserLineRenderer.material = _missMaterial;
                        _laserLineRenderer.SetPosition(1, hit.point);
                        Debug.Log("Has item and hit non-dropzone");
                        _canDrop = false;
                        _currentDropzone = null;
                    }
                }
                else
                {
                    _laserLineRenderer.material = _missMaterial;
                    _laserLineRenderer.SetPosition(1, transform.TransformDirection(Vector3.forward) * 10f);
                    Debug.Log("Has item and missed");
                    _canDrop = false;
                    _currentDropzone = null;
                }
            }

            else if (!_hasItem)
            {
                _canDrop = false;

                if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 50f))
                {

                    if (hit.transform.CompareTag("Stem"))
                    {
                        _laserLineRenderer.material = _hitMaterial;
                        _laserLineRenderer.SetPosition(1, hit.point);
                        Debug.Log("Doesn't have item and hit grabbable");
                        _canGrab = true;
                        _currentGrabbable = hit.transform.gameObject;
                    }
                    else if (hit.transform.tag != null)
                    {
                        _laserLineRenderer.material = _missMaterial;
                        _laserLineRenderer.SetPosition(1, hit.point);
                        Debug.Log("Doesn't have item and hit non-grabbable");
                        _canGrab = false;
                        _currentGrabbable = null;
                    }
                }
                else
                {
                    _laserLineRenderer.material = _missMaterial;
                    _laserLineRenderer.SetPosition(1, transform.TransformDirection(Vector3.forward * 10f));
                    Debug.Log("Doesn't have item and didn't hit");
                    _canGrab = false;
                    _currentGrabbable = null;
                }
            }

        }
        else
        {
            _laserLineRenderer.material = _missMaterial;
            _laserLineRenderer.SetPosition(1, transform.TransformDirection(Vector3.forward * 10f));
            _currentDropzone = null;
            _canGrab = false;
            _canDrop = false;
        }
    }


}
