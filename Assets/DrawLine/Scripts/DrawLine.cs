using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class DrawLine : MonoBehaviour
{
    public Material line;
    public GameObject lineFX;
    public Toggle gravityCheck;

    [Header("Gameplay Config"), Tooltip("The color of the drawn lines")]
    public Color lineColor;

    private List<GameObject> listLine = new List<GameObject>();
    private List<Vector2> listPoint = new List<Vector2>();

    private GameObject currentLine;
    private GameObject currentColliderObject;

    private BoxCollider2D currentBoxCollider2D;
    private LineRenderer currentLineRenderer;

    private bool stopHolding;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            stopHolding = false;
            listPoint.Clear();
            CreateLine(Input.mousePosition);
        }

        if (Input.GetMouseButton(0) && !stopHolding)
        {
            Vector2 item = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            lineFX.transform.position = new Vector2(item.x, item.y);
            lineFX.SetActive(true);

            if (!listPoint.Contains(item))
            {
                listPoint.Add(item);
                currentLineRenderer.positionCount = listPoint.Count;
                currentLineRenderer.SetPosition(listPoint.Count - 1, listPoint[listPoint.Count - 1]);

                if (listPoint.Count >= 2)
                {
                    Vector2 vector = listPoint[listPoint.Count - 2];
                    Vector2 vector2 = listPoint[listPoint.Count - 1];

                    currentColliderObject = new GameObject("Collider");
                    currentColliderObject.transform.position = ((vector + vector2) / 2f);
                    currentColliderObject.transform.right = ((vector2 - vector).normalized);
                    currentColliderObject.transform.parent = currentLine.transform;
                    currentBoxCollider2D = currentColliderObject.AddComponent<BoxCollider2D>();
                    currentBoxCollider2D.size = new Vector3((vector2 - vector).magnitude, 0.1f, 0.1f);
                    currentBoxCollider2D.enabled = false;
                }
            }
        }
        if (Input.GetMouseButtonUp(0) && !stopHolding)
        {
            lineFX.SetActive(false);

            if (currentLine.transform.childCount > 0)
            {
                for (int j = 0; j < currentLine.transform.childCount; j++)
                {
                    currentLine.transform.GetChild(j).GetComponent<BoxCollider2D>().enabled = true;
                }

                listLine.Add(currentLine);

                if (gravityCheck.isOn)
                {
                    currentLine.AddComponent<Rigidbody2D>().useAutoMass = true;
                }
            }
            else
            {
                Destroy(currentLine);
            }
        }
    }

    private void CreateLine(Vector2 mousePosition)
    {
        currentLine = new GameObject("Line");
        currentLine.tag = "Line";
        currentLineRenderer = this.currentLine.AddComponent<LineRenderer>();
        currentLineRenderer.material = line;
        currentLineRenderer.material.EnableKeyword("_EMISSION");
        currentLineRenderer.material.SetColor("_EmissionColor", this.lineColor);
        currentLineRenderer.positionCount = 0;
        currentLineRenderer.startWidth = 0.1f;
        currentLineRenderer.endWidth = 0.1f;
        currentLineRenderer.startColor = lineColor;
        currentLineRenderer.endColor = lineColor;
        currentLineRenderer.useWorldSpace = false;
    }

    public void ClearAll()
    {
        GameObject[] temp = GameObject.FindGameObjectsWithTag("Line");

        foreach (var item in temp)
        {
            Destroy(item);
        }
    }
}
