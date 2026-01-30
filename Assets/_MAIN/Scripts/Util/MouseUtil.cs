using UnityEngine;
using UnityEngine.InputSystem;

public static class MouseUtil
{

    public static Vector3 GetMousePositionInWorldSpace(float zValue = 0f, float offset = 10f)
    {
        Vector2 mousePos = Pointer.current.position.ReadValue();
        Plane dragPlane = new(Camera.main.transform.forward, new Vector3(0, 0, zValue));
        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        Vector3 offsetOrigin = ray.origin - Camera.main.transform.forward * offset;
        Ray offsetRay = new Ray(offsetOrigin, ray.direction);

        Debug.DrawRay(offsetRay.origin, offsetRay.direction, Color.red);
        if (dragPlane.Raycast(offsetRay, out float distance))
        {
            return offsetRay.GetPoint(distance); // Retorna o ponto de interseção
        }
#if MOBILE
        return Pointer.current.position.ReadValue();
#endif
        return Mouse.current.position.ReadValue();
    }

    public static Vector2 GetMousePosition()
    {
#if MOBILE
        return Pointer.current.position.ReadValue();
#endif
        return Mouse.current.position.ReadValue();
    }
}
