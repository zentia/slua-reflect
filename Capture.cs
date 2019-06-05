class Capture
{
	public static float DisPoint2Surface(Vector3 point, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        var a = (p2.y - p1.y) * (p3.z - p1.z) - (p2.z - p1.z) * (p3.y - p1.y);
        var b = (p2.z - p1.z) * (p3.x - p1.x) - (p2.x - p1.x) * (p3.z - p1.z);
        var c = (p2.x - p1.x) * (p3.y - p1.y) - (p2.y - p1.y) * (p3.x - p1.x);
        var d = 0 - (a * p1.x + b * p1.y + c * p1.z);
        return DisPoint2Surface(point, a, b, c, d);
    }

    static Vector3[] GetCorners(Camera theCamera, float distance)
    {
        Vector3[] corners = new Vector3[4];
        var tx = theCamera.transform;
        float halfFOV = (theCamera.fieldOfView * 0.5f) * Mathf.Deg2Rad;
        float aspect = theCamera.aspect;
        float height = distance * Mathf.Tan(halfFOV);
        float width = height * aspect;
        // UpperLeft
        corners[0] = tx.position - (tx.right * width);
        corners[0] += tx.up * height;
        corners[0] += tx.forward * distance;
        // UpperRight
        corners[1] = tx.position + (tx.right * width);
        corners[1] += tx.up * height;
        corners[1] += tx.forward * distance;
        // LowerLeft
        corners[2] = tx.position - (tx.right * width);
        corners[2] -= tx.up         * height;
        corners[2] += tx.forward * distance;
        // LowerRight
        corners[3] = tx.position + (tx.right * width);
        corners[3] -= tx.up * height;
        corners[3] += tx.forward * distance;
        return corners;
    }

    static Vector3 GetPosition(Camera camera, float distance, Vector3 upperLeft)
    {
        var tf = camera.transform;
        float halfFOV = camera.fieldOfView * 0.5f * Mathf.Deg2Rad;
        float aspect = camera.aspect;
        float height = distance * Mathf.Tan(halfFOV);
        float width = height * aspect;
        var position = upperLeft - tf.forward * distance;
        position -= tf.up * height;
        position += tf.right * width;
        return position;
    }

    public static float DisPoint2Surface(Vector3 pt, float a, float b, float c, float d)
    {
        return Mathf.Abs(a * pt.x + b * pt.y + c * pt.z + d) / Mathf.Sqrt(a * a + b * b + c * c);
    }

    public static void CaptureCameraExt(Camera cam, GameObject lefttop, GameObject rightbottom, GameObject obj)
    {
        var distance = DisPoint2Surface(cam.transform.position, lefttop.transform.position, rightbottom.transform.position, obj.transform.position);
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture rt = RenderTexture.GetTemporary(Screen.width, Screen.height, 24);
        cam.targetTexture = rt;
        RenderTexture.active = rt;
        Texture2D png = new Texture2D(3*Screen.width, 2*Screen.height, TextureFormat.ARGB32, false);
        var o = cam.transform.position;
        cam.transform.position = GetPosition(cam, distance, lefttop.transform.position);
        for (int i = 0; i < 3; i++)
        {
            var old = cam.transform.position;
            for(int j = 0;j < 2; j++)
            {
                cam.Render();
                png.ReadPixels(new Rect(0,0,Screen.width,Screen.height), i*Screen.width,(1-j)*Screen.height);
                var cors = GetCorners(cam, distance);
                cam.transform.position = GetPosition(cam, distance, cors[2]);
            }
            cam.transform.position = old;
            var corners = GetCorners(cam, distance);
            cam.transform.position = GetPosition(cam, distance, corners[1]);
        }
        cam.transform.position = o;
        cam.targetTexture = null;
        RenderTexture.active = currentRT;
        RenderTexture.ReleaseTemporary(rt);
        byte[] bytes = png.EncodeToPNG();
        File.WriteAllBytes(Instance.GetFullPath(), bytes);
        Destroy(png);
        png = null;
    }
}