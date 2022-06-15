using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles all code for creating and rendering the Grapple.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(HingeJoint2D))]
public class PlayerGrapple : MonoBehaviour
{
    public float MinGrappleDistance = 1.1f;
    public float GrappleSpeed = 7;

    /// <summary>
    /// If true, the a coroutine is running which is drawing the grapple,
    /// then setting the hinge.
    /// </summary>
    public bool IsDrawingGrapple = false;

    private Coroutine DrawThenSetGrappleCoroutine;

    private Rigidbody2D PlayerRigidBody;
    private LineRenderer LineRenderer;
    private HingeJoint2D Hinge;

    /// <summary>
    /// Enables or disables the HingeJoint2D component.
    /// </summary>
    public bool IsEnabled { get => Hinge.enabled; set => Hinge.enabled = value; }

    private void Awake()
    {
        PlayerRigidBody = GetComponent<Rigidbody2D>();
        LineRenderer = GetComponent<LineRenderer>();
        LineRenderer.positionCount = 2;
        LineRenderer.enabled = false;

        Hinge = GetComponent<HingeJoint2D>();
        Hinge.enabled = false;
    }

    public void AttemptGrapple(Vector3 clickLocation)
    {
        RaycastHit2D hit = Physics2D.Raycast(clickLocation, Vector2.zero);

        if (hit == false 
            || hit.collider.gameObject == gameObject
            || MinGrappleDistance >= Vector2.Distance(
                hit.collider.transform.position, 
                PlayerRigidBody.transform.position))
            return;

        if (!IsDrawingGrapple)
            DrawThenSetGrappleCoroutine = StartCoroutine(DrawThenSetGrapple(hit.collider.gameObject.transform.position, hit));
    }

    public void RemoveHinge()
    {
        DisableLineRenderer();
        Hinge.anchor = Vector2.zero;
        Hinge.enabled = false;
    }

    private void SetHinge(GameObject node)
    {
        Hinge.enabled = true;
        Hinge.anchor = PlayerRigidBody.transform.InverseTransformPoint(node.transform.position);
    }

    private IEnumerator DrawThenSetGrapple(Vector3 point, RaycastHit2D hit)
    {
        IsDrawingGrapple = true;
        LineRenderer.enabled = true;

        float time = 0;
        Vector3 startPos = PlayerRigidBody.transform.position;

        while (time < 1)
        {
            LineRenderer.SetPosition(1, Vector3.Lerp(startPos, point, time));
            time += GrappleSpeed * Time.deltaTime;

            yield return null;
        }

        LineRenderer.SetPosition(1, point);
        SetHinge(hit.collider.gameObject);

        IsDrawingGrapple = false;
    }

    /// <summary>
    /// Stops the coroutine which draws and sets the grapple.
    /// </summary>
    public void StopDrawThenSetGrapple()
    {
        StopCoroutine(DrawThenSetGrappleCoroutine);
        IsDrawingGrapple = false;
    }

    public void UpdateLine()
    {
        if (LineRenderer.enabled)
            LineRenderer.SetPosition(0, PlayerRigidBody.transform.position);
    }

    /// <summary>
    /// It is reccomended that you use this rather than set LineRenderer.enabled = false.
    /// </summary>
    public void DisableLineRenderer()
    {
        LineRenderer.enabled = false;
        LineRenderer.SetPositions(new[] { Vector3.zero, Vector3.zero });
    }
}
