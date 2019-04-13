using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

[RequireComponent(typeof(LineRenderer))]

public class RaycastGrabber : MonoBehaviour
{


    public Material _hitMaterial;
    public Material _missMaterial;

    public int _grabbableLayer = 8;
    public int _dropzoneLayer = 9;

    private bool _hasItem = false;


    private LineRenderer _laserLineRenderer;

    // Start is called before the first frame update
    void Start()
    {
        _laserLineRenderer = GetComponent<LineRenderer>();

        _grabbableLayer = 1 << _grabbableLayer;
        _dropzoneLayer = 1 << _dropzoneLayer;
    }

    // Update is called once per frame
    void Update()
    {
        _laserLineRenderer.SetPosition(0, transform.position);
        RaycastHit hit;

        if (_hasItem)
        {
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 50f, _dropzoneLayer))
            {
                _laserLineRenderer.material = _hitMaterial;
                _laserLineRenderer.SetPosition(1, hit.point);
                Debug.Log("Has item and hit dropzone");

            }
            else if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 50f))
            {
                _laserLineRenderer.material = _missMaterial;
                _laserLineRenderer.SetPosition(1, hit.point);
                Debug.Log("Has item and hit non-dropzone");
            }
            else {
                _laserLineRenderer.material = _missMaterial;
                _laserLineRenderer.SetPosition(1, transform.TransformDirection(Vector3.forward) * 10f);
                Debug.Log("Has item and missed");
            }
        } else if (!_hasItem){
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 50f, _grabbableLayer)){
                _laserLineRenderer.material = _hitMaterial;
                _laserLineRenderer.SetPosition(1, hit.point);
                Debug.Log("Doesn't have item and hit grabbable");
            }
            else if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 50f)){
                _laserLineRenderer.material = _missMaterial;
                _laserLineRenderer.SetPosition(1, hit.point);
                Debug.Log("Doesn't have item and hit non-grabbable");
            }
            else {
                _laserLineRenderer.material = _missMaterial;
                _laserLineRenderer.SetPosition(1, transform.TransformDirection(Vector3.forward) * 10f);
                Debug.Log("Doesn't have item and didn't hit");
            }
        }

        if (Input.GetKeyDown("i")){
            _hasItem = !_hasItem;
        }
    }


}
