using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * Basic implementation of how to use a Vive controller as an input device.
 * Can only interact with items with InteractableBase component
 */
public class WandController : MonoBehaviour
{
    private Valve.VR.EVRButtonId gripButton = Valve.VR.EVRButtonId.k_EButton_Grip;
    private Valve.VR.EVRButtonId triggerButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;

    private SteamVR_Controller.Device controller { get { return SteamVR_Controller.Input((int)trackedObj.index); } }
    private SteamVR_TrackedObject trackedObj;

    private GameObject pickup;

    private GameObject prefabProjectile;

    private bool isHoldingShip = false;

    // Use this for initialization
    void Start()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        prefabProjectile = Resources.Load("Projectile") as GameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (controller == null)
        {
            Debug.Log("Controller not initialized");
            return;
        }

        if (controller.GetPressDown(triggerButton))
        {
            if (!this.isHoldingShip && pickup != null)
            {
                // pick up the ship
                pickup.transform.parent = this.transform;
                pickup.transform.localPosition = new Vector3(0, 0, 0);
                pickup.transform.localRotation = Quaternion.Euler(60, 0, 0);
                SteamVR_Utils.Event.Send("hide_render_models", true);
                this.isHoldingShip = true;
            }
            else if (this.isHoldingShip)
            {
                // after picking up the ship, trigger fires projectiles
                GameObject p = Instantiate(prefabProjectile) as GameObject;
                p.transform.position = pickup.transform.position + pickup.transform.forward * 0.04f;
                Rigidbody rb = p.GetComponent<Rigidbody>();
                rb.velocity = pickup.transform.forward * 25.0f;
            }
            else { }
        }
    }

    // Adds all colliding items to a HashSet for processing which is closest
    private void OnTriggerEnter(Collider collider)
    {
        if(!this.isHoldingShip)
            pickup = collider.gameObject;
    }

    // Remove all items no longer colliding with to avoid further processing
    private void OnTriggerExit(Collider collider)
    {
        if(!this.isHoldingShip)
            pickup = null;
    }
}