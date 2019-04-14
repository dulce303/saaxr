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

    private bool _hasItem = false;
    private bool _canGrab = false;
    private bool _canDrop = false;
    private bool _canOpenContainer = false;


    private StemSoundBehavior _currentGrabbable = null;
    private DropzoneBehavior _currentDropzone = null;
    private ContainerBehavior _currentContainerBehavior = null;
    private StemSoundBehavior _currentPickedUpStem = null;


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

    }

	private void OnDestroy()
	{
        MLInput.OnTriggerDown -= HandleOnTriggerDown;

	}

    private void HandleOnTriggerDown(byte controllerId, float pressure)
    {
        if (_canGrab)
        {
            if (_currentGrabbable != null)
            {
                _currentPickedUpStem = _currentGrabbable;
                _currentPickedUpStem.transform.SetParent(_grabPoint);
                _currentPickedUpStem.transform.position = _grabPoint.position;

                _currentGrabbable = null;

                _volumeGUI.SetActive(true);
                _canGrab = false;

            }
        }
        else if (_canDrop)
        {
            if (_currentDropzone != null && _currentPickedUpStem != null)
            {
                _currentPickedUpStem.transform.SetParent(_currentDropzone.transform);
                _currentPickedUpStem.transform.position = _currentDropzone.transform.position;
                _currentPickedUpStem.transform.rotation = _currentDropzone.transform.rotation;

                _currentPickedUpStem = null;
                _currentDropzone = null;

                _volumeGUI.SetActive(false);

            }
        } 
        else if (_canOpenContainer)
        {
            if (_currentContainerBehavior != null)
            {
                _currentContainerBehavior.Open();
                _currentContainerBehavior = null;
            }
        }
    }


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

            var _volume = Mathf.Clamp(controller.Touch1PosAndForce.x, 0.0f, 1.0f);
            _currentGrabbable.SetVolume(_volume);
            _volume = Mathf.RoundToInt((_volume * 10));
            _volumeNumber.SetText(_volume.ToString());
        }



    }

    private void LaserGrabber()
    {
        Vector3 _lineEndPoint = new Vector3();
        bool _useHitMat = false;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 50f))
        {
            if (hit.transform.CompareTag("Character"))
            {
                CharacterAnimationBehavior _char = hit.transform.GetComponent<CharacterAnimationBehavior>();
                _char.TriggerAnim();
                ClearAllCurrents();

                _lineEndPoint = hit.point;
                _useHitMat = false;
            }
            else if (hit.transform.CompareTag("Container"))
            {
                _currentContainerBehavior = hit.transform.GetComponent<ContainerBehavior>();
                _canOpenContainer = true;

                _lineEndPoint = hit.point;
                _useHitMat = true;

            }
            else if (_hasItem)
            {
                //If we've got an item we need to
                _canGrab = false;


                if (hit.transform.CompareTag("Dropzone"))
                {
                    _canDrop = true;
                    _currentDropzone = hit.transform.GetComponent<DropzoneBehavior>();


                    _lineEndPoint = hit.point;
                    _useHitMat = true;
                }
                else
                {
                    //Thunk
                    ClearAllCurrents();


                    _lineEndPoint = hit.point;
                    _useHitMat = false;
                }
            }
            else if (!_hasItem)
            {
                //If we don't have an item we need to
                _canDrop = false;


                if (hit.transform.CompareTag("Stem"))
                {
                    _canGrab = true;
                    if (_currentGrabbable == null)
                        _currentGrabbable = hit.transform.GetComponent<StemSoundBehavior>();

                    _lineEndPoint = hit.point;
                    _useHitMat = true;
                }
                else
                {
                    //Thunk
                    ClearAllCurrents();
                    _lineEndPoint = hit.point;
                    _useHitMat = false;
                }
            }
        }
        else
        {
            //Miss
            ClearAllCurrents();
            _lineEndPoint = transform.TransformDirection(Vector3.forward * 10f);
            _useHitMat = false;
        }

        UpdateLine(_lineEndPoint, _useHitMat);
    }

    private void ClearAllCurrents(){
        _currentDropzone = null;
        _currentGrabbable = null;
        _currentContainerBehavior = null;
        _canGrab = false;
        _canDrop = false;
        _canOpenContainer = false;
    }

    private void UpdateLine(Vector3 _endPoint, bool _didHit){
        _laserLineRenderer.SetPosition(0, transform.position);
        _laserLineRenderer.SetPosition(1, _endPoint);
        if (_didHit){
            _laserLineRenderer.material = _hitMaterial;
        } else{
            _laserLineRenderer.material = _missMaterial;
        }
    }


    //Old, over-complex LaserGrabber
    /*
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
                _canOpenContainer = false;
                _containerBehavior = null;
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
                        _containerBehavior = null;
                        _canOpenContainer = false;
                    }
                    else if (hit.transform.CompareTag("Container"))
                    {
                        _laserLineRenderer.material = _hitMaterial;
                        _laserLineRenderer.SetPosition(1, hit.point);
                        if (_containerBehavior != null){
                            _containerBehavior = hit.transform.GetComponent<ContainerBehavior>();
                        }
                        _canOpenContainer = true;
                    }
                    else if (hit.transform.tag != null)
                    {
                        _laserLineRenderer.material = _missMaterial;
                        _laserLineRenderer.SetPosition(1, hit.point);
                        Debug.Log("Has item and hit non-dropzone");
                        _canDrop = false;
                        _currentDropzone = null;
                        _containerBehavior = null;
                        _canOpenContainer = false;
                    }
                }


                else
                {
                    _laserLineRenderer.material = _missMaterial;
                    _laserLineRenderer.SetPosition(1, transform.TransformDirection(Vector3.forward) * 10f);
                    Debug.Log("Has item and missed");
                    _canDrop = false;
                    _currentDropzone = null;
                    _containerBehavior = null;
                    _canOpenContainer = false;
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
                        _containerBehavior = null;
                        _canOpenContainer = false;
                    }
                    else if (hit.transform.CompareTag("Container"))
                    {
                        _laserLineRenderer.material = _hitMaterial;
                        _laserLineRenderer.SetPosition(1, hit.point);
                        if (_containerBehavior != null)
                        {
                            _containerBehavior = hit.transform.GetComponent<ContainerBehavior>();
                        }
                        _canOpenContainer = true;
                    }
                    else if (hit.transform.tag != null)
                    {
                        _laserLineRenderer.material = _missMaterial;
                        _laserLineRenderer.SetPosition(1, hit.point);
                        Debug.Log("Doesn't have item and hit non-grabbable");
                        _canGrab = false;
                        _currentGrabbable = null;
                        _containerBehavior = null;
                        _canOpenContainer = false;
                    }
                }
                else
                {
                    _laserLineRenderer.material = _missMaterial;
                    _laserLineRenderer.SetPosition(1, transform.TransformDirection(Vector3.forward * 10f));
                    Debug.Log("Doesn't have item and didn't hit");
                    _canGrab = false;
                    _currentGrabbable = null;
                    _containerBehavior = null;
                    _canOpenContainer = false;
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
            _containerBehavior = null;
        }
    }
    */

    //Old trigger behavior
    /*
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
    } else if (_canDrop)
    {
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
    else if (_canOpenContainer)
    {
        _containerBehavior.Open();
        _containerBehavior = null;
    }
}
*/


}
