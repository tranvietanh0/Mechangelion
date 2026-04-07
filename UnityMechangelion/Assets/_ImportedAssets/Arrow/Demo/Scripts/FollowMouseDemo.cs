using UnityEngine;

public class FollowMouseDemo : MonoBehaviour
{
    public ArrowRenderer arrowRenderer;
    public float distanceFromScreen = 5f;

    public void SetRenderer(ArrowRenderer value)
    {
        if (this.arrowRenderer) this.arrowRenderer.gameObject.SetActive(false);

        this.arrowRenderer = value;

        if (this.arrowRenderer) this.arrowRenderer.gameObject.SetActive(true);
    }

    void Update()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = this.distanceFromScreen;

        Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        this.arrowRenderer.SetPositions(this.transform.position, worldMousePosition);
    }
}
