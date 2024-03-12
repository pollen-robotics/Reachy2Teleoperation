using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

namespace TeleopReachy
{
    public static class ChangeRenderer
    {
        // Turn on or off the renderer of a gameObject and of all its children
        public static void switchRenderer(this Transform t, bool enabled)
        {
            foreach (Transform child in t)
            {
                switchRenderer(child, enabled);
            }
            Renderer r = t.gameObject.GetComponent<Renderer>();
            if (r != null)
            {
                r.enabled = enabled;
            }
        }
    }

    public static class ChangeActiveChildStatus
    {
        // Activate of deactivate all children of a gameObject (but not the gameObject)
        public static void ActivateChildren(this Transform t, bool enabled)
        {
            foreach (Transform child in t)
            {
                child.gameObject.SetActive(enabled);
            }
        }
    }
}